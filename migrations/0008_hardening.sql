-- v0.3 hardening migrations.

-- M4: a wallet may hold at most ONE active (queued) ticket. Closes the
-- check-then-insert race in queue_join that could double-queue a player
-- across concurrent requests / restarts.
CREATE UNIQUE INDEX IF NOT EXISTS amp_queue_tickets_one_active
    ON amp_queue_tickets (wallet)
    WHERE status = 'queued';

-- Leaderboard support: rank-order scans of ratings per game/ruleset.
CREATE INDEX IF NOT EXISTS amp_ratings_leaderboard_idx
    ON amp_ratings (game_id, ruleset_id, rating DESC);

-- M9: relayer job claiming. The claim transaction flips pending →
-- processing atomically; claimed_at drives crash-recovery requeueing.
ALTER TABLE relayer_jobs ADD COLUMN IF NOT EXISTS claimed_at TIMESTAMPTZ;
CREATE INDEX IF NOT EXISTS relayer_jobs_claim_idx
    ON relayer_jobs (status, claimed_at);
