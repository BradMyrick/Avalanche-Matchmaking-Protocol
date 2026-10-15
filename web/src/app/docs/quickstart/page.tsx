import Link from "next/link";
import { Rocket, Key, Trophy, Coins, Cpu, ShieldCheck } from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";
import SiteHeader from "@/app/components/SiteHeader";
import CodeSample from "@/app/components/CodeSample";

export const metadata = {
  title: "AMP Quickstart — Tournament Protocol Integration",
  description: "Step-by-step guide to integrate AMP tournament prize pools and claims: Unity, Unreal, and Web.",
};

const steps = [
  {
    icon: Cpu,
    title: "1. Install the SDK For Your Engine",
    code: `// Unity: Add via Package Manager git URL:
// https://github.com/BradMyrick/amp-sdk-unity.git?path=com.amp.sdk

// Unreal Engine 5: Clone AmpUnreal into your project's Plugins/ folder:
// git clone https://github.com/BradMyrick/amp-sdk-cpp.git Plugins/AmpUnreal

// Web / Node.js: Install via npm:
npm install @amp/sdk ethers@^6`,
    note: "The SDK calls Avalanche RPC and AMPTournamentCup. It does not need this site.",
  },
  {
    icon: Trophy,
    title: "2. Inspect a Tournament Cup",
    code: `import { AMPTournamentClient } from "@amp/sdk";

// Initialize client pointing to Avalanche Fuji testnet
const amp = new AMPTournamentClient();

// Query Tournament Cup #1
const cup = await amp.getTournament(1n);

console.log(\`Sponsor: \${cup.sponsor}\`);
console.log(\`Prize Pool: \${cup.prizePoolAvax} AVAX\`);
console.log(\`Payout Split: \${cup.payoutBps.map(b => b / 100 + "%").join(" / ")}\`);`,
    note: "Reads on-chain state directly via getTournament(uint256) on AMPTournamentCup.sol.",
  },
  {
    icon: ShieldCheck,
    title: "3. Run Brackets Locally with @amp/sdk",
    code: `import { Tournament, TournamentState, Outcome } from "@amp/sdk";

// Seed 8 players into a Single Elimination or Swiss bracket
const entrants = [
  { id: 1, seed: 1 },
  { id: 2, seed: 2 },
  { id: 3, seed: 3 },
  { id: 4, seed: 4 },
];

const bracket = Tournament.new({ kind: TournamentState.SingleElimination }, entrants);

// Record match results as games finish
bracket.record(0, Outcome.A); // Player 1 defeats Player 4

// Get final standings when complete
const standings = bracket.standings();
console.log(\`Champion: Player #\${standings[0].playerId}\`);`,
    note: "Runs in the SDK.",
  },
  {
    icon: Key,
    title: "4. Settle with EIP-712 Attestation",
    code: `import { PrivateKeySigner } from "@amp/sdk";

// Verifier signs the final winner placements
const verifierSigner = new PrivateKeySigner(verifierPrivateKey);

const signature = await amp.signTournamentResult(
  1n, // tournamentId
  [winner1Address, winner2Address, winner3Address], // winner addresses
  verifierSigner
);

// Finalize on-chain (can be submitted by organizer or verifier)
await amp.finalizeTournament(1n, [winner1Address, winner2Address, winner3Address], signature);`,
    note: "Verifies against domain: { name: 'AMPTournamentCup', version: '1', chainId: 43113, verifyingContract: '0x7C743c1C9AE3e7A65d030098F2249B7787d66dfF' }.",
  },
  {
    icon: Coins,
    title: "5. In-Game Pull-Claim Payouts",
    code: `// Unity C# In-Game Prize Claim:
var amp = AmpTournamentManager.Instance;
var check = await amp.CanClaimAsync(tournamentId: 1, playerWallet);

if (check.CanClaim) {
    Debug.Log($"You won {check.EstimatedPrizeAvax} AVAX! Claiming...");
    var tx = await amp.ClaimPrizeAsync(tournamentId: 1, check.Placement);
    Debug.Log($"Claim confirmed in tx: {tx.Hash}");
}

// Unreal Engine 5 Blueprint:
// Use the "AMP Async Action: Check Can Claim" node -> wire into "AMP Async Action: Claim Prize"!`,
    note: "Winners call claimPrize from their wallet.",
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
            Quickstart
          </h1>
          <p className="text-zinc-400">
            SDK and AMPTournamentCup.
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
          {steps.map((step, i) => (
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

        {/* Next Links */}
        <div className="mt-16 pt-8 border-t border-white/10 flex justify-between items-center text-sm">
          <Link href="/docs" className="text-zinc-500 hover:text-white transition-colors">
            ← Overview
          </Link>
          <Link
            href="/docs/contracts"
            className="text-brand-cyan hover:underline font-bold"
          >
            Contracts & Addresses →
          </Link>
        </div>
      </div>
    </div>
  );
}
