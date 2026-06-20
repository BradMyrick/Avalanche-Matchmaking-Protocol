<!-- AMP pull request template. See CONTRIBUTING.md and prodpath.md. -->

## Summary

<!-- What does this PR change and why? -->

## Related audit finding / prodpath phase

<!-- e.g. "Closes F1 (Phase 1.1): NONE-outcome fund lock" or "N/A". Reference
     the finding ID from prodpath.md whenever possible. -->

## Verification

<!-- Check the boxes that apply; see CONTRIBUTING.md for commands. -->
- [ ] `cargo fmt --all -- --check`
- [ ] `cargo clippy --workspace --all-targets -- -D warnings`
- [ ] `cargo test --workspace`
- [ ] `cd contracts && forge test -vvv`
- [ ] Relevant SDK suites (`make test-sdk-{go,python,js}`)
- [ ] `make test-integration`
- [ ] Perf gate unaffected (if hot-path touched): matchmaking p99 < 1 ms
- [ ] Cross-language EIP-712 KAT byte-identical (if signing touched)

## Risk / regression notes

<!-- Any new unwrap/panic on production paths? new unsafe? locks held across await?
     Does this reintroduce a closed SECURITY_REVIEW finding? -->

## Changelog

- [ ] Updated `CHANGELOG.md` under `[Unreleased]`
- [ ] Updated docs (`docs/`, `README.md`, `SECURITY_REVIEW.md`) if behavior changed
