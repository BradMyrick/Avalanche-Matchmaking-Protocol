import Link from "next/link";
import Image from "next/image";
import SiteHeader from "./components/SiteHeader";
import RotatingText from "./components/RotatingText";
import GrantBadge from "./components/GrantBadge";
import EngineTabs from "./components/EngineTabs";
import TournamentInteractiveDemo from "./components/TournamentInteractiveDemo";
import FujiNetwork from "./components/FujiNetwork";
import SecurityAudits from "./components/SecurityAudits";
import { Trophy, Coins, Github, ExternalLink } from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";

export default function Home() {
  const verifiedContractUrl = `${EXPLORER_URL}/address/${CUP_ADDRESS}`;

  return (
    <div className="relative min-h-screen overflow-hidden antialiased bg-black text-white">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[400px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[400px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      <SiteHeader />

      <main className="relative z-10 pt-16">
        <section className="max-w-7xl mx-auto px-6 flex flex-col items-center text-center pb-8">
          <div className="mt-4 mb-3">
            <Image
              src="/amp-icon.png"
              alt="AMP"
              width={64}
              height={64}
              priority
              className="w-14 h-14"
            />
          </div>

          <GrantBadge />

          <div className="inline-flex flex-wrap items-center justify-center gap-2 px-3 py-1 rounded-full bg-black/60 border border-brand-cyan/30 text-[11px] text-zinc-300 mb-4">
            <span className="flex h-1.5 w-1.5 relative">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
              <span className="relative inline-flex rounded-full h-1.5 w-1.5 bg-green-400"></span>
            </span>
            <span className="text-zinc-500">Fuji</span>
            <a
              href={verifiedContractUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="font-mono text-brand-cyan hover:text-white flex items-center gap-1"
            >
              <span>{CUP_ADDRESS.slice(0, 6)}…{CUP_ADDRESS.slice(-4)}</span>
              <ExternalLink className="w-3 h-3" />
            </a>
            <span className="text-yellow-400">AMPTournamentCup</span>
          </div>

          <h1 className="text-5xl sm:text-6xl lg:text-7xl font-black tracking-tighter mb-2 leading-[0.95] uppercase">
            TOURNAMENTS
            <span className="text-xl sm:text-2xl text-zinc-500 lowercase font-medium tracking-normal block my-1">
              for
            </span>
            <RotatingText />
          </h1>

          <p className="text-base sm:text-lg text-zinc-300 mb-5">
            SDK plus AMPTournamentCup.
          </p>

          <div className="flex flex-col sm:flex-row items-center justify-center gap-3 mb-6">
            <Link
              href="/setup"
              className="px-6 py-3 rounded-xl font-bold text-black bg-brand-cyan hover:bg-white transition-colors flex items-center justify-center gap-2 uppercase tracking-wider text-sm"
            >
              <Trophy className="w-4 h-4" />
              Host a Cup
            </Link>
            <Link
              href="/claim"
              className="px-6 py-3 rounded-xl font-bold text-white bg-black/60 border border-brand-cyan/40 hover:border-brand-cyan transition-colors flex items-center justify-center gap-2 uppercase tracking-wider text-sm"
            >
              <Coins className="w-4 h-4 text-yellow-400" />
              Claim Prize
            </Link>
          </div>
        </section>

        <section id="tools" className="py-16 max-w-7xl mx-auto px-6 scroll-mt-20">
          <div className="text-center max-w-3xl mx-auto mb-8">
            <h2 className="text-3xl sm:text-5xl font-black uppercase tracking-tight text-white mb-3">
              Tournament SDKs
            </h2>
            <p className="text-zinc-400">
              Brackets run in the SDK. Prizes sit in AMPTournamentCup.
            </p>
          </div>
          <EngineTabs />
        </section>

        <TournamentInteractiveDemo />

        <div id="payouts" className="scroll-mt-20">
          <FujiNetwork />
        </div>

        <div id="security" className="scroll-mt-20">
          <SecurityAudits />
        </div>
      </main>

      <footer className="py-8 border-t border-brand-cyan/20 bg-black/80 relative z-10">
        <div className="max-w-7xl mx-auto px-6 flex flex-col md:flex-row justify-between items-center gap-4">
          <span className="font-bold tracking-widest uppercase text-white text-sm">AMP Tournament Protocol</span>
          <p className="text-sm text-zinc-500">© 2026 AMP Tournament Protocol.</p>
          <div className="flex gap-5 text-zinc-400 uppercase tracking-wider text-xs">
            <Link
              href="https://github.com/bradmyrick/Avalanche-Matchmaking-Protocol"
              className="hover:text-brand-cyan inline-flex items-center gap-1"
              target="_blank"
            >
              <Github className="w-3.5 h-3.5" /> GitHub
            </Link>
            <Link href="mailto:brad@kodr.pro" className="hover:text-brand-cyan">Contact</Link>
            <Link href="/terms" className="hover:text-brand-cyan">Terms</Link>
            <Link href="/docs" className="hover:text-brand-cyan">Docs</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
