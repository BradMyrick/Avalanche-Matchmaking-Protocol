import type { Pool } from "pg";

export type TournamentState = "OPEN" | "FINALIZED" | "COMPLETE" | "CANCELLED";

export interface TournamentRecord {
  tournamentId: number;
  sponsor: string;
  prizePoolWei: string;
  token: string;
  payoutBps: number[];
  winnerWallets: string[];
  state: TournamentState;
  mode?: "instant" | "bracket";
  manageToken?: string;
  organizerWallet?: string;
  paypalOrderId?: string;
  txHash?: string | null;
  createdAt: number;
}

export interface BracketState {
  format: "single_elimination" | "round_robin" | "swiss";
  swissRounds?: number;
  players: { id: number; wallet: string; name: string; seed: number }[];
  results: { matchId: number; outcome: "A" | "B" | "Draw" | "Void" }[];
  finalized?: boolean;
  computedWinners?: string[];
  reports?: { matchId: number; side: "A" | "B"; wallet: string; outcome: "A" | "B" | "Draw" }[];
  disputes?: number[];
}

export interface RelayerJob {
  id: number;
  kind: "fund" | "finalize";
  status: "pending" | "done" | "failed";
  tournamentId?: number | null;
  txHash?: string | null;
  error?: string | null;
}

const MAX_BODY = 64 * 1024;
const MAX_ARRAY = 1024;

export function validateBodySize(req: Request): Response | null {
  const cl = Number(req.headers.get("content-length") ?? 0);
  if (cl > MAX_BODY) {
    return Response.json({ error: "payload too large" }, { status: 413 });
  }
  return null;
}

export function capArray<T>(arr: T[] | undefined, max: number, name: string): T[] {
  if (!arr) return [];
  if (arr.length > max) throw new Error(`${name} exceeds ${max} entries`);
  return arr;
}

class PostgresStore {
  private poolPromise: Promise<Pool> | null = null;

  private pool(): Promise<Pool> {
    if (!process.env.DATABASE_URL) {
      throw new Error("DATABASE_URL not set — Postgres is required.");
    }
    if (!this.poolPromise) {
      this.poolPromise = import("pg").then(({ default: pg }) => {
        return new pg.Pool({ connectionString: process.env.DATABASE_URL, max: 3 });
      });
    }
    return this.poolPromise;
  }

  async saveTournament(record: TournamentRecord): Promise<void> {
    const pool = await this.pool();
    await pool.query(
      `INSERT INTO tournaments
         (tournament_id, sponsor, prize_pool_wei, token, payout_bps, winner_wallets, state, mode, manage_token, organizer_wallet, paypal_order_id, tx_hash, created_at)
       VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,$11,$12,to_timestamp($13))
       ON CONFLICT (tournament_id) DO UPDATE SET
         winner_wallets = EXCLUDED.winner_wallets,
         state = EXCLUDED.state, mode = EXCLUDED.mode,
         manage_token = COALESCE(EXCLUDED.manage_token, tournaments.manage_token),
         organizer_wallet = COALESCE(EXCLUDED.organizer_wallet, tournaments.organizer_wallet),
         tx_hash = EXCLUDED.tx_hash`,
      [
        record.tournamentId, record.sponsor, record.prizePoolWei, record.token,
        JSON.stringify(record.payoutBps), JSON.stringify(record.winnerWallets),
        record.state, record.mode ?? null, record.manageToken ?? null,
        record.organizerWallet ?? null, record.paypalOrderId ?? null,
        record.txHash ?? null, record.createdAt / 1000,
      ]
    );
  }

  async getTournament(id: number): Promise<TournamentRecord | null> {
    const pool = await this.pool();
    const res = await pool.query("SELECT * FROM tournaments WHERE tournament_id = $1", [id]);
    return res.rows.length ? rowToRecord(res.rows[0]) : null;
  }

  async listTournaments(limit = 50): Promise<TournamentRecord[]> {
    const pool = await this.pool();
    const res = await pool.query("SELECT * FROM tournaments ORDER BY tournament_id DESC LIMIT $1", [limit]);
    return res.rows.map(rowToRecord);
  }

