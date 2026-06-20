use std::cell::Cell;
use std::env;
use std::net::SocketAddr;
use std::sync::Arc;

use anyhow::Result;
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp, twoparty};
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use tracing::{error, info};

mod config;
mod custodial;
mod error;
mod gas;
mod nonce;
mod settlement;

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

// Phase 3.4: ethers `abigen!` → alloy `sol!`. `#[sol(rpc)]` enables the
// provider-bound contract instance + call builders.
alloy_sol_types::sol! {
    #[sol(rpc)]
    interface IAMPRegistry {
        function matches(uint256 matchId) external view returns (uint256 gameId, address playerA, uint8 state, address playerB, uint64 createdAt, uint256 stakeAmount, uint256 stakeAmountB);
        function games(uint256 gameId) external view returns (address admin, uint8 mode, uint256 minStake, address stakeToken, address arbiter, uint256 matchTimeout);
    }
    #[sol(rpc)]
    interface IAMPSettlement {
        struct AsyncResult { uint256 matchId; uint8 outcome; bytes32 transcriptHash; bytes signature; }
        function submitAsyncResult(uint256 matchId, AsyncResult calldata result) external;
        function settlements(uint256 matchId) external view returns (uint256 matchId_, uint8 outcome, bytes32 transcriptHash, uint256 settledAt);
    }
}

use alloy_primitives::{Address, U256};
use alloy_provider::{DynProvider, Provider, ProviderBuilder};
use alloy_signer_local::PrivateKeySigner;
use dashmap::DashSet;

#[derive(Clone)]
pub struct RelayerState {
    /// Read-only provider for chain reads (balances, contract view calls).
    pub read_provider: DynProvider,
    /// Master wallet-fillable provider (gas top-ups from the master key).
    pub master_provider: DynProvider,
    /// Master signing key — used to derive per-game custodial signers.
    pub master_signer: PrivateKeySigner,
    /// RPC URL — used to build a wallet-fillable provider per custodial signer.
    pub rpc_url: String,
    pub chain_id: u64,
    pub settlement_addr: Address,
    pub registry_addr: Address,
    pub pending_topups: Arc<DashSet<Address>>,
}

struct RelayerImpl {
    state: RelayerState,
    queue: Arc<settlement::SettlementQueue>,
    authenticated: Cell<bool>,
    api_keys: Arc<std::collections::HashSet<String>>,
}

impl relayer_capnp::relayer_service::Server for RelayerImpl {
    fn authenticate(
        &mut self,
        params: relayer_capnp::relayer_service::AuthenticateParams,
        mut results: relayer_capnp::relayer_service::AuthenticateResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let params_reader = capnp_rpc::pry!(params.get());
        let api_key_bytes = capnp_rpc::pry!(params_reader.get_api_key());
        let api_key = capnp_rpc::pry!(std::str::from_utf8(api_key_bytes));

        let ok = config::verify_api_key(api_key, &self.api_keys);
        if ok {
            self.authenticated.set(true);
            info!("Relayer client authenticated successfully");
        } else {
            tracing::warn!("Authentication failed: invalid API key");
        }

        results.get().set_ok(ok);
        capnp::capability::Promise::ok(())
    }

    fn get_game_admin(
        &mut self,
        params: relayer_capnp::relayer_service::GetGameAdminParams,
        mut results: relayer_capnp::relayer_service::GetGameAdminResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let game_id = capnp_rpc::pry!(params.get()).get_game_id();
        let state = self.state.clone();

        capnp::capability::Promise::from_future(async move {
            let registry = IAMPRegistry::new(state.registry_addr, state.read_provider.clone());
            match registry.games(U256::from(game_id)).call().await {
                Ok(game_data) => {
                    results.get().set_admin(game_data.admin.as_slice());
                    Ok(())
                }
                Err(e) => Err(capnp::Error::failed(format!(
                    "Registry query failed: {:?}",
                    e
                ))),
            }
        })
    }

