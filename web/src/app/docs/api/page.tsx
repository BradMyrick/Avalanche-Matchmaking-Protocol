import Link from "next/link";
import { Code2, Cpu, Trophy, Workflow } from "lucide-react";
import SiteHeader from "@/app/components/SiteHeader";
import CodeSample from "@/app/components/CodeSample";

export const metadata = {
  title: "AMP SDK Reference — The Five Calls",
  description: "The complete AMP API: five calls run a tournament — createTournament, run the bracket, signFinalize, finalizeTournament, claimPrize.",
};

function ApiMethod({
  signature,
  returnType,
  desc,
  example,
}: {
  signature: string;
  returnType: string;
  desc: string;
  example: string;
}) {
  return (
    <div className="glass-panel rounded-2xl border border-white/10 p-5 mb-5">
      <div className="flex items-center justify-between gap-3 mb-2 flex-wrap">
        <code className="text-sm font-mono text-brand-cyan font-bold">{signature}</code>
        <span className="text-xs font-mono px-2.5 py-0.5 rounded border border-yellow-400/30 bg-yellow-400/10 text-yellow-400">
          {returnType}
        </span>
      </div>
      <p className="text-sm text-zinc-300 mb-3">{desc}</p>
      <div className="bg-black/60 border border-white/10 rounded-xl p-3">
        <CodeSample code={example} />
      </div>
    </div>
  );
}

