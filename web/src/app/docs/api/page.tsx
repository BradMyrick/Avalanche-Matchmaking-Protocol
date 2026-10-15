import Link from "next/link";
import { Code2, Terminal, Cpu, FileJson, Shield, Trophy } from "lucide-react";

export const metadata = {
  title: "AMP API & SDK Reference — Tournament Protocol",
  description: "Complete API reference for the AMP Tournament Protocol: Client SDKs, Bracket Engine, and Web API.",
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
      <pre className="bg-black/60 border border-white/10 rounded-xl p-3 text-xs font-mono text-zinc-300 overflow-x-auto">
        <code>{example}</code>
      </pre>
    </div>
  );
}

export default function ApiReferencePage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <div className="relative z-10 max-w-4xl mx-auto px-6 py-16">
        <div className="mb-12">
          <Link href="/docs" className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors">
            ← Docs
          </Link>
          <div className="inline-flex items-center gap-2 rounded-full border border-brand-cyan/30 bg-brand-cyan/5 px-4 py-1.5 text-xs font-bold uppercase tracking-widest text-brand-cyan mt-6 mb-4">
            <Code2 className="w-3.5 h-3.5" />
            API & SDK Reference
          </div>
          <h1 className="text-4xl font-black uppercase tracking-tight mb-4">
            AMP Tournament <span className="text-brand-cyan">SDK Reference</span>
          </h1>
          <p className="text-lg text-zinc-400 leading-relaxed">
            Consistent method signatures across TypeScript (`@amp/sdk`), Unity (`com.amp.sdk`),
            Unreal Engine 5 (`AmpUnreal`), C# (`Amp.Sdk`), and Rust (`amp-sdk`).
          </p>
        </div>

        {/* Tournament Client Methods */}
        <h2 className="text-xl font-black uppercase tracking-tight mb-4 flex items-center gap-2">
          <Trophy className="w-5 h-5 text-brand-cyan" />
          AMPTournamentClient Methods
        </h2>
        <p className="text-sm text-zinc-400 mb-6">
          Methods for interacting with on-chain tournament escrows and player pull-claims:
        </p>

        <ApiMethod
          signature="getTournament(tournamentId: bigint)"
          returnType="Promise<TournamentData>"
          desc="Fetches on-chain tournament data (sponsor, token, prizePool, payoutBps, verifier, winners, state, deadlines)."
          example={`const cup = await amp.getTournament(1n);
console.log(cup.prizePoolAvax); // "0.1"`}
        />

        <ApiMethod
          signature="canClaimPrize(tournamentId: bigint, wallet: string)"
          returnType="Promise<CanClaimResult>"
          desc="Checks whether a player placed in the prize bracket and has an unclaimed reward available to pull."
          example={`const claim = await amp.canClaimPrize(1n, playerAddress);
if (claim.canClaim) {
  console.log(\`Estimated payout: \${claim.estimatedPrizeAvax} AVAX (Rank: \${claim.placement + 1})\`);
}`}
        />

        <ApiMethod
          signature="claimPrize(tournamentId: bigint, placement: number)"
          returnType="Promise<ContractTransactionReceipt>"
          desc="Sends an on-chain transaction calling claimPrize(uint256,uint256) to pull funds directly into the player's wallet."
          example={`const receipt = await amp.claimPrize(1n, 0); // 0 = 1st place
console.log(\`Prize claimed in block: \${receipt.blockNumber}\`);`}
        />

        <ApiMethod
          signature="signTournamentResult(tournamentId: bigint, winners: string[], signer: Signer)"
          returnType="Promise<string>"
          desc="Generates an EIP-712 cryptographic signature over TournamentResult(uint256 tournamentId, address[] winners)."
          example={`const sig = await amp.signTournamentResult(1n, [alice, bob], verifierSigner);`}
        />

        {/* Bracket Engine Methods */}
        <h2 className="text-xl font-black uppercase tracking-tight mb-4 mt-12 flex items-center gap-2">
          <Cpu className="w-5 h-5 text-yellow-400" />
          Bracket Engine (@amp/sdk)
        </h2>
        <p className="text-sm text-zinc-400 mb-6">
          Zero-dependency local tournament calculation engine for Single Elimination, Swiss, and Round Robin:
        </p>

        <ApiMethod
          signature="Tournament.new(format, entrants)"
          returnType="Tournament<T>"
          desc="Initializes a new tournament bracket with seeding and pairings."
          example={`const bracket = Tournament.new(
  { kind: TournamentState.SingleElimination },
  [{ id: 1, seed: 1 }, { id: 2, seed: 2 }, { id: 3, seed: 3 }, { id: 4, seed: 4 }]
);`}
        />

        <ApiMethod
          signature="bracket.currentRound()"
          returnType="Match<T>[]"
          desc="Returns the active matches for the current round."
          example={`const matches = bracket.currentRound();
for (const m of matches) {
  console.log(\`Match \${m.id}: Player \${m.player1} vs Player \${m.player2}\`);
}`}
        />

        <ApiMethod
          signature="bracket.record(matchId: number, outcome: Outcome)"
          returnType="void"
          desc="Records the outcome of a match (Outcome.A = Player 1 wins, Outcome.B = Player 2 wins, Outcome.Draw)."
          example={`bracket.record(0, Outcome.A);`}
        />

        <ApiMethod
          signature="bracket.standings()"
          returnType="Standing<T>[]"
          desc="Returns sorted standings and final placements once all rounds are complete."
          example={`const rankings = bracket.standings();
const champion = rankings[0].playerId;`}
        />

        {/* Back Link */}
        <div className="mt-16 pt-8 border-t border-white/10 flex justify-between items-center text-sm">
          <Link href="/docs/contracts" className="text-zinc-500 hover:text-white transition-colors">
            ← Contracts
          </Link>
          <Link href="/docs/faq" className="text-brand-cyan hover:underline font-bold">
            FAQ & Architecture →
          </Link>
        </div>
      </div>
    </div>
  );
}
