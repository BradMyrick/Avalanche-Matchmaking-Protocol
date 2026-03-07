# AMP Telemetry Receiver

A lightweight Cap'n Proto RPC server that receives `TelemetryEvent` emissions from AMP Matchmaker sessions and outputs them as JSON traces for analysis.

## Usage

Start the receiver (listens on `127.0.0.1:4317` by default):
```bash
cargo run
```

When an AMP client emits a telemetry event via the `MatchSession::emitTelemetry` capability, this service will output a JSON trace span to `stdout`.

## Integrating with Trace Viewer

The [Trace Viewer](../trace-viewer/README.md) visualizes the text output of this service.

1. Capture the output of `amp-telemetry` to a file:
   ```bash
   cargo run > my_traces.log
   ```
2. Open `../trace-viewer/index.html` in your browser.
3. Click "Load Trace File" and select `my_traces.log`.

*(Note: In the MVP `test_mvp.sh` script, the output is automatically piped to `telemetry.log` which can be directly loaded into the viewer).*
