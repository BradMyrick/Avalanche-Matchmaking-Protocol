use capnp_rpc::{rpc_twoparty_capnp, twoparty, RpcSystem};
use futures_util::AsyncReadExt;
use serde::Deserialize;
use std::env;
use std::sync::Arc;
use tokio::net::TcpStream;
use tokio::sync::mpsc;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use warp::Filter;

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

#[derive(Deserialize, Clone)]
struct DemoSubmitRequest {
    match_id: String,
    outcome: u8,
    transcript_hash: String,
}

async fn handle_demo_submit(
    req: DemoSubmitRequest,
    sender: mpsc::Sender<DemoSubmitRequest>,
) -> Result<impl warp::Reply, warp::Rejection> {
    if let Err(e) = sender.send(req).await {
        eprintln!("Failed to queue capnp flow: {}", e);
    }
    Ok(warp::reply::json(&serde_json::json!({"status": "queued"})))
}

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

    // 1. Login
    let mut login_req = client.login_request();
    login_req.get().set_signed_challenge(b"demo-user-123");
    let login_res = login_req.send().promise.await?;
    let user_session: user_session::Client = login_res.get()?.get_session()?;

    // 2. Request Match
    let mut match_req = user_session.request_match_request();
    let mut session_req = match_req.get().init_req();
    session_req.set_game_id(b"demo-game");
    let match_res = match_req.send().promise.await?;
    let match_session: match_session::Client = match_res.get()?.get_session()?;

    // 3. Submit Outcome
    let mut outcome_req = match_session.submit_outcome_request();
    let mut submission = outcome_req.get().init_submission();
    submission.set_match_id(req.match_id.as_bytes());
    let t_bytes = hex::decode(&req.transcript_hash).unwrap_or_default();
    submission.set_replay_hash(&t_bytes);
    submission.init_outcome().set_victor(req.outcome);
    
    let outcome_res = outcome_req.send().promise.await?;
    let _sig = outcome_res.get()?.get_signature()?;
    
    println!("Successfully submitted simulated outcome for {}", req.match_id);

    Ok(())
}

#[tokio::main]
async fn main() {
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
                        eprintln!("Demo gateway capnp error: {}", e);
                    }
                });
            }
        });
    });

    let demo_submit = warp::post()
        .and(warp::path("demo-submit"))
        .and(warp::body::json())
        .and(warp::any().map(move || tx.clone()))
        .and_then(handle_demo_submit);

    let cors = warp::cors().allow_any_origin().allow_headers(vec!["content-type"]);
    let routes = demo_submit.with(cors);

    let http_addr = env::var("AMP_HTTP_ADDR").unwrap_or_else(|_| "0.0.0.0:50052".to_string());
    let http_socket: std::net::SocketAddr = http_addr.parse().expect("Invalid AMP_HTTP_ADDR");
    
    println!("AMP Demo Gateway starting on {}", http_addr);
    
    warp::serve(routes).run(http_socket).await;
}
