use anyhow::{Context, Result};
use ethers_core::types::{Address, Signature, H256};
use ethers_core::utils::hash_message;
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use tracing::warn;

use crate::state::now_ns;

const CHALLENGE_TTL_NS: u64 = 300_000_000_000;

pub struct AuthChallenge {
    pub message: Vec<u8>,
    pub created_at: u64,
    pub game_id: u64,
}

pub struct AuthService {
    challenges: Arc<RwLock<HashMap<String, AuthChallenge>>>,
}

impl AuthService {
    pub fn new() -> Self {
        Self {
            challenges: Arc::new(RwLock::new(HashMap::new())),
        }
    }

    pub async fn create_challenge(&self, game_id: u64) -> Vec<u8> {
        let nonce = uuid::Uuid::new_v4().to_string();
        let message = format!("AMP_AUTH:{}:{}", game_id, nonce);
        let msg_bytes = message.as_bytes().to_vec();
        self.challenges.write().await.insert(
            nonce.clone(),
            AuthChallenge {
                message: msg_bytes.clone(),
                created_at: now_ns(),
                game_id,
            },
        );
        msg_bytes
    }

    pub async fn verify_login(
        &self,
        game_id: u64,
        sig_bytes: &[u8],
    ) -> Result<Address> {
        let address = recover_address_from_signature(sig_bytes)
            .context("failed to recover address from signature")?;

        let address_hex = hex::encode(address.as_bytes());
        info!(target: "auth", "Recovered address: 0x{}", address_hex);

        let _ = game_id;
        Ok(address)
    }

    pub async fn cleanup_expired(&self) {
        let now = now_ns();
        let mut challenges = self.challenges.write().await;
        challenges.retain(|_, c| now.saturating_sub(c.created_at) < CHALLENGE_TTL_NS);
    }
}

fn recover_address_from_signature(sig_bytes: &[u8]) -> Result<Address> {
    if sig_bytes.len() < 65 {
        anyhow::bail!("signature too short: {} bytes", sig_bytes.len());
    }

    let sig = Signature::try_from(sig_bytes)
        .map_err(|e| anyhow::anyhow!("invalid signature format: {:?}", e))?;

    let message = b"AMP_LOGIN";
    let msg_hash: H256 = hash_message(message);

    let address = sig
        .recover(msg_hash)
        .map_err(|e| anyhow::anyhow!("address recovery failed: {:?}", e))?;

    Ok(address)
}

use tracing::info;

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_reject_short_signature() {
        let result = recover_address_from_signature(&[0u8; 32]);
        assert!(result.is_err());
    }

    #[test]
    fn test_reject_garbage_signature() {
        let result = recover_address_from_signature(&[0u8; 65]);
        assert!(result.is_err());
    }
}
