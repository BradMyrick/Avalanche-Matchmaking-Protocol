# AMP Upgrade Plan

## Phase A: CI Hardening + Contract Cleanup (Days 1-2)

| # | Task | Files | Status |
|---|------|-------|--------|
| A1 | Expand `ci.yml` with `cargo fmt --check`, `cargo clippy -D warnings`, `forge fmt --check` | `.github/workflows/ci.yml` | [x] |
| A2 | Add `amp-relayer` to CI test matrix | `.github/workflows/ci.yml` | [x] |
| A3 | Add C++ (CMake) and C# (dotnet build) SDK build verification to CI | `.github/workflows/ci.yml` | [x] |
| A4 | Add `Pausable` to `AMPRegistry` and `AMPSettlement` with `onlyOwner` pause/unpause | `contracts/src/AMPRegistry.sol`, `contracts/src/AMPSettlement.sol` | [x] |
| A5 | Upgrade `AMPRegistry` owner from `immutable` to `Ownable2Step` | `contracts/src/AMPRegistry.sol` | [x] |
| A6 | Implement `cancelMatch(uint256)` for OPEN matches, `expireMatch(uint256)` with timeout | `contracts/src/AMPRegistry.sol` | [x] |
| A7 | Cap verifier array length to 10 | `contracts/src/AMPRegistry.sol` | [x] |
| A8 | Delete stale `IAMPRegistry.sol` and `IAMPSettlement.sol` (bytes32 ID versions) | `contracts/src/` | [x] |
| A9 | Unify pragma to `^0.8.33` across all `.sol` files | All `.sol` files | [x] |
| A10 | Fix `generate_bindings.sh` hardcoded paths to use relative paths | `amp-sdk/generate_bindings.sh` | [x] |

## Phase B: Docker + Relayer Refactor (Days 2-4)

| # | Task | Files | Status |
|---|------|-------|--------|
| B1 | Create multi-stage `Dockerfile` for `amp-server` | `docker/amp-server.Dockerfile` | [x] |
| B2 | Create multi-stage `Dockerfile` for `amp-relayer` | `docker/amp-relayer.Dockerfile` | [x] |
| B3 | Create multi-stage `Dockerfile` for `amp-telemetry` | `docker/amp-telemetry.Dockerfile` | [x] |
| B4 | Fix root `docker-compose.yml` to build from Dockerfiles | `docker-compose.yml` | [x] |
| B5 | Fix localnet `docker-compose.yml` (remove broken `amp-core` reference, use Anvil) | `docker/localnet/docker-compose.yml` | [x] |
| B6 | Refactor `amp-relayer` into modular structure: `main.rs`, `relayer.rs`, `settlement.rs`, `gas.rs`, `nonce.rs`, `custodial.rs`, `config.rs` | `amp-relayer/src/*.rs` | [x] |
| B7 | Implement `SettlementQueue` with sled-backed persistence | `amp-relayer/src/settlement.rs` | [x] |
| B8 | Implement retry logic: exponential backoff (1s->16s, max 5 retries) + dead-letter | `amp-relayer/src/settlement.rs` | [x] |
| B9 | Implement EIP-1559 gas fee bumping for pending txs | `amp-relayer/src/gas.rs` | [x] |
| B10 | Implement per-custodial-wallet nonce tracking | `amp-relayer/src/nonce.rs` | [x] |
| B11 | Add startup replay: resume pending settlements from sled on boot | `amp-relayer/src/main.rs` | [x] |

## Phase C: Contract Tests + Relayer Tests (Days 4-6)

| # | Task | Files | Status |
|---|------|-------|--------|
| C1 | Expand `AMPRegistry.t.sol` to ~20 tests: access control, ERC20, reentrancy, pause, cancel, expiry, ownership transfer | `contracts/test/AMPRegistry.t.sol` | [x] |
| C2 | Expand `AMPSettlement.t.sol` to ~25 tests: all outcomes, invalid sigs, wrong mode, non-player, non-arbiter, fee edge cases, RT agreement, pause | `contracts/test/AMPSettlement.t.sol` | [x] |
| C3 | Create `AMPInvariant.t.sol`: fuzz `submitAsyncResult` with random sigs, invariant on fund conservation, fuzz state machine, fuzz payout math | `contracts/test/AMPInvariant.t.sol` | [ ] |
| C4 | Add gas optimization: `mapping(address => bool)` for verifier lookup, struct packing | `contracts/src/AMPSettlement.sol`, `contracts/src/AMPTypes.sol` | [ ] |
| C5 | Relayer unit tests: `SettlementQueue` enqueue/process/retry/dead-letter | `amp-relayer/src/settlement.rs` (inline tests) | [ ] |
| C6 | Relayer unit tests: nonce tracking under concurrent access | `amp-relayer/src/nonce.rs` (inline tests) | [ ] |
| C7 | Relayer unit tests: gas fee bumping logic | `amp-relayer/src/gas.rs` (inline tests) | [ ] |
| C8 | Relayer integration test: submit outcome to Anvil, verify on-chain settlement | `amp-relayer/tests/integration.rs` | [ ] |

## Phase D: SDK Library + Release Pipeline (Days 6-9)

