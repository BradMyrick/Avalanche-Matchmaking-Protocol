# AMP – Avalanche Matchmaking & Settlement Protocol

AMP (Avalanche Matchmaking & Settlement Protocol) is a **general‑purpose on‑chain match and settlement layer for games**.

Any game that can produce a deterministic proof of what happened—a replay, an event transcript, or an oracle result—can use AMP to:

- Discover and match players
- Escrow stakes and rewards
- Verify outcomes via off‑chain verifiers
- Settle payouts and publish results on Avalanche C‑Chain

Gameplay stays where it belongs (your engine, your servers). AMP standardizes the **game ↔ chain** boundary so you don’t have to rebuild matchmaking, escrow, and settlement for every title.

> **Status:** [x] MVP COMPLETED successfully for Avalanche Build Games 2026.

***

## What is AMP?

Most Web3 and on‑chain games reinvent the same stack: custom lobbies, bespoke escrow contracts, ad‑hoc result reporting, and half‑trusted backends. AMP replaces that with a **shared, chain‑anchored protocol and Rust service** that any game can plug into.

At a high level:

- **Generic core, specific adapters**
  - AMP defines a small, game‑agnostic core: `GameId`, `MatchId`, `PlayerId` (wallet), `Stake`, `TranscriptHash`, `Outcome`, and `Verifier`.
  - Each game (or game type) registers a config and plugs in a verifier adapter that knows how to interpret its replays or transcripts.

- **Cap’n Proto schemas as the “language of games”**
  - Game clients speak typed Cap’n Proto messages to the AMP Rust service.
  - The service handles matchmaking, verifies transcripts/replays, and performs on‑chain actions on Avalanche.

- **Off‑chain simulation, on‑chain truth**
  - Matches run off‑chain (Unity, Unreal, Godot, Rust, JS, mobile, etc.).
  - AMP receives only hashes, transcripts, and signed outcomes, and enforces the economic truth on C‑Chain.

- **Built for Avalanche first**
  - Contracts and tooling target Avalanche C‑Chain (Fuji → mainnet) for fast, cheap settlement.
  - The architecture leaves room to move heavy traffic to an AMP‑centric gaming subnet later.

Think of AMP as **“one infra layer for match lifecycle + value flow across any game that can prove its outcomes.”**

***

## Core Architecture

AMP consists of **on‑chain smart contracts**, a **Rust matchmaking + verification service**, and **SDKs + schemas** that game developers integrate.

### On‑chain: Solidity contracts (Foundry)

Located in `/contracts`.

- **AMPRegistry**
  - Registers games with a minimal, generic config:
    - `gameId`, `rulesType` (e.g., TURN_BASED, REALTIME, ORACLE), `settlementMode`, verifier keys, stake token(s), and basic limits.
  - Creates and joins matches with escrowed stakes (AVAX or ERC‑20).
  - Emits:
    - `GameRegistered(gameId, rulesType, verifier)`
    - `MatchCreated(matchId, gameId, players, stake)`
    - `MatchJoined(matchId, player)`

- **AMPSettlement**
  - Receives match outcomes and verifies signatures from registered verifiers and/or players.
  - Supports multiple **settlement modes** (see below).
  - Executes payouts, fee distribution, and dispute resolution.
  - Emits:
    - `MatchSettled(matchId, outcome, stakeToken, amounts)`
    - `MatchDisputed(matchId, reason)`

Contracts are built and tested with Foundry and deploy first to **Avalanche Fuji**, then mainnet C‑Chain.

***

### Off‑chain: Rust Matchmaking & Verification Service

Located in `/match_maker`.

This is the “brain” of AMP. Game clients never talk to contracts directly; they talk to this service via Cap’n Proto RPC.

- **Cap’n Proto RPC interfaces** (schemas live in `/schemas`):
  - **GameSessionService** (high‑level entry point for clients)
    - `requestMatch(GameMatchRequest) -> MatchAssignment`
    - `submitOutcome(OutcomeSubmission) -> SettlementStatus`
  - **MatchSession** (capability bound to a single match)
    - Methods to stream transcripts, send final replays, query status, etc.

- **Player pools & matchmaking**
  - Maintains queues keyed by `gameId`, `rulesType`, stake range, basic rating band, and optional region.
  - When compatible players are found, it:
    - Creates an AMP match on‑chain via `AMPRegistry`.
    - Returns a `MatchAssignment` with match details and an `Opponent` descriptor (wallet, profile, metadata).

