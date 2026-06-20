# AMP Security Review & Remediation Status

This document tracks the security review performed on the AMP SDKs and
the server-side enforcement code, and the remediation status of each
finding. New contributors should read this before changing authentication,
signing, transport, or RPC-boundary validation code.

## Severity scale

- **CRITICAL** — Active exploit or foundational integrity gap. Must fix
  before any production deployment.
- **HIGH** — Practical attack vector against the default deployment or
  against a documented public API.
- **MEDIUM** — Hardening gap, footgun, or DoS vector that requires
  specific conditions to exploit.
- **LOW** — Hygiene, code-quality, or defense-in-depth issue.

## Findings & Status

### CRITICAL

| ID | Finding | Status | Resolution |
|:---|:---|:---|:---|
| **S1** | Server did not verify the submitter signature in `submit_outcome`. Any match participant could self-declare victory and get the verifier's countersignature. | **FIXED** | `amp-server/src/main.rs`: `MatchSessionImpl::submit_outcome` now reads `submission.get_signature()`, recovers the address via EIP-712 over `(matchId, outcome, transcriptHash)`, and requires it to match the caller's player_id. Test coverage in `test_verify_outcome_signature_round_trip`. |
| **S2** | C# and Python SDKs silently generated an ephemeral wallet key when no `privateKeyHex`/`signCallback` was passed. The key was never returned, persisted, or associated — defeating authentication. | **FIXED** | Both SDKs now require an explicit signer and raise a typed error otherwise. See `amp-sdk/csharp/AmpSdk/AmpClient.cs:AuthenticateAsync` and `amp-sdk/python/amp_sdk/client.py:authenticate`. |

### HIGH

