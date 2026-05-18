use crate::state::{
    AvoidanceParams, ConnectionQualityParams, LanguageParams, LatencyParams,
    MatchQualityDetail, PreferenceParams, QueueEntry, RegionParams, RuleParams,
    RuleType, SkillDecayParams, SkillParams, StoredBackfillPolicy, StoredRule,
    StoredRuleSet, TeamBalanceParams, now_ms,
};

pub struct RuleEvaluationResult {
    pub passes: bool,
    pub quality: MatchQualityDetail,
}

pub fn evaluate_rules(
    entry_a: &QueueEntry,
    entry_b: &QueueEntry,
    ruleset: &StoredRuleSet,
) -> RuleEvaluationResult {
    let mut quality = MatchQualityDetail::default();
    let queue_duration_a = now_ms().saturating_sub(entry_a.enqueued_at_ms);
    let queue_duration_b = now_ms().saturating_sub(entry_b.enqueued_at_ms);
    let max_queue_duration = queue_duration_a.max(queue_duration_b);

    let effective_skill_diff =
        compute_effective_skill_diff(ruleset, max_queue_duration);

    let mut hard_pass = true;
    let mut total_weight: f32 = 0.0;

    let sorted_rules = sort_rules(&ruleset.rules);

    for rule in &sorted_rules {
        if rule.is_hard_constraint && !hard_pass {
            break;
        }

        let result = evaluate_single_rule(
            rule,
            entry_a,
            entry_b,
            effective_skill_diff,
            max_queue_duration,
        );

        if rule.is_hard_constraint && !result.passes {
            hard_pass = false;
        }

        accumulate_quality(
            &mut quality,
            &rule.rule_type,
            result.score,
            rule.weight,
        );
        total_weight += rule.weight;
    }

    if let Some(ref backfill) = ruleset.backfill
        && backfill.enabled
        && max_queue_duration > backfill.max_time_ms
    {
        apply_backfill(
            &mut quality,
            backfill,
            entry_a,
            entry_b,
            effective_skill_diff,
        );
        hard_pass = true;
    }

    if total_weight > 0.0 {
        quality.total_score /= total_weight;
    }
    quality.total_score = quality.total_score.clamp(0.0, 1.0);

    RuleEvaluationResult {
        passes: hard_pass,
        quality,
    }
}

fn compute_effective_skill_diff(
    ruleset: &StoredRuleSet,
    queue_duration_ms: u64,
) -> f32 {
    let base = ruleset.max_skill_diff;
    for rule in &ruleset.rules {
        if let RuleParams::Skill(sp) = &rule.params
            && sp.time_decay
            && sp.decay_rate > 0.0
        {
            let minutes = queue_duration_ms as f32 / 60_000.0;
            return base + sp.decay_rate * minutes * base;
        }
    }
    base
}

struct SingleResult {
    passes: bool,
    score: f32,
}

fn evaluate_single_rule(
    rule: &StoredRule,
    a: &QueueEntry,
    b: &QueueEntry,
    effective_skill_diff: f32,
    _queue_duration_ms: u64,
) -> SingleResult {
    match &rule.params {
        RuleParams::Skill(params) => {
            evaluate_skill(params, a, b, effective_skill_diff)
        }
        RuleParams::Latency(params) => evaluate_latency(params, a, b),
        RuleParams::Region(params) => evaluate_region(params, a, b),
        RuleParams::Language(params) => evaluate_language(params, a, b),
        RuleParams::TeamBalance(params) => evaluate_team_balance(params, a, b),
        RuleParams::Avoidance(params) => evaluate_avoidance(params, a, b),
        RuleParams::Preference(params) => evaluate_preference(params, a, b),
        RuleParams::ConnectionQuality(params) => {
            evaluate_connection_quality(params, a, b)
        }
        RuleParams::Schedule(_) => SingleResult {
            passes: true,
            score: 1.0,
        },
        RuleParams::Inventory(_) => SingleResult {
            passes: true,
            score: 1.0,
        },
        RuleParams::Party(_) => SingleResult {
            passes: true,
            score: 1.0,
        },
        RuleParams::Custom(_) => SingleResult {
            passes: true,
            score: 1.0,
        },
        RuleParams::PingBased(params) => evaluate_latency(
            &LatencyParams {
                max_ping_ms: params.max_ping_ms,
                measurement_method: "direct".to_string(),
                allow_region_override: params.region_override,
            },
            a,
            b,
        ),
        RuleParams::SkillDecay(params) => evaluate_skill_decay(params, a, b),
        RuleParams::RecentMatches(_) => SingleResult {
            passes: true,
            score: 1.0,
        },
    }
}

