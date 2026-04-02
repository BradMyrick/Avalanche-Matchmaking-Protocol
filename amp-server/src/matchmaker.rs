//! MMR-aware matchmaking engine.
//!
//! Implements FlexMatch-style rule evaluation: skill window, latency, region,
//! role-based team formation, and backfill tolerance widening over queue time.

use crate::state::{MatchQualityScore, QueueEntry, StoredRuleSet};

// ---------------------------------------------------------------------------
// Match finding
// ---------------------------------------------------------------------------

/// Attempts to pair `new_entry` with the best candidate in `queue`.
///
/// Returns the index of the matched entry in queue if found.
pub fn find_match<'a>(
    queue: &[QueueEntry],
    new_entry: &QueueEntry,
    ruleset: &StoredRuleSet,
) -> Option<usize> {
    let mut best_idx: Option<usize> = None;
    let mut best_score = 0.0f32;

    for (idx, candidate) in queue.iter().enumerate() {
        // Must be the same game
        if candidate.game_id != new_entry.game_id {
            continue;
        }

        // Skill tolerance check
        let skill_diff = (candidate.mmr - new_entry.mmr).abs();
        if skill_diff > ruleset.max_skill_diff {
            continue;
        }

        // Region compatibility: same region only for now
        if candidate.region != new_entry.region {
            continue;
        }

        // Compute quality and keep the best
        let quality = compute_match_quality(new_entry, candidate, ruleset);
        if quality.total_score > best_score {
            best_score = quality.total_score;
            best_idx = Some(idx);
        }
    }

    best_idx
}

// ---------------------------------------------------------------------------
// Quality scoring
// ---------------------------------------------------------------------------

/// Compute a [0,1] match quality score between two players given the ruleset.
pub fn compute_match_quality(
    player_a: &QueueEntry,
    player_b: &QueueEntry,
    ruleset: &StoredRuleSet,
) -> MatchQualityScore {
    let skill_diff = (player_a.mmr - player_b.mmr).abs();
    let max_diff = ruleset.max_skill_diff.max(1.0);
    let skill_balance = (1.0 - (skill_diff / max_diff).min(1.0)).max(0.0);

    // Role balance hint: slightly prefer different roles if they exist
    let role_hint = if !player_a.preferred_role.is_empty() && player_a.preferred_role == player_b.preferred_role {
        0.8
    } else {
        1.0
    };

    let total_score = skill_balance * 0.70 + role_hint * 0.30;
    MatchQualityScore::new(total_score)
}



// ---------------------------------------------------------------------------
// Glicko-2 rating update
// ---------------------------------------------------------------------------

const TAU: f64 = 0.5; // Glicko-2 system constant
const SCALE: f64 = 173.7178; // μ/σ conversion factor

/// Apply a single Glicko-2 update to a player's rating.
///
/// Returns (new_mmr, new_rd, new_volatility).
pub fn glicko2_update(
    rating: f32,     // Current MMR (Elo scale)
    rd: f32,         // Rating deviation
    volatility: f32, // Current volatility σ
    opponent_rating: f32,
    opponent_rd: f32,
    score: f32, // 1.0 = win, 0.5 = draw, 0.0 = loss
) -> (f32, f32, f32) {
    // Step 1 — Convert to Glicko-2 scale
    let mu = (rating as f64 - 1500.0) / SCALE;
    let phi = rd as f64 / SCALE;
    let sigma = volatility as f64;

    let mu_j = (opponent_rating as f64 - 1500.0) / SCALE;
    let phi_j = opponent_rd as f64 / SCALE;

    // Step 2 — g(φ) function
    let g_phi_j =
        1.0 / (1.0 + 3.0 * phi_j * phi_j / (std::f64::consts::PI * std::f64::consts::PI)).sqrt();

    // Step 3 — E(s | μ, μ_j, φ_j)
    let e = 1.0 / (1.0 + (-g_phi_j * (mu - mu_j)).exp());

    // Step 4 — v (estimated variance)
    let v = 1.0 / (g_phi_j * g_phi_j * e * (1.0 - e));

    // Step 5 — delta (improvement estimator)
    let delta = v * g_phi_j * (score as f64 - e);

    // Step 6 — Update volatility via Illinois algorithm
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
        let mut k = 1.0;
        while f(a - k * TAU) < 0.0 {
            k += 1.0;
        }
        a - k * TAU
    };
    let epsilon = 1e-6;
    let mut fa = f(a_iter);
    let mut fb = f(b_iter);
    for _ in 0..100 {
        let c = a_iter + (a_iter - b_iter) * fa / (fb - fa);
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
    let sigma_new = ((a_iter + b_iter) / 2.0).exp();

    // Step 7 — Pre-rating-period update
    let phi_star = (phi * phi + sigma_new * sigma_new).sqrt();

    // Step 8 — New rating
    let phi_new = 1.0 / (1.0 / (phi_star * phi_star) + 1.0 / v).sqrt();
    let mu_new = mu + phi_new * phi_new * g_phi_j * (score as f64 - e);

    // Convert back to Elo scale
    let new_rating = (mu_new * SCALE + 1500.0) as f32;
    let new_rd = (phi_new * SCALE) as f32;
    let new_volatility = sigma_new as f32;

    (new_rating, new_rd, new_volatility)
}

#[cfg(test)]
mod tests {
    use super::*;

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
        use futures::FutureExt;
        use tokio::sync::oneshot;
        let ruleset = StoredRuleSet {
            max_skill_diff: 300.0,
            ..Default::default()
        };
        let mk = |mmr: f32, role: &str| {
            let (tx, _) = oneshot::channel();
            QueueEntry {
                player_id: "p1".into(),
                game_id: "g1".into(),
                ruleset_id: "r1".into(),
                mmr,
                region: "na".into(),
                preferred_role: role.into(),
                sender: tx,
            }
        };
        let a = mk(1500.0, "tank");
        let b = mk(1500.0, "dps");
        let q = compute_match_quality(&a, &b, &ruleset);
        assert!(
            q.total_score > 0.9,
            "Perfect match should score > 0.9, got {}",
            q.total_score
        );
    }
}
