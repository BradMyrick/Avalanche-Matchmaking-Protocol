-- Relayer job queue — the web enqueues funding/finalize jobs; the isolated
-- Rust relayer dequeues, signs with the funded key, submits on-chain, and
-- writes back the result. The web app never touches the funded key.
CREATE TABLE IF NOT EXISTS relayer_jobs (
    id            BIGSERIAL PRIMARY KEY,
    kind          TEXT      NOT NULL,         -- 'fund' | 'finalize'
    tournament_id BIGINT,                    -- filled by the relayer after funding
    payload       JSONB     NOT NULL,         -- {payoutBps, winnerWallets, fundedAvax, mode, finalizeDays}
    status        TEXT      NOT NULL DEFAULT 'pending',  -- pending | done | failed
    tx_hash       TEXT,
    error         TEXT,
    created_at    TIMESTAMPTZ NOT NULL DEFAULT now(),
    completed_at  TIMESTAMPTZ
);
CREATE INDEX IF NOT EXISTS relayer_jobs_status_idx ON relayer_jobs (status, created_at);
