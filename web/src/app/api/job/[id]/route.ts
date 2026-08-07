import { NextResponse } from "next/server";
import { getStore } from "@/lib/store";

export const runtime = "nodejs";

/** GET /api/job/[id] — relayer job status. Read-only, no payload data exposed. */
export async function GET(
  _request: Request,
  { params }: { params: Promise<{ id: string }> }
) {
  const { id } = await params;
  const jobId = Number(id);
  if (!Number.isFinite(jobId)) return NextResponse.json({ error: "bad id" }, { status: 400 });

  const job = await getStore().getJob(jobId);
  if (!job) return NextResponse.json({ error: "not found" }, { status: 404 });

  return NextResponse.json(job);
}
