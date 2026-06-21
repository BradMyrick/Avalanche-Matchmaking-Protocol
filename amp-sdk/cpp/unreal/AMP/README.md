# AMP Unreal Engine Plugin

Blueprint-callable wrapper around the AMP C++ SDK. Exposes connect, auth,
matchmaking, outcome submission, and event subscription to Unreal Blueprints,
with all Cap'n Proto RPC running on a dedicated AMP background thread (KJ's
event loop is thread-affine).

## Installation

1. Copy this `AMP/` directory into your project's `Plugins/` folder:
   `<Project>/Plugins/AMP/`.
2. Provide Cap'n Proto 1.3+ to the build. Two options:
   - **System capnp (Linux editor only):** no extra setup; `AMP.Build.cs`
     links `capnp-rpc`/`capnp`/`kj-async`/`kj` from the system.
   - **Bundled (recommended for Windows/Mac):** drop the capnp + kj headers
     and static libs into `<Project>/Source/ThirdParty/AMPCapnp/{include,lib}`
     and uncomment the matching block in
     `Source/AMP/AMP.Build.cs`.
3. Regenerate the capnp C++ bindings when schemas change:
   ```sh
   ./amp-sdk/generate_bindings.sh
   ```
   (the plugin compiles against `amp-sdk/schemas/generated/cpp/`).
4. Restart the editor and enable the **AMP SDK** plugin.

## TLS

Link **libkj-tls** alongside capnp/kj and define `AMP_HAS_KJ_TLS` (the header
is auto-detected via `__has_include(<kj/tls.h>)`). `ConnectTLS` then becomes
callable from Blueprint.

## Async API

The underlying `amp::AMPClient` offers `*_async` overloads returning
`kj::Promise<>`. Enable them by building the SDK with
`AMP_USE_COROUTINES=1` (requires a KJ built with coroutine support). The
Blueprint wrapper is non-blocking regardless (it uses a background thread).

## Blueprint flow

1. `Create AMPClientObject` → `Connect("host:port")`.
2. `Authenticate(GameId)` → `OnChallengeReceived`.
3. Sign the challenge bytes with your wallet plugin (EIP-191).
4. `LoginWithSignature(GameId, Signature, Challenge)` → `OnLoginResult`.
5. `RequestMatch(GameId)` → `OnMatchResult(MatchId, Quality)`.
6. `SubscribeToEvents()` → `OnMatchSettled` / `OnOpponentDisconnected`.
7. Sign the EIP-712 outcome digest over `(matchId, outcome, transcriptHash)`.
8. `SubmitOutcome(MatchId, Outcome, TranscriptHash, Signature)` →
   `OnOutcomeSubmitted(VerifierSignature)`.

See `amp-sdk/docs/signing.mdx` for the canonical digest construction and the
cross-language known-answer vector.
