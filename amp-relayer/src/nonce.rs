use crate::error::RelayerError;
use ethers::prelude::*;
use std::collections::HashMap;
use std::sync::Mutex;

pub struct NonceManager {
    cache: Mutex<HashMap<Address, u64>>,
}

impl NonceManager {
    pub fn new() -> Self {
        Self {
            cache: Mutex::new(HashMap::new()),
        }
    }

    pub async fn next_nonce(
        &self,
        addr: &Address,
        provider: &Provider<Http>,
    ) -> Result<U256, RelayerError> {
        let cached = {
            let cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
            cache.get(addr).copied()
        };

        let nonce = match cached {
            Some(n) => n,
            None => {
                let chain_nonce = provider.get_transaction_count(*addr, None).await?.as_u64();
                let mut cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
                let nonce = cache.get(addr).copied().unwrap_or(chain_nonce);
                cache.insert(*addr, nonce + 1);
                return Ok(U256::from(nonce));
            }
        };

        {
            let mut cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
            if let Some(n) = cache.get_mut(addr) {
                *n = nonce + 1;
            }
        }

        Ok(U256::from(nonce))
    }

    pub fn reset(&self, addr: &Address) {
        let mut cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
        cache.remove(addr);
    }
}
