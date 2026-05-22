use anyhow::Result;
use capnp::capability::Promise;
use capnp_rpc::{RpcSystem, pry, rpc_twoparty_capnp, twoparty};
use ethers_core::types::{Address, H256, U256};
use ethers_signers::LocalWallet;
use std::env;
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

type MatchQueue = Arc<tokio::sync::Mutex<match_queue::IndexedQueue>>;

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
    queue: MatchQueue,
    cancel: tokio_util::sync::CancellationToken,
) {
    tokio::spawn(async move {
        loop {
            tokio::select! {
                _ = cancel.cancelled() => {
                    info!("Matchmaker loop shutting down, draining queue...");
                    let mut q = queue.lock().await;
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

            let rulesets_snapshot = { state.rulesets.read().await.clone() };

            let mut active_count = { state.active_matches.read().await.len() };

            let mut q = queue.lock().await;
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

                    let _ = m.entry_a.sender.send(p1);
                    let _ = m.entry_b.sender.send(p2);

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
                    state
                        .active_matches
                        .write()
                        .await
                        .insert(match_id, active.clone());
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

struct SigningConfig {
    wallet: LocalWallet,
    chain_id: u64,
    settlement_addr: Address,
}

async fn sign_match_outcome(
    config: &SigningConfig,
    match_id: &str,
    outcome: u8,
    transcript_hash: &[u8],
) -> Result<Vec<u8>> {
    let wallet = &config.wallet;
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

    let signature = wallet.sign_hash(digest)?;
    Ok(signature.to_vec())
}

async fn notify_relayer_rpc(
    match_id: &str,
    outcome: u8,
    transcript_hash: &[u8],
    signature: &[u8],
) -> Result<()> {
    let relayer_rpc_addr =
        env::var("RELAYER_RPC_ADDR").unwrap_or_else(|_| "localhost:50052".to_string());
    let api_key = match env::var("RELAYER_API_KEY_FILE") {
        Ok(path) => std::fs::read_to_string(&path)
            .map(|s| s.trim().to_string())
            .unwrap_or_default(),
        Err(_) => env::var("RELAYER_API_KEY").unwrap_or_default(),
    };
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

    if !api_key.is_empty() {
        let mut auth_req = client.authenticate_request();
        auth_req.get().set_api_key(api_key.as_bytes());
        let auth_resp = auth_req.send().promise.await?;
        if !auth_resp.get()?.get_ok() {
            anyhow::bail!("relayer authentication failed: invalid API key");
        }
    }

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
    let telemetry_addr =
        env::var("TELEMETRY_ADDR").unwrap_or_else(|_| "127.0.0.1:4317".to_string());
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
        warn!(
            "Failed to connect to telemetry service at {}",
            telemetry_addr
        );
    }
}

struct MatchSessionImpl {
    match_id: String,
    game_id: String,
    players: Vec<String>,
    player_service: Arc<player_service::PlayerServiceImpl>,
    state: AppState,
    signing_config: Arc<SigningConfig>,
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
        let signing_config = self.signing_config.clone();

        Promise::from_future(async move {
            match sign_match_outcome(&signing_config, &m_id, outcome_val, &r_hash).await {
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

                    {
                        let match_id_for_update = m_id.clone();
                        let mut matches = state.active_matches.write().await;
                        if let Some(m) = matches.get_mut(&match_id_for_update) {
                            if m.settled {
                                drop(matches);
                                return Err(::capnp::Error::failed("Match already settled".into()));
                            }
                            m.settled = true;
                            m.settled_at_ms = Some(now_ms());
                            let m_clone = ActiveMatch {
                                match_id: m.match_id.clone(),
                                game_id: m.game_id.clone(),
                                players: m.players.clone(),
                                created_at_ms: m.created_at_ms,
                                settled: true,
                                settled_at_ms: m.settled_at_ms,
                                expires_at_ms: m.expires_at_ms,
                            };
                            drop(matches);
                            state
                                .archive_settled_match(&match_id_for_update, &m_clone)
                                .await;
                        }
                    }

                    tokio::task::spawn_local(async move {
                        let mut attempts = 0u32;
                        loop {
                            match notify_relayer_rpc(&m_id, outcome_val, &r_hash, &sig).await {
                                Ok(()) => break,
                                Err(e) => {
                                    attempts += 1;
                                    if attempts >= 5 {
                                        error!(
                                            "Failed to notify relayer for {} after {} attempts: {:?}",
                                            m_id, attempts, e
                                        );
                                        break;
                                    }
                                    warn!(
                                        "Relayer notify attempt {} failed for {}: {:?}",
                                        attempts, m_id, e
                                    );
                                    tokio::time::sleep(Duration::from_millis(
                                        500 * 2u64.pow(attempts.min(3)),
                                    ))
                                    .await;
                                }
                            }
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
    match_queue: MatchQueue,
    signing_config: Arc<SigningConfig>,
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
            queue.lock().await.push(QueueEntry {
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

        Promise::from_future(async move {
            let matches = state.active_matches.read().await;
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

    let match_queue: MatchQueue =
        Arc::new(tokio::sync::Mutex::new(match_queue::IndexedQueue::new()));

    let cancel = tokio_util::sync::CancellationToken::new();
    let cancel_clone = cancel.clone();

    let signing_config = Arc::new(SigningConfig {
        wallet: load_secret("VERIFIER_KEY", "VERIFIER_KEY_FILE")?.parse()?,
        chain_id: env::var("AMP_CHAIN_ID")
            .unwrap_or_else(|_| "43113".to_string())
            .parse()?,
        settlement_addr: env::var("AMP_SETTLEMENT_ADDRESS")?.parse()?,
    });

    start_matchmaker_loop(state.clone(), match_queue.clone(), cancel.clone()).await;
    start_cleanup_loop(state.clone(), auth_service.clone(), cancel.clone());

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

    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("Listening on {}", addr);
    info!(
        "Rate limiting: max {} concurrent, max {} per IP/min",
        max_concurrent, max_per_ip
    );

    let local = tokio::task::LocalSet::new();

    local
        .run_until(async move {
            let state_for_shutdown = state.clone();
            tokio::select! {
                _ = tokio::signal::ctrl_c() => {
                    info!("Received shutdown signal, draining...");
                    cancel_clone.cancel();

                    let matches = state_for_shutdown.active_matches.read().await;
                    let active = matches.iter()
                        .filter(|(_, m)| !m.settled)
                        .count();
                    info!("Active unsettled matches: {}", active);
                    drop(matches);

                    tokio::time::sleep(Duration::from_millis(500)).await;
                    info!("Shutdown complete.");
                }
                _ = accept_loop(listener, state, p_service, auth_service, match_queue, tls_acceptor, rate_limiter, signing_config) => {}
            }
        })
        .await;

    Ok(())
}

#[allow(clippy::too_many_arguments)]
async fn accept_loop(
    listener: tokio::net::TcpListener,
    state: AppState,
    p_service: Arc<player_service::PlayerServiceImpl>,
    auth_service: Arc<auth::AuthService>,
    match_queue: MatchQueue,
    tls_acceptor: Option<amp_tls::TlsAcceptor>,
    rate_limiter: Arc<rate_limit::ConnectionRateLimiter>,
    signing_config: Arc<SigningConfig>,
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
        let s_clone = state.clone();
        let ps_clone = p_service.clone();
        let auth_clone = auth_service.clone();
        let q_clone = match_queue.clone();
        let acceptor = tls_acceptor.clone();
        let limiter = rate_limiter.clone();
        let sc_clone = signing_config.clone();

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
                        serve_rpc(
                            reader, writer, s_clone, ps_clone, auth_clone, q_clone, sc_clone,
                        )
                        .await;
                    }
                    Err(e) => {
                        error!("TLS handshake failed for {}: {}", ip, e);
                    }
                }
            } else {
                let (reader, writer) = stream.into_split();
                serve_rpc(
                    reader, writer, s_clone, ps_clone, auth_clone, q_clone, sc_clone,
                )
                .await;
            }
        });
    }
}

async fn serve_rpc<R, W>(
    reader: R,
    writer: W,
    state: AppState,
    p_service: Arc<player_service::PlayerServiceImpl>,
    auth_service: Arc<auth::AuthService>,
    match_queue: MatchQueue,
    signing_config: Arc<SigningConfig>,
) where
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
        state,
        player_service: p_service,
        auth_service,
        match_queue,
        signing_config,
    });
    let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));
    if let Err(e) = rpc_system.await {
        error!("RPC error: {}", e);
    }
}
