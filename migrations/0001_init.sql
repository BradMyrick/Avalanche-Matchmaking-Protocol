-- AMP tournament persistence schema.
-- Run on Vercel Postgres / Neon once DATABASE_URL is configured.

CREATE TABLE IF NOT EXISTS tournaments (
    tournament_id    BIGINT PRIMARY KEY,
    sponsor          TEXT    NOT NULL,
    prize_pool_wei   TEXT    NOT NULL,        -- wei as decimal string
    token            TEXT    NOT NULL,        -- address(0) == native AVAX
    payout_bps       JSONB   NOT NULL,        -- e.g. [7000, 3000]
    winner_wallets   JSONB   NOT NULL,        -- ordered: index 0 == 1st place
    state            TEXT    NOT NULL,        -- OPEN | FINALIZED | COMPLETE | CANCELLED
    paypal_order_id  TEXT,
    tx_hash          TEXT,
    created_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS tournaments_state_idx     ON tournaments (state);
CREATE INDEX IF NOT EXISTS tournaments_sponsor_idx   ON tournaments (sponsor);
CREATE INDEX IF NOT EXISTS tournaments_created_idx   ON tournaments (created_at DESC);

-- Off-chain bracket state (Phase D). One row per tournament; the engine is
-- deterministic so format + players + recorded results reconstruct the bracket.
CREATE TABLE IF NOT EXISTS brackets (
    tournament_id BIGINT PRIMARY KEY REFERENCES tournaments(tournament_id) ON DELETE CASCADE,
    state         JSONB   NOT NULL,
    updated_at    TIMESTAMPTZ NOT NULL DEFAULT now()
);
