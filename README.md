# AMP — Tournament Protocol on Avalanche

### Real prize tournaments for any game — fund a pool, run the bracket, winners claim on-chain.

AMP is an open protocol for running prize tournaments inside games. A sponsor
funds a prize pool in AVAX (or ERC-20) into the **AMPTournamentCup** contract,
the game runs the tournament bracket with an SDK, a verifier wallet signs the
final standings (EIP-712), and winners pull-claim their prize directly from
the contract. No custodian, no backend your game depends on.

**Try it:** [playwithamp.xyz](https://playwithamp.xyz) → host a cup, watch a
bracket, claim a prize (Fuji testnet).

## The whole protocol in five steps

```ts
import { AMPTournamentClient, Tournament, TournamentState, Outcome } from "@amp/sdk";

// 1. Sponsor funds the pool: split 60/30/10, 7-day finalize deadline, 1 AVAX
const amp = new AMPTournamentClient().connect(sponsorWallet);
const { tournamentId } = await amp.createTournament(
  [6000, 3000, 1000], verifierAddress, deadlineUnix, parseEther("1.0")
);

// 2. The game runs the bracket — local, deterministic, unit-testable
const bracket = Tournament.new({ kind: TournamentState.SingleElimination }, entrants);
for (const id of bracket.pending()) bracket.record(id, playMatch(id));

// 3. The verifier signs the final standings
const signature = await verifierClient.signFinalize(tournamentId, winnerAddresses);

// 4. Anyone submits the signature on-chain
await amp.finalizeTournament(tournamentId, winnerAddresses, signature);

// 5. Winners pull-claim — in-game or at playwithamp.xyz/claim
const prize = await amp.getClaimablePrize(tournamentId, playerAddress);
if (prize) await amp.claimPrize(tournamentId, prize.placement);
```

Full walkthrough: [Quickstart](https://playwithamp.xyz/docs/quickstart) ·
[SDK Reference](https://playwithamp.xyz/docs/api) ·
[FAQ](https://playwithamp.xyz/docs/faq)

## Building with an AI agent?

AMP ships as a skill file. Point your coding agent at
[`agents/SKILL.md`](agents/SKILL.md) (or grab it at
[playwithamp.xyz/download/SKILL.md](https://playwithamp.xyz/download/SKILL.md))
and it can run the entire integration: installs per engine, the five-step
flow, security rules, test templates, and a definition of done.

## Deployed contract (Fuji testnet)

| Contract | Address | Role |
|:---|:---|:---|
| `AMPTournamentCup` | [`0x7C743c1C9AE3e7A65d030098F2249B7787d66dfF`](https://testnet.snowtrace.io/address/0x7C743c1C9AE3e7A65d030098F2249B7787d66dfF) | Prize escrow: sponsor funds, verifier attests, winners pull-claim |

Protocol fee: 2% (200 bps) per payout, read live via `protocolFeeBps()`.
Manifest: [`contracts/deployment-fuji-tournament.json`](contracts/deployment-fuji-tournament.json).

## Repo layout

```
├── contracts/         # Foundry: AMPTournamentCup (Forge tests + invariants)
├── web/               # playwithamp.xyz — cup launcher, bracket console, claim portal, docs
├── relayer/           # custody relayer: card-funded cups (PayPal → on-chain funding)
├── agents/SKILL.md    # AI-agent integration playbook
├── migrations/        # Postgres schema for the hosted console
├── amp-sdk-* (../)    # sibling SDK repos: TypeScript, Unity, Unreal, C#, Rust
└── docs/              # design records + legacy notes
```

> Legacy: `amp-server/` and `amp-match-core/` are the remains of an earlier
> matchmaking/multiplayer direction, retained for reference. They are not part
> of the tournament product and not on the integration path. The old
> `AMPMultiplayer` / `AMPRegistry` / `AMPSettlement` contracts on Fuji are
> retired experiments (source-verified, no user funds) — target
> `AMPTournamentCup` only.

## Security model

- **Pull-payments only** — prizes move when winners call `claimPrize`; the
  contract has no push path to player funds.
- **Refund safety** — every cup has a finalize deadline; unfinalized cups are
  sponsor-refundable, so funds can never be trapped in an abandoned event.
- **EIP-712 attestations** — identical digests in Solidity, Rust, and the
  browser; the verifier key is the only key that can release prizes.
- **Exact accounting** — deposits measured on-chain (fee-on-transfer safe);
  Forge unit tests + conservation invariants; source-verified on Snowtrace.
- **Road to mainnet** — external audit + timelock governance are prerequisites
  before any mainnet deployment. Fuji only until then.

## Run the pieces

```bash
# contracts (Foundry)
cd contracts && forge test

# web (Next.js)
cd web && npm install && npm run dev

# relayer (custody — card funding; never co-deploy with web)
cd relayer && cargo run --release
```

## License

Apache License 2.0. See [LICENSE](LICENSE).
