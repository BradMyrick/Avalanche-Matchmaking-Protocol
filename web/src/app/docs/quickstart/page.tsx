import Link from "next/link";
import { Rocket, Coins, Cpu, Flag, Wallet, Bot } from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";
import SiteHeader from "@/app/components/SiteHeader";
import CodeSample from "@/app/components/CodeSample";
import AgentInstall from "@/app/components/AgentInstall";

export const metadata = {
  title: "AMP Quickstart — Run Your First Tournament",
  description: "Install the AMP SDK, fund a prize pool, run a bracket, finalize results, and let winners claim — in five steps.",
};

const steps = [
  {
    icon: Cpu,
    title: "1. Install the SDK",
    code: `# Web / Node.js
npm install @amp/sdk ethers@^6

# Unity (Package Manager → Add package from git URL)
https://github.com/BradMyrick/amp-sdk-unity.git

# Unreal Engine 5 (from your project root)
git clone https://github.com/BradMyrick/amp-sdk-cpp.git amp-sdk
mv amp-sdk/AmpUnreal Plugins/AmpUnreal`,
    note: "Everything talks directly to Avalanche RPC and the AMPTournamentCup contract. There is no AMP backend your game depends on.",
  },
  {
    icon: Coins,
    title: "2. Fund a prize pool",
    code: `import { AMPTournamentClient } from "@amp/sdk";
import { parseEther } from "ethers";

const amp = new AMPTournamentClient().connect(sponsorWallet);

const { tournamentId, txHash } = await amp.createTournament(
  [6000, 3000, 1000],        // payout split in bps: 60/30/10, must sum to 10000
  verifierAddress,           // wallet allowed to sign final results
  deadlineUnix,              // after this, an unfinalized cup can be refunded
  parseEther("1.0")          // prize pool in AVAX (ERC-20 variant also available)
);`,
    note: "The AVAX sits in the contract — not in your wallet, not in ours. Up to 16 paid placements.",
  },
  {
    icon: Flag,
    title: "3. Run the bracket in your game",
    code: `import { Tournament, TournamentState, Outcome } from "@amp/sdk";

// Same engine ships in every SDK — pure, deterministic, no network calls
const bracket = Tournament.new(
  { kind: TournamentState.SingleElimination },
  [{ id: 1, seed: 1 }, { id: 2, seed: 2 }, { id: 3, seed: 3 }, { id: 4, seed: 4 }]
);

bracket.record(0, Outcome.A);        // feed in results as matches finish
const next = bracket.pending();       // matches still to play
const standings = bracket.standings();// final placements, sorted`,
    note: "Pairings, byes, and standings are computed locally. Test it with unit tests — no chain needed until finalize.",
  },
  {
    icon: Bot,
    title: "4. Finalize with a signed result",
    code: `// Verifier wallet signs the final placements (EIP-712)
const verifier = new AMPTournamentClient().connect(verifierWallet);
const signature = await verifier.signFinalize(
  tournamentId,
  [firstPlace, secondPlace, thirdPlace]   // must match the payout split length
);

// Anyone can submit the signature on-chain — organizer, relayer, or the game server
await amp.finalizeTournament(tournamentId, [firstPlace, secondPlace, thirdPlace], signature);`,
    note: "The contract verifies the signature against the verifier address set at creation. Only that key can release prizes.",
  },
  {
    icon: Wallet,
    title: "5. Winners claim their prize",
    code: `// TypeScript — in your game UI or web portal
const prize = await amp.getClaimablePrize(tournamentId, playerAddress);
if (prize) {
  await amp.claimPrize(tournamentId, prize.placement); // AVAX lands in the player's wallet
}

// Unity:  await AmpTournamentManager.Instance.ClaimPrizeAsync(tournamentId, placement);
// Unreal: "AMP Async Action: Check Can Claim" → "AMP Async Action: Claim Prize" nodes`,
    note: "Pull-payments only: winners call claimPrize themselves — from in-game, or at playwithamp.xyz/claim with just a wallet.",
  },
];

export default function QuickstartPage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <SiteHeader />
      <div className="relative z-10 max-w-4xl mx-auto px-6 pt-24 pb-16">
        {/* Header */}
        <div className="mb-12">
          <Link
            href="/docs"
            className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors"
          >
            ← Docs
          </Link>
          <div className="inline-flex items-center gap-2 rounded-full border border-brand-cyan/30 bg-brand-cyan/5 px-4 py-1.5 text-xs font-bold uppercase tracking-widest text-brand-cyan mt-6 mb-4">
            <Rocket className="w-3.5 h-3.5" />
            Quickstart
          </div>
          <h1 className="text-4xl font-black uppercase tracking-tight mb-4">
            Run your first tournament
          </h1>
          <p className="text-zinc-400 max-w-2xl">
            Five steps: install the SDK, fund a prize pool, run the bracket in your game,
            finalize with a signed result, and let winners claim. No backend, no API keys —
            your game talks straight to the contract.
          </p>
        </div>

        {/* Contract Reference Banner */}
        <div className="glass-panel rounded-2xl border border-white/10 p-5 mb-10 flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4">
          <div>
            <div className="text-xs font-bold uppercase tracking-wider text-zinc-400">
              Verified Fuji Contract
            </div>
            <div className="font-mono text-sm text-brand-cyan font-bold">
              {CUP_ADDRESS}
            </div>
          </div>
          <a
            href={`${EXPLORER_URL}/address/${CUP_ADDRESS}`}
            target="_blank"
            rel="noopener noreferrer"
            className="px-4 py-2 rounded-xl text-xs font-bold bg-white/10 hover:bg-white/20 text-white transition-all border border-white/10"
          >
            Snowtrace Explorer ↗
          </a>
        </div>

        {/* Steps */}
        <div className="space-y-8">
          {steps.map((step) => (
            <div
              key={step.title}
              className="glass-panel rounded-3xl border border-white/10 p-8 relative overflow-hidden"
            >
              <div className="flex items-center gap-3 mb-4">
                <div className="w-10 h-10 rounded-xl bg-brand-cyan/15 border border-brand-cyan/30 flex items-center justify-center text-brand-cyan">
                  <step.icon className="w-5 h-5" />
                </div>
                <h2 className="text-xl font-bold uppercase tracking-tight text-white">
                  {step.title}
                </h2>
              </div>

              <div className="bg-black/70 border border-white/10 rounded-2xl p-5 mb-4">
                <CodeSample code={step.code} />
              </div>

              <div className="text-xs text-zinc-400 border-l-2 border-brand-cyan/50 pl-3">
                {step.note}
              </div>
            </div>
          ))}
        </div>

        {/* Agentic install */}
        <div className="mt-16 scroll-mt-24" id="agentic">
          <AgentInstall />
        </div>

        {/* Next Links */}
        <div className="mt-16 pt-8 border-t border-white/10 flex justify-between items-center text-sm">
          <Link href="/docs" className="text-zinc-500 hover:text-white transition-colors">
            ← Overview
          </Link>
          <Link
            href="/docs/api"
            className="text-brand-cyan hover:underline font-bold"
          >
            Full SDK Reference →
          </Link>
        </div>
      </div>
    </div>
  );
}
