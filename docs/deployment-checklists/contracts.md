# 01 - Contracts (MVP: Turn-Based Async)

## Scope (MVP)
- [ ] Only ASYNC_VERIFIER mode enabled on-chain.
- [ ] Single stake token (e.g., testnet ERC20 or AVAX).
- [ ] Simple 2-player matches (no team payouts).
- [ ] No upgrades beyond emergency pause.

## Pre-deploy (must-have)
- [ ] Compile & test with Foundry/Hardhat; CI green.
- [ ] Unit tests cover:
  - [ ] match creation with stake escrow,
  - [ ] verifier attestation flow,
  - [ ] payout to winner / draw / refund,
  - [ ] pause/unpause.
- [ ] Invariants:
  - [ ] total escrowed == sum of open matches,
  - [ ] no funds lost on cancel/timeout.

## Security (MVP)
- [ ] Static analysis run (Slither or similar); criticals fixed.
- [ ] Re-entrancy guards around settlement.
- [ ] Role model:
  - [ ] OWNER (pause, fee change),
  - [ ] GAME_ADMIN (register games),
  - [ ] VERIFIER (submit results).

## Deployment (Fuji only for MVP)
- [ ] Fuji deployment script committed.
- [ ] Addresses + ABIs recorded in `/docs/contracts.md`.
- [ ] One demo game registered (e.g., `CardGameV1`).

## Demo readiness
- [ ] End-to-end flow on Fuji:
  - [ ] stake → match → verifier attests → payout observable in explorer.
