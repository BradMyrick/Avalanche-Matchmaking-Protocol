use anyhow::Result;
use capnp::capability::Promise;
use capnp_rpc::{RpcSystem, pry, rpc_twoparty_capnp, twoparty};
use ethers_core::types::{H256, U256};
use ethers_core::utils::hash_message;
use ethers_signers::{LocalWallet, Signer};
use futures_util::{AsyncReadExt, FutureExt, StreamExt};
use lazy_static::lazy_static;
use std::env;
use std::sync::Arc;
use tokio::sync::{Mutex, oneshot};
use tracing::{info, error};

use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use uuid::Uuid;

/// Auto-generated modules from Cap'n Proto schemas
pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}
pub mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
pub mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
pub mod relayer_capnp {
    include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs"));
}

use match_capnp::*;
use service_capnp::*;

lazy_static! {
    /// Global queue for players waiting for a match.
    static ref MATCH_QUEUE: Arc<Mutex<Vec<QueueEntry>>> = Arc::new(Mutex::new(Vec::new()));
}

/// Represents a player in the matchmaking queue.
struct QueueEntry {
    player_id: String,
    game_id: String,
    sender: oneshot::Sender<(String, String)>,
}
// QueueEntry

/// Signs a match outcome for submission to the settlement contract.
async fn sign_match_outcome(match_id: &str, outcome: u8, transcript_hash: &[u8]) -> Result<Vec<u8>> {
    let key = env::var("VERIFIER_KEY")?;
    let wallet: LocalWallet = key.parse()?;

    // Usenumeric match ID if possible, otherwise hash the string ID
    let match_id_val = if let Ok(val) = U256::from_dec_str(match_id) {
        val
    } else {
        U256::from_big_endian(&ethers_core::utils::keccak256(match_id.as_bytes()))
    };

    let t_hash = if transcript_hash.len() == 32 {
        H256::from_slice(transcript_hash)
    } else {
        H256::zero()
    };

    let encoded = ethers_core::abi::encode(&[
        ethers_core::abi::Token::Uint(match_id_val),
        ethers_core::abi::Token::Uint(U256::from(outcome)),
        ethers_core::abi::Token::FixedBytes(t_hash.as_bytes().to_vec()),
    ]);
    let struct_hash = ethers_core::utils::keccak256(&encoded);
    let digest = hash_message(struct_hash);

    let signature = wallet.sign_hash(H256::from(digest))?;
    Ok(signature.to_vec())
}
// sign_match_outcome(match_id, outcome, transcript_hash)

/// Notifies the Relayer of a match outcome via Cap'n Proto RPC.
async fn notify_relayer_rpc(match_id: &str, outcome: u8, transcript_hash: &[u8], signature: &[u8]) -> Result<()> {
    let relayer_rpc_addr = env::var("RELAYER_RPC_ADDR").unwrap_or_else(|_| "localhost:50052".to_string());
    
    let stream = tokio::net::TcpStream::connect(&relayer_rpc_addr).await?;
    let (reader, writer) = stream.into_split();
    let network = twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        rpc_twoparty_capnp::Side::Client,
        Default::default(),
    );
    
    let mut rpc_system = RpcSystem::new(Box::new(network), None);
    let client: relayer_capnp::relayer_service::Client = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
    tokio::task::spawn_local(rpc_system);

    let mut req = client.submit_outcome_request();
    let match_id_val = if let Ok(val) = U256::from_dec_str(match_id) {
        let mut b = [0u8; 32];
        val.to_big_endian(&mut b);
        b.to_vec()
    } else {
        match_id.as_bytes().to_vec()
    };
    
    req.get().set_match_id(&match_id_val);
    req.get().set_outcome(outcome);
    req.get().set_transcript_hash(transcript_hash);
    req.get().set_signature(signature);
    
    let _ = req.send().promise.await?;
    info!("Successfully notified relayer via RPC for match {}", match_id);
    Ok(())
}
// notify_relayer_rpc(match_id, outcome, transcript_hash, signature)

/// Implementation of the MatchSession Cap'n Proto service.
struct MatchSessionImpl {
    match_id: String,
    game_id: u64,
}
// MatchSessionImpl

