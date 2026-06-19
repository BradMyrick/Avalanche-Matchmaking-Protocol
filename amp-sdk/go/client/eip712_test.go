package client

import (
	"encoding/hex"
	"testing"
)

// Cross-language KAT: must match the Rust/C#/Python/JS digest helpers and the
// server's compute_outcome_eip712_digest
// (amp-server/src/main.rs::test_outcome_digest_known_vector_cross_lang).
func TestComputeOutcomeEip712DigestKnownVector(t *testing.T) {
	var zeroAddr [20]byte
	digest, err := ComputeOutcomeEip712Digest("1", 1, make([]byte, 32), 43113, zeroAddr[:])
	if err != nil {
		t.Fatalf("digest error: %v", err)
	}
	got := hex.EncodeToString(digest[:])
	want := "2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c"
	if got != want {
		t.Fatalf("digest mismatch:\n got %s\nwant %s", got, want)
	}
}

func TestComputeOutcomeEip712DigestRejectsShortTranscript(t *testing.T) {
	var zeroAddr [20]byte
	if _, err := ComputeOutcomeEip712Digest("1", 1, make([]byte, 16), 43113, zeroAddr[:]); err == nil {
		t.Fatal("expected error for 16-byte transcript hash")
	}
}
