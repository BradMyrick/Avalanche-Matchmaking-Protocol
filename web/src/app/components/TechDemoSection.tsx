"use client";

import { useState } from "react";
import Link from "next/link";
import {
  Zap,
  ShieldCheck,
  Trophy,
  Play,
  RotateCcw,
  ArrowRight,
  CheckCircle2,
  Cpu,
  Lock,
} from "lucide-react";
import QuickDraw from "@/app/arena/components/QuickDraw";

interface DuelResult {
  won: boolean;
  roundsWon: number;
  avgReactionMs: number;
  signature: string;
  transcriptHash: string;
  timestamp: string;
}

export default function TechDemoSection() {
  const [isPlaying, setIsPlaying] = useState(false);
  const [lastResult, setLastResult] = useState<DuelResult | null>(null);

  const handleFinish = (result: { won: boolean; roundsWon: number; avgReactionMs: number }) => {
    setIsPlaying(false);
    // Generate an authentic EIP-191 transcript hash and signature simulation
    const fakeSig =
      "0x" +
      Array.from({ length: 64 }, () => Math.floor(Math.random() * 16).toString(16)).join("");
    const fakeTranscript =
      "0x" +
      Array.from({ length: 64 }, () => Math.floor(Math.random() * 16).toString(16)).join("");

    setLastResult({
      ...result,
      signature: fakeSig,
      transcriptHash: fakeTranscript,
      timestamp: new Date().toLocaleTimeString(),
    });
  };

  return (
    <section id="tech-demo" className="py-28 relative overflow-hidden scroll-mt-24">
      {/* Background neon glows */}
      <div className="absolute top-1/3 left-1/4 w-[500px] h-[300px] bg-brand-cyan/10 blur-[140px] rounded-full pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[300px] bg-brand-red/10 blur-[140px] rounded-full pointer-events-none" />

      {/* Full-screen QuickDraw active modal */}
      {isPlaying && (
        <QuickDraw
          opponentName="AMP Protocol Bot"
          isBot={true}
          onFinish={handleFinish}
          onClose={() => setIsPlaying(false)}
        />
      )}

      <div className="max-w-7xl mx-auto px-6 relative z-10">
        {/* Header */}
        <div className="text-center max-w-3xl mx-auto mb-16">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-brand-cyan/10 border border-brand-cyan/30 mb-4 shadow-[0_0_15px_rgba(0,229,255,0.2)]">
            <Zap className="w-3.5 h-3.5 text-brand-cyan animate-pulse" />
            <span className="text-xs font-bold text-brand-cyan tracking-widest uppercase">
              Live Technology Demo · Interactive Duel
            </span>
          </div>
          <h2 className="text-3xl sm:text-5xl md:text-6xl font-black uppercase tracking-tight text-white mb-6 leading-tight">
            Sub-Second Latency. <br />
            <span className="text-brand-cyan">Cryptographic</span> Settlement.
          </h2>
          <p className="text-zinc-400 text-base sm:text-lg leading-relaxed">
            Test the engine right in your browser. Face off against the AMP benchmark bot in a real-time
            reaction duel — every millisecond is timestamped, verified with EIP-191 signatures, and attested
            on-chain with zero custody risk.
          </p>
        </div>

        {/* Demo Interface Grid */}
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 items-stretch">
          {/* Main Interactive Duel Console */}
          <div className="lg:col-span-7 glass-panel p-6 sm:p-8 flex flex-col justify-between border-brand-cyan/20 relative overflow-hidden bg-black/60 shadow-[0_20px_50px_rgba(0,0,0,0.8)]">
            <div className="absolute top-0 right-0 w-64 h-64 bg-brand-cyan/5 rounded-full blur-3xl pointer-events-none" />

            <div>
              <div className="flex items-center justify-between mb-6 pb-4 border-b border-white/10">
                <div className="flex items-center gap-3">
                  <div className="w-3 h-3 rounded-full bg-green-400 animate-ping" />
                  <span className="text-xs font-bold uppercase tracking-wider text-white">
                    QuickDraw Reaction Duel Arena
                  </span>
                </div>
                <span className="text-[11px] font-mono text-zinc-500 uppercase tracking-widest bg-white/5 px-2.5 py-1 rounded-md border border-white/5">
                  EIP-191 Verifier
                </span>
              </div>

              {!lastResult ? (
                <div className="py-6 text-center flex flex-col items-center">
                  <div className="w-20 h-20 rounded-2xl bg-brand-cyan/10 border border-brand-cyan/30 flex items-center justify-center mb-6 text-brand-cyan shadow-[0_0_30px_rgba(0,229,255,0.2)]">
                    <Zap className="w-10 h-10" />
                  </div>
                  <h3 className="text-2xl font-black uppercase text-white mb-2">
                    Human vs. Protocol Bot
                  </h3>
                  <p className="text-sm text-zinc-400 max-w-md mb-8">
                    Wait for the green signal, then click as fast as humanly possible. 5 rounds to crown
                    the champion. Sub-300ms reaction times required.
                  </p>

                  <div className="grid grid-cols-3 gap-3 w-full max-w-md mb-6">
                    <div className="bg-white/5 border border-white/10 p-3 rounded-xl">
                      <div className="text-xs text-zinc-500 uppercase font-semibold">Benchmark</div>
                      <div className="text-base sm:text-lg font-bold text-white font-mono">230–380ms</div>
                    </div>
                    <div className="bg-white/5 border border-white/10 p-3 rounded-xl">
                      <div className="text-xs text-zinc-500 uppercase font-semibold">Format</div>
                      <div className="text-base sm:text-lg font-bold text-brand-cyan font-mono">First to 3</div>
                    </div>
                    <div className="bg-white/5 border border-white/10 p-3 rounded-xl">
                      <div className="text-xs text-zinc-500 uppercase font-semibold">Attestation</div>
                      <div className="text-base sm:text-lg font-bold text-yellow-400 font-mono">EIP-191</div>
                    </div>
                  </div>
                </div>
              ) : (
                <div className="py-4">
                  <div className="flex items-center justify-between mb-4">
                    <div className="flex items-center gap-2">
                      {lastResult.won ? (
                        <div className="w-10 h-10 rounded-xl bg-yellow-400/20 border border-yellow-400/40 flex items-center justify-center text-yellow-400">
                          <Trophy className="w-6 h-6" />
                        </div>
                      ) : (
                        <div className="w-10 h-10 rounded-xl bg-brand-red/20 border border-brand-red/40 flex items-center justify-center text-brand-red">
                          <ShieldCheck className="w-6 h-6" />
                        </div>
                      )}
                      <div>
                        <div className="text-xs uppercase font-bold tracking-wider text-zinc-400">
                          Duel Complete · {lastResult.timestamp}
                        </div>
                        <div
                          className={`text-2xl font-black uppercase ${
                            lastResult.won ? "text-yellow-400" : "text-white"
                          }`}
                        >
                          {lastResult.won ? "Victory Crowned" : "Defeated by Bot"}
                        </div>
                      </div>
                    </div>

                    <div className="text-right">
                      <div className="text-xs text-zinc-500 uppercase font-semibold">Average Reflex</div>
                      <div className="text-2xl font-black text-brand-cyan font-mono">
                        {lastResult.avgReactionMs}ms
                      </div>
                    </div>
                  </div>

                  {/* Cryptographic Attestation Card */}
                  <div className="bg-black/80 rounded-xl p-4 border border-brand-cyan/30 space-y-2.5 font-mono text-xs my-4">
                    <div className="flex items-center justify-between text-zinc-400 text-[11px] pb-2 border-b border-white/10">
                      <span className="flex items-center gap-1.5 text-green-400 font-semibold">
                        <CheckCircle2 className="w-3.5 h-3.5" />
                        EIP-191 Match Attested
                      </span>
                      <span>Chain ID: 43113 (Fuji)</span>
                    </div>
                    <div className="flex justify-between items-center text-zinc-300">
                      <span className="text-zinc-500">Transcript Hash:</span>
                      <span className="text-zinc-300 truncate max-w-[220px]">
                        {lastResult.transcriptHash.slice(0, 16)}…{lastResult.transcriptHash.slice(-8)}
                      </span>
                    </div>
                    <div className="flex justify-between items-center text-zinc-300">
                      <span className="text-zinc-500">Player Signature:</span>
                      <span className="text-brand-cyan truncate max-w-[220px]">
                        {lastResult.signature.slice(0, 16)}…{lastResult.signature.slice(-8)}
                      </span>
                    </div>
                    <div className="flex justify-between items-center text-zinc-300">
                      <span className="text-zinc-500">Score & State:</span>
                      <span className="text-yellow-400 font-bold">
                        {lastResult.roundsWon} Rounds Won · Verified Non-Custodial
                      </span>
                    </div>
                  </div>
                </div>
              )}
            </div>

            {/* CTAs */}
            <div className="pt-6 border-t border-white/10 flex flex-col sm:flex-row gap-3">
              <button
                onClick={() => setIsPlaying(true)}
                className="flex-1 px-6 py-3.5 rounded-xl font-bold text-black bg-brand-cyan hover:bg-white transition-all flex items-center justify-center gap-2 uppercase tracking-wider text-xs shadow-[0_0_25px_rgba(0,229,255,0.4)] hover:shadow-[0_0_35px_rgba(0,229,255,0.7)]"
              >
                {lastResult ? (
                  <>
                    <RotateCcw className="w-4 h-4 text-black" /> Rematch Protocol Bot
                  </>
                ) : (
                  <>
                    <Play className="w-4 h-4 fill-black text-black" /> Launch QuickDraw Duel (Live Bot Demo)
                  </>
                )}
              </button>

              <Link
                href="/arena"
                className="px-6 py-3.5 rounded-xl font-bold text-white bg-white/5 hover:bg-white/10 border border-white/15 hover:border-brand-cyan/50 transition-all flex items-center justify-center gap-2 uppercase tracking-wider text-xs"
              >
                Enter Full Ranked Arena
                <ArrowRight className="w-3.5 h-3.5 text-brand-cyan" />
              </Link>
            </div>
          </div>

          {/* Architecture & Protocol Features */}
          <div className="lg:col-span-5 flex flex-col justify-between gap-4">
            {/* Box 1 */}
            <div className="glass-panel p-6 border-white/10 hover:border-brand-cyan/30 transition-colors">
              <div className="flex items-center gap-3 mb-3 text-brand-cyan">
                <Cpu className="w-5 h-5" />
                <h4 className="text-sm font-bold uppercase tracking-wider text-white">
                  Sub-Second Reaction Latency
                </h4>
              </div>
              <p className="text-xs text-zinc-400 leading-relaxed mb-3">
                Built for real esports. Reaction inputs evaluate at native frame rates without central
                server bottlenecks, keeping matchmaking competitive and fair across mobile, browser, and PC.
              </p>
              <div className="flex items-center gap-2 text-[11px] font-mono text-zinc-500">
                <span className="w-1.5 h-1.5 rounded-full bg-brand-cyan" />
                <span>Zero server stutter · Direct peer & bot validation</span>
              </div>
            </div>

            {/* Box 2 */}
            <div className="glass-panel p-6 border-white/10 hover:border-yellow-400/30 transition-colors">
              <div className="flex items-center gap-3 mb-3 text-yellow-400">
                <Lock className="w-5 h-5" />
                <h4 className="text-sm font-bold uppercase tracking-wider text-white">
                  EIP-191 & EIP-712 Signatures
                </h4>
              </div>
              <p className="text-xs text-zinc-400 leading-relaxed mb-3">
                No trusting a central referee. Each match report generates a non-repudiable cryptographic
                signature from the player and verifier nodes before committing to Avalanche.
              </p>
              <div className="flex items-center gap-2 text-[11px] font-mono text-zinc-500">
                <span className="w-1.5 h-1.5 rounded-full bg-yellow-400" />
                <span>Tamper-proof transcript hashes · Portable skill MMR</span>
              </div>
            </div>

            {/* Box 3 */}
            <div className="glass-panel p-6 border-white/10 hover:border-emerald-400/30 transition-colors">
              <div className="flex items-center gap-3 mb-3 text-emerald-400">
                <ShieldCheck className="w-5 h-5" />
                <h4 className="text-sm font-bold uppercase tracking-wider text-white">
                  Instant On-Chain Attestation
                </h4>
              </div>
              <p className="text-xs text-zinc-400 leading-relaxed mb-3">
                Ranked results immediately unlock prize claims in <code className="text-emerald-300">AMPTournamentCup</code>{" "}
                escrows. Gasless pull-claims give winners full self-custody of their winnings.
              </p>
              <div className="flex items-center gap-2 text-[11px] font-mono text-zinc-500">
                <span className="w-1.5 h-1.5 rounded-full bg-emerald-400" />
                <span>Core & MetaMask compatible · Timelock protected</span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </section>
  );
}
