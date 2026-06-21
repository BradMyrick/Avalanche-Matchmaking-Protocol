use alloy_primitives::U256;
use alloy_provider::Provider;

pub struct GasManager {
    pub bump_percent: u64,
}

impl GasManager {
    pub fn new(bump_percent: u64) -> Self {
        Self { bump_percent }
    }

    /// Estimate current EIP-1559 fees from the network.
    ///
    /// Returns `(max_fee_per_gas, max_priority_fee_per_gas)`.
    pub async fn estimate_eip1559_fees<P: Provider>(
        &self,
        provider: &P,
    ) -> anyhow::Result<(U256, U256)> {
        let est = provider.estimate_eip1559_fees().await?;
        Ok((
            U256::from(est.max_fee_per_gas),
            U256::from(est.max_priority_fee_per_gas),
        ))
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
        let bumped_prev_max = prev_max.saturating_mul(U256::from(bump_factor)) / U256::from(100);
        let bumped_prev_prio = prev_prio.saturating_mul(U256::from(bump_factor)) / U256::from(100);

        let new_max = bumped_prev_max.max(network_max);
        let new_prio = bumped_prev_prio.max(network_prio);

        (new_max, new_prio)
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn test_bump_fees_no_retry() {
        let gm = GasManager::new(10);
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
        let gm = GasManager::new(10);
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
