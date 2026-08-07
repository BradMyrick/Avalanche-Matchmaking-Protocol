import { NextResponse } from "next/server";
import { verifyOrder } from "@/lib/paypal";
import { getStore, type BracketState } from "@/lib/store";

export const runtime = "nodejs";

/**
 * Capture + enqueue a custodial funding job.
 *
 * The web app verifies the PayPal order, then enqueues a `fund` job in
 * `relayer_jobs`. The isolated Rust relayer drains the queue, signs with the
 * funded key, and submits on-chain. The web never sees the key.
 *
 * Returns { ok, jobId, pending: true }. Poll /api/job/[id] for the tournamentId.
 */
export async function POST(request: Request) {
  const body = (await request.json().catch(() => ({}))) as {
    orderID?: string;
    tournament?: {
      name?: string;
      payoutBps?: number[];
      finalizeDays?: number;
      mode?: "instant" | "bracket";
      winnerWallets?: string[];
      format?: BracketState["format"];
      swissRounds?: number;
      players?: BracketState["players"];
    };
  };

  const t = body.tournament;
  if (!body.orderID || !t?.payoutBps) {
    return NextResponse.json({ error: "Missing orderID or payoutBps" }, { status: 400 });
  }
  const mode = t.mode ?? "instant";
  if (mode === "instant") {
    if (!t.winnerWallets || t.winnerWallets.length !== t.payoutBps.length) {
      return NextResponse.json({ error: "winners count != placements" }, { status: 400 });
    }
  } else if (!t.format || !Array.isArray(t.players)) {
    return NextResponse.json({ error: "bracket mode needs format + players" }, { status: 400 });
  }

  let amountUsd: number;
  try {
    const order = await verifyOrder(body.orderID);
    if (order.status !== "COMPLETED" && order.status !== "APPROVED") {
      return NextResponse.json({ error: `Order not completable (${order.status})` }, { status: 400 });
    }
    amountUsd = order.amountUsd;
  } catch (e) {
    return NextResponse.json({ error: (e as Error).message }, { status: 502 });
  }

  const jobId = await getStore().enqueueJob("fund", {
    payoutBps: t.payoutBps,
    winnerWallets: mode === "instant" ? t.winnerWallets : [],
    fundedAvax: amountUsd, // MVP demo mapping (testnet 1:1)
    mode,
    finalizeDays: t.finalizeDays ?? 7,
    format: t.format,
    swissRounds: t.swissRounds,
    players: t.players,
    paypalOrderId: body.orderID,
  });

  return NextResponse.json({ ok: true, jobId, pending: true, amountUsd });
}
