use crate::state::now_ns;
use anyhow::{Context, Result, bail};
use ethers_core::types::{Address, H256, Signature};
use ethers_core::utils::hash_message;
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use tracing::info;

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

    pub async fn create_challenge(&self, game_id: u64) -> (Vec<u8>, u64) {
        let nonce = uuid::Uuid::new_v4().to_string();
        let message = format!("AMP_AUTH:{}:{}", game_id, nonce);
        let msg_bytes = message.as_bytes().to_vec();
        let created_at = now_ns();
        let expires_at = created_at + CHALLENGE_TTL_NS;

        self.challenges.write().await.insert(
            nonce.clone(),
            AuthChallenge {
                message: msg_bytes.clone(),
                created_at,
                game_id,
            },
        );

        info!(target: "auth", "Created challenge for game_id={}", game_id);
        (msg_bytes, expires_at)
    }

    pub async fn verify_login(
        &self,
        game_id: u64,
        sig_bytes: &[u8],
    ) -> Result<Address> {
        let address = recover_address(sig_bytes)
            .context("failed to recover address from signature")?;

        let address_hex = hex::encode(address.as_bytes());
        info!(target: "auth", "Recovered address: 0x{}", address_hex);

        let challenge = extract_challenge(sig_bytes);
        let nonce = match challenge {
            Some(n) => n,
            None => {
                bail!("signature does not contain a valid challenge reference")
            }
        };

        let mut challenges = self.challenges.write().await;
        let entry = match challenges.remove(&nonce) {
            Some(e) => e,
            None => bail!("unknown or expired challenge nonce"),
        };

        let now = now_ns();
        if now.saturating_sub(entry.created_at) > CHALLENGE_TTL_NS {
            bail!("challenge expired");
        }

        if entry.game_id != game_id {
            bail!(
                "challenge was issued for game_id={}, not {}",
                entry.game_id,
                game_id
            );
        }

        let expected_hash: H256 = hash_message(&entry.message);
        let recovered = Signature::try_from(sig_bytes)
            .map_err(|e| anyhow::anyhow!("invalid signature: {:?}", e))?
            .recover(expected_hash)
            .map_err(|e| anyhow::anyhow!("recovery failed: {:?}", e))?;

        if recovered != address {
            bail!("signature does not match challenge message");
        }

        info!(target: "auth", "Authenticated 0x{} for game_id={}", address_hex, game_id);
        Ok(address)
    }

    #[allow(dead_code)]
    pub async fn cleanup_expired(&self) {
        let now = now_ns();
        let mut challenges = self.challenges.write().await;
        let before = challenges.len();
        challenges
            .retain(|_, c| now.saturating_sub(c.created_at) < CHALLENGE_TTL_NS);
        let removed = before - challenges.len();
        if removed > 0 {
            info!(target: "auth", "Cleaned up {} expired challenges", removed);
        }
    }
}

fn extract_challenge(sig_bytes: &[u8]) -> Option<String> {
    if sig_bytes.len() < 65 {
        return None;
    }
    let payload = &sig_bytes[65..];
    let message = String::from_utf8_lossy(payload);
    if message.starts_with("AMP_AUTH:") {
        let parts: Vec<&str> = message.splitn(3, ':').collect();
        if parts.len() == 3 {
            return Some(parts[2].to_string());
        }
    }
    None
}

fn recover_address(sig_bytes: &[u8]) -> Result<Address> {
    if sig_bytes.len() < 65 {
        bail!("signature too short: {} bytes", sig_bytes.len());
    }

    let sig = Signature::try_from(&sig_bytes[..65])
        .map_err(|e| anyhow::anyhow!("invalid signature format: {:?}", e))?;

    let payload = if sig_bytes.len() > 65 {
        &sig_bytes[65..]
    } else {
        b"AMP_LOGIN"
    };

    let msg_hash: H256 = hash_message(payload);
    let address = sig
        .recover(msg_hash)
        .map_err(|e| anyhow::anyhow!("address recovery failed: {:?}", e))?;

    Ok(address)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_reject_short_signature() {
        let result = recover_address(&[0u8; 32]);
        assert!(result.is_err());
    }

    #[test]
    fn test_reject_garbage_signature() {
        let result = recover_address(&[0u8; 65]);
        assert!(result.is_err());
    }

    #[test]
    fn test_extract_challenge_valid() {
        let sig = [0u8; 65];
        let mut full = sig.to_vec();
        full.extend_from_slice(b"AMP_AUTH:42:some-nonce");
        let result = extract_challenge(&full);
        assert_eq!(result, Some("some-nonce".to_string()));
    }

    #[test]
    fn test_extract_challenge_no_payload() {
        assert!(extract_challenge(&[0u8; 65]).is_none());
    }

    #[test]
    fn test_extract_challenge_wrong_prefix() {
        let sig = [0u8; 65];
        let mut full = sig.to_vec();
        full.extend_from_slice(b"WRONG_PREFIX:42:nonce");
        assert!(extract_challenge(&full).is_none());
    }
}
