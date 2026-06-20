use crate::error::RelayerError;
use alloy_primitives::{Address, U256};
use alloy_provider::Provider;
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

    /// Allocate the next nonce for `addr`.
    ///
    /// Phase 2.3 correctness fix: the previous implementation dropped the cache
    /// lock between the read and the write, so two concurrent callers could
    /// both observe the same cached nonce and both return it (a double-spend
    /// nonce). The cached (hot) path now atomically reads-and-increments under
    /// a single lock hold (see `reserve_cached`). The cold-start path still
    /// awaits the provider, but re-checks the cache under the lock before
    /// inserting so a racing cold start cannot clobber a higher nonce.
    pub async fn next_nonce<P: Provider>(
        &self,
        addr: &Address,
        provider: &P,
    ) -> Result<U256, RelayerError> {
        // Hot path: cached nonce present → atomically reserve it under one lock.
        if let Some(n) = self.reserve_cached(addr) {
            return Ok(U256::from(n));
        }

        // Cold path: no cached nonce for this address. Query the chain (await
        // outside the lock), then insert under the lock, keeping the larger of
        // the racing values if another task inserted meanwhile.
        let chain_nonce: u64 = provider
            .get_transaction_count(*addr)
            .pending()
            .await
            .map_err(RelayerError::Network)?;
        let allocated = {
            let mut cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
            let existing = cache.get(addr).copied();
            let next = match existing {
                Some(e) if e > chain_nonce => e,
                _ => chain_nonce,
            };
            cache.insert(*addr, next + 1);
            next
        };
        Ok(U256::from(allocated))
    }

    /// Atomically read-and-increment the cached nonce for `addr`. Returns
    /// `Some(allocated)` if a cached entry exists (hot path), `None` if the
    /// cold-start chain query is required. Held entirely under one lock so
    /// concurrent callers can never observe the same nonce (Phase 2.3).
    fn reserve_cached(&self, addr: &Address) -> Option<u64> {
        let mut cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
        if let Some(n) = cache.get_mut(addr) {
            let allocated = *n;
            *n = allocated + 1;
            Some(allocated)
        } else {
            None
        }
    }

    #[allow(dead_code)]
    pub fn reset(&self, addr: &Address) {
        let mut cache = self.cache.lock().unwrap_or_else(|e| e.into_inner());
        cache.remove(addr);
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    /// Phase 2.3 regression guard: concurrent reservations for the same address
    /// must never return a duplicate nonce. The previous read-then-write
    /// implementation failed this under contention.
    #[test]
    fn reserve_cached_never_duplicates_under_concurrency() {
        use std::sync::Arc;
        use std::thread;

        let nm = Arc::new(NonceManager::new());
        let addr = alloy_primitives::address!("0000000000000000000000000000000000001234");
        // Seed the cache so every caller takes the (concurrency-sensitive) hot path.
        {
            let mut cache = nm.cache.lock().unwrap();
            cache.insert(addr, 0);
        }

        let threads = 8;
        let per_thread = 1000;
        let mut handles = Vec::new();
        for _ in 0..threads {
            let nm = nm.clone();
            handles.push(thread::spawn(move || {
                let mut got = Vec::with_capacity(per_thread);
                for _ in 0..per_thread {
                    if let Some(n) = nm.reserve_cached(&addr) {
                        got.push(n);
                    }
                }
                got
            }));
        }
        let mut all: Vec<u64> = Vec::with_capacity(threads * per_thread);
        for h in handles {
            all.extend(h.join().unwrap());
        }
        all.sort_unstable();
        let dup_count = all.windows(2).filter(|w| w[0] == w[1]).count();
        assert_eq!(dup_count, 0, "duplicate nonces allocated under concurrency");
        // Every reservation must be unique and within [0, total).
        let mut expected: Vec<u64> = (0..all.len() as u64).collect();
        expected.sort_unstable();
        assert_eq!(all, expected, "nonce set must be a contiguous 0..n range");
    }
}
