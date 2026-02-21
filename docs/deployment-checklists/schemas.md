# 02 - Transcripts & Schemas (MVP Turn-Based)

## Scope (MVP)
- [ ] Cap’n Proto schemas for:
  - [ ] MatchConfig,
  - [ ] TurnEvent (or InputFrame),
  - [ ] MatchTranscript (sequence of turns, final state).
- [ ] Single version per game (no live upgrades).

## Determinism
- [ ] Turn ordering defined (turn index, player index).
- [ ] RNG is either:
  - [ ] fully derived from a seed in MatchConfig, or
  - [ ] pre-drawn and recorded in transcript (no hidden randomness).
- [ ] Hash function and serialization documented for transcripts.

## Tooling
- [ ] Codegen wired for Rust (backend) + TS (demo client).
- [ ] Round-trip test: build transcript in client, parse in verifier, hashes match.
