use arc_swap::ArcSwap;
use dashmap::DashMap;
use serde::{Deserialize, Serialize};
use std::collections::HashMap;
use std::ops::{Deref, DerefMut};
use std::sync::Arc;
use std::time::{SystemTime, UNIX_EPOCH};
use tokio::sync::oneshot;

use crate::persistence::Persistence;

// Canonical rule types come from the embeddable `amp-match-core` library. The
// historical names (`StoredRuleSet`, `StoredRule`, etc.) are preserved as
// type aliases so existing amp-server call sites keep compiling — but
// `amp-server` no longer owns a parallel copy of the data model. This means
// a studio embedding `amp-match-core` and a studio running `AMP-Server` see
// the exact same algorithm and types.
#[allow(unused_imports)] // re-exported for downstream callers / tests
pub use amp_match_core::types::{
    AvoidanceParams as StoredAvoidanceParams, BackfillPolicy as StoredBackfillPolicy,
    ConnectionQualityParams as StoredConnectionQualityParams, CustomParams as StoredCustomParams,
    InventoryParams as StoredInventoryParams, LanguageParams as StoredLanguageParams,
    LatencyParams as StoredLatencyParams, MatchQualityDetail, PartyParams as StoredPartyParams,
    PartySkillMethod as StoredPartySkillMethod, PingBasedParams as StoredPingBasedParams,
    PlayerTicket, PreferenceParams as StoredPreferenceParams,
    RecentMatchesParams as StoredRecentMatchesParams, RegionParams as StoredRegionParams,
    Rule as StoredRule, RuleParams as StoredRuleParams, RuleSet as StoredRuleSet,
    RuleType as StoredRuleType, ScheduleParams as StoredScheduleParams,
    SkillDecayParams as StoredSkillDecayParams, SkillParams as StoredSkillParams,
    TeamBalanceParams as StoredTeamBalanceParams,
};

pub type AmpId = String;

pub fn now_ns() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("system clock is before UNIX_EPOCH")
        .as_nanos() as u64
}

pub fn now_ms() -> u64 {
    SystemTime::now()
        .duration_since(UNIX_EPOCH)
        .expect("system clock is before UNIX_EPOCH")
        .as_millis() as u64
}

#[derive(Debug, Clone)]
pub struct MatchSettledEvent {
    pub outcome_type: u16,
    pub victor: u8,
    pub scores: Vec<u64>,
}

/// Server→subscriber push events. `Settled` is terminal (clears the
/// subscriber list); `OpponentDisconnected` is informational and leaves the
/// subscription live so the remaining player can keep interacting.
#[derive(Debug, Clone)]
pub enum MatchEvent {
    Settled(MatchSettledEvent),
    OpponentDisconnected { player_id: String },
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

/// Server-side wrapper around `amp_match_core::PlayerTicket` that adds the
/// async notification channel the queue needs to wake players when paired.
///
/// `Deref`/`DerefMut` to `PlayerTicket` keep existing call sites
/// (`entry.player_id`, `entry.mmr`, etc.) working unchanged — the matchmaking
/// logic in `amp-match-core` operates on the inner ticket via `AsRef`.
pub struct QueueEntry {
    pub ticket: PlayerTicket,
    pub sender: oneshot::Sender<MatchFoundPayload>,
}

impl Deref for QueueEntry {
    type Target = PlayerTicket;
    fn deref(&self) -> &PlayerTicket {
        &self.ticket
    }
}

impl DerefMut for QueueEntry {
    fn deref_mut(&mut self) -> &mut PlayerTicket {
        &mut self.ticket
    }
}

impl AsRef<PlayerTicket> for QueueEntry {
    fn as_ref(&self) -> &PlayerTicket {
        &self.ticket
    }
}

#[derive(Debug, Clone, PartialEq)]
pub struct MatchFoundPayload {
    pub match_id: String,
    pub opponent_ids: Vec<String>,
    pub quality: MatchQualityDetail,
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
    /// Set by the settlement-reconciliation loop when the relayer reports a
    /// terminal on-chain failure (reverted / timed-out / dead-lettered) after
    /// the server already marked the match settled. Closes the silent
    /// server/chain state-desync gap
    #[serde(default)]
    pub settlement_failed: bool,
    /// On-chain tx hash once the relayer reports it back via
    /// getSettlementStatus. Empty until known.
    #[serde(default)]
    pub settlement_tx_hash: String,
}

pub const MAX_ACTIVE_MATCHES: usize = 10_000;
pub const MATCH_TTL_MS: u64 = 3_600_000;
pub const SETTLED_ARCHIVE_TREE: &str = "settled_matches";

pub struct InnerState {
    pub players: DashMap<AmpId, StoredPlayerProfile>,
    /// Read-heavy / write-rare → `ArcSwap` snapshot is the right structure.
    pub rulesets: Arc<ArcSwap<HashMap<AmpId, Arc<StoredRuleSet>>>>,
    /// Read+write hot path → `DashMap` (shard-locked, O(1), no clone).
    pub active_matches: DashMap<String, ActiveMatch>,
    pub match_event_senders: DashMap<String, Vec<tokio::sync::mpsc::UnboundedSender<MatchEvent>>>,
    persistence: Option<Persistence>,
}

impl InnerState {
    pub fn new(persistence: Option<Persistence>) -> Self {
        let state = Self {
            players: DashMap::new(),
            rulesets: Arc::new(ArcSwap::from_pointee(HashMap::new())),
            active_matches: DashMap::new(),
            match_event_senders: DashMap::new(),
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
                for (id, m) in matches {
                    state.active_matches.insert(id, m);
                }
            }
        }
        state
    }

