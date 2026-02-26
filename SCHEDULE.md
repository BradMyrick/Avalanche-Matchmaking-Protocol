# AMP – Avalanche Build Games Execution Plan (6 Weeks)

This is a pragmatic, infra‑heavy schedule optimized for a strong demo, clear story, and decent sleep.

---

## Week 1 – Narrative, Scope, and Pitch

### Goals
- Lock narrative, scope, and technical cuts for a six‑week build.
- Prepare the 1‑minute pitch and initial collateral.

### Day 1–2: Narrative and Requirements
- Write a one‑pager:
  - Problem: on‑chain PvP matchmaking/settlement is fragmented, bespoke, and insecure.
  - Solution: AMP as a shared, Avalanche‑native matchmaking + settlement layer.
  - Core features for MVP:
    - Game registration (`AMPRegistry`).
    - Match creation/joining with stakes.
    - Two settlement modes: `ASYNC_VERIFIER` and `RT_HASH_AGREE`.
    - Off‑chain verifier service and a simple reference game.
  - Why Avalanche: low‑latency finality, gaming push, future subnet.
- Define **non‑goals** for MVP:
  - No tournaments/brackets.
  - No complex rating/ELO.
  - One stake token (e.g., a test ERC‑20), one verifier.
- Draft architecture diagram:
  - Players ↔ Game Client ↔ AMP SDK ↔ C‑Chain contracts.
  - Game server/verifier service reading transcripts, signing results.

### Day 3: Contract and Protocol Design
- Specify `AMPRegistry` interface:
  - `registerGame(gameId, rulesType, settlementMode, verifier, stakeToken)`.
  - `createMatch(gameId, stakeAmount, players, timeout)`.
  - `joinMatch(matchId)` and events (`GameRegistered`, `MatchCreated`, `MatchJoined`).
- Specify `AMPSettlement` interface:
  - `lockStake(matchId)`, `submitResult(matchId, result, transcriptHash, sigs)`.
  - Settlement modes handling:
    - `ASYNC_VERIFIER`: single verifier signature.
    - `RT_HASH_AGREE`: N‑of‑N player signatures on hash + winner; verifier only on dispute.
  - `resolveDispute(matchId, verifierSig)`, `claim(matchId)`.
- Define key data structures (Solidity structs, enums).

### Day 4: Off‑Chain + Cap’n Proto Design
- Design Cap’n Proto schemas:
  - `MatchConfig` (gameId, playerIds, seed, mode).
  - `InputFrame` (frameIndex, playerId, input payload).
  - `GameEvent` (enum type, payload).
  - `MatchTranscript` (config, events, finalResult).
- Decide serialization + hashing:
  - Canonical encoding → `keccak256(transcriptBytes)`.
- Outline verifier flow:
  - Receive transcript.
  - Re‑simulate/validate.
  - Produce `{matchId, winner, hash, signature}`.

### Day 5: SDK + Reference Game Design
- Decide languages:
  - SDK: TypeScript (browser + Node).
  - Verifier: Rust or Go (your choice; pick stronger muscle).
  - Reference game: very simple web game (TS + canvas/React) or minimal server‑authoritative loop.
- Define SDK surface:
  - `registerGame`, `createMatch`, `joinMatch`.
  - `submitAsyncResult`, `submitRealtimeResult`.
  - Utility for encoding transcripts + signing client messages.
- UX design for reference game:
  - Lobby screen, “Queue for match,” basic match UI, “Match result” screen.

### Day 6–7: Pitch and Repo Setup
- Prepare 1‑minute pitch script and dry run a few times.
- Create repos:
  - `amp-contracts`, `amp-verifier`, `amp-sdk`, `amp-reference-game`, `amp-docs`.
- Add initial READMEs with high‑level vision and TODOs.
- Set basic CI (lint, tests, build).

---

## Week 2 – Core Contracts and Verifier Skeleton

### Goals
- Contracts compiling and partially tested.
- Verifier service scaffolded with Cap’n Proto parsing and stubbed rules.

