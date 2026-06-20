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


