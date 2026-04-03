package player

import (
	"math"

	"github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated"
	"capnproto.org/go/capnp/v3"
)

// ProfileBuilder constructs a PlayerProfile Cap'n Proto message.
type ProfileBuilder struct {
	msg     *capnp.Message
	profile generated.PlayerProfile
}

// NewProfileBuilder creates a new struct wrapper for PlayerProfile.
func NewProfileBuilder() (*ProfileBuilder, error) {
	msg, seg, err := capnp.NewMessage(capnp.SingleSegment(nil))
	if err != nil {
		return nil, err
	}

	profile, err := generated.NewPlayerProfile(seg)
	if err != nil {
		return nil, err
	}

	// Set default MMR values
	mmr, _ := profile.NewGlobalMMR()
	mmr.SetRating(1200)
	mmr.SetUncertainty(350)
	mmr.SetVolatility(0.06)

	return &ProfileBuilder{
		msg:     msg,
		profile: profile,
	}, nil
}

// WithID sets the player ID.
func (b *ProfileBuilder) WithID(id string) *ProfileBuilder {
	_ = b.profile.SetPlayerId([]byte(id))
	return b
}

// WithDisplayName sets the player's display name.
func (b *ProfileBuilder) WithDisplayName(name string) *ProfileBuilder {
	_ = b.profile.SetDisplayName(name)
	return b
}

// Build finalizes and returns the built PlayerProfile.
func (b *ProfileBuilder) Build() generated.PlayerProfile {
	return b.profile
}

// -------------------------------------------------------------------------
// Glicko-2 Local Implementation
// -------------------------------------------------------------------------

const (
	Tau   = 0.5
	Scale = 173.7178
)

// UpdateMMR computes the new Glicko-2 rating after a match.
// score is 1.0 (win), 0.5 (draw), or 0.0 (loss).
func UpdateMMR(playerRating, playerRD, playerVol, oppRating, oppRD, score float64) (float64, float64, float64) {
	// Convert to Glicko-2 scale
	mu := (playerRating - 1500) / Scale
	phi := playerRD / Scale
	
	muOpp := (oppRating - 1500) / Scale
	phiOpp := oppRD / Scale

	g := 1.0 / math.Sqrt(1.0+3.0*phiOpp*phiOpp/(math.Pi*math.Pi))
	e := 1.0 / (1.0 + math.Exp(-g*(mu-muOpp)))
	v := 1.0 / (g * g * e * (1.0 - e))
	
	denom := phi*phi + v
	delta := v * g * (score - e)

	a := math.Log(playerVol * playerVol)
	f := func(x float64) float64 {
		ex := math.Exp(x)
		return (ex*(delta*delta-phi*phi-v-ex))/(2.0*denom*denom) - (x-a)/(Tau*Tau)
	}

	A := a
	var B float64
	if delta*delta > phi*phi+v {
		B = math.Log(delta*delta - phi*phi - v)
	} else {
		k := 1.0
		for f(a-k*Tau) < 0 {
			k += 1.0
		}
		B = a - k*Tau
	}

	fa, fb := f(A), f(B)
	for math.Abs(B-A) > 0.000001 {
		C := A + (A-B)*fa/(fb-fa)
		fc := f(C)
		if fc*fb <= 0 {
			A, fa = B, fb
		} else {
			fa /= 2.0
		}
		B, fb = C, fc
	}

	newVol := math.Exp(A / 2.0)
	phiStar := math.Sqrt(phi*phi + newVol*newVol)
	newPhi := 1.0 / math.Sqrt(1.0/(phiStar*phiStar)+1.0/v)
	newMu := mu + newPhi*newPhi*g*(score-e)

	return newMu*Scale + 1500, newPhi * Scale, newVol
}
