# 04 - Matchmaker (MVP: Turn-Based Only)

## Scope (MVP)
- [ ] Handles join/cancel queue for turn-based 1v1.
- [ ] Returns MatchConfig with:
  - [ ] gameId,
  - [ ] stake amount/token,
  - [ ] verifier address(es),
  - [ ] opponent info (playerId).
- [ ] No cross-region sharding; single region cluster is fine.

## Functional (must-have)
- [ ] Simple matchmaking policy:
  - [ ] FIFO per game/elo bucket is acceptable.
- [ ] Queue cancellation works and refunds stake (through contracts).
- [ ] Basic discovery:
  - [ ] hard-coded endpoint or simple HTTP “where is matchmaker” for the demo.

## Performance (MVP)
- [ ] Bench: 100–500 concurrent matches without errors.
- [ ] Connection caps so you can’t blow up a single node.
- [ ] Backpressure: when queue > N, reject with clear client error.

## Observability (MVP)
- [ ] Logs:
  - [ ] queue join/cancel,
  - [ ] match formed (with matchId).
- [ ] Metrics:
  - [ ] queue_depth per game,
  - [ ] matches_created_total.
