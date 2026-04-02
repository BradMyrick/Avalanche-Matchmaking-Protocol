//! Game registry and ruleset service implementation.

use std::time::{SystemTime, UNIX_EPOCH};
use tracing::{info, warn};
use uuid::Uuid;

use crate::state::{AppState, StoredGameConfig, StoredGameMode, StoredRuleSet};

pub struct GameRegistryServiceImpl {
    pub state: AppState,
}

pub struct RulesetServiceImpl {
    pub state: AppState,
}

fn now_ns() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .unwrap_or_default()
        .as_nanos() as u64
}

// ---------------------------------------------------------------------------
// Game Registry
// ---------------------------------------------------------------------------

impl GameRegistryServiceImpl {
    /// Register a new game with the protocol. Returns the assigned game ID.
    pub async fn register_game(
        &self,
        name: String,
        description: String,
        developer_id: String,
        admin_address: Vec<u8>,
        supported_regions: Vec<String>,
        modes: Vec<(String, String, u8, u8, u8, bool)>, // (mode_id, name, min, max, teams, ranked)
    ) -> String {
        let game_id = Uuid::new_v4().to_string();
        let stored_modes: Vec<StoredGameMode> = modes
            .into_iter()
            .map(|(mode_id, mode_name, min_players, max_players, team_count, is_ranked)| {
                StoredGameMode {
                    mode_id: if mode_id.is_empty() {
                        Uuid::new_v4().to_string()
                    } else {
                        mode_id
                    },
                    name: mode_name,
                    min_players,
                    max_players,
                    team_count,
                    is_ranked,
                }
            })
            .collect();

        let config = StoredGameConfig {
            game_id: game_id.clone(),
            name: name.clone(),
            description,
            developer_id,
            admin_address,
            supported_regions,
            modes: stored_modes,
            is_active: true,
            total_matches: 0,
        };

        let mut state = self.state.write().await;
        state.games.insert(game_id.clone(), config);
        info!("Registered game '{}' with id {}", name, game_id);
        game_id
    }

    /// Get a game configuration by ID.
    pub async fn get_game(&self, game_id: &str) -> Option<StoredGameConfig> {
        let state = self.state.read().await;
        state.games.get(game_id).cloned()
    }

    /// List all active games.
    pub async fn list_games(&self, active_only: bool) -> Vec<StoredGameConfig> {
        let state = self.state.read().await;
        state
            .games
            .values()
            .filter(|g| !active_only || g.is_active)
            .cloned()
            .collect()
    }

    /// Suspend or reactivate a game.
    pub async fn set_game_active(&self, game_id: &str, active: bool) -> Result<(), String> {
        let mut state = self.state.write().await;
        let game = state
            .games
            .get_mut(game_id)
            .ok_or_else(|| format!("Game {} not found", game_id))?;
        game.is_active = active;
        warn!("Set game {} active={}", game_id, active);
        Ok(())
    }

    /// Increment the total match counter for a game.
    pub async fn increment_match_count(&self, game_id: &str) {
        let mut state = self.state.write().await;
        if let Some(game) = state.games.get_mut(game_id) {
            game.total_matches += 1;
        }
    }
}

// ---------------------------------------------------------------------------
// Ruleset Service
// ---------------------------------------------------------------------------

impl RulesetServiceImpl {
    /// Create a new ruleset. Returns the assigned ruleset ID.
    pub async fn create_ruleset(
        &self,
        game_id: String,
        name: String,
        description: String,
        max_skill_diff: f32,
        max_ping_ms: u32,
        timeout_ms: u64,
        backfill_enabled: bool,
        backfill_skill_tolerance: f32,
        required_roles: Vec<String>,
    ) -> String {
        let ruleset_id = Uuid::new_v4().to_string();
        let ruleset = StoredRuleSet {
            ruleset_id: ruleset_id.clone(),
            game_id,
            name: name.clone(),
            description,
            max_skill_diff,
            max_ping_ms,
            timeout_ms,
            backfill_enabled,
            backfill_skill_tolerance,
            required_roles,
            is_active: true,
        };

        let mut state = self.state.write().await;
        state.rulesets.insert(ruleset_id.clone(), ruleset);
        info!("Created ruleset '{}' ({})", name, ruleset_id);
        ruleset_id
    }

    /// Get a ruleset by ID.
    pub async fn get_ruleset(&self, ruleset_id: &str) -> Option<StoredRuleSet> {
        let state = self.state.read().await;
        state.rulesets.get(ruleset_id).cloned()
    }

    /// Get the first active ruleset for a game (fallback default).
    pub async fn get_default_ruleset_for_game(&self, game_id: &str) -> Option<StoredRuleSet> {
        let state = self.state.read().await;
        state
            .rulesets
            .values()
            .find(|r| r.game_id == game_id && r.is_active)
            .cloned()
    }

    /// Get an active ruleset by ID, falling back to a built-in default.
    pub async fn get_or_default(&self, ruleset_id: &str, game_id: &str) -> StoredRuleSet {
        let state = self.state.read().await;

        // Try exact match
        if let Some(rs) = state.rulesets.get(ruleset_id) {
            return rs.clone();
        }
        // Try any ruleset for this game
        if let Some(rs) = state.rulesets.values().find(|r| r.game_id == game_id && r.is_active) {
            return rs.clone();
        }
        // Built-in safe default — 300 MMR window, 150ms max ping, 60s timeout
        StoredRuleSet {
            ruleset_id: "default".to_string(),
            game_id: game_id.to_string(),
            name: "Default".to_string(),
            ..Default::default()
        }
    }

    /// List all rulesets for a game.
    pub async fn list_rulesets_for_game(&self, game_id: &str) -> Vec<StoredRuleSet> {
        let state = self.state.read().await;
        state.rulesets.values().filter(|r| r.game_id == game_id).cloned().collect()
    }

    /// Delete a ruleset.
    pub async fn delete_ruleset(&self, ruleset_id: &str) -> Result<(), String> {
        let mut state = self.state.write().await;
        state
            .rulesets
            .remove(ruleset_id)
            .map(|_| ())
            .ok_or_else(|| format!("Ruleset {} not found", ruleset_id))
    }
}
