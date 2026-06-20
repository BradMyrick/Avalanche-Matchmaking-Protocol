use crate::IAMPRegistry;
use crate::IAMPSettlement;
use crate::error::RelayerError;
use alloy_primitives::U256;
use alloy_provider::{DynProvider, Provider, ProviderBuilder};
use serde::{Deserialize, Serialize};
use sled::Db;
use std::sync::{Arc, Mutex};
use tokio::sync::Notify;
use tracing::{info, warn};

use super::RelayerState;
use super::custodial;
use super::gas::GasManager;
use super::nonce::NonceManager;

const CF_PENDING: &str = "pending_settlements";
/// Terminal outcomes (Confirmed / Reverted / TimedOut / DeadLettered) are
/// retained here so `getSettlementStatus` can report them after the entry
/// leaves the pending tree. Keyed by match_id.
const CF_RESULTS: &str = "settlement_results";

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum SettlementStatus {
    Queued = 1,
    InFlight = 2,
    Confirmed = 3,
    Reverted = 4,
    TimedOut = 5,
    DeadLettered = 6,
}

impl SettlementStatus {
    /// Wire code returned by `getSettlementStatus`. `Unknown` (0) is only used
    /// by the RPC layer when no record is found in any tree.
    pub fn wire_code(self) -> u8 {
        self as u8
    }
    #[allow(dead_code)]
    pub fn is_terminal(self) -> bool {
        matches!(
            self,
            SettlementStatus::Confirmed
                | SettlementStatus::Reverted
                | SettlementStatus::TimedOut
                | SettlementStatus::DeadLettered
        )
    }
}

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PendingSettlement {
    pub match_id: Vec<u8>,
    pub outcome: u8,
    pub transcript_hash: Vec<u8>,
    pub signature: Vec<u8>,
    pub retry_count: u32,
    pub enqueued_at_ms: u64,
    pub last_attempt_at_ms: Option<u64>,
    #[serde(default = "default_status")]
    pub status: SettlementStatus,
    #[serde(default)]
    pub last_max_fee: Option<String>,
    #[serde(default)]
    pub last_priority_fee: Option<String>,
    /// Broadcast tx hash (hex-encoded with 0x prefix) once known. Empty until
    /// the submit call returns a pending tx. Reported back to the server via
    /// `getSettlementStatus`.
    #[serde(default)]
    pub tx_hash: String,
    /// Wall-clock millis of the last status transition. Used to TTL results.
    #[serde(default)]
    pub updated_at_ms: u64,
}

fn default_status() -> SettlementStatus {
    SettlementStatus::Queued
}

pub struct SettlementQueue {
    db: Arc<Db>,
    max_retries: u32,
    base_retry_delay_ms: u64,
    gas_manager: GasManager,
    /// Signaled by `enqueue` so the settlement processor wakes instantly when
    /// new work arrives instead of waiting on a fixed poll interval (Phase 2.2:
    /// previously the processor slept `base_retry_delay_ms` — default 1000 ms —
    /// on every iteration, hard-capping throughput at ~1 settlement/s).
    notify: Arc<Notify>,
    /// Serializes the claim read-modify-write in `mark_in_flight` so multiple
    /// concurrent workers (RELAYER_CONCURRENCY) cannot double-claim the same
    /// task (Phase 2.3). Held only across the (sync) claim, never across an
    /// `await`, so the slow chain submission still parallelizes across workers.
    claim_lock: Mutex<()>,
}

impl SettlementQueue {
    pub fn new(
        db: Arc<Db>,
        max_retries: u32,
        base_retry_delay_ms: u64,
        gas_manager: GasManager,
    ) -> Self {
        Self {
            db,
            max_retries,
            base_retry_delay_ms,
            gas_manager,
            notify: Arc::new(Notify::new()),
            claim_lock: Mutex::new(()),
        }
    }

    /// Handle used by the settlement processor to await newly-enqueued work.
    pub fn notify(&self) -> &Arc<Notify> {
        &self.notify
    }

    pub fn enqueue(&self, settlement: PendingSettlement) -> Result<(), RelayerError> {
        let cf = self.db.open_tree(CF_PENDING)?;
        let key = settlement.match_id.clone();
        if cf.contains_key(&key)? {
            warn!(
                "Settlement for match {:?} already enqueued, skipping duplicate",
                hex::encode(&key)
            );
            return Ok(());
        }
        let value = bincode::serialize(&settlement)?;
        cf.insert(&key, &value as &[u8])?;
        // Wake any idle settlement worker so new work is picked up immediately
        // rather than after the idle-poll fallback (Phase 2.2).
        self.notify.notify_one();
        info!("Enqueued settlement for match {:?}", hex::encode(&key));
        Ok(())
    }

