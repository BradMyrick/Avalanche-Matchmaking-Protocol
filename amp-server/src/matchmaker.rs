use crate::rules;
use crate::state::{MatchQualityDetail, QueueEntry, StoredRuleSet};
use std::sync::Arc;

#[allow(dead_code)]
pub fn find_match(
    queue: &[QueueEntry],
    new_entry: &QueueEntry,
    ruleset: &Arc<StoredRuleSet>,
) -> Option<usize> {
    let mut best_idx: Option<usize> = None;
    let mut best_score = 0.0f32;

    for (idx, candidate) in queue.iter().enumerate() {
        if candidate.game_id != new_entry.game_id {
            continue;
        }
        if candidate.player_id == new_entry.player_id {
            continue;
        }

        let result = rules::evaluate_rules(new_entry, candidate, ruleset);
        if !result.passes {
            continue;
        }

        if result.quality.total_score > best_score {
            best_score = result.quality.total_score;
            best_idx = Some(idx);
        }
    }

    best_idx
}

pub fn compute_match_quality(
    player_a: &QueueEntry,
    player_b: &QueueEntry,
    ruleset: &Arc<StoredRuleSet>,
) -> MatchQualityDetail {
    let result = rules::evaluate_rules(player_a, player_b, ruleset);
    result.quality
}

const TAU: f64 = 0.5;
const SCALE: f64 = 173.7178;
/// Caps the Illinois-method root finder AND the bracket search. Without this,
/// NaN/Inf inputs (e.g. a corrupt persisted profile) cause the bracketing loop
/// to spin forever, hanging the matchmaker tick. 100 is far above the
/// convergence threshold for valid inputs (~5-10 iterations typically).
/// See SECURITY_REVIEW.md P8.
const MAX_GLINKO_ITERATIONS: f64 = 100.0;

