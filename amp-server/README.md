# AMP Server

The **AMP Server** is the core coordination layer of the protocol, handling player authentication, matchmaking, and match session issuance. It is implemented as a high-performance **Cap'n Proto RPC** server.

## 🚀 Features
- **Pure Cap'n Proto RPC**: Binary protocol on port 50051 for sub-millisecond coordination.
- **Capability-Based Security**: Issues `UserSession` and `MatchSession` capabilities to authenticated clients.
- **On-Chain Admin Verification**: Authenticates game studios by querying the Relayer RPC.
- **Secure Outcome Signing**: Signs match results using the `VERIFIER_KEY` for on-chain settlement.
- **Telemetry Forwarding**: Securely forwards match telemetry to the `AMP Telemetry` service.

## 🏗 Architecture
- **Non-Blocking RPC**: Built on `tokio` and `capnp-rpc` for massive concurrency.
- **Matchmaking Queue**: Global FIFO queue for pairing players by `gameId`.
- **Relayer Integration**: Communicates with the `AMP Relayer` via RPC (port 50052) to verify game ownership.

## 🛠 Configuration
- `AMP_ADDR`: The address to listen for RPC connections (default: `0.0.0.0:50051`).
- `RELAYER_RPC_ADDR`: The address of the Relayer's RPC service (default: `localhost:50052`).
- `TELEMETRY_ADDR`: The address of the Telemetry service (default: `127.0.0.1:4317`).
- `VERIFIER_KEY`: The private key used by the server to sign match outcomes.

## 📖 RPC Interface
The server implements the `GameSessionService` (see `service.capnp`):
- `login(gameId, signedChallenge)`: Authenticates a game session.
- `requestMatch(req)`: Returns a `MatchAssignment` and a `MatchSession` capability.
- `submitOutcome(submission)`: Submits the final result for signing.
