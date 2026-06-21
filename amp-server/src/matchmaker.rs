//! amp-server's matchmaker is a thin re-export over the embeddable
//! `amp_match_core` library. The Glicko-2 rating math, rule evaluation,
//! and quality scoring live there; this module preserves the historical
//! `compute_match_quality` API for existing call sites.

pub use amp_match_core::glicko2_update;

use crate::state::{MatchQualityDetail, QueueEntry, StoredRuleSet};
use std::sync::Arc;

/// Compute the quality breakdown for a candidate pair. Delegates to
/// `amp_match_core::evaluate_rules` (operating on the inner `PlayerTicket`
/// via `AsRef`) and returns just the quality score.
#[allow(dead_code)] // historical API kept for call-site stability
pub fn compute_match_quality(
    player_a: &QueueEntry,
    player_b: &QueueEntry,
    ruleset: &Arc<StoredRuleSet>,
) -> MatchQualityDetail {
    amp_match_core::evaluate_rules(player_a.as_ref(), player_b.as_ref(), ruleset).quality
}

#[cfg(test)]
mod tests {
    use super::*;
    use tokio::sync::oneshot;

    fn make_queue_entry(mmr: f32, role: &str) -> QueueEntry {
        let (tx, _) = oneshot::channel();
        QueueEntry {
            ticket: amp_match_core::PlayerTicket {
                player_id: "p1".into(),
                game_id: "g1".into(),
                ruleset_id: "r1".into(),
                mmr,
                mmr_uncertainty: 200.0,
                region: "na".into(),
                preferred_role: role.into(),
                language: "en".into(),
                max_ping_ms: 150,
                enqueued_at_ms: crate::state::now_ms(),
            },
            sender: tx,
        }
    }

    #[test]
    fn test_glicko2_win_increases_rating() {
        let (new_r, _, _) = glicko2_update(1500.0, 200.0, 0.06, 1400.0, 30.0, 1.0);
        assert!(new_r > 1500.0, "Win should increase rating, got {}", new_r);
    }

    #[test]
    fn test_glicko2_loss_decreases_rating() {
        let (new_r, _, _) = glicko2_update(1500.0, 200.0, 0.06, 1600.0, 30.0, 0.0);
        assert!(new_r < 1500.0, "Loss should decrease rating, got {}", new_r);
    }

    #[test]
    fn test_glicko2_draw_near_stable() {
        let (new_r, _, _) = glicko2_update(1500.0, 200.0, 0.06, 1500.0, 30.0, 0.5);
        assert!(
            (new_r - 1500.0).abs() < 10.0,
            "Draw should be near stable, got {}",
            new_r
        );
    }

    #[test]
    fn test_match_quality_perfect() {
        let a = make_queue_entry(1500.0, "tank");
        let b = make_queue_entry(1500.0, "dps");
        let ruleset = Arc::new(StoredRuleSet::default());
        let q = compute_match_quality(&a, &b, &ruleset);
        assert!(
            q.total_score > 0.5,
            "Perfect match should score well, got {}",
            q.total_score
        );
    }

    #[test]
    fn test_glicko2_nan_input_returns_original() {
        let (r, _, _) = glicko2_update(f32::NAN, 200.0, 0.06, 1400.0, 30.0, 1.0);
        assert!(r.is_nan(), "NaN rating input echoes back unchanged");
    }

    #[test]
    fn test_glicko2_inf_input_returns_original() {
        let (r, _rd, _vol) = glicko2_update(1500.0, f32::INFINITY, 0.06, 1400.0, 30.0, 1.0);
        assert!(
            r == 1500.0,
            "Inf rd must fall back to the original rating, got {}",
            r
        );
    }

    #[test]
    fn test_glicko2_non_positive_volatility_returns_original() {
        let (r, _rd, _vol) = glicko2_update(1500.0, 200.0, 0.0, 1400.0, 30.0, 1.0);
        assert_eq!(r, 1500.0, "zero volatility must be rejected");
    }

    #[test]
    fn test_glicko2_extreme_rating_gap_no_corruption() {
        let (r, rd, vol) = glicko2_update(1500.0, 200.0, 0.06, 10000.0, 30.0, 1.0);
        assert!(r.is_finite(), "rating must be finite, got {}", r);
        assert!(rd.is_finite(), "rd must be finite, got {}", rd);
        assert!(vol.is_finite(), "volatility must be finite, got {}", vol);
    }

    #[test]
    fn test_glicko2_normal_inputs_still_update() {
        let (r, _rd, _vol) = glicko2_update(1500.0, 200.0, 0.06, 1400.0, 30.0, 1.0);
        assert!(r > 1500.0, "win should still increase rating, got {}", r);
    }
}
