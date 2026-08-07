import type { Pool } from "pg";

/**
 * Tournament + bracket + relayer-job persistence. Postgres only — the web app
 * never holds the funded key; it enqueues jobs in `relayer_jobs` and the
 * isolated Rust relayer (`/relayer`) drains them. Requires DATABASE_URL.
 */

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
  payload: Record<string, unknown>;
}

class PostgresStore {
  private poolPromise: Promise<Pool> | null = null;

  private pool(): Promise<Pool> {
    if (!process.env.DATABASE_URL) {
      throw new Error("DATABASE_URL not set — Postgres is required (no in-memory fallback).");
    }
    if (!this.poolPromise) {
      this.poolPromise = import("pg").then(({ default: pg }) => {
        return new pg.Pool({ connectionString: process.env.DATABASE_URL });
      });
    }
    return this.poolPromise;
  }

  async saveTournament(record: TournamentRecord): Promise<void> {
    const pool = await this.pool();
    await pool.query(
      `INSERT INTO tournaments
         (tournament_id, sponsor, prize_pool_wei, token, payout_bps, winner_wallets, state, mode, paypal_order_id, tx_hash, created_at)
       VALUES ($1,$2,$3,$4,$5,$6,$7,$8,$9,$10,to_timestamp($11))
       ON CONFLICT (tournament_id) DO UPDATE SET
         winner_wallets = EXCLUDED.winner_wallets,
         state = EXCLUDED.state,
         mode = EXCLUDED.mode,
         tx_hash = EXCLUDED.tx_hash`,
      [
        record.tournamentId,
        record.sponsor,
        record.prizePoolWei,
        record.token,
        JSON.stringify(record.payoutBps),
        JSON.stringify(record.winnerWallets),
        record.state,
        record.mode ?? null,
        record.paypalOrderId ?? null,
        record.txHash ?? null,
        record.createdAt / 1000,
      ]
    );
  }

  async getTournament(id: number): Promise<TournamentRecord | null> {
    const pool = await this.pool();
    const res = await pool.query("SELECT * FROM tournaments WHERE tournament_id = $1", [id]);
    return res.rows.length ? rowToRecord(res.rows[0]) : null;
  }

  async listTournaments(limit = 20): Promise<TournamentRecord[]> {
    const pool = await this.pool();
    const res = await pool.query("SELECT * FROM tournaments ORDER BY tournament_id DESC LIMIT $1", [limit]);
    return res.rows.map(rowToRecord);
  }

  async saveBracket(tournamentId: number, state: BracketState): Promise<void> {
    const pool = await this.pool();
    await pool.query(
      `INSERT INTO brackets (tournament_id, state, updated_at)
       VALUES ($1, $2, now())
       ON CONFLICT (tournament_id) DO UPDATE SET state = EXCLUDED.state, updated_at = now()`,
      [tournamentId, JSON.stringify(state)]
    );
  }

  async getBracket(tournamentId: number): Promise<BracketState | null> {
    const pool = await this.pool();
    const res = await pool.query("SELECT state FROM brackets WHERE tournament_id = $1", [tournamentId]);
    if (!res.rows.length) return null;
    return typeof res.rows[0].state === "string"
      ? (JSON.parse(res.rows[0].state) as BracketState)
      : (res.rows[0].state as BracketState);
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
      `SELECT id, kind, status, tournament_id, tx_hash, error, payload::text as payload FROM relayer_jobs WHERE id = $1`,
      [id]
    );
    if (!res.rows.length) return null;
    const r = res.rows[0];
    return {
      id: r.id,
      kind: r.kind,
      status: r.status,
      tournamentId: r.tournament_id ?? null,
      txHash: r.tx_hash ?? null,
      error: r.error ?? null,
      payload: JSON.parse(r.payload ?? "{}"),
    };
  }
}

function rowToRecord(r: Record<string, unknown>): TournamentRecord {
  return {
    tournamentId: Number(r.tournament_id),
    sponsor: String(r.sponsor),
    prizePoolWei: String(r.prize_pool_wei),
    token: String(r.token),
    payoutBps: Array.isArray(r.payout_bps) ? (r.payout_bps as number[]) : JSON.parse(String(r.payout_bps)),
    winnerWallets: Array.isArray(r.winner_wallets) ? (r.winner_wallets as string[]) : JSON.parse(String(r.winner_wallets)),
    state: String(r.state) as TournamentState,
    mode: (r.mode as "instant" | "bracket") ?? undefined,
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
