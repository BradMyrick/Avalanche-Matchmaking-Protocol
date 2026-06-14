package player

import (
	"errors"
	"fmt"
	"math"

	"capnproto.org/go/capnp/v3"
	"github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated"
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
	// MaxGlickoIterations caps the Illinois-method root finder. Without this,
	// NaN/Inf inputs (e.g. from a corrupt persisted profile) cause the loop to
	// spin forever, hanging the caller. 100 is far above the convergence
	// threshold for valid inputs (~5-10 iterations typically).
	MaxGlickoIterations = 100
)

// ErrInvalidMMR is returned by UpdateMMR when an input is NaN/Inf or the
// solver fails to converge. Callers should treat this as a corruption signal
// and refuse to persist the result.
var ErrInvalidMMR = errors.New("glicko-2: invalid MMR input or non-convergence")

// UpdateMMR computes the new Glicko-2 rating after a match.
// score is 1.0 (win), 0.5 (draw), or 0.0 (loss).
//
// Returns ErrInvalidMMR (wrapping the original result tuple as zeros) if any
// input is NaN/Inf or the iterative solver fails to converge within
// MaxGlickoIterations. This prevents a single corrupt profile from hanging
// the caller — see SECURITY_REVIEW.md P8.
func UpdateMMR(playerRating, playerRD, playerVol, oppRating, oppRD, score float64) (float64, float64, float64, error) {
	// Reject NaN/Inf inputs up front.
	for _, v := range []float64{playerRating, playerRD, playerVol, oppRating, oppRD, score} {
		if math.IsNaN(v) || math.IsInf(v, 0) {
			return 1500, 350, 0.06, fmt.Errorf("%w: NaN/Inf input %v", ErrInvalidMMR, v)
		}
	}
	if playerVol <= 0 {
		return 1500, 350, 0.06, fmt.Errorf("%w: non-positive volatility %v", ErrInvalidMMR, playerVol)
	}

	// Convert to Glicko-2 scale
	mu := (playerRating - 1500) / Scale
	phi := playerRD / Scale

	muOpp := (oppRating - 1500) / Scale
	phiOpp := oppRD / Scale

	g := 1.0 / math.Sqrt(1.0+3.0*phiOpp*phiOpp/(math.Pi*math.Pi))
	e := 1.0 / (1.0 + math.Exp(-g*(mu-muOpp)))
	v := 1.0 / (g * g * e * (1.0 - e))

	denom := phi*phi + v
	if denom == 0 {
		return playerRating, playerRD, playerVol, fmt.Errorf("%w: zero denominator", ErrInvalidMMR)
	}
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
		// Bound the search to avoid spinning forever on pathological inputs.
		for k <= MaxGlickoIterations && f(a-k*Tau) < 0 {
			k += 1.0
		}
		if k > MaxGlickoIterations {
			return playerRating, playerRD, playerVol, fmt.Errorf("%w: bracketing failed", ErrInvalidMMR)
		}
		B = a - k*Tau
	}

	fa, fb := f(A), f(B)
	// Capped iteration loop — previously unbounded.
	for iter := 0; iter < MaxGlickoIterations && math.Abs(B-A) > 0.000001; iter++ {
		denomF := fb - fa
		if math.Abs(denomF) < 1e-300 {
			// Illinois method stalls when fa == fb; abort rather than divide by 0.
			return playerRating, playerRD, playerVol, fmt.Errorf("%w: solver stalled", ErrInvalidMMR)
		}
		C := A + (A-B)*fa/denomF
		fc := f(C)
		if fc*fb <= 0 {
			A, fa = B, fb
		} else {
			fa /= 2.0
		}
		B, fb = C, fc
	}
	if math.IsNaN(A) || math.IsNaN(B) {
		return playerRating, playerRD, playerVol, fmt.Errorf("%w: solver diverged to NaN", ErrInvalidMMR)
	}

	newVol := math.Exp(A / 2.0)
	phiStar := math.Sqrt(phi*phi + newVol*newVol)
	newPhi := 1.0 / math.Sqrt(1.0/(phiStar*phiStar)+1.0/v)
	newMu := mu + newPhi*newPhi*g*(score-e)

	return newMu*Scale + 1500, newPhi * Scale, newVol, nil
}
