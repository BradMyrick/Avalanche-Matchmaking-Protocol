use capnp::capability::Promise;
use capnp_rpc::pry;
use tracing::{info, warn};

use crate::matchmaker::glicko2_update;
use crate::state::{AmpId, AppState, StoredGameStats, StoredPlayerProfile, now_ms, now_ns};

pub struct PlayerServiceImpl {
    pub state: AppState,
}

impl PlayerServiceImpl {
    pub async fn record_match_result(
        &self,
        player_id: &str,
        opponent_id: &str,
        game_id: &str,
        score: f32,
        play_time_ms: u64,
    ) -> Result<(), String> {
        let (opp_mmr, opp_rd) = self
            .state
            .players
            .get(opponent_id)
            .map(|p| (p.global_mmr, p.mmr_uncertainty))
            .unwrap_or((1200.0, 350.0));

        let mut player = self
            .state
            .players
            .get_mut(player_id)
            .ok_or_else(|| format!("Player {} not found", player_id))?;

        let (new_mmr, new_rd, new_vol) = glicko2_update(
            player.global_mmr,
            player.mmr_uncertainty,
            player.mmr_volatility,
            opp_mmr,
            opp_rd,
            score,
        );
        let old_mmr = player.global_mmr;
        player.global_mmr = new_mmr;
        player.mmr_uncertainty = new_rd;
        player.mmr_volatility = new_vol;
        player.games_played += 1;

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
            "Updated {} MMR: {:.0} -> {:.0} (+/-{:.0}) vs {}",
            player_id, old_mmr, new_mmr, new_rd, opponent_id
        );

        let profile = player.clone();
        drop(player);
        self.state.persist_player(player_id, &profile).await;

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
        let initial_mmr = p.get_initial_mmr();

