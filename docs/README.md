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

### Matchmaking & Verification Service (Rust + Cap’n Proto)

Located in `/match_maker`.

- **Cap’n Proto RPC Interfaces** (defined in `/schemas`):
  - **GameConnector**:
    - `requestGameService`: Entry point for clients. Validates player pools, spawns matches with unique UUIDs, and returns a `MatchAssignment`.
  - **MatchMaker**:
    - `verifyAsyncReplay`: Capability-based interface for turn-based transcript verification.
    - `verifyRealTimeTranscript`: Placeholder for real-time agreement modes.
- **Networking**: 
  - Supports high-performance TCP rpc-twoparty connections.
  - Robust client logic with connection retries and error handling.
- **Match Lifecycle**: 
  - Dynamic match spawning with collision-resistant IDs.
  - Capability delivery: clients receive a direct object reference (`MatchMaker`) to settle their specific match.

MVP ships with a turn-based matchmaking client (`/mm-client`) demonstrating the full handshake and capability usage.

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
/match_maker    # Rust matchmaking & verification service (Cap'n Proto RPC)
/mm-client      # Rust example client for matchmaking service
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

3. **Match Maker – build**

```bash
cd ../match_maker
cargo build
# To run: AMP_ADDR=0.0.0.0:50051 cargo run
```

4. **Client – build**

```bash
cd ../mm-client
cargo build
# To run: cargo run -- 127.0.0.1:50051
```

5. **SDK – build**

```bash
cd ../sdk/js
npm install
npm run build
```

6. **Web demo – run**

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

**MVP Integration & Polish**

- [x] **Matchmaking & Capability Core**: Bridge TCP clients to the matchmaking service with UUID-based spawning and capability delivery.
- [ ] **EIP-712 Signing**: Implement cryptographic signing of match results in `mm_impl.rs` using a secure private key.
- [ ] **On-Chain Bridge**: Connect the `match_maker` results to the `AMPSettlement` contract to complete the payout loop.
- [ ] **Deterministic Simulation**: Replace verifier placeholders with actual game-specific simulation logic (e.g., card game rules, physics-lite).
- [ ] **Security**: Implement TLS and client authentication for the Cap'n Proto RPC layer.

**Post‑MVP Roadmap**

- [ ] **Real-Time Mode**: Fully implement the `RT_HASH_AGREE` production-grade flow for low-latency games.
- [ ] **ELO & Queues**: Add sophisticated matchmaking queues with ELO-based pairing and region affinity.
- [ ] **Advanced Anti‑Cheat**: Integrate machine learning or heuristics-based anti-cheat signals into the verification pipeline.
- [ ] **Avalanche Subnet**: Design the migration path to a dedicated AMP-centric Avalanche gaming subnet for ultra-low fees and high throughput.

---

## Contributing

Issues and PRs are welcome. For now, contributions are focused on:

- New game integrations (async first).
- Verifier implementations for specific genres.
- SDK ergonomics and docs.

Please open an issue describing your use case before large changes.