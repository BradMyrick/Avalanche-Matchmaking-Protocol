# Avalanche Matchmaking Protocol (AMP)

### Any Engine. Any Language. Trustless Settlement.

AMP is a decentralized, high-performance matchmaking protocol built on Avalanche. It provides game developers with enterprise-grade matchmaking capabilities—similar to AWS FlexMatch—but with the added security and transparency of on-chain verifiers and cryptographically settled outcomes.

---

## Award Winning Technology

AMP was awarded a **$15,000.00 Merit Grant** for placing in the **top 20 projects** of the **Avalanche Build Games 2026**. This recognition highlights our commitment to building the future of competitive gaming on the Avalanche C-Chain.

---

## Key Features

*   **Sub-Millisecond Latency**: Powered by Cap'n Proto RPC with zero-copy serialization.
*   **Trustless Settlement**: Off-chain verifiers process deterministic match transcripts and commit outcome attestations directly to Avalanche smart contracts.
*   **Challenge-Response Auth**: Server-issued nonces signed by player wallets prevent replay attacks.
*   **MMR & Glicko-2 Support**: Advanced player skill tracking with binary search matchmaking (50ms tick).
*   **Region-Aware Matching**: Intelligent player grouping based on geographic latency to minimize lag.
*   **Dual Settlement Modes**: `RT_HASH_AGREE` for instant hash comparison; `ASYNC_VERIFIER` for full server-side replay.
*   **Reliable Settlement Bridge**: Sled-backed persistent queue with EIP-1559 gas, nonce tracking, retry with exponential backoff, and dead-letter queue.
*   **Unified Schema**: A single Cap'n Proto schema source generates native bindings for **Go, Rust, C++, C#, and Python**.

---

## Architecture Overview

AMP consists of four primary layers:

1.  **On-Chain (Solidity/Foundry)**: `AMPRegistry` and `AMPSettlement` contracts handle game registration, match lifecycle, escrow, attestation verification, and payouts.
2.  **Verifier Network (Rust)**: High-performance `amp-server` that performs matchmaking (`IndexedQueue` with MMR-sorted buckets), challenge-response auth, and match outcome attestation.
3.  **Settlement Relayer (Rust)**: `amp-relayer` bridges off-chain outcomes to on-chain settlement with persistent queue, gas management, and retry logic.
4.  **Client SDKs**: Native libraries for Unity (C#), Unreal (C++), Go, Python, and Rust.

```
AMP/
├── amp-server/        # Matchmaker & Verifier implementation (Rust, Cap'n Proto RPC)
├── amp-relayer/       # On-chain settlement bridge (Rust, sled-backed queue)
├── amp-sdk/           # Multi-language SDKs & Cap'n Proto schemas
│   ├── cpp/           # C++ SDK (Unreal)
│   ├── csharp/        # C# SDK (Unity/Godot)
│   ├── go/            # Go SDK (server-side)
│   ├── python/        # Python SDK (AI/research)
│   ├── rust/          # Rust SDK
│   └── schemas/       # Cap'n Proto schema definitions
├── amp-loadtest/      # Load testing tool
├── contracts/         # Solidity contracts (Forge, ^0.8.33)
│   ├── src/           # AMPRegistry, AMPSettlement, AMPTypes
│   └── test/          # 31 Forge tests
└── scripts/           # start-localnet.sh and utility scripts
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

### Local Development

```bash
# Start local Avalanche fork with AMP contracts deployed
./scripts/start-localnet.sh

# Build all Rust components
cargo build --release

# Run the verifier
AMP_LISTEN_ADDR=0.0.0.0:5555 \
AMP_REGISTRY_ADDR=0x... \
AMP_VERIFIER_KEY=0x... \
./target/release/amp-server

# Run the relayer
AMP_RELAYER_REGISTRY=0x... \
AMP_RELAYER_KEY=0x... \
./target/release/amp-relayer
```

### With Docker

```bash
docker compose up --build
```

### SDK Integration

```bash
# Go
go get github.com/avalanche-matchmaking-protocol/amp-sdk/go

# Rust (Cargo.toml)
amp-sdk = { path = "../amp-sdk/rust" }
```

See the [Full Documentation](https://docs.page/BradMyrick/Avax-Build-Games-2026) for comprehensive integration guides, API references, and architecture details.

---

## Test Status

| Suite | Count | Command |
| :--- | :--- | :--- |
| Rust tests | 22 | `cargo test --workspace` |
| Forge tests | 31 | `cd contracts && forge test -vvv` |
| Clippy | clean | `cargo clippy --workspace -- -D warnings` |
| Formatting | clean | `cargo fmt --check` |

---

## Roadmap

### Completed (Q1 2026)
- [x] Core `amp-server` with Cap'n Proto RPC and `IndexedQueue` matchmaker
- [x] `AMPRegistry` + `AMPSettlement` contracts with full match lifecycle
- [x] Challenge-response authentication
- [x] `amp-relayer` with sled-backed queue, EIP-1559, retry, dead-letter
- [x] 5 native SDKs (C++, C#, Go, Python, Rust)
- [x] Docker + CI/CD pipeline
- [x] Load testing tool

### In Progress (Q2 2026)
- [ ] OpenTelemetry + Prometheus metrics
- [ ] Invariant/fuzz contract tests
- [ ] 72-hour soak test
- [ ] napi-rs Node.js/TypeScript SDK
- [ ] Per-language example projects

### Planned (Q3--Q4 2026)
- [ ] Mainnet launch on Avalanche C-Chain
- [ ] Verifier Whitelist DAO
- [ ] ERC-20 staking (USDC, JOE, etc.)
- [ ] Dedicated Avalanche Subnet for high-frequency settlement
- [ ] Fraud Proof / Challenge mechanism

---

## License

AMP is licensed under the **AMP Non-Commercial Source License**.
Personal and educational use is permitted for free. **Commercial use requires a separate license agreement.**

See the [LICENSE](LICENSE) file for the full terms or contact the repository owner for commercial inquiries.
