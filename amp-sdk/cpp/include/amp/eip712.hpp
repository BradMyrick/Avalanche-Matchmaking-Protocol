#pragma once
#include <cstdint>
#include <string>
#include <vector>

namespace amp {

/// Configuration for the EIP-712 domain separator. Set `verifying_contract`
/// to your deployment's AMPSettlement address (20 bytes) and `chain_id` to
/// your chain (43113 for Fuji, 43114 for mainnet) before calling
/// `compute_outcome_eip712_digest`.
struct Eip712Domain {
    std::vector<uint8_t> verifying_contract; // 20 bytes
    uint64_t chain_id = 43113;               // Fuji default; mainnet = 43114
};

/// Compute the canonical EIP-712 digest for an AMP match outcome.
///
/// Inputs:
///   - match_id:        decimal-string uint256 OR hex-string bytes32
///                      (matches the server's matchId parsing).
///   - outcome:         1 = WIN_A, 2 = WIN_B, 3 = DRAW, 4 = CANCELLED.
///   - transcript_hash: 32-byte commitment to the game transcript.
///   - domain:          chain_id + verifying_contract (your deployment).
///
/// Returns 32 bytes: keccak256(0x1901 || domainSeparator || structHash).
///
/// The returned digest is what the player signs with their secp256k1 wallet
/// (via SignOutcomeCallback in client.hpp) and what the AMP server verifies
/// against their authenticated player_id. The digest is byte-identical with
/// the Rust server's `compute_outcome_eip712_digest` and the C# SDK's
/// `OutcomeEip712.ComputeDigest` — pinned by the cross-language KAT vector
/// `2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c` for
/// (matchId="1", outcome=1, transcript=zero[32], chain=43113, contract=0).
std::vector<uint8_t> compute_outcome_eip712_digest(
    const std::string& match_id,
    uint8_t outcome,
    const std::vector<uint8_t>& transcript_hash,
    const Eip712Domain& domain);

} // namespace amp
