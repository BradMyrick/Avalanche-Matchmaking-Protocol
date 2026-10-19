import Link from "next/link";
import { FileText, ExternalLink, Shield } from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";
import SiteHeader from "@/app/components/SiteHeader";
import CodeSample from "@/app/components/CodeSample";

export const metadata = {
  title: "AMP Contracts — Deployed Addresses & ABI Reference",
  description: "Deployed contract addresses on Avalanche Fuji, ABI selectors, EIP-712 specs, and gas costs.",
};

const contracts = [
  {
    name: "AMPTournamentCup",
    version: "v1.0 (Live)",
    address: CUP_ADDRESS,
    desc: "Tournament prize escrow, EIP-712 verifier attestation, and pull-claim payout router on Avalanche. This is the contract — the whole product runs on it.",
    featured: true,
    features: [
      "Native AVAX & ERC-20 Support",
      "Multi-tier Basis Point Splits",
      "Automated Refund Safety Window",
      "Pull-Payment Claim Architecture",
      "EIP-712 Verifier Attestation",
    ],
    functions: [
      { name: "createTournament(...)", selector: "0x39d87eda", desc: "Sponsor funds prize pool with basis-point payouts" },
      { name: "createTournamentERC20(...)", selector: "0x529e0cff", desc: "ERC-20 prize variant (fee-on-transfer safe)" },
      { name: "finalizeTournament(...)", selector: "0x8ef13499", desc: "Submit EIP-712 verifier attestation signature" },
      { name: "claimPrize(uint256,uint256)", selector: "0x7b154140", desc: "Winner pulls prize payout to their wallet" },
      { name: "cancelTournament(uint256)", selector: "0x32432233", desc: "Refund the sponsor if never finalized" },
      { name: "getTournament(uint256)", selector: "0x1a5bd7fc", desc: "Read tournament escrow, state, and winners" },
      { name: "protocolFeeBps()", selector: "0x35659fb8", desc: "Current protocol fee in basis points" },
    ],
  },
];

export default function ContractsPage() {
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
            <FileText className="w-3.5 h-3.5" />
            On-chain Reference
          </div>
          <h1 className="text-4xl font-black uppercase tracking-tight mb-4">
            Smart Contracts & <span className="text-brand-cyan">ABIs</span>
          </h1>
          <p className="text-zinc-400">
            Source-verified on Fuji (chain 43113).
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

        {/* Retired experiments note */}
        <div className="glass-panel rounded-2xl border border-white/5 p-5 mb-12">
          <p className="text-xs text-zinc-500 leading-relaxed">
            Earlier AMP iterations included an N-player escrow (AMPMultiplayer) and a 1v1
            match escrow (AMPRegistry/AMPSettlement). Those were experiments and are
            retired from the product — the contracts remain source-verified on Fuji with
            no user funds, but integrators should target AMPTournamentCup only.
          </p>
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
          <div className="bg-black/60 border border-white/10 rounded-2xl p-4">
            <CodeSample code={`Domain:
  name: "AMPTournamentCup"
  version: "1"
  chainId: 43113
  verifyingContract: "${CUP_ADDRESS}"

Type:
  TournamentResult(uint256 tournamentId, address[] winners)`} />
          </div>
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
