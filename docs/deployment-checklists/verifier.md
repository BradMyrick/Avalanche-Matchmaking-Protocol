# 03 - Verifier (MVP: Turn-Based Async)

## Scope (MVP)
- [ ] Supports 1–2 game types:
  - [ ] simple card game,
  - [ ] Pokémon-like turn-based battle.
- [ ] Verifies a single canonical replay format per game version.

## Determinism & correctness
- [ ] Given (initial_state, transcript) → deterministic winner.
- [ ] Golden replays:
  - [ ] checked into repo,
  - [ ] used in tests to ensure replay stays stable.
- [ ] Invalid transcript handling:
  - [ ] reject with explicit reason,
  - [ ] no panic / crash.

## On-chain integration
- [ ] Sign result attestations with verifier key.
- [ ] Settlement contract verifies:
  - [ ] signature,
  - [ ] gameId & matchId,
  - [ ] outcome enum.
- [ ] Happy-path settlement demo passing (2–3 example matches).

## Ops (MVP)
- [ ] Single instance OK; document how to scale later.
- [ ] Basic logs: matchId, gameId, result, replay duration.
