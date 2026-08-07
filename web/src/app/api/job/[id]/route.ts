import { NextResponse } from "next/server";
import { getStore } from "@/lib/store";

export const runtime = "nodejs";

/** GET /api/job/[id] — relayer job status. When `done`, lazily provisions the
 *  tournaments/brackets rows (idempotent) so /manage can load the tournament. */
export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const jobId = Number(id);
  if (!Number.isFinite(jobId)) return NextResponse.json({ error: "bad id" }, { status: 400 });

  const store = getStore();
  const job = await store.getJob(jobId);
  if (!job) return NextResponse.json({ error: "not found" }, { status: 404 });

  // Provision DB rows once the relayer has funded a bracket-mode tournament.
  if (job.status === "done" && job.tournamentId != null) {
    const tid = job.tournamentId;
    const existing = await store.getTournament(tid);
    if (!existing) {
      const p = job.payload as {
        payoutBps?: number[];
        format?: BracketFormat;
        swissRounds?: number;
        players?: { id: number; wallet: string; name: string; seed: number }[];
        winnerWallets?: string[];
        fundedAvax?: number;
        paypalOrderId?: string;
      };
      await store.saveTournament({
        tournamentId: tid,
        sponsor: "custodial-relayer",
        prizePoolWei: Math.round((p.fundedAvax ?? 0) * 1e6).toString() + "000000000000",
        token: "0x0000000000000000000000000000000000000000",
        payoutBps: p.payoutBps ?? [],
        winnerWallets: p.winnerWallets ?? [],
        state: "OPEN",
        mode: "bracket",
        paypalOrderId: p.paypalOrderId,
        createdAt: Date.now(),
      });
      if (p.format && p.players) {
        await store.saveBracket(tid, {
          format: p.format,
          swissRounds: p.swissRounds,
          players: p.players,
          results: [],
        });
      }
    }
  }

  return NextResponse.json(job);
}

type BracketFormat = "single_elimination" | "round_robin" | "swiss";