impl match_session::Server for MatchSessionImpl {
    /// Submits the final outcome of a match.
    fn submit_outcome(
        &mut self,
        params: match_session::SubmitOutcomeParams,
        mut results: match_session::SubmitOutcomeResults,
    ) -> Promise<(), ::capnp::Error> {
        let submission = pry!(pry!(params.get()).get_submission());
        let match_id_bytes = pry!(submission.get_match_id());
        let match_id = String::from_utf8_lossy(match_id_bytes).to_string();
        let outcome = pry!(submission.get_outcome());
        let outcome_val = outcome.get_victor();
        let replay_hash = pry!(submission.get_replay_hash());
        let r_hash = replay_hash.to_vec();

        Promise::from_future(async move {
            match sign_match_outcome(&match_id, outcome_val, &r_hash).await {
                Ok(sig) => {
                    results.get().set_signature(&sig);
                    let m_id = match_id.clone();
                    tokio::task::spawn_local(async move {
                        if let Err(e) = notify_relayer_rpc(&m_id, outcome_val, &r_hash, &sig).await {
                            error!("Failed to notify relayer for {}: {:?}", m_id, e);
                        }
                    });
                    Ok(())
                }
                Err(e) => Err(::capnp::Error::failed(format!("Signer error: {}", e))),
            }
        })
    }
    // submit_outcome(params, results)

    fn subscribe_to_events(
        &mut self,
        _params: match_session::SubscribeToEventsParams,
        _results: match_session::SubscribeToEventsResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }
    // subscribe_to_events(params, results)

    fn emit_game_event(
        &mut self,
        _params: match_session::EmitGameEventParams,
        _results: match_session::EmitGameEventResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }
    // emit_game_event(params, results)

    /// Forwards telemetry events to the binary logger.
    fn emit_telemetry(
        &mut self,
        params: match_session::EmitTelemetryParams,
        _results: match_session::EmitTelemetryResults,
    ) -> Promise<(), ::capnp::Error> {
        let params_reader = pry!(params.get());
        let event_reader = pry!(params_reader.get_event());
        let event_type = event_reader.get_event_type().unwrap();
        let timestamp = event_reader.get_timestamp();
        let event_data = event_reader.get_event_data().unwrap_or(&[]).to_vec();
        
        let game_id = self.game_id;
        let match_id = self.match_id.clone();

        Promise::from_future(async move {
            let telemetry_addr = env::var("TELEMETRY_ADDR").unwrap_or_else(|_| "127.0.0.1:4317".to_string());
            if let Ok(stream) = tokio::net::TcpStream::connect(&telemetry_addr).await {
                let (reader, writer) = stream.into_split();
                let network = twoparty::VatNetwork::new(
                    reader.compat(),
                    writer.compat_write(),
                    rpc_twoparty_capnp::Side::Client,
                    Default::default(),
                );
                
                let mut rpc_system = RpcSystem::new(Box::new(network), None);
                let receiver: amp_telemetry_capnp::telemetry_receiver::Client = 
                    rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
                tokio::task::spawn_local(rpc_system);

                let mut req = receiver.log_event_request();
                let mut ev = req.get().init_event();
                ev.set_match_id(match_id.as_bytes());
                ev.set_game_id(game_id);
                ev.set_event_type(event_type);
                ev.set_timestamp(timestamp);
                ev.set_event_data(&event_data);

                let _ = req.send().promise.await;
            }
            Ok(())
        })
    }
    // emit_telemetry(params, results)
}
// match_session::Server for MatchSessionImpl

/// Implementation of the UserSession Cap'n Proto service.
struct UserSessionImpl {
    player_id: String,
    game_id: u64,
}
// UserSessionImpl

impl user_session::Server for UserSessionImpl {
    /// Requests a new match or joins the queue.
    fn request_match(
        &mut self,
        _params: user_session::RequestMatchParams,
        mut results: user_session::RequestMatchResults,
    ) -> Promise<(), ::capnp::Error> {
        let game_id = self.game_id;
        let game_id_str = game_id.to_string();
        let player_id_clone = self.player_id.clone();

        Promise::from_future(async move {
            let (tx, rx) = oneshot::channel();
            let mut queue = MATCH_QUEUE.lock().await;

            if let Some(pos) = queue.iter().position(|e| e.game_id == game_id_str) {
                let entry = queue.remove(pos);
                let match_id = format!("match-{}", Uuid::new_v4());

                let _ = entry.sender.send((match_id.clone(), player_id_clone.clone()));

                let mut assignment = results.get().init_assignment();
                assignment.set_match_id(match_id.as_bytes());

                let session = capnp_rpc::new_client(MatchSessionImpl {
                    match_id,
                    game_id,
                });
                results.get().set_session(session);
                Ok(())
            } else {
                queue.push(QueueEntry {
                    player_id: player_id_clone,
                    game_id: game_id_str,
                    sender: tx,
                });
                drop(queue);

                if let Ok((assign_match_id, _)) = rx.await {
                    let mut assignment = results.get().init_assignment();
                    assignment.set_match_id(assign_match_id.as_bytes());

                    let session = capnp_rpc::new_client(MatchSessionImpl {
                        match_id: assign_match_id,
                        game_id,
                    });
                    results.get().set_session(session);
                    Ok(())
                } else {
                    Err(::capnp::Error::failed("Matchmaking cancelled".to_string()))
                }
            }
        })
    }
    // request_match(params, results)