    fn get_custodial_address(
        &mut self,
        params: relayer_capnp::relayer_service::GetCustodialAddressParams,
        mut results: relayer_capnp::relayer_service::GetCustodialAddressResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        if !self.authenticated.get() && !self.api_keys.is_empty() {
            return capnp::capability::Promise::err(capnp::Error::failed(
                "unauthorized: call authenticate() first".to_string(),
            ));
        }

        let game_id = capnp_rpc::pry!(params.get()).get_game_id();
        let chain_id = self.state.chain_id;
        let wallet = capnp_rpc::pry!(
            custodial::derive_custodial_signer(
                &self.state.master_signer,
                "settlement",
                game_id,
                chain_id,
            )
            .map_err(|e| capnp::Error::failed(format!("custodial derivation failed: {}", e)))
        );

        results.get().set_address(wallet.address().as_slice());
        capnp::capability::Promise::ok(())
    }

    fn submit_outcome(
        &mut self,
        params: relayer_capnp::relayer_service::SubmitOutcomeParams,
        mut results: relayer_capnp::relayer_service::SubmitOutcomeResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        if !self.authenticated.get() && !self.api_keys.is_empty() {
            return capnp::capability::Promise::err(capnp::Error::failed(
                "unauthorized: call authenticate() first".to_string(),
            ));
        }

        let params_reader = capnp_rpc::pry!(params.get());
        let match_id_bytes = capnp_rpc::pry!(params_reader.get_match_id()).to_vec();
        if match_id_bytes.is_empty() || match_id_bytes.len() > 64 {
            return capnp::capability::Promise::err(capnp::Error::failed(
                "match_id must be 1-64 bytes".to_string(),
            ));
        }
        let outcome = params_reader.get_outcome();
        if !(1..=4).contains(&outcome) {
            return capnp::capability::Promise::err(capnp::Error::failed(format!(
                "outcome must be 1-4, got {}",
                outcome
            )));
        }
        let transcript_hash_bytes = capnp_rpc::pry!(params_reader.get_transcript_hash()).to_vec();
        if transcript_hash_bytes.len() != 32 {
            return capnp::capability::Promise::err(capnp::Error::failed(format!(
                "transcript_hash must be 32 bytes, got {}",
                transcript_hash_bytes.len()
            )));
        }
        let signature_bytes = capnp_rpc::pry!(params_reader.get_signature()).to_vec();
        if signature_bytes.len() != 65 {
            return capnp::capability::Promise::err(capnp::Error::failed(format!(
                "signature must be 65 bytes (secp256k1), got {}",
                signature_bytes.len()
            )));
        }

        let queue = self.queue.clone();

        capnp::capability::Promise::from_future(async move {
            let now = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .unwrap_or_default()
                .as_millis() as u64;

            let pending = settlement::PendingSettlement {
                match_id: match_id_bytes,
                outcome,
                transcript_hash: transcript_hash_bytes,
                signature: signature_bytes,
                retry_count: 0,
                enqueued_at_ms: now,
                last_attempt_at_ms: None,
                status: settlement::SettlementStatus::Queued,
                last_max_fee: None,
                last_priority_fee: None,
                tx_hash: String::new(),
                updated_at_ms: now,
            };

            queue
                .enqueue(pending)
                .map_err(|e| capnp::Error::failed(format!("Queue error: {:?}", e)))?;

            results.get().set_tx_hash(b"queued");
            Ok(())
        })
    }

