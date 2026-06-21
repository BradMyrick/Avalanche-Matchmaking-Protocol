# AMP Server

The **AMP Server** is the core coordination layer of the protocol, handling
player authentication, matchmaking, and match session issuance. It is
implemented as a high-performance **Cap'n Proto RPC** server.

## Features
- **Pure Cap'n Proto RPC**: Binary protocol on port 50051 for sub-millisecond coordination.
- **Capability-Based Security**: Issues `UserSession` and `MatchSession` capabilities to authenticated clients.
- **EIP-191 Challenge/Response Auth**: Single-use nonce, 5-minute TTL, recovered signer address bound to the session.
- **Submitter Signature Verification**: `submitOutcome` rejects submissions whose EIP-712 signature does not recover to the caller's player_id (see `docs/signing.mdx`).
- **Secure Outcome Signing**: Signs match results with the `VERIFIER_KEY` using EIP-712 for on-chain settlement.
- **Bounded DoS Surfaces**: Per-IP rate limiting, 1000 concurrent connection cap, 100k-challenge map with TTL pruning, 16-subscriber-per-match cap.
- **Telemetry Forwarding**: Securely forwards match telemetry to the `AMP Telemetry` service.

## Architecture
- **Non-Blocking RPC**: Built on `tokio` and `capnp-rpc` for massive concurrency.
- **Thread-per-Core**: `SO_REUSEPORT` workers each drive an independent `LocalSet`.
- **Matchmaking Queue**: Global FIFO queue for pairing players by `gameId`.
- **Relayer Integration**: Communicates with the `AMP Relayer` via RPC (port 50052). The relayer API key is **required by default** (see `AMP_ALLOW_UNAUTHENTICATED_RELAYER`).

## Configuration
- `AMP_ADDR`: The address to listen for RPC connections (default: `0.0.0.0:50051`).
- `RELAYER_RPC_ADDR`: The address of the Relayer's RPC service (default: `localhost:50052`).
- `TELEMETRY_ADDR`: The address of the Telemetry service (default: `127.0.0.1:9317`).
- `VERIFIER_KEY` / `VERIFIER_KEY_FILE`: Private key used by the server to sign match outcomes.
- `RELAYER_API_KEY` / `RELAYER_API_KEY_FILE`: API key for relayer authentication (**required by default**).
- `AMP_ALLOW_UNAUTHENTICATED_RELAYER`: Set to `1` to bypass the API key requirement (**NOT RECOMMENDED**).
- `AMP_TLS_CERT_FILE` / `AMP_TLS_KEY_FILE`: When both are set, TLS is enforced (no silent fallback).
- `AMP_MAX_CONCURRENT`: Max simultaneous connections (default: 1000).
- `AMP_MAX_PER_IP_PER_MIN`: Per-IP rate limit (default: 60).
- `AMP_EIP712_CHAIN_ID`: EIP-712 chain ID for the outcome signature domain (default: 43113 / Fuji).
- `AMP_SETTLEMENT_ADDRESS`: EIP-712 verifying contract for the outcome signature domain.

See `.env.example` for the complete list.

## RPC Interface
The server implements `GameSessionService` (see `amp-sdk/schemas/service.capnp`):
- `requestChallenge(gameId)`: Returns the EIP-191 challenge bytes and an absolute expiry timestamp (nanoseconds).
- `login(gameId, signature, challengePayload)`: Verifies the signature, recovers the player address, returns a `UserSession` capability.

`UserSession`:
- `requestMatch(req)`: Returns a `MatchAssignment` and a `MatchSession` capability.
- `reconnect(matchId)`: Reclaims a capability for an existing active match.

`MatchSession`:
- `submitOutcome(submission)`: Verifies the submitter signature, marks the match settled, applies the verifier's EIP-712 signature, returns it for on-chain settlement. Rejects submissions without a valid 65-byte signature or 32-byte transcript hash.
- `subscribeToEvents(subscriber)`: Registers a `MatchListener` for match-settled notifications (capped at 16 per match).
- `emitGameEvent(event)`: Fire-and-forget gameplay event.
- `emitTelemetry(event)`: Forward to the telemetry service.

## Security
See [`docs/signing.mdx`](../docs/signing.mdx) for the canonical signing schemes and [`SECURITY.md`](../SECURITY.md) for the responsible disclosure policy.
