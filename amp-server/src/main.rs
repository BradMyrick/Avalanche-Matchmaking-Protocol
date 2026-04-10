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
use tracing::{info, error, warn};

mod state;
mod matchmaker;
mod player_service;
mod persistence;
mod auth;
mod rules;

use state::{AppState, QueueEntry, MatchFoundPayload, ActiveMatch, now_ms};

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

lazy_static! {
    static ref MATCH_QUEUE: Arc<tokio::sync::Mutex<Vec<QueueEntry>>> = Arc::new(tokio::sync::Mutex::new(Vec::new()));
}

async fn start_matchmaker_loop(state: AppState, cancel: tokio_util::sync::CancellationToken) {
    tokio::spawn(async move {
        loop {
            tokio::select! {
                _ = cancel.cancelled() => {
                    info!("Matchmaker loop shutting down, draining queue...");
                    let mut queue = MATCH_QUEUE.lock().await;
                    for entry in queue.drain(..) {
                        let _ = entry.sender.send(MatchFoundPayload {
                            match_id: String::new(),
                            opponent_ids: vec![],
                            quality: Default::default(),
                        });
                    }
                    info!("Queue drained.");
                    return;
                }
                _ = tokio::time::sleep(Duration::from_millis(500)) => {}
            }

            let mut queue = MATCH_QUEUE.lock().await;
            if queue.len() < 2 { continue; }

            let mut i = 0;
            while i < queue.len() {
                let entry_a = &queue[i];

                let s = state.read().await;
                let ruleset = s.rulesets.get(&entry_a.ruleset_id).cloned().unwrap_or_default();
                drop(s);

                let sub_queue = &queue[i + 1..];
                if let Some(mut target_idx) = matchmaker::find_match(sub_queue, entry_a, &ruleset) {
                    target_idx += i + 1;

                    let entry_b = queue.remove(target_idx);
                    let entry_a = queue.remove(i);

                    let quality = matchmaker::compute_match_quality(&entry_a, &entry_b, &ruleset);
                    let match_id = Uuid::new_v4().to_string();
                    info!(
                        "Match found! [{}] vs [{}] Quality={:.2} (skill={:.2} role={:.2} region={:.2})",
                        entry_a.player_id, entry_b.player_id, quality.total_score,
                        quality.skill_balance, quality.role_balance, quality.region_score
                    );

                    let p1 = MatchFoundPayload {
                        match_id: match_id.clone(),
                        opponent_ids: vec![entry_b.player_id.clone()],
                        quality: quality.clone(),
                    };
                    let p2 = MatchFoundPayload {
                        match_id: match_id.clone(),
                        opponent_ids: vec![entry_a.player_id.clone()],
                        quality,
                    };

                    let _ = entry_a.sender.send(p1);
                    let _ = entry_b.sender.send(p2);

                    let mut s_write = state.write().await;
                    let active = ActiveMatch {
                        match_id: match_id.clone(),
                        game_id: entry_a.game_id.clone(),
                        players: vec![entry_a.player_id, entry_b.player_id],
                        created_at_ms: now_ms(),
                        settled: false,
                    };
                    s_write.active_matches.insert(match_id, active.clone());
                    s_write.persist_match(&active.match_id, &active);

                    continue;
                }
                i += 1;
            }
        }
    });
}

async fn sign_match_outcome(match_id: &str, outcome: u8, transcript_hash: &[u8]) -> Result<Vec<u8>> {
    let key = env::var("VERIFIER_KEY")?;
    let wallet: LocalWallet = key.parse()?;
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

async fn notify_relayer_rpc(match_id: &str, outcome: u8, transcript_hash: &[u8], signature: &[u8]) -> Result<()> {
    let relayer_rpc_addr =
        env::var("RELAYER_RPC_ADDR").unwrap_or_else(|_| "localhost:50052".to_string());
    let stream = tokio::net::TcpStream::connect(&relayer_rpc_addr).await?;
    let (reader, writer) = stream.into_split();
    let network = twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        rpc_twoparty_capnp::Side::Client,
        Default::default(),
    );
    let mut rpc_system = RpcSystem::new(Box::new(network), None);
    let client: relayer_capnp::relayer_service::Client =
        rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
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
    info!("Notified relayer for match {}", match_id);
    Ok(())
}

