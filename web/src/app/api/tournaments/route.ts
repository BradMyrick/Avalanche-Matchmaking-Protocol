import { NextResponse } from "next/server";
import { getStore } from "@/lib/store";
import { ethers } from "ethers";

export const runtime = "nodejs";

/**
 * GET /api/tournaments?wallet=0x...
 * Returns tournaments where the wallet is the organizer or sponsor.
 */
export async function GET(request: Request) {
  const url = new URL(request.url);
  const wallet = (url.searchParams.get("wallet") ?? "").toLowerCase();
  if (!wallet || !ethers.isAddress(wallet)) {
    return NextResponse.json({ error: "valid wallet required" }, { status: 400 });
  }

  const store = getStore();
  const all = await store.listTournaments(200);
  const mine = all.filter(
    (t: { organizerWallet?: string; sponsor?: string }) =>
      t.organizerWallet?.toLowerCase() === wallet ||
      t.sponsor?.toLowerCase() === wallet
  );
  return NextResponse.json({ tournaments: mine });
}
