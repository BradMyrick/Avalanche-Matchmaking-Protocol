use capnp_rpc::{rpc_twoparty_capnp, twoparty, RpcSystem};
use futures_util::AsyncReadExt;
use serde::Deserialize;
use std::env;
use std::sync::Arc;
use tokio::net::TcpStream;
use tokio::sync::mpsc;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use warp::Filter;

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

use service_capnp::{game_session_service, match_session, user_session};

/// Request object for simulated demo submissions.
#[derive(Deserialize, Clone)]
struct DemoSubmitRequest {
    match_id: String,
    outcome: u8,
    transcript_hash: String,
}
// DemoSubmitRequest

/// Warp handler that queues a demo submission for processing.
async fn handle_demo_submit(
    req: DemoSubmitRequest,
    sender: mpsc::Sender<DemoSubmitRequest>,
) -> Result<impl warp::Reply, warp::Rejection> {
    if let Err(e) = sender.send(req).await {
        eprintln!("Failed to queue capnp flow: {}", e);
    }
    Ok(warp::reply::json(&serde_json::json!({"status": "queued"})))
}
// handle_demo_submit(req, sender)

/// Orchestrates a full Cap'n Proto RPC login and submission flow.
async fn run_capnp_flow(addr: String, req: DemoSubmitRequest) -> anyhow::Result<()> {
    let stream = TcpStream::connect(&addr).await?;
    let (read_half, write_half) = stream.into_split();

    let network = twoparty::VatNetwork::new(
        read_half.compat(),
        write_half.compat_write(),
        rpc_twoparty_capnp::Side::Client,
        Default::default(),
    );

    let mut rpc_system = RpcSystem::new(Box::new(network), None);
    let client: game_session_service::Client =
        rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);

    tokio::task::spawn_local(async move {
        if let Err(e) = rpc_system.await {
            eprintln!("RPC error: {}", e);
        }
    });

    // 1. Login (simulated signature)
    let mut login_req = client.login_request();
    login_req.get().set_game_id(0); // Demo game ID 0
    // Signature needs to match the admin address of game 0.
    // In e2e, we often use the master admin key to sign and register.
    // For the demo gateway simulate a basic signature or valid data.
    login_req.get().set_signed_challenge(b"demo-user-123"); 

    let login_res = login_req.send().promise.await?;
    let user_session_res = login_res.get()?;
    
    if !user_session_res.has_session() {
        return Err(anyhow::anyhow!("Login failed: No session returned"));
    }
    let user_session: user_session::Client = user_session_res.get_session()?;

    // 2. Request Match
    let mut match_req = user_session.request_match_request();
    let _ = match_req.get().init_req();
    let match_res = match_req.send().promise.await?;
    let match_assignment_res = match_res.get()?;
    let match_session: match_session::Client = match_assignment_res.get_session()?;

    // 3. Submit Outcome
    let mut outcome_req = match_session.submit_outcome_request();
    let mut submission = outcome_req.get().init_submission();
    
    let match_id_val = if let Ok(val) = ethers_core::types::U256::from_dec_str(&req.match_id) {
        let mut b = [0u8; 32];
        let val: ethers_core::types::U256 = val;
        val.to_big_endian(&mut b);
        b.to_vec()
    } else {
        req.match_id.as_bytes().to_vec()
    };
    
    submission.set_match_id(&match_id_val);
    let t_bytes = hex::decode(req.transcript_hash.trim_start_matches("0x")).unwrap_or_default();
    submission.set_replay_hash(&t_bytes);
    submission.init_outcome().set_victor(req.outcome);
    
    let outcome_res = outcome_req.send().promise.await?;
    let _sig = outcome_res.get()?.get_signature()?;
    
    println!("Successfully submitted simulated outcome for match {}", req.match_id);

    Ok(())
}
// run_capnp_flow(addr, req)

#[tokio::main]
async fn main() {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let (tx, mut rx) = mpsc::channel::<DemoSubmitRequest>(100);

    // Run the LocalSet on a dedicated thread to handle !Send capnp futures
    std::thread::spawn(move || {
        let rt = tokio::runtime::Builder::new_current_thread()
            .enable_all()
            .build()
            .unwrap();
        let local = tokio::task::LocalSet::new();
        local.block_on(&rt, async move {
            while let Some(req) = rx.recv().await {
                tokio::task::spawn_local(async move {
                    let addr = env::var("AMP_ADDR").unwrap_or_else(|_| "127.0.0.1:50051".to_string());
                    if let Err(e) = run_capnp_flow(addr, req).await {
                        eprintln!("[gateway] capnp error: {}", e);
                    }
                });
            }
        });
    });

    let state_filter = warp::any().map(move || tx.clone());

    let demo_submit = warp::post()
        .and(warp::path("demo-submit"))
        .and(warp::body::json())
        .and(state_filter)
        .and_then(handle_demo_submit);

    let cors = warp::cors().allow_any_origin().allow_headers(vec!["content-type"]);
    let routes = demo_submit.with(cors);

    let http_addr = env::var("AMP_HTTP_ADDR").unwrap_or_else(|_| "0.0.0.0:50053".to_string());
    let http_socket: std::net::SocketAddr = http_addr.parse().expect("Invalid AMP_HTTP_ADDR");
    
    println!("AMP Demo Gateway starting on {}", http_addr);
    
    warp::serve(routes).run(http_socket).await;
}
// main()
