use alloy_primitives::{Address, B256, U256, keccak256};
use alloy_signer::SignerSync;
use alloy_signer_local::PrivateKeySigner;
use anyhow::{Context, Result};
use capnp::capability::Promise;
use capnp_rpc::{RpcSystem, pry, rpc_twoparty_capnp, twoparty};
use std::env;
use std::net::SocketAddr;
use std::sync::Arc;
use std::time::Duration;
use tokio::sync::oneshot;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use tracing::{debug, error, info, warn};
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
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
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/security_capnp.rs"));
}

use service_capnp::*;

/// Commands sent to the matchmaker loop's queue. `RemovePlayer` lets the
/// disconnect-detection path dequeue a player who dropped while waiting.
enum QueueCmd {
    Enqueue(QueueEntry),
    RemovePlayer(String),
}

type MatchQueue = tokio::sync::mpsc::Sender<QueueCmd>;

/// Grace period workers get to finish in-flight RPCs (e.g. a `submit_outcome`
/// that already has the verifier signature but still needs to enqueue the
/// `RelayerTask`) after the shutdown signal arrives. Override via
/// `AMP_SHUTDOWN_GRACE_MS`.
fn shutdown_grace() -> Duration {
    let ms = std::env::var("AMP_SHUTDOWN_GRACE_MS")
        .ok()
        .and_then(|v| v.parse::<u64>().ok())
        .unwrap_or(5_000);
    Duration::from_millis(ms.max(100))
}

mod rate_limit {
    use std::collections::HashMap;
    use std::net::IpAddr;
    use std::sync::Arc;
    use std::sync::Mutex;
    use std::sync::atomic::{AtomicUsize, Ordering};
    use std::time::Instant;

    /// Hard cap on the number of distinct source IPs tracked. Without this,
    /// a spoofed-source-IP flood grows the outer HashMap without bound.
    /// When exceeded (or periodically), expired windows and empty entries
    /// are swept, bounding live-set memory to ~this many IPs.
    const MAX_TRACKED_IPS: usize = 100_000;
    /// Sweep amortization: run a maintenance sweep every N calls even when below
    /// the cap, so benign one-shot IPs don't linger until the cap is hit.
    const SWEEP_EVERY: usize = 2048;

    pub struct ConnectionRateLimiter {
        inner: Mutex<Inner>,
        max_per_ip_per_minute: usize,
        current_connections: AtomicUsize,
        max_concurrent: usize,
    }

