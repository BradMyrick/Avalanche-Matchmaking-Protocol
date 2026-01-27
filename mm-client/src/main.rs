pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
use anyhow::{Result, bail};
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp::Side, twoparty};
use std::env;
use std::fmt::Debug;
use std::net::ToSocketAddrs;
use tokio::net::TcpStream;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

#[tokio::main]
async fn main() -> Result<()> {
    tokio::task::LocalSet::new().run_until(run()).await
}

async fn run() -> Result<()> {
    let args: Vec<String> = env::args().collect();
    if args.len() < 2 {
        bail!("Usage: {} <address:port>", args[0]);
    }

    let address = &args[1];
    let stream = TcpStream::connect(&address).await?;
    stream.set_nodelay(true)?;
    let (reader, writer) = tokio::io::split(stream);

    let network = twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        Side::Client,
        Default::default(),
    );

    let mut rpc_system: RpcSystem<twoparty::VatId> =
        RpcSystem::new(Box::new(network), None);

    let game_connector_client: match_capnp::game_connector::Client =
        rpc_system.bootstrap(Side::Server);

    tokio::task::spawn_local(rpc_system);
    {
        let mut req = game_connector_client.request_game_service_request();
        {
            let mut assignement = req.get().init_assignment_request();

            let mut player_pool = assignement.reborrow().init_player_pool(5);
            for i in 0..5 {
                let mut player = player_pool.reborrow().get(i);
                let bytes: &[u8] = &[i as u8; 32];
                player.set_player_id(bytes);
                player.set_elo(match_capnp::Elo::Gold);
                player.set_mode(match_capnp::MatchType::Tb);
                player.set_region(match_capnp::Region::As);
                player.set_display_name("Kodr");
                let mut pw = player.init_player_wallet();
                pw.set_fee_token(42);
                pw.set_auth_spend(42);
                pw.set_payer_wallet(42);
            }
        }

        let response = req.send().promise.await?;

        let res_reader = response.get()?;
        let ar = res_reader.get_assignment()?;
        let match_maker: match_capnp::match_maker::Client = match ar.which()? {
            match_capnp::assignment_result::Match(Ok(m)) => m.get_settle()?,
            match_capnp::assignment_result::Err(Ok(e)) => {
                let msg = e.get_msg()?;
                bail!("Matchmaking Failed: {:?}", msg.to_str());
            }
            _ => {
                bail!("Unexpected response from matchmaking");
            }
        };

        Ok(())
    }
}
