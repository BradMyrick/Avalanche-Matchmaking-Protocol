use anyhow::Result;
use capnp::message::ReaderOptions;
use capnp::serialize_packed;
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp, twoparty};
use futures_util::AsyncReadExt;
use std::io::{BufWriter, Read, Write};
use std::net::ToSocketAddrs;
use std::sync::{Arc, Mutex};
use tokio::net::TcpListener;
use tokio::task::LocalSet;
use tokio_util::compat::TokioAsyncReadCompatExt;

use amp_telemetry::amp_telemetry_capnp::TelemetryEventType;
use amp_telemetry::amp_telemetry_capnp::telemetry_receiver;

/// Implementation of the TelemetryReceiver service.
/// Persists incoming telemetry events to a packed binary log file.
struct TelemetryReceiverImpl {
    log_writer: Arc<Mutex<BufWriter<std::fs::File>>>,
}
// TelemetryReceiverImpl

impl telemetry_receiver::Server for TelemetryReceiverImpl {
    /// Appends a telemetry event to the persistent binary log.
    fn log_event(
        &mut self,
        params: telemetry_receiver::LogEventParams,
        mut _results: telemetry_receiver::LogEventResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let params_reader = match params.get() {
            Ok(p) => p,
            Err(e) => return capnp::capability::Promise::err(e),
        };

        let event_reader = match params_reader.get_event() {
            Ok(e) => e,
            Err(e) => return capnp::capability::Promise::err(e),
        };

        let mut out_msg = capnp::message::Builder::new_default();
        out_msg.set_root(event_reader).unwrap();

        let mut packed_buf: Vec<u8> = Vec::new();
        serialize_packed::write_message(&mut packed_buf, &out_msg).unwrap();

        let len = packed_buf.len() as u32;
        let mut writer = self.log_writer.lock().unwrap();
        writer.write_all(&len.to_le_bytes()).unwrap();
        writer.write_all(&packed_buf).unwrap();
        writer.flush().unwrap();

        let match_id = match event_reader.get_match_id() {
            Ok(data) => hex::encode(data),
            Err(_) => "unknown".to_string(),
        };
        let game_id = event_reader.get_game_id();
        let ev_type = event_reader
            .get_event_type()
            .unwrap_or(TelemetryEventType::Unknown);
        let ts = event_reader.get_timestamp();

        eprintln!(
            "[telemetry] ts={} type={:?} match={} game={}",
            ts, ev_type, match_id, game_id
        );

        capnp::capability::Promise::ok(())
    }
    // log_event(params, results)
}
// telemetry_receiver::Server for TelemetryReceiverImpl

/// Exports the binary log file to a JSON array for external processing.
fn export_json(path: &str) -> Result<()> {
    let mut file = std::fs::File::open(path)?;
    let mut stdout = std::io::stdout().lock();
    stdout.write_all(b"[\n")?;
    let mut first = true;

    loop {
        let mut len_buf = [0u8; 4];
        match file.read_exact(&mut len_buf) {
            Ok(()) => {}
            Err(ref e) if e.kind() == std::io::ErrorKind::UnexpectedEof => break,
            Err(e) => return Err(e.into()),
        }
        let len = u32::from_le_bytes(len_buf) as usize;

        let mut msg_buf = vec![0u8; len];
        file.read_exact(&mut msg_buf)?;

        let reader = serialize_packed::read_message(&mut msg_buf.as_slice(), ReaderOptions::new())?;
        let event = reader
            .get_root::<amp_telemetry::amp_telemetry_capnp::amp_telemetry_event::Reader<'_>>()?;

        let match_id = match event.get_match_id() {
            Ok(d) => hex::encode(d),
            Err(_) => String::new(),
        };
        let game_id = event.get_game_id();
        let verifier_id = match event.get_verifier_id() {
            Ok(d) => hex::encode(d),
            Err(_) => String::new(),
        };
        let event_data = match event.get_event_data() {
            Ok(d) => hex::encode(d),
            Err(_) => String::new(),
        };
        let ev_type = event
            .get_event_type()
            .unwrap_or(TelemetryEventType::Unknown);
        let ts = event.get_timestamp();

        if !first {
            stdout.write_all(b",\n")?;
        }
        first = false;

        write!(
            stdout,
            "  {{\"match_id\":\"{}\",\"game_id\":{},\"event_type\":\"{:?}\",\"timestamp\":{},\"verifier_id\":\"{}\",\"event_data\":\"{}\"}}",
            match_id, game_id, ev_type, ts, verifier_id, event_data
        )?;
    }

    stdout.write_all(b"\n]\n")?;
    stdout.flush()?;
    Ok(())
}
// export_json(path)

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<()> {
    dotenv::dotenv().ok();
    let args: Vec<String> = std::env::args().collect();

    if args.len() >= 3 && args[1] == "--export" {
        return export_json(&args[2]);
    }

    let addr_str = args.get(1).map(|s| s.as_str()).unwrap_or("127.0.0.1:4317");
    let log_path = args.get(2).map(|s| s.as_str()).unwrap_or("telemetry.bin");

    let addr = addr_str
        .to_socket_addrs()?
        .next()
        .ok_or_else(|| anyhow::anyhow!("Invalid address"))?;

    let log_file = std::fs::OpenOptions::new()
        .create(true)
        .append(true)
        .open(log_path)?;
    let log_writer = Arc::new(Mutex::new(BufWriter::new(log_file)));

    let receiver: telemetry_receiver::Client = capnp_rpc::new_client(TelemetryReceiverImpl {
        log_writer,
    });

    let listener = TcpListener::bind(&addr).await?;
    eprintln!(
        "AMP Telemetry Receiver listening on {}  (log → {})",
        addr, log_path
    );

    let local = LocalSet::new();
    local.run_until(async move {
        loop {
            if let Ok((stream, _)) = listener.accept().await {
                stream.set_nodelay(true).unwrap_or(());
                let (reader, writer) = stream.compat().split();
                let network = twoparty::VatNetwork::new(
                    reader,
                    writer,
                    rpc_twoparty_capnp::Side::Server,
                    Default::default(),
                );
                let rpc_system = RpcSystem::new(Box::new(network), Some(receiver.clone().client));
                tokio::task::spawn_local(rpc_system);
            }
        }
    }).await;

    Ok(())
}
// main()