    fn get_settlement_status(
        &mut self,
        params: relayer_capnp::relayer_service::GetSettlementStatusParams,
        mut results: relayer_capnp::relayer_service::GetSettlementStatusResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        if !self.authenticated.get() && !self.api_keys.is_empty() {
            return capnp::capability::Promise::err(capnp::Error::failed(
                "unauthorized: call authenticate() first".to_string(),
            ));
        }

        let params_reader = capnp_rpc::pry!(params.get());
        let match_id_bytes = capnp_rpc::pry!(params_reader.get_match_id()).to_vec();
        let queue = self.queue.clone();

        capnp::capability::Promise::from_future(async move {
            let mut info = results.get().init_info();
            match queue.get_status(&match_id_bytes) {
                Ok(Some((status, tx_hash, retry_count, updated_at))) => {
                    info.set_status(status.wire_code());
                    info.set_retry_count(retry_count);
                    info.set_updated_at_ms(updated_at);
                    info.set_tx_hash(tx_hash.as_bytes());
                }
                Ok(None) => {
                    // Unknown: leave status at the default 0.
                    info.set_status(0);
                }
                Err(e) => {
                    return Err(capnp::Error::failed(format!(
                        "status lookup failed: {:?}",
                        e
                    )));
                }
            }
            Ok(())
        })
    }
}

async fn run_cli_mode(args: Vec<String>) -> Result<()> {
    let rpc_addr = env::var("RPC_PORT")
        .map(|p| format!("127.0.0.1:{}", p))
        .unwrap_or_else(|_| "127.0.0.1:50052".to_string());

    if args[1] == "query-custodial" || args[1] == "query-admin" {
        let game_id: u64 = args[2].parse()?;
        let stream = tokio::net::TcpStream::connect(&rpc_addr).await?;
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

        let local = tokio::task::LocalSet::new();
        local
            .run_until(async move {
                tokio::task::spawn_local(rpc_system);

                if let Ok(api_key) = env::var("RELAYER_API_KEY") {
                    let mut auth_req = client.authenticate_request();
                    auth_req.get().set_api_key(api_key.as_bytes());
                    let auth_resp = auth_req.send().promise.await?;
                    if !auth_resp.get()?.get_ok() {
                        anyhow::bail!("authentication failed: invalid API key");
                    }
                }

                if args[1] == "query-custodial" {
                    let mut req = client.get_custodial_address_request();
                    req.get().set_game_id(game_id);
                    let res = req.send().promise.await?;
                    println!("0x{}", hex::encode(res.get()?.get_address()?));
                } else {
                    let mut req = client.get_game_admin_request();
                    req.get().set_game_id(game_id);
                    let res = req.send().promise.await?;
                    println!("0x{}", hex::encode(res.get()?.get_admin()?));
                }
                Ok::<(), anyhow::Error>(())
            })
            .await?;
    }
    Ok(())
}

async fn run_settlement_processor(
    state: RelayerState,
    queue: Arc<settlement::SettlementQueue>,
    cfg: config::Config,
    cancel: tokio_util::sync::CancellationToken,
) {
    let nonce_manager = Arc::new(nonce::NonceManager::new());
    // Fallback idle poll in case a Notify signal races with the worker reaching
    // the idle wait (defensive; the Notify path wakes instantly on enqueue).
    let idle_fallback = std::time::Duration::from_secs(1);

    // Phase 2.3: run N concurrent settlement workers. Claims are serialized by
    // `SettlementQueue::claim_lock`, but the slow chain submission (the `await`
    // in `process_next`) runs concurrently, which helps multi-game throughput
    // (different games settle from different custodial addresses).
    let concurrency = cfg.settlement_concurrency.max(1);
    let notify = queue.notify().clone();
    let mut handles = Vec::new();
    for worker_id in 0..concurrency {
        let state = state.clone();
        let queue = queue.clone();
        let nonce_manager = nonce_manager.clone();
        let cancel = cancel.clone();
        let notify = notify.clone();
        handles.push(tokio::spawn(async move {
            settlement_worker(
                worker_id,
                state,
                queue,
                nonce_manager,
                cancel,
                notify,
                idle_fallback,
            )
            .await;
        }));
    }
    for h in handles {
        let _ = h.await;
    }
}

