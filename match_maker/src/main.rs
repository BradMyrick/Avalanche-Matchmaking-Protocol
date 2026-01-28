pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
mod gc_impl;
mod mm_impl;
use anyhow::{Result, bail};
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp::Side, twoparty};
use std::{fs, path::Path};
use tokio::net::UnixListener;
use tokio::sync::mpsc;
use tokio_util::compat::{
    FuturesAsyncReadCompatExt, TokioAsyncReadCompatExt,
    TokioAsyncWriteCompatExt,
};

#[tokio::main]
async fn main() -> Result<()> {
    let p = Path::new("/run/amp/amp.sock");
    tokio::task::LocalSet::new()
        .run_until(async move {
            let listener = UnixListener::bind(&p)?;

            println!("AMP Verifier Service starting on {p:?}");

            loop {
                let (stream, _) = listener.accept().await?;

                let (reader, writer) = tokio::io::split(stream);

                let network = twoparty::VatNetwork::new(
                    reader.compat(),
                    writer.compat_write(),
                    Side::Server,
                    Default::default(),
                );

                let client: match_capnp::game_connector::Client =
                    capnp_rpc::new_client(gc_impl::GameConnectorImpl::new());

                let rpc_system = RpcSystem::new(
                    Box::new(network),
                    Some(client.clone().client),
                );
                tokio::task::spawn_local(rpc_system);
            }
        })
        .await
}
