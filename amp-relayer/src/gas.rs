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

    pub async fn estimate_eip1559_fees(
        &self,
        provider: &Provider<Http>,
    ) -> anyhow::Result<(U256, U256)> {
        let (max_fee, priority_fee) =
            provider.estimate_eip1559_fees(None).await?;
        Ok((max_fee, priority_fee))
    }

    #[allow(dead_code)]
    pub fn bump_fees(&self, max_fee: U256, priority_fee: U256) -> (U256, U256) {
        let bump_factor = (100 + self.bump_percent) as u128;
        let new_max = max_fee * U256::from(bump_factor) / U256::from(100);
        let new_priority =
            priority_fee * U256::from(bump_factor) / U256::from(100);
        (new_max, new_priority)
    }
}
