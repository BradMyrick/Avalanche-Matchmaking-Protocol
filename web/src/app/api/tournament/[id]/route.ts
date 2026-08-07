import { NextResponse } from "next/server";
import { getStore } from "@/lib/store";

export const runtime = "nodejs";

/** GET /api/tournament/[id] — return the tournament record + bracket state. */
export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const tid = Number(id);
  if (!Number.isFinite(tid)) {
    return NextResponse.json({ error: "bad id" }, { status: 400 });
  }
  const store = getStore();
  const [tournament, bracket] = await Promise.all([
    store.getTournament(tid),
    store.getBracket(tid),
  ]);
  if (!tournament) {
    return NextResponse.json({ error: "not found" }, { status: 404 });
  }
  return NextResponse.json({ tournament, bracket });
}
