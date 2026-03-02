use anyhow::Result;
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp, twoparty};
use futures_util::AsyncReadExt;
use std::net::ToSocketAddrs;
use tokio::net::TcpListener;
use tokio::task::LocalSet;
use tokio_util::compat::TokioAsyncReadCompatExt;

use amp_telemetry::amp_telemetry_capnp::TelemetryEventType;
use amp_telemetry::amp_telemetry_capnp::telemetry_receiver;

struct TelemetryReceiverImpl;

impl telemetry_receiver::Server for TelemetryReceiverImpl {
    fn log_event(
        &mut self,
        params: telemetry_receiver::LogEventParams,
        mut _results: telemetry_receiver::LogEventResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let event_result = params.get().and_then(|p| p.get_event());
        let event = match event_result {
            Ok(e) => e,
            Err(e) => return capnp::capability::Promise::err(e),
        };

        let match_id = match event.get_match_id() {
            Ok(data) => hex::encode(data),
            Err(_) => "unknown".to_string(),
        };
        let ts = event.get_timestamp();
        let ev_type = match event.get_event_type() {
            Ok(t) => t,
            Err(_) => TelemetryEventType::Unknown,
        };

        println!("--- AMP Telemetry Event ---");
        println!("Match ID: {}", match_id);
        println!("Type:     {:?}", ev_type);
        println!("Time:     {}", ts);
        println!("---------------------------");

        capnp::capability::Promise::ok(())
    }
}

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<()> {
    let args: Vec<String> = std::env::args().collect();
    let addr_str = if args.len() == 2 {
        &args[1]
    } else {
        "127.0.0.1:4317" // match standard port pattern or provide default
    };

    let addr = addr_str
        .to_socket_addrs()?
        .next()
        .expect("could not parse address");
    let listener = TcpListener::bind(&addr).await?;
    let receiver: telemetry_receiver::Client = capnp_rpc::new_client(TelemetryReceiverImpl);

    println!("Starting AMP Telemetry Receiver on {}", addr);

    let local = LocalSet::new();
    local
        .run_until(async move {
            loop {
                if let Ok((stream, _)) = listener.accept().await {
                    stream.set_nodelay(true).unwrap();
                    let (reader, writer) = stream.compat().split();
                    let network = twoparty::VatNetwork::new(
                        reader,
                        writer,
                        rpc_twoparty_capnp::Side::Server,
                        Default::default(),
                    );
                    let rpc_system =
                        RpcSystem::new(Box::new(network), Some(receiver.clone().client));
                    tokio::task::spawn_local(rpc_system);
                }
            }
        })
        .await;

    Ok(())
}
