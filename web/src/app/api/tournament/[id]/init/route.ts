import { NextResponse } from "next/server";
import { getStore, type BracketState, type TournamentRecord } from "@/lib/store";

export const runtime = "nodejs";

/**
 * POST /api/tournament/[id]/init — after a sponsor funds a bracket-mode
 * tournament on-chain (AVAX path), the client calls this to register the
 * off-chain record + initial bracket state so /manage can load it.
 *
 * Body: { sponsor, prizePoolWei, payoutBps, format, swissRounds?, players, txHash? }
 */
export async function POST(
  request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const tid = Number(id);
  if (!Number.isFinite(tid)) {
    return NextResponse.json({ error: "bad id" }, { status: 400 });
  }
  const body = (await request.json().catch(() => ({}))) as {
    sponsor?: string;
    prizePoolWei?: string;
    payoutBps?: number[];
    format?: BracketState["format"];
    swissRounds?: number;
    players?: BracketState["players"];
    txHash?: string | null;
  };

  if (!body.payoutBps || !body.format || !Array.isArray(body.players)) {
    return NextResponse.json({ error: "missing fields" }, { status: 400 });
  }

  const store = getStore();
  const record: TournamentRecord = {
    tournamentId: tid,
    sponsor: body.sponsor ?? "0x0",
    prizePoolWei: body.prizePoolWei ?? "0",
    token: "0x0000000000000000000000000000000000000000",
    payoutBps: body.payoutBps,
    winnerWallets: [],
    state: "OPEN",
    mode: "bracket",
    txHash: body.txHash ?? null,
    createdAt: Date.now(),
  };
  await store.saveTournament(record);

  const bracket: BracketState = {
    format: body.format,
    swissRounds: body.swissRounds,
    players: body.players,
    results: [],
  };
  await store.saveBracket(tid, bracket);

  return NextResponse.json({ ok: true });
}
