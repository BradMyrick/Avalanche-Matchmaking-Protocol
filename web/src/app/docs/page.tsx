import Link from "next/link";
import SiteHeader from "@/app/components/SiteHeader";
import AgentInstall from "@/app/components/AgentInstall";
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
  title: "AMP Documentation — Avalanche Tournament Protocol",
  description:
    "Everything you need to integrate AMP tournament prize pools, verifiable brackets, and in-game prize claims on Avalanche.",
};

const sections = [
  {
    href: "/docs/quickstart",
    icon: Rocket,
    title: "Quickstart Guide",
    desc: "Install the SDK for your engine and ship your first funded tournament — five steps, real code, no backend.",
    tag: "Start here",
  },
  {
    href: "/docs/contracts",
    icon: FileText,
    title: "Contracts & Addresses",
    desc: "The deployed AMPTournamentCup address on Fuji, ABI selectors, the EIP-712 attestation domain, and refund safety.",
    tag: "On-chain",
  },
  {
    href: "/docs/api",
    icon: Code2,
    title: "SDK Reference",
    desc: "The whole protocol in five calls — create, bracket, finalize, claim — with the exact signatures for every engine.",
    tag: "SDKs",
  },
  {
    href: "/docs/faq",
    icon: HelpCircle,
    title: "FAQ",
    desc: "What AMP is, who holds the money, fees, disputes, refunds, security, and the road to mainnet.",
    tag: "Answers",
  },
];

const features = [
  {
    icon: Coins,
    title: "Non-Custodial Prize Escrow",
    desc: "Prizes sit in the source-verified cup contract — not in a wallet, not with us — until a winner claims.",
  },
  {
    icon: Trophy,
    title: "Brackets in the SDK",
    desc: "Single elimination and round robin run locally in your game, deterministic and unit-testable.",
  },
  {
    icon: Cpu,
    title: "Every Engine",
    desc: "Unity, Unreal Engine 5, TypeScript, C#, and Rust — same five calls everywhere.",
  },
  {
    icon: Shield,
    title: "In-Game Pull Claims",
    desc: "Winners claim prizes straight to their Core or MetaMask wallet, from inside your game.",
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
            AMP runs real prize tournaments inside your game. Fund a pool with AVAX
            or a card, run the bracket, finalize with a signed result, and winners
            claim from the contract. These docs cover the whole path.
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

        {/* Agentic install */}
        <AgentInstall />

        {/* Back Link */}
        <div className="text-center pt-8 mt-16 border-t border-white/10">
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
