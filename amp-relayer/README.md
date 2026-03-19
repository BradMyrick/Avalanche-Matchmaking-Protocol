# AMP Relayer

The **AMP Relayer** is a pure Cap'n Proto RPC service that bridges the off-chain AMP protocol to the Avalanche blockchain. It handles custodial wallet derivation, gas auto-funding, and transaction submission for match settlements.

## 🚀 Features
- **Pure Cap'n Proto RPC**: High-performance internal API on port 50052.
- **Custodial Signers**: Derives unique signing keys for each `gameId` from a master relayer key.
- **Gas Auto-Funding**: Automatically tops up custodial wallets from a master fund if they fall below 0.05 AVAX.
- **On-Chain Settlement**: Submits match outcomes directly to the `AMPSettlement` contract.

## 🛠 Configuration
The Relayer is configured via environment variables:
- `RPC_PORT`: The port to listen for Cap'n Proto RPC connections (default: 50052).
- `FUJI_RPC_URL`: The Avalanche/Anvil RPC endpoint.
- `RELAYER_PRIVATE_KEY`: The master key used for funding and derivation.
- `CONTRACT_REGISTRY`: Address of the `AMPRegistry` contract.
- `CONTRACT_SETTLEMENT`: Address of the `AMPSettlement` contract.

## 📖 RPC Interface
The `RelayerService` (defined in `relayer.capnp`) provides:
- `getGameAdmin(gameId)`: Resolves the on-chain admin for a game.
- `getCustodialAddress(gameId)`: Returns the derived address for a game.
- `submitOutcome(matchId, outcome, transcriptHash, signature)`: Submits a settled match outcome.

## 🖥 CLI Usage
The binary also supports a CLI mode for administrative queries:
```bash
# Query the custodial address for Game 0
./amp-relayer query-custodial 0

# Query the registered admin for Game 0
./amp-relayer query-admin 0
```
