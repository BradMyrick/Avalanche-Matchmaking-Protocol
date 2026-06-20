#![allow(warnings)]
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
mod match_capnp {
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
mod game_types_capnp {
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
mod service_capnp {
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
mod game_core_capnp {
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
mod amp_telemetry_capnp {
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
mod player_profile_capnp {
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
mod matchmaking_rules_capnp {
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
mod inventory_capnp {
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
mod tournament_capnp {
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
mod security_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/security_capnp.rs"));
}

#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
mod relayer_capnp {
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
mod game_registry_capnp {
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
mod rust_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/rust_capnp.rs"));
}

use std::sync::Arc;
use std::sync::atomic::{AtomicU64, AtomicUsize, Ordering};
use std::time::{Duration, Instant};

use anyhow::Result;
use clap::Parser;
use alloy_primitives::{eip191_hash_message, keccak256, B256};
use alloy_signer::SignerSync;
use alloy_signer_local::PrivateKeySigner;
use hdrhistogram::Histogram;
use tokio::net::TcpStream;
use tokio::task::LocalSet;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use tracing::{error, info, warn};

#[derive(Parser)]
#[command(
    name = "amp-loadtest",
    about = "Load tester for AMP matchmaking server"
)]
struct Args {
    #[arg(long, default_value = "localhost:50051")]
    addr: String,

    #[arg(long, default_value_t = 100)]
    clients: usize,

    #[arg(long, default_value_t = 30)]
    duration: u64,
}

#[derive(Default)]
struct Stats {
    #[allow(dead_code)]
    connect: AtomicU64,
    login: AtomicU64,
    match_found: AtomicU64,
    outcome: AtomicU64,
    errors: AtomicUsize,
}

#[allow(clippy::too_many_arguments)]
async fn run_client(
    addr: &str,
    client_id: u64,
    deadline: Instant,
    stats: &Stats,
    connect_hist: &std::sync::Mutex<Histogram<u64>>,
    login_hist: &std::sync::Mutex<Histogram<u64>>,
    match_hist: &std::sync::Mutex<Histogram<u64>>,
    total_hist: &std::sync::Mutex<Histogram<u64>>,
) {
    let total_start = Instant::now();

    let key_input = format!("amp-loadtest-client-{}", client_id);
    let key_bytes = keccak256(key_input);
    let wallet = PrivateKeySigner::from_slice(key_bytes.as_slice()).expect("valid loadtest wallet key");

    let connect_start = Instant::now();
    let stream = match TcpStream::connect(addr).await {
        Ok(s) => s,
        Err(e) => {
            error!(client_id, "connect failed: {e}");
            stats.errors.fetch_add(1, Ordering::Relaxed);
            return;
        }
    };
    if let Err(e) = stream.set_nodelay(true) {
        tracing::warn!("Failed to set TCP_NODELAY: {}", e);
    }
    let connect_ns = connect_start.elapsed().as_nanos() as u64;
    if let Ok(mut h) = connect_hist.lock() {
        let _ = h
            .record(connect_ns)
            .map_err(|e| tracing::warn!("Histogram record failed: {}", e));
    }

    let (reader, writer) = tokio::io::split(stream);
    let network = capnp_rpc::twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        capnp_rpc::rpc_twoparty_capnp::Side::Client,
        Default::default(),
    );
    let mut rpc_system = capnp_rpc::RpcSystem::new(Box::new(network), None);
    let service: service_capnp::game_session_service::Client =
        rpc_system.bootstrap(capnp_rpc::rpc_twoparty_capnp::Side::Server);
    tokio::task::spawn_local(async move {
        if let Err(e) = rpc_system.await {
            tracing::warn!("Loadtest RPC system error: {}", e);
        }
    });

    let login_start = Instant::now();
    // Bucket clients into a small game pool so the matchmaker can pair them
    // (request_match uses the SESSION game_id from login, so the shared bucket
    // must be established here, not in the request payload).
    let session = match login(&service, client_id % 8, &wallet).await {
        Ok(s) => s,
        Err(e) => {
            error!(client_id, "login failed: {e}");
            stats.errors.fetch_add(1, Ordering::Relaxed);
            return;
        }
    };
    let login_ns = login_start.elapsed().as_nanos() as u64;
    stats.login.fetch_add(1, Ordering::Relaxed);
    if let Ok(mut h) = login_hist.lock() {
        let _ = h
            .record(login_ns)
            .map_err(|e| tracing::warn!("Histogram record failed: {}", e));
    }

