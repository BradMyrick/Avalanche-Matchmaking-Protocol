# AMP — On-Chain Tournament Skill for AI Agents

> **Real prize tournaments for any game, on Avalanche.**
> One contract holds the money. One SDK runs the brackets. No backend.

You are adding **on-chain tournaments** to a game using the AMP Tournament
Protocol. Follow this file top to bottom. When you finish, the game can fund
an AVAX prize pool, run a bracket, finalize results with an EIP-712
signature, and pay winners directly from the contract — with no AMP server in
the loop.

## What AMP is

- **AMPTournamentCup** (Solidity): sponsor funds a prize pool (native AVAX or
  ERC-20), commits to a payout split in basis points, and names a **verifier**
  address. After the event, the verifier signs the final placements; winners
  **pull-claim** their prize with their own wallet.
- **AMP SDKs**: the same bracket engine + cup client in TypeScript (`@amp/sdk`),
  Unity, Unreal Engine 5, C#, and Rust. The bracket runs locally in the game;
  only fund / finalize / claim touch the chain.
- **This website** (playwithamp.xyz) is an optional hosted console — a no-code
  cup launcher and a claim portal. The protocol does not depend on it.

## Where it lives today

| Thing | Value |
|---|---|
| Chain | Avalanche Fuji testnet (chainId `43113`) |
| AMPTournamentCup | `0x7C743c1C9AE3e7A65d030098F2249B7787d66dfF` (source-verified) |
| EIP-712 domain | `{ name: "AMPTournamentCup", version: "1", chainId, verifyingContract }` |
| Protocol fee | `protocolFeeBps()` on the contract (currently 200 = 2%, taken per payout) |
| Mainnet | Not live yet. Addresses will ship in `contracts/deployment-*.json` in the repo. Fuji first, always. |

## The five-step integration (TypeScript shown; same calls in every SDK)

```ts
import { AMPTournamentClient, Tournament, TournamentState, Outcome } from "@amp/sdk";
import { parseEther } from "ethers";

// 1. Install: npm install @amp/sdk ethers@^6
//    Unity:   Package Manager → Add package from git URL:
//             https://github.com/BradMyrick/amp-sdk-unity.git
//    UE5:     git clone https://github.com/BradMyrick/amp-sdk-cpp.git amp-sdk
//             mv amp-sdk/AmpUnreal <Project>/Plugins/AmpUnreal

// 2. Sponsor funds a prize pool
const amp = new AMPTournamentClient().connect(sponsorWallet);
const { tournamentId } = await amp.createTournament(
  [6000, 3000, 1000],          // payout bps, sums to exactly 10000
  verifierAddress,             // who may sign final results
  Math.floor(Date.now() / 1000) + 7 * 24 * 3600,  // finalize deadline
  parseEther("1.0")            // AVAX prize pool (or createTournamentERC20)
);

// 3. Game runs the bracket — pure, local, deterministic, unit-testable
const bracket = Tournament.new(
  { kind: TournamentState.SingleElimination },
  entrants  // [{ id, seed }, ...]
);
bracket.record(matchId, Outcome.A);
const standings = bracket.standings();   // sorted placements

// 4. Verifier signs the final placements; anyone submits on-chain
const verifier = new AMPTournamentClient().connect(verifierWallet);
const signature = await verifier.signFinalize(tournamentId, winnerAddresses);
await amp.finalizeTournament(tournamentId, winnerAddresses, signature);

// 5. Winners pull-claim (in-game, or at playwithamp.xyz/claim)
const prize = await amp.getClaimablePrize(tournamentId, playerAddress);
if (prize) await amp.claimPrize(tournamentId, prize.placement);
```

## Rules you MUST follow

1. **Derive winners from recorded results — never from client input.** Feed
   match outcomes into the bracket engine and take `standings()`. A client
   that can name winners can rob the prize pool's integrity. The verifier key
   signs only what the bracket computed.
2. **payoutBps must sum to exactly 10000**, every entry `> 0`, at most 16
   placements (index 0 = 1st). The contract rejects anything else.
