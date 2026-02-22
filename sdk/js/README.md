# AMP JavaScript SDK - MVP Implementation

## Architecture overview
The JavaScript SDK is implemented in TypeScript using the `ethers` v6 provider library to integrate the Web3 RPC layer and a standard DOM-compatible `WebSocket` object to interface with the off-chain Matchmaker.

### Components
1. **`AMPClient`**: The primary entry point.
   - **`connectMatchmaker(playerAddress)`**: Opens a bi-directional persistent WebSocket to the Rust node mapping the wallet to an identity.
   - **`requestMatch(gameId)` / `submitOutcomeToMatchmaker(match_id, outcome, transcriptHash)`**: Event driven listeners that await responses from the queue or signature endpoint asynchronously.
   - **`createMatch` / `joinMatch` / `submitAsyncResult`**: Web3 integration translating domain-specific calls to ABI interactions with `AMPRegistry` and `AMPSettlement`.

## Security & Usage notes
- The Javascript SDK delegates exact Cap'n Proto operations to the compiled Rust binary `mm-client` via standard stdio, fully removing arbitrary JSON payloads over websockets.
- The Foundry deployer artifacts are hardcoded into static typed definitions in `AMPRegistryABI.ts` to ensure 0 dynamic JSON dependencies exist.