    /// Number of matches currently in the active store. O(1)-ish (sums shard
    /// len counters). Used by metrics + matchmaker backpressure.
    /// Synchronously flush all pending sled writes to disk. Called on the
    /// shutdown path so terminal settlement state survives process exit
    ///. No-op when persistence
    /// is disabled (in-memory mode).
    pub fn flush(&self) -> anyhow::Result<()> {
        match &self.persistence {
            Some(p) => p.flush(),
            None => Ok(()),
        }
    }

    pub fn active_match_count(&self) -> usize {
        self.active_matches.len()
    }

    /// Clone a match out of the store (guard dropped before return). Used by
    /// call sites that need the fields but must not hold a shard lock across
    /// an `await` or a long scope.
    pub fn get_active_match(&self, match_id: &str) -> Option<ActiveMatch> {
        self.active_matches.get(match_id).map(|r| r.clone())
    }

    /// Snapshot every active match as owned `(id, match)` pairs. Only call from
    /// rare paths (player disconnect fan-out, shutdown drain) — it walks + clones.
    pub fn active_matches_snapshot(&self) -> Vec<(String, ActiveMatch)> {
        self.active_matches
            .iter()
            .map(|r| (r.key().clone(), r.value().clone()))
            .collect()
    }

    pub fn insert_active_match(&self, match_id: String, active: ActiveMatch) {
        self.active_matches.insert(match_id, active);
    }

    pub fn update_active_match<F>(&self, match_id: &str, mut f: F) -> Option<()>
    where
        F: FnMut(&mut ActiveMatch),
    {
        if let Some(mut m) = self.active_matches.get_mut(match_id) {
            f(m.value_mut());
            Some(())
        } else {
            None
        }
    }

    /// Called by the settlement-reconciliation loop when the relayer reports a
    /// terminal on-chain failure. Flips `settlement_failed` and re-persists so
    /// the flag survives a restart. This is the non-silent half of the
    /// server/chain state-desync fix
    pub async fn mark_settlement_failed(&self, match_id: &str) {
        let mut snapshot: Option<ActiveMatch> = None;
        if self
            .update_active_match(match_id, |m| {
                m.settlement_failed = true;
                snapshot = Some(m.clone());
            })
            .is_some()
        {
            if let Some(m) = snapshot {
                self.persist_match(match_id, &m).await;
            }
            warn!(
                target: "settlement",
                "Match {} flagged as settlement-failed (on-chain revert/timeout/dead-letter)",
                match_id
            );
        }
    }

