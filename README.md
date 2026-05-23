# Avalanche Matchmaking Protocol (AMP)

### Any Engine. Any Language. Trustless Settlement.

AMP is a decentralized, high-performance matchmaking protocol built on Avalanche. It provides game developers with enterprise-grade matchmaking capabilities — similar to AWS FlexMatch — but with the added security and transparency of on-chain verifiers and cryptographically settled outcomes.

---

## Award Winning Technology

AMP was awarded a **$15,000.00 Merit Grant** for placing in the **top 20 projects** of the **Avalanche Build Games 2026**.

---

## Key Features

- **Thread-per-Core Architecture**: `SO_REUSEPORT` worker threads scale across all CPU cores. Each worker runs an independent `LocalSet` for Cap'n Proto RPC — zero contention on the accept path.
- **Sub-Millisecond Latency**: Cap'n Proto RPC with zero-copy serialization. ECDSA signature verification offloaded to `spawn_blocking` to keep worker event loops responsive.
- **Trustless Settlement**: Off-chain verifiers process deterministic match transcripts and commit outcome attestations directly to Avalanche smart contracts.
- **Challenge-Response Auth**: Server-issued nonces signed by player wallets prevent replay attacks. Authenticated sessions gate all mutating RPCs.
- **MMR & Glicko-2**: Advanced player skill tracking with binary search matchmaking (50ms tick). Crash-safe settle-before-MMR ordering prevents double-count corruption.
- **Reliable Settlement Bridge**: Sled-backed persistent queue with EIP-1559 gas management, nonce tracking, exponential backoff retry, and dead-letter queue. Input validation at RPC boundary (match_id, transcript_hash, signature, outcome).
- **TLS Support**: Optional `rustls`-based TLS for all services. Misconfiguration is a hard error (never silent fallback to plaintext).
- **Graceful Shutdown**: SIGINT and SIGTERM handling across all services. CancellationToken propagation drains active matches and flushes persistence before exit.
- **Docker-Native**: Network segmentation (internal + frontend), Docker secrets for all private keys, TLS cert mounts, `stop_grace_period`, non-root containers, and `SO_REUSEPORT` compatibility.
- **Unified Schema**: Single Cap'n Proto schema source generates native bindings for Go, Rust, C++, C#, and Python.

---

## Architecture

```
                   AMP v0.0.1 Architecture

┌─────────────────────────────────────────────────────────────┐
│                 Main OS Thread (Coordinator)                │
│  - SIGINT / SIGTERM → CancellationToken propagation        │
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
└─────────────────────────────────────────────────────────────┘
         │ log events                │ settlement tasks
         ▼                           ▼
┌──────────────────────┐  ┌──────────────────────┐
│  Telemetry Client    │  │  Relayer Client      │
│  Dedicated OS thread │  │  Dedicated OS thread │
│  Persistent TCP      │  │  Persistent TCP+Auth │
└──────────────────────┘  └──────────────────────┘
```

### Project Layout

```
AMP/
├── amp-server/        # Matchmaker & Verifier (Rust, Cap'n Proto, SO_REUSEPORT)
├── amp-relayer/       # Settlement bridge (Rust, sled queue, EIP-1559)
├── amp-telemetry/     # Binary telemetry log with JSON export
├── amp-tls/           # Shared rustls TLS acceptor factory
├── amp-sdk/           # Multi-language SDKs & Cap'n Proto schemas
│   ├── schemas/       # Canonical .capnp definitions
│   ├── rust/          # Rust SDK
│   ├── go/            # Go SDK
│   ├── cpp/           # C++ SDK (Unreal)
│   ├── csharp/        # C# SDK (Unity/Godot)
│   └── python/        # Python SDK
├── amp-loadtest/      # Load testing tool
├── contracts/         # Solidity (Forge, ^0.8.33, OpenZeppelin v5.6.1)
├── docker/            # Multi-stage Dockerfiles
├── trace-viewer/      # Telemetry binary log viewer
└── scripts/           # Deployment utilities
```

---

## Smart Contracts (Fuji Testnet)

| Contract | Address |
| :--- | :--- |
| **AMPRegistry** | `0x8479491220D8d56F32f1a4A5Cc827cf056a9aC34` |
| **AMPSettlement** | `0xecD9C6C1727d610A7C0Aeb3a37A6278049791a24` |

