# AMP → Mainnet: Complete, Verifiable Plan

> Fuji-first path. Phase 10 (C-Chain mainnet) and Phase 11 (L1) are deferred
> until the Fuji testnet audit gate (Phase 9) is GREEN.

## Locked decisions

| Area | Decision |
|---|---|
| **License** | Fully permissive — **Apache-2.0** across server, contracts, all SDKs, docs. |
| **Mainnet target** | **Avalanche C-Chain (43114)** after Fuji gate clean; **L1** as post-release phase. |
| **Dependency migrations** | `bincode→postcard` + `sled→redb` in this path; `ethers→alloy` as immediate follow-on phase. |
| **Latency SLO** | **p99 < 1 ms** matchmaking @ 10 k active matches / 1 k concurrent conns (server-internal); **p99 < 2 ms** submit-outcome node. CI perf-gate enforced. |

## The Uniformity Standard (governs ALL SDK work)

A single spec, `amp-sdk/CONFORMANCE.md`, defines the **Common Capability Set (CCS)**
every SDK must implement identically. No SDK ships without passing it.

**CCS** = { plaintext connect, TLS connect, challenge request, EIP-191 challenge
sign, requestMatch, submitOutcome (internal EIP-712 digest+sign),
subscribeToEvents/MatchListener, reconnect, Glicko-2 UpdateMMR, typed error
hierarchy, Config::Debug redaction } + shared EIP-712 KAT vector across all 6
languages + shared negative-test vector set.

Enforcement: a CI job `sdk-conformance` runs every SDK's conformance test against
the **same** fixture vectors and fails on any divergence.

---

## Master map: every audit finding → phase → verification test

| # | Audit finding | Phase | Verification test (named) |
|---|---|---|---|
| F1 | `NONE`-outcome fund lock | P1 | `test_payout_reverts_on_none_outcome` + `invariant_value_conservation` |
| F2 | Non-reproducible deploy / OZ floating | P7 | `verify-bytecode` CI job |
| F3 | License conflict | P8 | `license-check` CI job |
| L1 | `arc_swap::rcu` full-map clone (latency) | P2 | `bench_matchstore_p99` perf-gate |
| L2 | Relayer ~1 tx/s serialization | P2 | `bench_relayer_throughput` (≥10/s) |
| L3 | Relayer single-threaded RPC | P2 | `test_relayer_concurrent_rpc` |
| L4 | Dead `bump_timeout_secs` | P2 | implement or remove |
| D1 | No sled flush / crash dup settlement | P3 (redb) | `test_crash_no_duplicate_settlement` |
| D2 | ethers/bincode/sled risk | P3 | `cargo audit` clean + `cargo deny` gate |
| C1 | Hand-rolled HKDF-keccak custodial | P6 | `test_custodial_derivation_kat` |
| S10 | Per-IP rate limiter unbounded | P6 | `test_rate_limiter_memory_bounded` |
| S15/ct | Constant-time API-key compare | P6 | `test_api_key_constant_time` |
| P8 | Glicko-2 stale doc | P5/P8 | `test_glicko_iteration_cap` + doc fix |
| Trust | Single verifier/arbiter, no timelock | P1/P7 | `test_treasury_timelock_delay` |
| SDK | C++/C#/JS/Go gaps; uniform parity | P4 | `sdk-conformance` matrix |
| CI | Go/Py/JS not in CI; C++ build swallow | P5 | CI matrix jobs |
| E2E | 1 binary, 0 `#[test]` | P5 | `integration-*` ≥12 scenarios |
| Release | No test gate | P5/P7 | `release.yml needs: [ci]` |
| Docs | Wrong counts, stale roadmap | P8 | `docs-consistency` check |
| Branch | `doc-fix` 35 ahead of `main` | P0 | `main` is release line |

---

## Phase 0 — Stop the bleeding (branch & release hygiene)
- 0.1 Merge `doc-fix` → `main`; delete merged long-lived branches. `main` = release line.
- 0.2 Add `CODEOWNERS`, `CONTRIBUTING.md`, PR template, issue templates.
- 0.3 Add `CHANGELOG.md` (Keep a Changelog format).
- 0.4 Single-source-of-truth version; bump to `0.1.0` consistently.

**Exit gate P0:** clean `main`; `CHANGELOG.md` populated; no `0.0.1` left.

## Phase 1 — Smart-contract hardening
- 1.1 **F1 fix:** reject `OutcomeCode.NONE` in `submitAsyncResult`/`submitRealTimeHashResult`/`resolveDispute`.
- 1.2 Value-conservation invariants.
- 1.3 OZ `ReentrancyGuard` on both contracts.
- 1.4 Timelock on owner economic controls.
- 1.5 Fee-on-transfer token support on `joinMatch`.
- 1.6 Switch to OZ `EIP712.sol`; Solidity-side KAT vs shared vector.
- 1.7 Slither config; `forge coverage` ≥90%.

**Exit gate P1:** forge unit+invariant+fuzz green; Slither 0 high/medium; coverage ≥90%.