  async saveBracket(tid: number, state: BracketState): Promise<void> {
    const pool = await this.pool();
    await pool.query(
      `INSERT INTO brackets (tournament_id, state, updated_at) VALUES ($1, $2, now())
       ON CONFLICT (tournament_id) DO UPDATE SET state = EXCLUDED.state, updated_at = now()`,
      [tid, JSON.stringify(state)]
    );
  }

  async getBracket(tid: number): Promise<BracketState | null> {
    const pool = await this.pool();
    const res = await pool.query("SELECT state FROM brackets WHERE tournament_id = $1", [tid]);
    if (!res.rows.length) return null;
    return typeof res.rows[0].state === "string"
      ? JSON.parse(res.rows[0].state) : res.rows[0].state;
  }

  async enqueueJob(kind: "fund" | "finalize", payload: Record<string, unknown>): Promise<number> {
    const pool = await this.pool();
    const res = await pool.query(
      `INSERT INTO relayer_jobs (kind, payload, status) VALUES ($1, $2, 'pending') RETURNING id`,
      [kind, JSON.stringify(payload)]
    );
    return res.rows[0].id;
  }

  async getJob(id: number): Promise<RelayerJob | null> {
    const pool = await this.pool();
    const res = await pool.query(
      `SELECT id, kind, status, tournament_id, tx_hash, error FROM relayer_jobs WHERE id = $1`, [id]
    );
    if (!res.rows.length) return null;
    const r = res.rows[0];
    return { id: r.id, kind: r.kind, status: r.status, tournamentId: r.tournament_id ?? null, txHash: r.tx_hash ?? null, error: r.error ?? null };
  }

  async useNonce(wallet: string, tid: number, matchId: number, nonce: string): Promise<boolean> {
    const pool = await this.pool();
    const res = await pool.query(
      `INSERT INTO report_nonces (wallet, tournament_id, match_id, nonce) VALUES ($1,$2,$3,$4) ON CONFLICT DO NOTHING RETURNING 1`,
      [wallet, tid, matchId, nonce]
    );
    return res.rows.length > 0;
  }

  async claimFundingIntent(orderId: string, amountUsd: number): Promise<{ first: boolean; jobId?: number; tournamentId?: number }> {
    const pool = await this.pool();
    const ins = await pool.query(
      `INSERT INTO funding_intents (paypal_order_id, amount_usd) VALUES ($1,$2) ON CONFLICT DO NOTHING RETURNING 1`,
      [orderId, amountUsd]
    );
    if (ins.rows.length > 0) return { first: true };
    const existing = await pool.query(`SELECT job_id, tournament_id FROM funding_intents WHERE paypal_order_id = $1`, [orderId]);
    return { first: false, jobId: existing.rows[0]?.job_id, tournamentId: existing.rows[0]?.tournament_id };
  }

  async updateFundingIntent(orderId: string, jobId: number, tournamentId: number | null): Promise<void> {
    const pool = await this.pool();
    await pool.query(`UPDATE funding_intents SET job_id=$2, tournament_id=$3 WHERE paypal_order_id=$1`, [orderId, jobId, tournamentId]);
  }
}

function rowToRecord(r: Record<string, unknown>): TournamentRecord {
  return {
    tournamentId: Number(r.tournament_id),
    sponsor: String(r.sponsor),
    prizePoolWei: String(r.prize_pool_wei),
    token: String(r.token),
    payoutBps: Array.isArray(r.payout_bps) ? r.payout_bps as number[] : JSON.parse(String(r.payout_bps)),
    winnerWallets: Array.isArray(r.winner_wallets) ? r.winner_wallets as string[] : JSON.parse(String(r.winner_wallets)),
    state: String(r.state) as TournamentState,
    mode: (r.mode as "instant" | "bracket") ?? undefined,
    manageToken: (r.manage_token as string) ?? undefined,
    organizerWallet: (r.organizer_wallet as string) ?? undefined,
    paypalOrderId: (r.paypal_order_id as string) ?? undefined,
    txHash: (r.tx_hash as string) ?? null,
    createdAt: Math.floor(new Date(r.created_at as string).getTime()),
  };
}

let cached: PostgresStore | null = null;
export function getStore(): PostgresStore {
  if (!cached) cached = new PostgresStore();
  return cached;
}