    pub fn mark_in_flight(&self) -> Result<Option<PendingSettlement>, RelayerError> {
        // Serialize the claim across concurrent workers so two workers can't
        // both read the same Queued task and both mark it InFlight (Phase 2.3).
        // The lock is released before returning, i.e. well before any `await`
        // in the caller, so the chain submission still runs concurrently.
        let _guard = self.claim_lock.lock().unwrap_or_else(|e| e.into_inner());
        let cf = self.db.open_tree(CF_PENDING)?;
        for item in cf.iter() {
            let (key, value) = item?;
            let mut settlement: PendingSettlement = bincode::deserialize(&value)?;

            if settlement.status == SettlementStatus::InFlight {
                continue;
            }

            let now = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)?
                .as_millis() as u64;

            if let Some(last) = settlement.last_attempt_at_ms {
                let delay = self.delay_with_jitter(settlement.retry_count);
                if now.saturating_sub(last) < delay {
                    continue;
                }
            }

            settlement.status = SettlementStatus::InFlight;
            let updated = bincode::serialize(&settlement)?;
            cf.insert(&key, &updated as &[u8])?;
            return Ok(Some(settlement));
        }
        Ok(None)
    }

    pub fn finalize_settlement(&self, match_id: &[u8]) -> Result<(), RelayerError> {
        let cf = self.db.open_tree(CF_PENDING)?;
        cf.remove(match_id)?;
        Ok(())
    }

    pub fn dead_letter(&self, mut settlement: PendingSettlement) -> Result<(), RelayerError> {
        let now = now_ms();
        settlement.status = SettlementStatus::DeadLettered;
        settlement.updated_at_ms = now;
        // Terminal outcomes are retained in CF_RESULTS so the server can
        // reconcile via getSettlementStatus (release Phase 2.1).
        let cf = self.db.open_tree(CF_RESULTS)?;
        let key = settlement.match_id.clone();
        let value = bincode::serialize(&settlement)?;
        cf.insert(&key, &value as &[u8])?;
        warn!(
            "Dead-lettered settlement for match {:?} after {} retries",
            hex::encode(&key),
            settlement.retry_count
        );
        Ok(())
    }

    /// Exponential backoff with up to ±25% jitter to prevent thundering-herd
    /// retries when many settlements fail simultaneously (release Phase 2.8).
    /// Caps the exponent at 4 (16×) regardless of retry_count.
    fn delay_with_jitter(&self, retry_count: u32) -> u64 {
        let base = self
            .base_retry_delay_ms
            .saturating_mul(2u64.pow(retry_count.min(4)));
        if base == 0 {
            return 0;
        }
        // Time-derived jitter in [0, base/4]: avoids a new dependency while
        // still decorrelating concurrent retries.
        let now_ns = std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)
            .map(|d| d.subsec_nanos() as u64)
            .unwrap_or(0);
        let jitter = now_ns % (base / 4).saturating_add(1);
        base.saturating_add(jitter)
    }

    /// Look up the current status of a settlement. Returns:
    /// - `Some((status, tx_hash, retry_count, updated_at))` if a record exists
    ///   in either the pending or results tree.
    /// - `None` if the match is unknown (RPC reports status code 0).
    pub fn get_status(
        &self,
        match_id: &[u8],
    ) -> Result<Option<(SettlementStatus, String, u32, u64)>, RelayerError> {
        if let Ok(Some(bytes)) = self.db.open_tree(CF_PENDING)?.get(match_id)
            && let Ok(s) = bincode::deserialize::<PendingSettlement>(&bytes)
        {
            return Ok(Some((s.status, s.tx_hash, s.retry_count, s.updated_at_ms)));
        }
        if let Ok(Some(bytes)) = self.db.open_tree(CF_RESULTS)?.get(match_id)
            && let Ok(s) = bincode::deserialize::<PendingSettlement>(&bytes)
        {
            return Ok(Some((s.status, s.tx_hash, s.retry_count, s.updated_at_ms)));
        }
        Ok(None)
    }

    pub async fn process_next(
        &self,
        state: &RelayerState,
        nonce_manager: &NonceManager,
    ) -> Result<bool, RelayerError> {
        let settlement = match self.mark_in_flight()? {
            Some(s) => s,
            None => return Ok(false),
        };

        let match_id_parsed = if settlement.match_id.len() == 32 {
            U256::from_be_bytes::<32>(
                settlement
                    .match_id
                    .as_slice()
                    .try_into()
                    .unwrap_or([0u8; 32]),
            )
        } else {
            let s = std::str::from_utf8(&settlement.match_id)
                .map_err(|e| RelayerError::Transaction(format!("invalid match_id UTF-8: {}", e)))?;
            U256::from_str_radix(s, 10).map_err(|e| {
                RelayerError::Transaction(format!("invalid match_id '{}': {}", s, e))
            })?
        };

        let result = self
            .submit_settlement(&settlement, match_id_parsed, state, nonce_manager)
            .await;

        match result {
            Ok(_) => {
                // Finalization happens asynchronously in the spawned task
                Ok(true)
            }
            Err(e) => {
                let mut retry = settlement.clone();
                retry.retry_count += 1;
                retry.last_attempt_at_ms = Some(
                    std::time::SystemTime::now()
                        .duration_since(std::time::UNIX_EPOCH)?
                        .as_millis() as u64,
                );

                if retry.retry_count >= self.max_retries {
                    self.finalize_settlement(&retry.match_id)?;
                    self.dead_letter(retry)?;
                } else {
                    warn!(
                        "Settlement for match {} failed to submit (attempt {}): {}. Re-queueing.",
                        match_id_parsed, retry.retry_count, e
                    );
                    retry.status = SettlementStatus::Queued;

                    let cf = self.db.open_tree(CF_PENDING)?;
                    let updated = bincode::serialize(&retry)?;
                    cf.insert(&retry.match_id, &updated as &[u8])?;
                }
                Ok(true)
            }
        }
    }

    async fn submit_settlement(
        &self,
        settlement: &PendingSettlement,
        match_id: U256,
        state: &RelayerState,
        nonce_manager: &NonceManager,
    ) -> Result<(), RelayerError> {
        // 7-tuple: (gameId, playerA, state, playerB, createdAt, stakeAmount, stakeAmountB).
        // stakeAmountB was added in Phase 1.5 for fee-on-transfer support.
        let registry = IAMPRegistry::new(state.registry_addr, state.read_provider.clone());
        let returned = registry
            .matches(match_id)
            .call()
            .await
            .map_err(|e| RelayerError::Contract(e.to_string()))?;
        let game_id = returned.gameId;

        let custodial_wallet = custodial::derive_custodial_signer(
            &state.master_signer,
            "settlement",
            game_id.to::<u64>(),
            state.chain_id,
        )
        .map_err(|e| RelayerError::Transaction(format!("custodial derivation failed: {}", e)))?;
        let custodial_addr = custodial_wallet.address();

        custodial::ensure_gas(custodial_addr, state).await?;

        // One wallet-fillable provider per custodial signer (the signer is
        // game-scoped, so a fresh provider per submission is correct).
        let custodial_provider = DynProvider::new(
            ProviderBuilder::new()
                .wallet(custodial_wallet)
                .connect(&state.rpc_url)
                .await
                .map_err(|e| RelayerError::Transaction(e.to_string()))?,
        );

        let nonce = nonce_manager
            .next_nonce(&custodial_addr, &custodial_provider)
            .await?;

        if settlement.transcript_hash.len() != 32 {
            return Err(RelayerError::Transaction(format!(
                "invalid transcript_hash length: expected 32, got {}",
                settlement.transcript_hash.len()
            )));
        }
        let mut t_hash = [0u8; 32];
        t_hash.copy_from_slice(&settlement.transcript_hash);

        let async_result = IAMPSettlement::AsyncResult {
            matchId: match_id,
            outcome: settlement.outcome,
            transcriptHash: t_hash.into(),
            signature: settlement.signature.clone().into(),
        };

        let settlement_contract =
            IAMPSettlement::new(state.settlement_addr, custodial_provider.clone());

        let (network_max_fee, network_priority_fee) = self
            .gas_manager
            .estimate_eip1559_fees(&custodial_provider)
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;

        let prev_max = settlement
            .last_max_fee
            .as_ref()
            .and_then(|s| U256::from_str_radix(s, 10).ok());
        let prev_prio = settlement
            .last_priority_fee
            .as_ref()
            .and_then(|s| U256::from_str_radix(s, 10).ok());

        let (effective_max_fee, effective_priority_fee) =
            if let (true, Some(pm), Some(pp)) = (settlement.retry_count > 0, prev_max, prev_prio) {
                self.gas_manager
                    .bump_fees(network_max_fee, network_priority_fee, pm, pp)
            } else {
                (network_max_fee, network_priority_fee)
            };

        let cf = self
            .db
            .open_tree(CF_PENDING)
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;
        let mut s = settlement.clone();
        s.last_max_fee = Some(effective_max_fee.to_string());
        s.last_priority_fee = Some(effective_priority_fee.to_string());
        if let Ok(updated) = bincode::serialize(&s) {
            let _ = cf.insert(&s.match_id, &updated as &[u8]);
        }

        // Phase 3.4: alloy's call builder sets EIP-1559 fields directly (no
        // Legacy→EIP1559 enum rewrite needed, unlike ethers' TypedTransaction).
        // Gas fields take u128 on this alloy version.
        let pending = settlement_contract
            .submitAsyncResult(match_id, async_result)
            .max_fee_per_gas(effective_max_fee.to::<u128>())
            .max_priority_fee_per_gas(effective_priority_fee.to::<u128>())
            .nonce(nonce.to::<u64>())
            .send()
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;
        let tx_hash = *pending.tx_hash();
        let tx_hash_str = format!("{:?}", tx_hash);
        info!("Submitted tx {} for match {}", tx_hash_str, match_id);
        metrics::counter!("amp_settlements_submitted_total").increment(1);

        let db_clone = self.db.clone();
        let match_id_clone = s.match_id.clone();
        let match_id_parsed_clone = match_id;
        let max_retries = self.max_retries;

        // Record the broadcast tx hash so getSettlementStatus can report it
        // while the entry is InFlight (release Phase 2.1).
        {
            if let Ok(cf) = db_clone.open_tree(CF_PENDING)
                && let Ok(Some(bytes)) = cf.get(&match_id_clone)
                && let Ok(mut inflight) = bincode::deserialize::<PendingSettlement>(&bytes)
            {
                inflight.tx_hash = tx_hash_str.clone();
                inflight.updated_at_ms = now_ms();
                if let Ok(updated) = bincode::serialize(&inflight) {
                    let _ = cf.insert(&match_id_clone, &updated as &[u8]);
                }
            }
        }

        let provider_clone = custodial_provider.clone();
        tokio::spawn(async move {
            let mut attempts = 0;
            loop {
                match provider_clone.get_transaction_receipt(tx_hash).await {
                    Ok(Some(receipt)) => {
                        if receipt.status() {
                            info!(
                                "Settlement for match {} successfully mined",
                                match_id_parsed_clone
                            );
                            if let Err(e) =
                                handle_confirmed(&db_clone, &match_id_clone, &tx_hash_str)
                            {
                                warn!(
                                    "Failed to record confirmed settlement for match {}: {:?}",
                                    match_id_parsed_clone, e
                                );
                            }
                        } else {
                            warn!(
                                "Settlement tx {} for match {} reverted on-chain",
                                tx_hash_str, match_id_parsed_clone
                            );
                            if let Err(e) = handle_failure(
                                &db_clone,
                                max_retries,
                                &match_id_clone,
                                SettlementStatus::Reverted,
                            ) {
                                warn!(
                                    "Failed to handle reverted settlement for match {}: {:?}",
                                    match_id_parsed_clone, e
                                );
                            }
                        }
                        break;
                    }
                    Ok(None) => {
                        attempts += 1;
                        if attempts > 60 {
                            // ~2 mins timeout
                            warn!(
                                "Settlement tx {} for match {} timed out waiting for receipt",
                                tx_hash_str, match_id_parsed_clone
                            );
                            if let Err(e) = handle_failure(
                                &db_clone,
                                max_retries,
                                &match_id_clone,
                                SettlementStatus::TimedOut,
                            ) {
                                warn!(
                                    "Failed to handle timed-out settlement for match {}: {:?}",
                                    match_id_parsed_clone, e
                                );
                            }
                            break;
                        }
                        tokio::time::sleep(std::time::Duration::from_secs(2)).await;
                    }
                    Err(e) => {
                        warn!("RPC error polling receipt for {}: {:?}", tx_hash_str, e);
                        tokio::time::sleep(std::time::Duration::from_secs(2)).await;
                    }
                }
            }
        });

        Ok(())
    }

    pub fn replay_pending(&self) -> Result<usize, RelayerError> {
        let cf = self.db.open_tree(CF_PENDING)?;
        let mut reset_count = 0usize;
        let mut to_reset: Vec<(Vec<u8>, PendingSettlement)> = Vec::new();

        for item in cf.iter() {
            let (key, value) = item?;
            let settlement: PendingSettlement = bincode::deserialize(&value)?;
            if settlement.status == SettlementStatus::InFlight {
                let mut s = settlement;
                s.status = SettlementStatus::Queued;
                to_reset.push((key.to_vec(), s));
            }
        }

        for (key, settlement) in &to_reset {
            let bytes = bincode::serialize(settlement)?;
            cf.insert(key.as_slice(), &bytes as &[u8])?;
            reset_count += 1;
        }

        let count = cf.len();
        if count > 0 {
            info!(
                "Found {} pending settlements to replay ({} reset from in-flight)",
                count, reset_count
            );
        }
        Ok(count)
    }
}