### Day 8–9: Implement Contracts v0
- Implement `AMPRegistry`:
  - Game registration with owner/admin.
  - Match struct, creation, joining, basic checks (game registered, stake amount).
- Implement `AMPSettlement`:
  - Per‑match state machine: `Pending`, `InProgress`, `AwaitingResult`, `Settled`, `Disputed`.
  - Stake escrow on match creation/join.
  - Events for lifecycle changes.

### Day 10: Contract Tests
- Hardhat/Foundry tests:
  - Register a game.
  - Create and join match with stake.
  - Edge cases: missing registration, wrong stake, duplicate joins.
- Add basic gas snapshots for common paths.

### Day 11–12: Verifier Service Skeleton
- Set up Rust/Go project:
  - HTTP/gRPC server (simple REST is fine for MVP).
  - Load Cap’n Proto schemas, implement encode/decode for transcripts.
  - Implement endpoint `/verifyTranscript`:
    - Accepts binary transcript + metadata.
    - Returns stubbed result `{winner: player0, hash: <computed>, signature: <dummy>}`.
- Integrate signing:
  - Store verifier keypair in config.
  - Sign `(matchId, transcriptHash, winner)`.

### Day 13–14: Contract–Verifier Integration Path
- Implement Solidity signature verification:
  - For verifier (single address).
  - For players (EOA signatures on transcript hash).
- Write tests:
  - Happy path: async mode with verifier signature.
  - Happy path: RT hash mode with both players signing same hash.
  - Dispute flow: mismatched hashes → mark `Disputed` and allow verifier override.

---

## Week 3 – SDK, Reference Game, and End‑to‑End Demo

### Goals
- Play one full game that creates a match, plays, generates transcript, and settles on‑chain.
- Have a working MVP demo by end of week.

### Day 15–16: AMP SDK Implementation
- TS SDK:
  - Config: RPC URL, contract addresses, signer.
  - `registerGame` (admin), `createMatch`, `joinMatch`, `listenMatchEvents`.
  - `submitAsyncResult(transcript, verifierSignature)` helper.
  - `submitRealtimeResult(transcriptHash, signatures)` helper.
- Helper functions:
  - Cap’n Proto encoding for `MatchTranscript` (via wasm or JS bindings to generated code / prebuilt pipeline).
  - `computeTranscriptHash(transcript)` → keccak256 of bytes.

### Day 17–18: Reference Game – Minimal MVP
- Build a super simple PvP game:
  - Example: “Click duel”:
    - Each player clicks as many times as possible in 10 seconds.
    - Inputs: “click” events with timestamps.
    - Winner: most valid clicks.
  - For async mode:
    - Client sends all inputs to verifier after match.
    - Verifier reconstructs winner.
  - For RT mode:
    - Both clients/host build transcript in real time, compute final hash, sign result.
- Integrate SDK:
  - Lobby: “Find match” calls `createMatch` / `joinMatch`.
  - On match end, call proper settlement function.

### Day 19: End‑to‑End Wiring
- Hook game client → SDK → contracts → verifier:
  - Async path:
    - Game posts transcript to verifier.
    - Verifier responds with signed result.
    - Client calls `submitResult` on `AMPSettlement`.
  - RT path:
    - Clients compute transcript hash and sign.
    - One client submits on‑chain with both signatures.
- Verify flows on a testnet:
  - Hardhat/Anvil local, then Avalanche Fuji.

### Day 20–21: Demo Hardening
- Script the demo:
  - Show registry, create and join match, play match, see settlement + payouts.
- Add basic UI for transaction status and error handling.
- Record an internal demo run and fix any obvious UX/happy‑path issues.

---

## Week 4 – Polish, Multi‑Game Story, and Docs

### Goals
- Make AMP look like infra reusable by others.
- Solid docs and improved UX.

### Day 22–23: Developer Docs
- `amp-docs`:
  - Getting started: deploy contracts, run verifier, integrate SDK.
  - “Integrate AMP in a day” guide using the reference game.
  - API reference: contracts ABIs, SDK functions, verifier endpoints.
