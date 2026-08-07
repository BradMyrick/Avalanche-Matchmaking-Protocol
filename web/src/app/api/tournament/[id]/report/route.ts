import { NextResponse } from "next/server";
import { getStore, type BracketState } from "@/lib/store";
import { Tournament, Outcome, TournamentState, type TournamentFormat } from "@/lib/engine";

export const runtime = "nodejs";

function formatOf(b: BracketState): TournamentFormat {
  if (b.format === "swiss") return { kind: TournamentState.Swiss, rounds: b.swissRounds ?? 3 };
  if (b.format === "round_robin") return { kind: TournamentState.RoundRobin };
  return { kind: TournamentState.SingleElimination };
}

function reconstruct(b: BracketState): Tournament<number> {
  const entrants = b.players.map((p) => ({ id: p.id, seed: p.seed }));
  const t = Tournament.new<number>(formatOf(b), entrants);
  for (const r of b.results) t.record(r.matchId, parseOutcome(r.outcome));
  return t;
}

function parseOutcome(o: string): Outcome {
  if (o === "B") return Outcome.B;
  if (o === "Draw") return Outcome.Draw;
  if (o === "Void") return Outcome.Void;
  return Outcome.A;
}

/**
 * POST /api/tournament/[id]/report — a player reports their match result.
 * Body: { wallet, matchId, outcome: "A"|"B"|"Draw" }.
 *
 * Reconciliation: both sides must report the same outcome for the result to
 * commit. Agreement → appended to `results` (engine advances). Disagreement →
 * the match is flagged in `disputes` for the organizer to resolve (D4).
 */
export async function POST(
  request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const tid = Number(id);
  if (!Number.isFinite(tid)) return NextResponse.json({ error: "bad id" }, { status: 400 });

  const body = (await request.json().catch(() => ({}))) as {
    wallet?: string;
    matchId?: number;
    outcome?: "A" | "B" | "Draw";
  };
  if (!body.wallet || body.matchId == null || !body.outcome) {
    return NextResponse.json({ error: "wallet, matchId, outcome required" }, { status: 400 });
  }

  const store = getStore();
  const bracket = await store.getBracket(tid);
  if (!bracket) return NextResponse.json({ error: "bracket not found" }, { status: 404 });
  if (bracket.finalized) return NextResponse.json({ error: "tournament finalized" }, { status: 400 });

  const wallet = body.wallet.toLowerCase();
  const player = bracket.players.find((p) => p.wallet.toLowerCase() === wallet);
  if (!player) return NextResponse.json({ error: "not a player in this tournament" }, { status: 403 });

  const engine = reconstruct(bracket);
  const match = engine.matches().find((m) => m.id === body.matchId);
  if (!match) return NextResponse.json({ error: "match not found" }, { status: 404 });
  if (match.outcome !== null) return NextResponse.json({ error: "match already decided" }, { status: 400 });

  let side: "A" | "B" | null = null;
  if (match.a?.id === player.id) side = "A";
  else if (match.b?.id === player.id) side = "B";
  if (!side) return NextResponse.json({ error: "you are not in this match" }, { status: 403 });
  const mySide = side;

  // Record/replace this side's report.
  const reports = (bracket.reports ?? []).filter(
    (r) => !(r.matchId === body.matchId && r.side === mySide)
  );
  reports.push({ matchId: body.matchId, side, wallet, outcome: body.outcome });

  // Is the other side's report in?
  const otherSide: "A" | "B" = mySide === "A" ? "B" : "A";
  const other = reports.find((r) => r.matchId === body.matchId && r.side === otherSide);

  let status: "waiting" | "confirmed" | "disputed" = "waiting";
  const disputes = bracket.disputes ?? [];

  if (other) {
    if (other.outcome === body.outcome) {
      // Agreement → commit the result.
      bracket.results = [...bracket.results, { matchId: body.matchId, outcome: body.outcome }];
      status = "confirmed";
    } else {
      // Disagreement → dispute.
      if (!disputes.includes(body.matchId)) disputes.push(body.matchId);
      status = "disputed";
    }
  }

  bracket.reports = reports;
  bracket.disputes = disputes;
  await store.saveBracket(tid, bracket);

  return NextResponse.json({ ok: true, status });
}
