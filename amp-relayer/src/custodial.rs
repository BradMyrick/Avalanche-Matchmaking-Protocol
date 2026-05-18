use ethers::prelude::*;
use tracing::info;

use super::RelayerState;

pub fn derive_custodial_signer(
    master_key: &LocalWallet,
    game_id: u64,
    chain_id: u64,
) -> LocalWallet {
    let mut bytes = [0u8; 32];
    bytes.copy_from_slice(&master_key.signer().to_bytes());

    let mut data = Vec::with_capacity(40);
    data.extend_from_slice(&bytes);
    data.extend_from_slice(&game_id.to_be_bytes());

    let derived_key = ethers::utils::keccak256(data);
    LocalWallet::from_bytes(&derived_key)
        .unwrap()
        .with_chain_id(chain_id)
}

pub async fn ensure_gas(
    custodial_addr: Address,
    state: &RelayerState,
) -> anyhow::Result<()> {
    let balance = state
        .master_client
        .provider()
        .get_balance(custodial_addr, None)
        .await?;
    let threshold = ethers::utils::parse_ether(0.05)?;

    if balance < threshold {
        let topup = ethers::utils::parse_ether(0.2)?;
        info!(
            "Custodial wallet {} low on gas ({:?}). Topping up...",
            custodial_addr, balance
        );

        let tx = TransactionRequest::new().to(custodial_addr).value(topup);

        state
            .master_client
            .send_transaction(tx, None)
            .await?
            .await?;
        info!("Top-up successful for {}", custodial_addr);
    }

    Ok(())
}
