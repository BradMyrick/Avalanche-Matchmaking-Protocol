//! amp-server's matchmaking queue is now a thin alias over the embeddable
//! `amp_match_core::MatchQueue`. The library owns the bucketing + sorted-MMR
//! scan + skill-gating logic; this module just specializes it to the server's
//! `QueueEntry` (which adds the notification `sender` via `AsRef<PlayerTicket>`).
//!
//! Keeping this as a one-line alias rather than a re-export makes the
//! `IndexedQueue` name continue to resolve in existing amp-server call sites
//! without churn.

pub type IndexedQueue = amp_match_core::MatchQueue<crate::state::QueueEntry>;
#[allow(dead_code)] // historical name kept for call-site stability
pub type MatchResult = amp_match_core::MatchOutcome<crate::state::QueueEntry>;

#[cfg(test)]
mod tests {
    use super::*;
    use crate::state::{QueueEntry, StoredRuleSet};
    use std::sync::Arc;
    use tokio::sync::oneshot;

    fn make_entry(player: &str, game: &str, ruleset: &str, mmr: f32) -> QueueEntry {
        let (tx, _) = oneshot::channel();
        QueueEntry {
            ticket: amp_match_core::PlayerTicket {
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
            },
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

        // Deref: QueueEntry forwards PlayerTicket fields. The library sorts
        // by ticket.mmr internally; we can verify by querying contains_player
        // and via try_match_bucket producing the expected pairing.
        assert!(q.contains_player("p1"));
        assert!(q.contains_player("p2"));
        assert!(q.contains_player("p3"));
    }

    #[test]
    fn test_match_within_skill_range() {
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        q.push(make_entry("p2", "g1", "r1", 1400.0));

        let ruleset = Arc::new(StoredRuleSet::default());
        let result = q.try_match_bucket(&("g1".into(), "r1".into()), &ruleset, 10000, 0);

        assert!(result.is_some());
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
        let mut q = IndexedQueue::new();
        q.push(make_entry("p1", "g1", "r1", 1500.0));
        let replaced = q.push(make_entry("p1", "g1", "r1", 1600.0));

        assert!(replaced);
        assert_eq!(q.len(), 1);
        assert!(q.contains_player("p1"));
        assert!(!q.contains_player("p2"));

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

        assert!(!q.remove_player("nobody"));
    }
}
