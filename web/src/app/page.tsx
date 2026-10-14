import Link from "next/link";
import Image from "next/image";
import MatchmakingDemo from "./components/MatchmakingDemo";
import FujiNetwork from "./components/FujiNetwork";
import RotatingText from "./components/RotatingText";
import EngineTabs from "./components/EngineTabs";
import GrantBadge from "./components/GrantBadge";
import SecurityAudits from "./components/SecurityAudits";
import InvestorCTA from "./components/InvestorCTA";
import TournamentShowcase from "./components/TournamentShowcase";
import WhyAmp from "./components/WhyAmp";
import TechDemoSection from "./components/TechDemoSection";
import {
  Trophy,
  Coins,
  ShieldCheck,
  Github,
  Boxes,
  Code2,
  ExternalLink,
  Sparkles,
  Zap,
} from "lucide-react";
import { CUP_ADDRESS, EXPLORER_URL } from "@/lib/ampCup";

export default function Home() {
  const verifiedContractUrl = `${EXPLORER_URL}/address/${CUP_ADDRESS}`;

  return (
    <div className="relative min-h-screen overflow-hidden antialiased bg-black text-white">
      {/* Dynamic Background Elements */}
      <div className="absolute top-0 -left-1/4 w-[150%] h-[500px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[500px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      {/* Navigation */}
      <header className="fixed top-0 w-full z-50 glass-panel border-b border-brand-cyan/10 rounded-none bg-black/70 backdrop-blur-xl">
        <div className="absolute bottom-0 left-0 w-full h-[1px] bg-gradient-to-r from-transparent via-brand-cyan/50 to-transparent" />
        <div className="max-w-7xl mx-auto px-6 h-20 flex items-center justify-between">
          <Link href="/" className="flex items-center gap-3 bg-black/50 border border-brand-cyan/30 px-4 py-2 rounded-2xl hover:bg-black/70 hover:border-brand-cyan/60 transition-all duration-300 backdrop-blur-xl shadow-[0_0_20px_rgba(0,229,255,0.1)] hover:shadow-[0_0_30px_rgba(0,229,255,0.25)] group">
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
              <span className="text-xl md:text-2xl font-black tracking-widest uppercase text-white drop-shadow-[0_0_8px_rgba(0,229,255,0.5)] leading-none mb-0.5">AMP</span>
              <span className="text-[9px] text-zinc-400 font-semibold tracking-widest uppercase leading-none hidden sm:block">Tournament Protocol</span>
            </div>
          </Link>

          <nav className="hidden lg:flex gap-7 text-xs font-semibold uppercase tracking-wider text-zinc-300">
            <Link href="/setup" className="text-brand-cyan hover:text-white transition-colors duration-300 flex items-center gap-1.5">
              <Trophy className="w-3.5 h-3.5" /> Create Cup
            </Link>
            <Link href="/claim" className="hover:text-yellow-400 transition-colors duration-300 flex items-center gap-1.5">
              <Coins className="w-3.5 h-3.5" /> Claim Prize
            </Link>
            <Link href="#features" className="hover:text-brand-cyan transition-colors duration-300">Features</Link>
            <Link href="#showcase" className="hover:text-brand-cyan transition-colors duration-300">Showcase</Link>
            <Link href="#tech-demo" className="hover:text-brand-cyan transition-colors duration-300">Live Demo</Link>
            <Link href="#fuji" className="hover:text-brand-cyan transition-colors duration-300">Prize Pools</Link>
            <Link href="#api" className="hover:text-brand-cyan transition-colors duration-300">SDKs</Link>
            <Link href="/arena" className="hover:text-brand-cyan transition-colors duration-300">Arena</Link>
            <Link href="/dashboard" className="hover:text-brand-cyan transition-colors duration-300">Dashboard</Link>
          </nav>

          <div className="flex items-center gap-3">
            <Link
              href="/setup"
              className="hidden sm:inline-flex px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-wider bg-brand-cyan text-black hover:bg-white transition-all shadow-[0_0_15px_rgba(0,229,255,0.3)]"
            >
              Host Cup
            </Link>
            <Link
              href="https://github.com/bradmyrick/Avalanche-Matchmaking-Protocol"
              target="_blank"
              className="bg-white/5 hover:bg-white/10 text-white px-4 py-2 rounded-xl font-bold transition-all border border-white/10 hover:border-brand-cyan/40 items-center gap-2 flex text-xs"
            >
              <Github className="w-4 h-4 text-brand-cyan" />
              <span className="hidden md:inline">GitHub</span>
            </Link>
          </div>
        </div>
      </header>

      {/* Hero Section */}
      <main className="relative z-10 pt-36 md:pt-40">
        <section className="max-w-7xl mx-auto px-6 flex flex-col items-center text-center pb-20">
          {/* Logo with ambient intense glow */}
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

          {/* Prominent $15,000 Avalanche Build Games Grant Badge */}
          <GrantBadge />

          {/* Featured Verified Fuji Contract Pill */}
          <div className="inline-flex flex-wrap items-center justify-center gap-2 px-4 py-2 rounded-full bg-black/60 border border-brand-cyan/30 text-xs text-zinc-300 backdrop-blur-xl mb-8 shadow-[0_0_25px_rgba(0,229,255,0.12)] hover:border-brand-cyan/60 transition-all group">
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

          {/* Primary Hero Title */}
          <h1 className="text-4xl sm:text-6xl md:text-7xl lg:text-8xl font-black tracking-tighter mb-6 leading-[1.08] uppercase drop-shadow-[0_0_35px_rgba(0,229,255,0.25)]">
            AMP — Avalanche <br className="hidden sm:block" />
            <span className="bg-gradient-to-r from-brand-cyan via-white to-yellow-400 bg-clip-text text-transparent">
              Tournament & Match
            </span>{" "}
            Protocol
          </h1>

          {/* Rotating Tagline */}
          <div className="inline-flex items-center gap-2 px-4 py-1.5 rounded-full bg-white/5 border border-white/10 text-xs text-zinc-300 mb-6 font-mono">
            <span className="text-zinc-400">Built for</span>
            <RotatingText />
          </div>

          {/* Subtitle */}
          <p className="max-w-4xl text-lg sm:text-xl md:text-2xl text-zinc-300 mb-12 leading-relaxed font-medium">
            Verifiable tournament prize pools, competitive brackets, and in-game rewards on Avalanche. Powered by smart contracts, drop-in game SDKs, and gasless settlement.
          </p>

          {/* Hero CTAs */}
          <div className="flex flex-col sm:flex-row flex-wrap items-center justify-center gap-4 sm:gap-5 mb-24 w-full max-w-3xl">
            {/* Primary CTA */}
            <Link
              href="/setup"
              className="w-full sm:w-auto group px-8 py-4 rounded-xl font-bold text-black bg-brand-cyan hover:bg-white transition-all flex items-center justify-center gap-2.5 shadow-[0_0_30px_rgba(0,229,255,0.5)] hover:shadow-[0_0_50px_rgba(0,229,255,0.8)] hover:-translate-y-1 transform duration-300 uppercase tracking-wider text-sm border-b-4 border-brand-dark-cyan active:border-b-0 active:translate-y-1"
            >
              <Trophy className="w-5 h-5 text-black" />
              Create a Tournament Cup
              <Sparkles className="w-4 h-4 text-black opacity-60 group-hover:opacity-100 transition-opacity" />
            </Link>

            {/* Secondary CTA */}
            <Link
              href="/claim"
              className="w-full sm:w-auto px-8 py-4 rounded-xl font-bold text-white bg-black/60 hover:bg-brand-cyan/10 border border-brand-cyan/40 hover:border-brand-cyan transition-all flex items-center justify-center gap-2.5 shadow-[0_0_20px_rgba(0,229,255,0.15)] hover:-translate-y-1 transform duration-300 uppercase tracking-wider text-sm backdrop-blur-md"
            >
              <Coins className="w-5 h-5 text-yellow-400" />
              Claim Your Prize
            </Link>

            {/* Tertiary CTA */}
            <Link
              href="/docs"
              className="w-full sm:w-auto px-7 py-4 rounded-xl font-semibold text-zinc-300 hover:text-white bg-white/5 hover:bg-white/10 border border-white/10 hover:border-white/30 transition-all flex items-center justify-center gap-2 uppercase tracking-wider text-xs backdrop-blur-md"
            >
              <Code2 className="w-4 h-4 text-brand-cyan" />
              Developer SDKs (Unity / Unreal / TS)
            </Link>
          </div>

          {/* Core Feature Grid */}
          <div id="features" className="w-full grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6 text-left scroll-mt-32 relative z-10">
            {/* Feature 1 */}
            <div className="glass-panel p-7 hover:bg-yellow-400/5 transition-all duration-300 group border border-white/10 hover:border-yellow-400/40 flex flex-col justify-between">
              <div>
                <div className="w-12 h-12 rounded-xl bg-yellow-400/20 flex items-center justify-center mb-6 text-yellow-400 group-hover:scale-110 transition-transform shadow-[0_0_20px_rgba(250,204,21,0.25)] border border-yellow-400/30">
                  <Coins className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold mb-3 text-white uppercase tracking-wide group-hover:text-yellow-400 transition-colors">
                  Trustless Sponsor Prize Pools
                </h3>
                <p className="text-zinc-400 text-sm leading-relaxed font-medium">
                  Sponsor funds AVAX or USDC into <code className="text-yellow-300 font-mono text-xs">AMPTournamentCup.sol</code>, commits to basis-point payouts, with automatic refund protection if abandoned.
                </p>
              </div>
              <div className="mt-5 pt-4 border-t border-white/5 flex items-center gap-2 text-xs text-zinc-500 font-mono">
                <span className="w-1.5 h-1.5 rounded-full bg-yellow-400" />
                <span>AVAX & USDC On-Chain Escrow</span>
              </div>
            </div>

            {/* Feature 2 */}
            <div className="glass-panel p-7 hover:bg-brand-cyan/5 transition-all duration-300 group border border-white/10 hover:border-brand-cyan/40 flex flex-col justify-between">
              <div>
                <div className="w-12 h-12 rounded-xl bg-brand-cyan/20 flex items-center justify-center mb-6 text-brand-cyan group-hover:scale-110 transition-transform shadow-[0_0_20px_rgba(0,229,255,0.25)] border border-brand-cyan/30">
                  <Trophy className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold mb-3 text-white uppercase tracking-wide group-hover:text-brand-cyan transition-colors">
                  Automated Bracket Engine
                </h3>
                <p className="text-zinc-400 text-sm leading-relaxed font-medium">
                  Single Elimination, Swiss pairings, and Round Robin run on-chain with EIP-712 verifier attestations.
                </p>
              </div>
              <div className="mt-5 pt-4 border-t border-white/5 flex items-center gap-2 text-xs text-zinc-500 font-mono">
                <span className="w-1.5 h-1.5 rounded-full bg-brand-cyan" />
                <span>EIP-712 Cryptographic Verification</span>
              </div>
            </div>

            {/* Feature 3 */}
            <div className="glass-panel p-7 hover:bg-brand-red/5 transition-all duration-300 group border border-white/10 hover:border-brand-red/40 flex flex-col justify-between">
              <div>
                <div className="w-12 h-12 rounded-xl bg-brand-dark-red/40 flex items-center justify-center mb-6 text-brand-red group-hover:scale-110 transition-transform shadow-[0_0_20px_rgba(232,65,66,0.25)] border border-brand-red/30">
                  <Boxes className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold mb-3 text-white uppercase tracking-wide group-hover:text-brand-red transition-colors">
                  Drop-in Game Engine SDKs
                </h3>
                <p className="text-zinc-400 text-sm leading-relaxed font-medium">
                  Ready-to-use Unity Package (<code className="text-brand-cyan font-mono text-xs">com.amp.sdk</code>), Unreal Engine 5 plugin (<code className="text-brand-cyan font-mono text-xs">AmpUnreal</code>), and TypeScript (<code className="text-brand-cyan font-mono text-xs">@amp/sdk</code>) for 3-line in-game prize claims.
                </p>
              </div>
              <div className="mt-5 pt-4 border-t border-white/5 flex items-center gap-2 text-xs text-zinc-500 font-mono">
                <span className="w-1.5 h-1.5 rounded-full bg-brand-red" />
                <span>Unity · Unreal Engine · TypeScript</span>
              </div>
            </div>

            {/* Feature 4 */}
            <div className="glass-panel p-7 hover:bg-emerald-500/5 transition-all duration-300 group border border-white/10 hover:border-emerald-500/40 flex flex-col justify-between">
              <div>
                <div className="w-12 h-12 rounded-xl bg-emerald-500/20 flex items-center justify-center mb-6 text-emerald-400 group-hover:scale-110 transition-transform shadow-[0_0_20px_rgba(16,185,129,0.25)] border border-emerald-500/30">
                  <ShieldCheck className="w-6 h-6" />
                </div>
                <h3 className="text-lg font-bold mb-3 text-white uppercase tracking-wide group-hover:text-emerald-400 transition-colors">
                  Gasless Winner Pull-Claims
                </h3>
                <p className="text-zinc-400 text-sm leading-relaxed font-medium">
                  Winners pull-claim prizes straight to their Core or MetaMask wallet with zero custody risk.
                </p>
              </div>
              <div className="mt-5 pt-4 border-t border-white/5 flex items-center gap-2 text-xs text-zinc-500 font-mono">
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                <span>Zero Custody · Direct Settlement</span>
              </div>
            </div>
          </div>
        </section>

        {/* Showcase - tournament media (photos/clips) */}
        <TournamentShowcase />

        {/* The three hard problems — cold start, oracle-free results, the moat */}
        <WhyAmp />

        {/* Interactive Demo - the 4-step flow */}
        <MatchmakingDemo />

        {/* Live Technology Demo Section - QuickDraw Reaction Duel & EIP-191 Attestation */}
        <TechDemoSection />

        {/* Fuji Network - on-chain prize pools */}
        <FujiNetwork />

        {/* SDK Section */}
        <section id="api" className="py-32 max-w-7xl mx-auto px-6 scroll-mt-32 relative">
          <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-[120%] h-[300px] bg-brand-cyan/5 blur-[150px] rounded-full pointer-events-none" />
          <EngineTabs />
        </section>

        {/* Security Audits */}
        <SecurityAudits />

        {/* Investor / Partnership CTA */}
        <InvestorCTA />
      </main>

      {/* Footer */}
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
            <Link href="https://github.com/bradmyrick/Avalanche-Matchmaking-Protocol" className="hover:text-brand-cyan transition-colors">GitHub</Link>
            <Link href="mailto:brad@kodr.pro" className="hover:text-brand-cyan transition-colors">Contact</Link>
            <Link href="/terms" className="hover:text-brand-cyan transition-colors">Terms</Link>
            <Link href="/docs" className="hover:text-brand-cyan transition-colors">Docs</Link>
          </div>
        </div>
      </footer>
    </div>
  );
}
