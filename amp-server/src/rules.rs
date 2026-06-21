//! amp-server's rule evaluation is now a thin re-export over the embeddable
//! `amp_match_core::rules` module. The library owns the hard/soft constraint
//! evaluation logic, skill-decay widening, and backfill path; this module
//! preserves the historical `rules::evaluate_rules` path so existing call
//! sites keep compiling.
//!
//! Note: `amp_match_core::rules::evaluate_rules` operates on `&PlayerTicket`
//! directly. amp-server callers that hold a `QueueEntry` should use the
//! `QueueEntry::as_ref()` adapter (the wrapper implements `AsRef<PlayerTicket>`).

#[allow(unused_imports)] // re-exported for downstream callers / tests
pub use amp_match_core::rules::{RuleEvaluationResult, evaluate_rules};

#[cfg(test)]
mod tests {
    use super::*;
    use crate::state::{QueueEntry, StoredRule, StoredRuleSet};
    use std::sync::Arc;
    use tokio::sync::oneshot;

    fn make_entry(mmr: f32, region: &str, role: &str, language: &str) -> QueueEntry {
        let (tx, _) = oneshot::channel();
        QueueEntry {
            ticket: amp_match_core::PlayerTicket {
                player_id: "p".into(),
                game_id: "g".into(),
                ruleset_id: "r".into(),
                mmr,
                mmr_uncertainty: 200.0,
                region: region.into(),
                preferred_role: role.into(),
                language: language.into(),
                max_ping_ms: 150,
                enqueued_at_ms: crate::state::now_ms(),
            },
            sender: tx,
        }
    }

    #[test]
    fn test_skill_rule_passes_within_range() {
        let a = make_entry(1500.0, "na", "tank", "en");
        let b = make_entry(1600.0, "na", "dps", "en");
        let rs = Arc::new(StoredRuleSet::default());
        let r = evaluate_rules(a.as_ref(), b.as_ref(), &rs);
        assert!(r.passes, "skill diff in range should pass");
        assert!(r.quality.skill_balance > 0.0);
    }

    #[test]
    fn test_skill_rule_fails_outside_range() {
        let a = make_entry(1500.0, "na", "tank", "en");
        let b = make_entry(5000.0, "na", "dps", "en");
        let rs = Arc::new(StoredRuleSet::default());
        let r = evaluate_rules(a.as_ref(), b.as_ref(), &rs);
        assert!(!r.passes, "skill diff out of range should fail");
    }

    #[test]
    fn test_region_rule_same_region_passes() {
        let a = make_entry(1500.0, "na", "tank", "en");
        let b = make_entry(1500.0, "na", "dps", "en");
        let rs = Arc::new(StoredRuleSet::default());
        let r = evaluate_rules(a.as_ref(), b.as_ref(), &rs);
        // Default ruleset only has the skill rule; same-region players still
        // pass the skill gate (region_score remains 0 unless a region rule is
        // added to the ruleset).
        assert!(r.passes, "same region should pass the default ruleset");
    }

    #[test]
    fn test_default_skill_rule_is_added() {
        let rs = StoredRuleSet::default();
        assert!(
            !rs.rules.is_empty(),
            "default ruleset should have at least one rule"
        );
        assert_eq!(rs.rules[0].rule_id, StoredRule::default_skill().rule_id);
    }
}
