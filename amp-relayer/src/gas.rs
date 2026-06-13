use ethers::prelude::*;

pub struct GasManager {
    pub bump_percent: u64,
    #[allow(dead_code)]
    pub bump_timeout_secs: u64,
}

impl GasManager {
    pub fn new(bump_percent: u64, bump_timeout_secs: u64) -> Self {
        Self {
            bump_percent,
            bump_timeout_secs,
        }
    }

    /// Estimate current EIP-1559 fees from the network.
    ///
    /// Returns `(max_fee_per_gas, max_priority_fee_per_gas)`.
    pub async fn estimate_eip1559_fees(
        &self,
        provider: &Provider<Http>,
    ) -> anyhow::Result<(U256, U256)> {
        let (max_fee, priority_fee) = provider.estimate_eip1559_fees(None).await?;
        Ok((max_fee, priority_fee))
    }

    /// Bump EIP-1559 fees on retry.
    ///
    /// Applies a cumulative bump of `bump_percent * retry_count` to both
    /// `max_fee_per_gas` and `max_priority_fee_per_gas`, ensuring replacement
    /// transactions will be picked up by miners/validators.
    ///
    /// For example, with `bump_percent = 10` and `retry_count = 2`:
    ///   new_max_fee = max_fee * (100 + 10*2) / 100 = max_fee * 1.20
    pub fn bump_fees(
        &self,
        network_max: U256,
        network_prio: U256,
        prev_max: U256,
        prev_prio: U256,
    ) -> (U256, U256) {
        let bump_factor = (100 + self.bump_percent) as u128;
        let bumped_prev_max = prev_max * U256::from(bump_factor) / U256::from(100);
        let bumped_prev_prio = prev_prio * U256::from(bump_factor) / U256::from(100);

        let new_max = if bumped_prev_max > network_max {
            bumped_prev_max
        } else {
            network_max
        };
        let new_prio = if bumped_prev_prio > network_prio {
            bumped_prev_prio
        } else {
            network_prio
        };

        (new_max, new_prio)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_bump_fees_no_retry() {
        let gm = GasManager::new(10, 30);
        let max = U256::from(1_000_000_000_000_000_000u128); // 1 ETH
        let prio = U256::from(1_000_000_000u128); // 1 gwei

        let (bumped_max, bumped_prio) = gm.bump_fees(max, prio, max, prio);
        let expected_max = U256::from(1_100_000_000_000_000_000u128); // +10%
        let expected_prio = U256::from(1_100_000_000u128); // +10%
        assert_eq!(bumped_max, expected_max);
        assert_eq!(bumped_prio, expected_prio);
    }

    #[test]
    fn test_bump_fees_cumulative() {
        let gm = GasManager::new(10, 30);
        let max = U256::from(1_000_000_000_000_000_000u128);
        let prio = U256::from(1_000_000_000u128);

        // First bump
        let (bump1_max, bump1_prio) = gm.bump_fees(max, prio, max, prio);
        // Second bump
        let (bump2_max, bump2_prio) = gm.bump_fees(max, prio, bump1_max, bump1_prio);

        let expected_max = U256::from(1_210_000_000_000_000_000u128); // 1.1 * 1.1 = 1.21
        let expected_prio = U256::from(1_210_000_000u128);
        assert_eq!(bump2_max, expected_max);
        assert_eq!(bump2_prio, expected_prio);
    }
}