| # | Task | Files | Status |
|---|------|-------|--------|
| D1 | Port C++ example client into `amp-sdk/cpp/src/client.cpp` (fill all stubs) | `amp-sdk/cpp/src/client.cpp`, `amp-sdk/cpp/include/` | [x] |
| D2 | Port C# example client into `amp-sdk/csharp/AmpSdk/AmpClient.cs` (uncomment all methods) | `amp-sdk/csharp/AmpSdk/AmpClient.cs` | [x] |
| D3 | Generate Go bindings, implement full `AMPClient` (RequestMatch, SubmitOutcome, etc.) | `amp-sdk/go/client/`, `amp-sdk/go/generated/` | [x] |
| D4 | Generate Python bindings, create `pyproject.toml`, verify all client methods | `amp-sdk/python/` | [x] |
| D5 | Create `amp-sdk/rust/src/lib.rs` with `AmpClient`, `UserSession`, `MatchSession` wrappers + `thiserror` error types | `amp-sdk/rust/src/` | [x] |
| D6 | Create napi-rs addon: `amp-sdk/js/` with Rust core wrapping Cap'n Proto client, exposing idiomatic TS API | `amp-sdk/js/src/`, `amp-sdk/js/index.ts` | [ ] |
| D7 | Create release workflow: cross-compile Rust binaries, build Docker images, package SDKs, create GitHub Release | `.github/workflows/release.yml` | [x] |
| D8 | Add SDK build verification to CI (validate all bindings compile after schema changes) | `.github/workflows/ci.yml` | [x] |

## Phase E: Load Testing + OpenTelemetry (Days 9-11)

| # | Task | Files | Status |
|---|------|-------|--------|
| E1 | Create `amp-loadtest` crate as workspace member | `amp-loadtest/Cargo.toml`, `amp-loadtest/src/` | [x] |
| E2 | Implement concurrent client runner: N clients login->match->outcome, latency histograms | `amp-loadtest/src/runner.rs`, `client.rs` | [x] |
| E3 | Add OpenTelemetry to `amp-server`: layered `tracing_subscriber` with OTLP export | `amp-server/src/main.rs`, `amp-server/Cargo.toml` | [ ] |
| E4 | Define spans: `login`, `request_match`, `find_match`, `submit_outcome`, `relay_tx` | `amp-server/src/main.rs`, `matchmaker.rs` | [ ] |
| E5 | Add Prometheus `/metrics` HTTP endpoint to `amp-server` | `amp-server/src/metrics.rs` (new) | [ ] |
| E6 | Add OpenTelemetry to `amp-relayer` | `amp-relayer/src/main.rs`, `Cargo.toml` | [ ] |
| E7 | Add `tikv-jemallocator` to `amp-server`, periodic memory stats logging | `amp-server/src/main.rs`, `Cargo.toml` | [ ] |

## Phase F: Examples + Docs + Quickstart (Days 11-13)

| # | Task | Files | Status |
|---|------|-------|--------|
| F1 | Create `amp-examples/basic-matchmaking/cpp/` -- minimal C++ example (< 50 lines) | `amp-examples/basic-matchmaking/cpp/` | [ ] |
| F2 | Create `amp-examples/basic-matchmaking/csharp/` -- minimal C# example | `amp-examples/basic-matchmaking/csharp/` | [ ] |
| F3 | Create `amp-examples/basic-matchmaking/go/` -- minimal Go example | `amp-examples/basic-matchmaking/go/` | [ ] |
| F4 | Create `amp-examples/basic-matchmaking/python/` -- minimal Python example | `amp-examples/basic-matchmaking/python/` | [ ] |
| F5 | Create `amp-examples/basic-matchmaking/rust/` -- minimal Rust example | `amp-examples/basic-matchmaking/rust/` | [ ] |
| F6 | Create `amp-examples/basic-matchmaking/node/` -- minimal Node.js example using napi addon | `amp-examples/basic-matchmaking/node/` | [ ] |
| F7 | Create `scripts/start-localnet.sh`: starts Anvil, deploys contracts, starts all services, verifies health | `scripts/start-localnet.sh` | [x] |
| F8 | Fix `trace-viewer` to read `telemetry.bin` (replace `load_blank()` with binary reader) | `trace-viewer/src/main.rs` | [x] |
| F9 | Add comprehensive doc comments to all public SDK APIs | All SDK source files | [ ] |
| F10 | Standardize error types across all SDKs: `AmpResult<T>` / typed errors | All SDK source files | [ ] |
| F11 | Create `amp-examples/basic-matchmaking/README.md` with per-language instructions | `amp-examples/basic-matchmaking/README.md` | [ ] |

## Phase G: Memory Stability + Contract Fuzzing (Days 13-14)

| # | Task | Files | Status |
|---|------|-------|--------|
| G1 | 72-hour soak test config: run `amp-loadtest` sustaining 1k connections, monitor heap | `amp-loadtest/` | [ ] |
| G2 | Complete invariant/fuzz testing for contracts (if not finished in Phase C) | `contracts/test/AMPInvariant.t.sol` | [ ] |
| G3 | Add `forge coverage` to CI with threshold gate (>90%) | `.github/workflows/ci.yml` | [ ] |