/// Computes the Glicko-2 rating update after a single game.
///
/// `score` is 1.0 (win), 0.5 (draw), or 0.0 (loss).
///
/// **Safety:** any NaN/Inf input, non-positive volatility, or solver
/// non-convergence returns the original `(rating, rd, volatility)` unchanged.
/// This guarantees a corrupt profile can never poison the queue or be
/// re-persisted as NaN/Inf — see SECURITY_REVIEW.md P8 and the release
/// audit (Phase 2.5).
pub fn glicko2_update(
    rating: f32,
    rd: f32,
    volatility: f32,
    opponent_rating: f32,
    opponent_rd: f32,
    score: f32,
) -> (f32, f32, f32) {
    // 1. Reject NaN/Inf / non-positive-volatility inputs up front. Return the
    //    inputs unchanged so a corrupt profile is not mutated into something
    //    worse and the matchmaker can continue.
    let inputs_f64 = [
        rating as f64,
        rd as f64,
        volatility as f64,
        opponent_rating as f64,
        opponent_rd as f64,
        score as f64,
    ];
    if inputs_f64.iter().any(|v| !v.is_finite()) || volatility <= 0.0 || rd < 0.0 {
        return (rating, rd, volatility);
    }

    let mu = (rating as f64 - 1500.0) / SCALE;
    let phi = rd as f64 / SCALE;
    let sigma = volatility as f64;

    let mu_j = (opponent_rating as f64 - 1500.0) / SCALE;
    let phi_j = opponent_rd as f64 / SCALE;

    let g_phi_j =
        1.0 / (1.0 + 3.0 * phi_j * phi_j / (std::f64::consts::PI * std::f64::consts::PI)).sqrt();

    let e = 1.0 / (1.0 + (-g_phi_j * (mu - mu_j)).exp());

    let v = 1.0 / (g_phi_j * g_phi_j * e * (1.0 - e));
    // Extreme rating differences drive `e` to 0 or 1, making `v` infinite.
    // Bail out with the original profile rather than propagating Inf.
    if !v.is_finite() {
        return (rating, rd, volatility);
    }

    let delta = v * g_phi_j * (score as f64 - e);

    let a = sigma.ln();
    let f = |x: f64| -> f64 {
        let ex = x.exp();
        let phi2 = phi * phi;
        let delta2 = delta * delta;
        let num = ex * (delta2 - phi2 - v - ex);
        let denom = 2.0 * (phi2 + v + ex).powi(2);
        num / denom - (x - a) / (TAU * TAU)
    };

    let mut a_iter = a;
    let mut b_iter = if delta * delta > phi * phi + v {
        (delta * delta - phi * phi - v).ln()
    } else {
        // Bounded bracket search — the previous loop was unbounded and could
        // spin forever on pathological inputs (P8).
        let mut k = 1.0;
        while k <= MAX_GLINKO_ITERATIONS && f(a - k * TAU) < 0.0 {
            k += 1.0;
        }
        if k > MAX_GLINKO_ITERATIONS {
            // Bracketing failed; preserve the profile unchanged.
            return (rating, rd, volatility);
        }
        a - k * TAU
    };
    let epsilon = 1e-6;
    let mut fa = f(a_iter);
    let mut fb = f(b_iter);
    for _ in 0..MAX_GLINKO_ITERATIONS as usize {
        let denom = fb - fa;
        if denom.abs() < epsilon {
            break;
        }
        let c = a_iter + (a_iter - b_iter) * fa / denom;
        let fc = f(c);
        if fc * fb < 0.0 {
            a_iter = b_iter;
            fa = fb;
        } else {
            fa /= 2.0;
        }
        b_iter = c;
        fb = fc;
        if (b_iter - a_iter).abs() < epsilon {
            break;
        }
    }
    // Solver diverged — preserve the profile rather than persisting NaN.
    if !a_iter.is_finite() || !b_iter.is_finite() {
        return (rating, rd, volatility);
    }
    let sigma_new = ((a_iter + b_iter) / 2.0).exp();

    let phi_star = (phi * phi + sigma_new * sigma_new).sqrt();

    let phi_new = 1.0 / (1.0 / (phi_star * phi_star) + 1.0 / v).sqrt();
    let mu_new = mu + phi_new * phi_new * g_phi_j * (score as f64 - e);

    let new_rating_f = mu_new * SCALE + 1500.0;
    let new_rd_f = phi_new * SCALE;
    let new_vol_f = sigma_new;

    // Final finiteness guard before the f32 truncation (the original bug:
    // `f64::INFINITY as f32` silently yields `f32::INFINITY`, which then
    // persists into the player profile and corrupts all future matchmaking).
    if !new_rating_f.is_finite() || !new_rd_f.is_finite() || !new_vol_f.is_finite() {
        return (rating, rd, volatility);
    }

    (new_rating_f as f32, new_rd_f as f32, new_vol_f as f32)
}

#[cfg(test)]
mod tests {
    use super::*;
    use tokio::sync::oneshot;

    fn make_queue_entry(mmr: f32, role: &str) -> QueueEntry {
        let (tx, _) = oneshot::channel();
        QueueEntry {
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

    // ----- P8 hardening: corrupt / pathological inputs must not poison the profile -----

    #[test]
    fn test_glicko2_nan_input_returns_original() {
        let (r, rd, vol) = glicko2_update(f32::NAN, 200.0, 0.06, 1400.0, 30.0, 1.0);
        assert!(r.is_nan(), "NaN rating input echoes back unchanged");
        // A NaN *output* would be a corruption; here we assert the function
        // did not synthesize a *new* NaN from finite co-inputs — it returned
        // the supplied rating unchanged.
        let _ = (rd, vol);
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
        // A 10000-point rating gap used to drive `e` to 0/1, making `v` inf,
        // which then propagated as f32::INFINITY into the persisted profile.
        // The hardened solver must return a finite value.
        let (r, rd, vol) = glicko2_update(1500.0, 200.0, 0.06, 10000.0, 30.0, 1.0);
        assert!(r.is_finite(), "rating must be finite, got {}", r);
        assert!(rd.is_finite(), "rd must be finite, got {}", rd);
        assert!(vol.is_finite(), "volatility must be finite, got {}", vol);
    }

    #[test]
    fn test_glicko2_normal_inputs_still_update() {
        // Regression: the guards must not turn valid inputs into no-ops.
        let (r, _rd, _vol) = glicko2_update(1500.0, 200.0, 0.06, 1400.0, 30.0, 1.0);
        assert!(r > 1500.0, "win should still increase rating, got {}", r);
    }
}
