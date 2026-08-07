import { NextResponse } from "next/server";
import { verifyOrder } from "@/lib/paypal";
import { getStore, type BracketState, validateBodySize } from "@/lib/store";
import { generateManageToken } from "@/lib/auth";

export const runtime = "nodejs";

export async function POST(request: Request) {
  if (validateBodySize(request)) return validateBodySize(request)!;

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
  if (t.payoutBps.length > 16) return NextResponse.json({ error: "too many placements" }, { status: 400 });

  const mode = t.mode ?? "instant";
  if (mode === "instant" && (!t.winnerWallets || t.winnerWallets.length !== t.payoutBps.length)) {
    return NextResponse.json({ error: "winners count != placements" }, { status: 400 });
  }
  if (mode === "bracket" && (!t.format || !Array.isArray(t.players))) {
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
    return NextResponse.json({ error: "order verification failed" }, { status: 502 });
  }

  // P0-3: idempotency — one PayPal order = one fund job.
  const store = getStore();
  const intent = await store.claimFundingIntent(body.orderID, amountUsd);
  if (!intent.first) {
    // Already processed — return the existing result (no second funding).
    return NextResponse.json({ ok: true, jobId: intent.jobId, tournamentId: intent.tournamentId, replay: true });
  }

  // Generate a manage token for bracket-mode tournaments.
  const manageToken = mode === "bracket" ? generateManageToken() : undefined;

  const jobId = await store.enqueueJob("fund", {
    payoutBps: t.payoutBps,
    winnerWallets: mode === "instant" ? t.winnerWallets : [],
    fundedAvax: amountUsd,
    mode,
    finalizeDays: t.finalizeDays ?? 7,
    format: t.format,
    swissRounds: t.swissRounds,
    players: t.players,
    paypalOrderId: body.orderID,
    manageToken,
  });

  await store.updateFundingIntent(body.orderID, jobId, null);
  return NextResponse.json({ ok: true, jobId, manageToken, pending: true, amountUsd });
}
