# AMP Matchmaker & Verifier - MVP Implementation

## Architecture overview
The matchmaker service is implemented in Rust as a high-performance **Cap'n Proto RPC** server over WebSocket. It serves raw binary capability methods directly matching the `.capnp` definitions.

### 1. Matchmaking Lifecycle
- **WebSocket Gateway**: Uses `tokio-tungstenite` to handle concurrent player connections.
- **Identity**: Players "login" by sending their wallet address.
- **Matchmaking Queue**: A global FIFO queue implementation (`MATCH_QUEUE`) using `Arc<Mutex<Vec<QueueEntry>>>`.
- **Match Assignment**: When two players enter the queue for the same `gameId`, the server generates a unique `match_id` (UUID format) and sends a `MatchAssignment` message to both.

### 2. Outcome Verification (`src/main.rs`)
- **`verify_and_sign`**: The primary verifier logic. It takes a `match_id`, an `outcome`, and a `transcript_hash`.
- **ECDSA Signing**: Using `ethers-core`, the server signs the match data according to EIP-191. This signature is required by the `AMPSettlement` contract to authorize payouts.
- **Payload Encoding**: Values are ABI-encoded identically to the Solidity contract's expectation to ensure the `keccak256` digest matches on-chain.

## Security Decisions Taken
- **Segregated Verifier Keys**: The matchmaker holds the `VERIFIER_KEY` in environment variables. This key is the "Source of Truth" registered in the `AMPRegistry`.
- **Deterministic Outlines**: Verification is restricted to authorized match identifiers to prevent signature replay across different match instances.
- **Asynchronous Safety**: Uses `tokio` tasks for each connection to ensure one hanging client doesn't block the matchmaking throughput of others.

## Running Locally against Anvil
To run the server locally for the MVP flow, you'll need Anvil contracts deployed first.

### Run Command
From inside the `amp-server/` directory:
```bash
export VERIFIER_KEY="0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef"
cargo run
```

### Happy Path MVP Demo
1. Start `anvil` and deploy `contracts/`.
2. Start `amp-server` as shown above.
3. Start the `amp-telemetry` receiver in a separate terminal.
4. Run either the C++ or C# SDK example, which will connect to the local server, request a match, simulate gameplay, emit telemetry, and submit its outcome to the server.
5. Watch the `amp-server` terminal issue a signature via the ECDSA verification logic.
