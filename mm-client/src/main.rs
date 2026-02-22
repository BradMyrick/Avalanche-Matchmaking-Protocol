pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}

use anyhow::{bail, Result};
use capnp_rpc::{rpc_twoparty_capnp::Side, twoparty, RpcSystem};
use futures_util::AsyncReadExt;
use std::env;
use tokio::net::TcpStream;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};


#[tokio::main]
async fn main() -> Result<()> {
    tokio::task::LocalSet::new().run_until(async {
        let args: Vec<String> = env::args().collect();
        if args.len() < 3 {
            bail!("Usage: mm-client <command> <addr> [args...]\nCommands: request-match, submit-outcome");
        }

        let cmd = &args[1];
        let addr = &args[2];

        let stream = TcpStream::connect(addr).await?;
        let (read_half, write_half) = stream.into_split();

        let network = twoparty::VatNetwork::new(
            read_half.compat(),
            write_half.compat_write(),
            Side::Client,
            Default::default(),
        );

        let mut rpc_system = RpcSystem::new(Box::new(network), None);
        let game_session: service_capnp::game_session_service::Client =
            rpc_system.bootstrap(Side::Server);

        tokio::task::spawn_local(rpc_system);

        match cmd.as_str() {
            "request-match" => {
                if args.len() != 5 {
                    bail!("Usage: mm-client request-match <addr> <game_id> <player_id_signature>");
                }
                let game_id = &args[3];
                let signature_str = &args[4];
                let signature = hex::decode(signature_str.trim_start_matches("0x")).unwrap_or_else(|_| signature_str.as_bytes().to_vec());

                // 1. Login
                let mut login_req = game_session.login_request();
                login_req.get().set_signed_challenge(&signature);
                let login_res = login_req.send().promise.await?;
                let user_session = login_res.get()?.get_session()?;

                // 2. Request match
                let mut match_req = user_session.request_match_request();
                let mut gm_req = match_req.get().init_req();
                gm_req.set_game_id(game_id.as_bytes());

                let match_res = match_req.send().promise.await?;
                let assignment = match_res.get()?.get_assignment()?;
                let match_id = assignment.get_match_id()?;
                let match_id_str = std::str::from_utf8(match_id).unwrap_or("BINARY");

                println!("{}", match_id_str);
            }
            "submit-outcome" => {
                if args.len() != 6 {
                    bail!("Usage: mm-client submit-outcome <addr> <match_id> <outcome> <signature>");
                }
                let match_id_str = &args[3];
                let outcome_val: u8 = args[4].parse().unwrap_or(1);
                let signature_str = &args[5];
                let signature = hex::decode(signature_str.trim_start_matches("0x")).unwrap_or_else(|_| signature_str.as_bytes().to_vec());

                // 1. Login (dummy sig for reconnection context)
                let mut login_req = game_session.login_request();
                login_req.get().set_signed_challenge(&signature);
                let login_res = login_req.send().promise.await?;
                let user_session = login_res.get()?.get_session()?;

                // 2. Reconnect
                let mut recon_req = user_session.reconnect_request();
                recon_req.get().set_match_id(match_id_str.as_bytes());
                let recon_res = recon_req.send().promise.await?;
                let match_session = recon_res.get()?.get_session()?;

                // 3. Submit Outcome
                let mut outcome_req = match_session.submit_outcome_request();
                let mut submission = outcome_req.get().init_submission();
                submission.set_match_id(match_id_str.as_bytes());
                let mut outcome_struct = submission.init_outcome();
                outcome_struct.set_victor(outcome_val);

                let outcome_res = outcome_req.send().promise.await?;
                let verifier_sig = outcome_res.get()?.get_signature()?;
                
                let sig_hex = hex::encode(verifier_sig);
                println!("0x{}", sig_hex);
            }
            _ => {
                bail!("Unknown command: {}", cmd);
            }
        }

        Ok(())
    }).await
}
