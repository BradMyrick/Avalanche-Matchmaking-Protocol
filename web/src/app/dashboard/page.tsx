"use client";

import { useState, useEffect, useCallback } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { ethers } from "ethers";
import {
  ArrowLeft,
  Trophy,
  Wallet,
  Loader2,
  ExternalLink,
  Plus,
  ChevronRight,
} from "lucide-react";
import { connectWallet } from "@/lib/ampCup";

interface Tournament {
  tournamentId: number;
  sponsor: string;
  prizePoolWei: string;
  payoutBps: number[];
  winnerWallets: string[];
  state: string;
  mode?: string;
  createdAt: number;
}

export default function DashboardPage() {
  const router = useRouter();
  const [wallet, setWallet] = useState<string | null>(null);
  const [tournaments, setTournaments] = useState<Tournament[]>([]);
  const [loading, setLoading] = useState(false);
  const [busy, setBusy] = useState<string | null>(null);
  const [error, setError] = useState<string | null>(null);

  const loadTournaments = useCallback(async (w: string) => {
    setLoading(true);
    try {
      const res = await fetch(`/api/tournaments?wallet=${w}`);
      const json = (await res.json()) as { tournaments?: Tournament[] };
      setTournaments(json.tournaments ?? []);
    } catch {
      setTournaments([]);
    } finally {
      setLoading(false);
    }
  }, []);

  async function connect() {
    setError(null);
    setBusy("Connecting wallet…");
    try {
      const provider = await connectWallet();
      const signer = await provider.getSigner();
      const addr = await signer.getAddress();
      setWallet(addr);
      await loadTournaments(addr);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(null);
    }
  }

  async function manage(tid: number) {
    if (!wallet) return;
    setBusy(`Authenticating for Cup #${tid}…`);
    setError(null);
    try {
      const provider = await connectWallet();
      const signer = await provider.getSigner();
      const ts = Math.floor(Date.now() / 1000);
      const message = `AMP-manage:${tid}:${ts}`;
      const sig = await signer.signMessage(message);

      const res = await fetch(`/api/tournament/${tid}/auth`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ wallet, sig, ts }),
      });
      const json = (await res.json()) as { manageToken?: string; error?: string };
      if (!json.manageToken) throw new Error(json.error || "auth failed");

      sessionStorage.setItem(`amp_manage_${tid}`, json.manageToken);
      router.push(`/manage/${tid}`);
    } catch (e) {
      setError((e as Error).message);
    } finally {
      setBusy(null);
    }
  }

  const stateColor = (s: string) =>
    s === "OPEN" ? "text-brand-cyan" : s === "FINALIZED" || s === "COMPLETE" ? "text-green-400" : "text-zinc-500";

  return (
    <div className="relative min-h-screen overflow-hidden antialiased bg-black text-white">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[500px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 -right-1/4 w-[150%] h-[500px] bg-brand-red/10 blur-[120px] rounded-full pointer-events-none" />

      <header className="relative z-10 max-w-5xl mx-auto px-6 pt-8 flex items-center justify-between">
        <Link href="/" className="inline-flex items-center gap-2 text-zinc-400 hover:text-brand-cyan text-sm">
          <ArrowLeft className="w-4 h-4" /> AMP
        </Link>
        <Link href="/setup" className="text-xs text-brand-cyan hover:underline flex items-center gap-1">
          <Plus className="w-3 h-3" /> New Tournament
        </Link>
      </header>

      <main className="relative z-10 max-w-5xl mx-auto px-6 py-10">
        <div className="mb-8">
          <h1 className="text-3xl font-black uppercase tracking-tight mb-1">
            Organizer <span className="text-brand-cyan">Dashboard</span>
          </h1>
          <p className="text-zinc-400 text-sm">
            {wallet ? `Connected: ${wallet.slice(0, 6)}…${wallet.slice(-4)}` : "Connect your wallet to manage your tournaments."}
          </p>
        </div>

        {error && (
          <div className="bg-brand-red/10 border border-brand-red/30 rounded-lg p-3 text-sm text-brand-red font-mono mb-4">
            {error}
          </div>
        )}

        {!wallet && (
          <div className="glass-panel p-8 text-center">
            <Wallet className="w-10 h-10 text-brand-cyan mx-auto mb-4" />
            <p className="text-zinc-300 mb-4">Connect the wallet you used to create tournaments.</p>
            <button
              onClick={connect}
              disabled={!!busy}
              className="px-6 py-3 rounded-sm font-bold text-black bg-brand-cyan hover:bg-white transition-colors uppercase tracking-widest text-sm disabled:opacity-40"
            >
              {busy ?? "Connect Wallet"}
            </button>
          </div>
        )}

        {wallet && loading && (
          <div className="flex items-center justify-center py-12 text-zinc-500">
            <Loader2 className="w-6 h-6 animate-spin" />
          </div>
        )}

        {wallet && !loading && (
          <>
            {tournaments.length === 0 ? (
              <div className="glass-panel p-8 text-center text-zinc-400">
                <Trophy className="w-8 h-8 text-zinc-600 mx-auto mb-3" />
                <p>No tournaments found for this wallet.</p>
                <Link href="/setup" className="text-brand-cyan hover:underline text-sm mt-2 inline-block">
                  Create your first tournament →
                </Link>
              </div>
            ) : (
              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                {tournaments.map((t) => (
                  <div key={t.tournamentId} className="glass-panel p-5 group hover:border-brand-cyan/30 transition-colors">
                    <div className="flex items-start justify-between mb-3">
                      <div>
                        <div className="flex items-center gap-2 mb-1">
                          <span className="text-xs font-bold uppercase tracking-wider text-brand-cyan">Cup #{t.tournamentId}</span>
                          <span className={`text-[10px] font-bold uppercase ${stateColor(t.state)}`}>{t.state}</span>
                        </div>
                        <div className="text-sm text-zinc-300">
                          {t.mode === "bracket" ? "Bracket" : "Instant"} · {ethers.formatEther(t.prizePoolWei).slice(0, 6)} AVAX
                        </div>
                      </div>
                    </div>
                    <div className="flex items-center gap-2 text-xs text-zinc-500 mb-4">
                      <span>{t.payoutBps.length} placement{t.payoutBps.length > 1 ? "s" : ""}</span>
                      <span>·</span>
                      <span>{new Date(t.createdAt).toLocaleDateString()}</span>
                    </div>
                    {t.state === "OPEN" || t.state === "FINALIZED" ? (
                      <button
                        onClick={() => manage(t.tournamentId)}
                        disabled={!!busy}
                        className="w-full px-4 py-2.5 rounded-sm font-bold text-black bg-brand-cyan/80 hover:bg-brand-cyan transition-colors uppercase tracking-widest text-xs disabled:opacity-40 flex items-center justify-center gap-2"
                      >
                        {busy?.includes(String(t.tournamentId)) ? busy : (
                          <>
                            {t.state === "OPEN" ? "Manage" : "View"} <ChevronRight className="w-3 h-3" />
                          </>
                        )}
                      </button>
                    ) : (
                      <div className="text-xs text-zinc-600 text-center py-2.5">—</div>
                    )}
                  </div>
                ))}
              </div>
            )}
          </>
        )}

        <div className="mt-10 pt-6 border-t border-white/10">
          <p className="text-xs text-zinc-600 text-center">
            Building your own integration? The AMP HTTP API is documented at{" "}
            <a href="https://docs.page/bradmyrick/Avalanche-Matchmaking-Protocol" target="_blank" rel="noreferrer" className="text-brand-cyan hover:underline">
              docs.page
            </a>. Every action on this dashboard is also available via <code className="text-zinc-500">/api/*</code>.
          </p>
        </div>
      </main>
    </div>
  );
}