Contracts use OpenZeppelin v5.6.1 with `Ownable2Step`, `Pausable`, and `ERC2771Context` (meta-transactions).

---

## Getting Started

### Prerequisites

- Rust 1.87+ (edition 2024)
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
./target/release/AMP-Server

# Run the settlement relayer
FUJI_RPC_URL=https://api.avax-test.network/ext/bc/C/rpc \
CONTRACT_REGISTRY=0x... \
CONTRACT_SETTLEMENT=0x... \
RELAYER_PRIVATE_KEY_FILE=./secrets/relayer_key.txt \
./target/release/amp-relayer

# Run the telemetry receiver
./target/release/amp-telemetry 0.0.0.0:4317 ./telemetry.bin
```

### Docker Deployment

```bash
# Create secrets directory
mkdir -p secrets
echo "0xYOUR_PRIVATE_KEY" > secrets/verifier_key.txt
echo "0xYOUR_RELAYER_KEY" > secrets/relayer_key.txt
echo "your-api-key-here" > secrets/relayer_api_key.txt

# Configure environment
cp .env.example .env
# Edit .env with your contract addresses and RPC URL

# Launch all services
docker compose up --build -d
```

### Environment Variables

See [.env.example](.env.example) for the complete list. Key variables:

| Variable | Service | Description |
|:---|:---|:---|
| `AMP_WORKERS` | server | Number of SO_REUSEPORT worker threads (default: num_cpus) |
| `AMP_MAX_CONCURRENT` | server | Max simultaneous connections (default: 1000) |
| `AMP_MAX_PER_IP_PER_MIN` | server | Rate limit per IP (default: 60) |
| `VERIFIER_KEY_FILE` | server | Path to Ethereum signing key for match attestations |
| `RELAYER_PRIVATE_KEY_FILE` | relayer | Path to settlement signing key |
| `RELAYER_API_KEY_FILE` | both | Path to API key for relayer authentication |
| `RELAYER_MAX_CONNECTIONS` | relayer | Max concurrent RPC connections (default: 50) |
| `TELEMETRY_MAX_CONNECTIONS` | telemetry | Max concurrent telemetry connections (default: 100) |

---

## AMP vs AWS FlexMatch

| Feature | AMP | AWS FlexMatch |
|:---|:---|:---|
| **Deployment** | Self-hosted, any cloud or bare metal | AWS only |
| **Latency** | Sub-ms (Cap'n Proto, zero-copy) | 10-50ms (REST/gRPC) |
| **Settlement** | On-chain, trustless, verifiable | N/A (off-chain only) |
| **Auth** | Wallet-based challenge-response | AWS IAM / Cognito |
| **Skill System** | Glicko-2 with configurable rules | Custom expressions |
| **Cost** | Infrastructure only | $0.50/GB data + compute |
| **Sovereignty** | Full control, no vendor lock-in | AWS dependency |
| **Protocol** | Cap'n Proto (Rust, Go, C++, C#, Python) | Proprietary SDK |
| **Scalability** | Thread-per-core, horizontal | AWS-managed |

---

## Test Status

| Suite | Count | Command |
|:---|:---|:---|
| Rust tests | 30 | `cargo test --workspace` |
| Forge tests | 31 | `cd contracts && forge test -vvv` |
| Clippy | clean | `cargo clippy --workspace --all-targets -- -D warnings` |
| Formatting | clean | `cargo fmt --all -- --check` |

---

## Security

- All private keys loaded via `*_FILE` env vars or Docker secrets (never inline)
- `Config::Debug` redacts private keys
- API keys hashed (SHA-256) before storage; constant-time comparison recommended
- TLS is opt-in but enforced when configured (no silent fallback)
- Non-root containers in all Docker images
- Input validation at RPC boundary (match_id, outcome, transcript_hash, signature lengths)
- Rate limiting with RAII connection guards
- Settle-before-MMR ordering prevents crash-induced rating corruption

---

## License

AMP is licensed under the **AMP Non-Commercial Source License**.
Personal and educational use is permitted for free. **Commercial use requires a separate license agreement.**

See the [LICENSE](LICENSE) file for the full terms or contact the repository owner for commercial inquiries.
