"use client";

import React, { useState, useMemo } from "react";
import {
  Trophy,
  Swords,
  Coins,
  ShieldCheck,
  RotateCcw,
  Play,
  ExternalLink,
  Search,
} from "lucide-react";
import {
  Tournament,
  Outcome,
  TournamentState,
  TournamentFormat,
  BracketMatch,
  isReady,
} from "@/lib/engine";
import { AMPCUP_ABI, CUP_ADDRESS, FUJI_RPC, EXPLORER_URL, PROTOCOL_FEE_BPS, PROTOCOL_FEE_LABEL } from "@/lib/ampCup";
import { ethers } from "ethers";

interface DemoPlayer {
  id: number;
  name: string;
  wallet: string;
  seed: number;
}

const INITIAL_PLAYERS: DemoPlayer[] = [
  { id: 1, name: "Valkyrie", wallet: "0x70997970C51812dc3A010C7d01b50e0d17dc79C8", seed: 1 },
  { id: 2, name: "Shadowblade", wallet: "0x3C44CdDdB6a900fa2b585dd299e03d12FA4293BC", seed: 2 },
  { id: 3, name: "Frostbite", wallet: "0x90F79bf6EB2c4f870365E785982E1f101E93b906", seed: 3 },
  { id: 4, name: "Nova", wallet: "0x15d34AAf54267DB7D7c367839AAf71A00a2C6A65", seed: 4 },
  { id: 5, name: "Cipher", wallet: "0x9965507D1a55bcC2695C58ba16FB37d819B0A4dc", seed: 5 },
  { id: 6, name: "Apex", wallet: "0x976EA74026E72CD5554282bf94FE6247749223Fe", seed: 6 },
  { id: 7, name: "Specter", wallet: "0xbDA5747bFD65F08deb54cb465eB87D40e51B197E", seed: 7 },
  { id: 8, name: "Echo", wallet: "0x2546BcD3c84621e976D8185a91A922aE77ECEc30", seed: 8 },
];