- **Verification pipeline**
  - For each `rulesType` or game, loads a verifier module:
    - Turn‑based: re‑simulate a `TurnBasedReplay` to compute a deterministic `Outcome`.
    - Real‑time: validate a `RealtimeTranscript` and its hash.
    - Oracle‑driven: validate an `OracleResult` signature.
  - Signs the outcome (EIP‑712) and calls `AMPSettlement` to finalize.

- **Networking**
  - High‑performance TCP Cap’n Proto RPC (`rpc-twoparty`) with robust reconnection and error handling.
  - Capability‑oriented: each player gets a capability pointing to their current `MatchSession`, not a global singleton.

MVP ships with a fully working turn‑based matchmaking flow and verifier stub.

***

### Schemas & Game Types

Located in `/schemas`.

AMP uses Cap’n Proto schemas to define both the **generic protocol surface** and **game‑type‑specific data**.

Examples:

- **Generic protocol types**
  - `GameMatchRequest`: identifies the game, player, desired stake, rules type, and optional rating band.
  - `MatchAssignment`: match id, opponent(s), initial configuration, and a capability to the `MatchSession`.
  - `OutcomeSubmission`: includes `matchId`, `Outcome`, and optional replay/transcript references.
  - `Outcome`: generic win/lose/draw, scores, and arbitrary metadata.

- **Turn‑based game type**
  - `TurnBasedReplay`: `initialState`, ordered `moves[]`, `rngSeed`, and metadata.
  - Verifier replays this and produces an `Outcome`.

- **Real‑time game type**
  - `RealtimeTranscript`: ordered `events[]`, `ticksPerSecond`, plus metadata.
  - Clients also submit `transcriptHash` for fast agreement; full transcript is only required on dispute.

- **Oracle‑driven game type**
  - `OracleResult`: external match id, winner(s)/scores, and oracle signature.
  - Enables integrations where the authoritative result comes from an existing backend or external system.

Games can use these schemas directly or extend them with custom metadata while still conforming to the generic `Outcome` and transcript/replay patterns that AMP understands.

***

### SDKs & Web Demo

- **C++ and C# SDKs** (in progress)

  - C++ for Unity and Unreal games.
    currently works via ./test_mvp.sh
    must build cpp test file using the following command:
    ``` bash
        cd ./sdk/cpp
        cmake --build build --target amp_test -j $(nproc)       
        cd ../..

        ./test_mvp.sh 
    ```

  - C# for Unity and .NET games.



***

## Settlement Modes

AMP is generic over games but opinionated about how results are proven. Settlement modes define that proof and how disputes work.

### 1. ASYNC_REPLAY

Best for deterministic, turn‑based or discrete‑tick games.

- Game records a replay: initial config + ordered inputs.
- Client sends the replay to the AMP service.
- Verifier re‑simulates to a unique `Outcome` and signs it.
- AMPSettlement verifies the signature and settles the match.

### 2. RT_HASH_AGREE

Designed for low‑latency real‑time games.

- Engines stream a `RealtimeTranscript` and compute a `transcriptHash`.
- Fast path:
  - All players submit matching `(transcriptHash, winner)` on‑chain.
  - AMP settles immediately.
- Dispute path:
  - Mismatch or timeout triggers a full transcript review by a verifier.
  - Verifier’s signed `Outcome` becomes final.

MVP may ship with this mode in a simplified or experimental form; the contract interface is designed to support the full flow.

### 3. ORACLE_OUTCOME

For games and systems where the authoritative result comes from an external oracle.

- A registered oracle service signs an `OracleResult` for a `matchId`.
- AMPSettlement verifies the oracle signature and performs payouts.
- Useful for hybrid Web2/Web3 games, fantasy sports, or any external event–driven game.

***


## Repository Layout

/contracts      # Solidity (Foundry) AMPRegistry + AMPSettlement
/match_maker    # Rust matchmaking & verification service (Cap'n Proto RPC)
/mm-client      # Rust example client for the matchmaking service
/sdk/js         # TypeScript SDK (AMPClient) for web and Node games
/sdk/cpp        # C++ SDK and example client (amp_test)
/web            # Avalanche-branded React + Tailwind web demo
/schemas        # Cap'n Proto schemas: generic protocol + game-type schemas
/docs           # Design docs, settlement mode specs, integration guides

---

## Quickstart (Local Dev)

> **Note:** These commands describe the current MVP developer workflow from a clean clone to a passing `./test_mvp.sh`.

Prereqs: Foundry, Rust, Node, and `capnp` installed.

1. **Clone the repo**

git clone https://github.com/BradMyrick/Avax-Build-Games-2026.git  
cd Avax-Build-Games-2026  
# (optional) git checkout amp-mvp-skeleton