    let match_start = Instant::now();
    let (match_session, match_id) =
        match request_match(&session, client_id, deadline, &wallet).await {
            Ok(r) => r,
            Err(e) => {
                warn!(client_id, "match request failed: {e}");
                stats.errors.fetch_add(1, Ordering::Relaxed);
                return;
            }
        };
    let match_ns = match_start.elapsed().as_nanos() as u64;
    stats.match_found.fetch_add(1, Ordering::Relaxed);
    if let Ok(mut h) = match_hist.lock() {
        let _ = h
            .record(match_ns)
            .map_err(|e| tracing::warn!("Histogram record failed: {}", e));
    }

    if let Err(e) = submit_outcome(&match_session, &match_id, client_id, &wallet).await {
        warn!(client_id, "outcome submission failed: {e}");
        stats.errors.fetch_add(1, Ordering::Relaxed);
    } else {
        stats.outcome.fetch_add(1, Ordering::Relaxed);
    }

    let total_ns = total_start.elapsed().as_nanos() as u64;
    if let Ok(mut h) = total_hist.lock() {
        let _ = h
            .record(total_ns)
            .map_err(|e| tracing::warn!("Histogram record failed: {}", e));
    }
}

async fn login(
    service: &service_capnp::game_session_service::Client,
    game_id: u64,
    wallet: &PrivateKeySigner,
) -> Result<service_capnp::user_session::Client> {
    let mut challenge_req = service.request_challenge_request();
    challenge_req.get().set_game_id(game_id);
    let challenge_resp = challenge_req.send().promise.await?;
    let challenge_bytes = challenge_resp.get()?.get_challenge()?.to_vec();

    let msg_hash: B256 = eip191_hash_message(&challenge_bytes);
    let sig = wallet.sign_hash_sync(&msg_hash)?.as_bytes().to_vec();
    let sig_bytes = sig.to_vec();

    let mut req = service.login_request();
    req.get().set_game_id(game_id);
    req.get().set_signature(&sig_bytes);
    req.get().set_challenge_payload(&challenge_bytes);
    let response = req.send().promise.await?;
    let session = response.get()?.get_session()?;
    Ok(session)
}

async fn request_match(
    session: &service_capnp::user_session::Client,
    client_id: u64,
    deadline: Instant,
    wallet: &PrivateKeySigner,
) -> Result<(service_capnp::match_session::Client, Vec<u8>)> {
    let mut req = session.request_match_request();
    {
        let mut builder = req.get().init_req();
        // Bucket clients into a small pool of games so the matchmaker can
        // actually pair them (a unique game_id per client = 0 matches found).
        let game_id = format!("loadtest-{}", client_id % 8);
        builder.set_game_id(game_id.as_bytes());
        builder.set_rules_type("standard");
        builder.set_match_type(match_capnp::MatchType::TurnBased);
        builder.set_timeout_ms(10_000);
        builder.reborrow().init_stake();
        {
            let mut pi = builder.reborrow().init_player_info();
            let player_id = format!("player-{client_id}");
            pi.set_player_id(player_id.as_bytes());
            pi.set_display_name(format!("Player {client_id}"));
            // Phase 2.6: bind the match to the client's real wallet so the
            // submit_outcome signature recovers to this player (post-S1).
            pi.set_player_wallet(wallet.address().as_slice());
        }
    }

    let remaining = deadline.duration_since(Instant::now());
    let result = tokio::time::timeout(remaining, req.send().promise).await??;
    let response = result.get()?;
    let assignment = response.get_assignment()?;
    let match_id = assignment.get_match_id()?.to_vec();
    let match_session = response.get_session()?;
    Ok((match_session, match_id))
}

async fn submit_outcome(
    match_session: &service_capnp::match_session::Client,
    match_id: &[u8],
    client_id: u64,
    wallet: &PrivateKeySigner,
) -> Result<()> {
    // Phase 2.6: compute a REAL EIP-712 outcome signature so the post-S1
    // verifier accepts it (previously sent [0u8;65], which the server now
    // rejects — silently zeroing the outcome counter and inflating errors).
    // The server maps the submission's `victor` field directly to the on-chain
    // outcome code (1=WIN_A, 2=WIN_B, 3=DRAW, 4=CANCELLED). Send 1/2 and sign
    // the EIP-712 digest with the same value so the verifier accepts it.
    let victor: u8 = if client_id.is_multiple_of(2) { 1 } else { 2 };
    let outcome_code = victor;
    let transcript_hash = [0u8; 32];
    let chain_id = std::env::var("AMP_CHAIN_ID")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or(43113u64);
    let verifying_contract: alloy_primitives::Address = std::env::var("AMP_SETTLEMENT_ADDRESS")
        .ok()
        .and_then(|v| v.parse().ok())
        .unwrap_or_default();
    let verifying_contract: [u8; 20] = verifying_contract.into();
    let match_id_str = String::from_utf8_lossy(match_id);
    let digest = amp_sdk::compute_outcome_eip712_digest(
        &match_id_str,
        outcome_code,
        &transcript_hash,
        chain_id,
        &verifying_contract,
    )?;
    let sig = wallet.sign_hash_sync(&B256::from(digest))?.as_bytes().to_vec();
    let sig_bytes = sig;

    let mut req = match_session.submit_outcome_request();
    {
        let mut sub = req.get().init_submission();
        sub.set_match_id(match_id);
        sub.set_replay_hash(&transcript_hash);
        sub.set_signature(&sig_bytes);
        {
            let mut outcome = sub.reborrow().init_outcome();
            outcome.set_type(match_capnp::OutcomeType::Win);
            outcome.set_victor(victor);
            let mut scores = outcome.init_scores(2);
            scores.set(0, 1);
            scores.set(1, 0);
        }
    }
    req.send().promise.await?;
    Ok(())
}

fn print_histogram(label: &str, hist: &Histogram<u64>) {
    if hist.is_empty() {
        println!("  {label}: no data");
        return;
    }
    let p50 = Duration::from_nanos(hist.value_at_percentile(50.0));
    let p95 = Duration::from_nanos(hist.value_at_percentile(95.0));
    let p99 = Duration::from_nanos(hist.value_at_percentile(99.0));
    let min = Duration::from_nanos(hist.min());
    let max = Duration::from_nanos(hist.max());
    println!("  {label}:");
    println!("    min={min:?}  p50={p50:?}  p95={p95:?}  p99={p99:?}  max={max:?}");
}

fn main() -> Result<()> {
    let args = Args::parse();

    tracing_subscriber::fmt()
        .with_env_filter(
            tracing_subscriber::EnvFilter::try_from_default_env()
                .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info")),
        )
        .init();

    info!(
        addr = %args.addr,
        clients = args.clients,
        duration = args.duration,
        "starting load test"
    );

    let rt = tokio::runtime::Builder::new_multi_thread()
        .enable_all()
        .build()?;

    let stats = Arc::new(Stats::default());
    let connect_hist = Arc::new(std::sync::Mutex::new(Histogram::new(3)?));
    let login_hist = Arc::new(std::sync::Mutex::new(Histogram::new(3)?));
    let match_hist = Arc::new(std::sync::Mutex::new(Histogram::new(3)?));
    let total_hist = Arc::new(std::sync::Mutex::new(Histogram::new(3)?));

    let deadline = Instant::now() + Duration::from_secs(args.duration);
    let clients = args.clients;
    let addr = args.addr.clone();

    let stats_run = stats.clone();
    let connect_hist_run = connect_hist.clone();
    let login_hist_run = login_hist.clone();
    let match_hist_run = match_hist.clone();
    let total_hist_run = total_hist.clone();

    let local = LocalSet::new();
    rt.block_on(local.run_until(async move {
        let mut handles = Vec::with_capacity(clients);
        for i in 0..clients {
            let stats = stats_run.clone();
            let connect_hist = connect_hist_run.clone();
            let login_hist = login_hist_run.clone();
            let match_hist = match_hist_run.clone();
            let total_hist = total_hist_run.clone();
            let addr = addr.clone();
            handles.push(tokio::task::spawn_local(async move {
                run_client(
                    &addr,
                    i as u64,
                    deadline,
                    &stats,
                    &connect_hist,
                    &login_hist,
                    &match_hist,
                    &total_hist,
                )
                .await;
            }));
        }
        for h in handles {
            if let Err(e) = h.await {
                tracing::warn!("Loadtest task failed: {}", e);
            }
        }
    }));

    println!("\n===== AMP Load Test Results =====");
    println!("  clients:        {}", clients);
    println!("  duration:       {}s", args.duration);
    println!("  target:         {}", args.addr);
    println!();
    println!("  logins:         {}", stats.login.load(Ordering::Relaxed));
    println!(
        "  matches found:  {}",
        stats.match_found.load(Ordering::Relaxed)
    );
    println!(
        "  outcomes:       {}",
        stats.outcome.load(Ordering::Relaxed)
    );
    println!("  errors:         {}", stats.errors.load(Ordering::Relaxed));
    println!();

    {
        let ch = connect_hist.lock().unwrap();
        print_histogram("connect", &ch);
    }
    {
        let lh = login_hist.lock().unwrap();
        print_histogram("login", &lh);
    }
    {
        let mh = match_hist.lock().unwrap();
        print_histogram("match", &mh);
    }
    {
        let th = total_hist.lock().unwrap();
        print_histogram("total", &th);
    }

    Ok(())
}
