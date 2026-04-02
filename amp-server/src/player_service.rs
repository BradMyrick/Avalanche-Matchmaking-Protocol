//! Cap'n Proto service implementation for PlayerProfile management.

use capnp::capability::Promise;
use capnp_rpc::pry;
use tracing::info;

use crate::matchmaker::glicko2_update;
use crate::state::{now_ns, AppState, StoredGameStats};

pub struct PlayerServiceImpl {
    pub state: AppState,
}

impl PlayerServiceImpl {
    /// Apply a match result and update this player's MMR via Glicko-2.
    ///
    /// `score`: 1.0 = win, 0.5 = draw, 0.0 = loss.
    pub async fn record_match_result(
        &self,
        player_id: &str,
        opponent_id: &str,
        game_id: &str,
        score: f32,
        play_time_ms: u64,
    ) -> Result<(), String> {
        let mut state = self.state.write().await;

        // Get opponent MMR snapshot (before update)
        let (opp_mmr, opp_rd) = state
            .players
            .get(opponent_id)
            .map(|p| (p.global_mmr, p.mmr_uncertainty))
            .unwrap_or((1200.0, 350.0));

        let player = state
            .players
            .get_mut(player_id)
            .ok_or_else(|| format!("Player {} not found", player_id))?;

        // Update global MMR
        let (new_mmr, new_rd, new_vol) = glicko2_update(
            player.global_mmr,
            player.mmr_uncertainty,
            player.mmr_volatility,
            opp_mmr,
            opp_rd,
            score,
        );
        player.global_mmr = new_mmr;
        player.mmr_uncertainty = new_rd;
        player.mmr_volatility = new_vol;
        player.games_played += 1;

        // Update per-game stats
        let stats = player
            .game_stats
            .entry(game_id.to_string())
            .or_insert_with(StoredGameStats::default);
        stats.matches_played += 1;
        stats.total_play_time_ms += play_time_ms;
        if score >= 1.0 {
            stats.wins += 1;
            stats.current_streak = stats.current_streak.max(0) + 1;
            if stats.current_streak as u16 > stats.best_streak {
                stats.best_streak = stats.current_streak as u16;
            }
        } else if score <= 0.0 {
            stats.losses += 1;
            stats.current_streak = stats.current_streak.min(0) - 1;
        } else {
            stats.draws += 1;
            stats.current_streak = 0;
        }

        info!(
            "Updated {} MMR: {:.0} → {:.0} (±{:.0}) after match vs {}",
            player_id, opp_mmr, new_mmr, new_rd, opponent_id
        );
        Ok(())
    }
}
use crate::player_profile_capnp;

impl player_profile_capnp::player_profile_service::Server for PlayerServiceImpl {
    fn create_or_update_profile(
        &mut self,
        params: player_profile_capnp::player_profile_service::CreateOrUpdateProfileParams,
        mut results: player_profile_capnp::player_profile_service::CreateOrUpdateProfileResults,
    ) -> Promise<(), ::capnp::Error> {
        let p = pry!(params.get());
        let player_id = p
            .get_player_id()
            .ok()
            .map(|s| String::from_utf8_lossy(s).to_string());
        let display_name = p
            .get_display_name()
            .ok()
            .and_then(|s| s.to_string().ok())
            .unwrap_or_default();
        let wallet_address = p.get_wallet_address().unwrap_or_default().to_vec();
        let preferred_role = p
            .get_preferred_role()
            .ok()
            .and_then(|s| s.to_string().ok())
            .unwrap_or_default();
        let language = p
            .get_language()
            .ok()
            .and_then(|s| s.to_string().ok())
            .unwrap_or_default();
        let platform = p
            .get_platform()
            .ok()
            .and_then(|s| s.to_string().ok())
            .unwrap_or_default();
        let region = p
            .get_region()
            .ok()
            .and_then(|s| s.to_string().ok())
            .unwrap_or_default();
        let max_ping_ms = p.get_max_ping_ms();
        let initial_mmr = Some(p.get_initial_mmr());

        let state = self.state.clone();
        Promise::from_future(async move {
            let mut state_w = state.write().await;
            let id = player_id.unwrap_or_else(|| uuid::Uuid::new_v4().to_string());
            let profile = state_w.players.entry(id.clone()).or_insert_with(crate::state::StoredPlayerProfile::default);
            if !display_name.is_empty() {
                profile.display_name = display_name;
            }
            if !wallet_address.is_empty() {
                profile.wallet_address = wallet_address;
            }
            if !preferred_role.is_empty() {
                profile.preferred_role = preferred_role;
            }
            if !language.is_empty() {
                profile.language = language;
            }
            if !platform.is_empty() {
                profile.platform = platform;
            }
            if !region.is_empty() {
                profile.region = region;
            }
            if max_ping_ms > 0 {
                profile.max_ping_ms = max_ping_ms;
            }
            if let Some(m) = initial_mmr {
                profile.global_mmr = m;
            }
            profile.last_login = now_ns();
            profile.is_online = true;

            results.get().set_player_id(id.as_bytes());
            Ok(())
        })
    }

    fn get_profile(
        &mut self,
        _: player_profile_capnp::player_profile_service::GetProfileParams,
        _: player_profile_capnp::player_profile_service::GetProfileResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }

    fn record_match_result(
        &mut self,
        _: player_profile_capnp::player_profile_service::RecordMatchResultParams,
        _: player_profile_capnp::player_profile_service::RecordMatchResultResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }

    fn set_offline(
        &mut self,
        _: player_profile_capnp::player_profile_service::SetOfflineParams,
        _: player_profile_capnp::player_profile_service::SetOfflineResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }

    fn list_players(
        &mut self,
        _: player_profile_capnp::player_profile_service::ListPlayersParams,
        _: player_profile_capnp::player_profile_service::ListPlayersResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }

    fn apply_restriction(
        &mut self,
        _: player_profile_capnp::player_profile_service::ApplyRestrictionParams,
        _: player_profile_capnp::player_profile_service::ApplyRestrictionResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }
}
