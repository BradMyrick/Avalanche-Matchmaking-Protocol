use arc_swap::ArcSwap;
use dashmap::DashMap;
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::oneshot;

use crate::persistence::Persistence;

pub type AmpId = String;

pub fn now_ns() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_nanos() as u64
}

pub fn now_ms() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_millis() as u64
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StoredPlayerProfile {
    pub display_name: String,
    pub wallet_address: Vec<u8>,

    pub global_mmr: f32,
    pub mmr_uncertainty: f32,
    pub mmr_volatility: f32,
    pub games_played: u32,

    pub game_stats: HashMap<AmpId, StoredGameStats>,

    pub preferred_role: String,
    pub language: String,
    pub platform: String,
    pub region: String,
    pub max_ping_ms: u32,

    pub is_online: bool,
    pub last_login: u64,

    pub restrictions: StoredPlayerRestrictions,
}

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
pub struct StoredGameStats {
    pub matches_played: u32,
    pub wins: u32,
    pub losses: u32,
    pub draws: u32,
    pub best_streak: u16,
    pub current_streak: i16,
    pub total_play_time_ms: u64,
}

#[derive(Debug, Clone, Default, Serialize, Deserialize)]
pub struct StoredPlayerRestrictions {
    pub is_banned: bool,
    pub ban_expiry: u64,
    pub ban_reason: String,
    pub matchmaking_cooldown_until: u64,
    pub daily_match_limit: u32,
    pub matches_today: u32,
    pub last_match_day: u64,
}

