use crate::error::RelayerError;
use ethers::prelude::*;
use tracing::info;

use super::RelayerState;

const DERIVATION_DOMAIN: &[u8] = b"AMP-custodial-v1";

pub fn derive_custodial_signer(
    master_key: &LocalWallet,
    purpose: &str,
    game_id: u64,
    chain_id: u64,
) -> LocalWallet {
    let master_bytes = master_key.signer().to_bytes();

    let mut preimage = Vec::with_capacity(
        DERIVATION_DOMAIN.len() + 1 + purpose.len() + 1 + 8 + master_bytes.len(),
    );
    preimage.extend_from_slice(DERIVATION_DOMAIN);
    preimage.push(0x00);
    preimage.extend_from_slice(purpose.as_bytes());
    preimage.push(0x00);
    preimage.extend_from_slice(&game_id.to_be_bytes());
    preimage.extend_from_slice(&master_bytes);

    let derived_key = ethers::utils::keccak256(preimage);
    LocalWallet::from_bytes(&derived_key)
        .unwrap()
        .with_chain_id(chain_id)
}

pub async fn ensure_gas(custodial_addr: Address, state: &RelayerState) -> Result<(), RelayerError> {
    if state.pending_topups.contains(&custodial_addr) {
        return Ok(());
    }

    let balance = state
        .master_client
        .provider()
        .get_balance(custodial_addr, None)
        .await?;
    let threshold =
        ethers::utils::parse_ether(0.05).map_err(|e| RelayerError::Transaction(e.to_string()))?;

    if balance < threshold {
        state.pending_topups.insert(custodial_addr);
        let topup = ethers::utils::parse_ether(0.2)
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;
        info!(
            "Custodial wallet {} low on gas ({:?}). Topping up...",
            custodial_addr, balance
        );
        let tx = TransactionRequest::new().to(custodial_addr).value(topup);

        state
            .master_client
            .send_transaction(tx, None)
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?
            .await
            .map_err(|e| RelayerError::Transaction(e.to_string()))?;
        info!("Top-up successful for {}", custodial_addr);
        state.pending_topups.remove(&custodial_addr);
    }

    Ok(())
}

#[cfg(test)]
mod tests {
    use super::*;

    fn make_test_wallet() -> LocalWallet {
        "ac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80"
            .parse()
            .unwrap()
    }

    #[test]
    fn test_deterministic_derivation() {
        let master = make_test_wallet();
        let a = derive_custodial_signer(&master, "settlement", 1, 43114);
        let b = derive_custodial_signer(&master, "settlement", 1, 43114);
        assert_eq!(a.address(), b.address());
    }

    #[test]
    fn test_different_games_different_keys() {
        let master = make_test_wallet();
        let a = derive_custodial_signer(&master, "settlement", 1, 43114);
        let b = derive_custodial_signer(&master, "settlement", 2, 43114);
        assert_ne!(a.address(), b.address());
    }

    #[test]
    fn test_different_purposes_different_keys() {
        let master = make_test_wallet();
        let a = derive_custodial_signer(&master, "settlement", 1, 43114);
        let b = derive_custodial_signer(&master, "gas-fund", 1, 43114);
        assert_ne!(a.address(), b.address());
    }

    #[test]
    fn test_derived_key_is_valid() {
        let master = make_test_wallet();
        let derived = derive_custodial_signer(&master, "settlement", 42, 43114);
        assert_ne!(derived.address(), master.address());
        assert_eq!(derived.chain_id(), 43114);
    }
}
