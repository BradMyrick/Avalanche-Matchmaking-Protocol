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

