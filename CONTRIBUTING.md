# Contributing to AMP

Thank you for your interest in improving AMP. This guide is the short version;
the full engineering roadmap and exit-gate criteria live in
[`prodpath.md`](prodpath.md).

## Project status

AMP is on a bounded path to an Avalanche C-Chain mainnet release. The
**Fuji testnet validation gate** (Phase 9 of `prodpath.md`) is the hard
prerequisite to mainnet: the automated `due-diligence` re-audit must be GREEN.
Treat anything that regresses an audit finding as a release blocker.

## Before you write code

1. **Read [`SECURITY_REVIEW.md`](SECURITY_REVIEW.md)** before touching
   authentication, signing, transport, or RPC-boundary validation code.
2. **Read [`prodpath.md`](prodpath.md)** so your change aligns with the active
   phase and doesn't reintroduce a closed finding.
3. Open an issue for anything beyond a small fix so the approach is agreed.

## Development prerequisites

- Rust **1.87+** (edition 2024)
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
- [ ] The relevant SDK test suites pass (`make test-sdk-{go,python,js}`).
- [ ] `make test-integration` passes.
- [ ] No new `unwrap()`/`expect()`/`panic!()` on production paths (tests are fine).
- [ ] No new `unsafe`, no `await` while holding a lock.
- [ ] If you touched a hot path, the Phase 2 perf gate (`bench_matchmaking_p99_under_load`) still reports **p99 < 1 ms**.
- [ ] If you touched signing/digest code, the cross-language EIP-712 KAT is still byte-identical across all SDKs.
- [ ] Update `CHANGELOG.md` under `[Unreleased]`.
- [ ] Update docs if behavior changed.

## Signing & cross-SDK integrity

The EIP-712 digest over `(matchId, outcome, transcriptHash)` MUST be
byte-identical across Rust, Go, C++, C#, Python, and JavaScript. The shared
known-answer vector is enforced per-SDK; if you change the digest logic you
must update **every** SDK and every conformance test in lockstep. See
[`docs/signing.mdx`](docs/signing.md) for the canonical scheme.

## Licensing

All contributions are accepted under the **Apache-2.0** license. By submitting
a pull request you agree your contributions are licensed accordingly.