// ─── module-level helpers (usable from spawned tasks without &self) ──────

fn now_ms() -> u64 {
    std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .map(|d| d.as_millis() as u64)
        .unwrap_or(0)
}

/// Write a terminal outcome to CF_RESULTS (keyed by match_id) so the server
/// can reconcile via getSettlementStatus.
fn record_result_db(db: &Db, settlement: &PendingSettlement) -> Result<(), RelayerError> {
    let cf = db.open_tree(CF_RESULTS)?;
    let value = bincode::serialize(settlement)?;
    cf.insert(&settlement.match_id, &value as &[u8])?;
    Ok(())
}

/// Mark a settlement as Confirmed: record the terminal status (with tx hash)
/// and remove the entry from the pending tree.
fn handle_confirmed(db: &Db, match_id: &[u8], tx_hash: &str) -> Result<(), RelayerError> {
    let cf = db.open_tree(CF_PENDING)?;
    if let Ok(Some(bytes)) = cf.get(match_id)
        && let Ok(mut s) = bincode::deserialize::<PendingSettlement>(&bytes)
    {
        s.status = SettlementStatus::Confirmed;
        s.tx_hash = tx_hash.to_string();
        s.updated_at_ms = now_ms();
        record_result_db(db, &s)?;
    }
    cf.remove(match_id)?;
    metrics::counter!("amp_settlements_confirmed_total").increment(1);
    Ok(())
}

