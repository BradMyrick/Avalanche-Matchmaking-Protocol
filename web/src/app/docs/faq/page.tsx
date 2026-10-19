import Link from "next/link";
import { ChevronDown } from "lucide-react";
import SiteHeader from "@/app/components/SiteHeader";
import { PROTOCOL_FEE_LABEL } from "@/lib/ampCup";

export const metadata = {
  title: "AMP FAQ — Tournament Protocol Questions",
  description: "What AMP is, how cups work, fees, claims, refunds, security, and the road to mainnet — answered thoroughly.",
};

const groups: { title: string; faqs: { q: string; a: string }[] }[] = [
  {
    title: "Basics",
    faqs: [
      {
        q: "What is AMP, in one paragraph?",
        a: "AMP is an open protocol for running prize tournaments inside any game, built on Avalanche. A sponsor funds a prize pool into a smart contract (the AMPTournamentCup), your game runs the tournament bracket using an SDK, a verifier wallet cryptographically signs the final standings, and winners claim their prize directly from the contract with their own wallet. There is no custodian: not you, not us — the contract holds the money and pays out only against the verifier's signature.",
      },
      {
        q: "Who is AMP for?",
        a: "Game studios and organizers who want real stakes — cash-equivalent prizes, community cups, seasonal ladders — without building escrow, payout plumbing, or a payments backend. Also sponsors (DAOs, guilds, brands) who want to fund a verifiable prize pool for someone else's community.",
      },
      {
        q: "What do I actually need to run a tournament?",
        a: "Three things: (1) AVAX on Fuji testnet for the prize pool and gas, (2) a verifier wallet you control — this is the key that signs final results, and (3) a way to decide results: your game's own match data through the SDK, or the bracket console on this site. Players need any EVM wallet (Core, MetaMask) and a little AVAX for the claim transaction.",
      },
      {
        q: "Do I need to run a server?",
        a: "No. The SDK talks directly to Avalanche RPC and the cup contract — that's the whole protocol. This website is an optional convenience: a no-code cup launcher, a bracket console, and a claim portal. If you use your own verifier key signing in-game results, you never need us at all.",
      },
      {
        q: "Which engines are supported?",
        a: "TypeScript/JavaScript (@amp/sdk on npm), Unity, Unreal Engine 5 (Blueprints and C++), C#/.NET, and Rust — plus a plain REST surface on this site for anything else. The bracket engine is deterministic and identical across SDKs, so a result computed in one SDK matches every other.",
      },
    ],
  },
  {
    title: "Running a tournament",
    faqs: [
      {
        q: "How does a cup work, end to end?",
        a: "1) Create: the sponsor calls createTournament with a payout split (e.g. 60/30/10), a verifier address, and a finalize deadline, funding it with AVAX or an ERC-20. 2) Play: your game runs the bracket with the SDK — pairings, byes, and standings are computed locally. 3) Finalize: the verifier wallet signs the final placements (EIP-712); anyone submits that signature on-chain before the deadline. 4) Claim: each winner calls claimPrize from their wallet — in-game or at /claim. The pool pays out per the split, minus the protocol fee.",
      },
      {
        q: "What is the verifier, and who should hold that key?",
        a: "The verifier is the single address authorized to sign final standings — it's set once at cup creation and cannot be changed. Treat it like the prize money itself: it should be an organizer/ops wallet kept in a secret manager, never in a client bundle or a repo. If you're running honest events, the verifier signs whatever the bracket engine computed from real match results.",
      },
      {
        q: "How are payouts split?",
        a: "You choose, in basis points, at creation: index 0 is 1st place, the split must sum to exactly 100%, and up to 16 paid placements are allowed. Example: [6000, 3000, 1000] pays 60/30/10. Winner-takes-all is [10000].",
      },
      {
        q: "What happens if the tournament never finishes — or the verifier key is lost?",
        a: "Every cup has a finalize deadline set at creation. If it passes without a finalization, cancelTournament refunds the entire prize pool to the sponsor — callable by the sponsor, or by anyone once the deadline has lapsed. Nobody's funds can be trapped in an abandoned cup. This is also why you should keep the verifier key recoverable (and why the deadline should reflect a realistic event length).",
      },
      {
        q: "Can results be disputed?",
        a: "On-chain, the contract accepts only the verifier's signature — it deliberately has no opinion about who won. Disputes are an organizer concern, resolved before finalization using your own evidence (match logs, replays, referee rulings). Once finalized, placements are immutable; that's the trade: you get instant, trustless payouts in exchange for finalizing only after you're confident in the standings.",
      },
      {
        q: "What's the refund logic during an event?",
        a: "Before the deadline and before finalization, the sponsor can cancel and recover the full pool. After finalization there are no refunds — prizes belong to the attested winners, claimable whenever they like (there's no claim deadline).",
      },
      {
        q: "Can players report their own results?",
        a: "Yes — the hosted console supports player-signed match reports (players sign with their wallets, and both sides confirm) for organizing brackets without custom code. When you integrate the SDK in-game, you'll usually skip that and feed your authoritative match data straight into the bracket engine.",
      },
    ],
  },
  {
    title: "Money",
    faqs: [
      {
        q: "What does AMP cost?",
        a: `A ${PROTOCOL_FEE_LABEL} protocol fee on each prize payout, taken on-chain at claim time (that's protocolFeeBps on the contract — currently 200 bps on Fuji). Plus normal Avalanche gas for the handful of transactions: create, finalize, and each claim. Nothing else — no subscriptions, no listing fees, no cut of your entry fees, because AMP never touches entry fees.`,
      },
      {
        q: "Who holds the prize money?",
        a: "The AMPTournamentCup contract, for the life of the event. Not the sponsor's wallet, not AMP, not a relayer. Payouts leave only via winner pull-claims or the sponsor refund path — the contract has no other exits.",
      },
      {
        q: "How do winners get paid?",
        a: "Pull-claim: after finalization, a winner calls claimPrize(tournamentId, placement) from the wallet that was attested as a winner. The contract sends their share immediately. In-game, that's one SDK call; without a game, it's playwithamp.xyz/claim with just a connected wallet.",
      },
      {
        q: "Can I fund cups with a card instead of crypto?",
        a: "Yes — the cup launcher on this site accepts card payments (PayPal) for the prize pool; a relayer converts the payment into an on-chain funding transaction. Card funding is sandbox-only while AMP is on testnet. Direct AVAX or ERC-20 funding needs no relayer at all.",
      },
      {
        q: "Do players need crypto to participate?",
        a: "To play, no — entry is your game's concern. To claim a prize, yes: the winner's wallet makes one transaction, which needs gas (a fraction of AVAX). A sponsor can't push prizes on a winner's behalf — pull-claims are a security feature.",
      },
    ],
  },
  {
    title: "Security & trust",
    faqs: [
      {
        q: "What's the security model?",
        a: "The cup contract is small, source-verified on Snowtrace, and built on OpenZeppelin primitives: reentrancy guards, pull-only payments, pausable, two-step ownership. Attestations are EIP-712 signatures, so browsers, servers, and the contract all compute identical digests. Prize accounting is exact: balances are measured at deposit (fee-on-transfer tokens are handled correctly) and every wei is either paid out or refunded.",
      },
      {
        q: "Has the contract been audited?",
        a: "Not yet by an external firm. The contract has a Forge test suite (unit + conservation invariants), is source-verified, and an external audit plus timelock governance are prerequisites we've set for ourselves before any mainnet deployment. Until then: treat Fuji as what it is — testnet with no real value at risk.",
      },
      {
        q: "Can AMP rug a cup?",
        a: "No. The contract has no owner path to prize funds — ownership administers the protocol fee and pause, never payouts. Once a cup is created, its money moves only by the rules set at creation: verifier-signed finalization, winner claims, sponsor refund.",
      },
      {
        q: "Is this gambling?",
        a: "AMP is prize infrastructure for skill-based competition — brackets where outcomes come from playing. It's not a lottery, casino, or betting protocol, and there's no AMP token. That said, prize competitions can carry legal obligations that vary by jurisdiction and prize size; organizers are responsible for their own events' compliance.",
      },
    ],
  },
  {
    title: "Platform & roadmap",
    faqs: [
      {
        q: "Testnet or mainnet?",
        a: "Fuji testnet today — every address, balance, and claim on this site is testnet value. Mainnet goes live only after an external audit, timelock governance on the contract, and updated terms. When it does, the addresses change and everything you built against the SDK keeps working by swapping config.",
      },
      {
        q: "Can I deploy the contract myself?",
        a: "Yes. AMPTournamentCup is Apache-2.0 Solidity with no Avalanche-specific dependencies — deploy it on any EVM chain (including an Avalanche L1) and point the SDKs at your address. You lose the hosted console and card funding, keep everything else.",
      },
      {
        q: "What was removed from AMP? I remember matchmaking and multiplayer escrows.",
        a: "Earlier iterations included a 1v1 ranked-matchmaking server and an N-player escrow contract. Those were experiments ahead of product clarity; AMP is now focused entirely on tournaments. The old contracts remain source-verified on Fuji with no user funds, but they're not part of the product and not the path forward.",
      },
      {
        q: "Where do I go deeper?",
        a: "The Quickstart has the full five-step integration with real code, the SDK Reference documents every call across engines, and the repo (github.com/bradmyrick/Avalanche-Matchmaking-Protocol) has the contracts, SDKs, and an agent skill file (SKILL.md) so your AI coding agent can do the integration with you.",
      },
    ],
  },
];

