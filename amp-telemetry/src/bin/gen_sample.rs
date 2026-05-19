//! Generates a sample `telemetry.bin` with synthetic Cap'n Proto events
//! so the trace-viewer can be tested without a live stack.
//!
//! Usage: cargo run --bin gen_sample -- [output_path]

use std::io::Write;

use amp_telemetry::amp_telemetry_capnp::TelemetryEventType;
use amp_telemetry::amp_telemetry_capnp::amp_telemetry_event;

fn main() {
    let path = std::env::args()
        .nth(1)
        .unwrap_or_else(|| "telemetry.bin".into());

    let mut file = std::fs::File::create(&path).expect("create output file");

    // Two synthetic match IDs (16 bytes each)
    let match_a: [u8; 16] = [
        0xAA, 0xBB, 0xCC, 0xDD, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77, 0x88,
        0x99, 0x00, 0xAB, 0xCD,
    ];
    let match_b: [u8; 16] = [
        0xDE, 0xAD, 0xBE, 0xEF, 0x00, 0x11, 0x22, 0x33, 0x44, 0x55, 0x66, 0x77,
        0x88, 0x99, 0xAA, 0xBB,
    ];

    // A fake verifier address (20 bytes)
    let verifier: [u8; 20] = [
        0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08, 0x09, 0x0A, 0x0B, 0x0C,
        0x0D, 0x0E, 0x0F, 0x10, 0x11, 0x12, 0x13, 0x14,
    ];

    let base_ts: u64 = 1_740_000_000_000_000_000; // ~2025-02-19 in nanos

    type SampleEvent<'a> =
        (&'a [u8], TelemetryEventType, u64, &'a [u8], &'a [u8]);

    let events: Vec<SampleEvent<'_>> = vec![
        (
            &match_a,
            TelemetryEventType::MatchCreated,
            base_ts,
            &[],
            b"",
        ),
        (
            &match_a,
            TelemetryEventType::MatchJoined,
            base_ts + 500_000_000, // +500ms
            &[],
            b"player2_joined",
        ),
        (
            &match_b,
            TelemetryEventType::MatchCreated,
            base_ts + 1_200_000_000, // +1.2s
            &[],
            b"",
        ),
        (
            &match_a,
            TelemetryEventType::SettlementSubmitted,
            base_ts + 3_000_000_000, // +3s
            &verifier,
            b"tx:0xabc123",
        ),
        (
            &match_b,
            TelemetryEventType::MatchJoined,
            base_ts + 3_500_000_000, // +3.5s
            &[],
            b"player2_joined",
        ),
        (
            &match_a,
            TelemetryEventType::VerifierResult,
            base_ts + 5_000_000_000, // +5s
            &verifier,
            b"result:valid",
        ),
        (
            &match_b,
            TelemetryEventType::SettlementSubmitted,
            base_ts + 6_000_000_000, // +6s
            &verifier,
            b"tx:0xdef456",
        ),
        (
            &match_b,
            TelemetryEventType::VerifierResult,
            base_ts + 8_000_000_000, // +8s
            &verifier,
            b"result:valid",
        ),
    ];

    for (mid, etype, ts, vid, edata) in &events {
        let mut msg = capnp::message::Builder::new_default();
        {
            let mut ev = msg.init_root::<amp_telemetry_event::Builder<'_>>();
            ev.set_match_id(mid);
            ev.set_event_type(*etype);
            ev.set_timestamp(*ts);
            if !vid.is_empty() {
                ev.set_verifier_id(vid);
            }
            if !edata.is_empty() {
                ev.set_event_data(edata);
            }
        }

        let mut packed_buf: Vec<u8> = Vec::new();
        capnp::serialize_packed::write_message(&mut packed_buf, &msg).unwrap();

        let len = packed_buf.len() as u32;
        file.write_all(&len.to_le_bytes()).unwrap();
        file.write_all(&packed_buf).unwrap();
    }

    eprintln!("Wrote {} events to {}", events.len(), path);
}
