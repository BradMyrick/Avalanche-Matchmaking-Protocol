import Link from "next/link";
import { FileText, ExternalLink, Shield, Trophy, Gauge, Layers, Coins } from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";

export const metadata = {
  title: "AMP Contracts — Deployed Addresses & ABI Reference",
  description: "Deployed contract addresses on Avalanche Fuji, ABI selectors, EIP-712 specs, and gas costs.",
};

const contracts = [
  {
    name: "AMPTournamentCup",
    version: "v1.0 (Live)",
    address: CUP_ADDRESS,
    desc: "Decentralized tournament prize escrow, EIP-712 verifier attestation, and pull-claim payout router on Avalanche.",
    featured: true,
    features: [
      "Native AVAX & ERC-20 Support",
      "Multi-tier Basis Point Splits",
      "Automated Refund Safety Window",
      "Pull-Payment Claim Architecture",
      "EIP-712 Verifier Attestation",
    ],
    functions: [
      { name: "getTournament(uint256)", selector: "0x1a5bd7fc", desc: "Read tournament escrow, state, and winners" },
      { name: "claimPrize(uint256,uint256)", selector: "0x7b154140", desc: "Winner pulls prize payout to their wallet" },
      { name: "claimed(uint256,uint256)", selector: "0xa54ab457", desc: "Check if placement prize was claimed" },
      { name: "createTournament(...)", selector: "0x...", desc: "Sponsor funds prize pool with basis-point payouts" },
      { name: "finalizeTournament(...)", selector: "0x...", desc: "Submit EIP-712 verifier attestation signature" },
    ],
  },
  {
    name: "AMPMultiplayer",
    version: "v2 (N-player Escrow)",
    address: "0xcabf7b626172fE55d54f03c346563671AbcC77f7",
    desc: "N-player dual-deposit escrow + K-of-N quorum settlement + prove-your-payout claims for battle royales and lobbies.",
    featured: false,
    features: ["2–64 player lobbies", "Stake + reporting bond", "K = ⌊2N/3⌋+1 quorum", "Pull-only payouts"],
  },
  {
    name: "AMPRegistry",
    version: "v1",
    address: "0xf6B0eA6c88c574c4BbEAdC186AAfe72C43C2cDc2",
    desc: "Game registry, escrow holder, verifier whitelisting",
    featured: false,
    features: ["Game registration", "Escrow custody", "Verifier management"],
  },
];

export default function ContractsPage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <div className="relative z-10 max-w-4xl mx-auto px-6 py-16">
        <div className="mb-12">
          <Link href="/docs" className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors">
            ← Docs
          </Link>
          <div className="inline-flex items-center gap-2 rounded-full border border-brand-cyan/30 bg-brand-cyan/5 px-4 py-1.5 text-xs font-bold uppercase tracking-widest text-brand-cyan mt-6 mb-4">
            <FileText className="w-3.5 h-3.5" />
            On-chain Reference
          </div>
          <h1 className="text-4xl font-black uppercase tracking-tight mb-4">
            Smart Contracts & <span className="text-brand-cyan">ABIs</span>
          </h1>
          <p className="text-lg text-zinc-400 leading-relaxed">
            All contracts deployed and verified on Avalanche Fuji testnet (Chain ID 43113).
            Compatible with Avalanche C-Chain and custom Avalanche L1s (Subnets).
          </p>
        </div>

        {/* Contract cards */}
        <div className="space-y-8 mb-12">
          {contracts.map((c) => (
            <div
              key={c.name}
              className={`glass-panel rounded-3xl p-7 border ${
                c.featured
                  ? "border-brand-cyan/40 bg-brand-cyan/5 shadow-[0_0_40px_rgba(0,229,255,0.1)]"
                  : "border-white/10"
              }`}
            >
              <div className="flex items-start justify-between mb-3 flex-wrap gap-2">
                <div className="flex items-center gap-3">
                  <h2 className="text-xl font-bold">{c.name}</h2>
                  <span
                    className={`text-[10px] font-black uppercase tracking-widest px-2.5 py-0.5 rounded-full border ${
                      c.featured
                        ? "bg-brand-cyan/20 border-brand-cyan text-brand-cyan"
                        : "border-white/10 text-zinc-400"
                    }`}
                  >
                    {c.version}
                  </span>
                </div>
                <a
                  href={`${EXPLORER_URL}/address/${c.address}`}
                  target="_blank"
                  rel="noopener noreferrer"
                  className="text-xs text-brand-cyan hover:underline inline-flex items-center gap-1 font-semibold"
                >
                  Snowtrace Explorer <ExternalLink className="w-3 h-3" />
                </a>
              </div>

              <code className="block text-xs font-mono text-brand-cyan bg-black/60 border border-white/10 rounded-xl px-4 py-2.5 mb-4 overflow-x-auto">
                {c.address}
              </code>

              <p className="text-sm text-zinc-300 mb-4 leading-relaxed">{c.desc}</p>

              <div className="flex flex-wrap gap-2 mb-6">
                {c.features.map((f) => (
                  <span
                    key={f}
                    className="text-[10px] font-bold uppercase tracking-wider text-zinc-400 border border-white/10 rounded-full px-3 py-1 bg-white/5"
                  >
                    {f}
                  </span>
                ))}
              </div>

              {c.functions && (
                <div className="pt-4 border-t border-white/10">
                  <h3 className="text-xs font-bold uppercase tracking-widest text-zinc-400 mb-3">
                    Core Function Selectors
                  </h3>
                  <div className="space-y-2">
                    {c.functions.map((fn) => (
                      <div
                        key={fn.name}
                        className="flex flex-col sm:flex-row sm:items-center justify-between p-2.5 rounded-xl bg-black/40 border border-white/5 text-xs font-mono gap-1"
                      >
                        <span className="text-white font-bold">{fn.name}</span>
                        <div className="flex items-center gap-3">
                          <span className="text-zinc-400 text-[11px] font-sans">{fn.desc}</span>
                          <span className="text-yellow-400 text-[11px] bg-yellow-400/10 px-2 py-0.5 rounded border border-yellow-400/20">
                            {fn.selector}
                          </span>
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>

        {/* EIP-712 Specification */}
        <div className="glass-panel rounded-3xl border border-white/10 p-7 mb-12">
          <h2 className="text-xl font-bold uppercase tracking-tight mb-3 flex items-center gap-2">
            <Shield className="w-5 h-5 text-brand-cyan" />
            EIP-712 Attestation Specification
          </h2>
          <p className="text-sm text-zinc-400 mb-4 leading-relaxed">
            Tournament outcomes are attested by designated verifiers using typed data matching OpenZeppelin EIP-712:
          </p>
          <pre className="bg-black/60 border border-white/10 rounded-2xl p-4 text-xs font-mono text-zinc-300 overflow-x-auto">
            <code>{`Domain:
  name: "AMPTournamentCup"
  version: "1"
  chainId: 43113
  verifyingContract: "${CUP_ADDRESS}"

Type:
  TournamentResult(uint256 tournamentId, address[] winners)

TypeHash:
  keccak256("TournamentResult(uint256 tournamentId,address[] winners)")`}</code>
          </pre>
        </div>

        {/* Back Link */}
        <div className="mt-16 pt-8 border-t border-white/10 flex justify-between items-center text-sm">
          <Link href="/docs/quickstart" className="text-zinc-500 hover:text-white transition-colors">
            ← Quickstart
          </Link>
          <Link href="/docs/api" className="text-brand-cyan hover:underline font-bold">
            SDK Reference →
          </Link>
        </div>
      </div>
    </div>
  );
}