async fn settlement_worker(
    worker_id: usize,
    state: RelayerState,
    queue: Arc<settlement::SettlementQueue>,
    nonce_manager: Arc<nonce::NonceManager>,
    cancel: tokio_util::sync::CancellationToken,
    notify: Arc<tokio::sync::Notify>,
    idle_fallback: std::time::Duration,
) {
    loop {
        // Drain all ready work back-to-back while the queue is non-empty
        // (Phase 2.2: previously slept `poll_interval` — default 1 s — on EVERY
        // iteration, even when work was queued, hard-capping throughput at
        // ~1 settlement/s per relayer instance).
        loop {
            if cancel.is_cancelled() {
                info!("Settlement worker {worker_id} shutting down...");
                return;
            }
            match queue.process_next(&state, &nonce_manager).await {
                Ok(true) => {
                    // Processed one; immediately try the next without sleeping.
                    continue;
                }
                Ok(false) => break, // nothing ready right now; fall through to idle wait
                Err(e) => {
                    error!("Settlement worker {worker_id} error: {:?}", e);
                    // Brief backoff on error to avoid a hot spin.
                    tokio::select! {
                        _ = cancel.cancelled() => return,
                        _ = tokio::time::sleep(std::time::Duration::from_millis(100)) => {}
                    }
                    break;
                }
            }
        }

        // Idle: wake instantly on new enqueue (Notify), or on the fallback
        // poll, or on shutdown.
        tokio::select! {
            _ = cancel.cancelled() => {
                info!("Settlement worker {worker_id} shutting down...");
                return;
            }
            _ = notify.notified() => {}
            _ = tokio::time::sleep(idle_fallback) => {}
        }
    }
}

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    // Install the Prometheus metrics exporter (release Phase 7). Exposes
    // /metrics on RELAYER_METRICS_ADDR (default 127.0.0.1:9101). Disable by
    // setting RELAYER_METRICS_ADDR=disabled.
    let metrics_addr =
        std::env::var("RELAYER_METRICS_ADDR").unwrap_or_else(|_| "127.0.0.1:9101".to_string());
    if metrics_addr != "disabled" {
        let parsed: std::net::SocketAddr = metrics_addr
            .parse()
            .unwrap_or_else(|_| "127.0.0.1:9101".parse().expect("fallback metrics addr"));
        match metrics_exporter_prometheus::PrometheusBuilder::new()
            .with_http_listener(parsed)
            .install()
        {
            Ok(()) => info!("Prometheus /metrics exposed on {}", parsed),
            Err(e) => tracing::warn!("Failed to install Prometheus exporter: {}", e),
        }
    }

    let args: Vec<String> = env::args().collect();
    if args.len() >= 3 {
        return run_cli_mode(args).await;
    }

    let cfg = config::Config::from_env()?;
    let addr: SocketAddr = cfg.rpc_addr.parse()?;

    // Mirror the server's policy: refuse to start without an API key unless
    // the operator explicitly opts out. Without this, the relayer's
    // submit_outcome endpoint is wide open to anyone who can reach the port
    // — see SECURITY_REVIEW.md S11.
    let allow_unauth = std::env::var("AMP_ALLOW_UNAUTHENTICATED_RELAYER")
        .map(|v| v == "1" || v.eq_ignore_ascii_case("true"))
        .unwrap_or(false);
    if cfg.api_keys.is_empty() && !allow_unauth {
        anyhow::bail!(
            "RELAYER_API_KEY (or RELAYER_API_KEY_FILE / RELAYER_API_KEYS) is \
             required for secure operation. Set \
             AMP_ALLOW_UNAUTHENTICATED_RELAYER=1 to bypass (NOT RECOMMENDED — \
             see SECURITY_REVIEW.md S11)."
        );
    } else if cfg.api_keys.is_empty() {
        tracing::warn!(
            "Relayer starting with no API keys. All incoming submit_outcome \
             requests will be accepted. This is insecure — see \
             SECURITY_REVIEW.md S11."
        );
    }

    let rpc_url = cfg.fuji_rpc_url.clone();
    let read_provider = DynProvider::new(ProviderBuilder::new().connect(&rpc_url).await?);
    let chain_id = read_provider.get_chain_id().await?;

    let master_signer: PrivateKeySigner = cfg.relayer_private_key.parse()?;
    let master_provider = DynProvider::new(
        ProviderBuilder::new()
            .wallet(master_signer.clone())
            .connect(&rpc_url)
            .await?,
    );

    let settlement_addr: Address = cfg.contract_settlement.parse()?;
    let registry_addr: Address = cfg.contract_registry.parse()?;

    let state = RelayerState {
        read_provider,
        master_provider,
        master_signer,
        rpc_url,
        chain_id,
        settlement_addr,
        registry_addr,
        pending_topups: Arc::new(DashSet::new()),
    };

    let db = sled::open(&cfg.db_path)?;
    let gas_manager = gas::GasManager::new(cfg.gas_bump_percent);
    let queue = Arc::new(settlement::SettlementQueue::new(
        Arc::new(db),
        cfg.max_retries,
        cfg.base_retry_delay_ms,
        gas_manager,
    ));

    queue.replay_pending()?;

    let cancel = tokio_util::sync::CancellationToken::new();
    let cancel_processor = cancel.clone();

    let state_processor = state.clone();
    let queue_processor = queue.clone();
    let cfg_processor = cfg.clone();

    let api_keys = Arc::new(cfg.api_keys.clone());

    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("AMP Relayer (Cap'n Proto RPC) listening on {}", addr);

    let tls_acceptor = match (&cfg.tls_cert_file, &cfg.tls_key_file) {
        (Some(cert_path), Some(key_path)) => {
            match amp_tls::create_tls_acceptor(cert_path, key_path) {
                Ok(acceptor) => {
                    info!("TLS enabled (cert={}, key={})", cert_path, key_path);
                    Some(acceptor)
                }
                Err(e) => {
                    anyhow::bail!(
                        "TLS configuration failed (cert={}, key={}): {}. Fix the certificates or remove RELAYER_TLS_CERT_FILE/RELAYER_TLS_KEY_FILE to use plain TCP.",
                        cert_path,
                        key_path,
                        e
                    );
                }
            }
        }
        _ => {
            info!(
                "TLS not configured (set RELAYER_TLS_CERT_FILE and RELAYER_TLS_KEY_FILE to enable)"
            );
            None
        }
    };

    tokio::spawn(run_settlement_processor(
        state_processor,
        queue_processor,
        cfg_processor,
        cancel_processor,
    ));

    let local = tokio::task::LocalSet::new();
    local
        .run_until(async move {
            tokio::select! {
                _ = tokio::signal::ctrl_c() => {
                    info!("Received shutdown signal");
                    cancel.cancel();
                    tokio::time::sleep(std::time::Duration::from_millis(500)).await;
                    info!("Shutdown complete.");
                }
                _ = wait_sigterm() => {
                    info!("Received SIGTERM");
                    cancel.cancel();
                    tokio::time::sleep(std::time::Duration::from_millis(500)).await;
                    info!("Shutdown complete.");
                }
                result = accept_loop(listener, state.clone(), queue.clone(), api_keys.clone(), tls_acceptor) => {
                    result?;
                }
            }
            Ok::<(), anyhow::Error>(())
        })
        .await?;

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