fn evaluate_skill(
    _params: &SkillParams,
    a: &QueueEntry,
    b: &QueueEntry,
    effective_max: f32,
) -> SingleResult {
    let diff = (a.mmr - b.mmr).abs();
    let max_diff = effective_max.max(1.0);
    let score = (1.0 - (diff / max_diff).min(1.0)).max(0.0);
    SingleResult {
        passes: diff <= effective_max,
        score,
    }
}

fn evaluate_latency(
    params: &LatencyParams,
    a: &QueueEntry,
    b: &QueueEntry,
) -> SingleResult {
    let max_ping = a.max_ping_ms.min(b.max_ping_ms).min(params.max_ping_ms);
    let estimated_ping = estimate_ping(&a.region, &b.region);
    if estimated_ping <= max_ping as f32 {
        SingleResult {
            passes: true,
            score: 1.0 - (estimated_ping / max_ping as f32).min(1.0),
        }
    } else if params.allow_region_override {
        SingleResult {
            passes: true,
            score: 0.3,
        }
    } else {
        SingleResult {
            passes: false,
            score: 0.0,
        }
    }
}

fn evaluate_region(
    params: &RegionParams,
    a: &QueueEntry,
    b: &QueueEntry,
) -> SingleResult {
    if a.region == b.region {
        return SingleResult {
            passes: true,
            score: 1.0,
        };
    }
    if params.allowed_regions.contains(&a.region)
        && params.allowed_regions.contains(&b.region)
    {
        return SingleResult {
            passes: true,
            score: 0.5,
        };
    }
    SingleResult {
        passes: false,
        score: 0.0,
    }
}

fn evaluate_language(
    params: &LanguageParams,
    a: &QueueEntry,
    b: &QueueEntry,
) -> SingleResult {
    if !params.prefer_same {
        return SingleResult {
            passes: true,
            score: 1.0,
        };
    }
    if a.language == b.language {
        SingleResult {
            passes: true,
            score: 1.0,
        }
    } else {
        SingleResult {
            passes: true,
            score: 0.5,
        }
    }
}

fn evaluate_team_balance(
    params: &TeamBalanceParams,
    a: &QueueEntry,
    b: &QueueEntry,
) -> SingleResult {
    if params.required_roles.is_empty() {
        return SingleResult {
            passes: true,
            score: 1.0,
        };
    }
    if a.preferred_role == b.preferred_role && params.max_duplicates > 0 {
        return SingleResult {
            passes: true,
            score: 0.6,
        };
    }
    let both_have_roles =
        params.required_roles.iter().any(|r| r == &a.preferred_role)
            && params.required_roles.iter().any(|r| r == &b.preferred_role);
    SingleResult {
        passes: both_have_roles || params.flex_slots > 0,
        score: if both_have_roles { 1.0 } else { 0.5 },
    }
}

fn evaluate_avoidance(
    _params: &AvoidanceParams,
    _a: &QueueEntry,
    _b: &QueueEntry,
) -> SingleResult {
    SingleResult {
        passes: true,
        score: 1.0,
    }
}

fn evaluate_preference(
    params: &PreferenceParams,
    _a: &QueueEntry,
    _b: &QueueEntry,
) -> SingleResult {
    SingleResult {
        passes: true,
        score: if params.prefer_new_opponents {
            0.8
        } else {
            1.0
        },
    }
}

fn evaluate_connection_quality(
    _params: &ConnectionQualityParams,
    _a: &QueueEntry,
    _b: &QueueEntry,
) -> SingleResult {
    SingleResult {
        passes: true,
        score: 1.0,
    }
}

fn evaluate_skill_decay(
    params: &SkillDecayParams,
    a: &QueueEntry,
    b: &QueueEntry,
) -> SingleResult {
    let diff = (a.mmr - b.mmr).abs();
    let floor = params.min_mmr;
    let a_floor = a.mmr >= floor;
    let b_floor = b.mmr >= floor;
    SingleResult {
        passes: a_floor && b_floor,
        score: if diff < 200.0 { 1.0 } else { 0.5 },
    }
}

fn sort_rules(rules: &[StoredRule]) -> Vec<&StoredRule> {
    let mut sorted: Vec<&StoredRule> = rules.iter().collect();
    sorted.sort_by(|a, b| {
        b.is_hard_constraint
            .cmp(&a.is_hard_constraint)
            .then_with(|| a.priority.cmp(&b.priority))
    });
    sorted
}

