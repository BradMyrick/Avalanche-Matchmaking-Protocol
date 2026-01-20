# AMP – Avalanche Matchmaking Protocol

AMP (Avalanche or Async Matchmaking Protocol) is a shared on‑chain matchmaking and settlement layer for PvP games on **Avalanche C‑Chain**. It lets any game that can output deterministic replays or event logs escrow stakes, resolve matches via verifiers, and execute trustless payouts through a simple SDK—while keeping real‑time gameplay off‑chain.

> **Status:** MVP in active development for Avalanche Build Games 2026.

---

## What is AMP?

Most Web3 games rebuild matchmaking, escrow, and payouts from scratch, then trust a custom backend to settle matches. AMP turns that into a reusable, chain‑level **protocol**: a set of contracts, verifiers, and SDKs that many games can plug into.[web:47][web:59]

- **Shared infra primitive:** One protocol, many games; each game registers a `gameId` and config, then uses AMP for match lifecycle and settlement.
- **Off‑chain gameplay, on‑chain settlement:** Engines (Unity, Unreal, JS, Rust, etc.) simulate matches off‑chain and emit deterministic replays or event logs. AMP only sees hashes + signatures on Avalanche.
- **Built for Avalanche:** Fast finality and EVM compatibility on C‑Chain now, with a clean path to a dedicated AMP gaming subnet later.

---

## Core Architecture

AMP is a hybrid system: **on‑chain contracts** on Avalanche C‑Chain, **off‑chain verifier services** in Rust, and **TypeScript SDK + web UI** for games and players.

### On‑chain (Solidity, Foundry)

Located in `/contracts`.

- **AMPRegistry**
  - Game registration (`gameId`, `rulesType`, `settlementMode`, verifiers, stake token).
  - Match creation & joining with stake escrow (AVAX or ERC‑20).
  - Emits `GameRegistered`, `MatchCreated`, `MatchJoined`.
- **AMPSettlement**
  - Receives match results and verifies signatures.
  - Supports:
    - `ASYNC_VERIFIER`: off‑chain verifier re‑simulates replay and signs outcome.
    - `RT_HASH_AGREE` (stubbed for MVP): real‑time transcript‑hash mutual agreement with verifier fallback.
  - Handles payouts and protocol fees, emitting `MatchSettled` and `MatchDisputed`.

Contracts use **Foundry** for builds/tests and are designed to deploy on **Avalanche Fuji testnet** first, then mainnet C‑Chain.

### Off‑chain verifier (Rust + Cap’n Proto)

Located in `/verifier`.

- Cap’n Proto schemas in `/schemas` define:
  - `MatchConfig`, `InputFrame`, `GameEvent`, `MatchTranscript`.
- Rust verifier service:
  - Accepts `MatchTranscript` via Cap’n Proto RPC.
  - Re‑simulates or validates the match.
  - Computes a canonical `resultHash` and returns `(winner, outcomeCode, resultHash)`.
  - Signs `(matchId, gameId, winner, outcomeCode, resultHash)` so `AMPSettlement` can verify and settle on‑chain.

MVP ships with a simple async duel game to prove the flow end‑to‑end.

### SDK and Avalanche web demo

- **TypeScript SDK** (`/sdk/js`):
  - `AMPClient` wraps:
    - `createMatch`, `joinMatch` via `AMPRegistry`.
    - `submitAsyncResult`, `submitRealTimeHashResult` via `AMPSettlement`.
    - `onMatchSettled` event subscription.
  - Designed so a game can “integrate AMP in a day”: plug in wallet provider, pass contract addresses, and call these methods from your client or server.

- **Web demo** (`/web`):
  - Vite + React + Tailwind with an **Avalanche‑branded dashboard** (dark theme, Avalanche red accents).
  - Shows:
    - Wallet connection (Fuji).
    - Create/join a staked duel.
    - Trigger off‑chain “play & settle” via verifier stub.
    - Live event log for `MatchSettled` on C‑Chain.

---

## Settlement Modes

AMP supports two settlement modes so both async and real‑time games can plug in:

- **ASYNC_VERIFIER**
  - Turn‑based / async games (card games, tactics, duels).
  - Game records deterministic replay → sends to verifier.
  - Verifier re‑simulates, signs the result, and AMP settles based on that attestation.

- **RT_HASH_AGREE** (v1 interface, extended later)
  - Real‑time games (shooters, brawlers, arena).
  - Engine emits `MatchTranscript`, computes `transcriptHash`.
  - Fast path: both players submit matching `(hash, winner)` → immediate settle.
  - Dispute path: mismatch or timeout → verifier adjudicates using full transcript.

This pattern mirrors emerging on‑chain gaming architectures that keep latency‑sensitive simulation off‑chain while anchoring economic truth on‑chain.

---

## Repo Layout

```text
/contracts      # Solidity (Foundry) AMPRegistry + AMPSettlement
/verifier       # Rust async verifier service (Cap'n Proto RPC)
/sdk/js         # TypeScript AMPClient SDK
/web            # Avalanche-branded React + Tailwind demo
/schemas        # Cap'n Proto schemas for MatchTranscript, etc.
/docs           # Design docs and integration guides
```

Check the `amp-mvp-skeleton` branch for the initial MVP implementation and git history.

---

## Quickstart (Local Dev)

> These steps assume Foundry, Rust, Node, and `capnp` are installed.

1. **Clone and checkout MVP branch**

```bash
git clone https://github.com/BradMyrick/Avax-Build-Games-2026.git
cd Avax-Build-Games-2026
git checkout amp-mvp-skeleton
```

2. **Contracts – build & test**

```bash
cd contracts
forge build
forge test
```

3. **Verifier – build**

```bash
cd ../verifier
cargo build
# Optional: cargo run
```

4. **SDK – build**

```bash
cd ../sdk/js
npm install
npm run build
```

5. **Web demo – run**

```bash
cd ../../web
npm install
npm run dev
# Open http://localhost:5173
```

Wire in your Fuji RPC and deployed contract addresses in the web config to interact with live contracts on Avalanche testnet.

---

## Integrating Your Game with AMP

A game can integrate with AMP if it can:

1. **Produce deterministic match data**
   - Async: replay of config + player inputs.
   - Real‑time: ordered event log / transcript.
2. **Call AMP contracts via SDK**
   - Register or reference a `gameId`.
   - Create/join matches with stakes.
   - Submit results (async attestation or transcript hash).
3. **Listen for settlements**
   - React to `MatchSettled` events for rewards, UI, or off‑chain services.

See `/docs` for “Integrate AMP in a day” recipes and example flows once those guides are published.

---

## Roadmap

**MVP (Build Games Week 3)**

- Full async‑mode pipeline:
  - Game → replay transcript → Rust verifier → signed result → `AMPSettlement` payout.
- Fuji deployment and hosted demo with live matches.

**Post‑MVP**

- Production‑grade `RT_HASH_AGREE` flow.
- Ratings / ranking (Elo/Glicko) and richer matchmaking queues.
- Advanced anti‑cheat signals feeding into verifiers.
- Migration path to an AMP‑centric Avalanche gaming subnet.

---

## Contributing

Issues and PRs are welcome. For now, contributions are focused on:

- New game integrations (async first).
- Verifier implementations for specific genres.
- SDK ergonomics and docs.

Please open an issue describing your use case before large changes.