# Changelog

All notable changes to the Avalanche Matchmaking Protocol (AMP) project are
documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

A running summary of the production-readiness path lives in [`prodpath.md`](prodpath.md).

## [Unreleased]

### Changed — Phase 0 (branch & release hygiene)
- Made `main` the sole release line; all active development tracks `prodpath` toward the Fuji validation gate.
- Introduced a **single source of truth** for package metadata via `[workspace.package]` in the root `Cargo.toml`; every workspace member now inherits `version`, `edition`, and `license` (`*.workspace = true`).
- **Unified the version to `0.1.0`** across all components (server crates were `0.0.1`; SDKs were `0.1.0`–`0.2.0`). No version drift remains.
- **Relicensed the entire project to Apache-2.0.** The previous "AMP Non-Commercial Source License" and the contradictory per-SDK Apache-2.0 declarations are replaced by a single permissive Apache-2.0 grant covering server, contracts, and all SDKs. Commercial use is now permitted.

### Added — Phase 0
- [`CODEOWNERS`](CODEOWNERS) for review routing.
- [`CONTRIBUTING.md`](CONTRIBUTING.md) contributor guide.
- GitHub PR template and issue templates (`.github/`).
- This `CHANGELOG.md`.

### Removed — Phase 0
- The standalone "AMP Non-Commercial Source License" text (replaced by Apache-2.0).

## [Unreleased] — Phase 1: Smart-contract hardening

### Fixed — custody correctness (audit F1, HIGH)
- `AMPSettlement._payout` now rejects `OutcomeCode.NONE` (and any unhandled future
  enum value) with `InvalidOutcome`. Previously a verifier/arbiter supplying NONE
  marked the match SETTLED while crediting both players 0, permanently trapping
  ~99% of the escrow pool with no recovery path. Tests: `testSubmitAsyncRevertsOnNoneOutcome`,
  `testResolveDisputeRevertsOnNoneOutcome`, `testFuzz_SettlementConservesValue`.

### Changed — Phase 1
- **Fee-on-transfer token support (audit M3):** `AMPRegistry.Match` now tracks each
  player's actual received stake (`stakeAmount` for A, `stakeAmountB` for B) via
  balance-delta measurement in both `createMatch` and `joinMatch`. `_payout`,
  `CANCELLED`, and `expireMatch` refunds now use the actual per-player totals.
  `joinMatch` no longer reverts for fee-charging tokens. Test: `testFeeOnTransferTokenSettlesAndRefundsActual`.
- **OpenZeppelin `ReentrancyGuard`** replaces the hand-rolled `bool locked` guard in
  `AMPRegistry`, and is now also applied to `AMPSettlement`'s state-mutating entry
  points (audit M4). 
- **OpenZeppelin `EIP712`** replaces the hand-rolled domain-separator caching in
  `AMPSettlement` (audit L1). Byte-identical for name=`AMPSettlement`, version=`1`.
  The shared cross-language EIP-712 digest vector is now pinned in Solidity by
  `testEIP712DigestMatchesCrossLangVector`.

### Added — Phase 1
- Value-conservation property tests: fuzz over outcome+stake asserting
  `playerCredits + protocolFee == totalPool`, plus a 256-run Foundry invariant
  (`invariant_registryNativeBacksAllCredits`) asserting the Registry native balance
  always backs every outstanding credit and accrued fee.
- `AMPGovernance.t.sol` — owner economic controls (`updateProtocolFeeBps`,
  `updateProtocolFeeRecipient`) proven to require `TimelockController` schedule +
  delay + execute (audit M1). Direct calls revert; pre-delay execution reverts.
- `contracts/test/mocks/FeeOnTransferToken.sol` for the fee-token path.
- `contracts/slither.config.json` and a `[profile.coverage]` Foundry profile
  (Phase 1.7 — Slither + `forge coverage` ≥90% gate).

### Notes
- All contract SPDX headers changed MIT → Apache-2.0 (Phase 8.1 license flip).
- Committed ABIs (`contracts/out/AMP{Registry,Settlement}.json`) regenerated to
  include `stakeAmountB`; `amp-relayer` `matches()` destructure updated to the 7-tuple.

## [Unreleased] — Phase 2: Latency & throughput