3. **Protect the verifier key like it's the prize money.** It releases every
   prize. Load it from an env var / secret store on the server, never in the
   client bundle, never in git. Signing happens server-side or in the
   organizer's wallet only.
4. **Always set a `finalizeDeadline`** (creation is rejected otherwise). If an
   event never finalizes, `cancelTournament` refunds the sponsor after the
   deadline — that's the safety valve; wire it into your ops runbook.
5. **Money math in wei/BigInt.** Format to AVAX only for display. Never do
   arithmetic on floats.
6. **Claims are pull-only.** The game's job is to *show* the player their
   `getClaimablePrize()` result and call `claimPrize(tournamentId, placement)`
   with their wallet. Never custodial-send prizes on their behalf.
7. **winners array must match the payout split length** and the EIP-712
   digest's domain (`AMPTournamentCup`, version `1`, the deployed chainId +
   address) — the SDK handles the encoding; don't hand-roll it.
8. **Fuji is testnet.** Nothing of monetary value until mainnet ships (with an
   external audit and timelock governance). Say "testnet" in your UI while on
   chainId 43113, and don't hardcode addresses — read them from config so the
   mainnet swap is one variable.

## Test template (REQUIRED — ship these three cases)

```ts
import { describe, it, expect } from "vitest";
import { Tournament, TournamentState, Outcome } from "@amp/sdk";

describe("tournament integration", () => {
  it("runs a 4-player single-elim bracket to completion", () => {
    const t = Tournament.new(
      { kind: TournamentState.SingleElimination },
      [{ id: 1, seed: 1 }, { id: 2, seed: 2 }, { id: 3, seed: 3 }, { id: 4, seed: 4 }]
    );
    expect(t.pending().length).toBeGreaterThan(0);
    for (const id of t.pending()) t.record(id, Outcome.A);
    const s = t.standings();
    expect(s.length).toBe(4);
    expect(s[0].place).toBe(1);
  });

  it("never finalizes with unresolved matches", () => {
    const t = Tournament.new({ kind: TournamentState.SingleElimination },
      [{ id: 1, seed: 1 }, { id: 2, seed: 2 }]);
    // no results recorded
    expect(() => t.standings()).toThrow(); // or assert incomplete per engine docs
  });

  it("validates payout splits before funding", () => {
    const split = [6000, 3000, 1000];
    expect(split.reduce((a, b) => a + b, 0)).toBe(10000);
    expect(split.length).toBeLessThanOrEqual(16);
  });
});
```

(Unity: EditMode tests against the same cases. UE5: the AmpUnreal spec tests.)

## Contract cheat-sheet

| Call | Who | Effect |
|---|---|---|
| `createTournament(payoutBps, verifier, deadline)` payable | sponsor | funds pool in AVAX, returns tournamentId |
| `createTournamentERC20(token, amount, payoutBps, verifier, deadline)` | sponsor | ERC-20 prize variant (fee-on-transfer safe) |
| `finalizeTournament(id, winners, signature)` | anyone (sig must be from `verifier`) | locks standings, opens claims; only before deadline |
| `claimPrize(id, placement)` | the attested winner, from their wallet | pulls their payout (protocol fee deducted on-chain) |
| `cancelTournament(id)` | sponsor (or anyone past deadline, if unfinalized) | refunds the pool |
| `getTournament(id)` | anyone | sponsor, prize, split, state, winners, deadlines |

## Definition of done for an integration

- [ ] SDK installed for the engine; builds clean.
- [ ] Cup creation wired with a validated payout split + real deadline.
- [ ] Bracket engine fed by actual match results; winners derived from standings.
- [ ] Verifier key in env/secret store; signature produced server-side or by
      the organizer's wallet; address/chainId from config, not hardcoded.
- [ ] Finalize flow submits the EIP-712 signature and succeeds on Fuji.
- [ ] Winner flow: `getClaimablePrize` → `claimPrize` confirmed in a tx.
- [ ] The three test cases above pass in CI.
- [ ] Refund path documented for the operator (cancelTournament after deadline).
