# 06 - Ops & Security (MVP Turn-Based)

## Runtime setup
- [ ] One “demo” environment (could be prod) with:
  - [ ] matchmaker,
  - [ ] verifier,
  - [ ] C-Chain contracts on Fuji.
- [ ] All config in env/secret manager (no secrets in git).

## Monitoring (minimal)
- [ ] Tailable logs for quick debugging during judging.
- [ ] Simple dashboard or CLI to show:
  - [ ] active queues,
  - [ ] recent matches,
  - [ ] settlement outcomes.

## Security (MVP)
- [ ] Verifier keys not committed; stored securely.
- [ ] Matchmaker endpoints not fully public (or at least rate-limited).
- [ ] Clear disclaimers: “testnet only, no real-money gambling.”
