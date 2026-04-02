# Avalanche Matchmaking Protocol (AMP)

![AMP Hero Banner](docs/images/hero_banner.png)

### Any Engine. Any Language. Trustless Settlement.

AMP is a decentralized, high-performance matchmaking protocol built on Avalanche. It provides game developers with enterprise-grade matchmaking capabilities—similar to AWS FlexMatch—but with the added security and transparency of on-chain verifiers and cryptographically settled outcomes.

---

## 🏆 Award Winning Technology

![AMP Grant Badge](docs/images/grant_badge.png)

AMP was awarded a **$15,000.00 Merit Grant** for placing in the **top 20 projects** of the **Avalanche Build Games 2026**. This recognition highlights our commitment to building the future of competitive gaming on the Avalanche C-Chain.

---

## Key Features

*   **⚡ Sub-Millisecond Latency**: Powered by Cap'n Proto RPC, ensuring that matchmaking doesn't slow down your game's real-time action.
*   **🛡️ Trustless Settlement**: Off-chain verifiers process deterministic match transcripts and commit outcome attestations directly to Avalanche smart contracts.
*   **📊 MMR & Glicko-2 Support**: Advanced player skill tracking built directly into the SDKs for fair and balanced matches.
*   **🌍 Region-Aware Matching**: Intelligent player grouping based on geographic latency to minimize lag.
*   **🔗 Unified Schema**: A single Cap'n Proto schema source generates native bindings for **Go, Rust, C++, C#, and Python**.

---

## Architecture Overview

AMP consists of three primary layers:

1.  **On-Chain (Solatify/Foundry)**: `AMPRegistry` and `AMPSettlement` contracts handle game registration and prize escrow/payouts.
2.  **Verifier Network (Rust)**: High-performance nodes that perform matchmaking and validate match transcripts.
3.  **Client SDKs**: Native libraries for Unity (C#), Unreal (C++), Go, and Python to easily integrate AMP into any game engine.

```
AMP/
├── amp-core/          # Core protocol logic and VM
├── amp-server/        # Matchmaker & Verifier implementation
├── amp-sdk/           # Multi-language SDKs & Schemas
├── amp-relayer/       # On-chain settlement bridge
└── amp-telemetry/     # Match state and performance monitoring
```

---

## Getting Started

Visit our [Full Documentation](https://docs.page/BradMyrick/Avax-Build-Games-2026) for comprehensive integration guides, API references, and architecture details.

### Native Integration

AMP provides dedicated SDKs to simplify the integration of complex matchmaking into your game project:

```bash
# Example: Adding the Go SDK
go get github.com/avalanche-matchmaking-protocol/amp-sdk/go
```

---

## License

AMP is licensed under the **AMP Non-Commercial Source License**.  
Personal and educational use is permitted for free. **Commercial use requires a separate license agreement.**

See the [LICENSE](LICENSE) file for the full terms or contact the repository owner for commercial inquiries.