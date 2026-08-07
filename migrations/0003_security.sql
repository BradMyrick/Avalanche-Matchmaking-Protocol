-- P0 security: organizer authorization, player report nonces, PayPal idempotency.

ALTER TABLE tournaments
  ADD COLUMN IF NOT EXISTS organizer_wallet TEXT,
  ADD COLUMN IF NOT EXISTS manage_token     TEXT,
  ADD COLUMN IF NOT EXISTS mode             TEXT;

CREATE TABLE IF NOT EXISTS report_nonces (
  wallet        TEXT      NOT NULL,
  tournament_id BIGINT    NOT NULL,
  match_id      BIGINT    NOT NULL,
  nonce         TEXT      NOT NULL,
  used_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (wallet, tournament_id, match_id, nonce)
);

CREATE TABLE IF NOT EXISTS funding_intents (
  paypal_order_id TEXT PRIMARY KEY,
  job_id          BIGINT,
  tournament_id   BIGINT,
  amount_usd      NUMERIC(12,2) NOT NULL,
  created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

ALTER TABLE relayer_jobs ADD COLUMN IF NOT EXISTS paypal_order_id TEXT;
