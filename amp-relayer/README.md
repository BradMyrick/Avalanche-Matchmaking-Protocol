# AMP Relayer

The **AMP Relayer** is a pure Cap'n Proto RPC service that bridges the
off-chain AMP protocol to the Avalanche blockchain. It handles custodial
wallet derivation, gas auto-funding, and transaction submission for match
settlements.

## Features
- **Pure Cap'n Proto RPC**: High-performance internal API on port 50052.
- **Custodial Signers**: Derives unique signing keys for each `gameId` from a master relayer key.
- **Gas Auto-Funding**: Automatically tops up custodial wallets from a master fund if they fall below 0.05 AVAX.
- **On-Chain Settlement**: Submits match outcomes directly to the `AMPSettlement` contract.
- **API-Key Authentication**: Required by default; per-IP connection cap to mitigate DoS.

## Configuration
The Relayer is configured via environment variables (see `.env.example`):
- `RPC_PORT`: The port to listen for Cap'n Proto RPC connections (default: 50052).
- `FUJI_RPC_URL`: The Avalanche/Anvil RPC endpoint.
- `RELAYER_PRIVATE_KEY` / `RELAYER_PRIVATE_KEY_FILE`: The master key used for funding and derivation.
- `CONTRACT_REGISTRY`: Address of the `AMPRegistry` contract.
- `CONTRACT_SETTLEMENT`: Address of the `AMPSettlement` contract.
- `RELAYER_API_KEY` / `RELAYER_API_KEY_FILE` / `RELAYER_API_KEYS`: API key(s) for authenticating incoming RPC. **Required by default**.
- `AMP_ALLOW_UNAUTHENTICATED_RELAYER`: Set to `1` to bypass the API key requirement (**NOT RECOMMENDED**).
- `RELAYER_MAX_CONNECTIONS`: Max concurrent RPC connections (default: 50).
- `RELAYER_MAX_PER_IP`: Max concurrent connections per source IP (default: 10).
- `RELAYER_TLS_CERT_FILE` / `RELAYER_TLS_KEY_FILE`: When both are set, TLS is enforced.

## RPC Interface
The `RelayerService` (defined in `amp-sdk/schemas/relayer.capnp`) provides:
- `authenticate(apiKey)`: Authenticates the connection. Required before any other call when API keys are configured.
- `getGameAdmin(gameId)`: Resolves the on-chain admin for a game. Read-only, does not require auth.
- `getCustodialAddress(gameId)`: Returns the derived address for a game. Requires auth.
- `submitOutcome(matchId, outcome, transcriptHash, signature)`: Submits a settled match outcome. Validates `outcome ∈ 1..=4`, `transcriptHash.len() == 32`, `signature.len() == 65`. Requires auth.

## CLI Usage
The binary also supports a CLI mode for administrative queries:
```bash
# Query the custodial address for Game 0
./amp-relayer query-custodial 0

# Query the registered admin for Game 0
./amp-relayer query-admin 0
```

## Security
See [`SECURITY.md`](../SECURITY.md) for the responsible disclosure policy.
