use anyhow::Result;
use capnp::capability::Promise;
use capnp_rpc::{RpcSystem, pry, rpc_twoparty_capnp, twoparty};
use ethers_core::types::{Address, H256, U256};
use ethers_signers::LocalWallet;
use std::env;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::oneshot;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use tracing::{error, info, warn};
use uuid::Uuid;

mod auth;
mod match_queue;
mod matchmaker;
mod persistence;
mod player_service;
mod rules;
mod state;

use state::{ActiveMatch, AppState, MatchFoundPayload, QueueEntry, StoredRuleSet, now_ms};

#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod relayer_capnp {
    include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod player_profile_capnp {
    include!(concat!(env!("OUT_DIR"), "/player_profile_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod matchmaking_rules_capnp {
    include!(concat!(env!("OUT_DIR"), "/matchmaking_rules_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_registry_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_registry_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod inventory_capnp {
    include!(concat!(env!("OUT_DIR"), "/inventory_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod tournament_capnp {
    include!(concat!(env!("OUT_DIR"), "/tournament_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod security_capnp {
    include!(concat!(env!("OUT_DIR"), "/security_capnp.rs"));
}

use service_capnp::*;

type MatchQueue = tokio::sync::mpsc::Sender<QueueEntry>;

mod rate_limit {
    use std::collections::HashMap;
    use std::net::IpAddr;
    use std::sync::Arc;
    use std::sync::Mutex;
    use std::sync::atomic::{AtomicUsize, Ordering};
    use std::time::Instant;

    pub struct ConnectionRateLimiter {
        inner: Mutex<Inner>,
        max_per_ip_per_minute: usize,
        current_connections: AtomicUsize,
        max_concurrent: usize,
    }

    struct Inner {
        ip_windows: HashMap<IpAddr, Vec<Instant>>,
    }

    pub struct ConnectionGuard(Arc<ConnectionRateLimiter>);

    impl Drop for ConnectionGuard {
        fn drop(&mut self) {
            self.0.current_connections.fetch_sub(1, Ordering::Release);
        }
    }

    impl ConnectionRateLimiter {
        pub fn new(max_concurrent: usize, max_per_ip_per_minute: usize) -> Self {
            Self {
                inner: Mutex::new(Inner {
                    ip_windows: HashMap::new(),
                }),
                max_per_ip_per_minute,
                current_connections: AtomicUsize::new(0),
                max_concurrent,
            }
        }

        pub fn check_ip(&self, ip: IpAddr) -> bool {
            let mut inner = self.inner.lock().unwrap_or_else(|e| e.into_inner());
            let now = Instant::now();
            let window = inner.ip_windows.entry(ip).or_default();
            window.retain(|t| now.duration_since(*t).as_secs() < 60);
            if window.len() >= self.max_per_ip_per_minute {
                return false;
            }
            window.push(now);
            true
        }

        pub fn try_acquire_slot(self: &Arc<Self>) -> Option<ConnectionGuard> {
            loop {
                let current = self.current_connections.load(Ordering::Acquire);
                if current >= self.max_concurrent {
                    return None;
                }
                if self
                    .current_connections
                    .compare_exchange_weak(
                        current,
                        current + 1,
                        Ordering::AcqRel,
                        Ordering::Relaxed,
                    )
                    .is_ok()
                {
                    return Some(ConnectionGuard(self.clone()));
                }
            }
        }
    }
}

fn load_secret(env_var: &str, file_env_var: &str) -> Result<String> {
    if let Ok(path) = env::var(file_env_var) {
        let contents = std::fs::read_to_string(&path)
            .map_err(|e| anyhow::anyhow!("failed to read {} from {}: {}", env_var, path, e))?;
        return Ok(contents.trim().to_string());
    }
    env::var(env_var)
        .map_err(|e| anyhow::anyhow!("{} (or {}) not set: {}", env_var, file_env_var, e))
}

async fn start_matchmaker_loop(
    state: AppState,
    mut rx: tokio::sync::mpsc::Receiver<QueueEntry>,
    cancel: tokio_util::sync::CancellationToken,
) {
    tokio::spawn(async move {
        let mut q = match_queue::IndexedQueue::new();
        let mut rulesets_cache = arc_swap::cache::Cache::new(state.rulesets.clone());
        let mut active_matches_cache = arc_swap::cache::Cache::new(state.active_matches.clone());
        loop {
            tokio::select! {
                _ = cancel.cancelled() => {
                    info!("Matchmaker loop shutting down, draining queue...");
                    for entry in q.drain_all() {
                        let _ = entry.sender.send(MatchFoundPayload {
                            match_id: String::new(),
                            opponent_ids: vec![],
                            quality: Default::default(),
                        });
                    }
                    info!("Queue drained.");
                    return;
                }
                _ = tokio::time::sleep(Duration::from_millis(50)) => {}
            }

            while let Ok(entry) = rx.try_recv() {
                q.push(entry);
            }

            let rulesets_snapshot = rulesets_cache.load();
            let mut active_count = active_matches_cache.load().len();

            if q.len() < 2 {
                continue;
            }

            let keys = q.bucket_keys();
            for key in keys {
                let ruleset = rulesets_snapshot
                    .get(&key.1)
                    .cloned()
                    .unwrap_or_else(StoredRuleSet::default_arc);

                loop {
                    let result =
                        q.try_match_bucket(&key, &ruleset, state::MAX_ACTIVE_MATCHES, active_count);
                    let m = match result {
                        Some(m) => m,
                        None => break,
                    };

                    let match_id = Uuid::new_v4().to_string();
                    info!(
                        "Match found! [{}] vs [{}] Quality={:.2} (skill={:.2} role={:.2} region={:.2})",
                        m.entry_a.player_id,
                        m.entry_b.player_id,
                        m.quality.total_score,
                        m.quality.skill_balance,
                        m.quality.role_balance,
                        m.quality.region_score
                    );

                    let p1 = MatchFoundPayload {
                        match_id: match_id.clone(),
                        opponent_ids: vec![m.entry_b.player_id.clone()],
                        quality: m.quality.clone(),
                    };
                    let p2 = MatchFoundPayload {
                        match_id: match_id.clone(),
                        opponent_ids: vec![m.entry_a.player_id.clone()],
                        quality: m.quality,
                    };

                    if let Err(e) = m.entry_a.sender.send(p1) {
                        error!("Failed to notify player {} of match {}: {:?}", m.entry_a.player_id, match_id, e);
                    }
                    if let Err(e) = m.entry_b.sender.send(p2) {
                        error!("Failed to notify player {} of match {}: {:?}", m.entry_b.player_id, match_id, e);
                    }

                    let now = now_ms();
                    let active = ActiveMatch {
                        match_id: match_id.clone(),
                        game_id: m.entry_a.game_id.clone(),
                        players: vec![m.entry_a.player_id, m.entry_b.player_id],
                        created_at_ms: now,
                        settled: false,
                        settled_at_ms: None,
                        expires_at_ms: Some(now + state::MATCH_TTL_MS),
                    };
                    state.insert_active_match(match_id, active.clone());
                    state.persist_match(&active.match_id, &active).await;
                    active_count += 1;
                }
            }
        }
    });
}

fn start_cleanup_loop(
    state: AppState,
    auth_service: Arc<auth::AuthService>,
    cancel: tokio_util::sync::CancellationToken,
) {
    tokio::spawn(async move {
        loop {
            tokio::select! {
                _ = cancel.cancelled() => {
                    info!("Cleanup loop shutting down");
                    return;
                }
                _ = tokio::time::sleep(Duration::from_secs(60)) => {}
            }

            {
                let removed = state.cleanup_expired_matches().await;
                if removed > 0 {
                    info!("Cleanup: removed {} expired matches", removed);
                }
            }

            auth_service.cleanup_expired().await;
        }
    });
}

pub(crate) struct SigningConfig {
    wallet: LocalWallet,
    chain_id: u64,
    settlement_addr: Address,
}

async fn sign_match_outcome(
    config: Arc<SigningConfig>,
    match_id: &str,
    outcome: u8,
    transcript_hash: &[u8],
) -> Result<Vec<u8>> {
    let chain_id = config.chain_id;
    let settlement_addr = config.settlement_addr;

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

    let async_result_typehash: [u8; 32] = ethers_core::utils::keccak256(
        "AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)".as_bytes(),
    );
    let struct_hash = ethers_core::utils::keccak256(ethers_core::abi::encode(&[
        ethers_core::abi::Token::FixedBytes(async_result_typehash.to_vec()),
        ethers_core::abi::Token::Uint(match_id_val),
        ethers_core::abi::Token::Uint(U256::from(outcome)),
        ethers_core::abi::Token::FixedBytes(t_hash.as_bytes().to_vec()),
    ]));

    let eip712_domain_typehash: [u8; 32] = ethers_core::utils::keccak256(
        "EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"
            .as_bytes(),
    );
    let domain_separator = ethers_core::utils::keccak256(ethers_core::abi::encode(&[
        ethers_core::abi::Token::FixedBytes(eip712_domain_typehash.to_vec()),
        ethers_core::abi::Token::String("AMPSettlement".to_string()),
        ethers_core::abi::Token::String("1".to_string()),
        ethers_core::abi::Token::Uint(U256::from(chain_id)),
        ethers_core::abi::Token::Address(settlement_addr),
    ]));

    let mut digest_input = vec![0x19, 0x01];
    digest_input.extend_from_slice(&domain_separator);
    digest_input.extend_from_slice(&struct_hash);
    let digest = H256::from_slice(&ethers_core::utils::keccak256(&digest_input));

    let signature = tokio::task::spawn_blocking(move || config.wallet.sign_hash(digest))
        .await
        .map_err(|e| anyhow::anyhow!("Spawn blocking failed: {}", e))??;

    Ok(signature.to_vec())
}

pub struct RelayerTask {
    match_id: String,
    outcome: u8,
    transcript_hash: Vec<u8>,
    signature: Vec<u8>,
}

async fn start_relayer_worker(
    mut rx: tokio::sync::mpsc::Receiver<RelayerTask>,
    cancel: tokio_util::sync::CancellationToken,
) {
    tokio::task::spawn_local(async move {
        let relayer_rpc_addr =
            env::var("RELAYER_RPC_ADDR").unwrap_or_else(|_| "localhost:50052".to_string());
        let api_key = match env::var("RELAYER_API_KEY_FILE") {
            Ok(path) => std::fs::read_to_string(&path)
                .map(|s| s.trim().to_string())
                .unwrap_or_else(|e| {
                    warn!("Failed to read RELAYER_API_KEY_FILE '{}': {}", path, e);
                    String::new()
                }),
            Err(_) => env::var("RELAYER_API_KEY").unwrap_or_else(|e| {
                warn!("RELAYER_API_KEY not set: {}. Relayer authentication disabled.", e);
                String::new()
            }),
        };
        if api_key.is_empty() {
            warn!("No relayer API key configured. Relayer requests will be unauthenticated.");
        } else {
            info!("Relayer API key loaded ({} chars)", api_key.len());
        }

        let mut client_opt: Option<relayer_capnp::relayer_service::Client> = None;

        loop {
            tokio::select! {
                _ = cancel.cancelled() => return,
                task_opt = rx.recv() => {
                    let task = match task_opt {
                        Some(t) => t,
                        None => return,
                    };

                    let mut retries = 0;
                    loop {
                        if client_opt.is_none() {
                            if let Ok(stream) = tokio::net::TcpStream::connect(&relayer_rpc_addr).await {
                                let (reader, writer) = stream.into_split();
                                let network = twoparty::VatNetwork::new(
                                    reader.compat(),
                                    writer.compat_write(),
                                    rpc_twoparty_capnp::Side::Client,
                                    Default::default(),
                                );
                                let mut rpc_system = RpcSystem::new(Box::new(network), None);
                                let c: relayer_capnp::relayer_service::Client = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
                                tokio::task::spawn_local(rpc_system);

                                if !api_key.is_empty() {
                                    let mut auth_req = c.authenticate_request();
                                    auth_req.get().set_api_key(api_key.as_bytes());
                                    if let Ok(auth_resp) = auth_req.send().promise.await {
                                        if auth_resp.get().map(|r| r.get_ok()).unwrap_or(false) {
                                            client_opt = Some(c);
                                        } else {
                                            error!("relayer authentication failed: invalid API key for match {}", task.match_id);
                                            break; // Fatal error, drop task
                                        }
                                    } else {
                                        // Network error on auth
                                    }
                                } else {
                                    client_opt = Some(c);
                                }
                            } else {
                                tokio::time::sleep(Duration::from_millis(500 * 2u64.pow(retries.min(3)))).await;
                                retries += 1;
                                if retries > 5 {
                                    error!("Failed to connect to relayer for {}", task.match_id);
                                    break; // drop task
                                }
                                continue;
                            }
                        }

                        if let Some(client) = &client_opt {
                            let mut req = client.submit_outcome_request();

                            let match_id_val = if let Ok(val) = U256::from_dec_str(&task.match_id) {
                                let mut b = [0u8; 32];
                                val.to_big_endian(&mut b);
                                b.to_vec()
                            } else {
                                task.match_id.as_bytes().to_vec()
                            };

                            req.get().set_match_id(&match_id_val);
                            req.get().set_outcome(task.outcome);
                            req.get().set_transcript_hash(&task.transcript_hash);
                            req.get().set_signature(&task.signature);

                            if req.send().promise.await.is_err() {
                                client_opt = None; // Reconnect
                            } else {
                                info!("Notified relayer for match {}", task.match_id);
                                break;
                            }
                        }
                    }
                }
            }
        }
    });
}

async fn start_telemetry_worker(
    mut rx: tokio::sync::mpsc::Receiver<Vec<u8>>,
    cancel: tokio_util::sync::CancellationToken,
) {
    tokio::task::spawn_local(async move {
        let telemetry_addr =
            env::var("TELEMETRY_ADDR").unwrap_or_else(|_| "127.0.0.1:4317".to_string());

        let mut client_opt: Option<amp_telemetry_capnp::telemetry_receiver::Client> = None;

        loop {
            tokio::select! {
                _ = cancel.cancelled() => return,
                evt_opt = rx.recv() => {
                    let event = match evt_opt {
                        Some(e) => e,
                        None => return,
                    };

                    if client_opt.is_none() {
                        if let Ok(stream) = tokio::net::TcpStream::connect(&telemetry_addr).await {
                            let (reader, writer) = stream.into_split();
                            let network = twoparty::VatNetwork::new(
                                reader.compat(),
                                writer.compat_write(),
                                rpc_twoparty_capnp::Side::Client,
                                Default::default(),
                            );
                            let mut rpc_system = RpcSystem::new(Box::new(network), None);
                            let c = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
                            tokio::task::spawn_local(rpc_system);
                            client_opt = Some(c);
                        } else {
                            continue; // Silently drop telemetry if disconnected
                        }
                    }

                    if let Some(client) = &client_opt {
                        let mut req = client.log_event_request();
                        req.get().init_event().set_event_data(&event);
                        if req.send().promise.await.is_err() {
                            client_opt = None;
                        }
                    }
                }
            }
        }
    });
}

struct MatchSessionImpl {
    match_id: String,
    game_id: String,
    players: Vec<String>,
    player_service: Arc<player_service::PlayerServiceImpl>,
    state: AppState,
    signing_config: Arc<SigningConfig>,
    telemetry_tx: tokio::sync::mpsc::Sender<Vec<u8>>,
    relayer_tx: tokio::sync::mpsc::Sender<RelayerTask>,
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
        let outcome_type_raw = outcome.get_type().unwrap_or(match_capnp::OutcomeType::Unknown);
        let scores_raw: Vec<u64> = outcome.get_scores().map(|s| s.iter().collect()).unwrap_or_default();
        if !(1..=3).contains(&outcome_val) {
            return Promise::err(::capnp::Error::failed(format!(
                "invalid outcome value: must be 1-3, got {}",
                outcome_val
            )));
        }
        let replay_hash = pry!(submission.get_replay_hash());
        let r_hash = replay_hash.to_vec();

        let m_id = self.match_id.clone();
        let g_id = self.game_id.clone();
        let p_ids = self.players.clone();
        let p_service = self.player_service.clone();
        let state = self.state.clone();
        let signing_config = self.signing_config.clone();
        let relayer_tx = self.relayer_tx.clone();

        Promise::from_future(async move {
            match sign_match_outcome(signing_config, &m_id, outcome_val, &r_hash).await {
                Ok(sig) => {
                    results.get().set_signature(&sig);

                    // 1. Atomically mark match as settled FIRST (before MMR)
                    let m_clone = {
                        let match_id_for_update = m_id.clone();
                        let mut m_clone_opt = None;

                        let updated = state.update_active_match(&match_id_for_update, |m| {
                            if !m.settled {
                                m.settled = true;
                                m.settled_at_ms = Some(now_ms());
                                m_clone_opt = Some(m.clone());
                            }
                        });

                        if updated.is_none() || m_clone_opt.is_none() {
                            return Err(::capnp::Error::failed(
                                "Match already settled or not found".into(),
                            ));
                        }

                        if let Some(m_clone) = m_clone_opt {
                            state
                                .archive_settled_match(&match_id_for_update, &m_clone)
                                .await;
                            Some(m_clone)
                        } else {
                            None
                        }
                    };

                    // 2. Now update MMR (after settlement is persisted)
                    if m_clone.is_some() && p_ids.len() >= 2 {
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

                        if let Err(e) = p_service
                            .record_match_result(p1, p2, &g_id, score1, 0)
                            .await
                        {
                            warn!("Failed to update MMR for {}: {}", p1, e);
                        }
                        if let Err(e) = p_service
                            .record_match_result(p2, p1, &g_id, score2, 0)
                            .await
                        {
                            warn!("Failed to update MMR for {}: {}", p2, e);
                        }
                    }

                    if let Err(e) = relayer_tx
                        .send(RelayerTask {
                            match_id: m_id.clone(),
                            outcome: outcome_val,
                            transcript_hash: r_hash,
                            signature: sig,
                        })
                        .await
                    {
                        warn!("Failed to queue relayer task for {}: {}", m_id, e);
                    }

                    state.notify_subscribers(&m_id, state::MatchSettledEvent {
                        outcome_type: u16::from(outcome_type_raw),
                        victor: outcome_val,
                        scores: scores_raw,
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
        let state = self.state.clone();

        let (tx, mut rx) = tokio::sync::mpsc::unbounded_channel::<state::MatchSettledEvent>();
        state.add_event_sender(&match_id, tx);

        info!("Client subscribed to events for match {}", match_id);

        tokio::task::spawn_local(async move {
            while let Some(event) = rx.recv().await {
                let mut req = subscriber.on_match_settled_request();
                {
                    let mut outcome = req.get().init_outcome();
                    outcome.set_victor(event.victor);
                    match match_capnp::OutcomeType::try_from(event.outcome_type) {
                        Ok(t) => outcome.set_type(t),
                        Err(_) => outcome.set_type(match_capnp::OutcomeType::Unknown),
                    }
                    {
                        let mut scores = outcome.init_scores(event.scores.len() as u32);
                        for (i, s) in event.scores.iter().enumerate() {
                            scores.set(i as u32, *s);
                        }
                    }
                }
                if let Err(e) = req.send().promise.await {
                    warn!("Failed to deliver onMatchSettled to subscriber for match {}: {}", match_id, e);
                    break;
                }
            }
        });

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
        info!("Game event in match {}: {}", self.match_id, event_type);
        Promise::ok(())
    }

    fn emit_telemetry(
        &mut self,
        params: match_session::EmitTelemetryParams,
        _: match_session::EmitTelemetryResults,
    ) -> Promise<(), ::capnp::Error> {
        let event_data = pry!(pry!(params.get()).get_event());
        let raw = event_data.get_event_data().unwrap_or_default().to_vec();
        let tx = self.telemetry_tx.clone();
        Promise::from_future(async move {
            if let Err(e) = tx.send(raw).await {
                warn!("Failed to queue telemetry event: {}", e);
            }
            Ok(())
        })
    }
}

struct UserSessionImpl {
    player_id: String,
    game_id: u64,
    state: AppState,
    player_service: Arc<player_service::PlayerServiceImpl>,
    match_queue: MatchQueue,
    signing_config: Arc<SigningConfig>,
    telemetry_tx: tokio::sync::mpsc::Sender<Vec<u8>>,
    relayer_tx: tokio::sync::mpsc::Sender<RelayerTask>,
}

impl user_session::Server for UserSessionImpl {
    fn request_match(
        &mut self,
        params: user_session::RequestMatchParams,
        mut results: user_session::RequestMatchResults,
    ) -> Promise<(), ::capnp::Error> {
        let req = pry!(pry!(params.get()).get_req());
        let game_id = self.game_id;
        let p_id = self.player_id.clone();
        let app_state = self.state.clone();
        let p_service = self.player_service.clone();
        let queue = self.match_queue.clone();
        let signing_config = self.signing_config.clone();
        let telemetry_tx = self.telemetry_tx.clone();
        let relayer_tx = self.relayer_tx.clone();

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
                if let Some(p) = app_state.players.get(&p_id) {
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
            if queue
                .try_send(QueueEntry {
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
                })
                .is_err()
            {
                return Err(::capnp::Error::failed(
                    "Server under heavy load, queue full".into(),
                ));
            }

            if let Ok(payload) = rx.await {
                if payload.match_id.is_empty() {
                    return Err(::capnp::Error::failed(
                        "Queue drained (server shutting down)".into(),
                    ));
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
                    signing_config: signing_config.clone(),
                    telemetry_tx: telemetry_tx.clone(),
                    relayer_tx: relayer_tx.clone(),
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
        let match_id = String::from_utf8_lossy(pry!(pry!(params.get()).get_match_id())).to_string();
        let state = self.state.clone();
        let p_service = self.player_service.clone();
        let _game_id = self.game_id;
        let player_id = self.player_id.clone();
        let signing_config = self.signing_config.clone();
        let telemetry_tx = self.telemetry_tx.clone();
        let relayer_tx = self.relayer_tx.clone();

        Promise::from_future(async move {
            let matches = state.active_matches.load();
            match matches.get(&match_id) {
                Some(m) if !m.settled => {
                    if !m.players.contains(&player_id) {
                        return Err(::capnp::Error::failed(format!(
                            "Player {} is not a participant in match {}",
                            player_id, match_id
                        )));
                    }
                    let session = capnp_rpc::new_client(MatchSessionImpl {
                        match_id: match_id.clone(),
                        game_id: m.game_id.clone(),
                        players: m.players.clone(),
                        player_service: p_service,
                        state: state.clone(),
                        signing_config: signing_config.clone(),
                        telemetry_tx: telemetry_tx.clone(),
                        relayer_tx: relayer_tx.clone(),
                    });
                    drop(matches);
                    results.get().set_session(session);
                    info!("Player {} reconnected to match {}", player_id, match_id);
                    Ok(())
                }
                Some(_m) => Err(::capnp::Error::failed(format!(
                    "Match {} is already settled",
                    match_id
                ))),
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
    match_queue: MatchQueue,
    signing_config: Arc<SigningConfig>,
    telemetry_tx: tokio::sync::mpsc::Sender<Vec<u8>>,
    relayer_tx: tokio::sync::mpsc::Sender<RelayerTask>,
}

impl game_session_service::Server for GameSessionServiceImpl {
    fn request_challenge(
        &mut self,
        params: game_session_service::RequestChallengeParams,
        mut results: game_session_service::RequestChallengeResults,
    ) -> Promise<(), ::capnp::Error> {
        let game_id = pry!(params.get()).get_game_id();
        let auth = self.auth_service.clone();

        Promise::from_future(async move {
            let (challenge_bytes, expires_at) = auth.create_challenge(game_id).await;
            results.get().set_challenge(&challenge_bytes);
            results.get().set_expires_at(expires_at);
            Ok(())
        })
    }

    fn login(
        &mut self,
        params: game_session_service::LoginParams,
        mut results: game_session_service::LoginResults,
    ) -> Promise<(), ::capnp::Error> {
        let params_reader = pry!(params.get());
        let game_id = params_reader.get_game_id();
        let sig_bytes = pry!(params_reader.get_signature()).to_vec();
        let challenge_payload = pry!(params_reader.get_challenge_payload()).to_vec();

        let state = self.state.clone();
        let p_service = self.player_service.clone();
        let auth = self.auth_service.clone();
        let queue = self.match_queue.clone();
        let signing_config = self.signing_config.clone();
        let telemetry_tx = self.telemetry_tx.clone();
        let relayer_tx = self.relayer_tx.clone();

        Promise::from_future(async move {
            match auth
                .verify_login(game_id, &sig_bytes, &challenge_payload)
                .await
            {
                Ok(address) => {
                    let player_id = format!("0x{}", hex::encode(address.as_bytes()));

                    {
                        let mut profile = state.players.entry(player_id.clone()).or_default();
                        profile.wallet_address = address.as_bytes().to_vec();
                        profile.is_online = true;
                        profile.last_login = state::now_ns();
                        let profile_clone = profile.clone();
                        drop(profile);
                        state.persist_player(&player_id, &profile_clone).await;
                    }

                    info!("Authenticated player {} for game {}", player_id, game_id);
                    let session = capnp_rpc::new_client(UserSessionImpl {
                        player_id,
                        game_id,
                        state,
                        player_service: p_service,
                        match_queue: queue,
                        signing_config,
                        telemetry_tx: telemetry_tx.clone(),
                        relayer_tx: relayer_tx.clone(),
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

fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let addr: SocketAddr = env::var("AMP_ADDR")
        .unwrap_or_else(|_| "0.0.0.0:50051".to_string())
        .parse()?;
    let db_path = env::var("AMP_DB_PATH").unwrap_or_else(|_| "./amp-data".to_string());

    info!("AMP Matchmaker starting on {}", addr);
    info!("Persistence layer at: {}", db_path);

    let persistence_result = persistence::Persistence::open(&db_path);
    let persist = match persistence_result {
        Ok(p) => {
            info!("Sled persistence layer initialized");
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

    let (match_queue_tx, match_queue_rx) = tokio::sync::mpsc::channel(10000);
    let match_queue: MatchQueue = match_queue_tx;

    let (telemetry_tx, telemetry_rx) = tokio::sync::mpsc::channel(10000);
    let (relayer_tx, relayer_rx) = tokio::sync::mpsc::channel(10000);

    let cancel = tokio_util::sync::CancellationToken::new();

    let signing_config = Arc::new(SigningConfig {
        wallet: load_secret("VERIFIER_KEY", "VERIFIER_KEY_FILE")?.parse()?,
        chain_id: env::var("AMP_CHAIN_ID")
            .unwrap_or_else(|_| "43113".to_string())
            .parse()?,
        settlement_addr: env::var("AMP_SETTLEMENT_ADDRESS")?.parse()?,
    });

    let tls_acceptor = match (
        env::var("AMP_TLS_CERT_FILE").ok(),
        env::var("AMP_TLS_KEY_FILE").ok(),
    ) {
        (Some(cert_path), Some(key_path)) => {
            match amp_tls::create_tls_acceptor(&cert_path, &key_path) {
                Ok(acceptor) => {
                    info!("TLS enabled (cert={}, key={})", cert_path, key_path);
                    Some(acceptor)
                }
                Err(e) => {
                    anyhow::bail!(
                        "TLS configuration failed (cert={}, key={}): {}. Fix the certificates or remove AMP_TLS_CERT_FILE/AMP_TLS_KEY_FILE to use plain TCP.",
                        cert_path,
                        key_path,
                        e
                    );
                }
            }
        }
        _ => {
            info!("TLS not configured (set AMP_TLS_CERT_FILE and AMP_TLS_KEY_FILE to enable)");
            None
        }
    };

    let max_concurrent: usize = env::var("AMP_MAX_CONCURRENT")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(1000);
    let max_per_ip: usize = env::var("AMP_MAX_PER_IP_PER_MIN")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(60);
    let rate_limiter = Arc::new(rate_limit::ConnectionRateLimiter::new(
        max_concurrent,
        max_per_ip,
    ));
    info!(
        "Rate limiting: max {} concurrent, max {} per IP/min",
        max_concurrent, max_per_ip
    );

    let ctx = ServiceContext {
        state,
        player_service: p_service,
        auth_service,
        match_queue,
        signing_config,
        telemetry_tx,
        relayer_tx,
    };

    // Spawn matchmaker on the multi-threaded runtime (Send task)
    let rt = tokio::runtime::Builder::new_multi_thread()
        .enable_all()
        .build()?;

    rt.block_on(async {
        start_matchmaker_loop(ctx.state.clone(), match_queue_rx, cancel.clone()).await;
        start_cleanup_loop(ctx.state.clone(), ctx.auth_service.clone(), cancel.clone());
    });

    // Spawn telemetry worker on its own dedicated thread with its own LocalSet
    let telemetry_cancel = cancel.clone();
    std::thread::Builder::new()
        .name("amp-telemetry-client".to_string())
        .spawn(move || {
            let rt = tokio::runtime::Builder::new_current_thread()
                .enable_all()
                .build()
                .unwrap();
            let local = tokio::task::LocalSet::new();
            rt.block_on(local.run_until(async {
                start_telemetry_worker(telemetry_rx, telemetry_cancel).await;
            }));
        })?;

    // Spawn relayer worker on its own dedicated thread with its own LocalSet
    let relayer_cancel = cancel.clone();
    std::thread::Builder::new()
        .name("amp-relayer-client".to_string())
        .spawn(move || {
            let rt = tokio::runtime::Builder::new_current_thread()
                .enable_all()
                .build()
                .unwrap();
            let local = tokio::task::LocalSet::new();
            rt.block_on(local.run_until(async {
                start_relayer_worker(relayer_rx, relayer_cancel).await;
            }));
        })?;

    // Spawn N worker threads, each with its own LocalSet and SO_REUSEPORT listener
    let num_workers = env::var("AMP_WORKERS")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or_else(num_cpus::get);

    info!("Spawning {} worker threads (SO_REUSEPORT)", num_workers);

    let mut worker_handles = Vec::new();
    for i in 0..num_workers {
        let worker_ctx = ctx.clone();
        let worker_tls = tls_acceptor.clone();
        let worker_limiter = rate_limiter.clone();
        let worker_cancel = cancel.clone();

        let handle = std::thread::Builder::new()
            .name(format!("amp-worker-{}", i))
            .spawn(move || {
                let rt = tokio::runtime::Builder::new_current_thread()
                    .enable_all()
                    .build()
                    .unwrap();
                rt.block_on(async {
                    if let Err(e) = run_worker(
                        i,
                        addr,
                        worker_ctx,
                        worker_tls,
                        worker_limiter,
                        worker_cancel,
                    )
                    .await
                    {
                        error!("Worker {} crashed: {}", i, e);
                    }
                });
            })?;
        worker_handles.push(handle);
    }

    // Wait for SIGINT or SIGTERM
    let rt = tokio::runtime::Builder::new_current_thread()
        .enable_all()
        .build()?;
    rt.block_on(async {
        tokio::select! {
            _ = tokio::signal::ctrl_c() => {}
            _ = wait_sigterm() => {}
        }
        info!("Received shutdown signal, draining...");
        cancel.cancel();

        let matches = ctx.state.active_matches.load();
        let active = matches.iter().filter(|(_, m)| !m.settled).count();
        info!("Active unsettled matches: {}", active);

        tokio::time::sleep(Duration::from_millis(500)).await;
        info!("Shutdown complete.");
    });

    drop(rt);

    Ok(())
}

#[cfg(unix)]
async fn wait_sigterm() {
    use tokio::signal::unix::{SignalKind, signal};
    let mut term = signal(SignalKind::terminate()).expect("failed to install SIGTERM handler");
    term.recv().await;
}

#[cfg(not(unix))]
async fn wait_sigterm() {
    std::future::pending::<()>().await;
}

async fn run_worker(
    worker_id: usize,
    addr: SocketAddr,
    ctx: ServiceContext,
    tls_acceptor: Option<amp_tls::TlsAcceptor>,
    rate_limiter: Arc<rate_limit::ConnectionRateLimiter>,
    cancel: tokio_util::sync::CancellationToken,
) -> Result<()> {
    let socket = socket2::Socket::new(
        socket2::Domain::for_address(addr),
        socket2::Type::STREAM,
        Some(socket2::Protocol::TCP),
    )?;
    socket.set_reuse_address(true)?;
    #[cfg(unix)]
    {
        socket.set_reuse_port(true)?;
    }
    socket.bind(&addr.into())?;
    socket.listen(1024)?;
    socket.set_nonblocking(true)?;

    let std_listener: std::net::TcpListener = socket.into();
    let listener = tokio::net::TcpListener::from_std(std_listener)?;

    info!("Worker {} listening on {}", worker_id, addr);

    let local = tokio::task::LocalSet::new();
    local
        .run_until(async move {
            tokio::select! {
                _ = cancel.cancelled() => {
                    info!("Worker {} shutting down", worker_id);
                }
                _ = accept_loop(listener, ctx, tls_acceptor, rate_limiter) => {}
            }
            Ok::<(), anyhow::Error>(())
        })
        .await?;

    Ok(())
}

#[derive(Clone)]
pub(crate) struct ServiceContext {
    pub state: AppState,
    pub player_service: Arc<player_service::PlayerServiceImpl>,
    pub auth_service: Arc<auth::AuthService>,
    pub match_queue: MatchQueue,
    pub signing_config: Arc<SigningConfig>,
    pub telemetry_tx: tokio::sync::mpsc::Sender<Vec<u8>>,
    pub relayer_tx: tokio::sync::mpsc::Sender<RelayerTask>,
}

async fn accept_loop(
    listener: tokio::net::TcpListener,
    ctx: ServiceContext,
    tls_acceptor: Option<amp_tls::TlsAcceptor>,
    rate_limiter: Arc<rate_limit::ConnectionRateLimiter>,
) {
    loop {
        let (stream, peer) = match listener.accept().await {
            Ok(s) => s,
            Err(e) => {
                error!("Accept error: {}", e);
                continue;
            }
        };

        let ip = peer.ip();
        if !rate_limiter.check_ip(ip) {
            warn!("Rate limit exceeded for {}", ip);
            drop(stream);
            continue;
        }

        stream.set_nodelay(true).unwrap_or(());
        let ctx_clone = ctx.clone();
        let acceptor = tls_acceptor.clone();
        let limiter = rate_limiter.clone();

        tokio::task::spawn_local(async move {
            let _guard = match limiter.try_acquire_slot() {
                Some(g) => g,
                None => {
                    warn!("Max concurrent connections reached, rejecting {}", ip);
                    return;
                }
            };

            if let Some(ref acc) = acceptor {
                match acc.accept(stream).await {
                    Ok(tls_stream) => {
                        let (reader, writer) = tokio::io::split(tls_stream);
                        serve_rpc(reader, writer, ctx_clone.clone()).await;
                    }
                    Err(e) => {
                        error!("TLS handshake failed for {}: {}", ip, e);
                    }
                }
            } else {
                let (reader, writer) = stream.into_split();
                serve_rpc(reader, writer, ctx_clone).await;
            }
        });
    }
}

async fn serve_rpc<R, W>(reader: R, writer: W, ctx: ServiceContext)
where
    R: tokio::io::AsyncRead + Unpin + 'static,
    W: tokio::io::AsyncWrite + Unpin + 'static,
{
    use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

    let network = twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        rpc_twoparty_capnp::Side::Server,
        Default::default(),
    );
    let service: game_session_service::Client = capnp_rpc::new_client(GameSessionServiceImpl {
        state: ctx.state,
        player_service: ctx.player_service,
        auth_service: ctx.auth_service,
        match_queue: ctx.match_queue,
        signing_config: ctx.signing_config,
        telemetry_tx: ctx.telemetry_tx,
        relayer_tx: ctx.relayer_tx,
    });
    let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));
    if let Err(e) = rpc_system.await {
        error!("RPC error: {}", e);
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::state::{InnerState, StoredRuleSet};
    use std::sync::Arc;
    use tokio::sync::oneshot;

    #[tokio::test]
    async fn test_matchmaker_actor_mpsc() {
        let state = InnerState::new(None);
        let app_state = Arc::new(state);

        let mut rs = StoredRuleSet::default();
        rs.rules.push(crate::state::StoredRule::default_skill());

        let mut map = std::collections::HashMap::new();
        map.insert("test_game".into(), Arc::new(rs));
        app_state.rulesets.store(Arc::new(map));

        let (tx, rx) = tokio::sync::mpsc::channel(100);
        let cancel = tokio_util::sync::CancellationToken::new();
        let cancel_clone = cancel.clone();

        let state_clone = app_state.clone();

        tokio::spawn(start_matchmaker_loop(state_clone, rx, cancel_clone));

        let (p1_tx, p1_rx) = oneshot::channel();
        let (p2_tx, p2_rx) = oneshot::channel();

        tx.send(QueueEntry {
            player_id: "p1".into(),
            game_id: "test_game".into(),
            ruleset_id: "test_game".into(),
            mmr: 1500.0,
            mmr_uncertainty: 200.0,
            region: "na".into(),
            preferred_role: "tank".into(),
            language: "en".into(),
            max_ping_ms: 100,
            enqueued_at_ms: crate::state::now_ms(),
            sender: p1_tx,
        })
        .await
        .unwrap();

        tx.send(QueueEntry {
            player_id: "p2".into(),
            game_id: "test_game".into(),
            ruleset_id: "test_game".into(),
            mmr: 1500.0,
            mmr_uncertainty: 200.0,
            region: "na".into(),
            preferred_role: "dps".into(),
            language: "en".into(),
            max_ping_ms: 100,
            enqueued_at_ms: crate::state::now_ms(),
            sender: p2_tx,
        })
        .await
        .unwrap();

        let m1 = p1_rx.await.unwrap();
        let m2 = p2_rx.await.unwrap();

        assert_eq!(m1.match_id, m2.match_id);

        let active = app_state.active_matches.load();
        assert!(active.contains_key(&m1.match_id));
        cancel.cancel();
    }
}
