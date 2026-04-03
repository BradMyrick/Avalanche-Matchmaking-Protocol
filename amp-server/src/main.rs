use anyhow::Result;
use capnp::capability::Promise;
use capnp_rpc::{RpcSystem, pry, rpc_twoparty_capnp, twoparty};
use ethers_core::types::{H256, U256};
use ethers_core::utils::hash_message;
use ethers_signers::LocalWallet;
use lazy_static::lazy_static;
use std::env;
use std::sync::Arc;
use uuid::Uuid;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use std::time::Duration;
use tokio::sync::oneshot;
use tracing::{info, error};

mod state;
mod matchmaker;
mod player_service;

use state::{AppState, QueueEntry, MatchFoundPayload};

/// Auto-generated modules from Cap'n Proto schemas
pub mod game_types_capnp { include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs")); }
pub mod match_capnp { include!(concat!(env!("OUT_DIR"), "/match_capnp.rs")); }
pub mod service_capnp { include!(concat!(env!("OUT_DIR"), "/service_capnp.rs")); }
pub mod game_core_capnp { include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs")); }
pub mod amp_telemetry_capnp { include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs")); }
pub mod relayer_capnp { include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs")); }
pub mod player_profile_capnp { include!(concat!(env!("OUT_DIR"), "/player_profile_capnp.rs")); }
pub mod matchmaking_rules_capnp { include!(concat!(env!("OUT_DIR"), "/matchmaking_rules_capnp.rs")); }
pub mod game_registry_capnp { include!(concat!(env!("OUT_DIR"), "/game_registry_capnp.rs")); }
pub mod inventory_capnp { include!(concat!(env!("OUT_DIR"), "/inventory_capnp.rs")); }
pub mod tournament_capnp { include!(concat!(env!("OUT_DIR"), "/tournament_capnp.rs")); }
pub mod security_capnp { include!(concat!(env!("OUT_DIR"), "/security_capnp.rs")); }

use service_capnp::*;

// Global match queue separated from main state for concurrent matchmaking access
lazy_static! {
    static ref MATCH_QUEUE: Arc<tokio::sync::Mutex<Vec<QueueEntry>>> = Arc::new(tokio::sync::Mutex::new(Vec::new()));
}

/// Start background task to constantly evaluate matchmaking rules
async fn start_matchmaker_loop(state: AppState) {
    tokio::spawn(async move {
        loop {
            tokio::time::sleep(Duration::from_millis(500)).await;
            let mut queue = MATCH_QUEUE.lock().await;
            if queue.len() < 2 { continue; }

            let mut i = 0;
            while i < queue.len() {
                let entry_a = &queue[i];
                
                // Fetch ruleset
                let s = state.read().await;
                let ruleset = s.rulesets.get(&entry_a.ruleset_id).cloned().unwrap_or_default();
                drop(s);

                // Start from next index to avoid duplicate pairing checks
                let sub_queue = &queue[i+1..];
                if let Some(mut target_idx) = matchmaker::find_match(sub_queue, entry_a, &ruleset) {
                    target_idx += i + 1; // adjust for sub_queue offset

                    // Found! Take both from the queue (must remove target first to preserve index i)
                    let entry_b = queue.remove(target_idx);
                    let entry_a = queue.remove(i);
                    
                    let quality = matchmaker::compute_match_quality(&entry_a, &entry_b, &ruleset);
                    let match_id = Uuid::new_v4().to_string();
                    info!("Match found! [{}] vs [{}] Quality={{Total: {:.2}}}", entry_a.player_id, entry_b.player_id, quality.total_score);
                    
                    // Signal players
                    let p1 = MatchFoundPayload { match_id: match_id.clone(), opponent_ids: vec![entry_b.player_id.clone()], quality: quality.clone() };
                    let p2 = MatchFoundPayload { match_id: match_id.clone(), opponent_ids: vec![entry_a.player_id.clone()], quality: quality.clone() };
                    
                    let _ = entry_a.sender.send(p1);
                    let _ = entry_b.sender.send(p2);
                    
                    let mut s_write = state.write().await;
                    s_write.active_matches.insert(match_id, vec![entry_a.player_id, entry_b.player_id]);
                    
                    // Continue without incrementing as both were removed
                    continue;
                }
                i += 1;
            }
        }
    });
}

// Relayer and outome helper methods remain identical
async fn sign_match_outcome(match_id: &str, outcome: u8, transcript_hash: &[u8]) -> Result<Vec<u8>> {
    let key = env::var("VERIFIER_KEY")?;
    let wallet: LocalWallet = key.parse()?;
    let match_id_val = if let Ok(val) = U256::from_dec_str(match_id) { val } else { U256::from_big_endian(&ethers_core::utils::keccak256(match_id.as_bytes())) };
    let t_hash = if transcript_hash.len() == 32 { H256::from_slice(transcript_hash) } else { H256::zero() };
    let encoded = ethers_core::abi::encode(&[ethers_core::abi::Token::Uint(match_id_val), ethers_core::abi::Token::Uint(U256::from(outcome)), ethers_core::abi::Token::FixedBytes(t_hash.as_bytes().to_vec())]);
    let struct_hash = ethers_core::utils::keccak256(&encoded);
    let digest = hash_message(struct_hash);
    let signature = wallet.sign_hash(H256::from(digest))?;
    Ok(signature.to_vec())
}

async fn notify_relayer_rpc(match_id: &str, outcome: u8, transcript_hash: &[u8], signature: &[u8]) -> Result<()> {
    let relayer_rpc_addr = env::var("RELAYER_RPC_ADDR").unwrap_or_else(|_| "localhost:50052".to_string());
    let stream = tokio::net::TcpStream::connect(&relayer_rpc_addr).await?;
    let (reader, writer) = stream.into_split();
    let network = twoparty::VatNetwork::new(reader.compat(), writer.compat_write(), rpc_twoparty_capnp::Side::Client, Default::default());
    let mut rpc_system = RpcSystem::new(Box::new(network), None);
    let client: relayer_capnp::relayer_service::Client = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
    tokio::task::spawn_local(rpc_system);
    let mut req = client.submit_outcome_request();
    let match_id_val = if let Ok(val) = U256::from_dec_str(match_id) { let mut b=[0u8;32]; val.to_big_endian(&mut b); b.to_vec() } else { match_id.as_bytes().to_vec() };
    req.get().set_match_id(&match_id_val);
    req.get().set_outcome(outcome);
    req.get().set_transcript_hash(transcript_hash);
    req.get().set_signature(signature);
    let _ = req.send().promise.await?;
    info!("Successfully notified relayer via RPC for match {}", match_id);
    Ok(())
}

struct MatchSessionImpl { 
    match_id: String, 
    game_id: String, 
    players: Vec<String>,
    player_service: Arc<player_service::PlayerServiceImpl>,
}

impl match_session::Server for MatchSessionImpl {
    fn submit_outcome(&mut self, params: match_session::SubmitOutcomeParams, mut results: match_session::SubmitOutcomeResults) -> Promise<(), ::capnp::Error> {
        let submission = pry!(pry!(params.get()).get_submission());
        let outcome = pry!(submission.get_outcome());
        let outcome_val = outcome.get_victor();
        let replay_hash = pry!(submission.get_replay_hash());
        let r_hash = replay_hash.to_vec();

        let m_id = self.match_id.clone();
        let g_id = self.game_id.clone();
        let p_ids = self.players.clone();
        let p_service = self.player_service.clone();

        Promise::from_future(async move {
            match sign_match_outcome(&m_id, outcome_val, &r_hash).await {
                Ok(sig) => {
                    results.get().set_signature(&sig);
                    
                    // Purposeful integration: update player MMRs
                    if p_ids.len() >= 2 {
                        let p1 = &p_ids[0];
                        let p2 = &p_ids[1];
                        let score1 = if outcome_val == 1 { 1.0 } else if outcome_val == 2 { 0.0 } else { 0.5 };
                        let score2 = 1.0 - score1;
                        
                        let _ = p_service.record_match_result(p1, p2, &g_id, score1, 0).await;
                        let _ = p_service.record_match_result(p2, p1, &g_id, score2, 0).await;
                    }

                    tokio::task::spawn_local(async move {
                        if let Err(e) = notify_relayer_rpc(&m_id, outcome_val, &r_hash, &sig).await { error!("Failed to notify relayer for {}: {:?}", m_id, e); }
                    });
                    Ok(())
                }
                Err(e) => Err(::capnp::Error::failed(format!("Signer error: {}", e))),
            }
        })
    }
    fn subscribe_to_events(&mut self, _: match_session::SubscribeToEventsParams, _: match_session::SubscribeToEventsResults) -> Promise<(), ::capnp::Error> { Promise::ok(()) }
    fn emit_game_event(&mut self, _: match_session::EmitGameEventParams, _: match_session::EmitGameEventResults) -> Promise<(), ::capnp::Error> { Promise::ok(()) }
    fn emit_telemetry(&mut self, _params: match_session::EmitTelemetryParams, _: match_session::EmitTelemetryResults) -> Promise<(), ::capnp::Error> { Promise::ok(()) }
}

struct UserSessionImpl { 
    player_id: String, 
    game_id: u64, 
    state: AppState,
    player_service: Arc<player_service::PlayerServiceImpl>,
}

impl user_session::Server for UserSessionImpl {
    fn request_match(&mut self, params: user_session::RequestMatchParams, mut results: user_session::RequestMatchResults) -> Promise<(), ::capnp::Error> {
        let req = pry!(params.get()).get_req().unwrap();
        let game_id = self.game_id;
        let p_id = self.player_id.clone();
        let app_state = self.state.clone();
        let p_service = self.player_service.clone();
        
        let ruleset_id_bytes = req.get_rule_set_id().unwrap_or(&[]);
        let ruleset_id = String::from_utf8_lossy(ruleset_id_bytes).to_string();
        
        let mut mmr = 1200.0;
        let mut region = "na".to_string();
        let mut role = "".to_string();

        Promise::from_future(async move {
            {
                let s = app_state.read().await;
                if let Some(p) = s.players.get(&p_id) { mmr = p.global_mmr; region = p.region.clone(); role = p.preferred_role.clone(); }
            }

            let (tx, rx) = oneshot::channel();
            MATCH_QUEUE.lock().await.push(QueueEntry {
                player_id: p_id,
                game_id: game_id.to_string(),
                ruleset_id: ruleset_id.clone(),
                mmr,
                region,
                preferred_role: role,
                sender: tx,
            });

            if let Ok(payload) = rx.await {
                let mut assignment = results.get().init_assignment();
                assignment.set_match_id(payload.match_id.as_bytes());
                assignment.set_match_quality(payload.quality.total_score);
                let session = capnp_rpc::new_client(MatchSessionImpl { 
                    match_id: payload.match_id.clone(), 
                    game_id: game_id.to_string(), 
                    players: vec![payload.opponent_ids[0].clone()], // In a real system we'd pass all
                    player_service: p_service,
                });
                results.get().set_session(session);
                Ok(())
            } else {
                Err(::capnp::Error::failed("Queue closed".into()))
            }
        })
    }
    
