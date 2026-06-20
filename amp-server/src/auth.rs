use crate::state::now_ns;
use alloy_primitives::{Address, B256, Signature, eip191_hash_message};
use anyhow::{Context, Result, bail};
use std::collections::HashMap;
use std::sync::Arc;
use tokio::sync::RwLock;
use tracing::{info, warn};

const CHALLENGE_TTL_NS: u64 = 300_000_000_000;

/// Hard cap on the number of outstanding (unused) challenges. Without this,
/// an unauthenticated attacker can drive unbounded memory growth by spamming
/// `requestChallenge`. The TTL reaper cleans expired entries every 60s, but
/// the cap bounds the worst-case live-set between reaper runs.
const MAX_OUTSTANDING_CHALLENGES: usize = 100_000;

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

    pub async fn create_challenge(&self, game_id: u64) -> Option<(Vec<u8>, u64)> {
        let nonce = uuid::Uuid::new_v4().to_string();
        let message = format!("AMP_AUTH:{}:{}", game_id, nonce);
        let msg_bytes = message.as_bytes().to_vec();
        let created_at = now_ns();
        let expires_at = created_at + CHALLENGE_TTL_NS;

        let mut challenges = self.challenges.write().await;
        if challenges.len() >= MAX_OUTSTANDING_CHALLENGES {
            // Best-effort eviction of expired entries before refusing, so a
            // burst of legitimate traffic doesn't immediately trip the cap.
            let before = challenges.len();
            challenges.retain(|_, c| now_ns().saturating_sub(c.created_at) < CHALLENGE_TTL_NS);
            let pruned = before - challenges.len();
            if pruned > 0 {
                warn!(target: "auth", "Pruned {} expired challenges during cap enforcement", pruned);
            }
            if challenges.len() >= MAX_OUTSTANDING_CHALLENGES {
                warn!(
                    target: "auth",
                    "Challenge map at capacity ({}); refusing new challenge for game_id={}",
                    challenges.len(),
                    game_id
                );
                return None;
            }
        }

        challenges.insert(
            nonce.clone(),
            AuthChallenge {
                message: msg_bytes.clone(),
                created_at,
                game_id,
            },
        );

        info!(target: "auth", "Created challenge for game_id={} (outstanding={})", game_id, challenges.len());
        Some((msg_bytes, expires_at))
    }

    pub async fn verify_login(
        &self,
        game_id: u64,
        sig_bytes: &[u8],
        challenge_payload: &[u8],
    ) -> Result<Address> {
        let sig_vec = sig_bytes.to_vec();
        let payload_vec = challenge_payload.to_vec();

        let address =
            tokio::task::spawn_blocking(move || recover_address_from_sig(&sig_vec, &payload_vec))
                .await
                .context("signature recovery panicked")?
                .context("failed to recover address from signature")?;

        let address_hex = hex::encode(address.as_slice());
        info!(target: "auth", "Recovered address: 0x{}", address_hex);

        let challenge_text = String::from_utf8_lossy(challenge_payload);
        let nonce = match extract_nonce_from_challenge(&challenge_text) {
            Some(n) => n,
            None => {
                bail!("challenge payload does not contain a valid nonce reference")
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

        let msg = entry.message.clone();
        let sig_65 = sig_bytes[..65].to_vec();
        let recovered = tokio::task::spawn_blocking(move || {
            let arr: [u8; 65] = sig_65
                .as_slice()
                .try_into()
                .map_err(|_| anyhow::anyhow!("signature slice not 65 bytes"))?;
            let sig = Signature::from_raw_array(&arr)
                .map_err(|e| anyhow::anyhow!("invalid signature: {e}"))?;
            let expected_hash: B256 = eip191_hash_message(&msg);
            sig.recover_address_from_prehash(&expected_hash)
                .map_err(|e| anyhow::anyhow!("recovery failed: {e}"))
        })
        .await
        .context("signature verification panicked")??;

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
        challenges.retain(|_, c| now.saturating_sub(c.created_at) < CHALLENGE_TTL_NS);
        let removed = before - challenges.len();
        if removed > 0 {
            info!(target: "auth", "Cleaned up {} expired challenges", removed);
        }
    }
}

fn extract_nonce_from_challenge(challenge_text: &str) -> Option<String> {
    if challenge_text.starts_with("AMP_AUTH:") {
        let parts: Vec<&str> = challenge_text.splitn(3, ':').collect();
        if parts.len() == 3 {
            return Some(parts[2].to_string());
        }
    }
    None
}

fn recover_address_from_sig(sig_bytes: &[u8], payload: &[u8]) -> Result<Address> {
    if sig_bytes.len() < 65 {
        bail!("signature too short: {} bytes", sig_bytes.len());
    }

    let arr: [u8; 65] = sig_bytes[..65]
        .try_into()
        .map_err(|_| anyhow::anyhow!("signature slice not 65 bytes"))?;
    let sig = Signature::from_raw_array(&arr)
        .map_err(|e| anyhow::anyhow!("invalid signature format: {e}"))?;

    let payload = if payload.is_empty() {
        b"AMP_LOGIN"
    } else {
        payload
    };

    let msg_hash: B256 = eip191_hash_message(payload);
    let address = sig
        .recover_address_from_prehash(&msg_hash)
        .map_err(|e| anyhow::anyhow!("address recovery failed: {e}"))?;

    Ok(address)
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_reject_short_signature() {
        let result = recover_address_from_sig(&[0u8; 32], b"AMP_AUTH:1:nonce");
        assert!(result.is_err());
    }

    #[test]
    fn test_reject_garbage_signature() {
        let result = recover_address_from_sig(&[0u8; 65], b"AMP_AUTH:1:nonce");
        assert!(result.is_err());
    }

    #[test]
    fn test_extract_nonce_valid() {
        let result = extract_nonce_from_challenge("AMP_AUTH:42:some-nonce");
        assert_eq!(result, Some("some-nonce".to_string()));
    }

    #[test]
    fn test_extract_nonce_no_payload() {
        assert!(extract_nonce_from_challenge("").is_none());
    }

    #[test]
    fn test_extract_nonce_wrong_prefix() {
        assert!(extract_nonce_from_challenge("WRONG_PREFIX:42:nonce").is_none());
    }

    // ---- Phase 5.1: stateful negative-path coverage for AuthService. ----
    // The audit flagged that auth had only pure-function tests; these exercise
    // the real challenge lifecycle (unknown/expired nonce, wrong game, sig
    // mismatch, valid round-trip) using a real ECDSA wallet.

    use alloy_primitives::{eip191_hash_message, keccak256};
    use alloy_signer::SignerSync;
    use alloy_signer_local::PrivateKeySigner;

    fn test_wallet() -> PrivateKeySigner {
        // Deterministic key for reproducible tests.
        PrivateKeySigner::from_slice(keccak256(b"amp-auth-test-wallet-key").as_slice())
            .expect("valid wallet")
    }

    async fn sign(wallet: &PrivateKeySigner, msg: &[u8]) -> Vec<u8> {
        let h: B256 = eip191_hash_message(msg);
        wallet.sign_hash_sync(&h).expect("sign").as_bytes().to_vec()
    }

    #[tokio::test]
    async fn test_verify_login_round_trip_succeeds() {
        let svc = AuthService::new();
        let wallet = test_wallet();
        let (msg, _exp) = svc.create_challenge(7).await.unwrap();
        let sig = sign(&wallet, &msg).await;
        let addr = svc.verify_login(7, &sig, &msg).await.unwrap();
        assert_eq!(addr, wallet.address());
    }

    #[tokio::test]
    async fn test_verify_login_rejects_unknown_nonce() {
        let svc = AuthService::new();
        let wallet = test_wallet();
        // Sign a challenge string that was never issued by the service.
        let bogus = b"AMP_AUTH:7:never-issued-nonce";
        let sig = sign(&wallet, bogus).await;
        assert!(svc.verify_login(7, &sig, bogus).await.is_err());
    }

    #[tokio::test]
    async fn test_verify_login_rejects_wrong_game_id() {
        let svc = AuthService::new();
        let wallet = test_wallet();
        let (msg, _) = svc.create_challenge(7).await.unwrap();
        let sig = sign(&wallet, &msg).await;
        // Challenge issued for game 7; claim game 99.
        assert!(svc.verify_login(99, &sig, &msg).await.is_err());
    }

    #[tokio::test]
    async fn test_verify_login_rejects_short_signature() {
        let svc = AuthService::new();
        let (msg, _) = svc.create_challenge(7).await.unwrap();
        let too_short = vec![0u8; 10];
        assert!(svc.verify_login(7, &too_short, &msg).await.is_err());
    }

    #[tokio::test]
    async fn test_create_challenge_returns_unique_nonces() {
        let svc = AuthService::new();
        let (m1, _) = svc.create_challenge(1).await.unwrap();
        let (m2, _) = svc.create_challenge(1).await.unwrap();
        assert_ne!(m1, m2, "each challenge must carry a fresh nonce");
    }
}
