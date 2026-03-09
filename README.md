# AMP - Avalanche Matchmaking Protocol

AMP is a capability-based matchmaking protocol and off-chain verifier network designed to secure high-frequency Web3 game state without sacrificing sub-millisecond latency.

By removing the reliance on centralized trust and standard, spoof-able shared tokens, AMP ensures fair play and irrefutable payouts for competitive Avalanche games. The protocol pairs players, verifies deterministic game transcripts off-chain, and commits settled outcomes directly to the Avalanche L1 (via local Anvil for this MVP). detailed documentation can be found throughout the modules.

---

## MVP Quickstart (Local Anvil)

The MVP is configured to deploy and test locally using **Anvil** instead of the live Fuji testnet. This allows judges to verify the entire flow independently.

### 1. Prerequisites 
Ensure you have ran the setup script to install OpenZeppelin contracts and build all SDK/Server binaries:
```bash
./mvp_dependency_checker.sh
./mvp_setup.sh
```

### 2. Run the Full Suite
You can verify the entire protocol automatically using:
```bash
./test_mvp.sh
```
This script will:
1. Start Anvil.
2. Deploy the `AMPRegistry` and `AMPSettlement` contracts.
3. Start the Rust `amp-telemetry` receiver.
4. Start the Rust `amp-server` (matchmaker & verifier).
5. Run the C++ SDK simulation (2 simulated players).
6. Run the C# SDK simulation.

---

## Project Structure & Links

*   **[Demo Website](./demo-website/README.md)**: A one-page "investable" front-end to visualize the flow.
*   **[Trace Viewer](./trace-viewer/README.md)**: A local UI to parse the `amp-telemetry` JSON traces.
*   **[Smart Contracts](./contracts/README.md)**: Solidity escrow and ECDSA verification routines.
*   **[Matchmaker & Verifier Server](./amp-server/README.md)**: Capability-based Rust backend.
*   **[SDK Usage Guide](./amp-sdk/docs/SDK_USAGE.md)**: Unity C# and Native C++ integration steps.

---

## Architecture Overview

1.  **Client Authentication**: Game clients cryptographically sign a payload to login to the AMP `GameSessionService` and receive a `UserSession` capability.
2.  **Match Assignment**: Calling `requestMatch` issues a dynamically-generated `MatchSession` capability specific to that pairing.
3.  **Secure Telemetry**: Only a client holding the `MatchSession` capability can emit events or submit outcomes for that specific match ID, guaranteeing isolation.
4.  **Off-Chain Verification**: The AMP node consumes game transcripts, simulates deterministic rollback, and cryptographically signs a valid outcome using the `VERIFIER_KEY`.
5.  **On-Chain Settlement**: The smart contract (`AMPSettlement`) verifies the payload's `ecrecover` signature against the authorized verifier, releasing escrowed game funds on the Ledger.


## Todo's
[] Add a test for non--native token withdrawals (e.g., USDC) to ensure the logic works across ERC20s.
[] Forwarder currently address(0) - ERC2771 is “wired but off,” and that registry/settlement rely on _msgSender() always



