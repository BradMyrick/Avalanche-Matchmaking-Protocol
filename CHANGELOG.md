# Changelog

All notable changes to the Avalanche Matchmaking Protocol (AMP) project are
documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

A running summary of the production-readiness path lives in [`prodpath.md`](prodpath.md).

## [v0.1.0-rc.1] — Open beta (Fuji testnet, play-money)

The first tagged release. Open beta on Fuji testnet, no real stakes, single
maintainer, full honesty pass on docs and known limitations. See
[`KNOWNS.md`](KNOWNS.md) for the complete list of what's not there yet.

### Fixed — MMR asymmetry (audit P1, HIGH)
- `amp-server/src/player_service.rs::record_match_result_symmetric` now
  snapshots BOTH players' `(mmr, rd, vol)` BEFORE either update, computing
  both against pre-match opponent values. The previous two-sequential-call
  path let the second player read the first's post-update rating, producing
  slow rating inflation across the population.
- Regression test `test_symmetric_mmr_update_uses_pre_match_opponent_ratings`
  in `amp-server/src/main.rs`.

### Fixed — backfill over-clears non-skill hard constraints (audit, HIGH)
- `amp-match-core/src/rules.rs::evaluate_rules` now tracks the FIRST failed
  hard rule by type. Backfill relaxes ONLY `Skill` / `SkillDecay` failures;
  failed `Region` / `Language` / `Latency` / `PingBased` hard constraints
  stay gating. Pre-fix, `if !hard_pass { hard_pass = true; }` ran
  unconditionally when backfill triggered, pairing players across regions /
  languages they had explicitly forbidden.
- Regression tests `backfill_does_not_override_non_skill_hard_constraints`
  and `backfill_relaxes_skill_hard_constraint` in `amp-match-core/src/rules.rs`.

### Added — Fuji mainnet deployment (Phase 7)
- Hardened contracts deployed to Fuji testnet (chain 43113), all three
  (`AMPRegistry`, `AMPSettlement`, `AMPTimelock`) source-verified on
  Sourcify / Snowtrace:
  - `AMPRegistry`: `0x27E02ebA98D2A50Cd1079b0a611320b05A278005`
  - `AMPSettlement`: `0xc1b12a7Ffad6CeFf045064f9fE3E8879F0F3c9eD`
  - `AMPTimelock`: `0xb6d9A7e2C6d1B551C8166d9E489a8BA39B008143`
- Deployment manifest at `contracts/deployment-fuji.json` includes block,
  OZ/Forge versions, ABI references, and governance status.
- Both contracts' ownership transferred to `AMPTimock` via `Ownable2Step`;
  post-`FinalizeGovernance.s.sol` every economic action requires schedule →
  1h delay → execute.

### Changed — supply-chain pinning (Phase 2)
- OpenZeppelin pinned to `v5.6.1` and forge-std pinned to `v1.10.0` in
  every `forge install` site (`ci.yml`, `release.yml`, `Makefile`,
  `docs-check.yml`). Previously floated `master` HEAD.
- Foundry installer pinned to `v1.0.0` in `docker/amp-relayer.Dockerfile`
  with a `forge --version` assertion post-install (replaces the printed-
  but-unchecked `sha256sum` theater).
- `rust-toolchain.toml` added, pinning the workspace to Rust 1.91.0 for
  reproducible builds (alloy 2.x requires 1.91).

### Added — minimal real CI gates (Phase 3)
- New `.github/workflows/docs-check.yml` implements the `license-check` and
  `docs-consistency` gates referenced in `prodpath.md` Phase 8. Both run
  on every PR.
- `prodpath.md` ghost-gate references (`sdk-conformance`, `verify-bytecode`,
  `due-diligence`) rephrased to honestly reflect "future work" status
  rather than implying they exist today.

### Changed — config / port hygiene (Phase 4)
- `/metrics` configuration in `.env.example` commented out with a "forward-
  looking, not yet implemented" notice. The HTTP metrics exporter is
  Phase 7 future work.