    /// Record the on-chain tx hash returned by the relayer's
    /// `getSettlementStatus`. Best-effort: a missing match (already archived)
    /// is a no-op.
    pub async fn record_settlement_tx_hash(&self, match_id: &str, tx_hash: &str) {
        let mut snapshot: Option<ActiveMatch> = None;
        if self
            .update_active_match(match_id, |m| {
                m.settlement_tx_hash = tx_hash.to_string();
                snapshot = Some(m.clone());
            })
            .is_some()
            && let Some(m) = snapshot
        {
            self.persist_match(match_id, &m).await;
        }
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

    pub fn add_event_sender(
        &self,
        match_id: &str,
        tx: tokio::sync::mpsc::UnboundedSender<MatchEvent>,
    ) {
        self.match_event_senders
            .entry(match_id.to_string())
            .or_default()
            .push(tx);
    }

    /// Number of currently-registered event subscribers for a match.
    /// Used by the RPC layer to enforce a per-match subscriber cap and
    /// prevent task-amplification DoS.
    pub fn subscriber_count(&self, match_id: &str) -> usize {
        self.match_event_senders
            .get(match_id)
            .map(|s| s.len())
            .unwrap_or(0)
    }

    pub fn notify_subscribers(&self, match_id: &str, event: MatchSettledEvent) {
        // Settled is terminal: drain and clear the subscriber list.
        if let Some((_, senders)) = self.match_event_senders.remove(match_id) {
            for tx in senders {
                if let Err(e) = tx.send(MatchEvent::Settled(event.clone())) {
                    warn!(target: "events", "Failed to send settled event to subscriber for match {}: {}", match_id, e);
                }
            }
        }
    }

    /// Notify a match's subscribers that a participant disconnected. Does NOT
    /// clear the subscriber list — the remaining player(s) may still interact
    /// with the match (e.g. claim a forfeit). Drives the schema's
    /// `MatchListener::onOpponentDisconnected` callback
    pub fn notify_opponent_disconnected(&self, match_id: &str, player_id: &str) {
        if let Some(ref mut entry) = self.match_event_senders.get_mut(match_id) {
            let senders = entry.value_mut();
            senders.retain(|tx| {
                tx.send(MatchEvent::OpponentDisconnected {
                    player_id: player_id.to_string(),
                })
                .is_ok()
            });
        }
    }

    pub fn remove_event_senders(&self, match_id: &str) {
        self.match_event_senders.remove(match_id);
    }

    pub async fn archive_settled_match(&self, id: &str, m: &ActiveMatch) {
        if let Some(ref p) = self.persistence
            && let Err(e) = p.save(SETTLED_ARCHIVE_TREE, id, m).await
        {
            warn!(target: "persist", "Failed to archive settled match {}: {}", id, e);
        }
        // O(1) shard removal (was a full-map clone via `rcu`).
        if self.active_matches.remove(id).is_none() {
            warn!(target: "cleanup", "archive_settled_match: match {} absent from active store", id);
        }
        if let Some(ref p) = self.persistence
            && let Err(e) = p.delete("matches", id).await
        {
            warn!(target: "persist", "Failed to delete active match {}: {}", id, e);
        }
    }

    pub async fn cleanup_expired_matches(&self) -> usize {
        let now = now_ms();
        // Single pass: collect owned snapshots of expired matches. Iteration
        // takes shard read-locks only briefly per shard (DashMap iter does not
        // hold all shards at once), and we never mutate during iteration.
        let expired: Vec<(String, ActiveMatch)> = self
            .active_matches
            .iter()
            .filter(|r| {
                let m = r.value();
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
            .map(|r| (r.key().clone(), r.value().clone()))
            .collect();

        let count = expired.len();
        for (id, m) in &expired {
            if let Some(ref p) = self.persistence {
                if let Err(e) = p.save(SETTLED_ARCHIVE_TREE, id, m).await {
                    error!(target: "cleanup", "Failed to archive expired match {}: {}", id, e);
                }
                if let Err(e) = p.delete("matches", id).await {
                    error!(target: "cleanup", "Failed to delete expired match {}: {}", id, e);
                }
            }
            self.remove_event_senders(id);
            warn!(
                target: "cleanup",
                "Expired match {} (settled={}, created={}ms ago)",
                id, m.settled, now.saturating_sub(m.created_at_ms)
            );
            // O(1) removal per expired id (was a second full-map clone).
            self.active_matches.remove(id);
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

use tracing::{error, warn};

pub type AppState = Arc<InnerState>;

pub fn new_state(persistence: Option<Persistence>) -> AppState {
    Arc::new(InnerState::new(persistence))
}
