# AMP JavaScript SDK

A Node.js / TypeScript SDK for the Avalanche Matchmaking Protocol, built on a
**native Rust core** via [napi-rs](https://napi.rs). Full Cap'n Proto RPC
(connect, authenticate, match, submit outcome, events) runs inside the native
module on a dedicated worker thread; crypto (EIP-191 challenge signing +
EIP-712 outcome signing) is done in JS with `ethers` so games can substitute
their own signer.

## Status

**Feature-complete for the Developer Beta.** Provides:

- ✅ `AmpClient.connect()` / `close()` — Cap'n Proto RPC over TCP.
- ✅ `AmpClient.authenticate(gameId, privateKeyHex | signCallback)` — EIP-191
  challenge/response login.
- ✅ `AmpClient.requestMatch()` / `reconnect()` — matchmaking.
- ✅ `AmpClient.submitOutcome()` — EIP-712 submitter signature + verifier
  countersignature.
- ✅ `AmpClient.subscribeToEvents()` — `onMatchSettled` / `onOpponentDisconnected`.
- ✅ `AmpClient.emitGameEvent()` / `emitTelemetry()`.
- ✅ `computeOutcomeEip712Digest()` — canonical digest, byte-identical to the
  server and the Rust/C#/Python/Go SDKs (cross-language KAT enforced).

## Native module

The Rust core lives in [`native/`](./native) and is built with
`@napi-rs/cli`. It wraps the [`amp-sdk` Rust crate](../rust) and parks the
`!Send` Cap'n Proto client on a dedicated OS thread (`LocalSet`), bridging JS
calls via a channel + oneshot replies so `#[napi] async fn` futures stay
`Send`. Match events use a poll-based API (`startEvents` + `pollEvent`) that
the JS `subscribeToEvents` drains into a callback loop.

For browsers, the AMP server must be fronted by a TLS-terminating WebSocket
bridge — raw TCP is not available in browsers.

## Build

```bash
npm install                 # pulls @napi-rs/cli + ethers
npm run build               # builds the native .node (napi build) + tsc
npm test                    # runs the EIP-712 KAT + native digest tests
```

The native artifact is emitted as `native/amp-native.<platform>.node` (e.g.
`amp-native.linux-x64-gnu.node`). Cross-platform distribution follows the
standard napi-rs `optionalDependencies` pattern (`@napi-rs/cli` artifacts).

## Usage

```typescript
import { AmpClient } from "@avalanche-matchmaking-protocol/amp-sdk";

const client = new AmpClient({
  address: "matchmaker.example:50051",
  domain: { chainId: 43113, verifyingContract: "0x..." }, // for outcome signing
});
await client.connect();
await client.authenticate(0, process.env.PLAYER_KEY!); // EIP-191 via ethers

const { matchId } = await client.requestMatch("0");

await client.subscribeToEvents({
  onSettled: ({ victor, scores }) => console.log("settled", victor, scores),
  onOpponentDisconnected: () => console.log("opponent left"),
});

const transcriptHash = new Uint8Array(32); // keccak256 of your match transcript
const verifierSig = await client.submitOutcome(matchId, 1, transcriptHash);
console.log("verifier signature:", Buffer.from(verifierSig).toString("hex"));

await client.close();
```

## Cross-language digest verification

The EIP-712 digest over `(matchId, outcome, transcriptHash)` is byte-identical
across Rust, Go, C#, Python, and JavaScript. The shared known-answer test
vector (matchId="1", outcome=1, transcript_hash=zero[32], chain_id=43113,
verifying_contract=0x000...000) is:

```
2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c
```

## License

Apache-2.0
