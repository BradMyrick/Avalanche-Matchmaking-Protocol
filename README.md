# AMP Protocol - Alpha Release

The AMP Protocol is a high-performance, capability-based matchmaking and settlement layer for gaming on Avalanche.

## 🚀 Alpha Features
- **Pure Cap'n Proto RPC**: High-performance, low-latency internal communication.
- **Custodial Wallets**: Automatic signer derivation and gas funding for every game studio.
- **On-Chain Verification**: Real-time validation of game registry and verifier permissions.
- **Binary Telemetry**: Efficient binary logging of match events.

## 🏗 Architecture
AMP uses a capability-based security model. Once a client logs in, they receive a `UserSession` capability, which allows them to request matches. Upon match assignment, they receive a `MatchSession` which is the ONLY way to emit events or submit telemetry for that specific match.

- **AMP Server**: The matchmaker and game session coordinator.
- **AMP Relayer**: The bridge to Avalanche, handling custodial signers and gas auto-funding.
- **AMP Telemetry**: A binary logging service for high-throughput game analytics.

## 🛠 Setup & Verification
Run the following scripts to build and verify the entire stack against a local Anvil node:
1. `./mvp_setup.sh` - Installs dependencies and builds all components.
2. `./e2e_verify.sh` - Runs the full end-to-end verification suite.

## 📄 License
This protocol is licensed under the MIT License.

---

## 📖 Documentation

For full technical documentation, architecture deep-dives, integration guides, and contract references, please visit our official documentation site:

### 👉 [https://docs.page/BradMyrick/Avax-Build-Games-2026](https://docs.page/BradMyrick/Avax-Build-Games-2026)

---

## 🔗 Project Links

*   **[Demo Website](./demo-website/README.md)**: Visualizing the match flow.
*   **[Trace Viewer](./trace-viewer/README.md)**: UI for parsing `amp-telemetry` JSON traces.
*   **[GitHub Repository](https://github.com/BradMyrick/Avax-Build-Games-2026)**: Source code and issue tracking.
