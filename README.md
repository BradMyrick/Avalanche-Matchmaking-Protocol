# AMP - Avalanche Matchmaking Protocol

**AMP** is a capability-based matchmaking protocol and off-chain verifier network designed to secure high-frequency Web3 game state without sacrificing sub-millisecond latency.

By removing the reliance on centralized trust, AMP ensures fair play and irrefutable payouts for competitive Avalanche games. The protocol pairs players, verifies deterministic game transcripts off-chain, and commits settled outcomes directly to Avalanche.

---

## 📖 Documentation

For full technical documentation, architecture deep-dives, integration guides, and contract references, please visit our official documentation site:

### 👉 [https://docs.page/BradMyrick/Avax-Build-Games-2026](https://docs.page/BradMyrick/Avax-Build-Games-2026)

---

## 🚀 Quick Start (Local MVP)

You can run the full AMP MVP locally against Anvil to verify the entire flow end-to-end.

```bash
# 1. Dependency check & setup
./mvp_dependency_checker.sh
./mvp_setup.sh

# 2. Run the end-to-end local test
./test_mvp.sh
```

For detailed instructions on running the MVP and integrating the SDKs, see the [Getting Started](https://docs.page/BradMyrick/Avax-Build-Games-2026/) section in our documentation.

---

## 🔗 Project Links

*   **[Demo Website](./demo-website/README.md)**: Visualizing the match flow.
*   **[Trace Viewer](./trace-viewer/README.md)**: UI for parsing `amp-telemetry` JSON traces.
*   **[GitHub Repository](https://github.com/BradMyrick/Avax-Build-Games-2026)**: Source code and issue tracking.
