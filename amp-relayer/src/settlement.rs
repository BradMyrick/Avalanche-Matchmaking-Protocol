use anyhow::Result;
use ethers::prelude::*;
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

#[derive(Debug, Clone, Serialize, Deserialize)]
pub struct PendingSettlement {
    pub match_id: Vec<u8>,
    pub outcome: u8,
    pub transcript_hash: Vec<u8>,
    pub signature: Vec<u8>,
    pub retry_count: u32,
    pub enqueued_at_ms: u64,
    pub last_attempt_at_ms: Option<u64>,
}

pub struct SettlementQueue {
    db: Arc<Db>,
    max_retries: u32,
    base_retry_delay_ms: u64,
}

impl SettlementQueue {
    pub fn new(
        db: Arc<Db>,
        max_retries: u32,
        base_retry_delay_ms: u64,
    ) -> Self {
        Self {
            db,
            max_retries,
            base_retry_delay_ms,
        }
    }

    pub fn enqueue(&self, settlement: PendingSettlement) -> Result<()> {
        let cf = self.db.open_tree(CF_PENDING)?;
        let key = settlement.match_id.clone();
        let value = bincode::serialize(&settlement)?;
        cf.insert(&key, &value as &[u8])?;
        info!("Enqueued settlement for match {:?}", hex::encode(&key));
        Ok(())
    }

    pub fn dequeue(&self) -> Result<Option<PendingSettlement>> {
        let cf = self.db.open_tree(CF_PENDING)?;
        for item in cf.iter() {
            let (key, value) = item?;
            let settlement: PendingSettlement = bincode::deserialize(&value)?;

            let now = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)?
                .as_millis() as u64;

            if let Some(last) = settlement.last_attempt_at_ms {
                let delay = self.base_retry_delay_ms
                    * 2u64.pow(settlement.retry_count.min(4));
                if now.saturating_sub(last) < delay {
                    continue;
                }
            }

            cf.remove(&key)?;
            return Ok(Some(settlement));
        }
        Ok(None)
    }

    pub fn dead_letter(&self, settlement: PendingSettlement) -> Result<()> {
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
        gas_manager: &GasManager,
        nonce_manager: &NonceManager,
    ) -> Result<bool> {
        let settlement = match self.dequeue()? {
            Some(s) => s,
            None => return Ok(false),
        };

        let match_id_parsed = if settlement.match_id.len() == 32 {
            U256::from_big_endian(&settlement.match_id)
        } else {
            let s = std::str::from_utf8(&settlement.match_id).unwrap_or("");
            U256::from_dec_str(s).unwrap_or(U256::zero())
        };

        let result = self
            .submit_settlement(
                &settlement,
                match_id_parsed,
                state,
                gas_manager,
                nonce_manager,
            )
            .await;

        match result {
            Ok(_) => {
                info!("Settlement for match {} succeeded", match_id_parsed);
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
                    self.dead_letter(retry)?;
                } else {
                    warn!(
                        "Settlement for match {} failed (attempt {}): {}. Re-queueing.",
                        match_id_parsed, retry.retry_count, e
                    );
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
        gas_manager: &GasManager,
        nonce_manager: &NonceManager,
    ) -> Result<()> {
        let (game_id, _, _, _, _, _) =
            state.registry.matches(match_id).call().await?;

        let chain_id = state.master_client.signer().chain_id();
        let custodial_wallet = custodial::derive_custodial_signer(
            state.master_client.signer(),
            "settlement",
            game_id.as_u64(),
            chain_id,
        );
        let custodial_addr = custodial_wallet.address();

        custodial::ensure_gas(custodial_addr, state).await?;

        let provider = state.master_client.provider().clone();
        let custodial_client =
            SignerMiddleware::new(provider.clone(), custodial_wallet);

        let nonce =
            nonce_manager.next_nonce(&custodial_addr, &provider).await?;

        let (max_fee, _priority_fee) = gas_manager
            .estimate_eip1559_fees(&provider)
            .await
            .unwrap_or((
                U256::from(30_000_000_000u64),
                U256::from(2_000_000_000u64),
            ));

        let mut t_hash = [0u8; 32];
        if settlement.transcript_hash.len() == 32 {
            t_hash.copy_from_slice(&settlement.transcript_hash);
        }

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

        let tx = settlement_custodial
            .submit_async_result(match_id, async_result)
            .gas_price(max_fee)
            .nonce(nonce);

        let pending = tx.send().await?;
        info!("Submitted tx {} for match {}", pending.tx_hash(), match_id);

        let _receipt = pending.await?;
        nonce_manager.reset(&custodial_addr);
        Ok(())
    }

    pub fn replay_pending(&self) -> Result<usize> {
        let cf = self.db.open_tree(CF_PENDING)?;
        let count = cf.len();
        if count > 0 {
            info!("Found {} pending settlements to replay", count);
        }
        Ok(count)
    }
}
