# AMP SDK Usage Guide

Welcome to the AMP SDK! This repository contains everything you need to
integrate your game engine (C++, C#, Go, Rust, Python, or JavaScript) with
the AMP capability-based matchmaker and verifier network.

## Core Concepts

AMP utilizes **Cap'n Proto RPC** to provide an extremely fast, capability-based
security model. Instead of relying on global tokens or shared secrets embedded
in your game client, your game requests specific **Capabilities** (like a
`MatchSession`) from the matchmaker.

This model ensures that untrusted or malicious clients cannot spoof telemetry
or game events because they do not physically possess the required capability
handle to invoke those RPC methods.

## Authentication

Players authenticate via an EIP-191 challenge/response flow:

1. **`requestChallenge(gameId)`** — server returns `(challenge, expiresAt)`.
2. **Sign the challenge** — the player's wallet signs the raw EIP-191 message
   hash. SDKs that ship custodial signing do this automatically when given a
   private key; wallet-integrated SDKs accept a callback.
3. **`login(gameId, signature, challengePayload)`** — server recovers the
   signer address and binds it to the resulting `UserSession` capability.
   That address is later used to verify the submitter signature on
   `submitOutcome`.

> **Client-side freshness:** the SDKs check `expiresAt` before signing so the
> one-time challenge isn't burned on a stale attempt. The server also enforces
> this — challenges are single-use with a 5-minute TTL.

See [`signing.mdx`](../../docs/signing.mdx) for the canonical EIP-191 auth
scheme and EIP-712 outcome signature details.

## Champion Flows

### 1. Match Create & Join
The entry point to the AMP network is the `GameSessionService`.

- **Login**: Provide a cryptographic signature proving your identity. The
  service returns a `UserSession` capability.
- **Request Match**: Using your `UserSession`, call `requestMatch(gameId)`.
  The RPC call will return an authoritative `MatchAssignment` and a direct
  `MatchSession` capability specific to that match.

### 2. In-Game Telemetry & Events
Once the match starts, your game engine should utilize the `MatchSession`
capability to emit verified events.

- **Game Events**: Call `emitGameEvent` for critical state changes (e.g.
  scoring, item pickup).
- **Telemetry**: Call `emitTelemetry` using the `AmpTelemetryEvent` schema to
  denote match lifecycle events like `matchJoined`, `settlementSubmitted`,
  etc. The capability-based design guarantees that these events are tied only
  to your specific assigned match.

### 3. Commit & Settle
When the match concludes, the verifier network must confirm the result.

- **Sign the outcome**: Compute the canonical EIP-712 digest over
  `(matchId, outcome, transcriptHash)` and sign it with the SAME wallet used
  at login. SDKs that ship custodial signing do this automatically.
- **Submit**: Using the `MatchSession` capability, call `submitOutcome()`,
  passing the 32-byte transcript hash and the 65-byte submitter signature.
- **Verify**: The server recovers the address from your signature and
  requires it to match the authenticated player. The server then applies its
  own EIP-712 verifier signature.
- **On-Chain Settlement**: The relayer submits the verifier's signature to
  the `AMPSettlement` contract on the Avalanche L1.

> **Critical:** Submissions without a valid 65-byte submitter signature are
> rejected. The transcript hash must be exactly 32 bytes (keccak256 of the
> match transcript). The outcome value must be 1..=4 (victor index).

## SDK-Specific Notes

### Go
The reference implementation. Supports TLS via `Options.TLS`. See
[`amp-sdk/go/client/client.go`](../go/client/client.go).

### Rust
Requires `run_in_localset(...)` wrapper because capnp-rpc is `!Send`. TLS
available with the `tls` cargo feature. See
[`amp-sdk/rust/src/lib.rs`](../rust/src/lib.rs).

### C# / Unity
ConfigureAwait(false) on all awaits; CancellationToken accepted on all
methods. Capnp.Net.Runtime 1.3.x has no native TLS — use a TLS-terminating
reverse proxy in production. See
[`amp-sdk/csharp/AmpSdk/AmpClient.cs`](../csharp/AmpSdk/AmpClient.cs).

### C++
Thread-affine (KJ event loop). Outcome signing delegated to a user-supplied
callback because the SDK intentionally does not bundle a vetted Keccak-256
implementation. See [`amp-sdk/cpp/include/amp/client.hpp`](../cpp/include/amp/client.hpp).

### Python
Async-first (asyncio). Custodial and callback signers supported. TLS via
standard `ssl.SSLContext`. See
[`amp-sdk/python/amp_sdk/client.py`](../python/amp_sdk/client.py).

### JavaScript / TypeScript
Node.js only (raw TCP). Provides `computeOutcomeEip712Digest()` verified by
the cross-language KAT. Browser support requires a WebSocket bridge.
See [`amp-sdk/js/src/index.ts`](../js/src/index.ts).

## Examples

Per-language end-to-end examples live in [`amp-sdk/examples/`](../examples/):

- **C++**: `examples/cpp/src/main.cpp`
- **C#**: `examples/csharp/Program.cs`
- **Python**: `examples/python/example.py`

Each example compiles cleanly and runs the full loop
(Match Create → Simulate → Submit Outcome).
