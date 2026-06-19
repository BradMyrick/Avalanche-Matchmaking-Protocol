use crate::state::{MatchQualityDetail, QueueEntry, StoredRuleSet};
use std::collections::HashMap;
use std::sync::Arc;

type BucketKey = (String, String);

pub struct IndexedQueue {
    buckets: HashMap<BucketKey, Vec<QueueEntry>>,
    total_count: usize,
}

impl IndexedQueue {
    pub fn new() -> Self {
        Self {
            buckets: HashMap::new(),
            total_count: 0,
        }
    }

    pub fn len(&self) -> usize {
        self.total_count
    }

    pub fn push(&mut self, entry: QueueEntry) -> bool {
        // Enqueue uniqueness: a player may only be queued once at a time.
        // A duplicate requestMatch (or a re-queue on reconnect) replaces the
        // stale entry; dropping its oneshot::Sender unblocks the original
        // request_match future as superseded rather than leaving it dangling.
        let replaced = self.remove_player(&entry.player_id);

        let key = (entry.game_id.clone(), entry.ruleset_id.clone());
        let bucket = self.buckets.entry(key).or_default();
        let mmr = entry.mmr;
        let pos = match bucket
            .binary_search_by(|e| e.mmr.partial_cmp(&mmr).unwrap_or(std::cmp::Ordering::Equal))
        {
            Ok(p) => p,
            Err(p) => p,
        };
        bucket.insert(pos, entry);
        self.total_count += 1;
        replaced
    }

    /// Remove any queued entry for `player_id`. Returns `true` if an entry was
    /// removed. Used both for dedup-on-push and for disconnect cleanup.
    pub fn remove_player(&mut self, player_id: &str) -> bool {
        let mut removed = false;
        for bucket in self.buckets.values_mut() {
            let before = bucket.len();
            bucket.retain(|e| e.player_id != player_id);
            let gone = before - bucket.len();
            self.total_count = self.total_count.saturating_sub(gone);
            removed |= gone > 0;
        }
        removed
    }

    /// Returns true if `player_id` is currently queued.
    #[allow(dead_code)]
    pub fn contains_player(&self, player_id: &str) -> bool {
        self.buckets
            .values()
            .any(|b| b.iter().any(|e| e.player_id == player_id))
    }

    pub fn drain_all(&mut self) -> Vec<QueueEntry> {
        let mut all = Vec::with_capacity(self.total_count);
        for (_, bucket) in self.buckets.drain() {
            all.extend(bucket);
        }
        self.total_count = 0;
        all
    }

    pub fn try_match_bucket(
        &mut self,
        key: &BucketKey,
        ruleset: &Arc<StoredRuleSet>,
        max_active: usize,
        current_active: usize,
    ) -> Option<MatchResult> {
        if current_active >= max_active {
            return None;
        }

        let bucket = self.buckets.get_mut(key)?;
        if bucket.len() < 2 {
            return None;
        }

        let max_diff = ruleset.max_skill_diff;

        let mut i = 0;
        while i < bucket.len() {
            let entry_a = &bucket[i];

            let hi = mmr_search_bound(entry_a.mmr, max_diff, true);

            let search_start = i + 1;

            let mut best_j: Option<usize> = None;
            let mut best_score = 0.0f32;

            for (j, candidate) in bucket.iter().enumerate().skip(search_start) {
                if candidate.mmr > hi {
                    break;
                }
                if candidate.player_id == entry_a.player_id {
                    continue;
                }

                let result = crate::rules::evaluate_rules(entry_a, candidate, ruleset);
                if result.passes && result.quality.total_score > best_score {
                    best_score = result.quality.total_score;
                    best_j = Some(j);
                }
            }

            if let Some(j) = best_j {
                let (entry_a, entry_b) = if j > i {
                    let b = bucket.remove(j);
                    let a = bucket.remove(i);
                    (a, b)
                } else {
                    let a = bucket.remove(i);
                    let b = bucket.remove(j);
                    (a, b)
                };
                self.total_count -= 2;

                let quality = crate::matchmaker::compute_match_quality(&entry_a, &entry_b, ruleset);

                return Some(MatchResult {
                    entry_a,
                    entry_b,
                    quality,
                });
            }

            i += 1;
        }

        None
    }