### Fixed — latency root cause (audit L1, the centerpiece)
- **Replaced `Arc<ArcSwap<HashMap<String, ActiveMatch>>>` with `DashMap`** in
  `amp-server::state::InnerState`. The previous design cloned the ENTIRE active-match
  map inside `rcu` on every insert/update/archive/cleanup — ~1.5–2 MB at the
  documented `MAX_ACTIVE_MATCHES = 10_000`, dominating the matchmaking hot path.
  Measured by the new `amp-benches` matchstore microbenchmark:

  | op @ 10k entries | before | after | speedup |
  |:--|--:|--:|--:|
  | insert | 1.44 ms | 364 ns | ~3,950× |
  | update | 1.46 ms | 72 ns | ~20,300× |
  | remove | ~1.4 ms | 238 ns | ~6,000× |

### Changed — relayer throughput & correctness
- **Removed the unconditional 1000 ms settlement-processor sleep** (Phase 2.2). The
  processor now drains the queue back-to-back while non-empty and `Notify`-wakes
  instantly on enqueue, with a 1 s fallback poll. Removes the ~1 settlement/s cap.
- **`RELAYER_SETTLEMENT_CONCURRENCY`** (Phase 2.3): N concurrent settlement workers.
  Claims are serialized by a new `SettlementQueue::claim_lock` (fixes the latent
  double-claim read-modify-write race); the slow chain submission parallelizes.
  Default 1 (behavior-preserving).
- **NonceManager TOCTOU fixed** (Phase 2.3): the cached nonce is now read-and-
  incremented under a single lock hold (`reserve_cached`); previously two concurrent
  callers could both receive the same nonce (double-spend). Regression test added.
- **Mutex-poison panics fixed** (Phase 2.4): the three `per_ip.lock().unwrap()` sites
  in the relayer accept loop now recover from poisoning, matching the rest of the
  codebase.
- **Removed the dead `RELAYER_GAS_BUMP_TIMEOUT_SECS` knob** (Phase 2.5). It was parsed,
  documented, and never read; fee escalation is retry-driven (`GasManager::bump_fees`).

### Added
- `amp-benches` crate: matchstore criterion microbenchmark + a deterministic latency
  gate test (`dashmap_matchstore_under_latency_gate`, median < 100µs at 10k entries).
- `scripts/loadtest-local.sh`: reproducible local load + latency harness.
- CI `perf-gate` job: runs the latency gate (blocking) + informational criterion bench.
- Fixed `amp-loadtest`: real EIP-712 outcome signatures (post-S1 compatible), shared
  game bucketing, correct outcome codes — loadtest now exercises the full
  connect→login→match→submit→settle pipeline (200 clients → 192 matches → 96 settlements).

### Notes
- `amp-server` main.rs: all `.load()` snapshots migrated to DashMap helpers
  (`active_match_count`, `get_active_match`, `active_matches_snapshot`); no shard lock
  is held across an `await`.

## [Unreleased] — Phase 3.4: ethers → alloy migration

The unmaintained `ethers-rs 2.0.14` is **fully removed** from the workspace and
replaced by `alloy`. This is the foundation under Foundry/modern EVM Rust tooling.

### Changed — all 4 Rust EVM consumers migrated
- **amp-server**: signing + primitives only. `LocalWallet` → `PrivateKeySigner`;
  `ethers_core::types::{Address,H256,U256}` → `alloy_primitives::{Address,B256,U256}`;
  `Signature::try_from` + `recover` → `Signature::from_raw_array` + `recover_address_from_prehash`;
  `SignerSync::sign_hash_sync`. The EIP-712 digest now builds ABI words manually —
  provably byte-identical to ethers' `Token::FixedBytes`/`Uint` and to Solidity's
  `abi.encode`. The cross-language KAT `test_outcome_digest_known_vector_cross_lang`
  still pins `2d2525ad…b99096c`.
- **amp-loadtest**: same primitives/signer swap; real EIP-712 outcome signatures preserved.
- **amp-integration-tests**: `Provider<Http>` + `SignerMiddleware` + `abigen!` →
  `ProviderBuilder` (wallet-fillable) + `sol!` contract bindings. The duplicate
  inline digest is replaced by a reuse of `amp_sdk::compute_outcome_eip712_digest`.
