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
    pub gas_bump_timeout_secs: u64,
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
            gas_bump_timeout_secs: env::var("RELAYER_GAS_BUMP_TIMEOUT_SECS")
                .ok()
                .and_then(|v| v.parse().ok())
                .unwrap_or(30),
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