    fn reconnect(&mut self, params: user_session::ReconnectParams, mut results: user_session::ReconnectResults) -> Promise<(), ::capnp::Error> {
        let match_id = String::from_utf8_lossy(pry!(pry!(params.get()).get_match_id())).to_string();
        let session = capnp_rpc::new_client(MatchSessionImpl { 
            match_id, 
            game_id: self.game_id.to_string(), 
            players: vec![], // Unknown on reconnect in this simplified state
            player_service: self.player_service.clone(),
        });
        results.get().set_session(session);
        Promise::ok(())
    }
}

// -----------------------------------------------------------------------------
// Core Entry
// -----------------------------------------------------------------------------

struct GameSessionServiceImpl { 
    state: AppState,
    player_service: Arc<player_service::PlayerServiceImpl>,
}

impl game_session_service::Server for GameSessionServiceImpl {
    fn login(&mut self, params: game_session_service::LoginParams, mut results: game_session_service::LoginResults) -> Promise<(), ::capnp::Error> {
        let game_id = pry!(params.get()).get_game_id();
        let sig_bytes = pry!(pry!(params.get()).get_signed_challenge()).to_vec();
        let player_id = String::from_utf8_lossy(&sig_bytes).to_string();
        let state = self.state.clone();
        let p_service = self.player_service.clone();

        Promise::from_future(async move {
            info!("Verified demo game {} logged in", game_id);
            let session = capnp_rpc::new_client(UserSessionImpl { player_id, game_id, state, player_service: p_service });
            results.get().set_session(session);
            Ok(())
        })
    }
}


#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let addr = env::var("AMP_ADDR").unwrap_or_else(|_| "0.0.0.0:50051".to_string());
    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("AMP Matchmaker (FlexMatch Edition) starting on {}", addr);

    let state = state::new_state();
    let p_service = Arc::new(player_service::PlayerServiceImpl { state: state.clone() });
    
    start_matchmaker_loop(state.clone()).await;

    let local = tokio::task::LocalSet::new();

    local.run_until(async move {
        while let Ok((stream, _)) = listener.accept().await {
            stream.set_nodelay(true).unwrap_or(());
            let s_clone = state.clone();
            let ps_clone = p_service.clone();
            tokio::task::spawn_local(async move {
                let (reader, writer) = stream.into_split();
                let network = twoparty::VatNetwork::new(reader.compat(), writer.compat_write(), rpc_twoparty_capnp::Side::Server, Default::default());
                let service: game_session_service::Client = capnp_rpc::new_client(GameSessionServiceImpl { state: s_clone, player_service: ps_clone });
                let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));
                if let Err(e) = rpc_system.await { error!("RPC error: {}", e); }
            });
        }
    }).await;

    Ok(())
}
