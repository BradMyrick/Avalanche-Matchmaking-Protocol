import { NextResponse } from "next/server";
import { getStore } from "@/lib/store";
import { generateManageToken } from "@/lib/auth";
import { ethers } from "ethers";

export const runtime = "nodejs";

/**
 * POST /api/tournament/[id]/auth
 * Wallet-based authentication: proves wallet ownership via EIP-191 signature,
 * returns a manage token for that tournament (if the wallet is the organizer).
 *
 * Body: { wallet, sig, ts }
 * Message: `AMP-manage:${tournamentId}:${ts}`
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
    sig?: string;
    ts?: number;
  };
  if (!body.wallet || !body.sig || !body.ts) {
    return NextResponse.json({ error: "wallet, sig, ts required" }, { status: 400 });
  }

  const now = Math.floor(Date.now() / 1000);
  if (Math.abs(now - body.ts) > 300) {
    return NextResponse.json({ error: "timestamp expired (5 min window)" }, { status: 400 });
  }

  // Verify the EIP-191 signature.
  const message = `AMP-manage:${tid}:${body.ts}`;
  let recovered: string;
  try {
    recovered = ethers.verifyMessage(message, body.sig);
  } catch {
    return NextResponse.json({ error: "invalid signature" }, { status: 401 });
  }
  if (recovered.toLowerCase() !== body.wallet.toLowerCase()) {
    return NextResponse.json({ error: "signature mismatch" }, { status: 401 });
  }

  // Check the wallet owns this tournament.
  const store = getStore();
  const rec = await store.getTournament(tid);
  if (!rec) return NextResponse.json({ error: "tournament not found" }, { status: 404 });

  const isOrganizer =
    rec.organizerWallet?.toLowerCase() === body.wallet.toLowerCase() ||
    rec.sponsor?.toLowerCase() === body.wallet.toLowerCase();
  if (!isOrganizer) {
    return NextResponse.json({ error: "not the organizer" }, { status: 403 });
  }

  // Issue (or reuse) a manage token.
  let token = rec.manageToken;
  if (!token) {
    token = generateManageToken();
    await store.saveTournament({ ...rec, manageToken: token });
  }
  return NextResponse.json({ manageToken: token });
}
