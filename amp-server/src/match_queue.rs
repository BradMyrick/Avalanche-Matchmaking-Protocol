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

    pub fn push(&mut self, entry: QueueEntry) {
        let key = (entry.game_id.clone(), entry.ruleset_id.clone());
        let bucket = self.buckets.entry(key).or_default();
        let mmr = entry.mmr;
        let pos = bucket
            .iter()
            .position(|e| e.mmr > mmr)
            .unwrap_or(bucket.len());
        bucket.insert(pos, entry);
        self.total_count += 1;
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

            let lo = mmr_search_bound(entry_a.mmr, max_diff, false);
            let hi = mmr_search_bound(entry_a.mmr, max_diff, true);

            let search_start = match bucket[i + 1..]
                .binary_search_by(|e| e.mmr.partial_cmp(&lo).unwrap_or(std::cmp::Ordering::Equal))
            {
                Ok(idx) => (i + 1) + idx,
                Err(idx) => (i + 1) + idx,
            };

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
}