2. **Contracts – install deps, build**

forge install OpenZeppelin/openzeppelin-contracts --no-commit  

forge clean  
forge build

3. **Generate Cap’n Proto C++ schemas**

capnp compile \
  -oc++:sdk/cpp/src/schemas \
  --src-prefix=schemas \
  schemas/game_types.capnp \
  schemas/match.capnp \
  schemas/service.capnp

4. **C++ SDK – build**

cd sdk/cpp  
rm -rf build  
cmake -S . -B build -DCMAKE_BUILD_TYPE=Release  
cmake --build build --config Release  
cd ../..

5. **Wire C++ test client for the MVP script**

mkdir -p build  
ln -sf ../sdk/cpp/build/amp_test build/amp_test

6. **Rust services & JS SDK (optional for local hacking)**

# Match maker  
cd match_maker  
cargo build  
cd ..  

# Example Rust client  
cd mm-client  
cargo build  
cd ..  

# JS SDK  
cd sdk/js  
npm install  
npm run build  
cd ../..

7. **Web demo (optional)**

cd web  
npm install  
npm run dev  
# Open http://localhost:5173  
cd ..

8. **End-to-end MVP test**

From repo root:

./test_mvp.sh

This will:
- Start a local Anvil chain  
- Deploy AMP contracts  
- Start the Rust matchmaker  
- Run the C++ SDK test client  
- Run the JS simulator and verify on-chain settlement

## Integrating Your Game with AMP

Your game can integrate with AMP if it can do three things:

1. **Produce a deterministic proof of the match**

   - Turn‑based: a replay (initial state + ordered moves + RNG seed).
   - Real‑time: an ordered event transcript or a transcript hash plus stored logs.
   - Oracle: an externally signed outcome.

2. **Send that proof via Cap’n Proto / SDK**

   - Use the provided schemas from `/schemas` for your game type.
   - Use the AMP SDK (or generated Cap’n Proto stubs) to:
     - Request matches and join queues.
     - Send replays or transcripts to the Rust service.
     - Receive match assignments and settlement results.

3. **React to settlement**

   - Listen for `MatchSettled` events on‑chain or via the SDK.
   - Update in‑game UI, unlock rewards, or trigger off‑chain services based on the final `Outcome`.

Detailed “Integrate AMP in a Day” recipes and code examples will live in `/docs` as the MVP solidifies.

***

## Roadmap

**MVP**
The initial MVP targets **turn-based `ASYNC_REPLAY` games**

- [x] Matchmaking & capability core: player pools, match spawning, Cap’n Proto RPC, and example client.
- [x] EIP‑191/712 signing: secure signing of match outcomes in the Rust service.
- [x] On‑chain bridge: wire verifier outputs into `AMPSettlement` for end‑to‑end payouts.
- [x] Deterministic simulation: plug in real game‑specific verifiers (e.g., card/tactics game rules, simple action game).
- [x] Security: add authentication for the matchmaker service.

**Post‑MVP**

- [ ] Production‑grade RT_HASH_AGREE for fast real‑time games.
- [ ] Rich matchmaking: ELO/Glicko ratings, queues with region/latency filters, parties.
- [ ] Advanced anti‑cheat: heuristics and ML‑style signals feeding into verifiers.
- [ ] Avalanche gaming subnet: design a path to an AMP‑centric subnet for higher throughput and ultra‑low fees.
- [ ] More game‑type schemas: fighting games, racing, co‑op PvE with shared rewards, etc.

***
## End-to-End Testing

The entire flow can be verified using the automated test script:

```bash
./test_mvp.sh
```

This script will:
1. Start an Anvil local chain.
2. Deploy the Registry and Settlement contracts using Foundry.
3. Start the `match_maker` Cap'n Proto RPC service over TCP.
4. Run the C++ SDK compiled test client (`sdk/cpp/build/amp_test`).
5. Run the TypeScript simulator (`sdk/js/sim.ts`) to orchestrate the on-chain flow.
6. Provide a 0 exit code on successful on-chain settlement.

***
## Contributing

AMP aims to be **generic infra** for all on‑chain and Web3‑adjacent games that can prove their outcomes. Contributions are especially welcome for:

- New game‑type schemas and verifier modules.
- Game integrations across engines (Unity, Unreal, Godot, JS, Rust).
- SDK ergonomics, dev tools, and documentation.

Before large changes, please open an issue describing:

- Your game type and engine.
- How you plan to represent replays/transcripts.
- What you need from AMP that isn’t covered by existing schemas or settlement modes.