        let state = self.state.clone();
        Promise::from_future(async move {
            let id = player_id.unwrap_or_else(|| uuid::Uuid::new_v4().to_string());
            let mut profile_entry = state
                .players
                .entry(id.clone())
                .or_insert_with(StoredPlayerProfile::default);
            if !display_name.is_empty() {
                profile_entry.display_name = display_name;
            }
            if !wallet_address.is_empty() {
                profile_entry.wallet_address = wallet_address;
            }
            if !preferred_role.is_empty() {
                profile_entry.preferred_role = preferred_role;
            }
            if !language.is_empty() {
                profile_entry.language = language;
            }
            if !platform.is_empty() {
                profile_entry.platform = platform;
            }
            if !region.is_empty() {
                profile_entry.region = region;
            }
            if max_ping_ms > 0 {
                profile_entry.max_ping_ms = max_ping_ms;
            }
            if initial_mmr > 0.0 {
                profile_entry.global_mmr = initial_mmr;
            }
            profile_entry.last_login = now_ns();
            profile_entry.is_online = true;

            let profile_for_persist = profile_entry.clone();
            drop(profile_entry);
            state.persist_player(&id, &profile_for_persist).await;

            results.get().set_player_id(id.as_bytes());
            Ok(())
        })
    }

    fn get_profile(
        &mut self,
        params: player_profile_capnp::player_profile_service::GetProfileParams,
        mut results: player_profile_capnp::player_profile_service::GetProfileResults,
    ) -> Promise<(), ::capnp::Error> {
        let p = pry!(params.get());
        let player_id_bytes = pry!(p.get_player_id());
        let player_id = String::from_utf8_lossy(player_id_bytes).to_string();
        let state = self.state.clone();

        Promise::from_future(async move {
            match state.players.get(&player_id) {
                Some(profile) => {
                    let mut out = results.get().init_profile();
                    out.set_display_name(&profile.display_name);
                    if !profile.wallet_address.is_empty() {
                        out.reborrow().set_wallet_address(&profile.wallet_address);
                    }

                    {
                        let mut mmr = out.reborrow().init_global_m_m_r();
                        mmr.set_rating(profile.global_mmr);
                        mmr.set_uncertainty(profile.mmr_uncertainty);
                        mmr.set_volatility(profile.mmr_volatility);
                    }

                    {
                        let mut attrs = out.reborrow().init_attributes();
                        if !profile.preferred_role.is_empty() {
                            attrs.set_preferred_role(&profile.preferred_role);
                        }
                        if !profile.language.is_empty() {
                            attrs.set_language(&profile.language);
                        }
                        if !profile.platform.is_empty() {
                            attrs.set_platform(&profile.platform);
                        }
                        attrs.set_max_ping_ms(profile.max_ping_ms);
                    }

                    {
                        let mut restr = out.reborrow().init_restrictions();
                        restr.set_is_banned(profile.restrictions.is_banned);
                        restr.set_ban_reason(&profile.restrictions.ban_reason);
                        restr.set_matchmaking_cool_down(
                            profile
                                .restrictions
                                .matchmaking_cooldown_until
                                .saturating_sub(now_ms()),
                        );
                    }
                }
                None => {
                    return Err(::capnp::Error::failed(format!(
                        "Player {} not found",
                        player_id
                    )));
                }
            }
            Ok(())
        })
    }

    fn record_match_result(
        &mut self,
        params: player_profile_capnp::player_profile_service::RecordMatchResultParams,
        _results: player_profile_capnp::player_profile_service::RecordMatchResultResults,
    ) -> Promise<(), ::capnp::Error> {
        let p = pry!(params.get());
        let player_id = String::from_utf8_lossy(pry!(p.get_player_id())).to_string();
        let opponent_id = String::from_utf8_lossy(pry!(p.get_opponent_id())).to_string();
        let game_id = String::from_utf8_lossy(pry!(p.get_game_id())).to_string();
        let score = p.get_score();
        let play_time_ms = p.get_play_time_ms();

        let state = self.state.clone();
        Promise::from_future(async move {
            let service = PlayerServiceImpl { state };
            if let Err(e) = service
                .record_match_result(&player_id, &opponent_id, &game_id, score, play_time_ms)
                .await
            {
                warn!("record_match_result failed: {}", e);
                return Err(::capnp::Error::failed(e));
            }
            Ok(())
        })
    }

    fn set_offline(
        &mut self,
        params: player_profile_capnp::player_profile_service::SetOfflineParams,
        _: player_profile_capnp::player_profile_service::SetOfflineResults,
    ) -> Promise<(), ::capnp::Error> {
        let p = pry!(params.get());
        let player_id = String::from_utf8_lossy(pry!(p.get_player_id())).to_string();
        let state = self.state.clone();

        Promise::from_future(async move {
            if let Some(mut profile) = state.players.get_mut(&player_id) {
                profile.is_online = false;
                let profile_clone = profile.clone();
                drop(profile);
                state.persist_player(&player_id, &profile_clone).await;
            }
            Ok(())
        })
    }

    fn list_players(
        &mut self,
        _: player_profile_capnp::player_profile_service::ListPlayersParams,
        mut results: player_profile_capnp::player_profile_service::ListPlayersResults,
    ) -> Promise<(), ::capnp::Error> {
        let state = self.state.clone();
        Promise::from_future(async move {
            let online_players: Vec<
                dashmap::mapref::multiple::RefMulti<AmpId, StoredPlayerProfile>,
            > = state.players.iter().filter(|p| p.is_online).collect();
            let mut list = results.get().init_players(online_players.len() as u32);
            for (i, profile) in online_players.iter().enumerate() {
                let mut entry = list.reborrow().get(i as u32);
                entry.set_display_name(&profile.display_name);
                if !profile.wallet_address.is_empty() {
                    entry.set_wallet_address(&profile.wallet_address);
                }
            }
            Ok(())
        })
    }

    fn apply_restriction(
        &mut self,
        params: player_profile_capnp::player_profile_service::ApplyRestrictionParams,
        _: player_profile_capnp::player_profile_service::ApplyRestrictionResults,
    ) -> Promise<(), ::capnp::Error> {
        let p = pry!(params.get());
        let player_id = String::from_utf8_lossy(pry!(p.get_player_id())).to_string();
        let ban = p.get_ban();
        let cooldown_ms = p.get_cooldown_ms();
        let state = self.state.clone();

        Promise::from_future(async move {
            if let Some(mut profile) = state.players.get_mut(&player_id) {
                if ban {
                    profile.restrictions.is_banned = true;
                    profile.restrictions.ban_reason = "admin_restriction".to_string();
                    profile.restrictions.ban_expiry = u64::MAX;
                    info!("Player {} banned", player_id);
                } else {
                    profile.restrictions.is_banned = false;
                    profile.restrictions.ban_reason = String::new();
                    profile.restrictions.ban_expiry = 0;
                    info!("Player {} unbanned", player_id);
                }
                if cooldown_ms > 0 {
                    profile.restrictions.matchmaking_cooldown_until = now_ms() + cooldown_ms;
                    info!("Player {} cooldown set for {}ms", player_id, cooldown_ms);
                }
                let profile_clone = profile.clone();
                drop(profile);
                state.persist_player(&player_id, &profile_clone).await;
            }
            Ok(())
        })
    }
}