## Phase 2 — Latency & throughput (centerpiece)
- 2.1 **`Arc<ArcSwap<HashMap<String,ActiveMatch>>>` → `DashMap<String,ActiveMatch>`** (`state.rs`).
- 2.2 Relayer: remove unconditional 1000 ms sleep; `Notify`-wake; `RELAYER_CONCURRENCY`.
- 2.3 Parallelize relayer RPCs; fix `NonceManager` TOCTOU.
- 2.4 Fix 3 `per_ip.lock().unwrap()` poison panics.
- 2.5 Implement or remove `RELAYER_GAS_BUMP_TIMEOUT_SECS`.
- 2.6 Add `amp-server/benches/` (criterion) + scripted loadtest.
- 2.7 Perf-gate CI job `perf-regression` (p99 < 1 ms matchmaking @ 10 k/1 k).

**Exit gate P2:** `bench_matchmaking_p99_under_load` p99 < 1 ms; relayer ≥10/s; perf-regression green.

## Phase 3 — Dependency migration (phased)
- 3.1 `bincode → postcard` for all persisted values.
- 3.2 `sled → redb`; single transaction for archive path; `flush()` on shutdown.
- 3.3 `cargo-deny` + `cargo-audit` CI; dedup rustls/h2/hyper.
- 3.4 (follow-on, risk-isolated) `ethers → alloy`.

**Exit gate P3:** zero RUSTSEC advisories; no duplicate versions; crash test passes.

## Phase 4 — SDK uniform parity (all 6 to CCS)
- 4.1 Write `amp-sdk/CONFORMANCE.md` + `amp-sdk/fixtures/`.
- 4.2 C++: canonical EIP-712 digest + first unit tests.
- 4.3 C#: digest tests + TLS + typed errors.
- 4.4 JS: `publishConfig.access`; platform sub-packages; `dial_tls`.
- 4.5 Go: fresh-clone build; port Glicko-2 to other 5 SDKs.
- 4.6 TLS in all 6.
- 4.7 Typed error hierarchy in C++/C#.
- 4.8 Per-SDK runnable example (full loop).

**Exit gate P4:** `sdk-conformance` green for all 6; fresh-clone build everywhere.

## Phase 5 — Test & quality infrastructure
- 5.1 Expand E2E into ≥12 named `#[test]` scenarios.
- 5.2 proptest for Glicko-2, match_queue, rate limiter, digest.
- 5.3 Foundry fuzz + invariant tests.
- 5.4 CI jobs for each SDK; remove `|| echo` C++ swallow.
- 5.5 OS matrix for Rust test job (ubuntu/macos/windows).
- 5.6 Release `needs:[ci]`; remove `if-no-files-found:ignore`.
- 5.7 Coverage tooling + gating (≥85% Rust core).

**Exit gate P5:** every language tested in CI; E2E ≥12; fuzz+invariant+proptest green.

## Phase 6 — Security hardening
- 6.1 Bound per-IP rate limiter; move retain off accept thread.
- 6.2 Constant-time API-key comparison.
- 6.3 Custodial derivation: vetted crate or published KAT vector.
- 6.4 PROXY-protocol support doc/parsing.
- 6.5 Third-party audit engagement.

**Exit gate P6:** all S/M items closed or documented-accepted.

## Phase 7 — Reproducible deployment (Fuji)
- 7.1 Pin OZ submodule to release tag v5.6.1; CI matches.
- 7.2 Deterministic `CREATE2` deploy + `deployment.json` manifest.
- 7.3 Deploy Fuji; `forge verify-check`; commit `deployment-fuji.json`.
- 7.4 Multisig + timelock ownership runbook.
- 7.5 Legacy-drain migration script.

**Exit gate P7:** Fuji deployment byte-identical to source, verified.

## Phase 8 — Documentation, license, governance
- 8.1 Apache-2.0 everywhere; remove Non-Commercial license.
- 8.2 Rewrite `SECURITY_REVIEW.md` to include contracts; fix stale rows.
- 8.3 Fix wrong test counts; remove "old style" README note; publish benchmarks.
- 8.4 Update architecture doc to DashMap reality; cite benchmarks.

**Exit gate P8:** `license-check` + `docs-consistency` green; no overstatements.

## Phase 9 — FUJI TESTNET VALIDATION (the audit-clean gate)
- 9.1 72 h soak @ 1 k concurrent.
- 9.2 Chaos: kill -9 ×100; assert no dup settlement / fund lock / queue replays.
- 9.3 Automated `due-diligence` CI re-audit job → all GREEN.
- 9.4 Tag `v0.1.0-rc.1` on Fuji.

**Exit gate P9:** `due-diligence` job fully GREEN = investor review clean on testnet.
**This is the hard prerequisite to mainnet (Phase 10, deferred).**

---

## Critical path & ordering
```
P0 → P1 ‖ P2 → (P3) → P4 ‖ P5 ‖ P6 → P7 → P8 → P9(GATE)
        └─ latency is the long pole; start P2 first
```

## Deferred (post-Fuji-gate)
- **Phase 10** — C-Chain mainnet `v1.0.0`.
- **Phase 11** — Avalanche L1 (appchain): genesis, validator set, Warp, ACP-176.
