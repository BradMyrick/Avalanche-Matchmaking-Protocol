-- Challenge creation timestamps (deterministic trim ordering going forward).
ALTER TABLE amp_auth_challenges ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT now();
