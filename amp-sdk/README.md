# AMP SDKs

The AMP (Avalanche Matchmaking Protocol) SDKs provide clients for various game
engines and development environments to connect to the AMP Local Matchmaker
Service, join matchmaking, and submit verified game outcomes.

## SDKs Provided

| Language | Path | Status | TLS | Outcome Signing |
|:---|:---|:---|:---:|:---:|
| **Go** | [`go/`](go/) | Stable | ✅ | via callback |
| **Rust** | [`rust/`](rust/) | Stable | ✅ (`tls` feature) | ✅ digest helper + callback |
| **C# (.NET)** | [`csharp/`](csharp/) | Beta (Unity/Godot) | via reverse proxy | ✅ (custodial) |
| **C++** | [`cpp/`](cpp/) | Beta (Unreal) | ✅ (`kj-tls`, optional) | via callback |
| **Python** | [`python/`](python/) | Beta | ✅ (`ssl.SSLContext`) | ✅ (custodial) |
| **JavaScript / TypeScript** | [`js/`](js/) | Beta (Node.js, native) | via native | ✅ (custodial) |

See [`docs/SDK_USAGE.md`](docs/SDK_USAGE.md) for the integration flow and
[`../docs/signing.mdx`](../docs/signing.mdx) for the canonical signing schemes.

## Common Setup

Build the AMP matchmaker server first (see the [root README](../README.md)).
Each SDK then connects to the server's RPC port (default `50051`).

```bash
# From the repository root:
cargo build --release          # Build the AMP server + relayer + telemetry
./target/release/AMP-Server &  # Start the matchmaker on :50051
```

## Per-Language Setup

### Go

```bash
cd amp-sdk/go
go test ./...                  # Run unit tests
```

The Go SDK is the most complete reference implementation. See
[`go/client/client.go`](go/client/client.go) for the full client API.

### Rust

```bash
cargo test -p amp-sdk         # Run unit tests
cargo build -p amp-sdk --features tls  # Build with TLS support
```

### C# (.NET)

```bash
cd amp-sdk/csharp/AmpSdk
dotnet build
```

The C# SDK ships with `link.xml` for Unity IL2CPP preservation and pins
`Portable.BouncyCastle 1.9.0` + `Newtonsoft.Json 13.0.3` (see
[`SECURITY_REVIEW.md`](../../SECURITY_REVIEW.md) S12).

### C++

```bash
cd amp-sdk/examples/cpp
cmake -B build -S .
cmake --build build
./build/amp_test 127.0.0.1:50051
```

> **Note:** The C++ example no longer ships a hardcoded signature. Set
> `AMP_EXAMPLE_SIGNATURE_HEX` (for local testing) or implement a real
> `signChallenge` callback in `examples/cpp/src/main.cpp`. See
> [`SECURITY_REVIEW.md`](../../SECURITY_REVIEW.md) S7.

### Python

```bash
cd amp-sdk/python
pip install -e ".[dev]"        # Editable install with dev extras
pytest                         # Run unit tests
```

### JavaScript / TypeScript

```bash
cd amp-sdk/js
npm install                    # pulls @napi-rs/cli + ethers
npm run build                  # builds the native .node (napi build) + tsc
npm test                       # cross-language EIP-712 KAT + native digest tests
```

The JS SDK is Node.js-only and built on a native Rust core (napi-rs). For
browser use, front the AMP server with a TLS-terminating WebSocket bridge.

## E2E Testing

Once the AMP server is running locally, you can execute a full end-to-end test
from the repository root:

```bash
make test-integration
```

This deploys contracts to a local Anvil network, runs telemetry/relayer/server,
and simulates end-to-end gameplay settlement.

## Cross-Language Integrity

The EIP-712 digest over `(matchId, outcome, transcriptHash)` is byte-identical
across Rust, C#, Python, and JavaScript. The shared known-answer test vector
is enforced in each language's test suite — see
[`docs/signing.mdx`](../docs/signing.mdx) for details.