fn accumulate_quality(
    quality: &mut MatchQualityDetail,
    rule_type: &RuleType,
    score: f32,
    weight: f32,
) {
    let weighted = score * weight;
    quality.total_score += weighted;
    match rule_type {
        RuleType::Skill | RuleType::SkillDecay => {
            quality.skill_balance += weighted
        }
        RuleType::Latency | RuleType::PingBased => {
            quality.latency_score += weighted
        }
        RuleType::Region => quality.region_score += weighted,
        RuleType::Language => quality.language_score += weighted,
        RuleType::TeamBalance => quality.role_balance += weighted,
        _ => {}
    }
}

fn apply_backfill(
    quality: &mut MatchQualityDetail,
    policy: &StoredBackfillPolicy,
    a: &QueueEntry,
    b: &QueueEntry,
    effective_skill_diff: f32,
) {
    let skill_diff = (a.mmr - b.mmr).abs();
    let relaxed_max = effective_skill_diff * policy.skill_tolerance_multiplier;
    if skill_diff <= relaxed_max {
        quality.skill_balance = (quality.skill_balance * 0.7).max(0.2);
    }
    if policy.role_flexibility {
        quality.role_balance = (quality.role_balance * 0.8).max(0.3);
    }
    if policy.connection_tolerance {
        quality.latency_score = (quality.latency_score * 0.8).max(0.2);
    }
    quality.total_score = quality.skill_balance
        + quality.role_balance
        + quality.region_score
        + quality.latency_score
        + quality.language_score;
}

fn estimate_ping(region_a: &str, region_b: &str) -> f32 {
    if region_a == region_b {
        return 20.0;
    }
    match (region_a, region_b) {
        ("na", "eu") | ("eu", "na") => 90.0,
        ("na", "sa") | ("sa", "na") => 120.0,
        ("na", "as") | ("as", "na") => 180.0,
        ("eu", "as") | ("as", "eu") => 160.0,
        ("eu", "sa") | ("sa", "eu") => 170.0,
        ("as", "sa") | ("sa", "as") => 250.0,
        _ => 150.0,
    }
}

#[cfg(test)]
mod tests {
    use super::*;
    use crate::state::StoredRule;
    use tokio::sync::oneshot;

    fn make_entry(mmr: f32, region: &str, role: &str) -> QueueEntry {
        let (tx, _) = oneshot::channel();
        QueueEntry {
            player_id: "p1".into(),
            game_id: "g1".into(),
            ruleset_id: "r1".into(),
            mmr,
            mmr_uncertainty: 200.0,
            region: region.into(),
            preferred_role: role.into(),
            language: "en".into(),
            max_ping_ms: 150,
            enqueued_at_ms: now_ms(),
            sender: tx,
        }
    }

    fn default_ruleset() -> StoredRuleSet {
        StoredRuleSet::default()
    }

    #[test]
    fn test_skill_match_passes() {
        let a = make_entry(1500.0, "na", "tank");
        let b = make_entry(1400.0, "na", "dps");
        let rs = default_ruleset();
        let result = evaluate_rules(&a, &b, &rs);
        assert!(result.passes);
        assert!(result.quality.skill_balance > 0.0);
    }

    #[test]
    fn test_region_mismatch_fails() {
        let a = make_entry(1500.0, "na", "tank");
        let b = make_entry(1500.0, "eu", "dps");
        let mut rs = default_ruleset();
        rs.rules.push(StoredRule {
            rule_id: "region".into(),
            name: "Region".into(),
            rule_type: RuleType::Region,
            params: RuleParams::Region(RegionParams {
                allowed_regions: vec![],
                cross_region_after_ms: 0,
            }),
            weight: 0.1,
            is_hard_constraint: true,
            priority: 1,
        });
        let result = evaluate_rules(&a, &b, &rs);
        assert!(!result.passes);
    }

    #[test]
    fn test_language_match_scores_higher() {
        let mut a = make_entry(1500.0, "na", "tank");
        a.language = "en".to_string();
        let mut b = make_entry(1500.0, "na", "dps");
        b.language = "en".to_string();

        let mut rs = default_ruleset();
        rs.rules.push(StoredRule {
            rule_id: "lang".into(),
            name: "Language".into(),
            rule_type: RuleType::Language,
            params: RuleParams::Language(LanguageParams {
                prefer_same: true,
                weight: 0.1,
            }),
            weight: 0.1,
            is_hard_constraint: false,
            priority: 10,
        });

        let result = evaluate_rules(&a, &b, &rs);
        assert!(result.quality.language_score > 0.0);
    }
}