/// Receipt-time failure (revert or timeout): re-queue, or dead-letter once
/// `max_retries` is exceeded. Unlike the pre-fix behavior — which re-queued
/// forever and never surfaced the failure to the server (release Phase 2.8) —
/// this records a terminal `DeadLettered` result so the server reconciliation
/// loop can flag the desync.
fn handle_failure(
    db: &Db,
    max_retries: u32,
    match_id: &[u8],
    failure: SettlementStatus,
) -> Result<(), RelayerError> {
    let cf = db.open_tree(CF_PENDING)?;
    let Some(bytes) = cf.get(match_id)? else {
        // Entry already gone (e.g. finalized); nothing to re-queue.
        return Ok(());
    };
    let mut s = bincode::deserialize::<PendingSettlement>(&bytes)?;
    let now = now_ms();
    s.retry_count = s.retry_count.saturating_add(1);
    s.last_attempt_at_ms = Some(now);
    s.updated_at_ms = now;

    let label = match failure {
        SettlementStatus::Reverted => "reverted on-chain",
        SettlementStatus::TimedOut => "timed out",
        _ => "failed",
    };

    if s.retry_count >= max_retries {
        s.status = SettlementStatus::DeadLettered;
        warn!(
            "Settlement for match {:?} {} for the last time after {} attempts; dead-lettering",
            hex::encode(&s.match_id),
            label,
            s.retry_count
        );
        metrics::counter!("amp_settlements_dead_lettered_total").increment(1);
        record_result_db(db, &s)?;
        cf.remove(&s.match_id)?;
    } else {
        s.status = SettlementStatus::Queued;
        warn!(
            "Settlement for match {:?} {} (attempt {}); re-queueing",
            hex::encode(&s.match_id),
            label,
            s.retry_count
        );
        metrics::counter!("amp_settlement_retries_total").increment(1);
        let updated = bincode::serialize(&s)?;
        cf.insert(&s.match_id, &updated as &[u8])?;
    }
    Ok(())
}
