import Link from "next/link";
import Image from "next/image";
import RotatingText from "./components/RotatingText";
import GrantBadge from "./components/GrantBadge";
import EngineTabs from "./components/EngineTabs";
import TechDemoSection from "./components/TechDemoSection";
import FujiNetwork from "./components/FujiNetwork";
import SecurityAudits from "./components/SecurityAudits";
import {
  Trophy,
  Coins,
  ShieldCheck,
  Github,
  Code2,
  ExternalLink,
  Sparkles,
  Zap,
  Rocket,
  Layers,
} from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";

export default function Home() {
  const verifiedContractUrl = `${EXPLORER_URL}/address/${CUP_ADDRESS}`;

  return (
    <div className="relative min-h-screen overflow-hidden antialiased bg-black text-white">
      {/* Dynamic Background Elements */}
      <div className="absolute top-0 -left-1/4 w-[150%] h-[500px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[500px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      {/* Navigation Header */}
      <header className="fixed top-0 w-full z-50 glass-panel border-b border-brand-cyan/10 rounded-none bg-black/75 backdrop-blur-xl">
        <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gradient-to-r from-transparent via-brand-cyan/50 to-transparent" />
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          <Link
            href="/"
            className="flex items-center gap-3 bg-black/50 border border-brand-cyan/30 px-4 py-2 rounded-2xl hover:bg-black/70 hover:border-brand-cyan/60 transition-all duration-300 backdrop-blur-xl shadow-[0_0_20px_rgba(0,229,255,0.1)] hover:shadow-[0_0_30px_rgba(0,229,255,0.25)] group"
          >
            <div className="w-9 h-9 rounded-xl overflow-hidden shadow-[0_0_15px_rgba(0,229,255,0.4)] group-hover:scale-105 transition-transform duration-300 bg-black flex items-center justify-center p-1">
              <Image
                src="/amp-icon.png"
                alt="AMP Icon"
                width={36}
                height={36}
                className="w-full h-full object-contain drop-shadow-[0_0_8px_rgba(255,255,255,0.8)]"
              />
            </div>
            <div className="flex flex-col justify-center">
              <span className="text-xl md:text-2xl font-black tracking-widest uppercase text-white drop-shadow-[0_0_8px_rgba(0,229,255,0.5)] leading-none mb-0.5">
                AMP
              </span>
              <span className="text-[9px] text-zinc-400 font-semibold tracking-widest uppercase leading-none hidden sm:block">
                Tournaments & Matchmaking
              </span>
            </div>
          </Link>

          <nav className="hidden lg:flex gap-7 text-xs font-semibold uppercase tracking-wider text-zinc-300">
            <Link
              href="/setup"
              className="text-brand-cyan hover:text-white transition-colors duration-300 flex items-center gap-1.5"
            >
              <Trophy className="w-3.5 h-3.5" /> Host Cup
            </Link>
            <Link
              href="/claim"
              className="hover:text-yellow-400 transition-colors duration-300 flex items-center gap-1.5"
            >
              <Coins className="w-3.5 h-3.5" /> Claim Prize
            </Link>
            <Link
              href="#tools"
              className="hover:text-brand-cyan transition-colors duration-300 flex items-center gap-1.5"
            >
              <Code2 className="w-3.5 h-3.5" /> Developer Tools
            </Link>
            <Link
              href="#demo"
              className="hover:text-brand-cyan transition-colors duration-300 flex items-center gap-1.5"
            >
              <Zap className="w-3.5 h-3.5" /> Live Duel
            </Link>
            <Link
              href="#payouts"
              className="hover:text-brand-cyan transition-colors duration-300"
            >
              Prize Pools
            </Link>
            <Link
              href="#security"
              className="hover:text-brand-cyan transition-colors duration-300"
            >
              Security
            </Link>
            <Link
              href="/docs"
              className="hover:text-brand-cyan transition-colors duration-300"
            >
              Docs
            </Link>
          </nav>

          <div className="flex items-center gap-3">
            <Link
              href="/setup"
              className="hidden sm:inline-flex px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-wider bg-brand-cyan text-black hover:bg-white transition-all shadow-[0_0_15px_rgba(0,229,255,0.3)] hover:scale-105"
            >
              Host Cup
            </Link>
            <Link
              href="https://github.com/bradmyrick/Avalanche-Matchmaking-Protocol"
              target="_blank"
              className="bg-white/5 hover:bg-white/10 text-white px-4 py-2 rounded-xl font-bold transition-all border border-white/10 hover:border-brand-cyan/40 items-center gap-2 flex text-xs backdrop-blur-md"
            >
              <Github className="w-4 h-4 text-brand-cyan" />
              <span className="hidden md:inline">GitHub</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Main Page Content */}
      <main className="relative z-10 pt-36 md:pt-40">
        {/* Hero Section */}
        <section className="max-w-7xl mx-auto px-6 flex flex-col items-center text-center pb-20">
          {/* Logo with Ambient Intense Glow */}
          <div className="relative mb-12 group mt-4">
            <div className="absolute -inset-10 bg-gradient-to-t from-brand-cyan/20 to-brand-red/15 blur-[100px] rounded-full opacity-70 group-hover:opacity-100 transition-opacity duration-1000 pointer-events-none" />

            <div className="relative transform-gpu transition-all duration-1000 hover:-translate-y-2 shadow-[0_30px_90px_-20px_rgba(0,0,0,1)] hover:shadow-[0_50px_110px_-20px_rgba(0,229,255,0.25)] rounded-[2.5rem] border border-white/10 bg-gradient-to-b from-white/10 to-transparent p-3 md:p-5 backdrop-blur-md z-20">
              <Image
                src="/amp-logo.png"
                alt="AMP Logo"
                width={576}
                height={200}
                priority
                className="w-64 sm:w-80 md:w-[26rem] lg:w-[32rem] h-auto rounded-[2rem] drop-shadow-[0_40px_35px_rgba(0,0,0,0.9)]"
              />
            </div>
          </div>

          {/* $15,000 Avalanche Build Games Grant Badge */}
          <GrantBadge />

          {/* Verified Fuji Contract Pill */}
          <div className="inline-flex flex-wrap items-center justify-center gap-2 px-4 py-2 rounded-full bg-black/60 border border-brand-cyan/30 text-xs text-zinc-300 backdrop-blur-xl mb-10 shadow-[0_0_25px_rgba(0,229,255,0.12)] hover:border-brand-cyan/60 transition-all group">
            <span className="flex h-2 w-2 relative">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-green-400 opacity-75"></span>
              <span className="relative inline-flex rounded-full h-2 w-2 bg-green-400"></span>
            </span>
            <span className="text-zinc-400 font-medium">Verified Fuji Contract:</span>
            <a
              href={verifiedContractUrl}
              target="_blank"
              rel="noopener noreferrer"
              className="font-mono text-brand-cyan hover:text-white transition-colors flex items-center gap-1 font-semibold group-hover:underline"
            >
              <span>{CUP_ADDRESS}</span>
              <ExternalLink className="w-3.5 h-3.5 text-brand-cyan/80" />
            </a>
            <span className="text-zinc-600 hidden sm:inline">|</span>
            <span className="text-yellow-400 font-semibold text-[11px] hidden sm:inline">AMPTournamentCup</span>
          </div>

          {/* Iconic Headline */}
          <h1 className="text-5xl sm:text-7xl md:text-8xl lg:text-9xl font-black tracking-tighter mb-8 leading-[1.05] uppercase drop-shadow-[0_0_35px_rgba(0,229,255,0.25)]">
            MATCHMA<span className="text-yellow-400 drop-shadow-[0_0_25px_rgba(250,204,21,0.85)]">KING</span>
            <br />
            <span className="text-3xl sm:text-4xl md:text-5xl text-zinc-400 lowercase font-medium tracking-normal block my-4">
              for
            </span>
            <RotatingText />
          </h1>

          {/* Concise Subtitle Under ANY GAME */}
          <p className="max-w-3xl text-xl sm:text-2xl md:text-3xl text-zinc-300 mb-10 font-medium tracking-tight">
            Drop-in tournament SDKs for Unity, Unreal Engine, or Web.
          </p>

          {/* Hero CTAs */}
          <div className="flex flex-col sm:flex-row items-center justify-center gap-4 mb-20 w-full max-w-md">
            <Link
              href="/setup"
              className="w-full sm:w-auto px-8 py-4 rounded-xl font-bold text-black bg-brand-cyan hover:bg-white transition-all flex items-center justify-center gap-2.5 shadow-[0_0_30px_rgba(0,229,255,0.5)] hover:shadow-[0_0_50px_rgba(0,229,255,0.8)] hover:-translate-y-1 transform duration-300 uppercase tracking-wider text-sm border-b-4 border-brand-dark-cyan active:border-b-0 active:translate-y-1"
            >
              <Trophy className="w-5 h-5 text-black" />
              Host a Cup
            </Link>

            <Link
              href="/claim"
              className="w-full sm:w-auto px-8 py-4 rounded-xl font-bold text-white bg-black/60 hover:bg-brand-cyan/10 border border-brand-cyan/40 hover:border-brand-cyan transition-all flex items-center justify-center gap-2.5 shadow-[0_0_20px_rgba(0,229,255,0.15)] hover:-translate-y-1 transform duration-300 uppercase tracking-wider text-sm backdrop-blur-md"
            >
              <Coins className="w-5 h-5 text-yellow-400" />
              Claim Prize
            </Link>
          </div>
        </section>

        {/* Developer Tools Section: Hero Feature - Drop-in SDKs */}
        <section id="tools" className="py-24 max-w-7xl mx-auto px-6 scroll-mt-24 relative">
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[120%] h-[350px] bg-brand-cyan/5 blur-[160px] rounded-full pointer-events-none" />

          <div className="text-center max-w-3xl mx-auto mb-14">
            <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-brand-cyan/10 border border-brand-cyan/30 mb-4 shadow-[0_0_15px_rgba(0,229,255,0.2)]">
              <Code2 className="w-3.5 h-3.5 text-brand-cyan" />
              <span className="text-xs font-bold text-brand-cyan tracking-widest uppercase">
                Developer Tooling
              </span>
            </div>
            <h2 className="text-3xl sm:text-5xl md:text-6xl font-black uppercase tracking-tight text-white mb-4">
              Drop-in SDKs <span className="text-brand-cyan">For Any Engine</span>
            </h2>
            <p className="text-zinc-400 text-base sm:text-lg">
              Choose your stack. Copy 3 lines of code. Your game is competing on Avalanche with zero backend infrastructure.
            </p>
          </div>

          <EngineTabs />
        </section>

        {/* Live Technology Demo Section: QuickDraw Reaction Duel */}
        <div id="demo" className="scroll-mt-24">
          <TechDemoSection />
        </div>

        {/* Live Fuji Network Activity */}
        <div id="payouts" className="scroll-mt-24">
          <FujiNetwork />
        </div>

        {/* Security & Formal Verification */}
        <div id="security" className="scroll-mt-24">
          <SecurityAudits />
        </div>
      </main>

      {/* Modern, Clean Footer */}
      <footer className="py-12 border-t border-brand-cyan/20 bg-black/80 backdrop-blur-xl relative z-10">
        <div className="absolute top-0 left-0 w-full h-[1px] bg-gradient-to-r from-transparent via-brand-cyan/50 to-transparent" />
        <div className="max-w-7xl mx-auto px-6 flex flex-col md:flex-row justify-between items-center gap-6">
          <div className="flex items-center gap-3">
            <div className="w-6 h-6 rounded-sm bg-gradient-to-tr from-brand-dark-cyan to-brand-cyan shadow-[0_0_10px_rgba(0,229,255,0.5)] flex items-center justify-center">
              <Zap className="w-3.5 h-3.5 text-black" />
            </div>
            <span className="font-bold tracking-widest uppercase text-white">AMP Protocols</span>
          </div>
          <div className="flex items-center gap-2">
            <p className="text-sm text-zinc-500 font-medium">
              &copy; 2026 Avalanche Matchmaking Protocol. Backed by Avalanche Build Games ($15,000 Grant).
            </p>
          </div>
          <div className="flex gap-6 text-zinc-400 font-medium uppercase tracking-wider text-xs">
            <Link
              href="https://github.com/bradmyrick/Avalanche-Matchmaking-Protocol"
              className="hover:text-brand-cyan transition-colors"
              target="_blank"
            >
              GitHub
            </Link>
            <Link href="mailto:brad@kodr.pro" className="hover:text-brand-cyan transition-colors">
              Contact
            </Link>
            <Link href="/terms" className="hover:text-brand-cyan transition-colors">
              Terms
            </Link>
            <Link href="/docs" className="hover:text-brand-cyan transition-colors">
              Docs
            </Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
