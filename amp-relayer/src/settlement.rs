use crate::error::RelayerError;
use ethers::prelude::*;
use ethers::types::transaction::eip2718::TypedTransaction;
use serde::{Deserialize, Serialize};
use sled::Db;
use std::sync::Arc;
use tracing::{info, warn};

use super::AsyncResult;
use super::RelayerState;
use super::custodial;
use super::gas::GasManager;
use super::nonce::NonceManager;

const CF_PENDING: &str = "pending_settlements";
const CF_DEAD_LETTER: &str = "dead_letter";

#[derive(Debug, Clone, Copy, PartialEq, Eq, Serialize, Deserialize)]
pub enum SettlementStatus {
    Queued,
    InFlight,
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
}

fn default_status() -> SettlementStatus {
    SettlementStatus::Queued
}

pub struct SettlementQueue {
    db: Arc<Db>,
    max_retries: u32,
    base_retry_delay_ms: u64,
    gas_manager: GasManager,
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
        }
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
        info!("Enqueued settlement for match {:?}", hex::encode(&key));
        Ok(())
    }

    pub fn mark_in_flight(&self) -> Result<Option<PendingSettlement>, RelayerError> {
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
                let delay = self.base_retry_delay_ms * 2u64.pow(settlement.retry_count.min(4));
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

    pub fn dead_letter(&self, settlement: PendingSettlement) -> Result<(), RelayerError> {
        let cf = self.db.open_tree(CF_DEAD_LETTER)?;
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
            U256::from_big_endian(&settlement.match_id)
        } else {
            let s = std::str::from_utf8(&settlement.match_id)
                .map_err(|e| RelayerError::Transaction(format!("invalid match_id UTF-8: {}", e)))?;
            U256::from_dec_str(s).map_err(|e| {
                RelayerError::Transaction(format!("invalid match_id '{}': {}", s, e))
            })?
        };

        let result = self
            .submit_settlement(&settlement, match_id_parsed, state, nonce_manager)
            .await;

        match result {
            Ok(_) => {
                self.finalize_settlement(&settlement.match_id)?;
                info!("Settlement for match {} succeeded", match_id_parsed);
                Ok(true)
            }
            Err(e) => {
                let reset_addr = match state.registry.matches(match_id_parsed).call().await {
                    Ok((game_id, _, _, _, _, _)) => {
                        let chain_id = state.master_client.signer().chain_id();
                        custodial::derive_custodial_signer(
                            state.master_client.signer(),
                            "settlement",
                            game_id.as_u64(),
                            chain_id,
                        )
                        .map(|w| w.address())
                        .unwrap_or_default()
                    }
                    Err(_) => Address::zero(),
                };
                nonce_manager.reset(&reset_addr);

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
                        "Settlement for match {} failed (attempt {}): {}. Re-queueing.",
                        match_id_parsed, retry.retry_count, e
                    );
                    retry.status = SettlementStatus::Queued;
                    self.enqueue(retry)?;
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
        let (game_id, _, _, _, _, _) = state
            .registry
            .matches(match_id)
            .call()
            .await
            .map_err(|e| RelayerError::Contract(e.to_string()))?;

        let chain_id = state.master_client.signer().chain_id();
        let custodial_wallet = custodial::derive_custodial_signer(
            state.master_client.signer(),
            "settlement",
            game_id.as_u64(),
            chain_id,
        )
        .map_err(|e| RelayerError::Transaction(format!("custodial derivation failed: {}", e)))?;
        let custodial_addr = custodial_wallet.address();

        custodial::ensure_gas(custodial_addr, state).await?;

        let provider = state.master_client.provider().clone();
        let custodial_client = SignerMiddleware::new(provider.clone(), custodial_wallet);

        let nonce = nonce_manager.next_nonce(&custodial_addr, &provider).await?;

        if settlement.transcript_hash.len() != 32 {
            return Err(RelayerError::Transaction(format!(
                "invalid transcript_hash length: expected 32, got {}",
                settlement.transcript_hash.len()
            )));
        }
        let mut t_hash = [0u8; 32];
        t_hash.copy_from_slice(&settlement.transcript_hash);

        let async_result = AsyncResult {
            match_id,
            outcome: settlement.outcome,
            transcript_hash: t_hash,
            signature: Bytes::from(settlement.signature.clone()),
        };

        let settlement_custodial = crate::AMPSettlementContract::new(
            state.settlement.address(),
            Arc::new(custodial_client),
        );

        // Estimate EIP-1559 fees: (max_fee_per_gas, max_priority_fee_per_gas)
        let (max_fee, priority_fee) = self
            .gas_manager
            .estimate_eip1559_fees(&provider)
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;

        // Apply gas bumping on retries using proper EIP-1559 fee fields
        let (effective_max_fee, effective_priority_fee) = if settlement.retry_count > 0 {
            self.gas_manager
                .bump_fees(max_fee, priority_fee, settlement.retry_count)
        } else {
            (max_fee, priority_fee)
        };

        // Build the contract call with proper EIP-1559 fields.
        //
        // IMPORTANT: We use `max_fee_per_gas` + `max_priority_fee_per_gas` instead of
        // the legacy `gas_price`. Avalanche C-Chain supports EIP-1559 since the Avalanche
        // Upgrade (Apricot). Using EIP-1559:
        //   - Provides better fee estimation and prevents overpaying
        //   - Allows priority fee to be set independently of max fee
        //   - Ensures compatibility with future network upgrades
        //   - Avoids the legacy gas price path which may be deprecated
        let mut call = settlement_custodial.submit_async_result(match_id, async_result);

        if let TypedTransaction::Legacy(ref legacy) = call.tx {
            let eip1559 = ethers::prelude::Eip1559TransactionRequest {
                from: legacy.from,
                to: legacy.to.clone(),
                gas: legacy.gas,
                max_fee_per_gas: Some(effective_max_fee),
                max_priority_fee_per_gas: Some(effective_priority_fee),
                value: legacy.value,
                data: legacy.data.clone(),
                nonce: Some(nonce),
                chain_id: None,
                access_list: Default::default(),
            };
            call.tx = TypedTransaction::Eip1559(eip1559);
        }

        let pending = call
            .send()
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;
        info!("Submitted tx {} for match {}", pending.tx_hash(), match_id);

        pending
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;
        nonce_manager.reset(&custodial_addr);
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
