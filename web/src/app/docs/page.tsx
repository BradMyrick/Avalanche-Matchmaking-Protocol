import Link from "next/link";
import SiteHeader from "@/app/components/SiteHeader";
import {
  Rocket,
  Code2,
  FileText,
  HelpCircle,
  ArrowRight,
  Shield,
  Trophy,
  Coins,
  Cpu,
} from "lucide-react";

export const metadata = {
  title: "AMP Documentation — Avalanche Tournament & Match Protocol",
  description:
    "Everything you need to integrate AMP tournament prize pools, verifiable brackets, and in-game prize claims on Avalanche.",
};

const sections = [
  {
    href: "/docs/quickstart",
    icon: Rocket,
    title: "Quickstart Guide",
    desc: "SDK and AMPTournamentCup. The library does not need this site.",
    tag: "Start here",
  },
  {
    href: "/docs/contracts",
    icon: FileText,
    title: "Contracts & Addresses",
    desc: "Deployed AMPTournamentCup address on Fuji, ABI selectors, EIP-712 attestation domain, and refund safety.",
    tag: "On-chain",
  },
  {
    href: "/docs/api",
    icon: Code2,
    title: "Client SDK Reference",
    desc: "API reference for Unity, Unreal Engine 5, TypeScript, C#, and Rust SDKs + local bracket engine.",
    tag: "SDKs",
  },
  {
    href: "/docs/faq",
    icon: HelpCircle,
    title: "FAQ & Architecture",
    desc: "Fees, claims, and SDKs.",
    tag: "Answers",
  },
];

const features = [
  {
    icon: Coins,
    title: "Non-Custodial Prize Escrow",
    desc: "Sponsors fund AVAX in the source-verified cup contract.",
  },
  {
    icon: Trophy,
    title: "Verifiable Brackets",
    desc: "Single elimination and Swiss run locally, then settle with EIP-712.",
  },
  {
    icon: Cpu,
    title: "Multi-Engine Support",
    desc: "Drop-in SDKs for Unity (com.amp.sdk), Unreal Engine 5 (AmpUnreal), TypeScript, C#, and Rust.",
  },
  {
    icon: Shield,
    title: "In-Game Pull Claims",
    desc: "Winning players claim rewards directly to their Core or MetaMask wallet from within your game UI.",
  },
];

export default function DocsHub() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[400px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[400px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      <SiteHeader />
      <div className="relative z-10 max-w-6xl mx-auto px-6 pt-24 pb-16">
        <div className="text-center mb-12">
          <h1 className="text-4xl md:text-5xl font-black uppercase tracking-tight mb-4">
            Build with <span className="text-brand-cyan">AMP</span>
          </h1>
          <p className="max-w-2xl mx-auto text-zinc-400">
            SDK and AMPTournamentCup. Local brackets, on-chain prizes.
          </p>
        </div>

        {/* Doc Cards */}
        <div className="grid grid-cols-1 md:grid-cols-2 gap-5 mb-16">
          {sections.map((s) => (
            <Link
              key={s.href}
              href={s.href}
              className="group glass-panel rounded-3xl border border-white/10 hover:border-brand-cyan/40 p-8 transition-all hover:-translate-y-1"
            >
              <div className="flex items-start justify-between mb-4">
                <div className="w-12 h-12 rounded-2xl bg-brand-cyan/10 border border-brand-cyan/20 flex items-center justify-center group-hover:scale-110 transition-transform">
                  <s.icon className="w-6 h-6 text-brand-cyan" />
                </div>
                <span className="text-[10px] font-black uppercase tracking-widest text-zinc-500 border border-zinc-700 rounded-full px-3 py-1">
                  {s.tag}
                </span>
              </div>
              <h2 className="text-xl font-bold mb-2 group-hover:text-brand-cyan transition-colors">
                {s.title}
              </h2>
              <p className="text-sm text-zinc-400 leading-relaxed">{s.desc}</p>
              <div className="mt-4 inline-flex items-center gap-1 text-sm text-brand-cyan opacity-0 group-hover:opacity-100 transition-opacity">
                Read Guide <ArrowRight className="w-4 h-4" />
              </div>
            </Link>
          ))}
        </div>

        {/* What AMP provides */}
        <div className="mb-16">
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            {features.map((f) => (
              <div
                key={f.title}
                className="glass-panel rounded-2xl border border-white/10 p-6 flex flex-col justify-between"
              >
                <div>
                  <f.icon className="w-8 h-8 text-brand-cyan mb-4" />
                  <h3 className="font-bold text-sm uppercase tracking-wide mb-2 text-white">
                    {f.title}
                  </h3>
                  <p className="text-xs text-zinc-400 leading-relaxed">{f.desc}</p>
                </div>
              </div>
            ))}
          </div>
        </div>

        {/* Back Link */}
        <div className="text-center pt-8 border-t border-white/10">
          <Link
            href="/"
            className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors"
          >
            ← Back to Homepage
          </Link>
        </div>
      </div>
    </div>
  );
}
