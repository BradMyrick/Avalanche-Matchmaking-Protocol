import { NextResponse } from "next/server";
import { getStore } from "@/lib/store";

export const runtime = "nodejs";

/**
 * POST /api/tournament/[id]/finalize — enqueue a custodial finalize job.
 * Body: { winnerWallets: string[] }. The Rust relayer signs + submits; the web
 * never holds the key. For sponsor-funded (AVAX) brackets, finalization is done
 * client-side in the browser (the sponsor's wallet signs) — this route is only
 * the custodial path.
 */
export async function POST(
  request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const tid = Number(id);
  if (!Number.isFinite(tid)) return NextResponse.json({ error: "bad id" }, { status: 400 });

  const body = (await request.json().catch(() => ({}))) as { winnerWallets?: string[] };
  if (!body.winnerWallets?.length) {
    return NextResponse.json({ error: "winnerWallets required" }, { status: 400 });
  }

  const jobId = await getStore().enqueueJob("finalize", {
    tournamentId: tid,
    winnerWallets: body.winnerWallets,
  });
  return NextResponse.json({ ok: true, jobId, pending: true });
}
