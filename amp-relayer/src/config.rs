use serde::{Deserialize, Serialize};
use std::collections::HashSet;
use std::env;

#[derive(Clone, Serialize, Deserialize)]
pub struct Config {
    pub rpc_addr: String,
    pub fuji_rpc_url: String,
    pub relayer_private_key: String,
    pub contract_settlement: String,
    pub contract_registry: String,
    pub db_path: String,
    pub max_retries: u32,
    pub base_retry_delay_ms: u64,
    pub gas_bump_percent: u64,
    /// Phase 2.3: number of concurrent settlement workers. Claims are
    /// serialized by `SettlementQueue::claim_lock`; the slow chain submission
    /// runs concurrently, aiding multi-game throughput (each game settles from
    /// a distinct custodial address). Default 1 = behavior-preserving.
    pub settlement_concurrency: usize,
    pub tls_cert_file: Option<String>,
    pub tls_key_file: Option<String>,
    pub api_keys: HashSet<String>,
}

impl std::fmt::Debug for Config {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Config")
            .field("rpc_addr", &self.rpc_addr)
            .field("fuji_rpc_url", &self.fuji_rpc_url)
            .field("relayer_private_key", &"[REDACTED]")
            .field("contract_settlement", &self.contract_settlement)
            .field("contract_registry", &self.contract_registry)
            .field("db_path", &self.db_path)
            .field("tls_cert_file", &self.tls_cert_file)
            .field("tls_key_file", &self.tls_key_file)
            .field(
                "api_keys",
                &format!("{} keys configured", self.api_keys.len()),
            )
            .finish()
    }
}

fn load_secret(env_var: &str, file_env_var: &str) -> anyhow::Result<String> {
    if let Ok(path) = env::var(file_env_var) {
        let contents = std::fs::read_to_string(&path)
            .map_err(|e| anyhow::anyhow!("failed to read key from {}: {}", path, e))?;
        return Ok(contents.trim().to_string());
    }
    env::var(env_var)
        .map_err(|e| anyhow::anyhow!("{} (or {}) not set: {}", env_var, file_env_var, e))
}

impl Config {
    pub fn from_env() -> anyhow::Result<Self> {
        let tls_cert_file = env::var("RELAYER_TLS_CERT_FILE").ok();
        let tls_key_file = env::var("RELAYER_TLS_KEY_FILE").ok();

        Ok(Self {
            rpc_addr: format!(
                "0.0.0.0:{}",
                env::var("RPC_PORT").unwrap_or_else(|_| "50052".to_string())
            ),
            fuji_rpc_url: env::var("FUJI_RPC_URL")
                .unwrap_or_else(|_| "http://localhost:8545".to_string()),
            relayer_private_key: load_secret("RELAYER_PRIVATE_KEY", "RELAYER_PRIVATE_KEY_FILE")?,
            contract_settlement: env::var("CONTRACT_SETTLEMENT")?,
            contract_registry: env::var("CONTRACT_REGISTRY")?,
            db_path: env::var("RELAYER_DB_PATH").unwrap_or_else(|_| "./relayer-data".to_string()),
            max_retries: env::var("RELAYER_MAX_RETRIES")
                .ok()
                .and_then(|v| v.parse().ok())
                .unwrap_or(5),
            base_retry_delay_ms: env::var("RELAYER_RETRY_DELAY_MS")
                .ok()
                .and_then(|v| v.parse().ok())
                .unwrap_or(1000),
            gas_bump_percent: env::var("RELAYER_GAS_BUMP_PERCENT")
                .ok()
                .and_then(|v| v.parse().ok())
                .unwrap_or(10),
            settlement_concurrency: env::var("RELAYER_SETTLEMENT_CONCURRENCY")
                .ok()
                .and_then(|v| v.parse().ok())
                .filter(|&v: &usize| (1..=64).contains(&v))
                .unwrap_or(1),
            tls_cert_file,
            tls_key_file,
            api_keys: load_api_keys(),
        })
    }
}

fn load_api_keys() -> HashSet<String> {
    let mut keys = HashSet::new();

    if let Ok(key) = env::var("RELAYER_API_KEY") {
        keys.insert(hash_api_key(&key));
    }

    if let Ok(path) = env::var("RELAYER_API_KEY_FILE")
        && let Ok(contents) = std::fs::read_to_string(&path)
    {
        for line in contents.lines() {
            let trimmed = line.trim();
            if !trimmed.is_empty() {
                keys.insert(hash_api_key(trimmed));
            }
        }
    }

    if let Ok(extra) = env::var("RELAYER_API_KEYS") {
        for key in extra.split(',') {
            let trimmed = key.trim();
            if !trimmed.is_empty() {
                keys.insert(hash_api_key(trimmed));
            }
        }
    }

    keys
}

pub fn hash_api_key(key: &str) -> String {
    use sha2::{Digest, Sha256};
    let mut hasher = Sha256::new();
    hasher.update(key.as_bytes());
    format!("{:x}", hasher.finalize())
}

/// Constant-time comparison of two strings (audit S15). Length is not treated
/// as a secret here because the inputs are SHA-256 hex digests (always 64 chars);
/// for differing lengths we still walk the shared prefix to avoid a length leak
/// on unequal-length inputs, then OR in any length difference.
pub fn ct_eq(a: &str, b: &str) -> bool {
    let ab = a.as_bytes();
    let bb = b.as_bytes();
    let mut diff: u8 = 0;
    let n = ab.len().min(bb.len());
    for i in 0..n {
        diff |= ab[i] ^ bb[i];
    }
    // Length difference folds in so unequal lengths never compare equal.
    diff |= (ab.len() as u8).wrapping_sub(bb.len() as u8);
    diff == 0
}

/// Constant-time API-key verification (audit S15). SHA-256 the candidate and
/// compare against EVERY stored hash with `ct_eq` without short-circuiting, so
/// the time does not depend on which (if any) key matched. The number of stored
/// keys is operator-controlled and small.
pub fn verify_api_key(candidate: &str, stored: &HashSet<String>) -> bool {
    let hashed = hash_api_key(candidate);
    let mut matched = false;
    for k in stored {
        if ct_eq(&hashed, k) {
            matched = true;
        }
    }
    matched
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn ct_eq_matches_and_rejects() {
        assert!(ct_eq("abc123", "abc123"));
        assert!(!ct_eq("abc123", "abc124"));
        assert!(!ct_eq("abc", "abcd"));
        assert!(!ct_eq("", "a"));
        assert!(ct_eq("", ""));
    }

    #[test]
    fn verify_api_key_round_trip_and_reject() {
        let mut set = HashSet::new();
        set.insert(hash_api_key("correct-horse-battery-staple"));
        assert!(verify_api_key("correct-horse-battery-staple", &set));
        assert!(!verify_api_key("wrong-key", &set));
        // Empty set never authenticates.
        assert!(!verify_api_key("anything", &HashSet::new()));
    }
}