impl Default for StoredPlayerProfile {
    fn default() -> Self {
        let now = now_ns();
        Self {
            display_name: String::new(),
            wallet_address: Vec::new(),
            global_mmr: 1200.0,
            mmr_uncertainty: 350.0,
            mmr_volatility: 0.06,
            games_played: 0,
            game_stats: HashMap::new(),
            preferred_role: String::new(),
            language: "en".to_string(),
            platform: "pc".to_string(),
            region: "na".to_string(),
            max_ping_ms: 150,
            is_online: false,
            last_login: now,
            restrictions: StoredPlayerRestrictions::default(),
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StoredRuleSet {
    pub ruleset_id: AmpId,
    pub name: String,
    pub game_id: AmpId,
    pub mode_id: AmpId,
    pub rules: Vec<StoredRule>,
    pub timeout_ms: u64,
    pub backfill: Option<StoredBackfillPolicy>,
    pub version: String,
    pub is_active: bool,
    pub max_skill_diff: f32,
}

impl StoredRuleSet {
    pub fn new_sorted(mut self) -> Arc<Self> {
        self.rules.sort_by(|a, b| {
            b.is_hard_constraint
                .cmp(&a.is_hard_constraint)
                .then_with(|| a.priority.cmp(&b.priority))
        });
        Arc::new(self)
    }

    pub fn default_arc() -> Arc<Self> {
        Self::default().new_sorted()
    }
}

impl Default for StoredRuleSet {
    fn default() -> Self {
        Self {
            ruleset_id: String::new(),
            name: "default".to_string(),
            game_id: String::new(),
            mode_id: String::new(),
            rules: vec![StoredRule::default_skill()],
            timeout_ms: 30_000,
            backfill: None,
            version: "1.0".to_string(),
            is_active: true,
            max_skill_diff: 500.0,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StoredRule {
    pub rule_id: String,
    pub name: String,
    pub rule_type: RuleType,
    pub params: RuleParams,
    pub weight: f32,
    pub is_hard_constraint: bool,
    pub priority: u8,
}

impl StoredRule {
    pub fn default_skill() -> Self {
        Self {
            rule_id: "default-skill".to_string(),
            name: "Skill Difference".to_string(),
            rule_type: RuleType::Skill,
            params: RuleParams::Skill(SkillParams {
                max_difference: 500.0,
                use_trueskill: false,
                team_variance: 0.0,
                time_decay: false,
                decay_rate: 0.0,
            }),
            weight: 0.70,
            is_hard_constraint: true,
            priority: 0,
        }
    }
}

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum RuleType {
    Latency,
    Skill,
    TeamBalance,
    Region,
    Language,
    Schedule,
    Inventory,
    Party,
    Avoidance,
    Preference,
    Custom,
    PingBased,
    SkillDecay,
    RecentMatches,
    ConnectionQuality,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub enum RuleParams {
    Latency(LatencyParams),
    Skill(SkillParams),
    TeamBalance(TeamBalanceParams),
    Region(RegionParams),
    Language(LanguageParams),
    Schedule(ScheduleParams),
    Inventory(InventoryParams),
    Party(PartyParams),
    Avoidance(AvoidanceParams),
    Preference(PreferenceParams),
    Custom(CustomParams),
    PingBased(PingBasedParams),
    SkillDecay(SkillDecayParams),
    RecentMatches(RecentMatchesParams),
    ConnectionQuality(ConnectionQualityParams),
}

impl Default for RuleParams {
    fn default() -> Self {
        RuleParams::Skill(SkillParams::default())
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LatencyParams {
    pub max_ping_ms: u32,
    pub measurement_method: String,
    pub allow_region_override: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SkillParams {
    pub max_difference: f32,
    pub use_trueskill: bool,
    pub team_variance: f32,
    pub time_decay: bool,
    pub decay_rate: f32,
}

impl Default for SkillParams {
    fn default() -> Self {
        Self {
            max_difference: 500.0,
            use_trueskill: false,
            team_variance: 0.0,
            time_decay: false,
            decay_rate: 0.0,
        }
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct TeamBalanceParams {
    pub required_roles: Vec<String>,
    pub max_duplicates: u8,
    pub team_count: u8,
    pub flex_slots: u8,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RegionParams {
    pub allowed_regions: Vec<String>,
    pub cross_region_after_ms: u64,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct LanguageParams {
    pub prefer_same: bool,
    pub weight: f32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ScheduleParams {
    pub time_windows: Vec<(u8, u8, u8)>,
    pub days_of_week: Vec<u8>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct InventoryParams {
    pub required_items: Vec<String>,
    pub banned_items: Vec<String>,
    pub min_collection_score: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PartyParams {
    pub allow_mixed: bool,
    pub max_size: u8,
    pub skill_method: PartySkillMethod,
    pub solo_queue_bonus: bool,
}

#[derive(Debug, Clone, Copy, Serialize, Deserialize)]
pub enum PartySkillMethod {
    Highest,
    Average,
    Weighted,
    AdjustedAverage,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct AvoidanceParams {
    pub max_recent_opponents: u8,
    pub avoid_blocked: bool,
    pub cooldown_minutes: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PreferenceParams {
    pub prefer_new_opponents: bool,
    pub prefer_similar_playtime: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct CustomParams {
    pub evaluator_id: String,
    pub config: Vec<u8>,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PingBasedParams {
    pub max_ping_ms: u32,
    pub region_override: bool,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct SkillDecayParams {
    pub inactive_days: u32,
    pub decay_per_day: f32,
    pub min_mmr: f32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct RecentMatchesParams {
    pub max_with_same_opponent: u8,
    pub window_minutes: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ConnectionQualityParams {
    pub max_packet_loss: f32,
    pub max_jitter_ms: u32,
    pub min_bandwidth_kbps: u32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct StoredBackfillPolicy {
    pub enabled: bool,
    pub max_time_ms: u64,
    pub skill_tolerance_multiplier: f32,
    pub partial_teams: bool,
    pub role_flexibility: bool,
    pub connection_tolerance: bool,
}

impl Default for StoredBackfillPolicy {
    fn default() -> Self {
        Self {
            enabled: true,
            max_time_ms: 15_000,
            skill_tolerance_multiplier: 1.5,
            partial_teams: true,
            role_flexibility: true,
            connection_tolerance: true,
        }
    }
}

pub struct QueueEntry {
    pub player_id: AmpId,
    pub game_id: String,
    pub ruleset_id: String,
    pub mmr: f32,
    #[allow(dead_code)]
    pub mmr_uncertainty: f32,
    pub region: String,
    pub preferred_role: String,
    pub language: String,
    pub max_ping_ms: u32,
    pub enqueued_at_ms: u64,
    pub sender: oneshot::Sender<MatchFoundPayload>,
}

#[derive(Debug, Clone, PartialEq)]
pub struct MatchFoundPayload {
    pub match_id: String,
    pub opponent_ids: Vec<String>,
    pub quality: MatchQualityDetail,
}

#[derive(Debug, Clone, Default, PartialEq)]
pub struct MatchQualityDetail {
    pub total_score: f32,
    pub skill_balance: f32,
    pub role_balance: f32,
    pub region_score: f32,
    pub latency_score: f32,
    pub language_score: f32,
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct ActiveMatch {
    pub match_id: String,
    pub game_id: String,
    pub players: Vec<String>,
    pub created_at_ms: u64,
    pub settled: bool,
    #[serde(default)]
    pub settled_at_ms: Option<u64>,
    #[serde(default)]
    pub expires_at_ms: Option<u64>,
}

pub const MAX_ACTIVE_MATCHES: usize = 10_000;
pub const MATCH_TTL_MS: u64 = 3_600_000;
pub const SETTLED_ARCHIVE_TREE: &str = "settled_matches";

pub struct InnerState {
    pub players: DashMap<AmpId, StoredPlayerProfile>,
    pub rulesets: Arc<ArcSwap<HashMap<AmpId, Arc<StoredRuleSet>>>>,
    pub active_matches: Arc<ArcSwap<HashMap<String, ActiveMatch>>>,
    persistence: Option<Persistence>,
}

impl InnerState {
    pub fn new(persistence: Option<Persistence>) -> Self {
        let state = Self {
            players: DashMap::new(),
            rulesets: Arc::new(ArcSwap::from_pointee(HashMap::new())),
            active_matches: Arc::new(ArcSwap::from_pointee(HashMap::new())),
            persistence,
        };
        if let Some(ref p) = state.persistence {
            if let Ok(players) = p.load_all::<StoredPlayerProfile>("players") {
                for (id, profile) in players {
                    state.players.insert(id, profile);
                }
            }
            if let Ok(rulesets) = p.load_all::<StoredRuleSet>("rulesets") {
                let mut rs = HashMap::new();
                for (id, r) in rulesets {
                    rs.insert(id, r.new_sorted());
                }
                state.rulesets.store(Arc::new(rs));
            }
            if let Ok(matches) = p.load_all::<ActiveMatch>("matches") {
                let mut am = HashMap::new();
                for (id, m) in matches {
                    am.insert(id, m);
                }
                state.active_matches.store(Arc::new(am));
            }
        }
        state
    }

    pub fn insert_active_match(&self, match_id: String, active: ActiveMatch) {
        self.active_matches.rcu(|am| {
            let mut matches = (**am).clone();
            matches.insert(match_id.clone(), active.clone());
            matches
        });
    }

    pub fn update_active_match<F>(&self, match_id: &str, mut f: F) -> Option<()>
    where
        F: FnMut(&mut ActiveMatch),
    {
        let mut updated = false;
        self.active_matches.rcu(|am| {
            let mut matches = (**am).clone();
            if let Some(m) = matches.get_mut(match_id) {
                f(m);
                updated = true;
            } else {
                updated = false;
            }
            matches
        });
        if updated { Some(()) } else { None }
    }

    pub async fn persist_player(&self, id: &str, profile: &StoredPlayerProfile) {
        if let Some(ref p) = self.persistence
            && let Err(e) = p.save("players", id, profile).await
        {
            warn!(target: "persist", "Failed to persist player {}: {}", id, e);
        }
    }

    #[allow(dead_code)]
    pub async fn persist_ruleset(&self, id: &str, ruleset: &StoredRuleSet) {
        if let Some(ref p) = self.persistence
            && let Err(e) = p.save("rulesets", id, ruleset).await
        {
            warn!(target: "persist", "Failed to persist ruleset {}: {}", id, e);
        }
    }

    pub async fn persist_match(&self, id: &str, m: &ActiveMatch) {
        if let Some(ref p) = self.persistence
            && let Err(e) = p.save("matches", id, m).await
        {
            warn!(target: "persist", "Failed to persist match {}: {}", id, e);
        }
    }

    pub async fn archive_settled_match(&self, id: &str, m: &ActiveMatch) {
        if let Some(ref p) = self.persistence
            && let Err(e) = p.save(SETTLED_ARCHIVE_TREE, id, m).await
        {
            warn!(target: "persist", "Failed to archive settled match {}: {}", id, e);
        }
        self.active_matches.rcu(|am| {
            let mut matches = (**am).clone();
            matches.remove(id);
            matches
        });
        if let Some(ref p) = self.persistence
            && let Err(e) = p.delete("matches", id).await
        {
            warn!(target: "persist", "Failed to delete active match {}: {}", id, e);
        }
    }

    pub async fn cleanup_expired_matches(&self) -> usize {
        let now = now_ms();
        let expired: Vec<String> = {
            let matches = self.active_matches.load();
            matches
                .iter()
                .filter(|(_, m)| {
                    if m.settled
                        && let Some(settled_at) = m.settled_at_ms
                    {
                        return now.saturating_sub(settled_at) > MATCH_TTL_MS;
                    }
                    if let Some(expires_at) = m.expires_at_ms {
                        return now > expires_at && !m.settled;
                    }
                    false
                })
                .map(|(id, _)| id.clone())
                .collect()
        };

        let count = expired.len();
        if count > 0 {
            // First collect the matches we are going to remove
            let mut to_persist = Vec::new();
            {
                let am = self.active_matches.load();
                for id in &expired {
                    if let Some(m) = am.get(id) {
                        to_persist.push((id.clone(), m.clone()));
                    }
                }
            }

            // Do persistence (async)
            for (id, m) in to_persist {
                if let Some(ref p) = self.persistence {
                    let _ = p.save(SETTLED_ARCHIVE_TREE, &id, &m).await;
                    let _ = p.delete("matches", &id).await;
                }
                warn!(target: "cleanup", "Expired match {} (settled={}, created={}ms ago)", id, m.settled, now.saturating_sub(m.created_at_ms));
            }

            // Finally, reliably remove them from the map via rcu
            self.active_matches.rcu(|am| {
                let mut matches = (**am).clone();
                for id in &expired {
                    matches.remove(id);
                }
                matches
            });
        }
        count
    }

    #[allow(dead_code)]
    pub async fn delete_match(&self, id: &str) {
        if let Some(ref p) = self.persistence
            && let Err(e) = p.delete("matches", id).await
        {
            warn!(target: "persist", "Failed to delete match {}: {}", id, e);
        }
    }
}

use tracing::warn;

pub type AppState = Arc<InnerState>;

pub fn new_state(persistence: Option<Persistence>) -> AppState {
    Arc::new(InnerState::new(persistence))
}
