use crate::error::RelayerError;
use ethers::prelude::*;
use tracing::info;

use super::RelayerState;

const DERIVATION_DOMAIN: &[u8] = b"AMP-custodial-v1";
const KECCAK256_BLOCK_SIZE: usize = 136; // keccak-256 rate in bytes (r = (1600 - 2*256) / 8)

/// HMAC construction using keccak256 as the underlying hash function.
///
/// This provides a proper PRF for HKDF instead of a raw hash, which prevents
/// length-extension attacks and provides the cryptographic guarantees expected
/// of a key derivation function.
fn hmac_keccak256(key: &[u8], message: &[u8]) -> [u8; 32] {
    // If key > block size, hash it first
    let mut key_padded = [0u8; KECCAK256_BLOCK_SIZE];
    if key.len() > KECCAK256_BLOCK_SIZE {
        key_padded[..32].copy_from_slice(&ethers::utils::keccak256(key));
    } else {
        key_padded[..key.len()].copy_from_slice(key);
    }

    // Inner hash: H((key ⊕ ipad) || message)
    let mut ipad = vec![0u8; KECCAK256_BLOCK_SIZE];
    let mut opad = vec![0u8; KECCAK256_BLOCK_SIZE];
    for i in 0..KECCAK256_BLOCK_SIZE {
        ipad[i] = key_padded[i] ^ 0x36;
        opad[i] = key_padded[i] ^ 0x5c;
    }

    let mut inner_input = Vec::with_capacity(KECCAK256_BLOCK_SIZE + message.len());
    inner_input.extend_from_slice(&ipad);
    inner_input.extend_from_slice(message);
    let inner_hash = ethers::utils::keccak256(&inner_input);

    // Outer hash: H((key ⊕ opad) || inner_hash)
    let mut outer_input = Vec::with_capacity(KECCAK256_BLOCK_SIZE + 32);
    outer_input.extend_from_slice(&opad);
    outer_input.extend_from_slice(&inner_hash);
    ethers::utils::keccak256(&outer_input)
}

/// HKDF-Extract: PRK = HMAC-Hash(salt, IKM)
///
/// Extracts a fixed-length pseudorandom key from the input keying material.
fn hkdf_extract(salt: &[u8], ikm: &[u8]) -> [u8; 32] {
    hmac_keccak256(salt, ikm)
}

/// HKDF-Expand: OKM = HMAC-Hash(PRK, T(i-1) || info || counter)
///
/// Expands the pseudorandom key into one or more output blocks.
fn hkdf_expand(prk: &[u8], info: &[u8], length: usize) -> Vec<u8> {
    let mut okm = Vec::with_capacity(length);
    let mut t = Vec::new();
    let mut counter: u8 = 0;

    while okm.len() < length {
        counter = counter
            .checked_add(1)
            .expect("HKDF-Expand counter overflow");

        let mut input = Vec::with_capacity(t.len() + info.len() + 1);
        input.extend_from_slice(&t);
        input.extend_from_slice(info);
        input.push(counter);

        t = hmac_keccak256(prk, &input).to_vec();
        okm.extend_from_slice(&t);
    }

    okm.truncate(length);
    okm
}

/// Derive a deterministic custodial signer for a given (purpose, gameId) pair.
///
/// Uses HKDF with HMAC-keccak256 instead of a raw keccak256 hash:
///
///   PRK = HMAC-keccak256(salt=DERIVATION_DOMAIN, ikm=master_key)
///   OKM = HKDF-Expand(PRK, info=purpose || 0x00 || gameId_u64_be, 32)
///
/// This provides:
/// - **Extraction-resistance**: The master key is not directly exposed in the output.
/// - **Domain separation**: Different (purpose, gameId) tuples yield unrelated keys.
/// - **Length-extension safety**: HMAC prevents length-extension attacks that
///   a raw hash composition would be vulnerable to.
/// - **Proper KDF properties**: HKDF is a proven, standardized construction
///   (RFC 5869, adapted here with keccak256 as the PRF).
pub fn derive_custodial_signer(
    master_key: &LocalWallet,
    purpose: &str,
    game_id: u64,
    chain_id: u64,
) -> LocalWallet {
    let master_bytes = master_key.signer().to_bytes();

    // Build info binding for HKDF-Expand: purpose || 0x00 || gameId (big-endian)
    let mut info = Vec::with_capacity(purpose.len() + 1 + 8);
    info.extend_from_slice(purpose.as_bytes());
    info.push(0x00);
    info.extend_from_slice(&game_id.to_be_bytes());

    // Extract: PRK = HMAC-keccak256(salt=DERIVATION_DOMAIN, ikm=master_key)
    let prk = hkdf_extract(DERIVATION_DOMAIN, &master_bytes);

    // Expand: OKM = HKDF-Expand(PRK, info, 32)
    let derived_key_bytes = hkdf_expand(&prk, &info, 32);
    let mut derived_key = [0u8; 32];
    derived_key.copy_from_slice(&derived_key_bytes);

    LocalWallet::from_bytes(&derived_key)
        .unwrap()
        .with_chain_id(chain_id)
}

/// Ensure the custodial wallet has sufficient gas to submit transactions.
///
/// If the balance falls below the threshold (0.05 AVAX), the master wallet
/// tops it up with 0.2 AVAX. Uses a DashSet to prevent concurrent top-ups
/// for the same address.
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

    #[test]
    fn test_hmac_keccak256_deterministic() {
        let key = b"test-key";
        let msg = b"test-message";
        let a = hmac_keccak256(key, msg);
        let b = hmac_keccak256(key, msg);
        assert_eq!(a, b);
    }

    #[test]
    fn test_hmac_keccak256_key_sensitive() {
        let msg = b"test-message";
        let a = hmac_keccak256(b"key-a", msg);
        let b = hmac_keccak256(b"key-b", msg);
        assert_ne!(a, b);
    }

    #[test]
    fn test_hkdf_expand_correct_length() {
        let prk = [0xab_u8; 32];
        let info = b"test-info";
        let okm = hkdf_expand(&prk, info, 64);
        assert_eq!(okm.len(), 64);
        // First 32 bytes != second 32 bytes (different counter values)
        assert_ne!(&okm[..32], &okm[32..]);
    }

    #[test]
    fn test_derived_key_differs_from_raw_hash() {
        // Verify the HKDF-derived key is NOT the same as the old raw keccak256 approach,
        // confirming the fix is substantive.
        let master = make_test_wallet();
        let master_bytes = master.signer().to_bytes();

        let mut preimage = Vec::new();
        preimage.extend_from_slice(DERIVATION_DOMAIN);
        preimage.push(0x00);
        preimage.extend_from_slice(b"settlement");
        preimage.push(0x00);
        preimage.extend_from_slice(&1u64.to_be_bytes());
        preimage.extend_from_slice(&master_bytes);
        let raw_hash = ethers::utils::keccak256(preimage);

        let derived = derive_custodial_signer(&master, "settlement", 1, 43114);
        let derived_bytes = derived.signer().to_bytes();

        assert_ne!(
            raw_hash.as_slice(),
            derived_bytes.as_slice(),
            "HKDF-derived key must differ from old raw keccak256 derivation"
        );
    }
}
