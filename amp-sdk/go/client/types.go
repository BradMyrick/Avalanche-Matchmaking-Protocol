package client

import "github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated"

// MatchSession holds a match capability plus a defensively-copied MatchID.
// The struct is safe to retain after the parent AMPClient is closed because
// MatchID is owned (not aliased to a capnp buffer).
type MatchSession struct {
	MatchID []byte
	Session generated.MatchSession
	client  *AMPClient
}

// Release frees the underlying capnp capability. Idempotent.
func (ms *MatchSession) Release() {
	if ms.Session.IsValid() {
		ms.Session.Release()
	}
}
