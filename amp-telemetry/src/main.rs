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

/// Wraps a buffered file writer behind a Mutex so the Cap'n Proto RPC handler
/// can append packed messages from any spawned task.
struct TelemetryReceiverImpl {
    log_writer: Arc<Mutex<BufWriter<std::fs::File>>>,
}

impl telemetry_receiver::Server for TelemetryReceiverImpl {
    fn log_event(
        &mut self,
        params: telemetry_receiver::LogEventParams,
        mut _results: telemetry_receiver::LogEventResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        // Read the incoming message and re-serialize as a packed Cap'n Proto
        // frame directly to the log file. This avoids any intermediate text
        // encoding — the on-disk format IS Cap'n Proto packed binary.
        let params_reader = match params.get() {
            Ok(p) => p,
            Err(e) => return capnp::capability::Promise::err(e),
        };

        // Build a new standalone message containing just the event, so the
        // log file is a sequence of independent packed messages.
        let event_reader = match params_reader.get_event() {
            Ok(e) => e,
            Err(e) => return capnp::capability::Promise::err(e),
        };

        let mut out_msg = capnp::message::Builder::new_default();
        out_msg.set_root(event_reader).unwrap();

        // Write length-prefixed packed message to the log file.
        // Frame format: [4-byte LE payload length][packed capnp bytes]
        let mut packed_buf: Vec<u8> = Vec::new();
        serialize_packed::write_message(&mut packed_buf, &out_msg).unwrap();

        let len = packed_buf.len() as u32;
        let mut writer = self.log_writer.lock().unwrap();
        writer.write_all(&len.to_le_bytes()).unwrap();
        writer.write_all(&packed_buf).unwrap();
        writer.flush().unwrap();

        // Also print a human-readable line to stdout for operator visibility
        let match_id = match event_reader.get_match_id() {
            Ok(data) => hex::encode(data),
            Err(_) => "unknown".to_string(),
        };
        let ev_type = event_reader
            .get_event_type()
            .unwrap_or(TelemetryEventType::Unknown);
        let ts = event_reader.get_timestamp();

        eprintln!(
            "[telemetry] ts={} type={:?} match={}",
            ts, ev_type, match_id
        );

        capnp::capability::Promise::ok(())
    }
}

/// Read a binary log file and print each event as a JSON object to stdout.
fn export_json(path: &str) -> Result<()> {
    let mut file = std::fs::File::open(path)?;
    let mut stdout = std::io::stdout().lock();
    stdout.write_all(b"[\n")?;
    let mut first = true;

    loop {
        // Read 4-byte LE length prefix
        let mut len_buf = [0u8; 4];
        match file.read_exact(&mut len_buf) {
            Ok(()) => {}
            Err(ref e) if e.kind() == std::io::ErrorKind::UnexpectedEof => break,
            Err(e) => return Err(e.into()),
        }
        let len = u32::from_le_bytes(len_buf) as usize;

        // Read the packed message bytes
        let mut msg_buf = vec![0u8; len];
        file.read_exact(&mut msg_buf)?;

        let reader = serialize_packed::read_message(&mut msg_buf.as_slice(), ReaderOptions::new())?;
        let event = reader
            .get_root::<amp_telemetry::amp_telemetry_capnp::amp_telemetry_event::Reader<'_>>()?;

        let match_id = match event.get_match_id() {
            Ok(d) => hex::encode(d),
            Err(_) => String::new(),
        };
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

        // Write a compact JSON object — the trace-viewer already handles this format
        write!(
            stdout,
            "  {{\"match_id\":\"{}\",\"event_type\":\"{:?}\",\"timestamp\":{},\"verifier_id\":\"{}\",\"event_data\":\"{}\"}}",
            match_id, ev_type, ts, verifier_id, event_data
        )?;
    }

    stdout.write_all(b"\n]\n")?;
    stdout.flush()?;
    Ok(())
}

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<()> {
    let args: Vec<String> = std::env::args().collect();

    // ── Export mode: `amp-telemetry --export telemetry.bin` ──────────
    if args.len() >= 3 && args[1] == "--export" {
        return export_json(&args[2]);
    }

    // ── Server mode ─────────────────────────────────────────────────
    let addr_str = args.get(1).map(|s| s.as_str()).unwrap_or("127.0.0.1:4317");
    let log_path = args.get(2).map(|s| s.as_str()).unwrap_or("telemetry.bin");

    let addr = addr_str
        .to_socket_addrs()?
        .next()
        .expect("could not parse address");

    let log_file = std::fs::OpenOptions::new()
        .create(true)
        .append(true)
        .open(log_path)?;
    let log_writer = Arc::new(Mutex::new(BufWriter::new(log_file)));

    let receiver: telemetry_receiver::Client = capnp_rpc::new_client(TelemetryReceiverImpl {
        log_writer: log_writer.clone(),
    });

    let listener = TcpListener::bind(&addr).await?;
    eprintln!(
        "AMP Telemetry Receiver listening on {}  (log → {})",
        addr, log_path
    );

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
