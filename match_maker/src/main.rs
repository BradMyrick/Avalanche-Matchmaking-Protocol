pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
mod gc_impl;
mod mm_impl;
use anyhow::Result;
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp::Side, twoparty};
use std::env;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

use tokio::task::LocalSet;

#[tokio::main]
async fn main() -> Result<()> {
    let addr = env::var("AMP_ADDR").unwrap_or_else(|_| "0.0.0.0:50051".to_string());
    let listener = tokio::net::TcpListener::bind(&addr).await?;

    println!("AMP Matchmaker Service starting on {}", addr);

    let local = LocalSet::new();
    local
        .run_until(async move {
            loop {
                let accept_res: Result<
                    (tokio::net::TcpStream, std::net::SocketAddr),
                    std::io::Error,
                > = listener.accept().await;
                match accept_res {
                    Ok((stream, _)) => {
                        let _ = stream.set_nodelay(true);
                        tokio::task::spawn_local(async move {
                            let rpc_res: Result<()> = async {
                                let (reader, writer) = tokio::io::split(stream);
                                let network = twoparty::VatNetwork::new(
                                    reader.compat(),
                                    writer.compat_write(),
                                    Side::Server,
                                    Default::default(),
                                );
                                let client: match_capnp::game_connector::Client =
                                    capnp_rpc::new_client(gc_impl::GameConnectorImpl::new());
                                let rpc_system =
                                    RpcSystem::new(Box::new(network), Some(client.clone().client));
                                rpc_system.await?;
                                Ok(())
                            }
                            .await;
                            if let Err(e) = rpc_res {
                                eprintln!("RPC System error: {:?}", e);
                            }
                        });
                    }
                    Err(e) => {
                        eprintln!("Accept error: {:?}", e);
                    }
                }
            }
        })
        .await;

    Ok(())
}