- Add diagrams:
  - Async mode sequence diagram.
  - RT hash mode sequence diagram.

### Day 24–25: UX and Dev Tooling
- Improve reference game UI:
  - Clear lobby and “in match” states.
  - Visual indicator when settlement tx is mined; show payout result.
- Basic dev dashboard (could be simple Next.js page):
  - List registered games.
  - Show recent matches and their states (pending, in progress, settled, disputed).

### Day 26: Multi‑Game Integration Story
- Design a second hypothetical integration:
  - Example: turn‑based tactics game; show how only the rules engine + transcripts change.
- Add doc section “Integrating a New Game”:
  - Define rules, encode events, wire verifier, use existing SDK calls.

### Day 27–28: Testing and Hardening
- Property/edge tests for contracts:
  - Re‑entrancy, replay attacks on signatures, invalid states.
- Chaos tests:
  - Missing verifier response → timeout & refund strategy (MVP: simple manual admin override or long timeout).
- Capture metrics:
  - Gas cost per match create/join/settle.
  - Latency end‑to‑end.

---

## Week 5 – GTM, Metrics, and Judge‑Friendly Story

### Goals
- Show traction, or at least strong pipeline.
- Build a “why this wins post‑competition” narrative.

### Day 29–30: GTM Plan
- Define target users:
  - Avalanche indie game teams, on‑chain game jams, hackathon projects.
- Design AMP as “infra as a service”:
  - Free for early adopters; later, protocol fee on stakes.
- Outline 6–12 month roadmap:
  - More settlement modes, tournaments, ranking, subnet migration.

### Day 31–32: Outreach and Early Adopters
- Reach out to:
  - Existing Avalanche game builders and communities.
  - Hackathon organizers / gaming discords.
- Aim for:
  - One committed early integrator.
  - A couple of “interested if you win” conversations.
- Capture quotes or informal commitments for your deck (if possible).

### Day 33–34: Metrics and Analytics
- Add simple analytics:
  - Count matches created, completed, disputed.
  - Track number of unique players (addresses).
- Run a small internal/closed alpha:
  - Friends/colleagues run through match flows.
  - Collect feedback on UX and stability.

### Day 35: Refine Pitch and Deck
- Build short deck:
  - Slide 1–3: Problem, solution, why Avalanche.
  - Slide 4–5: Architecture and demo screenshots.
  - Slide 6–7: Traction, GTM, roadmap.
- Practice 5–7 minute pitch:
  - Hit problem → solution → demo → traction → future.

---

## Week 6 – Final Polish and Presentation

### Goals
- Zero‑surprise demo.
- Clear, confident story tailored to judges.

### Day 36–37: Demo Rehearsals and Guardrails
- Run the live demo multiple times on Fuji:
  - With fresh wallets, realistic network conditions.
- Create a fallback:
  - Pre‑recorded video in case of live infra failure.
  - Script to quickly reseed/fund accounts.

### Day 38: Security and Risk Slide
- Prepare concise explanation of:
  - Trust assumptions (verifier trust, player honesty).
  - Dispute resolution logic.
  - Upgrade path for contracts (if any).
- Be ready with answers about:
  - Why settlement is off‑chain replay + on‑chain commitments (and why that’s right for real‑time games).

### Day 39: Tighten Messaging
- Refine key lines:
  - “AMP is to Avalanche PvP games what payments rails are to ecommerce.”
  - “Integrate matchmaking and on‑chain settlement in a day.”
- Make sure the story emphasizes:
  - Multi‑game, multi‑studio applicability.
  - Fit with Avalanche’s push for on‑chain gaming infra.

### Day 40–42: Final Checks and Rest
- Freeze features; only fix critical bugs.
- Sanity check:
  - Contracts verified on explorer.
  - Links to GitHub, docs, and live demo ready.
- Sleep, do light rehearsal, and go in as rested and calm as possible.

---
