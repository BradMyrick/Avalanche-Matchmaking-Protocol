import { NextResponse } from "next/server";
import { getStore, type BracketState } from "@/lib/store";

export const runtime = "nodejs";

/** PUT /api/tournament/[id]/bracket — persist bracket state after each result. */
export async function PUT(
  request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const tid = Number(id);
  if (!Number.isFinite(tid)) {
    return NextResponse.json({ error: "bad id" }, { status: 400 });
  }
  const body = (await request.json().catch(() => ({}))) as Partial<BracketState>;
  if (!body.format || !Array.isArray(body.players) || !Array.isArray(body.results)) {
    return NextResponse.json({ error: "invalid bracket state" }, { status: 400 });
  }
  await getStore().saveBracket(tid, body as BracketState);
  return NextResponse.json({ ok: true });
}
