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
