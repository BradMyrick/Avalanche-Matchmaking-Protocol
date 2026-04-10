use crate::rules;
use crate::state::{MatchQualityDetail, QueueEntry, StoredRuleSet};

pub fn find_match(
    queue: &[QueueEntry],
    new_entry: &QueueEntry,
    ruleset: &StoredRuleSet,
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
    ruleset: &StoredRuleSet,
) -> MatchQualityDetail {
    let result = rules::evaluate_rules(player_a, player_b, ruleset);
    result.quality
}

const TAU: f64 = 0.5;
const SCALE: f64 = 173.7178;

pub fn glicko2_update(
    rating: f32,
    rd: f32,
    volatility: f32,
    opponent_rating: f32,
    opponent_rd: f32,
    score: f32,
) -> (f32, f32, f32) {
    let mu = (rating as f64 - 1500.0) / SCALE;
    let phi = rd as f64 / SCALE;
    let sigma = volatility as f64;

    let mu_j = (opponent_rating as f64 - 1500.0) / SCALE;
    let phi_j = opponent_rd as f64 / SCALE;

    let g_phi_j = 1.0
        / (1.0
            + 3.0 * phi_j * phi_j
                / (std::f64::consts::PI * std::f64::consts::PI))
            .sqrt();

    let e = 1.0 / (1.0 + (-g_phi_j * (mu - mu_j)).exp());

    let v = 1.0 / (g_phi_j * g_phi_j * e * (1.0 - e));

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

    let phi_star = (phi * phi + sigma_new * sigma_new).sqrt();

    let phi_new = 1.0 / (1.0 / (phi_star * phi_star) + 1.0 / v).sqrt();
    let mu_new = mu + phi_new * phi_new * g_phi_j * (score as f64 - e);

    let new_rating = (mu_new * SCALE + 1500.0) as f32;
    let new_rd = (phi_new * SCALE) as f32;
    let new_volatility = sigma_new as f32;

    (new_rating, new_rd, new_volatility)
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
        let (new_r, _, _) =
            glicko2_update(1500.0, 200.0, 0.06, 1400.0, 30.0, 1.0);
        assert!(new_r > 1500.0, "Win should increase rating, got {}", new_r);
    }

    #[test]
    fn test_glicko2_loss_decreases_rating() {
        let (new_r, _, _) =
            glicko2_update(1500.0, 200.0, 0.06, 1600.0, 30.0, 0.0);
        assert!(new_r < 1500.0, "Loss should decrease rating, got {}", new_r);
    }

    #[test]
    fn test_glicko2_draw_near_stable() {
        let (new_r, _, _) =
            glicko2_update(1500.0, 200.0, 0.06, 1500.0, 30.0, 0.5);
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
        let ruleset = StoredRuleSet::default();
        let q = compute_match_quality(&a, &b, &ruleset);
        assert!(
            q.total_score > 0.5,
            "Perfect match should score well, got {}",
            q.total_score
        );
    }
}