async fn forward_telemetry(event: &[u8]) {
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
        let client: amp_telemetry_capnp::telemetry_receiver::Client =
            rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
        tokio::task::spawn_local(rpc_system);
        let mut req = client.log_event_request();
        let mut evt = req.get().init_event();
        evt.set_event_data(event);
        let _ = req.send().promise.await;
    } else {
        warn!("Failed to connect to telemetry service at {}", telemetry_addr);
    }
}

struct MatchSessionImpl {
    match_id: String,
    game_id: String,
    players: Vec<String>,
    player_service: Arc<player_service::PlayerServiceImpl>,
    state: AppState,
}

impl match_session::Server for MatchSessionImpl {
    fn submit_outcome(
        &mut self,
        params: match_session::SubmitOutcomeParams,
        mut results: match_session::SubmitOutcomeResults,
    ) -> Promise<(), ::capnp::Error> {
        let submission = pry!(pry!(params.get()).get_submission());
        let outcome = pry!(submission.get_outcome());
        let outcome_val = outcome.get_victor();
        let replay_hash = pry!(submission.get_replay_hash());
        let r_hash = replay_hash.to_vec();

        let m_id = self.match_id.clone();
        let g_id = self.game_id.clone();
        let p_ids = self.players.clone();
        let p_service = self.player_service.clone();
        let state = self.state.clone();

        Promise::from_future(async move {
            match sign_match_outcome(&m_id, outcome_val, &r_hash).await {
                Ok(sig) => {
                    results.get().set_signature(&sig);

                    if p_ids.len() >= 2 {
                        let p1 = &p_ids[0];
                        let p2 = &p_ids[1];
                        let score1 = if outcome_val == 1 {
                            1.0
                        } else if outcome_val == 2 {
                            0.0
                        } else {
                            0.5
                        };
                        let score2 = 1.0 - score1;

                        let _ = p_service
                            .record_match_result(p1, p2, &g_id, score1, 0)
                            .await;
                        let _ = p_service
                            .record_match_result(p2, p1, &g_id, score2, 0)
                            .await;
                    }

                    {
                        let match_id_for_update = m_id.clone();
                        let mut s = state.write().await;
                        if let Some(m) = s.active_matches.get_mut(&match_id_for_update) {
                            m.settled = true;
                            let match_id_persist = m.match_id.clone();
                            let m_clone = ActiveMatch {
                                match_id: m.match_id.clone(),
                                game_id: m.game_id.clone(),
                                players: m.players.clone(),
                                created_at_ms: m.created_at_ms,
                                settled: true,
                            };
                            drop(s);
                            let mut s2 = state.write().await;
                            s2.persist_match(&match_id_persist, &m_clone);
                        }
                    }

                    tokio::task::spawn_local(async move {
                        if let Err(e) = notify_relayer_rpc(&m_id, outcome_val, &r_hash, &sig).await
                        {
                            error!("Failed to notify relayer for {}: {:?}", m_id, e);
                        }
                    });
                    Ok(())
                }
                Err(e) => Err(::capnp::Error::failed(format!("Signer error: {}", e))),
            }
        })
    }

    fn subscribe_to_events(
        &mut self,
        params: match_session::SubscribeToEventsParams,
        _: match_session::SubscribeToEventsResults,
    ) -> Promise<(), ::capnp::Error> {
        let subscriber = pry!(pry!(params.get()).get_subscriber());
        let match_id = self.match_id.clone();
        info!("Client subscribed to events for match {}", match_id);
        let _ = subscriber;
        Promise::ok(())
    }

    fn emit_game_event(
        &mut self,
        params: match_session::EmitGameEventParams,
        _: match_session::EmitGameEventResults,
    ) -> Promise<(), ::capnp::Error> {
        let event = pry!(pry!(params.get()).get_event());
        let event_type = event
            .get_event_type()
            .ok()
            .and_then(|s| s.to_string().ok())
            .unwrap_or_default();
        info!(
            "Game event in match {}: {}",
            self.match_id,
            event_type
        );
        Promise::ok(())
    }

    fn emit_telemetry(
        &mut self,
        params: match_session::EmitTelemetryParams,
        _: match_session::EmitTelemetryResults,
    ) -> Promise<(), ::capnp::Error> {
        let event_data = pry!(pry!(params.get()).get_event());
        let raw = event_data
            .get_event_data()
            .unwrap_or_default()
            .to_vec();
        Promise::from_future(async move {
            forward_telemetry(&raw).await;
            Ok(())
        })
    }
}

struct UserSessionImpl {
    player_id: String,
    game_id: u64,
    state: AppState,
    player_service: Arc<player_service::PlayerServiceImpl>,
}