async fn accept_loop(
    listener: tokio::net::TcpListener,
    state: RelayerState,
    queue: Arc<settlement::SettlementQueue>,
    api_keys: Arc<std::collections::HashSet<String>>,
    tls_acceptor: Option<amp_tls::TlsAcceptor>,
) -> Result<()> {
    use std::sync::atomic::{AtomicUsize, Ordering};

    let max_connections: usize = std::env::var("RELAYER_MAX_CONNECTIONS")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(50);
    // Per-IP connection cap. The server-to-relayer hop is normally a single
    // known source; a burst from one IP signals misbehavior. Default 10.
    let max_per_ip: usize = std::env::var("RELAYER_MAX_PER_IP")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(10);
    let active = Arc::new(AtomicUsize::new(0));
    let per_ip: Arc<std::sync::Mutex<std::collections::HashMap<std::net::IpAddr, usize>>> =
        Arc::new(std::sync::Mutex::new(std::collections::HashMap::new()));
    info!(
        "Relayer max concurrent connections: {} (max per IP: {})",
        max_connections, max_per_ip
    );

    loop {
        let (stream, peer) = listener.accept().await?;
        stream.set_nodelay(true)?;

        let current = active.load(Ordering::Relaxed);
        if current >= max_connections {
            tracing::warn!("Relayer at capacity ({}), rejecting {}", current, peer);
            drop(stream);
            continue;
        }

        // Per-IP cap to prevent a single compromised server from saturating
        // the relayer's connection pool. Lifted when the connection closes.
        // Phase 2.4: recover from Mutex poisoning (every other lock in the
        // codebase does); a poisoned per-IP lock must not crash the accept loop.
        let ip = peer.ip();
        let over_ip = {
            let mut m = per_ip.lock().unwrap_or_else(|e| e.into_inner());
            let e = m.entry(ip).or_insert(0);
            if *e >= max_per_ip {
                true
            } else {
                *e += 1;
                false
            }
        };
        if over_ip {
            tracing::warn!(
                "Relayer per-IP cap reached for {} ({}/{}), rejecting",
                ip,
                {
                    let m = per_ip.lock().unwrap_or_else(|e| e.into_inner());
                    m.get(&ip).copied().unwrap_or(0)
                },
                max_per_ip
            );
            drop(stream);
            continue;
        }

        active.fetch_add(1, Ordering::Relaxed);

        let state = state.clone();
        let queue = queue.clone();
        let api_keys = api_keys.clone();
        let acceptor = tls_acceptor.clone();
        let counter = active.clone();
        let per_ip_clone = per_ip.clone();

        tokio::task::spawn_local(async move {
            let relayer_impl = RelayerImpl {
                state,
                queue,
                authenticated: Cell::new(false),
                api_keys,
            };
            let client: relayer_capnp::relayer_service::Client =
                capnp_rpc::new_client(relayer_impl);

            if let Some(ref acc) = acceptor {
                match acc.accept(stream).await {
                    Ok(tls_stream) => {
                        let (reader, writer) = tokio::io::split(tls_stream);
                        run_rpc(reader, writer, client).await;
                    }
                    Err(e) => {
                        tracing::error!("TLS handshake failed for {}: {}", peer, e);
                    }
                }
            } else {
                let (reader, writer) = stream.into_split();
                run_rpc(reader, writer, client).await;
            }
            counter.fetch_sub(1, Ordering::Relaxed);
            // Release the per-IP slot (poison-recovering, Phase 2.4).
            let mut guard = per_ip_clone.lock().unwrap_or_else(|e| e.into_inner());
            let should_remove = guard.get_mut(&ip).map(|c| {
                if *c > 0 {
                    *c -= 1;
                }
                *c == 0
            }) == Some(true);
            if should_remove {
                guard.remove(&ip);
            }
            drop(guard);
        });
    }
}

async fn run_rpc<R, W>(reader: R, writer: W, relayer_client: relayer_capnp::relayer_service::Client)
where
    R: tokio::io::AsyncRead + Unpin + 'static,
    W: tokio::io::AsyncWrite + Unpin + 'static,
{
    let network = twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        rpc_twoparty_capnp::Side::Server,
        Default::default(),
    );
    let rpc_system = RpcSystem::new(Box::new(network), Some(relayer_client.client));
    if let Err(e) = rpc_system.await {
        error!("Relayer RPC system terminated with error: {}", e);
    }
}
