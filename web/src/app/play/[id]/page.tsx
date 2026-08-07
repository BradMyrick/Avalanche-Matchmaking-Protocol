"use client";

import { useEffect, useMemo, useState, useCallback } from "react";
import Link from "next/link";
import { useParams } from "next/navigation";
import {
  ArrowLeft,
  Loader2,
  Swords,
  Trophy,
  Check,
  AlertTriangle,
  Clock,
} from "lucide-react";
import {
  Tournament,
  Outcome,
  TournamentState,
  type TournamentFormat,
} from "@/lib/engine";
import type { BracketState } from "@/lib/store";
import { connectWallet } from "@/lib/ampCup";

interface Player {
  id: number;
  wallet: string;
  name: string;
  seed: number;
}

function formatOf(b: BracketState): TournamentFormat {
  if (b.format === "swiss") return { kind: TournamentState.Swiss, rounds: b.swissRounds ?? 3 };
  if (b.format === "round_robin") return { kind: TournamentState.RoundRobin };
  return { kind: TournamentState.SingleElimination };
}

function reconstruct(b: BracketState | null): Tournament<number> | null {
  if (!b) return null;
  const entrants = b.players.map((p) => ({ id: p.id, seed: p.seed }));
  const t = Tournament.new<number>(formatOf(b), entrants);
  for (const r of b.results) {
    const o = r.outcome === "B" ? Outcome.B : r.outcome === "Draw" ? Outcome.Draw : Outcome.A;
    t.record(r.matchId, o);
  }
  return t;
}