- `amp-telemetry` default port moved off IANA OTLP/gRPC port 4317 → 9317.
  The service is NOT OTLP-compatible (custom Cap'n Proto schema); 4317
  was misleading downstream tooling.

### Added — open-beta documentation (Phase 6)
- New `KNOWNS.md`: single-source honest limitations list. Linked from
  README, docs/index.mdx, and SECURITY.md.
- New public `SECURITY.md`: responsible disclosure policy, scope, response
  SLAs, supported versions. Distinct from the internal `SECURITY_REVIEW.md`.
- New `docs/beta-guide.mdx`: end-to-end onboarding walkthrough for beta
  developers (Rust SDK + Fuji + docker-compose).
- `C++` example marked prominently as a skeleton requiring a wallet
  callback (audit S7 carry-forward).

## [Unreleased] — Developer-beta honesty pass

### Fixed — operator-rug vector closed at deploy time (audit re-rating, CRITICAL)
- `contracts/script/Deploy.s.sol` now deploys a `TimelockController` and
  transfers ownership of **both** `AMPRegistry` and `AMPSettlement` to it via
  `Ownable2Step`. For local/anvil (delay=0) the finalize step runs in the same
  script; for Fuji/mainnet (delay>0) the operator runs
  `contracts/script/FinalizeGovernance.s.sol` after the delay elapses. After
  finalize, `setSettlement`, `pause`, `withdrawFees`, `updateProtocolFeeBps`,
  and `updateProtocolFeeRecipient` all require schedule → delay → execute,
  closing the single-tx rug path that the previous "documented + tested"
  status did not actually close.
- New tests in `contracts/test/AMPGovernance.t.sol`:
  `testDirectSetSettlementRevertsUnderTimelock`,
  `testSetSettlementRequiresTimelockDelay`, `testDirectPauseRevertsUnderTimelock`
  (plus the existing fee-bps / fee-recipient coverage).

### Fixed — persistence durability (audit D1, HIGH)
- `amp-server/src/persistence.rs` now exposes `Persistence::flush()` and is
  called from the SIGINT/SIGTERM shutdown path. README claim "flushes
  persistence before exit" is now true.
- `amp-relayer/src/settlement.rs::SettlementQueue::flush` added and called
  from the relayer shutdown path. Closes the silent-loss-of-terminal-state
  window where a `kill -9` in the last ~500 ms could drop the
  `archive_settled_match` call and leave a "settled" match reappearing as
  active on restart.

### Added — `amp-match-core` embeddable library
- New workspace member `amp-match-core/` — a dependency-light crate containing
  the canonical Glicko-2 rating math, composable rule evaluation, and the
  bucketed matchmaking queue. Zero server / RPC / async / crypto deps.
  Studios can now embed AMP-quality matchmaking inside their own game
  servers without running the AMP service.
- API surface: `glicko2_update`, `evaluate_rules`, `MatchQueue<T: AsRef<PlayerTicket>>`,
  `PlayerTicket`, `RuleSet`, `Rule`, `MatchQualityDetail`, `MatchOutcome`.
- Includes `examples/embedded.rs` (runnable demo), `tests/integration.rs`
  (proves the library works without amp-server), and a comprehensive README
  with an honest scope section.
- `amp-server` now uses `amp-match-core` as the canonical source for
  matchmaking types and logic (no parallel copies); `QueueEntry` wraps a
  `PlayerTicket` + the notification sender via `Deref`/`AsRef`.

### Added — supply-chain CI gate
- New `.github/workflows/supply-chain.yml` runs `cargo deny --workspace
  check` on every PR and on a weekly Sunday schedule. Closes the Phase 3.3
  / `prodpath.md` exit-gate "zero RUSTSEC advisories" gap that was previously
  a paper check.
- New `deny.toml` at workspace root defines advisory policy (deny-by-default
  with justified ignores for known transitive advisories), license allowlist,
  source policy (crates.io only), and duplicate-version warnings.
- The gate also asserts `Cargo.lock` is committed and up to date.

### Changed — rebrand to honest framing
- README rewritten: tagline is now "Open-source matchmaking with optional
  on-chain settlement. Built for 1v1 escrowed indie PvP." Previous tagline
  ("Trustless Settlement") overstated the actual trust model — AMP is a
  custodial-escrow product with cryptographic outcome integrity, not a
  trustless protocol.
- Added explicit "Trust model (honest version)" and "Honest Scope" sections
  to README. Stubbed rule evaluators and deferred features are listed
  outright rather than implied.
- `docs/index.mdx` rewritten with the same honesty pass.
- `SECURITY_REVIEW.md` updated: C4 re-rated from MEDIUM to CRITICAL with
  the corrected status (the rug vector is now actually closed, not just
  documented); new D1 row added for the durability fix; Trust Model
  section rewritten to reflect deploy-time timelock wrapping.

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




