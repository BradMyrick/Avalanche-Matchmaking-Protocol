# Avalanche Matchmaking Protocol (AMP)

### High-performance matchmaking with on-chain settlement. Built for competitive games on Avalanche.

AMP pairs players, runs the match, and settles the outcome on-chain — all with sub-millisecond matchmaking and cryptographically verifiable payouts. Today's open beta ships 1v1 wagered play; the same architecture scales to large-team formats, battle royales, and persistent-world play as the protocol matures.

**Documentation:** [docs.page](https://docs.page/bradmyrick/avalanche-matchmaking-protocol) (or read the [`docs/`](docs/) folder locally).

---

## Award

<p align="center">
  <img src="docs/images/grant_badge.png" alt="Avalanche Build Games 2026 Merit Grant" width="300" />
</p>

AMP was awarded a **$15,000 Merit Grant** for placing in the **top 20 projects** of the **Avalanche Build Games 2026**.

---

## Why AMP?

- **Sub-millisecond matchmaking.** Cap'n Proto RPC, thread-per-core architecture, DashMap matchstore.
- **Verifiable outcomes.** EIP-712 verifier signatures committed to smart-contract escrow on Avalanche.
- **Custodial escrow, not custody.** Stakes sit in the `AMPRegistry` contract — never in the operator's wallet. Payouts are deterministic, governed by code.
- **Open source, Apache-2.0.** Run it, embed it, fork it, contribute back.

---

## Two integration paths

### Run the server

Deploy `AMP-Server` + `AMP-Relayer` + the `AMPRegistry` / `AMPSettlement` contracts. Players authenticate via wallet challenge-response, get matched, play, and settle on-chain. The Docker Compose stack gets you running in minutes.

### Embed the library

Pull in `amp-match-core` — a dependency-light Rust crate containing the Glicko-2 rating math, the rule engine, and the matchmaking queue. No server, RPC, async, or crypto deps. Use it inside your own game server, peer-to-peer game, or analytics tool. See [`amp-match-core/README.md`](amp-match-core/README.md).

---

## Key Features

- **Embeddable matchmaking library** (`amp-match-core`): Glicko-2 + composable rule evaluation + bucketed queue, zero server/RPC/async/crypto deps.
- **Thread-per-Core Server**: `SO_REUSEPORT` worker threads scale across all CPU cores. Each worker runs an independent `LocalSet` for Cap'n Proto RPC.
- **Capability-based RPC** with **challenge-response auth**: server-issued nonces signed by player wallets prevent replay attacks.
- **Glicko-2 rating system** with NaN/Inf/non-positive-volatility guards and a bounded Illinois-method solver.
- **Custodial escrow with on-chain settlement**: stakes sit in the `AMPRegistry` contract — never in the operator's wallet. Outcomes are committed via verifier signatures (EIP-712) and enforced by the contract's payout rules.
- **Reliable settlement bridge** (`amp-relayer`): sled-backed persistent queue with explicit flush on shutdown, EIP-1559 gas management, nonce tracking, exponential backoff retry, dead-letter queue, and `Notify`-driven wake.
- **TLS support**: optional `rustls`-based TLS for all services. Misconfiguration is a hard error (never silent fallback to plaintext).
- **Graceful shutdown**: SIGINT/SIGTERM handling across all services. `CancellationToken` propagation drains active matches and flushes persistence before exit.
- **Docker-native**: network segmentation, Docker secrets, TLS cert mounts, non-root containers.
- **Unified schema**: single Cap'n Proto schema source generates native bindings for Rust, Go, C++, and C#.
- **Supply-chain CI gates**: `cargo-deny` enforces RUSTSEC advisory policy, license allowlist, and source policy.
- **All six SDKs tier-1 for outcome signing**: Rust, Go, C++, C#, Python, JS all ship with EIP-712 digest verified against a cross-language KAT in CI. C++ and C# bundle the Keccak-256 needed for digest computation.

---

## SDKs

| SDK | Engines / Use cases | TLS | Tests |
|:---|:---|:---:|:---:|
| **Rust** | Reference implementation, performance tooling | ✅ | ✅ |
| **Go** | Server-side, high-frequency backends | ✅ | ✅ |
| **C++** | Unreal Engine, custom engines | ✅ | ✅ |
| **C#** | Unity, Godot | reverse-proxy | ✅ |
| **Python** | Scripting, AI agents, research | ✅ | ✅ |
| **JS / TS** | Node.js services | n/a | ✅ |

See the [SDK Overview](https://docs.page/bradmyrick/avalanche-matchmaking-protocol/sdk-overview) for per-language setup.

---

## Architecture

```
                    AMP v0.1.0 Architecture

┌─────────────────────────────────────────────────────────────┐
│                 Main OS Thread (Coordinator)                │
│  - SIGINT / SIGTERM → CancellationToken propagation        │
│  - Flush persistence before exit                           │
└──────────────────────────────┬──────────────────────────────┘
                               │
         ┌─────────────────────┼─────────────────────┐
         ▼                     ▼                     ▼
┌──────────────────┐ ┌──────────────────┐ ┌──────────────────┐
│   amp-worker-0   │ │   amp-worker-1   │ │   amp-worker-N   │
│ SO_REUSEPORT     │ │ SO_REUSEPORT     │ │ SO_REUSEPORT     │
│ LocalSet + TLS   │ │ LocalSet + TLS   │ │ LocalSet + TLS   │
│ Cap'n Proto RPC  │ │ Cap'n Proto RPC  │ │ Cap'n Proto RPC  │
└──────────────────┘ └──────────────────┘ └──────────────────┘
         │                     │                     │
         │  QueueEntry (mpsc)  │  RelayerTask (mpsc) │  Vec<u8> (mpsc)
         ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────────┐
│             Tokio Multi-threaded Runtime                    │
│  Matchmaker Loop ────── Cleanup Loop ────── Signer         │
│  (amp-match-core queue + rules + Glicko-2)                  │
└─────────────────────────────────────────────────────────────┘
         │ log events                │ settlement tasks
         ▼                           ▼
┌──────────────────────┐  ┌──────────────────────┐
│  Telemetry Client    │  │  Relayer Client      │
│  Dedicated OS thread │  │  Dedicated OS thread │
│  Persistent TCP      │  │  Persistent TCP+Auth │
└──────────────────────┘  └──────────────────────┘
```

Full architecture: [`docs/architecture.mdx`](docs/architecture.mdx).

---

## Project Layout

```
AMP/
├── amp-server/        # Matchmaker & Verifier (Rust, Cap'n Proto, SO_REUSEPORT)
├── amp-relayer/       # Settlement bridge (Rust, sled queue, EIP-1559)
├── amp-match-core/    # Embeddable matchmaking library (Glicko-2 + rules + queue)
├── amp-telemetry/     # Binary telemetry log with JSON export
├── amp-tls/           # Shared rustls TLS acceptor factory
├── amp-sdk/           # Multi-language SDKs & Cap'n Proto schemas
├── amp-loadtest/      # Load testing tool
├── contracts/         # Solidity (Forge, ^0.8.33, OpenZeppelin v5.6.1)
├── docs/              # Documentation (docs.page source)
├── docker/            # Multi-stage Dockerfiles
├── trace-viewer/      # Telemetry binary log viewer
└── scripts/           # Deployment utilities
```

---

## Smart Contracts (Live on Fuji)

| Contract | Address |
|:---|:---|
| `AMPRegistry` | [`0x27E02ebA98D2A50Cd1079b0a611320b05A278005`](https://testnet.snowtrace.io/address/0x27E02ebA98D2A50Cd1079b0a611320b05A278005) |
| `AMPSettlement` | [`0xc1b12a7Ffad6CeFf045064f9fE3E8879F0F3c9eD`](https://testnet.snowtrace.io/address/0xc1b12a7Ffad6CeFf045064f9fE3E8879F0F3c9eD) |
| `AMPTimelock` | [`0xb6d9A7e2C6d1B551C8166d9E489a8BA39B008143`](https://testnet.snowtrace.io/address/0xb6d9A7e2C6d1B551C8166d9E489a8BA39B008143) |

Both contracts are `TimelockController`-wrapped (governance finalized). Deployment manifest: [`contracts/deployment-fuji.json`](contracts/deployment-fuji.json). C-Chain mainnet deployment follows the [Roadmap](https://docs.page/bradmyrick/avalanche-matchmaking-protocol/roadmap) v0.3 milestone.

---

## Getting Started

### Prerequisites

- Rust 1.91+ (the `rust-toolchain.toml` pins the version)
- capnproto C library (`apt install capnproto` or `brew install capnp`)
- Foundry (for contract builds)
- Docker + Docker Compose (for containerized deployment)

### Local Development

```bash
# Build all Rust components
cargo build --release

# Run the matchmaker/verifier
AMP_ADDR=0.0.0.0:50051 \
AMP_SETTLEMENT_ADDRESS=0x... \
VERIFIER_KEY_FILE=./secrets/verifier_key.txt \
RELAYER_RPC_ADDR=localhost:50052 \
RELAYER_API_KEY_FILE=./secrets/relayer_api_key.txt \
./target/release/AMP-Server

# Run the settlement relayer
FUJI_RPC_URL=https://api.avax-test.network/ext/bc/C/rpc \
CONTRACT_REGISTRY=0x... \
CONTRACT_SETTLEMENT=0x... \
RELAYER_PRIVATE_KEY_FILE=./secrets/relayer_key.txt \
RELAYER_API_KEY_FILE=./secrets/relayer_api_key.txt \
./target/release/amp-relayer

# Run the telemetry receiver
./target/release/amp-telemetry 0.0.0.0:9317 ./telemetry.bin
```

### Docker Deployment

```bash
mkdir -p secrets
echo "0xYOUR_PRIVATE_KEY" > secrets/verifier_key.txt
echo "0xYOUR_RELAYER_KEY" > secrets/relayer_key.txt
echo "your-api-key-here" > secrets/relayer_api_key.txt

cp .env.example .env  # Edit with your contract addresses and RPC URL

docker compose up --build -d
```

See [.env.example](.env.example) for the full environment-variable reference. For an end-to-end walkthrough, read the [Beta Guide](https://docs.page/bradmyrick/avalanche-matchmaking-protocol/beta-guide).

---

## Testing & Verification

| Target | Command |
|:---|:---|
| Rust unit tests | `cargo test --workspace` |
| Solidity contracts | `cd contracts && forge test -vvv` |
| Embeddable library | `cargo test -p amp-match-core` |
| C++ SDK tests | `cd amp-sdk/cpp && cmake -B build-tests && cmake --build build-tests && ./build-tests/amp_tests` |
| C# SDK tests | `cd amp-sdk/csharp/AmpSdk.Tests && dotnet test` |
| Go SDK tests | `make test-sdk-go` |
| Python SDK tests | `cd amp-sdk/python && pytest` |
| JS SDK tests | `cd amp-sdk/js && npm test` |
| E2E integration | `make test-integration` |
| Supply-chain | `cargo deny --workspace check --config deny.toml` |
| Lint | `make lint` |
| Format check | `make format` |

---

## Security

- All private keys loaded via `*_FILE` env vars or Docker secrets (never inline)
- API keys hashed (SHA-256) with constant-time comparison
- Inter-service API key required by default
- TLS opt-in, enforced when configured (no silent fallback)
- Non-root containers in all Docker images
- Input validation at RPC boundary
- Submitter signature verified on `submitOutcome` — recovered address must match the caller's `player_id`
- Both contracts `TimelockController`-wrapped at deploy time
- Persistence explicitly flushed on shutdown

For the responsible disclosure policy, see [`SECURITY.md`](SECURITY.md).

---

## License

AMP is licensed under the **Apache License, Version 2.0**. See the [LICENSE](LICENSE) file for the full terms.
