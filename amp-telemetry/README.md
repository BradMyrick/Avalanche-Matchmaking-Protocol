# AMP Telemetry

The **AMP Telemetry** service is a high-throughput binary logging system for the AMP protocol. It receives packed Cap'n Proto telemetry events from the AMP Server and persists them to a binary log file.

## 🚀 Features
- **Pure Cap'n Proto RPC**: Binary receiver on port 4317.
- **Packed Binary Persistence**: Efficiently stores events in a length-prefixed packed format (`telemetry.bin`).
- **Low Overhead**: Zero JSON encoding/decoding during the hot path.
- **Export to JSON**: Built-in CLI for exporting binary logs to JSON for analysis.

## 🛠 Usage

### Start Receiver
```bash
# Listen on 127.0.0.1:4317, log to telemetry.bin
cargo run -- bin 127.0.0.1:4317 telemetry.bin
```

### Export to JSON
```bash
cargo run -- --export telemetry.bin > traces.json
```

## 🏗 Architecture
The receiver implements the `TelemetryReceiver` interface (see `amp_telemetry.capnp`). It is designed to be a "dumb" sink for high-volume game events, ensuring the matchmaker is never blocked by telemetry processing.
