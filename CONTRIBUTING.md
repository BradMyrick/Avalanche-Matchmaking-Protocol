# Contributing to AMP

Thanks for your interest in improving AMP.

## Project status

AMP is in open beta on the Fuji testnet. The path to C-Chain mainnet is on the [Roadmap](https://docs.page/bradmyrick/avalanche-matchmaking-protocol/roadmap).

## Before you write code

1. Open an issue for anything beyond a small fix so the approach is agreed.
2. Read the relevant docs in [`docs/`](docs/) (also hosted at [amp-docs.pages.dev](https://docs.page/bradmyrick/avalanche-matchmaking-protocol)) before touching authentication, signing, transport, or RPC-boundary code.
3. Treat anything that regresses an audit finding as a release blocker.

## Development prerequisites

- Rust **1.91+** (pinned by `rust-toolchain.toml`, edition 2024)
- Cap'n Proto C++ library (`apt install capnproto` / `brew install capnp`)
- Foundry (`forge` + `anvil`)
- Go 1.22+, Python 3.9+, Node.js 18+, .NET 8.0+ (for the multi-language SDKs)

## Development loop

```bash
make setup        # fetch deps, install forge libs
make build        # forge build + cargo build --release
make lint         # clippy -D warnings + forge fmt --check
make format       # auto-format
make test         # all workspace + SDK tests
make test-integration  # full E2E: anvil + contracts + 3 services
```

## Pull request checklist

- [ ] `cargo fmt --all -- --check` is clean.
- [ ] `cargo clippy --workspace --all-targets -- -D warnings` is clean.
- [ ] `cargo test --workspace` passes.
- [ ] `cd contracts && forge test -vvv` passes.
- [ ] The relevant SDK test suites pass (`make test-sdk-{go,python,js}`, `amp-sdk/cpp` CMake, `amp-sdk/csharp` dotnet test).
- [ ] `make test-integration` passes.
- [ ] `cargo deny --workspace check --config deny.toml` passes.
- [ ] No new `unwrap()`/`expect()`/`panic!()` on production paths (tests are fine).
- [ ] No new `unsafe`, no `await` while holding a lock.
- [ ] If you touched a hot path, the latency gate (`amp-benches/tests/latency_gate.rs`) still passes.
- [ ] If you touched signing/digest code, the cross-language EIP-712 KAT is still byte-identical across all SDKs.
- [ ] Update `CHANGELOG.md` under `[Unreleased]`.
- [ ] Update docs if behavior changed.

## Signing & cross-SDK integrity

The EIP-712 digest over `(matchId, outcome, transcriptHash)` is byte-identical across Rust, Go, C++, C#, Python, and JavaScript. The shared known-answer vector (`2d2525ad…b99096c`) is enforced per-SDK in CI. If you change the digest logic, update every SDK and every conformance test in lockstep. See [`docs/signing.mdx`](docs/signing.mdx) for the canonical scheme.

## Licensing

All contributions are accepted under the **Apache-2.0** license. By submitting a pull request you agree your contributions are licensed accordingly.