impl user_session::Server for UserSessionImpl {
    fn request_match(
        &mut self,
        params: user_session::RequestMatchParams,
        mut results: user_session::RequestMatchResults,
    ) -> Promise<(), ::capnp::Error> {
        let req = pry!(params.get()).get_req().unwrap();
        let game_id = self.game_id;
        let p_id = self.player_id.clone();
        let app_state = self.state.clone();
        let p_service = self.player_service.clone();

        let ruleset_id_bytes = req.get_rule_set_id().unwrap_or(&[]);
        let ruleset_id = String::from_utf8_lossy(ruleset_id_bytes).to_string();

        let mut mmr = 1200.0;
        let mut mmr_unc = 350.0;
        let mut region = "na".to_string();
        let mut role = String::new();
        let mut language = "en".to_string();
        let mut max_ping = 150;

        Promise::from_future(async move {
            {
                let s = app_state.read().await;
                if let Some(p) = s.players.get(&p_id) {
                    mmr = p.global_mmr;
                    mmr_unc = p.mmr_uncertainty;
                    region = p.region.clone();
                    role = p.preferred_role.clone();
                    language = p.language.clone();
                    max_ping = p.max_ping_ms;

                    if p.restrictions.is_banned {
                        return Err(::capnp::Error::failed("Player is banned".into()));
                    }
                    let now = now_ms();
                    if p.restrictions.matchmaking_cooldown_until > now {
                        return Err(::capnp::Error::failed(format!(
                            "Player is in cooldown for {}ms",
                            p.restrictions.matchmaking_cooldown_until - now
                        )));
                    }
                }
            }

            let (tx, rx) = oneshot::channel();
            MATCH_QUEUE.lock().await.push(QueueEntry {
                player_id: p_id.clone(),
                game_id: game_id.to_string(),
                ruleset_id: ruleset_id.clone(),
                mmr,
                mmr_uncertainty: mmr_unc,
                region,
                preferred_role: role,
                language,
                max_ping_ms: max_ping,
                enqueued_at_ms: now_ms(),
                sender: tx,
            });

            if let Ok(payload) = rx.await {
                if payload.match_id.is_empty() {
                    return Err(::capnp::Error::failed("Queue drained (server shutting down)".into()));
                }

                let mut assignment = results.get().init_assignment();
                assignment.set_match_id(payload.match_id.as_bytes());
                assignment.set_match_quality(payload.quality.total_score);
                let session = capnp_rpc::new_client(MatchSessionImpl {
                    match_id: payload.match_id.clone(),
                    game_id: game_id.to_string(),
                    players: std::iter::once(p_id.clone())
                        .chain(payload.opponent_ids.iter().cloned())
                        .collect(),
                    player_service: p_service,
                    state: app_state,
                });
                results.get().set_session(session);
                Ok(())
            } else {
                Err(::capnp::Error::failed("Queue closed".into()))
            }
        })
    }

    fn reconnect(
        &mut self,
        params: user_session::ReconnectParams,
        mut results: user_session::ReconnectResults,
    ) -> Promise<(), ::capnp::Error> {
        let match_id =
            String::from_utf8_lossy(pry!(pry!(params.get()).get_match_id())).to_string();
        let state = self.state.clone();
        let p_service = self.player_service.clone();
        let game_id = self.game_id;
        let player_id = self.player_id.clone();

        Promise::from_future(async move {
            let s = state.read().await;
            match s.active_matches.get(&match_id) {
                Some(m) if !m.settled => {
                    let session = capnp_rpc::new_client(MatchSessionImpl {
                        match_id: match_id.clone(),
                        game_id: m.game_id.clone(),
                        players: m.players.clone(),
                        player_service: p_service,
                        state: state.clone(),
                    });
                    drop(s);
                    results.get().set_session(session);
                    info!(
                        "Player {} reconnected to match {}",
                        player_id, match_id
                    );
                    Ok(())
                }
                Some(m) => {
                    Err(::capnp::Error::failed(format!(
                        "Match {} is already settled",
                        match_id
                    )))
                }
                None => Err(::capnp::Error::failed(format!(
                    "Match {} not found",
                    match_id
                ))),
            }
        })
    }
}

struct GameSessionServiceImpl {
    state: AppState,
    player_service: Arc<player_service::PlayerServiceImpl>,
    auth_service: Arc<auth::AuthService>,
}

