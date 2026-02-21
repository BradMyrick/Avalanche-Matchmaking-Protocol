pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}

use anyhow::{Result, bail};
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp::Side, twoparty};
use std::env;
use tokio::net::TcpStream;
use tokio_util::compat::TokioAsyncReadCompatExt;
use tokio_util::compat::TokioAsyncWriteCompatExt;

#[tokio::main]
async fn main() -> Result<()> {
    tokio::task::LocalSet::new()
        .run_until(async {
            let args: Vec<String> = env::args().collect();
            let addr = if args.len() >= 2 {
                &args[1]
            } else {
                "127.0.0.1:50051"
            };

            let stream = TcpStream::connect(addr).await?;
            stream.set_nodelay(true)?;

            let (reader, writer) = tokio::io::split(stream);
            let network = twoparty::VatNetwork::new(
                reader.compat(),
                writer.compat_write(),
                Side::Client,
                Default::default(),
            );

            let mut rpc_system = RpcSystem::new(Box::new(network), None);
            let game_session: service_capnp::game_session_service::Client =
                rpc_system.bootstrap(Side::Server);

            tokio::task::spawn_local(rpc_system);

            println!("Identifying user...");

            // 1. Login
            let mut login_req = game_session.login_request();
            let _ = login_req.get().init_signed_challenge(65); // dummy signature
            let login_res = login_req.send().promise.await?;
            let user_session = login_res.get()?.get_session()?;

            println!("Logged in. Requesting match...");

            // 2. Request Match
            let mut match_req = user_session.request_match_request();
            let mut gm_req = match_req.get().init_req();
            gm_req.set_game_id(b"my-game-v1");
            // ... set other fields

            let match_res = match_req.send().promise.await?;
            let assignment = match_res.get()?.get_assignment()?;
            let match_id = assignment.get_match_id()?;

            println!(
                "Match found! ID: {:?}",
                std::str::from_utf8(match_id).unwrap_or("BINARY")
            );

            // 3. Submit Dummy Move
            let match_session = match_res.get()?.get_session()?;
            let mut outcome_req = match_session.submit_outcome_request();
            let mut submission = outcome_req.get().init_submission();
            submission.set_match_id(match_id);

            let outcome_res = outcome_req.send().promise.await?;
            let accepted = outcome_res.get()?.get_accepted();

            println!("Outcome submitted? {}", accepted);

            Ok(())
        })
        .await
}
