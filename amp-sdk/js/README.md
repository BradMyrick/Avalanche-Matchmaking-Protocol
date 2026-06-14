# AMP JavaScript SDK

A Node.js / TypeScript SDK for the Avalanche Matchmaking Protocol.

## Status

**Work in progress.** This SDK currently provides:

- ✅ `computeOutcomeEip712Digest(matchId, outcome, transcriptHash, domain?)` —
  the canonical EIP-712 digest shared with the Rust/C#/Python SDKs.
  Cross-language KAT verified.
- ✅ `AmpClient.connect()` / `close()` — TCP and TLS transport via Node
  `net` / `tls`.
- ✅ `AmpClient.signOutcome()` — custodial wallet outcome signing.
- ⚠️ `AmpClient.authenticate()` / `requestMatch()` / `submitOutcome()` —
  stubbed (raise a clear error). Full Cap'n Proto RPC framing is in
  development; until then use the Go or Python SDK for end-to-end functionality.

For browser use, the AMP server must be fronted by a TLS-terminating
WebSocket bridge — raw TCP is not available in browsers.

## Install

```bash
npm install @avalanche-matchmaking-protocol/amp-sdk ethers
```

## Usage

```typescript
import { AmpClient, computeOutcomeEip712Digest } from "@avalanche-matchmaking-protocol/amp-sdk";

// Compute the canonical EIP-712 digest (works today, no RPC needed).
const digest = computeOutcomeEip712Digest(
  "42",
  1,
  new Uint8Array(32), // 32-byte keccak256 of the match transcript
);
console.log("digest:", Buffer.from(digest).toString("hex"));

// Connect over TLS.
const client = new AmpClient({
  address: "matchmaker.example:50051",
  tls: true,
});
await client.connect();
await client.close();
```

## Cross-language digest verification

The EIP-712 digest over `(matchId, outcome, transcriptHash)` is byte-identical
across Rust, C#, Python, and JavaScript. The shared known-answer test vector
(matchId="1", outcome=1, transcript_hash=zero[32], chain_id=43113,
verifying_contract=0x000...000) is:

```
2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c
```

Run the JS test with `npm test` after `npm run build`. The Rust, C#, and
Python SDKs each have an equivalent test.

## License

Apache-2.0