export default function ApiReferencePage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <SiteHeader />
      <div className="relative z-10 max-w-4xl mx-auto px-6 pt-24 pb-16">
        <div className="mb-12">
          <Link href="/docs" className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors">
            ← Docs
          </Link>
          <div className="inline-flex items-center gap-2 rounded-full border border-brand-cyan/30 bg-brand-cyan/5 px-4 py-1.5 text-xs font-bold uppercase tracking-widest text-brand-cyan mt-6 mb-4">
            <Code2 className="w-3.5 h-3.5" />
            SDK Reference
          </div>
          <h1 className="text-4xl font-black uppercase tracking-tight mb-4">
            The whole protocol is <span className="text-brand-cyan">five calls</span>
          </h1>
          <p className="text-zinc-400 max-w-2xl">
            This page is the entire integration. You create a cup, run the bracket
            locally, sign the result, submit it, and winners claim. The same
            method names exist in TypeScript, Unity, Unreal, C#, and Rust — only
            the syntax changes.
          </p>
        </div>

        {/* Lifecycle map */}
        <div className="glass-panel rounded-2xl border border-white/10 p-6 mb-12">
          <div className="flex items-center gap-2 mb-4">
            <Workflow className="w-5 h-5 text-brand-cyan" />
            <h2 className="text-sm font-black uppercase tracking-widest text-white">
              How the pieces fit
            </h2>
          </div>
          <CodeSample
            code={`sponsor                your game (local)              verifier              winner
   │                        │                            │                    │
   ├─ createTournament ──▶  │                            │                    │
   │   (fund + split +      ├─ Tournament.new ─▶ record ─┤                    │
   │    verifier +          │   (bracket, off-chain)      ├─ signFinalize      │
   │    deadline)           │                            │   (EIP-712 sig)     │
   │                        │                            │        │           │
   │                        │            anyone ─▶ finalizeTournament        │
   │                        │                            │        │           │
   │                        │                            │        └─▶ claimPrize ─▶ AVAX
   │                        │                            │
   └─ cancelTournament ◀─ (refund if never finalized, after deadline)`}
          />
          <p className="text-xs text-zinc-500 mt-4">
            On-chain calls are the three in the middle column edges: create, finalize,
            claim. Everything else is local and free.
          </p>
        </div>

        {/* Tournament Client Methods */}
        <h2 className="text-xl font-black uppercase tracking-tight mb-4 flex items-center gap-2">
          <Trophy className="w-5 h-5 text-brand-cyan" />
          Cup client — on-chain calls
        </h2>

        <ApiMethod
          signature="createTournament(payoutBps, verifierAddress, deadlineUnix, prizePoolAvaxWei)"
          returnType="Promise&lt;{ tournamentId, txHash }&gt;"
          desc="Sponsor funds a new prize pool in AVAX. payoutBps (e.g. [6000, 3000, 1000]) sums to exactly 10000, max 16 placements. ERC-20 prizes: createTournamentERC20(token, amount, …)."
          example={`const { tournamentId } = await amp.createTournament(
  [6000, 3000, 1000],
  verifierAddress,
  deadlineUnix,
  parseEther("1.0")
);`}
        />

        <ApiMethod
          signature="getTournament(tournamentId)"
          returnType="Promise&lt;TournamentDetails&gt;"
          desc="Read everything on-chain about a cup: sponsor, prize pool (prizePoolFormatted), payout split, verifier, winners, state, deadlines. Pure read — no wallet needed."
          example={`const cup = await amp.getTournament(tournamentId);
console.log(cup.state, cup.prizePoolFormatted + " AVAX");`}
        />

        <ApiMethod
          signature="signFinalize(tournamentId, winners)"
          returnType="Promise&lt;string&gt;"
          desc="The verifier signs the final placements (EIP-712). Called with the verifier's wallet connected — this is the only step that requires the verifier key, so it belongs on the server or organizer's machine."
          example={`const verifier = new AMPTournamentClient().connect(verifierWallet);
const signature = await verifier.signFinalize(tournamentId, [first, second, third]);`}
        />

        <ApiMethod
          signature="finalizeTournament(tournamentId, winners, signature)"
          returnType="Promise&lt;TransactionReceipt&gt;"
          desc="Submit the verifier's signature on-chain. Anyone can send this transaction — organizer, game server, relayer. Must happen before the cup's finalize deadline."
          example={`await amp.finalizeTournament(tournamentId, [first, second, third], signature);`}
        />

        <ApiMethod
          signature="getClaimablePrize(tournamentId, walletAddress)"
          returnType="Promise&lt;ClaimableInfo | null&gt;"
          desc="Check a wallet for an unclaimed prize. Returns placement, rank, gross/net amounts, and the fee — or null if nothing to claim. Show this in your winner UI."
          example={`const prize = await amp.getClaimablePrize(tournamentId, player);
if (prize) console.log(\`You won \${prize.formattedAmount} AVAX (rank \${prize.placementRank})\`);`}
        />

        <ApiMethod
          signature="claimPrize(tournamentId, placement)"
          returnType="Promise&lt;TransactionReceipt&gt;"
          desc="Winner pulls their prize from the contract into their own wallet. Needs the winner's signature (gas) — never sponsor-side custody."
          example={`await amp.claimPrize(tournamentId, prize.placement);`}
        />

        <ApiMethod
          signature="cancelTournament(tournamentId)"
          returnType="Promise&lt;TransactionReceipt&gt;"
          desc="Refund path: sponsor cancels an unfinalized cup; after the finalize deadline anyone can. The full pool returns to the sponsor."
          example={`await amp.cancelTournament(tournamentId);`}
        />

        {/* Bracket Engine Methods */}
        <h2 className="text-xl font-black uppercase tracking-tight mb-4 mt-12 flex items-center gap-2">
          <Cpu className="w-5 h-5 text-yellow-400" />
          Bracket engine — local, off-chain
        </h2>
        <p className="text-sm text-zinc-400 mb-6">
          The tournament itself runs inside your game with zero network calls.
          Deterministic and identical in every SDK, so it unit-tests like any
          other code.
        </p>

        <ApiMethod
          signature="Tournament.new(format, entrants)"
          returnType="Tournament&lt;T&gt;"
          desc="Build a bracket from entrants with seeds. Formats: SingleElimination and RoundRobin (plus Swiss pairing helpers). Handles byes for non-power-of-two fields."
          example={`const bracket = Tournament.new(
  { kind: TournamentState.SingleElimination },
  [{ id: 1, seed: 1 }, { id: 2, seed: 2 }, { id: 3, seed: 3 }, { id: 4, seed: 4 }]
);`}
        />

        <ApiMethod
          signature="bracket.matches() / bracket.pending()"
          returnType="BracketMatch[] / MatchId[]"
          desc="All matches, or just the ones still to play. Feed pending() to your matchmaking/queue and record outcomes as games finish."
          example={`for (const id of bracket.pending()) {
  const result = await playMatch(id);
  bracket.record(id, result);
}`}
        />

        <ApiMethod
          signature="bracket.record(matchId, outcome)"
          returnType="void"
          desc="Record a result: Outcome.A (player 1), Outcome.B (player 2), or Outcome.Draw where the format allows it."
          example={`bracket.record(0, Outcome.A);`}
        />

        <ApiMethod
          signature="bracket.standings()"
          returnType="Standing&lt;T&gt;[]"
          desc="Sorted final placements once the bracket resolves. The top of this list (matching your payout split length) is exactly what you pass to signFinalize."
          example={`const standings = bracket.standings();
const winners = standings.slice(0, 3).map(s => walletOf(s.id));
const signature = await verifier.signFinalize(tournamentId, winners);`}
        />

        {/* Back Link */}
        <div className="mt-16 pt-8 border-t border-white/10 flex justify-between items-center text-sm">
          <Link href="/docs/contracts" className="text-zinc-500 hover:text-white transition-colors">
            ← Contracts
          </Link>
          <Link href="/docs/faq" className="text-brand-cyan hover:underline font-bold">
            FAQ →
          </Link>
        </div>
      </div>
    </div>
  );
}
