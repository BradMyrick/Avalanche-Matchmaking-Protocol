pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
mod verifier_impl;

use crate::verifier_impl::VerifierImpl;
use capnp_rpc::{
    RpcSystem, rpc_twoparty_capnp, rpc_twoparty_capnp::Side, twoparty,
};
use std::net::ToSocketAddrs;
use tokio::net::TcpListener;

#[tokio::main]
async fn main() -> Result<(), Box<dyn std::error::Error>> {
    let addr = "127.0.0.1:4000".to_socket_addrs()?.next().unwrap();
    let listener = TcpListener::bind(&addr).await?;

    println!("AMP Verifier Service starting on {}", addr);

    // TODO: Properly integrate generated capnp code.
    // This is a placeholder for the RPC server loop.
    /*
    loop {
        let (stream, _) = listener.accept().await?;
        stream.set_nodelay(true)?;
        let (reader, writer) = tokio_util::compat::TokioAsyncReadCompatExt::compat(stream).split();

        let network = twoparty::VatNetwork::new(
            reader,
            writer,
            Side::Server,
            Default::default(),
        );

        let service = match_capnp::verifier::Client::new(VerifierImpl).into_client();
        let mut rpc_system = RpcSystem::new(Box::new(network), Some(service.client));

        tokio::spawn(async move {
            rpc_system.await.unwrap();
        });
    }
    */

    println!(
        "Server loop reached (placeholder). Logs: Waiting for connections..."
    );

    // Keep process alive for the demo
    tokio::signal::ctrl_c().await?;
    Ok(())
}
