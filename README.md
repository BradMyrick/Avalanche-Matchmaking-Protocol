# Avalanche Matchmaking Protocol (AMP)

### Any Engine. Any Language. Trustless Settlement.

AMP is a decentralized, high-performance matchmaking protocol built on Avalanche. It provides game developers with enterprise-grade matchmaking capabilities, with the added security and transparency of on-chain verifiers and cryptographically settled outcomes.

---

## Award Winning Technology

<p align="center">
  <img src="docs/images/grant_badge.png" alt="Avalanche Build Games 2026 Merit Grant" width="300" />
</p>

AMP was awarded a **$15,000.00 Merit Grant** for placing in the **top 20 projects** of the **Avalanche Build Games 2026**.

---

## Documentation

Full documentation including architecture deep-dives, integration guides, SDK references, and contract API docs is available in the [`docs/`](docs/) directory.

---

## Key Features

- **Thread-per-Core Architecture**: `SO_REUSEPORT` worker threads scale across all CPU cores. Each worker runs an independent `LocalSet` for Cap'n Proto RPC — zero contention on the accept path.
- **Sub-Millisecond Matchmaking**: Cap'n Proto RPC with zero-copy serialization, a `DashMap` matchstore (no clone-on-write), and ECDSA verification offloaded to `spawn_blocking`. Measured: matchstore update **72 ns** @ 10k matches (was ~1.4 ms before the `DashMap` migration). See [`docs/benchmarks.md`](docs/benchmarks.md).
- **Trustless Settlement**: Off-chain verifiers process deterministic match transcripts and commit outcome attestations directly to Avalanche smart contracts.
- **Challenge-Response Auth**: Server-issued nonces signed by player wallets prevent replay attacks. Authenticated sessions gate all mutating RPCs.
- **MMR & Glicko-2**: Advanced player skill tracking with binary search matchmaking (50ms tick). Settle-before-MMR ordering + sled schema versioning prevent double-count corruption (durability hardening scheduled with the sled→redb migration).
- **Reliable Settlement Bridge**: Sled-backed persistent queue with EIP-1559 gas management, nonce tracking, exponential backoff retry, dead-letter queue, and a `Notify`-driven processor (no fixed throughput cap). Input validation at RPC boundary (match_id, transcript_hash, signature, outcome).
- **TLS Support**: Optional `rustls`-based TLS for all services. Misconfiguration is a hard error (never silent fallback to plaintext).
- **Graceful Shutdown**: SIGINT and SIGTERM handling across all services. CancellationToken propagation drains active matches and flushes persistence before exit.
- **Docker-Native**: Network segmentation (internal + frontend), Docker secrets for all private keys, TLS cert mounts, `stop_grace_period`, non-root containers, and `SO_REUSEPORT` compatibility.
- **Unified Schema**: Single Cap'n Proto schema source generates native bindings for Go, Rust, C++, C#, and Python.

---

## Architecture

```
                    AMP v0.1.0 Architecture

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
│   ├── python/        # Python SDK
│   ├── js/            # JavaScript / TypeScript SDK (Node.js)
│   └── examples/      # Per-language end-to-end examples
├── amp-loadtest/      # Load testing tool
├── contracts/         # Solidity (Forge, ^0.8.33, OpenZeppelin v5.6.1)
├── docs/              # Documentation (signing.mdx, architecture, SDK guides, contracts reference)
├── SECURITY_REVIEW.md # Threat model, findings, and remediation status
├── docker/            # Multi-stage Dockerfiles
├── trace-viewer/      # Telemetry binary log viewer
└── scripts/           # Deployment utilities
```

---

## Smart Contracts

The contracts (`AMPRegistry` custodial escrow + `AMPSettlement` verifier) are
hardened and covered by unit, fuzz, and value-conservation invariant tests
(Phase 1). They are deployed on the Fuji testnet; a reproducible, verified
Fuji redeployment + C-Chain mainnet deployment is scoped in
[`prodpath.md`](prodpath.md) Phases 7 and 10. See
[`docs/contracts-reference.mdx`](docs/contracts-reference.mdx) for the API.

---

## Getting Started

### Prerequisites

- Rust 1.87+ (edition 2024)
- capnproto C library (`apt install capnproto` or `brew install capnp`)
- Foundry (for contract builds)
- Docker + Docker Compose (for containerized deployment)

### Local Development

