# Security Policy

## Supported versions

AMP is **open beta**. The only supported version is the latest tag on `main`. Pre-release versions are not long-term supported; update to the latest before reporting issues.

| Version | Supported | Notes |
|:---|:---|:---|
| `v0.1.0-rc.x` | ✅ Latest tag only | Open beta, Fuji testnet |
| `< v0.1.0-rc.1` | ❌ | Pre-beta tags; do not run |
| `main` HEAD | ⚠️ Best effort | Use a tagged release instead |

Avalanche C-Chain mainnet is not yet supported. All contract deployments are on Fuji testnet. Mainnet deployment follows the [Roadmap](https://docs.page/bradmyrick/avalanche-matchmaking-protocol/roadmap) v0.3 milestone (independent audit, 72h soak, chaos testing).

## Reporting a vulnerability

**Do NOT open public GitHub issues for security vulnerabilities.** Report via one of:

1. **GitHub private vulnerability advisory** (preferred): https://github.com/BradMyrick/Avalanche-Matchmaking-Protocol/security/advisories/new
2. **Email**: brad@<your-domain-here> with `[AMP SECURITY]` in the subject.

Please include:
- A description of the issue and its impact
- The component affected (`amp-server`, `amp-relayer`, `amp-match-core`, `contracts/`, an SDK, supply-chain)
- A minimal reproducer or proof-of-concept
- Suggested fix if you have one

Initial response within **72 hours**. Coordinated disclosure timeline is **90 days** from acknowledgment (extendable on request).

## Scope

**In scope:**
- Rust crates in this workspace (`amp-server`, `amp-relayer`, `amp-match-core`, `amp-telemetry`, `amp-tls`, `amp-sdk/rust`, `amp-sdk/js/native`)
- Solidity contracts (`contracts/src/AMPRegistry.sol`, `contracts/src/AMPSettlement.sol`, `contracts/src/AMPTypes.sol`)
- Deployment scripts (`contracts/script/Deploy.s.sol`, `contracts/script/FinalizeGovernance.s.sol`)
- SDK clients in `amp-sdk/{go,cpp,csharp,python,js}`

**Out of scope:**
- Issues in upstream dependencies (report to the upstream maintainer; if AMP-specific exposure exists, that's in scope)
- Social engineering, phishing, physical attacks on operators
- DoS vectors that require capabilities already gated by authentication

## Acknowledgments

We credit reporters in the release notes of the fix release unless you prefer otherwise. There is no monetary bug bounty at this time.

## Public disclosures

 disclosed vulnerabilities are documented in [`CHANGELOG.md`](CHANGELOG.md). Material disclosures are also called out in release notes.

## Past disclosures

_None yet._
