//! Shared in-memory state for AMP server.
//!
//! TODO: Replace HashMap stores with RocksDB for persistence across restarts.

use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use uuid::Uuid;

/// Unique identifier type used throughout AMP.
pub type AmpId = String;

// ---------------------------------------------------------------------------
// Player Profile Store
// ---------------------------------------------------------------------------

/// Skill tiers mapped from the Cap'n Proto Elo enum.
#[derive(Debug, Clone, Copy, PartialEq, PartialOrd)]
pub enum EloTier {
    Unranked,
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond,
    Master,
    Grandmaster,
}

impl EloTier {
    /// Returns the midpoint MMR for tier display purposes.
    pub fn midpoint_mmr(&self) -> f32 {
        match self {
            EloTier::Unranked => 0.0,
            EloTier::Bronze => 600.0,
            EloTier::Silver => 1350.0,
            EloTier::Gold => 1650.0,
            EloTier::Platinum => 1900.0,
            EloTier::Diamond => 2100.0,
            EloTier::Master => 2300.0,
            EloTier::Grandmaster => 2500.0,
        }
    }

    pub fn from_mmr(mmr: f32) -> Self {
        match mmr as u32 {
            0..=1199 => EloTier::Bronze,
            1200..=1499 => EloTier::Silver,
            1500..=1799 => EloTier::Gold,
            1800..=1999 => EloTier::Platinum,
            2000..=2199 => EloTier::Diamond,
            2200..=2399 => EloTier::Master,
            2400..=u32::MAX => EloTier::Grandmaster,
        }
    }
}

/// A stored player profile in the AMP server.
#[derive(Debug, Clone)]
pub struct StoredPlayerProfile {
    pub player_id: AmpId,
    pub display_name: String,
    pub wallet_address: Vec<u8>,

    // Skill
    pub global_mmr: f32,
    pub mmr_uncertainty: f32,   // Glicko-2 RD (rating deviation)
    pub mmr_volatility: f32,    // Glicko-2 sigma
    pub games_played: u32,

    // Stats per game (game_id -> stats)
    pub game_stats: HashMap<AmpId, StoredGameStats>,

    // Attributes
    pub preferred_role: String,
    pub language: String,
    pub platform: String,
    pub region: String,
    pub max_ping_ms: u32,

    pub is_online: bool,
    pub created_at: u64,
    pub last_login: u64,

    // Restrictions
    pub is_banned: bool,
    pub matchmaking_cooldown_until: u64, // nanoseconds epoch
}

#[derive(Debug, Clone, Default)]
pub struct StoredGameStats {
    pub game_id: AmpId,
    pub matches_played: u32,
    pub wins: u32,
    pub losses: u32,
    pub draws: u32,
    pub best_streak: u16,
    pub current_streak: i16,
    pub total_play_time_ms: u64,
}

impl Default for StoredPlayerProfile {
    fn default() -> Self {
        let now = std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)
            .unwrap_or_default()
            .as_nanos() as u64;
        Self {
            player_id: Uuid::new_v4().to_string(),
            display_name: String::new(),
            wallet_address: Vec::new(),
            global_mmr: 1200.0, // Start at silver
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
            created_at: now,
            last_login: now,
            is_banned: false,
            matchmaking_cooldown_until: 0,
        }
    }
}

// ---------------------------------------------------------------------------
// Game Registry Store
// ---------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct StoredGameConfig {
    pub game_id: AmpId,
    pub name: String,
    pub description: String,
    pub developer_id: AmpId,
    pub admin_address: Vec<u8>,
    pub supported_regions: Vec<String>,
    pub modes: Vec<StoredGameMode>,
    pub is_active: bool,
    pub total_matches: u64,
}

#[derive(Debug, Clone)]
pub struct StoredGameMode {
    pub mode_id: AmpId,
    pub name: String,
    pub min_players: u8,
    pub max_players: u8,
    pub team_count: u8,
    pub is_ranked: bool,
}

// ---------------------------------------------------------------------------
// Ruleset Store
// ---------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct StoredRuleSet {
    pub ruleset_id: AmpId,
    pub game_id: AmpId,
    pub name: String,
    pub description: String,
    pub max_skill_diff: f32,
    pub max_ping_ms: u32,
    pub timeout_ms: u64,
    pub backfill_enabled: bool,
    pub backfill_skill_tolerance: f32,
    pub required_roles: Vec<String>,
    pub is_active: bool,
}

impl Default for StoredRuleSet {
    fn default() -> Self {
        Self {
            ruleset_id: Uuid::new_v4().to_string(),
            game_id: String::new(),
            name: "Default".to_string(),
            description: String::new(),
            max_skill_diff: 300.0,
            max_ping_ms: 150,
            timeout_ms: 60_000,
            backfill_enabled: false,
            backfill_skill_tolerance: 100.0,
            required_roles: Vec::new(),
            is_active: true,
        }
    }
}

// ---------------------------------------------------------------------------
// Matchmaking Queue
// ---------------------------------------------------------------------------

use tokio::sync::oneshot;

/// An entry in the matchmaking queue.
pub struct QueueEntry {
    pub player_id: String,
    pub game_id: String,
    pub mode_id: String,
    pub ruleset_id: String,
    pub mmr: f32,
    pub mmr_uncertainty: f32,
    pub region: String,
    pub preferred_role: String,
    pub max_ping_ms: u32,
    pub enqueued_at: std::time::Instant,
    /// Resolves when a match is found; carries (match_id, opponent_player_id).
    pub sender: oneshot::Sender<MatchFoundPayload>,
}

#[derive(Debug, Clone)]
pub struct MatchFoundPayload {
    pub match_id: String,
    pub opponent_ids: Vec<String>,
    pub quality: MatchQualityScore,
    pub region: String,
}

/// Computed quality scores for a proposed match.
#[derive(Debug, Clone)]
pub struct MatchQualityScore {
    pub total_score: f32,
    pub skill_balance: f32,
    pub latency_score: f32,
    pub role_balance: f32,
}

impl MatchQualityScore {
    pub fn perfect() -> Self {
        Self { total_score: 1.0, skill_balance: 1.0, latency_score: 1.0, role_balance: 1.0 }
    }
}

// ---------------------------------------------------------------------------
// Tournament Store
// ---------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct StoredTournament {
    pub tournament_id: AmpId,
    pub game_id: AmpId,
    pub name: String,
    pub description: String,
    pub organizer_id: AmpId,
    pub max_participants: u32,
    pub is_active: bool,
    pub participants: Vec<AmpId>,
    pub created_at: u64,
}

// ---------------------------------------------------------------------------
// App State (shared across all service implementations)
// ---------------------------------------------------------------------------

#[derive(Debug, Default)]
pub struct InnerState {
    pub players: HashMap<AmpId, StoredPlayerProfile>,
    pub games: HashMap<AmpId, StoredGameConfig>,
    pub rulesets: HashMap<AmpId, StoredRuleSet>,
    pub tournaments: HashMap<AmpId, StoredTournament>,
    /// Active matches: match_id -> (player_ids)
    pub active_matches: HashMap<String, Vec<String>>,
}

/// Thread-safe shared application state.
pub type AppState = Arc<RwLock<InnerState>>;

pub fn new_state() -> AppState {
    Arc::new(RwLock::new(InnerState::default()))
}