> **Note:** Both the server and the relayer now require `RELAYER_API_KEY` by default. Set `AMP_ALLOW_UNAUTHENTICATED_RELAYER=1` for local dev to bypass (see [SECURITY_REVIEW.md](SECURITY_REVIEW.md) S11).

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
| `RELAYER_API_KEY_FILE` | both | Path to API key for relayer authentication (**required by default**) |
| `AMP_ALLOW_UNAUTHENTICATED_RELAYER` | both | Set to `1` to bypass the API key requirement (**NOT RECOMMENDED** — see [SECURITY_REVIEW.md](SECURITY_REVIEW.md) S11) |
| `RELAYER_MAX_CONNECTIONS` | relayer | Max concurrent RPC connections (default: 50) |
| `RELAYER_MAX_PER_IP` | relayer | Max concurrent connections per source IP (default: 10) |
| `TELEMETRY_MAX_CONNECTIONS` | telemetry | Max concurrent telemetry connections (default: 100) |
| `AMP_EIP712_CHAIN_ID` | sdk | EIP-712 domain chain ID for outcome signatures (default: 43113 / Fuji) |
| `AMP_EIP712_VERIFYING_CONTRACT` | sdk | EIP-712 domain verifying contract (default: zero — must be set before signing) |

---

## Testing & Verification

AMP is equipped with modular unit test suites and a comprehensive end-to-end integration test suite.

### Prerequisites
- **capnp** C++ library (`apt install capnproto` or `brew install capnp`)
- **Foundry** (specifically `forge` and `anvil`) for EVM simulation: `curl -L https://foundry.paradigm.xyz | bash && foundryup`
- **Go** (for Go SDK testing)
- **Python 3.9+** (for Python SDK testing)
- **Node.js 18+** (for JavaScript SDK testing)
- **.NET SDK 8.0+** (for C# SDK builds)

### Testing Commands

| Target | Description | Command |
|:---|:---|:---|
| **Rust Unit Tests** | Verifies matchmaking logic, Glicko-2 MMR updates, server components, and the cross-language EIP-712 digest KAT. | `cargo test --workspace` |
| **Solidity Contract Tests** | Verifies match registry rules, state machine transitions, and dispute resolution. | `cd contracts && forge test -vvv` |
| **Go SDK Tests** | Verifies Go client integrations, Glicko-2 MMR (with NaN guard), and schema deserialization. | `make test-sdk-go` |
| **Python SDK Tests** | Verifies digest computation, address derivation, expiry checks, and challenge signing. | `cd amp-sdk/python && pytest` |
| **JavaScript SDK Tests** | Verifies the EIP-712 digest KAT across all four implemented languages. | `cd amp-sdk/js && npm test` |
| **E2E Integration Tests** | Orchestrates a live local Anvil network, deploys contracts, runs telemetry/relayer/servers, and simulates E2E gameplay settlement. | `make test-integration` |
| **All Tests** | Runs all workspace unit and SDK tests sequentially. | `make test` |
| **Linters & Formatting** | Runs workspace lint check and code formatter checks. | `make lint` and `make format` |

To run the full E2E validation suite and verify system readiness to host players, execute:
```bash
make test-integration
```

---

## Security

- All private keys loaded via `*_FILE` env vars or Docker secrets (never inline)
- `Config::Debug` redacts private keys
- API keys hashed (SHA-256) before storage; **constant-time comparison** enforced on the relayer (S16)
- **Inter-service API key required by default** — both server and relayer refuse to start without `RELAYER_API_KEY` unless `AMP_ALLOW_UNAUTHENTICATED_RELAYER=1` is set (see [SECURITY_REVIEW.md](SECURITY_REVIEW.md) S11)
- TLS is opt-in but enforced when configured (no silent fallback)
- Non-root containers in all Docker images
- Input validation at RPC boundary (`match_id`, `outcome`, `transcript_hash`, `signature` lengths)
- **Submitter signature verified on `submitOutcome`** — the recovered address must match the caller's `player_id` (see [SECURITY_REVIEW.md](SECURITY_REVIEW.md) S1 and [docs/signing.mdx](docs/signing.mdx))
- **`replay_hash` must be exactly 32 bytes**; outcome range aligned to `1..=4` between server and relayer (S5)
- **Challenge map bounded at 100k entries** with TTL pruning to prevent memory-growth DoS (S9)
- **Per-IP rate limiting on both server and relayer** with a memory-bounded distinct-IP set (`AMP_MAX_PER_IP_PER_MIN`, `RELAYER_MAX_PER_IP`; S10)
- **Subscriber cap per match** — at most 16 `MatchListener` callbacks to prevent task-amplification DoS (P5)
- Rate limiting with RAII connection guards
- Settle-before-MMR ordering + sled schema versioning prevent crash-induced rating corruption; durability hardening (explicit `flush`, transactions) is scheduled with the sled→redb migration (Phase 3)

See [SECURITY_REVIEW.md](SECURITY_REVIEW.md) for the full threat model and remediation status, and [docs/signing.mdx](docs/signing.mdx) for the canonical EIP-191 / EIP-712 signing schemes.

---

## License

AMP is licensed under the **Apache License, Version 2.0**.

Permission is hereby granted, free of charge, to any person obtaining a copy of
this software and associated documentation files (the "Software"), to deal in
the Software for both personal and **commercial** use, subject to the conditions
of Apache-2.0 (attribution, notice of changes, and the trademark/patent terms).

See the [LICENSE](LICENSE) file for the full terms.
