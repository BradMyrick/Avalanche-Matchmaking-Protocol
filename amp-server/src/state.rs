//! Shared in-memory state for AMP server.
//!
//! TODO: Replace HashMap stores with RocksDB for persistence across restarts.

use std::collections::HashMap;
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::RwLock;

/// Unique identifier type used throughout AMP.
pub type AmpId = String;

/// Helper: current time in nanoseconds since epoch.
pub fn now_ns() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_nanos() as u64
}

// ---------------------------------------------------------------------------
// Player Profile Store
// ---------------------------------------------------------------------------

/// A stored player profile in the AMP server.
#[derive(Debug, Clone)]
pub struct StoredPlayerProfile {
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
    pub last_login: u64,
}

#[derive(Debug, Clone, Default)]
pub struct StoredGameStats {
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
        }
    }
}

// ---------------------------------------------------------------------------
// Ruleset Store
// ---------------------------------------------------------------------------

#[derive(Debug, Clone)]
pub struct StoredRuleSet {
    pub max_skill_diff: f32,
}

impl Default for StoredRuleSet {
    fn default() -> Self {
        Self {
            max_skill_diff: 500.0,
        }
    }
}

// ---------------------------------------------------------------------------
// Matchmaking Queue
// ---------------------------------------------------------------------------

use tokio::sync::oneshot;

/// An entry in the matchmaking queue.
pub struct QueueEntry {
    pub player_id: AmpId,
    pub game_id: String,
    pub ruleset_id: String,
    pub mmr: f32,
    pub region: String,
    pub preferred_role: String,
    pub sender: oneshot::Sender<MatchFoundPayload>,
}

#[derive(Debug, Clone)]
pub struct MatchFoundPayload {
    pub match_id: String,
    pub opponent_ids: Vec<String>,
    pub quality: MatchQualityScore,
}

/// Computed quality scores for a proposed match.
#[derive(Debug, Clone)]
pub struct MatchQualityScore {
    pub total_score: f32,
}

impl MatchQualityScore {
    pub fn new(score: f32) -> Self {
        Self { total_score: score }
    }
}

// ---------------------------------------------------------------------------
// App State (shared across all service implementations)
// ---------------------------------------------------------------------------

#[derive(Debug, Default)]
pub struct InnerState {
    pub players: HashMap<AmpId, StoredPlayerProfile>,
    pub rulesets: HashMap<AmpId, StoredRuleSet>,
    /// Active matches: match_id -> (player_ids)
    pub active_matches: HashMap<String, Vec<String>>,
}

/// Thread-safe shared application state.
pub type AppState = Arc<RwLock<InnerState>>;

pub fn new_state() -> AppState {
    Arc::new(RwLock::new(InnerState::default()))
}