| ID | Finding | Status | Resolution |
|:---|:---|:---|:---|
| **S3** | No TLS in any SDK by default. Signatures, capabilities, and the inter-service API key traversed plaintext TCP. | **PARTIAL** | Go SDK: `Options{TLS: ...}` constructor wires `crypto/tls`. Rust SDK: `dial_tls` constructor with `tokio-rustls`. Python SDK: `tls_context` constructor parameter (ssl.SSLContext). C# SDK: documented limitation of Capnp.Net.Runtime 1.3.x — production deployments should use a TLS-terminating reverse proxy (nginx/envoy). C++ SDK: TODO (libkj-tls is available locally but not yet wired). |
| **S4** | Game-admin authorization not enforced server-side. The schema comment claimed "for the game's admin address" but any Ethereum wallet could authenticate for any game_id. | **CLARIFIED** | Schema comments updated to reflect actual semantics (player authentication, not admin verification). Admin-level RPCs (`applyRestriction`, `recordMatchResult`) are intentionally NOT wired into the public capability. Per-game player allowlisting is left as a deployment-policy decision. |
| **S5** | `replay_hash` length not validated server-side; `sign_match_outcome` silently substituted `H256::zero()` for non-32-byte inputs, marking the match settled. The relayer then rejected the settlement, causing server-chain state desync. | **FIXED** | Server now requires `transcript_hash.len() == 32` up front. Outcome range aligned to `1..=4` between server and relayer. |
| **S6** | C++, C#, and Python SDKs never populated `OutcomeSubmission.signature`. Combined with S1, the integrity path was unenforced end-to-end. | **FIXED for C#/Python** | Both SDKs now compute the canonical EIP-712 digest and sign it with the player's wallet. Cross-language test vector in `test_outcome_digest_known_vector_cross_lang` ensures Rust/C#/Python produce identical digests. C++ SDK still pending (see TODO in `examples/cpp/src/main.cpp`). |
| **S7** | C++ example shipped a hardcoded 65-byte EIP-712 signature blob, used by both Player A and Player B. Either real (replayable) or fake (proving the server didn't verify). | **FIXED** | `examples/cpp/src/main.cpp` now refuses to run without `AMP_EXAMPLE_SIGNATURE_HEX` or a user-supplied `signChallenge` callback. The hardcoded blob is removed. |
| **S8** | `expiresAt` returned by `requestChallenge` was never validated client-side. | **FIXED for C#/Python/Go** | Each SDK now exposes a helper or check; see `CheckChallengeExpiry` (Go), `is_challenge_expired` (Rust), and inline checks in C#/Python `authenticate`. |

### MEDIUM

| ID | Finding | Status | Resolution |
|:---|:---|:---|:---|
| **S9** | Server's challenge map was unbounded. Anonymous pre-auth RPC could drive unbounded memory growth. | **FIXED** | `AuthService::create_challenge` now caps at `MAX_OUTSTANDING_CHALLENGES = 100_000`, pruning expired entries before refusing. |
| **S10** | Rate limiter trusts TCP peer address; per-IP `Vec<Instant>` unbounded; slowloris-susceptible. | **DOCUMENTED** | Existing limiter preserved for backward compatibility. PROXY-protocol support is a deployment concern — document in production checklist. |
| **S11** | Inter-service API key defaulted to empty. Server skipped auth; relayer short-circuited to "allow" when no keys configured. | **FIXED** | Both binaries now refuse to start without an API key unless `AMP_ALLOW_UNAUTHENTICATED_RELAYER=1` is set explicitly. |
| **S12** | C# transitive closure pulled 7-year-old `Portable.BouncyCastle 1.8.2` and `Newtonsoft.Json 11.0.2` (known advisories). | **FIXED** | Direct pins on `Portable.BouncyCastle 1.9.0` and `Newtonsoft.Json 13.0.3` in `AmpSdk.csproj`. |
| **S13** | Dead admin RPC surface in `player_service.rs` (`apply_restriction`, `record_match_result`, `create_or_update_profile`) — no auth. Any future change exposing the capability would enable unauthenticated tampering. | **MITIGATED** | These methods are intentionally not wired into `serve_rpc`. They're flagged with `#[allow(dead_code)]`. A future admin capability should require operator-issued credentials. |

### LOW

| ID | Finding | Status | Resolution |
|:---|:---|:---|:---|
| **S14** | Go SDK returned a `MatchID` slice backed by the released capnp answer buffer (use-after-free risk). | **FIXED** | `RequestMatch` and `Reconnect` now defensively copy `matchID` into an owned `[]byte`. |
| **S15** | Login signature verification used `sig_bytes[..65]` and ignored trailing bytes; no canonical-s check. | **DOCUMENTED** | ethers' `Signature::try_from` accepts the canonical form; trailing bytes are now length-checked up front (must be exactly 65). Strict canonical-s enforcement is left to the ethers library. |

## Smart Contract Findings (Phase 1 hardening)

The original `SECURITY_REVIEW.md` covered only the SDK and Rust server. The
investor due-diligence audit found the Solidity contracts had **never been
reviewed** here. Phase 1 closes the HIGH-severity custody bug and adds the
hardening below; a third-party audit is scoped for Phase 6.5.

| ID | Finding | Severity | Status | Resolution / Test |
|:---|:---|:---|:---|:---|
| **C1** | `_payout` silently trapped ~99% of the pool when `outcome == NONE` (no if-branch → 0 credits, match marked SETTLED). | **HIGH** | **FIXED** | `AMPSettlement._payout` reverts `InvalidOutcome` on NONE and any unhandled enum value. Tests: `testSubmitAsyncRevertsOnNoneOutcome`, `testResolveDisputeRevertsOnNoneOutcome`, `testFuzz_SettlementConservesValue`. |
| **C2** | `joinMatch` reverted for fee-on-transfer tokens (`received != stakeAmount`). | MEDIUM | **FIXED** | Per-player actual-stake tracking (`stakeAmountB`); balance-delta measurement in `joinMatch`. Test: `testFeeOnTransferTokenSettlesAndRefundsActual`. |
| **C3** | Hand-rolled reentrancy guard; `AMPSettlement` had none. | MEDIUM | **FIXED** | OpenZeppelin `ReentrancyGuard` on both contracts. |
| **C4** | Owner economic controls (fee bps, fee recipient) had no timelock — a compromised key could extract in the same block. | MEDIUM | **FIXED** | Governance via `TimelockController` documented + tested. Tests: `AMPGovernance.t.sol` (3 tests). |
| **C5** | Hand-rolled EIP-712 domain separator bypassed audited OZ `EIP712`. | LOW | **FIXED** | Switched to OpenZeppelin `EIP712`; Solidity-side KAT pins the cross-language digest vector. Test: `testEIP712DigestMatchesCrossLangVector`. |
| **C6** | No value-conservation invariant / fuzz coverage. | MEDIUM | **FIXED** | `testFuzz_SettlementConservesValue` (outcome+stake fuzz) + `invariant_registryNativeBacksAllCredits` (256-run handler invariant). |
| **C7** | No Slither config, no coverage profile. | LOW | **FIXED** | `contracts/slither.config.json` + `[profile.coverage]`; CI gate lands in Phase 5. |

### Trust model (unchanged by Phase 1 — operational, documented)
- A single whitelisted verifier key per game can settle any match in that game;
  key hygiene is enforced off-chain. Phase 6 + Phase 7 add multisig/timelock
  ownership and a third-party audit.
- The deployer of `AMPRegistry`/`AMPSettlement` is the initial owner and fee
  recipient; the Phase 7 deploy runbook transfers ownership to a
  `TimelockController`-gated multisig before mainnet.

## Performance fixes

### Phase 6 — hardening

| ID | Finding | Severity | Status | Resolution / Test |
|:---|:---|:---|:---|:---|
| **S10** | Server per-IP rate-limiter `HashMap<IpAddr, Vec<Instant>>` was unbounded; a spoofed-source-IP flood grew memory without limit, and the `retain` ran on the accept thread. | MEDIUM | **FIXED** | `ConnectionRateLimiter::check_ip` now runs a `sweep_windows` pass (drop expired windows + empty entries) when the distinct-IP set exceeds `MAX_TRACKED_IPS = 100_000` or on a `SWEEP_EVERY = 2048` cadence. Tests: `sweep_removes_expired_and_empty_windows`, `check_ip_caps_per_ip_and_accepts_until_cap`. |
| **S15** | Relayer API-key check used `HashSet::contains`, whose timing varies with hash buckets. | LOW | **FIXED** | `config::verify_api_key` SHA-256-hashes the candidate and compares against every stored hash with a constant-time `ct_eq` (no short-circuit). Tests: `ct_eq_matches_and_rejects`, `verify_api_key_round_trip_and_reject`. |
| **C1** | Custodial key derivation is hand-rolled HKDF-HMAC-keccak256 with no published reference vectors. | LOW | **MITIGATED** | keccak256 is not a standard HKDF hash so no vetted crate applies directly; a pinned known-answer vector (`test_custodial_derivation_known_vector`, derived address `0x70d8a…736a` for the canonical inputs) now catches any drift. A future migration to BIP-32-style secp256k1 derivation is tracked. |

### Phase 6 — operational hardening (deployment guidance)

- **PROXY-protocol (S10 deployment gap):** when the server/relayer sit behind a
  load balancer, `peer.ip()` resolves to the LB, not the client, defeating per-IP
  rate limiting. Operators MUST enable PROXY-protocol v2 header preservation at
  the LB and terminate it at an edge that forwards the real client IP (e.g. via
  `PROXY-protocol`-aware ingress or a sidecar that rewrites the source). Native
  PROXY-protocol parsing in the AMP accept loop is tracked as future work.
- **Third-party audit (Phase 6.5):** an independent Solidity + Rust audit
  (Spearbit / Ackee / Trail of Bits) is scoped before the C-Chain mainnet tag
  (Phase 10). The Phase 1 self-review + fuzz/invariant suite is the pre-audit
  baseline; the audit report will be committed and findings remediated before
  `v1.0.0`.

## Performance fixes

| ID | Finding | Status |
|:---|:---|:---|
| **P4** | `sign_match_outcome` ran BEFORE the idempotency check. Repeat calls on settled matches burned CPU on each retry. | **FIXED** — `submit_outcome` now marks settled FIRST, then signs. |
| **P5** | `subscribe_to_events` had no subscriber cap; a malicious participant could register unbounded callbacks. | **FIXED** — Capped at `MAX_SUBSCRIBERS_PER_MATCH = 16` via `AppState::subscriber_count`. |
| **P8** | Go Glicko-2 solver has no iteration cap. | **OPEN** — See TODO in `amp-sdk/go/player/profile.go`. |
| **P9** | Rust SDK shipped 98,531 lines of committed-but-unused `*_capnp.rs`. | **FIXED** — Files deleted; `build.rs` regenerates into `OUT_DIR` only. |

## Cross-SDK integrity

The EIP-712 digest over `(matchId, outcome, transcriptHash)` is now
byte-identical across Rust, C#, and Python SDKs. The known-answer vector
in `amp-server/src/main.rs::test_outcome_digest_known_vector_cross_lang`
fails loudly if any SDK diverges.

## Open work

The Developer Beta closes the original open-work list. Status as of the Beta:

1. C++ SDK: TLS via `libkj-tls` (guarded by `__has_include`), async coroutine
   API (guarded by `AMP_USE_COROUTINES`), and `submit_outcome` callback
   signing — **DONE**.
2. C++ SDK: Unreal Engine plugin descriptor + Blueprint wrappers — **DONE**
   (`amp-sdk/cpp/unreal/`).
3. JS SDK: native TypeScript/Node.js implementation via **napi-rs** wrapping
   the Rust SDK — **DONE** (full RPC: connect, auth, match, submitOutcome,
   events). Was a stub; now feature-complete.
4. All SDKs: `subscribeToEvents` / `MatchListener` — **DONE** in Rust, Go,
   C++, C#, Python, and JS.
5. All SDKs: add comprehensive tests (only Go has any today).

   * Go has Glicko-2 + EIP-712 KAT tests; Rust has the digest KAT + a
     disconnect-cleanup test; JS has the cross-language digest KAT +
     native-digest tests; Python has the digest KAT + helper tests.
     C++ and C# still lack unit tests (tracked).
6. Server: per-IP rate limit on the relayer (added per-IP cap in this pass;
   PROXY-protocol support still open).

## Configuration changes

Operators upgrading to this version should:

1. Set `RELAYER_API_KEY` (or `RELAYER_API_KEY_FILE`) on **both** the
   server and relayer. Previous default of "no auth" is now a hard error.
2. Update C# clients to pass `privateKeyHex` or `signCallback` to
   `AuthenticateAsync`. Silent ephemeral keys are no longer generated.
3. Update Python clients similarly: `authenticate()` now requires an
   explicit signer.
4. Update C++ clients to set `AMP_EXAMPLE_SIGNATURE_HEX` (for local
   testing) or implement a real `signChallenge` wallet callback.
5. Update all clients to compute and submit the EIP-712 outcome signature.
   The server now rejects submissions without a valid 65-byte signature.

## See also

- `amp-sdk/schemas/match.capnp` — schema-level signature conventions
- `amp-sdk/schemas/service.capnp` — login/authorization semantics
- `amp-server/src/auth.rs` — challenge/response implementation
- `amp-server/src/main.rs::compute_outcome_eip712_digest` — canonical
  digest reference