    fn reconnect(
        &mut self,
        params: user_session::ReconnectParams,
        mut results: user_session::ReconnectResults,
    ) -> Promise<(), ::capnp::Error> {
        let match_id_bytes = pry!(pry!(params.get()).get_match_id());
        let match_id = String::from_utf8_lossy(match_id_bytes).to_string();
        let game_id = self.game_id;

        let session = capnp_rpc::new_client(MatchSessionImpl {
            match_id,
            game_id,
        });
        results.get().set_session(session);
        Promise::ok(())
    }
    // reconnect(params, results)
}
// user_session::Server for UserSessionImpl

/// Implementation of the core GameSessionService.
struct GameSessionServiceImpl;
// GameSessionServiceImpl

impl game_session_service::Server for GameSessionServiceImpl {
    /// Handles initial player login and studio admin verification.
    fn login(
        &mut self,
        params: game_session_service::LoginParams,
        mut results: game_session_service::LoginResults,
    ) -> Promise<(), ::capnp::Error> {
        let params_reader = pry!(params.get());
        let game_id = params_reader.get_game_id();
        let sig_bytes = pry!(params_reader.get_signed_challenge());
        
        let player_id = String::from_utf8_lossy(sig_bytes).to_string();

        Promise::from_future(async move {
            let relayer_rpc_addr = env::var("RELAYER_RPC_ADDR").unwrap_or_else(|_| "localhost:50052".to_string());
            
            let stream = tokio::net::TcpStream::connect(&relayer_rpc_addr).await
                .map_err(|e| ::capnp::Error::failed(format!("Failed to connect to relayer: {:?}", e)))?;
            let (reader, writer) = stream.into_split();
            let network = twoparty::VatNetwork::new(
                reader.compat(),
                writer.compat_write(),
                rpc_twoparty_capnp::Side::Client,
                Default::default(),
            );
            
            let mut rpc_system = RpcSystem::new(Box::new(network), None);
            let client: relayer_capnp::relayer_service::Client = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
            tokio::task::spawn_local(rpc_system);

            let mut req = client.get_game_admin_request();
            req.get().set_game_id(game_id);
            
            let admin_res = req.send().promise.await
                .map_err(|e| ::capnp::Error::failed(format!("Relayer admin query failed: {:?}", e)))?;
            
            if let Some(admin_addr_raw) = admin_res.get()?.get_admin().ok() {
                if admin_addr_raw.is_empty() {
                    return Err(::capnp::Error::failed(format!("Unauthorized: Game {} not found", game_id)));
                }

                let admin_addr = ethers_core::types::Address::from_slice(admin_addr_raw);
                
                // Verify signature: client signs the game_id as a U256
                let mut challenge_bytes = [0u8; 32];
                ethers_core::types::U256::from(game_id).to_big_endian(&mut challenge_bytes);
                let message_hash = ethers_core::utils::hash_message(ethers_core::utils::keccak256(&challenge_bytes));
                
                let signature = ethers_signers::Signature::try_from(sig_bytes)
                    .map_err(|e| ::capnp::Error::failed(format!("Invalid signature format: {:?}", e)))?;
                
                if let Ok(recovered_addr) = signature.recover(message_hash) {
                    if recovered_addr == admin_addr {
                        info!("Verified game {} admin {} successfully logged in", game_id, recovered_addr);
                        let session = capnp_rpc::new_client(UserSessionImpl { 
                            player_id,
                            game_id,
                        });
                        results.get().set_session(session);
                        Ok(())
                    } else {
                        Err(::capnp::Error::failed(format!("Signature mismatch: expected {}, got {}", admin_addr, recovered_addr)))
                    }
                } else {
                    Err(::capnp::Error::failed("Failed to recover address from signature".to_string()))
                }
            } else {
                Err(::capnp::Error::failed(format!("Unauthorized: Game {} not found", game_id)))
            }
        })
    }
    // login(params, results)
}
// game_session_service::Server for GameSessionServiceImpl

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let addr = env::var("AMP_ADDR").unwrap_or_else(|_| "0.0.0.0:50051".to_string());
    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("AMP Matchmaker (Pure Cap'n Proto RPC) starting on {}", addr);

    let local = tokio::task::LocalSet::new();

    local.run_until(async move {
        while let Ok((stream, _)) = listener.accept().await {
            stream.set_nodelay(true).unwrap_or(());
            tokio::task::spawn_local(async move {
                let (reader, writer) = stream.into_split();
                let network = twoparty::VatNetwork::new(
                    reader.compat(),
                    writer.compat_write(),
                    rpc_twoparty_capnp::Side::Server,
                    Default::default(),
                );

                let service: game_session_service::Client = capnp_rpc::new_client(GameSessionServiceImpl);
                let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));

                if let Err(e) = rpc_system.await {
                    error!("RPC system error: {}", e);
                }
            });
        }
    }).await;

    Ok(())
}
// main()