export default function FaqPage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <SiteHeader />
      <div className="relative z-10 max-w-3xl mx-auto px-6 pt-24 pb-16">
        <div className="mb-10 text-center">
          <Link href="/docs" className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors">
            ← Docs
          </Link>
          <h1 className="text-4xl font-black uppercase tracking-tight mt-4 mb-2">
            Questions
          </h1>
          <p className="text-sm text-zinc-500">
            Everything people actually ask before running a cup.
          </p>
        </div>

        <div className="space-y-10">
          {groups.map((group) => (
            <section key={group.title}>
              <h2 className="text-xs font-black uppercase tracking-widest text-brand-cyan mb-3 px-1">
                {group.title}
              </h2>
              <div className="space-y-3">
                {group.faqs.map((faq, i) => (
                  <details
                    key={i}
                    className="glass-panel rounded-2xl border border-white/10 overflow-hidden group"
                  >
                    <summary className="flex items-center justify-between cursor-pointer px-6 py-4 hover:bg-white/5 transition-colors list-none">
                      <span className="font-bold text-sm md:text-base">{faq.q}</span>
                      <ChevronDown className="w-4 h-4 text-zinc-500 shrink-0 ml-4 group-open:rotate-180 transition-transform" />
                    </summary>
                    <div className="px-6 pb-5 pt-0">
                      <p className="text-sm text-zinc-400 leading-relaxed">{faq.a}</p>
                    </div>
                  </details>
                ))}
              </div>
            </section>
          ))}
        </div>

        <div className="mt-12 text-center space-y-2">
          <p className="text-sm text-zinc-500">Ready to integrate?</p>
          <Link href="/docs/quickstart" className="text-sm text-brand-cyan hover:underline font-bold">
            Read the Quickstart Guide →
          </Link>
        </div>
      </div>
    </div>
  );
}
