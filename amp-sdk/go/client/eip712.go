package client

import (
	"fmt"
	"math/big"

	"golang.org/x/crypto/sha3"
)

// ComputeOutcomeEip712Digest computes the canonical EIP-712 digest over
// (matchId, outcome, transcriptHash) that the AMP server verifies in
// submitOutcome and the verifier signs.
//
// Byte-identical to AMP-Server::compute_outcome_eip712_digest and the
// Rust/C#/Python/JS helpers. The shared known-answer vector is enforced in
// the test below and across the other SDKs.
//
//   - matchId: if it parses as a decimal uint256 it is used directly; otherwise
//     it is keccak256(utf8).
//   - outcome: the victor index (1..=4).
//   - transcriptHash: the 32-byte keccak256 transcript/replay hash.
//   - chainID: the EIP-712 domain chain id (e.g. 43113 for Fuji).
//   - verifyingContract: the 20-byte AMPSettlement contract address.
func ComputeOutcomeEip712Digest(matchID string, outcome uint8, transcriptHash []byte, chainID uint64, verifyingContract []byte) ([32]byte, error) {
	var out [32]byte
	if len(transcriptHash) != 32 {
		return out, fmt.Errorf("transcript_hash must be exactly 32 bytes, got %d", len(transcriptHash))
	}
	if len(verifyingContract) != 20 {
		return out, fmt.Errorf("verifying_contract must be exactly 20 bytes, got %d", len(verifyingContract))
	}

	asyncResultTypehash := keccak256([]byte("AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)"))

	// matchId → uint256 (decimal parse) else keccak256(utf8).
	var matchIDWord [32]byte
	if v, ok := new(big.Int).SetString(matchID, 10); ok {
		v.FillBytes(matchIDWord[:])
	} else {
		matchIDWord = keccak256([]byte(matchID))
	}

	var outcomeWord [32]byte
	outcomeWord[31] = outcome

	// structHash = keccak256(abi.encode(typeHash, matchId, outcome, transcriptHash))
	structInput := make([]byte, 0, 128)
	structInput = append(structInput, asyncResultTypehash[:]...)
	structInput = append(structInput, matchIDWord[:]...)
	structInput = append(structInput, outcomeWord[:]...)
	structInput = append(structInput, transcriptHash...)
	structHash := keccak256(structInput)

	domainTypehash := keccak256([]byte("EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"))
	nameHash := keccak256([]byte("AMPSettlement"))
	versionHash := keccak256([]byte("1"))

	var chainWord [32]byte
	new(big.Int).SetUint64(chainID).FillBytes(chainWord[:])

	var addrWord [32]byte
	copy(addrWord[12:], verifyingContract)

	domainInput := make([]byte, 0, 160)
	domainInput = append(domainInput, domainTypehash[:]...)
	domainInput = append(domainInput, nameHash[:]...)
	domainInput = append(domainInput, versionHash[:]...)
	domainInput = append(domainInput, chainWord[:]...)
	domainInput = append(domainInput, addrWord[:]...)
	domainSeparator := keccak256(domainInput)

	digestInput := append([]byte{0x19, 0x01}, domainSeparator[:]...)
	digestInput = append(digestInput, structHash[:]...)
	return keccak256(digestInput), nil
}

func keccak256(b []byte) [32]byte {
	h := sha3.NewLegacyKeccak256()
	h.Write(b)
	var out [32]byte
	h.Sum(out[:0])
	return out
}