- **amp-relayer** (heaviest): `SignerMiddleware`/`TypedTransaction`/`Eip1559TransactionRequest`
  → a wallet-fillable provider per custodial signer + alloy's call builder
  (`.max_fee_per_gas`/`.max_priority_fee_per_gas`/`.nonce` directly — no Legacy→EIP1559
  enum rewrite). `RelayerState` now holds `DynProvider`s + addresses + the master signer.
  `ProviderError` → `alloy_transport::TransportError`. Custodial HKDF/HMAC-keccak
  preserved exactly; the pinned KAT address (`0x70d8a…736a`) is unchanged.

### Dependency hygiene wins
- `ethers 2.0.14` (+ its transitive closure) removed from `Cargo.lock`.
- `h2` deduped (0.3 + 0.4 → 0.4.15); `hyper` deduped (0.14 + 1.10 → 1.10.1).
- 33 maintained `alloy` crates pulled in. (`rustls 0.21` remains — it comes from
  `amp-tls`, a separate modernization; not ethers-related.)

### Verification
- 63 Rust unit tests (incl. EIP-712 KAT, nonce-concurrency, custodial KAT, auth
  negative paths, skill-pairing proptest) + 46 Forge tests green.
- Full E2E (anvil → deploy → matchmaking → submitOutcome → on-chain settlement →
  MMR) passes through the alloy stack end-to-end.


## [Unreleased] — Phase 5: Test & CI infrastructure

### Added — failure-path coverage (audit: "1 E2E happy path, no negative tests")
- `amp-server/src/auth.rs`: 6 stateful negative-path tests covering the real
  challenge lifecycle (round-trip, unknown nonce, wrong game_id, short signature,
  unique nonces) — previously only pure-function helpers were tested.
- `amp-server/src/match_queue.rs`: `prop_skill_pairing_respects_max_diff` — 1 024-case
  `proptest` asserting two same-bucket players pair iff `|mmr_a − mmr_b| ≤ max_skill_diff`.
- `amp-integration-tests`: duplicate-submit idempotency check — a second submit on a
  settled match must be rejected (audit P4). Wrong-signature rejection is covered by
  `amp-server` unit tests (`test_verify_outcome_signature_round_trip`).
- Phase 1 already added Solidity fuzz + a 256-run value-conservation invariant.

### Changed — CI gating (audit: "3 SDKs untested in CI; C++ build swallow; release untested")
- `ci.yml`: added `sdk-go`, `sdk-python`, `sdk-js` jobs (previously absent from CI).
- `ci.yml`: the C++ build step no longer swallows failures (`|| echo` removed).
- `ci.yml`: `rust-test` now runs on an **ubuntu/macos/windows matrix** so release
  binaries are test-executed, not just cross-compiled (integration crate excluded —
  anvil is Linux-only).
- `ci.yml`: new `coverage` job (cargo-llvm-cov + `forge coverage`, uploaded to Codecov;
  non-blocking until the baseline stabilizes).
- `release.yml`: a `prerelease-tests` job now gates every tag — binaries/docker/SDKs
  only build after fmt + clippy + unit + Solidity tests pass. `if-no-files-found` set
  to `error` (was `ignore`).
- `ci.yml`/`release.yml` triggers broadened to the `prodpath` branch.

## [Unreleased] — Phase 6: Security hardening

### Fixed
- **S10:** server per-IP rate limiter now bounds distinct-IP memory (`MAX_TRACKED_IPS`
  cap + `SWEEP_EVERY` cadence); expired windows and empty entries are swept. Previously
  the outer `HashMap` grew without limit under a spoofed-source-IP flood.
- **S15:** relayer API-key verification is now constant-time (`config::ct_eq` /
  `verify_api_key`); no `HashSet` bucket-timing leak.
- **C1:** custodial key derivation now has a pinned known-answer vector
  (`0x70d8a…736a`) so any drift in the hand-rolled HKDF-HMAC-keccak256 path is caught.

### Added
- `ConnectionRateLimiter::sweep_windows` (testable) + 2 rate-limit tests.
- `config::ct_eq` / `verify_api_key` + 2 tests; custodial KAT test.
- `SECURITY_REVIEW.md`: Phase 6 section + PROXY-protocol deployment guidance +
  third-party audit scoping (pre-`v1.0.0`).