    struct Inner {
        ip_windows: HashMap<IpAddr, Vec<Instant>>,
        calls_since_sweep: usize,
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
                    calls_since_sweep: 0,
                }),
                max_per_ip_per_minute,
                current_connections: AtomicUsize::new(0),
                max_concurrent,
            }
        }

        pub fn check_ip(&self, ip: IpAddr) -> bool {
            let mut inner = self.inner.lock().unwrap_or_else(|e| e.into_inner());
            let now = Instant::now();

            // Bound distinct-IP memory. Sweep expired windows + drop empty
            // entries when the tracked set grows large or on the periodic
            // cadence. The common small-map path skips this.
            inner.calls_since_sweep = inner.calls_since_sweep.saturating_add(1);
            if inner.ip_windows.len() > MAX_TRACKED_IPS || inner.calls_since_sweep >= SWEEP_EVERY {
                sweep_windows(&mut inner.ip_windows, now);
                inner.calls_since_sweep = 0;
            }

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

    /// Drop expired timestamps from every IP window and remove IPs whose window
    /// is now empty. Extracted so the memory-bounding logic is unit-testable.
    fn sweep_windows(ip_windows: &mut HashMap<IpAddr, Vec<Instant>>, now: Instant) {
        ip_windows.retain(|_, w| {
            w.retain(|t| now.duration_since(*t).as_secs() < 60);
            !w.is_empty()
        });
    }

    #[cfg(test)]
    mod rate_limit_tests {
        use super::*;

        #[test]
        fn sweep_removes_expired_and_empty_windows() {
            // "now" is far enough in the future that older entries are >60s old.
            let base = Instant::now();
            let later = base + std::time::Duration::from_secs(120);

            let ip = |a: u8| IpAddr::V4(std::net::Ipv4Addr::new(a, 0, 0, 1));
            let mut map: HashMap<IpAddr, Vec<Instant>> = HashMap::new();
            // Fresh window → retained.
            map.insert(ip(1), vec![later]);
            // Stale-only window → removed entirely (empty after prune).
            map.insert(ip(2), vec![base]);
            // Mixed window → stale pruned, fresh kept.
            map.insert(ip(3), vec![base, later]);

            sweep_windows(&mut map, later);

            assert!(map.contains_key(&ip(1)));
            assert!(
                !map.contains_key(&ip(2)),
                "empty windows must be dropped (S10 bound)"
            );
            assert_eq!(map.get(&ip(3)).map(|w| w.len()), Some(1));
        }

        #[test]
        fn check_ip_caps_per_ip_and_accepts_until_cap() {
            let lim = ConnectionRateLimiter::new(1000, 3);
            let ip = IpAddr::V4(std::net::Ipv4Addr::new(9, 9, 9, 9));
            assert!(lim.check_ip(ip));
            assert!(lim.check_ip(ip));
            assert!(lim.check_ip(ip));
            // 4th within the rolling minute is rejected.
            assert!(!lim.check_ip(ip));
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
    mut rx: tokio::sync::mpsc::Receiver<QueueCmd>,
    cancel: tokio_util::sync::CancellationToken,
) {
    tokio::spawn(async move {
        let mut q = match_queue::IndexedQueue::new();
        let mut rulesets_cache = arc_swap::cache::Cache::new(state.rulesets.clone());
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

            while let Ok(cmd) = rx.try_recv() {
                match cmd {
                    QueueCmd::Enqueue(entry) => {
                        q.push(entry);
                    }
                    QueueCmd::RemovePlayer(pid) => {
                        if q.remove_player(&pid) {
                            info!("Removed disconnected player {} from match queue", pid);
                        }
                    }
                }
            }
            metrics::gauge!("amp_queue_depth").set(q.len() as f64);
            metrics::gauge!("amp_active_matches").set(state.active_match_count() as f64);

            let rulesets_snapshot = rulesets_cache.load();
            let mut active_count = state.active_match_count();

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

                    // Ghost-match guard: if either participant dropped their
                    // `request_match` future before we could notify them,
                    // abandon the pairing rather than persisting a one-sided
                    // active match that would occupy a MAX_ACTIVE_MATCHES slot
                    // and pollute reconnect lookups. Dropping both senders
                    // unblocks the still-live receiver as Err so that player
                    // re-queues.
                    if m.entry_a.sender.is_closed() || m.entry_b.sender.is_closed() {
                        info!(
                            "Skipping ghost pairing [{}] vs [{}]: a participant disconnected \
                             while queued",
                            m.entry_a.player_id, m.entry_b.player_id
                        );
                        continue;
                    }

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
                        error!(
                            "Failed to notify player {} of match {}: {:?}",
                            m.entry_a.ticket.player_id, match_id, e
                        );
                    }
                    if let Err(e) = m.entry_b.sender.send(p2) {
                        error!(
                            "Failed to notify player {} of match {}: {:?}",
                            m.entry_b.ticket.player_id, match_id, e
                        );
                    }

                    let now = now_ms();
                    let active = ActiveMatch {
                        match_id: match_id.clone(),
                        game_id: m.entry_a.ticket.game_id.clone(),
                        players: vec![
                            m.entry_a.ticket.player_id.clone(),
                            m.entry_b.ticket.player_id.clone(),
                        ],
                        created_at_ms: now,
                        settled: false,
                        settled_at_ms: None,
                        expires_at_ms: Some(now + state::MATCH_TTL_MS),
                        settlement_failed: false,
                        settlement_tx_hash: String::new(),
                    };
                    state.insert_active_match(match_id, active.clone());
                    state.persist_match(&active.match_id, &active).await;
                    active_count += 1;
                    metrics::counter!("amp_matches_created_total").increment(1);
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
    wallet: PrivateKeySigner,
    chain_id: u64,
    settlement_addr: Address,
}

/// Compute the canonical EIP-712 digest over `(matchId, outcome, transcriptHash)`.
///
/// Both the server's verifier signature and the client's submitter signature
/// are produced over this same digest, so the on-chain contract and any
/// observer can confirm both parties attested to the same payload.
pub(crate) fn compute_outcome_eip712_digest(
    match_id: &str,
    outcome: u8,
    transcript_hash: &[u8],
    chain_id: u64,
    settlement_addr: Address,
) -> Result<B256> {
    if transcript_hash.len() != 32 {
        anyhow::bail!(
            "transcript_hash must be exactly 32 bytes, got {}",
            transcript_hash.len()
        );
    }

    // matchId: decimal string -> uint256; otherwise keccak256 of the raw id bytes.
    let match_id_val = match U256::from_str_radix(match_id, 10) {
        Ok(v) => v,
        Err(_) => U256::from_be_bytes::<32>(keccak256(match_id.as_bytes()).into()),
    };

    let async_result_typehash =
        keccak256("AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)".as_bytes());
    let t_hash: [u8; 32] = transcript_hash.try_into().expect("checked 32 bytes");

    // struct_hash = keccak256(abi.encode(bytes32 typeHash, uint256 matchId,
    //   uint256 outcome, bytes32 transcriptHash)) — four ABI words (128 bytes).
    // Built manually so the encoding is provably byte-identical to ethers'
    // Token::FixedBytes + Token::Uint and to Solidity's abi.encode (cross-SDK KAT).
    let mut struct_input = [0u8; 128];
    struct_input[0..32].copy_from_slice(async_result_typehash.as_slice());
    struct_input[32..64].copy_from_slice(&match_id_val.to_be_bytes::<32>());
    struct_input[64..96].copy_from_slice(&U256::from(outcome).to_be_bytes::<32>());
    struct_input[96..128].copy_from_slice(&t_hash);
    let struct_hash = keccak256(struct_input);

    let eip712_domain_typehash = keccak256(
        "EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"
            .as_bytes(),
    );
    let name_hash = keccak256("AMPSettlement".as_bytes());
    let version_hash = keccak256("1".as_bytes());

    // domain_separator = keccak256(abi.encode(bytes32, bytes32, bytes32, uint256, address)).
    let mut domain_input = [0u8; 160];
    domain_input[0..32].copy_from_slice(eip712_domain_typehash.as_slice());
    domain_input[32..64].copy_from_slice(name_hash.as_slice());
    domain_input[64..96].copy_from_slice(version_hash.as_slice());
    domain_input[96..128].copy_from_slice(&U256::from(chain_id).to_be_bytes::<32>());
    // Address is right-aligned in its 32-byte word (left-padded with zeros).
    domain_input[140..160].copy_from_slice(settlement_addr.as_slice());
    let domain_separator = keccak256(domain_input);

    // digest = keccak256(0x1901 || domain_separator || struct_hash)
    let mut digest_input = [0u8; 66];
    digest_input[0] = 0x19;
    digest_input[1] = 0x01;
    digest_input[2..34].copy_from_slice(domain_separator.as_slice());
    digest_input[34..66].copy_from_slice(struct_hash.as_slice());
    Ok(keccak256(digest_input))
}

async fn sign_match_outcome(
    config: Arc<SigningConfig>,
    match_id: &str,
    outcome: u8,
    transcript_hash: &[u8],
) -> Result<Vec<u8>> {
    let digest = compute_outcome_eip712_digest(
        match_id,
        outcome,
        transcript_hash,
        config.chain_id,
        config.settlement_addr,
    )?;

    // alloy's `Signer` trait exposes a sync `sign_hash_sync` for local keys
    // (non-async ECDSA); a single signature is sub-millisecond CPU work.
    let signature = config
        .wallet
        .sign_hash_sync(&digest)
        .map_err(|e| anyhow::anyhow!("verifier sign_hash failed: {e}"))?;

    Ok(signature.as_bytes().to_vec())
}

/// Verify a submitter's EIP-712 signature over `(matchId, outcome, transcriptHash)`.
///
/// Returns `Ok(())` only if the recovered signer equals `expected_address`.
/// This is the server-side enforcement of the schema-documented
/// `OutcomeSubmission.signature` field (see `amp-sdk/schemas/match.capnp:114`).
pub(crate) async fn verify_outcome_signature(
    signature_bytes: &[u8],
    match_id: &str,
    outcome: u8,
    transcript_hash: &[u8],
    expected_address: Address,
    chain_id: u64,
    settlement_addr: Address,
) -> Result<()> {
    if signature_bytes.len() != 65 {
        anyhow::bail!(
            "submitter signature must be exactly 65 bytes (secp256k1+r+s+v), got {}",
            signature_bytes.len()
        );
    }

    let digest = compute_outcome_eip712_digest(
        match_id,
        outcome,
        transcript_hash,
        chain_id,
        settlement_addr,
    )?;

    let sig_vec = signature_bytes.to_vec();
    let recovered = tokio::task::spawn_blocking(move || {
        let sig_arr: [u8; 65] = sig_vec[..65]
            .try_into()
            .map_err(|_| anyhow::anyhow!("submitter signature slice not 65 bytes"))?;
        let sig = alloy_primitives::Signature::from_raw_array(&sig_arr)
            .map_err(|e| anyhow::anyhow!("invalid signature: {e}"))?;
        sig.recover_address_from_prehash(&digest)
            .map_err(|e| anyhow::anyhow!("recovery failed: {e}"))
    })
    .await
    .context("submitter signature verification panicked")??;

    if recovered != expected_address {
        anyhow::bail!(
            "submitter signature does not match participant address (expected 0x{}, recovered 0x{})",
            hex::encode(expected_address.as_slice()),
            hex::encode(recovered.as_slice())
        );
    }
    Ok(())
}

/// Parse a player_id string of the form `"0x" + hex(address)` into an
/// Ethereum [`Address`]. Returns `None` if the string is malformed, which
/// signals that the player was authenticated by a non-Ethereem path (e.g.
/// internal test harness) and therefore cannot sign outcomes.
pub(crate) fn parse_player_address(player_id: &str) -> Option<Address> {
    let hex = player_id.strip_prefix("0x")?;
    let bytes = hex::decode(hex).ok()?;
    if bytes.len() != 20 {
        return None;
    }
    Some(Address::from_slice(&bytes))
}

pub struct RelayerTask {
    match_id: String,
    outcome: u8,
    transcript_hash: Vec<u8>,
    signature: Vec<u8>,
}

async fn start_relayer_worker(
    mut rx: tokio::sync::mpsc::Receiver<RelayerTask>,
    state: state::AppState,
    cancel: tokio_util::sync::CancellationToken,
) {
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
            warn!(
                "RELAYER_API_KEY not set: {}. Relayer authentication disabled.",
                e
            );
            String::new()
        }),
    };
    // Production policy: refuse to start without an API key unless the
    // operator explicitly opts out via AMP_ALLOW_UNAUTHENTICATED_RELAYER=1.
    // The default-unauthenticated posture lets any network actor submit
    // fraudulent settlements when the relayer is reachable.
    let allow_unauth = env::var("AMP_ALLOW_UNAUTHENTICATED_RELAYER")
        .map(|v| v == "1" || v.eq_ignore_ascii_case("true"))
        .unwrap_or(false);
    if api_key.is_empty() && !allow_unauth {
        error!(
            "RELAYER_API_KEY or RELAYER_API_KEY_FILE is required for secure \
             operation. Set AMP_ALLOW_UNAUTHENTICATED_RELAYER=1 to bypass \
             (NOT RECOMMENDED)."
        );
        std::process::exit(1);
    } else if api_key.is_empty() {
        warn!(
            "No relayer API key configured. Relayer requests will be unauthenticated. \
             This is insecure."
        );
    } else {
        info!("Relayer API key loaded ({} chars)", api_key.len());
    }

    let mut client_opt: Option<relayer_capnp::relayer_service::Client> = None;

    loop {
        tokio::select! {
            _ = cancel.cancelled() => {
                // Drain phase: server workers may still be
                // finishing in-flight `submit_outcome` calls during their
                // shutdown grace and will enqueue RelayerTasks after we've
                // received the cancel signal. Keep processing the queue,
                // bounded by the grace deadline, so those settlements actually
                // reach the relayer instead of being dropped.
                let deadline = tokio::time::Instant::now() + shutdown_grace();
                info!(
                    "Relayer worker draining pending settlement tasks until {:?}",
                    deadline
                );
                loop {
                    match tokio::time::timeout_at(deadline, rx.recv()).await {
                        Ok(Some(task)) => {
                            process_relayer_task(
                                &mut client_opt,
                                task,
                                &relayer_rpc_addr,
                                &api_key,
                                &state,
                            )
                            .await;
                        }
                        _ => {
                            info!("Relayer worker drain complete");
                            return;
                        }
                    }
                }
            }
            task_opt = rx.recv() => {
                let task = match task_opt {
                    Some(t) => t,
                    None => return,
                };
                process_relayer_task(
                    &mut client_opt,
                    task,
                    &relayer_rpc_addr,
                    &api_key,
                    &state,
                )
                .await;
            }
        }
    }
}

