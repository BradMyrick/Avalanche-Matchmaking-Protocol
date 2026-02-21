# 05 - SDK & Demo Games (MVP)

## Scope (MVP)
- [ ] One SDK (TS or Rust) good enough for:
  - [ ] our demo card game,
  - [ ] an example Pokémon-style game.
- [ ] Both games use the same AMP flows.

## SDK capabilities (MVP)
- [ ] Connect to matchmaker (address from config).
- [ ] Join queue with:
  - [ ] gameId,
  - [ ] stake amount.
- [ ] Receive MatchConfig and opponent info.
- [ ] Build and send turn events → transcript to verifier or relay.
- [ ] Poll / subscribe for settlement result (win/lose/draw).

## Demo experience
- [ ] “Game A” (card / poker-lite) wired to AMP.
- [ ] “Game B” (monster-battle) wired to AMP.
- [ ] For both:
  - [ ] stake → play → settle → on-chain view in explorer.
- [ ] Judges script written:
  - [ ] steps for starting games,
  - [ ] what to look at on-chain / in logs.