export default function PlayPage() {
  const params = useParams<{ id: string }>();
  const tid = Number(params.id);
  const [bracket, setBracket] = useState<BracketState | null>(null);
  const [wallet, setWallet] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);
  const [busy, setBusy] = useState<string | null>(null);
  const [status, setStatus] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const load = useCallback(async () => {
    const res = await fetch(`/api/tournament/${tid}`);
    const json = (await res.json()) as { bracket?: BracketState };
    setBracket(json.bracket ?? null);
  }, [tid]);

  useEffect(() => {
    load().finally(() => setLoading(false));
  }, [load]);

  const engine = useMemo(() => reconstruct(bracket), [bracket]);
  const players = bracket?.players ?? [];
  const playerById = useMemo(() => {
    const m = new Map<number, Player>();
    players.forEach((p) => m.set(p.id, p));
    return m;
  }, [players]);

  const me: Player | undefined = useMemo(
    () => (wallet ? players.find((p) => p.wallet.toLowerCase() === wallet.toLowerCase()) : undefined),
    [wallet, players]
  );

  // The player's current pending match (if any).
  const myMatch = useMemo(() => {
    if (!engine || !me) return null;
    const pending = engine.pending();
    return engine.matches().find((m) => pending.includes(m.id) && (m.a?.id === me.id || m.b?.id === me.id)) ?? null;
  }, [engine, me]);

  const mySide = useMemo(() => {
    if (!myMatch || !me) return null;
    if (myMatch.a?.id === me.id) return "A" as const;
    if (myMatch.b?.id === me.id) return "B" as const;
    return null;
  }, [myMatch, me]);

  const opponent = useMemo(() => {
    if (!myMatch || !mySide) return null;
    const opSide = mySide === "A" ? myMatch.b : myMatch.a;
    return opSide ? playerById.get(opSide.id) ?? null : null;
  }, [myMatch, mySide, playerById]);

  async function connect() {
    try {
      setError(null);
      const provider = await connectWallet();
      const signer = await provider.getSigner();
      setWallet(await signer.getAddress());
    } catch (e) {
      setError((e as Error).message);
    }
  }

  async function report(outcome: "A" | "B" | "Draw") {
    if (!wallet || !myMatch) return;
    setBusy("Submitting result…");
    setError(null);
    try {
      // Normalize: the player reports from their perspective; the contract
      // outcome is absolute side. "I won" → my side; "I lost" → other side.
      const reported = outcome === "Draw" ? "Draw" : outcome === mySide ? mySide : mySide === "A" ? "B" : "A";
      const res = await fetch(`/api/tournament/${tid}/report`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ wallet, matchId: myMatch.id, outcome: reported }),
      });
      const json = (await res.json()) as { ok?: boolean; status?: string; error?: string };
      if (!json.ok) throw new Error(json.error || "report failed");
      setStatus(json.status ?? "waiting");
      await load();
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(null);
    }
  }

  if (loading) {
    return (
      <div className="min-h-screen bg-black flex items-center justify-center text-zinc-500">
        <Loader2 className="w-6 h-6 animate-spin" />
      </div>
    );
  }

  return (
    <div className="relative min-h-screen overflow-hidden antialiased bg-black text-white">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[500px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[500px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      <header className="relative z-10 max-w-3xl mx-auto px-6 pt-8 flex items-center justify-between">
        <Link href="/" className="inline-flex items-center gap-2 text-zinc-400 hover:text-brand-cyan transition-colors text-sm">
          <ArrowLeft className="w-4 h-4" /> AMP
        </Link>
        <Link href={`/cup/${tid}`} className="text-xs text-zinc-400 hover:text-brand-cyan">Spectator view →</Link>
      </header>

      <main className="relative z-10 max-w-xl mx-auto px-6 py-10">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-black uppercase tracking-tight mb-2">Player Console</h1>
          <p className="text-zinc-400 text-sm">Cup #{tid}</p>
        </div>

        {!wallet && (
          <div className="glass-panel p-8 text-center">
            <p className="text-zinc-300 mb-4">Connect the wallet you registered with to play.</p>
            <button
              onClick={connect}
              className="px-6 py-3 rounded-sm font-bold text-black bg-brand-cyan hover:bg-white transition-colors uppercase tracking-widest text-sm"
            >
              Connect Wallet
            </button>
            {error && <p className="text-xs text-brand-red mt-4 font-mono">{error}</p>}
          </div>
        )}

        {wallet && !me && (
          <div className="glass-panel p-6 text-center text-zinc-400">
            <AlertTriangle className="w-6 h-6 text-yellow-400 mx-auto mb-2" />
            You&rsquo;re not registered in this tournament.
          </div>
        )}

        {wallet && me && (
          <>
            {error && <div className="bg-brand-red/10 border border-brand-red/30 rounded-lg p-3 text-sm text-brand-red font-mono mb-4">{error}</div>}

            {!myMatch && (
              <div className="glass-panel p-6 text-center">
                {engine?.isComplete() ? (
                  <>
                    <Trophy className="w-8 h-8 text-yellow-400 mx-auto mb-2" />
                    <p className="text-white font-bold">Tournament complete.</p>
                    {engine.winners()[0] === me.id ? (
                      <p className="text-yellow-400 mt-1">You won the cup! 🏆</p>
                    ) : (
                      <p className="text-zinc-400 mt-1">Thanks for playing.</p>
                    )}
                  </>
                ) : (
                  <>
                    <Clock className="w-6 h-6 text-zinc-500 mx-auto mb-2" />
                    <p className="text-zinc-300">No active match for you right now.</p>
                    <p className="text-xs text-zinc-500 mt-1">Waiting for the bracket to advance.</p>
                  </>
                )}
              </div>
            )}

            {myMatch && mySide && opponent && (
              <div className="glass-panel p-6">
                <div className="text-center mb-5">
                  <div className="text-xs uppercase tracking-widest text-brand-cyan mb-1">Your match</div>
                  <div className="flex items-center justify-center gap-4">
                    <div className={`text-center ${mySide === "A" ? "text-brand-cyan" : "text-zinc-400"}`}>
                      <div className="text-lg font-black">{mySide === "A" ? me.name : opponent.name}</div>
                      <div className="text-[10px]">Side A</div>
                    </div>
                    <Swords className="w-5 h-5 text-zinc-600" />
                    <div className={`text-center ${mySide === "B" ? "text-brand-cyan" : "text-zinc-400"}`}>
                      <div className="text-lg font-black">{mySide === "B" ? me.name : opponent.name}</div>
                      <div className="text-[10px]">Side B</div>
                    </div>
                  </div>
                </div>

                <div className="text-xs uppercase tracking-wider text-zinc-500 text-center mb-3">Report your result</div>
                <div className="grid grid-cols-3 gap-2">
                  <button
                    onClick={() => report(mySide)}
                    disabled={!!busy}
                    className="p-3 rounded-lg bg-green-500/10 border border-green-500/30 text-green-400 hover:bg-green-500/20 text-sm font-bold uppercase disabled:opacity-40"
                  >
                    I won
                  </button>
                  <button
                    onClick={() => report("Draw")}
                    disabled={!!busy}
                    className="p-3 rounded-lg bg-white/5 border border-white/10 text-zinc-300 hover:bg-white/10 text-sm font-bold uppercase disabled:opacity-40"
                  >
                    Draw
                  </button>
                  <button
                    onClick={() => report(mySide === "A" ? "B" : "A")}
                    disabled={!!busy}
                    className="p-3 rounded-lg bg-brand-red/10 border border-brand-red/30 text-brand-red hover:bg-brand-red/20 text-sm font-bold uppercase disabled:opacity-40"
                  >
                    I lost
                  </button>
                </div>

                {busy && <p className="text-center text-zinc-400 text-sm mt-4">{busy}</p>}

                {status === "waiting" && !busy && (
                  <p className="text-center text-zinc-400 text-sm mt-4 flex items-center justify-center gap-2">
                    <Clock className="w-4 h-4" /> Reported — waiting for {opponent.name} to confirm.
                  </p>
                )}
                {status === "confirmed" && (
                  <p className="text-center text-green-400 text-sm mt-4 flex items-center justify-center gap-2">
                    <Check className="w-4 h-4" /> Confirmed — bracket advancing.
                  </p>
                )}
                {status === "disputed" && (
                  <p className="text-center text-yellow-400 text-sm mt-4 flex items-center justify-center gap-2">
                    <AlertTriangle className="w-4 h-4" /> Disagreement — flagged for the organizer.
                  </p>
                )}
              </div>
            )}

            <div className="mt-6 text-center text-xs text-zinc-500">
              Playing as <span className="text-zinc-300">{me.name}</span> · refresh to update
              <button onClick={() => load()} className="ml-2 text-brand-cyan hover:underline">refresh</button>
            </div>
          </>
        )}
      </main>
    </div>
  );
}
