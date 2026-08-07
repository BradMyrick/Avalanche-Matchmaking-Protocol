import { NextResponse } from "next/server";
import { getStore, type BracketState, validateBodySize } from "@/lib/store";
import { requireOrganizer } from "@/lib/auth";

export const runtime = "nodejs";

/** PUT /api/tournament/[id]/bracket — persist bracket state. Organizer-only. */
export async function PUT(
  request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  if (validateBodySize(request)) return validateBodySize(request)!;
  const { id } = await params;
  const tid = Number(id);
  if (!Number.isFinite(tid)) return NextResponse.json({ error: "bad id" }, { status: 400 });

  const rec = await requireOrganizer(request, tid);
  if (!rec) return NextResponse.json({ error: "unauthorized" }, { status: 401 });

  const body = (await request.json().catch(() => ({}))) as Partial<BracketState>;
  if (!body.format || !Array.isArray(body.players) || !Array.isArray(body.results)) {
    return NextResponse.json({ error: "invalid bracket state" }, { status: 400 });
  }
  if (body.players.length > 1024 || body.results.length > 4096) {
    return NextResponse.json({ error: "too many entries" }, { status: 400 });
  }
  await getStore().saveBracket(tid, body as BracketState);
  return NextResponse.json({ ok: true });
}