impl game_session_service::Server for GameSessionServiceImpl {
    fn login(
        &mut self,
        params: game_session_service::LoginParams,
        mut results: game_session_service::LoginResults,
    ) -> Promise<(), ::capnp::Error> {
        let game_id = pry!(params.get()).get_game_id();
        let sig_bytes = pry!(pry!(params.get()).get_signed_challenge()).to_vec();

        let state = self.state.clone();
        let p_service = self.player_service.clone();
        let auth = self.auth_service.clone();

        Promise::from_future(async move {
            match auth.verify_login(game_id, &sig_bytes).await {
                Ok(address) => {
                    let player_id = format!("0x{}", hex::encode(address.as_bytes()));

                    {
                        let mut s = state.write().await;
                        let profile = s
                            .players
                            .entry(player_id.clone())
                            .or_insert_with(state::StoredPlayerProfile::default);
                        profile.wallet_address = address.as_bytes().to_vec();
                        profile.is_online = true;
                        profile.last_login = state::now_ns();
                        let profile_clone = profile.clone();
                        s.persist_player(&player_id, &profile_clone);
                    }

                    info!("Authenticated player {} for game {}", player_id, game_id);
                    let session = capnp_rpc::new_client(UserSessionImpl {
                        player_id,
                        game_id,
                        state,
                        player_service: p_service,
                    });
                    results.get().set_session(session);
                    Ok(())
                }
                Err(e) => {
                    warn!("Authentication failed for game {}: {}", game_id, e);
                    Err(::capnp::Error::failed(format!(
                        "Authentication failed: {}",
                        e
                    )))
                }
            }
        })
    }
}

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let addr = env::var("AMP_ADDR").unwrap_or_else(|_| "0.0.0.0:50051".to_string());
    let db_path = env::var("AMP_DB_PATH").unwrap_or_else(|_| "./amp-data".to_string());

    info!("AMP Matchmaker (FlexMatch Edition) starting on {}", addr);
    info!("Persistence layer at: {}", db_path);

    let persistence_result = persistence::Persistence::open(&db_path);
    let persist = match persistence_result {
        Ok(p) => {
            info!("RocksDB persistence layer initialized");
            Some(p)
        }
        Err(e) => {
            warn!("Persistence unavailable (running in-memory): {}", e);
            None
        }
    };

    let state = state::new_state(persist);
    let p_service = Arc::new(player_service::PlayerServiceImpl {
        state: state.clone(),
    });
    let auth_service = Arc::new(auth::AuthService::new());

    let cancel = tokio_util::sync::CancellationToken::new();
    let cancel_clone = cancel.clone();

    start_matchmaker_loop(state.clone(), cancel.clone()).await;

    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("Listening on {}", addr);

    let local = tokio::task::LocalSet::new();

    local
        .run_until(async move {
            let state_for_shutdown = state.clone();
            tokio::select! {
                _ = tokio::signal::ctrl_c() => {
                    info!("Received shutdown signal, draining...");
                    cancel_clone.cancel();

                    let s = state_for_shutdown.read().await;
                    let active = s.active_matches.iter()
                        .filter(|(_, m)| !m.settled)
                        .count();
                    info!("Active unsettled matches: {}", active);
                    drop(s);

                    tokio::time::sleep(Duration::from_millis(500)).await;
                    info!("Shutdown complete.");
                }
                _ = accept_loop(listener, state, p_service, auth_service) => {}
            }
        })
        .await;

    Ok(())
}

async fn accept_loop(
    listener: tokio::net::TcpListener,
    state: AppState,
    p_service: Arc<player_service::PlayerServiceImpl>,
    auth_service: Arc<auth::AuthService>,
) {
    while let Ok((stream, _)) = listener.accept().await {
        stream.set_nodelay(true).unwrap_or(());
        let s_clone = state.clone();
        let ps_clone = p_service.clone();
        let auth_clone = auth_service.clone();
        tokio::task::spawn_local(async move {
            let (reader, writer) = stream.into_split();
            let network = twoparty::VatNetwork::new(
                reader.compat(),
                writer.compat_write(),
                rpc_twoparty_capnp::Side::Server,
                Default::default(),
            );
            let service: game_session_service::Client = capnp_rpc::new_client(
                GameSessionServiceImpl {
                    state: s_clone,
                    player_service: ps_clone,
                    auth_service: auth_clone,
                },
            );
            let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));
            if let Err(e) = rpc_system.await {
                error!("RPC error: {}", e);
            }
        });
    }
}