    pub fn bucket_keys(&self) -> Vec<BucketKey> {
        self.buckets.keys().cloned().collect()
    }
}

fn mmr_search_bound(mmr: f32, max_diff: f32, upper: bool) -> f32 {
    if upper {
        mmr + max_diff
    } else {
        mmr - max_diff
    }
}

pub struct MatchResult {
    pub entry_a: QueueEntry,
    pub entry_b: QueueEntry,
    pub quality: MatchQualityDetail,
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::state::StoredRuleSet;
    use tokio::sync::oneshot;

    fn make_entry(player: &str, game: &str, ruleset: &str, mmr: f32) -> QueueEntry {
        let (tx, _) = oneshot::channel();
        QueueEntry {
            player_id: player.into(),
            game_id: game.into(),
            ruleset_id: ruleset.into(),
            mmr,
            mmr_uncertainty: 200.0,
            region: "na".into(),
            preferred_role: "tank".into(),
            language: "en".into(),
            max_ping_ms: 150,
            enqueued_at_ms: crate::state::now_ms(),
            sender: tx,
        }
    }

    #[test]
    fn test_bucket_partitioning() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g2", "r1", 1500.0));
        q.push(make_entry("p3", "g1", "r1", 1400.0));

        assert_eq!(q.len(), 3);
        assert_eq!(q.bucket_keys().len(), 2);
    }

    #[test]
    fn test_mmr_sorted_insert() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g1", "r1", 1200.0));
        q.push(make_entry("p3", "g1", "r1", 1800.0));

        let bucket = &q.buckets[&("g1".into(), "r1".into())];
        assert_eq!(bucket[0].player_id, "p2");
        assert_eq!(bucket[1].player_id, "p1");
        assert_eq!(bucket[2].player_id, "p3");
    }

    #[test]
    fn test_match_within_skill_range() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g1", "r1", 1400.0));

        let ruleset = Arc::new(StoredRuleSet::default());
        let result = q.try_match_bucket(&("g1".into(), "r1".into()), &ruleset, 10000, 0);

        assert!(result.is_some());
        let _m = result.unwrap();
        assert_eq!(q.len(), 0);
    }

    #[test]
    fn test_no_match_outside_skill_range() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g1", "r1", 3000.0));

        let ruleset = Arc::new(StoredRuleSet {
            max_skill_diff: 500.0,
            ..Default::default()
        });
        let result = q.try_match_bucket(&("g1".into(), "r1".into()), &ruleset, 10000, 0);

        assert!(result.is_none());
        assert_eq!(q.len(), 2);
    }

    #[test]
    fn test_no_match_at_capacity() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g1", "r1", 1400.0));

        let ruleset = Arc::new(StoredRuleSet::default());
        let result = q.try_match_bucket(&("g1".into(), "r1".into()), &ruleset, 100, 100);

        assert!(result.is_none());
    }

    #[test]
    fn test_drain_all() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g2", "r1", 1400.0));
        let all = q.drain_all();
        assert_eq!(all.len(), 2);
        assert_eq!(q.len(), 0);
    }

    #[test]
    fn test_push_dedups_existing_player() {
        // A player who re-queues must replace their stale entry rather than
        // appear twice (which would allow being matched into two matches).
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        // Same player, different mmr/ruleset — should replace, not append.
        let replaced = q.push(make_entry("p1", "g1", "r1", 1600.0));

        assert!(replaced, "push should report it replaced an existing entry");
        assert_eq!(q.len(), 1, "queue must hold exactly one entry per player");
        assert!(q.contains_player("p1"));
        assert!(!q.contains_player("p2"));

        // A second distinct player does not trigger replacement.
        let replaced2 = q.push(make_entry("p2", "g1", "r1", 1400.0));
        assert!(!replaced2);
        assert_eq!(q.len(), 2);
    }

    #[test]
    fn test_remove_player_clears_across_buckets() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g2", "r1", 1400.0));

        assert!(q.remove_player("p1"));
        assert!(!q.contains_player("p1"));
        assert_eq!(q.len(), 1);

        // Removing a non-queued player is a no-op.
        assert!(!q.remove_player("nobody"));
    }
}