/// Process one settlement task: connect/auth to the relayer (with backoff),
/// submit the outcome, and on success spawn a reconciliation poll. Extracted
/// from `start_relayer_worker` so the shutdown drain path can reuse it
/// .
async fn process_relayer_task(
    client_opt: &mut Option<relayer_capnp::relayer_service::Client>,
    task: RelayerTask,
    relayer_rpc_addr: &str,
    api_key: &str,
    state: &state::AppState,
) {
    let mut retries = 0;
    loop {
        if client_opt.is_none() {
            if let Ok(stream) = tokio::net::TcpStream::connect(relayer_rpc_addr).await {
                let (reader, writer) = stream.into_split();
                let network = twoparty::VatNetwork::new(
                    reader.compat(),
                    writer.compat_write(),
                    rpc_twoparty_capnp::Side::Client,
                    Default::default(),
                );
                let mut rpc_system = RpcSystem::new(Box::new(network), None);
                let c: relayer_capnp::relayer_service::Client =
                    rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
                tokio::task::spawn_local(rpc_system);

                if !api_key.is_empty() {
                    let mut auth_req = c.authenticate_request();
                    auth_req.get().set_api_key(api_key.as_bytes());
                    if let Ok(auth_resp) = auth_req.send().promise.await {
                        if auth_resp.get().map(|r| r.get_ok()).unwrap_or(false) {
                            *client_opt = Some(c);
                            retries = 0; // connection healthy again
                        } else {
                            error!(
                                "relayer authentication failed: invalid API key for match {}",
                                task.match_id
                            );
                            break; // Fatal error, drop task
                        }
                    } else {
                        // Network error on the auth RPC. Previously this fell
                        // through and reconnected in a tight loop (CPU spin)
                        //
                        tokio::time::sleep(Duration::from_millis(500 * 2u64.pow(retries.min(3))))
                            .await;
                        retries += 1;
                        if retries > 5 {
                            error!(
                                "Failed to authenticate to relayer for {}: repeated auth RPC errors",
                                task.match_id
                            );
                            break; // drop task
                        }
                        continue;
                    }
                } else {
                    *client_opt = Some(c);
                    retries = 0;
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

        if let Some(client) = client_opt.as_ref() {
            let mut req = client.submit_outcome_request();

            let match_id_val = if let Ok(val) = U256::from_str_radix(&task.match_id, 10) {
                val.to_be_bytes::<32>().to_vec()
            } else {
                let hash = keccak256(task.match_id.as_bytes());
                hash.to_vec()
            };

            req.get().set_match_id(&match_id_val);
            req.get().set_outcome(task.outcome);
            req.get().set_transcript_hash(&task.transcript_hash);
            req.get().set_signature(&task.signature);

            if req.send().promise.await.is_err() {
                *client_opt = None; // Reconnect
                // Back off before reconnecting so a relayer that is
                // TCP-reachable but erroring doesn't CPU-spin this task
                // forever .
                tokio::time::sleep(Duration::from_millis(500 * 2u64.pow(retries.min(3)))).await;
                retries += 1;
                if retries > 5 {
                    error!(
                        "Failed to submit outcome to relayer for {} after {} retries",
                        task.match_id, retries
                    );
                    break; // drop task
                }
            } else {
                info!("Notified relayer for match {}", task.match_id);

                // Spawn a reconciliation poll: the server has already marked
                // the match settled + returned the verifier signature, but the
                // relayer may still fail to land the tx on-chain (revert,
                // timeout, dead-letter). Previously this was fire-and-forget
                // and the desync was invisible
                let poll_client = client.clone();
                let poll_match_id = task.match_id.clone();
                let poll_match_id_bytes = match_id_val.clone();
                let poll_state = state.clone();
                tokio::task::spawn_local(async move {
                    reconcile_settlement(
                        poll_client,
                        poll_match_id,
                        poll_match_id_bytes,
                        poll_state,
                    )
                    .await;
                });
                break;
            }
        }
    }
}

/// Polls the relayer's `getSettlementStatus` for a match that the server has
/// already marked settled, until it reaches a terminal on-chain outcome.
///
/// On `Confirmed`: records the tx hash (best-effort) and exits.
/// On `Reverted`/`TimedOut`/`DeadLettered`: flips `ActiveMatch::settlement_failed`
/// so the desync is observable instead of silent .
async fn reconcile_settlement(
    client: relayer_capnp::relayer_service::Client,
    match_id: String,
    match_id_bytes: Vec<u8>,
    state: state::AppState,
) {
    // Status codes from relayer.capnp::getSettlementStatus.
    const CONFIRMED: u8 = 3;
    const REVERTED: u8 = 4;
    const TIMED_OUT: u8 = 5;
    const DEAD_LETTERED: u8 = 6;

    // ~3 minutes max reconciliation window at 10s spacing. Generous enough for
    // the relayer's own ~2-min receipt poll plus retry.
    const POLL_INTERVAL: Duration = Duration::from_secs(10);
    const MAX_POLL_ATTEMPTS: u32 = 18;

    let mut attempts = 0u32;
    loop {
        tokio::time::sleep(POLL_INTERVAL).await;
        attempts += 1;

        let mut req = client.get_settlement_status_request();
        req.get().set_match_id(&match_id_bytes);

        // Copy the primitive values out of the borrowed capnp reader so they
        // outlive the response.
        let (status, tx_hash_bytes): (u8, Vec<u8>) = match req.send().promise.await {
            Ok(r) => match r.get() {
                Ok(g) => match g.get_info() {
                    Ok(info) => (
                        info.get_status(),
                        info.get_tx_hash().unwrap_or(&[]).to_vec(),
                    ),
                    Err(_) => (0, Vec::new()),
                },
                Err(_) => (0, Vec::new()),
            },
            Err(e) => {
                warn!(
                    "settlement status poll failed for match {}: {}",
                    match_id, e
                );
                if attempts >= MAX_POLL_ATTEMPTS {
                    return;
                }
                continue;
            }
        };

        // Best-effort: surface the tx hash as soon as the relayer knows it.
        if !tx_hash_bytes.is_empty()
            && let Ok(s) = std::str::from_utf8(&tx_hash_bytes)
        {
            state.record_settlement_tx_hash(&match_id, s).await;
        }

        match status {
            CONFIRMED => {
                info!(
                    "Settlement confirmed on-chain for match {} after {} poll(s)",
                    match_id, attempts
                );
                return;
            }
            REVERTED | TIMED_OUT | DEAD_LETTERED => {
                error!(
                    "Settlement for match {} failed on-chain (relayer status {}); \
                     flagging settlement_failed — server/chain state is desynced",
                    match_id, status
                );
                metrics::counter!("amp_settlement_desync_total").increment(1);
                state.mark_settlement_failed(&match_id).await;
                return;
            }
            _ => {
                // 0 unknown / 1 queued / 2 inFlight — keep polling.
            }
        }

        if attempts >= MAX_POLL_ATTEMPTS {
            warn!(
                "Settlement reconciliation for match {} timed out without a terminal status \
                 (last status {}); giving up",
                match_id, status
            );
            return;
        }
    }
}

async fn start_telemetry_worker(
    mut rx: tokio::sync::mpsc::Receiver<Vec<u8>>,
    cancel: tokio_util::sync::CancellationToken,
) {
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
}

struct MatchSessionImpl {
    match_id: String,
    game_id: String,
    /// The Ethereum address (as `"0x" + hex`) of the player that holds this
    /// capability. Used to verify the submitter signature on `submit_outcome`.
    player_id: String,
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
        let outcome_type_raw = outcome
            .get_type()
            .unwrap_or(match_capnp::OutcomeType::Unknown);
        let scores_raw: Vec<u64> = outcome
            .get_scores()
            .map(|s| s.iter().collect())
            .unwrap_or_default();
        // Aligned with the relayer, which accepts 1..=4 (see
        // amp-relayer/src/main.rs:150). 0 is reserved for "unknown" and is
        // not a valid victor index.
        if !(1..=4).contains(&outcome_val) {
            return Promise::err(::capnp::Error::failed(format!(
                "invalid outcome value: must be 1-4, got {}",
                outcome_val
            )));
        }
        let replay_hash = pry!(submission.get_replay_hash());
        let r_hash = replay_hash.to_vec();
        // Enforce 32-byte transcript hash to match the relayer's invariant
        // (amp-relayer/src/main.rs:156-162) and avoid silent zero-hash
        // signing that would permanently desync server and chain state.
        if r_hash.len() != 32 {
            return Promise::err(::capnp::Error::failed(format!(
                "transcript_hash must be exactly 32 bytes, got {}",
                r_hash.len()
            )));
        }
        let submitter_sig = pry!(submission.get_signature()).to_vec();
        // The schema (`amp-sdk/schemas/match.capnp:114`) requires a 65-byte
        // EIP-712 signature over (matchId, outcome, replayHash) from the
        // submitter. Reject submissions without one.
        if submitter_sig.len() != 65 {
            return Promise::err(::capnp::Error::failed(format!(
                "submitter signature must be exactly 65 bytes (secp256k1+r+s+v), got {}; \
                 sign the EIP-712 digest over (matchId, outcome, replayHash) with your wallet",
                submitter_sig.len()
            )));
        }

        let m_id = self.match_id.clone();
        let g_id = self.game_id.clone();
        let caller_player_id = self.player_id.clone();
        let p_ids = self.players.clone();
        let p_service = self.player_service.clone();
        let state = self.state.clone();
        let signing_config = self.signing_config.clone();
        let relayer_tx = self.relayer_tx.clone();
        let sig_for_verify = submitter_sig.clone();

        Promise::from_future(async move {
            // 0. Verify the submitter signature BEFORE signing ourselves.
            //    This is the integrity gate: only a participant holding the
            //    matching wallet can submit an outcome that the verifier will
            //    countersign.
            let caller_address = parse_player_address(&caller_player_id).ok_or_else(|| {
                ::capnp::Error::failed(format!(
                    "caller player_id '{}' is not a valid Ethereum address; \
                     cannot verify submitter signature",
                    caller_player_id
                ))
            })?;
            if !p_ids.contains(&caller_player_id) {
                return Err(::capnp::Error::failed(format!(
                    "caller {} is not a participant in match {}",
                    caller_player_id, m_id
                )));
            }
            verify_outcome_signature(
                &sig_for_verify,
                &m_id,
                outcome_val,
                &r_hash,
                caller_address,
                signing_config.chain_id,
                signing_config.settlement_addr,
            )
            .await
            .map_err(|e| ::capnp::Error::failed(format!("submitter signature rejected: {}", e)))?;

            // 1. Atomically mark match as settled FIRST (before signing and
            //    before MMR). This is the idempotency gate: if the match was
            //    already settled, we bail out WITHOUT performing the expensive
            //    EIP-712 signing.
            let _m_clone = {
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
                    m_clone
                } else {
                    unreachable!("m_clone_opt checked above")
                }
            };

            // 2. Sign the outcome with the verifier key. Runs only once per
            // match because the settled-check above is the gate.
            let sig = sign_match_outcome(signing_config, &m_id, outcome_val, &r_hash)
                .await
                .map_err(|e| ::capnp::Error::failed(format!("Signer error: {}", e)))?;
            results.get().set_signature(&sig);
            metrics::counter!("amp_matches_settled_total").increment(1);

            // 3. Now update MMR (after settlement is persisted) — symmetric.
            //    Audit P1 fix: previously two sequential `record_match_result`
            //    calls let the second read the first's post-update rating,
            //    producing slow rating inflation across the population. The
            //    symmetric path snapshots both profiles' (mmr, rd, vol) up
            //    front and computes both updates against the pre-match
            //    opponent values.
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

                if let Err(e) = p_service
                    .record_match_result_symmetric(p1, p2, &g_id, score1, 0)
                    .await
                {
                    warn!(
                        "Failed to update MMR symmetrically for {} vs {}: {}",
                        p1, p2, e
                    );
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

            state.notify_subscribers(
                &m_id,
                state::MatchSettledEvent {
                    outcome_type: u16::from(outcome_type_raw),
                    victor: outcome_val,
                    scores: scores_raw,
                },
            );

            Ok(())
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

        // Bound the number of subscribers per match to prevent task-amplification
        // DoS: a malicious participant could otherwise register arbitrarily many
        // MatchListener callbacks, each spawning a long-lived task. 16 is well
        // above the realistic need (2 participants + a handful of observers).
        const MAX_SUBSCRIBERS_PER_MATCH: usize = 16;
        let current = state.subscriber_count(&match_id);
        if current >= MAX_SUBSCRIBERS_PER_MATCH {
            return Promise::err(::capnp::Error::failed(format!(
                "subscriber cap reached for match {} ({}/{}); refusing new subscription",
                match_id, current, MAX_SUBSCRIBERS_PER_MATCH
            )));
        }

        let (tx, mut rx) = tokio::sync::mpsc::unbounded_channel::<state::MatchEvent>();
        state.add_event_sender(&match_id, tx);

        info!(
            "Client subscribed to events for match {} ({}/{})",
            match_id,
            current + 1,
            MAX_SUBSCRIBERS_PER_MATCH
        );

        tokio::task::spawn_local(async move {
            while let Some(event) = rx.recv().await {
                match event {
                    state::MatchEvent::Settled(settled) => {
                        let mut req = subscriber.on_match_settled_request();
                        {
                            let mut outcome = req.get().init_outcome();
                            outcome.set_victor(settled.victor);
                            match match_capnp::OutcomeType::try_from(settled.outcome_type) {
                                Ok(t) => outcome.set_type(t),
                                Err(_) => outcome.set_type(match_capnp::OutcomeType::Unknown),
                            }
                            {
                                let mut scores = outcome.init_scores(settled.scores.len() as u32);
                                for (i, s) in settled.scores.iter().enumerate() {
                                    scores.set(i as u32, *s);
                                }
                            }
                        }
                        if let Err(e) = req.send().promise.await {
                            warn!(
                                "Failed to deliver onMatchSettled to subscriber for match {}: {}",
                                match_id, e
                            );
                            break;
                        }
                    }
                    state::MatchEvent::OpponentDisconnected { player_id } => {
                        debug!(
                            "Notifying subscriber for match {} that opponent {} disconnected",
                            match_id, player_id
                        );
                        let req = subscriber.on_opponent_disconnected_request();
                        if let Err(e) = req.send().promise.await {
                            warn!(
                                "Failed to deliver onOpponentDisconnected to subscriber for \
                                 match {}: {}",
                                match_id, e
                            );
                            break;
                        }
                    }
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
                .try_send(QueueCmd::Enqueue(QueueEntry {
                    ticket: state::PlayerTicket {
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
                    },
                    sender: tx,
                }))
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
                    player_id: p_id.clone(),
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
            // Clone the match out of the DashMap (guard dropped immediately) so no
            // shard lock is held across the session construction .
            let m_opt = state.get_active_match(&match_id);
            match m_opt {
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
                        player_id: player_id.clone(),
                        players: m.players.clone(),
                        player_service: p_service,
                        state: state.clone(),
                        signing_config: signing_config.clone(),
                        telemetry_tx: telemetry_tx.clone(),
                        relayer_tx: relayer_tx.clone(),
                    });
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
    /// Set on successful `login` so the connection's serving loop can run
    /// disconnect cleanup (dequeue, mark offline, notify opponents) once the
    /// TCP connection drops .
    logged_in_player: Arc<std::sync::Mutex<Option<String>>>,
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
            match auth.create_challenge(game_id).await {
                Some((challenge_bytes, expires_at)) => {
                    results.get().set_challenge(&challenge_bytes);
                    results.get().set_expires_at(expires_at);
                    Ok(())
                }
                None => Err(::capnp::Error::failed(
                    "Server challenge capacity reached; try again shortly".into(),
                )),
            }
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
        let logged_in = self.logged_in_player.clone();

        Promise::from_future(async move {
            match auth
                .verify_login(game_id, &sig_bytes, &challenge_payload)
                .await
            {
                Ok(address) => {
                    let player_id = format!("0x{}", hex::encode(address.as_slice()));

                    {
                        let mut profile = state.players.entry(player_id.clone()).or_default();
                        profile.wallet_address = address.as_slice().to_vec();
                        profile.is_online = true;
                        profile.last_login = state::now_ns();
                        let profile_clone = profile.clone();
                        drop(profile);
                        state.persist_player(&player_id, &profile_clone).await;
                    }

                    // Record the authenticated player so this connection's
                    // serving loop can clean up when the socket drops.
                    if let Ok(mut g) = logged_in.lock() {
                        *g = Some(player_id.clone());
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
                    metrics::counter!("amp_auth_failures_total").increment(1);
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

    // Install the Prometheus metrics exporter . Exposes
    // /metrics on AMP_METRICS_ADDR (default 127.0.0.1:9100). Disable by
    // setting AMP_METRICS_ADDR=disabled.
    let metrics_addr =
        std::env::var("AMP_METRICS_ADDR").unwrap_or_else(|_| "127.0.0.1:9100".to_string());
    if metrics_addr != "disabled" {
        let parsed: std::net::SocketAddr = metrics_addr
            .parse()
            .unwrap_or_else(|_| "127.0.0.1:9100".parse().expect("fallback metrics addr"));
        match metrics_exporter_prometheus::PrometheusBuilder::new()
            .with_http_listener(parsed)
            .install()
        {
            Ok(()) => info!("Prometheus /metrics exposed on {}", parsed),
            Err(e) => warn!("Failed to install Prometheus exporter: {}", e),
        }
    }

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
        chain_id: env::var("AMP_EIP712_CHAIN_ID")
            .or_else(|_| env::var("AMP_CHAIN_ID"))
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
    let relayer_state = ctx.state.clone();
    std::thread::Builder::new()
        .name("amp-relayer-client".to_string())
        .spawn(move || {
            let rt = tokio::runtime::Builder::new_current_thread()
                .enable_all()
                .build()
                .unwrap();
            let local = tokio::task::LocalSet::new();
            rt.block_on(local.run_until(async {
                start_relayer_worker(relayer_rx, relayer_state, relayer_cancel).await;
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
    rt.block_on(async move {
        tokio::select! {
            _ = tokio::signal::ctrl_c() => {}
            _ = wait_sigterm() => {}
        }
        info!("Received shutdown signal, draining...");
        cancel.cancel();

        let matches = ctx.state.active_matches_snapshot();
        let active = matches.iter().filter(|(_, m)| !m.settled).count();
        info!("Active unsettled matches: {}", active);

        // Give workers (and the relayer/telemetry client threads) the full
        // grace window to drain in-flight RPCs and flush queued settlement /
        // telemetry tasks before we tear the process down .
        let grace = shutdown_grace();
        info!("Waiting {:?} for workers to drain in-flight RPCs...", grace);
        tokio::time::sleep(grace).await;

        // Best-effort join: observe panicked workers and warn about any that
        // are still alive (don't block shutdown on a hung thread).
        for (i, h) in worker_handles.into_iter().enumerate() {
            if h.is_finished() {
                if let Err(p) = h.join() {
                    error!("Worker {} panicked during shutdown: {:?}", i, p);
                }
            } else {
                warn!(
                    "Worker {} still alive after grace period; forcing process exit",
                    i
                );
            }
        }

        // Flush sled before tearing the process down. sled's default
        // `flush_every_ms = 500ms` means up to 500ms of writes can otherwise
        // be lost on exit.
        if let Err(e) = ctx.state.flush() {
            error!(
                "Failed to flush persistence on shutdown: {} (terminal state may be lost)",
                e
            );
        } else {
            info!("Persistence flushed.");
        }

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
            // Drain window: keep the LocalSet alive so in-flight serve_rpc
            // tasks (spawned by accept_loop) can finish — notably any
            // submit_outcome that already returned a verifier signature but
            // still needs to enqueue the RelayerTask. Dropping the LocalSet
            // immediately would abandon those settlements mid-flight
            // .
            let grace = shutdown_grace();
            info!(
                "Worker {} draining in-flight RPCs for {:?}",
                worker_id, grace
            );
            tokio::time::sleep(grace).await;
            info!("Worker {} drain complete", worker_id);
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

    // Clone the pieces needed for disconnect cleanup before moving the rest
    // into the session capability. On TCP/RPC termination we dequeue the
    // player, mark them offline, and notify any active match's subscribers
    // that an opponent disconnected . Previously the
    // server had no disconnect observation at all, leaving ghost matches and
    // stale `is_online` flags.
    let logged_in_player = Arc::new(std::sync::Mutex::new(None::<String>));
    let cleanup_state = ctx.state.clone();
    let cleanup_queue = ctx.match_queue.clone();

    let service: game_session_service::Client = capnp_rpc::new_client(GameSessionServiceImpl {
        state: ctx.state,
        player_service: ctx.player_service,
        auth_service: ctx.auth_service,
        match_queue: ctx.match_queue,
        signing_config: ctx.signing_config,
        telemetry_tx: ctx.telemetry_tx,
        relayer_tx: ctx.relayer_tx,
        logged_in_player: logged_in_player.clone(),
    });
    let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));
    if let Err(e) = rpc_system.await {
        debug!("RPC connection ended: {}", e);
    }

    // Connection is gone — run disconnect cleanup for whoever logged in on it.
    let player_id = logged_in_player.lock().map(|g| g.clone()).ok().flatten();
    if let Some(pid) = player_id {
        cleanup_on_disconnect(&pid, cleanup_state, cleanup_queue).await;
    }
}

/// Disconnect cleanup :
/// 1. Dequeue the player so they can't be matched into a ghost game.
/// 2. Flip `is_online = false` and persist.
/// 3. For every unsettled active match the player was part of, notify the
///    other participants' subscribers via `onOpponentDisconnected`.
async fn cleanup_on_disconnect(player_id: &str, state: AppState, queue: MatchQueue) {
    info!("Player {} disconnected; running cleanup", player_id);
    metrics::counter!("amp_player_disconnects_total").increment(1);

    // (1) Best-effort dequeue. The matchmaker loop applies this on its next
    //     50ms tick. Ignore send errors (server shutting down).
    let _ = queue.try_send(QueueCmd::RemovePlayer(player_id.to_string()));

    // (2) Mark offline + persist.
    let profile_snapshot = {
        if let Some(mut p) = state.players.get_mut(player_id) {
            p.is_online = false;
            Some(p.clone())
        } else {
            None
        }
    };
    if let Some(p) = profile_snapshot {
        state.persist_player(player_id, &p).await;
    }

    // (3) Notify opponents in any live (unsettled) match.
    for (match_id, m) in state.active_matches_snapshot() {
        if m.settled {
            continue;
        }
        if m.players.iter().any(|p| p == player_id) {
            state.notify_opponent_disconnected(&match_id, player_id);
        }
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

        tx.send(QueueCmd::Enqueue(QueueEntry {
            ticket: state::PlayerTicket {
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
            },
            sender: p1_tx,
        }))
        .await
        .unwrap();

        tx.send(QueueCmd::Enqueue(QueueEntry {
            ticket: state::PlayerTicket {
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
            },
            sender: p2_tx,
        }))
        .await
        .unwrap();

        let m1 = p1_rx.await.unwrap();
        let m2 = p2_rx.await.unwrap();

        assert_eq!(m1.match_id, m2.match_id);

        assert!(app_state.active_matches.contains_key(&m1.match_id));
        cancel.cancel();
    }

    /// Symmetric rating-update regression test: `record_match_result_symmetric`
    /// snapshots both profiles up front, so the loser's penalty must be
    /// computed against the winner's PRE-match rating.
    #[tokio::test]
    async fn test_symmetric_mmr_uses_pre_match_ratings() {
        use crate::player_service::PlayerServiceImpl;
        use crate::state::{StoredGameStats, StoredPlayerProfile};

        let state = InnerState::new(None);
        let app_state = std::sync::Arc::new(state);

        // Two players with materially different ratings so the asymmetry
        // would be visible if it regressed.
        let alice = StoredPlayerProfile {
            global_mmr: 1600.0,
            mmr_uncertainty: 100.0,
            mmr_volatility: 0.06,
            game_stats: [("g".into(), StoredGameStats::default())].into(),
            ..StoredPlayerProfile::default()
        };

        let bob = StoredPlayerProfile {
            global_mmr: 1400.0,
            mmr_uncertainty: 100.0,
            mmr_volatility: 0.06,
            game_stats: [("g".into(), StoredGameStats::default())].into(),
            ..StoredPlayerProfile::default()
        };

        app_state.players.insert("alice".into(), alice.clone());
        app_state.players.insert("bob".into(), bob.clone());

        // Sanity: capture what the correct (symmetric) update would produce,
        // computed independently of player_service.
        let (alice_correct_post, _, _) = crate::matchmaker::glicko2_update(
            alice.global_mmr,
            alice.mmr_uncertainty,
            alice.mmr_volatility,
            bob.global_mmr,
            bob.mmr_uncertainty,
            1.0, // alice wins
        );
        let (bob_correct_post, _, _) = crate::matchmaker::glicko2_update(
            bob.global_mmr,
            bob.mmr_uncertainty,
            bob.mmr_volatility,
            alice.global_mmr,
            alice.mmr_uncertainty,
            0.0, // bob loses
        );

        let service = PlayerServiceImpl {
            state: app_state.clone(),
        };
        service
            .record_match_result_symmetric("alice", "bob", "g", 1.0, 0)
            .await
            .expect("symmetric update must succeed");

        let alice_after = app_state
            .players
            .get("alice")
            .expect("alice present")
            .clone();
        let bob_after = app_state.players.get("bob").expect("bob present").clone();

        // The asymmetric (buggy) path would have computed bob's update
        // against alice's POST-update (~1624) rather than pre-update (1600),
        // producing a numerically different (smaller-magnitude) loss for bob.
        assert!(
            (alice_after.global_mmr - alice_correct_post).abs() < 0.001,
            "alice rating must match symmetric pre-match-opponent computation: got {} vs expected {}",
            alice_after.global_mmr,
            alice_correct_post,
        );
        assert!(
            (bob_after.global_mmr - bob_correct_post).abs() < 0.001,
            "bob rating must match symmetric pre-match-opponent computation: got {} vs expected {} \
             (if this fails, the second player is being updated against the first's post-update rating)",
            bob_after.global_mmr,
            bob_correct_post,
        );
    }

    /// The digest must be byte-identical for identical inputs — both server
    /// and client rely on this to produce/verify the same signature.
    #[test]
    fn test_outcome_digest_is_deterministic() {
        let match_id = "42";
        let outcome: u8 = 1;
        let transcript = [0xabu8; 32];
        let chain_id = 43113;
        let settlement = "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef"
            .parse::<Address>()
            .unwrap();

        let d1 =
            compute_outcome_eip712_digest(match_id, outcome, &transcript, chain_id, settlement)
                .unwrap();
        let d2 =
            compute_outcome_eip712_digest(match_id, outcome, &transcript, chain_id, settlement)
                .unwrap();
        assert_eq!(d1, d2);
    }

    /// Known-answer test vector computed by the C# and Python SDKs.
    /// matchId="1", outcome=1, transcript_hash=zero[32], chain_id=43113,
    /// verifying_contract=zero[20]. Expected digest:
    ///   2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c
    #[test]
    fn test_outcome_digest_known_vector_cross_lang() {
        let settlement = "0x0000000000000000000000000000000000000000"
            .parse::<Address>()
            .unwrap();
        let d = compute_outcome_eip712_digest("1", 1, &[0u8; 32], 43113, settlement).unwrap();
        assert_eq!(
            format!("{:x}", d),
            "2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c",
            "Rust digest must match C# and Python SDK output for cross-lang interop"
        );
    }

    /// Wrong transcript hash must yield a different digest.
    #[test]
    fn test_outcome_digest_differs_on_transcript() {
        let settlement = "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef"
            .parse::<Address>()
            .unwrap();
        let d1 = compute_outcome_eip712_digest("1", 1, &[0u8; 32], 43113, settlement).unwrap();
        let d2 = compute_outcome_eip712_digest("1", 1, &[1u8; 32], 43113, settlement).unwrap();
        assert_ne!(d1, d2);
    }

    /// Non-32-byte transcript must be rejected up front.
    #[test]
    fn test_outcome_digest_rejects_short_transcript() {
        let settlement = "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef"
            .parse::<Address>()
            .unwrap();
        let err = compute_outcome_eip712_digest("1", 1, &[0u8; 16], 43113, settlement).unwrap_err();
        assert!(err.to_string().contains("32 bytes"));
    }

    /// End-to-end: a wallet signs the digest, the server recovers the right address.
    #[tokio::test]
    async fn test_verify_outcome_signature_round_trip() {
        let wallet = PrivateKeySigner::random();
        let expected_address = wallet.address();
        let settlement = "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef"
            .parse::<Address>()
            .unwrap();
        let transcript = [0xcdu8; 32];

        let digest =
            compute_outcome_eip712_digest("42", 1, &transcript, 43113, settlement).unwrap();
        let sig = wallet.sign_hash_sync(&digest).unwrap().as_bytes().to_vec();

        verify_outcome_signature(
            &sig,
            "42",
            1,
            &transcript,
            expected_address,
            43113,
            settlement,
        )
        .await
        .expect("self-signed signature must verify");

        // Wrong expected address must fail.
        let other = PrivateKeySigner::random().address();
        let err = verify_outcome_signature(&sig, "42", 1, &transcript, other, 43113, settlement)
            .await
            .unwrap_err();
        assert!(err.to_string().contains("does not match"));
    }

    /// Malformed signatures (wrong length) must be rejected without recovery.
    #[tokio::test]
    async fn test_verify_outcome_signature_rejects_wrong_length() {
        let settlement = "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef"
            .parse::<Address>()
            .unwrap();
        let err = verify_outcome_signature(
            &[0u8; 64],
            "1",
            1,
            &[0u8; 32],
            Address::ZERO,
            43113,
            settlement,
        )
        .await
        .unwrap_err();
        assert!(err.to_string().contains("65 bytes"));
    }

    /// `parse_player_address` accepts canonical `"0x" + 40-hex` and rejects the rest.
    #[test]
    fn test_parse_player_address_canonical() {
        let addr = "0xdeadbeefdeadbeefdeadbeefdeadbeefdeadbeef";
        assert!(parse_player_address(addr).is_some());

        // Missing 0x prefix
        assert!(parse_player_address("deadbeefdeadbeefdeadbeefdeadbeefdeadbeef").is_none());
        // Wrong length
        assert!(parse_player_address("0xdead").is_none());
        // Non-hex
        assert!(parse_player_address("0xZZzzbeefdeadbeefdeadbeefdeadbeefdeadbeef").is_none());
        // Internal test-player id used by the harness
        assert!(parse_player_address("p1").is_none());
    }

    /// Disconnect cleanup : a disconnecting player must be marked
    /// offline and their opponents' subscribers must receive an
    /// `onOpponentDisconnected` notification, without touching settled matches.
    #[tokio::test]
    async fn test_cleanup_on_disconnect_marks_offline_and_notifies() {
        let app_state: AppState = Arc::new(InnerState::new(None));

        // Online player who will disconnect.
        {
            let mut p = app_state.players.entry("0xp1".into()).or_default();
            p.is_online = true;
        }

        // An unsettled live match containing p1 + p2, plus a settled match
        // that must NOT trigger a notification.
        let now = crate::state::now_ms();
        app_state.insert_active_match(
            "m_live".into(),
            crate::state::ActiveMatch {
                match_id: "m_live".into(),
                game_id: "g".into(),
                players: vec!["0xp1".into(), "0xp2".into()],
                created_at_ms: now,
                settled: false,
                settled_at_ms: None,
                expires_at_ms: None,
                settlement_failed: false,
                settlement_tx_hash: String::new(),
            },
        );
        app_state.insert_active_match(
            "m_done".into(),
            crate::state::ActiveMatch {
                match_id: "m_done".into(),
                game_id: "g".into(),
                players: vec!["0xp1".into(), "0xp3".into()],
                created_at_ms: now,
                settled: true,
                settled_at_ms: Some(now),
                expires_at_ms: None,
                settlement_failed: false,
                settlement_tx_hash: String::new(),
            },
        );

        // Subscribe a listener to the live match.
        let (tx, mut rx) = tokio::sync::mpsc::unbounded_channel::<crate::state::MatchEvent>();
        app_state.add_event_sender("m_live", tx);

        // Dropped receiver simulates a relayer-less queue; cleanup ignores the
        // send error.
        let (qtx, _qrx) = tokio::sync::mpsc::channel::<QueueCmd>(8);
        cleanup_on_disconnect("0xp1", app_state.clone(), qtx).await;

        // is_online flipped + persisted path executed.
        assert!(
            !app_state
                .players
                .get("0xp1")
                .map(|p| p.is_online)
                .unwrap_or(true),
            "disconnect must mark the player offline"
        );

        // The live match's subscriber received exactly one OpponentDisconnected.
        let evt = rx.recv().await.expect("subscriber should be notified");
        match evt {
            crate::state::MatchEvent::OpponentDisconnected { player_id } => {
                assert_eq!(player_id, "0xp1");
            }
            other => panic!("expected OpponentDisconnected, got {:?}", other),
        }
        // No further events (settled match did not notify).
        assert!(rx.try_recv().is_err());
    }
}