export default function TournamentInteractiveDemo() {
  const [activeTab, setActiveTab] = useState<"simulator" | "inspector">("simulator");
  const [format, setFormat] = useState<"single" | "swiss">("single");
  const [matchResults, setMatchResults] = useState<{ matchId: number; outcome: Outcome }[]>([]);

  // On-Chain Fuji Inspector State
  const [inspectId, setInspectId] = useState("1");
  const [inspectLoading, setInspectLoading] = useState(false);
  const [inspectData, setInspectData] = useState<{
    id: string;
    sponsor: string;
    prizePoolAvax: string;
    state: string;
    payoutBps: number[];
    winners: string[];
  } | null>(null);
  const [inspectError, setInspectError] = useState<string | null>(null);

  // Find player by ID
  const playerMap = useMemo(() => {
    const map = new Map<number, DemoPlayer>();
    for (const p of INITIAL_PLAYERS) map.set(p.id, p);
    return map;
  }, []);

  // Reconstruct tournament engine from recorded results
  const tournament = useMemo(() => {
    const entrants = INITIAL_PLAYERS.map((p) => ({ id: p.id, seed: p.seed }));
    const formatSpec: TournamentFormat =
      format === "swiss"
        ? { kind: TournamentState.Swiss, rounds: 3 }
        : { kind: TournamentState.SingleElimination };

    const t = Tournament.new<number>(formatSpec, entrants);
    for (const r of matchResults) {
      try {
        t.record(r.matchId, r.outcome);
      } catch {
        // ignore out of order
      }
    }
    return t;
  }, [format, matchResults]);

  const readyMatches = tournament.matches().filter(isReady);
  const isComplete = tournament.isComplete();
  const standings = tournament.standings();

  const handlePickWinner = (matchId: number, outcome: Outcome) => {
    setMatchResults((prev) => [...prev, { matchId, outcome }]);
  };

  const handleReset = () => {
    setMatchResults([]);
  };

  const handleSimulateAll = () => {
    const results: { matchId: number; outcome: Outcome }[] = [];
    const entrants = INITIAL_PLAYERS.map((p) => ({ id: p.id, seed: p.seed }));
    const formatSpec: TournamentFormat =
      format === "swiss"
        ? { kind: TournamentState.Swiss, rounds: 3 }
        : { kind: TournamentState.SingleElimination };

    const t = Tournament.new<number>(formatSpec, entrants);

    while (!t.isComplete()) {
      const pending = t.matches().filter(isReady);
      if (pending.length === 0) break;
      for (const m of pending) {
        const p1 = m.a ? playerMap.get(m.a.id) : null;
        const p2 = m.b ? playerMap.get(m.b.id) : null;
        const outcome = (p1?.seed ?? 99) <= (p2?.seed ?? 99) ? Outcome.A : Outcome.B;
        t.record(m.id, outcome);
        results.push({ matchId: m.id, outcome });
      }
    }

    setMatchResults(results);
  };

  // Inspect live Fuji tournament on-chain
  const handleInspectFuji = async () => {
    setInspectLoading(true);
    setInspectError(null);
    setInspectData(null);
    try {
      const provider = new ethers.JsonRpcProvider(FUJI_RPC);
      const contract = new ethers.Contract(CUP_ADDRESS, AMPCUP_ABI, provider);
      const cup = await contract.getTournament(BigInt(inspectId));

      const states = ["Open", "Finalized", "Complete", "Cancelled"];
      setInspectData({
        id: inspectId,
        sponsor: cup.sponsor,
        prizePoolAvax: ethers.formatEther(cup.prizePool),
        state: states[Number(cup.state)] ?? "Unknown",
        payoutBps: Array.from(cup.payoutBps as ArrayLike<number>).map((n) => Number(n)),
        winners: Array.from(cup.winners as ArrayLike<string>),
      });
    } catch (err: unknown) {
      setInspectError((err as Error).message || "Failed to query Fuji contract");
    } finally {
      setInspectLoading(false);
    }
  };

  const prizePoolAvax = 5.0;
  const netPoolAvax = prizePoolAvax * (1 - PROTOCOL_FEE_BPS / 10000);
  const prizes = [
    { place: "1st", bps: 6000, avax: (netPoolAvax * 0.6).toFixed(3) },
    { place: "2nd", bps: 3000, avax: (netPoolAvax * 0.3).toFixed(3) },
    { place: "3rd", bps: 1000, avax: (netPoolAvax * 0.1).toFixed(3) },
  ];

  return (
    <section id="demo" className="py-24 max-w-7xl mx-auto px-6 scroll-mt-24 relative">
      {/* Background Glow */}
      <div className="absolute top-1/3 left-1/4 w-[500px] h-[300px] bg-brand-cyan/10 blur-[140px] rounded-full pointer-events-none" />
      <div className="absolute bottom-1/4 right-1/4 w-[500px] h-[300px] bg-yellow-400/10 blur-[140px] rounded-full pointer-events-none" />

      <div className="relative z-10">
        {/* Section Header */}
        <div className="text-center max-w-3xl mx-auto mb-12">
          <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-brand-cyan/10 border border-brand-cyan/30 mb-4 shadow-[0_0_15px_rgba(0,229,255,0.2)]">
            <Trophy className="w-3.5 h-3.5 text-brand-cyan animate-pulse" />
            <span className="text-xs font-bold text-brand-cyan tracking-widest uppercase">
              Interactive Protocol Demo
            </span>
          </div>
                    <h2 className="text-3xl sm:text-5xl font-black uppercase tracking-tight text-white mb-3">
            Tournament <span className="text-brand-cyan">Engine</span>
          </h2>
          <p className="text-zinc-400">
            Local bracket. Then the cup on Fuji.
          </p>

          {/* Toggle Tabs */}
          <div className="inline-flex p-1 rounded-xl bg-white/5 border border-white/10 mt-6">
            <button
              onClick={() => setActiveTab("simulator")}
              className={`px-5 py-2 rounded-lg text-xs font-bold uppercase tracking-wider transition-all flex items-center gap-2 ${
                activeTab === "simulator"
                  ? "bg-brand-cyan text-black shadow-[0_0_15px_rgba(0,229,255,0.4)]"
                  : "text-zinc-400 hover:text-white"
              }`}
            >
              <Swords className="w-4 h-4" />
              Bracket Simulator
            </button>
            <button
              onClick={() => {
                setActiveTab("inspector");
                if (!inspectData) handleInspectFuji();
              }}
              className={`px-5 py-2 rounded-lg text-xs font-bold uppercase tracking-wider transition-all flex items-center gap-2 ${
                activeTab === "inspector"
                  ? "bg-brand-cyan text-black shadow-[0_0_15px_rgba(0,229,255,0.4)]"
                  : "text-zinc-400 hover:text-white"
              }`}
            >
              <Search className="w-4 h-4" />
              Fuji On-Chain Inspector
            </button>
          </div>
        </div>

        {/* Tab 1: Bracket Simulator */}
        {activeTab === "simulator" && (
          <div className="glass-panel p-6 sm:p-8 border border-white/10 bg-black/60 shadow-[0_20px_50px_rgba(0,0,0,0.8)] rounded-3xl">
            {/* Control Bar */}
            <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 pb-6 mb-6 border-b border-white/10">
              <div className="flex items-center gap-3">
                <span className="text-xs font-bold uppercase text-zinc-400">Format:</span>
                <button
                  onClick={() => {
                    setFormat("single");
                    handleReset();
                  }}
                  className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
                    format === "single"
                      ? "bg-brand-cyan/20 border border-brand-cyan text-brand-cyan"
                      : "bg-white/5 text-zinc-400 hover:text-white border border-white/5"
                  }`}
                >
                  Single Elimination (8-Player)
                </button>
                <button
                  onClick={() => {
                    setFormat("swiss");
                    handleReset();
                  }}
                  className={`px-3 py-1.5 rounded-lg text-xs font-bold transition-all ${
                    format === "swiss"
                      ? "bg-brand-cyan/20 border border-brand-cyan text-brand-cyan"
                      : "bg-white/5 text-zinc-400 hover:text-white border border-white/5"
                  }`}
                >
                  Swiss (3 Rounds)
                </button>
              </div>

              <div className="flex items-center gap-2 w-full sm:w-auto">
                {!isComplete && (
                  <button
                    onClick={handleSimulateAll}
                    className="flex-1 sm:flex-none px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-wider bg-white/10 hover:bg-white/20 text-white transition-all flex items-center justify-center gap-2 border border-white/10"
                  >
                    <Play className="w-3.5 h-3.5 text-yellow-400 fill-yellow-400" />
                    Simulate Remaining
                  </button>
                )}
                <button
                  onClick={handleReset}
                  className="px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-wider bg-white/5 hover:bg-white/10 text-zinc-400 hover:text-white transition-all flex items-center gap-1.5 border border-white/10"
                >
                  <RotateCcw className="w-3.5 h-3.5" />
                  Reset
                </button>
              </div>
            </div>

            {/* Main Interactive Stage */}
            <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
              {/* Left Column: Active Matches */}
              <div className="lg:col-span-7">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-sm font-bold uppercase tracking-wider text-zinc-300 flex items-center gap-2">
                    <Swords className="w-4 h-4 text-brand-cyan" />
                    {isComplete ? "Tournament Completed" : "Current Round Matches"}
                  </h3>
                  <span className="text-xs font-mono text-zinc-500">
                    {matchResults.length} matches recorded
                  </span>
                </div>

                {!isComplete ? (
                  <div className="space-y-3">
                    {readyMatches.map((match: BracketMatch<number>) => {
                      const p1 = match.a ? playerMap.get(match.a.id) : null;
                      const p2 = match.b ? playerMap.get(match.b.id) : null;
                      return (
                        <div
                          key={match.id}
                          className="p-4 rounded-2xl bg-white/5 border border-white/10 hover:border-brand-cyan/30 transition-all flex items-center justify-between"
                        >
                          {/* Player 1 */}
                          <div className="flex-1">
                            <div className="text-xs text-zinc-500 font-mono">Seed #{p1?.seed}</div>
                            <div className="font-bold text-white text-base">{p1?.name}</div>
                          </div>

                          {/* Action Buttons */}
                          <div className="flex items-center gap-2 px-3">
                            <button
                              onClick={() => handlePickWinner(match.id, Outcome.A)}
                              className="px-3 py-1.5 rounded-lg text-xs font-bold bg-brand-cyan/15 hover:bg-brand-cyan text-brand-cyan hover:text-black border border-brand-cyan/30 transition-all"
                            >
                              Win
                            </button>
                            <span className="text-xs font-mono text-zinc-600">vs</span>
                            <button
                              onClick={() => handlePickWinner(match.id, Outcome.B)}
                              className="px-3 py-1.5 rounded-lg text-xs font-bold bg-brand-cyan/15 hover:bg-brand-cyan text-brand-cyan hover:text-black border border-brand-cyan/30 transition-all"
                            >
                              Win
                            </button>
                          </div>

                          {/* Player 2 */}
                          <div className="flex-1 text-right">
                            <div className="text-xs text-zinc-500 font-mono">Seed #{p2?.seed}</div>
                            <div className="font-bold text-white text-base">{p2?.name}</div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                ) : (
                  <div className="p-8 rounded-2xl bg-brand-cyan/5 border border-brand-cyan/30 text-center">
                    <div className="w-14 h-14 rounded-2xl bg-yellow-400/20 border border-yellow-400/40 flex items-center justify-center mx-auto mb-4 text-yellow-400 shadow-[0_0_30px_rgba(250,204,21,0.3)]">
                      <Trophy className="w-8 h-8" />
                    </div>
                    <h4 className="text-2xl font-black uppercase text-white mb-2">
                      Champion: {playerMap.get(standings[0]?.id)?.name}
                    </h4>
                    <p className="text-sm text-zinc-400 mb-6">
                      Bracket finished in the SDK.
                    </p>
                    <button
                      onClick={handleReset}
                      className="px-6 py-2.5 rounded-xl text-xs font-bold uppercase tracking-wider bg-brand-cyan text-black hover:bg-white transition-all"
                    >
                      Run Another Bracket
                    </button>
                  </div>
                )}
              </div>

              {/* Right Column: Standings & Attestation Output */}
              <div className="lg:col-span-5 flex flex-col justify-between space-y-6">
                {/* Standings */}
                <div className="p-5 rounded-2xl bg-white/5 border border-white/10">
                  <div className="mb-4">
                    <span className="text-xs font-bold uppercase tracking-wider text-zinc-400 flex items-center gap-1.5">
                      <Coins className="w-4 h-4 text-yellow-400" />
                      5 AVAX pool · {PROTOCOL_FEE_LABEL} fee
                    </span>
                  </div>

                  <div className="space-y-2.5">
                    {prizes.map((prize, idx) => {
                      const winnerPlayer = isComplete ? playerMap.get(standings[idx]?.id) : undefined;
                      return (
                        <div
                          key={prize.place}
                          className="flex items-center justify-between p-3 rounded-xl bg-black/40 border border-white/5"
                        >
                          <div className="flex items-center gap-3">
                            <span
                              className={`w-6 h-6 rounded-md flex items-center justify-center text-xs font-black ${
                                idx === 0
                                  ? "bg-yellow-400/20 text-yellow-400 border border-yellow-400/40"
                                  : idx === 1
                                  ? "bg-zinc-300/20 text-zinc-300 border border-zinc-300/40"
                                  : "bg-amber-600/20 text-amber-500 border border-amber-600/40"
                              }`}
                            >
                              {idx + 1}
                            </span>
                            <div>
                              <div className="text-xs font-bold text-white">
                                {winnerPlayer ? winnerPlayer.name : "TBD"}
                              </div>
                              <div className="text-[10px] font-mono text-zinc-500">
                                {prize.bps / 100}% payout
                              </div>
                            </div>
                          </div>
                          <div className="text-right">
                            <div className="text-sm font-bold font-mono text-brand-cyan">
                              {prize.avax} AVAX
                            </div>
                          </div>
                        </div>
                      );
                    })}
                  </div>
                </div>

                {/* EIP-712 Attestation Card */}
                <div className="p-5 rounded-2xl bg-black/70 border border-brand-cyan/20 font-mono text-xs">
                  <div className="flex items-center justify-between text-zinc-400 mb-2">
                    <span className="flex items-center gap-1.5 text-brand-cyan font-bold uppercase tracking-wider text-[11px]">
                      <ShieldCheck className="w-3.5 h-3.5 text-brand-cyan" />
                      EIP-712 Attestation
                    </span>
                    <span className="text-[10px] text-zinc-500">Chain: 43113</span>
                  </div>
                  <div className="text-[11px] text-zinc-300 space-y-1">
                    <div>
                      <span className="text-zinc-500">Type: </span>
                      <span className="text-yellow-400 font-semibold">TournamentResult(uint256,address[])</span>
                    </div>
                    <div>
                      <span className="text-zinc-500">Verifying Contract: </span>
                      <span className="text-white text-[10px]">
                        {CUP_ADDRESS.slice(0, 10)}...{CUP_ADDRESS.slice(-8)}
                      </span>
                    </div>
                    <div className="pt-2 border-t border-white/5 flex items-center justify-between text-[10px]">
                      <span className="text-zinc-500">Winner 1st:</span>
                      <span className="text-brand-cyan">
                        {standings[0] ? playerMap.get(standings[0].id)?.wallet.slice(0, 14) + "..." : "Pending"}
                      </span>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Tab 2: Live Fuji On-Chain Inspector */}
        {activeTab === "inspector" && (
          <div className="glass-panel p-6 sm:p-8 border border-white/10 bg-black/60 shadow-[0_20px_50px_rgba(0,0,0,0.8)] rounded-3xl">
            <div className="max-w-2xl mx-auto">
              <div className="flex items-center gap-3 mb-6">
                <input
                  type="number"
                  min="1"
                  value={inspectId}
                  onChange={(e) => setInspectId(e.target.value)}
                  placeholder="Tournament ID (e.g. 1, 8)"
                  className="flex-1 bg-white/5 border border-white/10 rounded-xl px-4 py-3 text-white font-mono text-sm focus:outline-none focus:border-brand-cyan"
                />
                <button
                  onClick={handleInspectFuji}
                  disabled={inspectLoading}
                  className="px-6 py-3 rounded-xl bg-brand-cyan text-black font-bold uppercase tracking-wider text-xs hover:bg-white transition-all flex items-center gap-2"
                >
                  {inspectLoading ? (
                    <span>Querying Fuji...</span>
                  ) : (
                    <>
                      <Search className="w-4 h-4" /> Query On-Chain
                    </>
                  )}
                </button>
              </div>

              {inspectError && (
                <div className="p-4 rounded-xl bg-red-500/10 border border-red-500/30 text-red-400 text-xs mb-6">
                  {inspectError}
                </div>
              )}

              {inspectData && (
                <div className="p-6 rounded-2xl bg-white/5 border border-brand-cyan/30 space-y-4">
                  <div className="flex items-center justify-between pb-4 border-b border-white/10">
                    <div>
                      <span className="text-xs text-zinc-500 uppercase font-semibold">Tournament Cup</span>
                      <div className="text-2xl font-black text-white">#{inspectData.id}</div>
                    </div>
                    <span className="px-3 py-1 rounded-full text-xs font-bold uppercase tracking-wider bg-brand-cyan/20 border border-brand-cyan text-brand-cyan">
                      {inspectData.state}
                    </span>
                  </div>

                  <div className="grid grid-cols-2 gap-4 text-xs">
                    <div>
                      <span className="text-zinc-500">Prize Pool:</span>
                      <div className="text-lg font-bold font-mono text-yellow-400">
                        {inspectData.prizePoolAvax} AVAX
                      </div>
                    </div>
                    <div>
                      <span className="text-zinc-500">Payout Splits:</span>
                      <div className="text-sm font-bold font-mono text-white">
                        {inspectData.payoutBps.map((b) => `${b / 100}%`).join(" / ")}
                      </div>
                    </div>
                  </div>

                  <div className="pt-2 text-xs">
                    <span className="text-zinc-500 block mb-1">Sponsor:</span>
                    <a
                      href={`${EXPLORER_URL}/address/${inspectData.sponsor}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="font-mono text-brand-cyan hover:underline flex items-center gap-1"
                    >
                      {inspectData.sponsor}
                      <ExternalLink className="w-3 h-3" />
                    </a>
                  </div>

                  {inspectData.winners.length > 0 && (
                    <div className="pt-2 text-xs">
                      <span className="text-zinc-500 block mb-1">Winners ({inspectData.winners.length}):</span>
                      <div className="space-y-1">
                        {inspectData.winners.map((w, i) => (
                          <div key={i} className="font-mono text-zinc-300 text-[11px] bg-black/40 p-1.5 rounded">
                            #{i + 1}: {w}
                          </div>
                        ))}
                      </div>
                    </div>
                  )}

                  <div className="pt-4 border-t border-white/10 flex justify-end">
                    <a
                      href={`${EXPLORER_URL}/address/${CUP_ADDRESS}`}
                      target="_blank"
                      rel="noopener noreferrer"
                      className="text-xs text-brand-cyan hover:underline flex items-center gap-1.5 font-semibold"
                    >
                      View Contract on Snowtrace <ExternalLink className="w-3.5 h-3.5" />
                    </a>
                  </div>
                </div>
              )}
            </div>
          </div>
        )}
      </div>
    </section>
  );
}
