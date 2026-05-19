use serde::{Deserialize, Serialize};
use std::env;

#[derive(Debug, Clone, Serialize, Deserialize)]
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
}

impl Config {
    pub fn from_env() -> anyhow::Result<Self> {
        Ok(Self {
            rpc_addr: format!(
                "0.0.0.0:{}",
                env::var("RPC_PORT").unwrap_or_else(|_| "50052".to_string())
            ),
            fuji_rpc_url: env::var("FUJI_RPC_URL")
                .unwrap_or_else(|_| "http://localhost:8545".to_string()),
            relayer_private_key: env::var("RELAYER_PRIVATE_KEY")?,
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
        })
    }
}
