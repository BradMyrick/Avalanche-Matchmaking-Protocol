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
    ) -> anyhow::Result<U256> {
        let cached = {
            let cache = self.cache.lock().unwrap();
            cache.get(addr).copied()
        };

        let nonce = match cached {
            Some(n) => n,
            None => {
                let chain_nonce =
                    provider.get_transaction_count(*addr, None).await?.as_u64();
                let mut cache = self.cache.lock().unwrap();
                cache.insert(*addr, chain_nonce);
                chain_nonce
            }
        };

        {
            let mut cache = self.cache.lock().unwrap();
            if let Some(n) = cache.get_mut(addr) {
                *n += 1;
            }
        }

        Ok(U256::from(nonce))
    }

    pub fn reset(&self, addr: &Address) {
        let mut cache = self.cache.lock().unwrap();
        cache.remove(addr);
    }
}
