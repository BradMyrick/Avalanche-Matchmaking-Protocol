package client

import "github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated"

type MatchSession struct {
	MatchID []byte
	Session generated.MatchSession
	client  *AMPClient
}

func (ms *MatchSession) Release() {
	if ms.Session.IsValid() {
		ms.Session.Release()
	}
}
