-- Retention sweep needs a timestamp on ladder reports.
ALTER TABLE amp_ladder_reports ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT now();
