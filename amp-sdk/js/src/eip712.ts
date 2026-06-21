/**
 * EIP-712 typed-data digest for AMP match outcomes.
 *
 * Mirrors `compute_outcome_eip712_digest` in amp-server/src/main.rs and the
 * helpers in amp-sdk/{csharp,python}/. The digest MUST be byte-identical
 * across all languages; the cross-language KAT
 * (`test_outcome_digest_known_vector_cross_lang` in amp-server) fails if any
 * SDK diverges.
 *
 * Known-answer test vector (chain_id=43113, verifying_contract=0x000...000):
 *   matchId="1", outcome=1, transcript_hash=zero[32] =>
 *   2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c
 */

import { TypedDataEncoder, keccak256, toUtf8Bytes, getBytes } from "ethers";

/** Default chain ID for the AMP settlement contract (Fuji testnet). */
export const DEFAULT_CHAIN_ID = 43113n;

/** Default verifying contract address (zero — override at app startup). */
export const DEFAULT_VERIFYING_CONTRACT = "0x0000000000000000000000000000000000000000";

/** Canonical domain name & version, fixed by the on-chain verifier. */
export const DOMAIN_NAME = "AMPSettlement";
export const DOMAIN_VERSION = "1";

/** Default EIP-712 domain parameters. Override at app startup. */
export interface Eip712DomainParams {
  chainId: bigint;
  verifyingContract: string;
}

/**
 * Compute the canonical EIP-712 digest over (matchId, outcome, transcriptHash).
 * Sign this digest with the player's wallet; the server will recover the
 * address and require it to match the authenticated player.
 *
 * @param matchId Match identifier as a UTF-8 string (numeric or UUID-like).
 *                Numeric strings are encoded as uint256 directly; others are
 *                keccak256-hashed to fit uint256.
 * @param outcome Victor index, 1..=4 (server invariant).
 * @param transcriptHash 32-byte keccak256 of the match transcript.
 * @param domain Override chain ID / verifying contract to match deployment.
 */
export function computeOutcomeEip712Digest(
  matchId: string,
  outcome: number,
  transcriptHash: Uint8Array,
  domain: Eip712DomainParams = {
    chainId: DEFAULT_CHAIN_ID,
    verifyingContract: DEFAULT_VERIFYING_CONTRACT,
  },
): Uint8Array {
  if (transcriptHash.length !== 32) {
    throw new Error(`transcript_hash must be exactly 32 bytes, got ${transcriptHash.length}`);
  }
  if (outcome < 1 || outcome > 4) {
    throw new Error(`outcome must be 1..=4 (server invariant), got ${outcome}`);
  }

  // matchId → uint256 (parse decimal, else keccak256-hash UTF-8 bytes).
  const matchIdU256 = tryParseUint256(matchId) ?? BigInt(keccak256(toUtf8Bytes(matchId)));

  const types = {
    AsyncResult: [
      { name: "matchId", type: "uint256" },
      { name: "outcome", type: "uint8" },
      { name: "transcriptHash", type: "bytes32" },
    ],
  };
  const value = {
    matchId: matchIdU256,
    outcome,
    transcriptHash: transcriptHash,
  };
  const domainObj = {
    name: DOMAIN_NAME,
    version: DOMAIN_VERSION,
    chainId: domain.chainId,
    verifyingContract: domain.verifyingContract,
  };
  // TypedDataEncoder.hash returns the canonical EIP-712 digest
  // (0x1901 || domainSeparator || structHash, keccak256'd).
  return getBytes(TypedDataEncoder.hash(domainObj, types, value));
}

function tryParseUint256(s: string): bigint | null {
  try {
    const v = BigInt(s);
    if (v < 0n || v > 2n ** 256n - 1n) return null;
    return v;
  } catch {
    return null;
  }
}
