package player

import (
	"math"
	"testing"
)

func TestUpdateMMR(t *testing.T) {
	// A player rated 1500 with RD 200 plays an opponent rated 1400 with RD 30
	// For testing Glicko-2, using step-by-step paper example

	// Example 1: Player wins
	r, rd, vol, err := UpdateMMR(1500, 200, 0.06, 1400, 30, 1.0)
	if err != nil {
		t.Fatalf("win case failed: %v", err)
	}
	if r <= 1500 {
		t.Errorf("Expected rating to increase after a win, got %f", r)
	}
	if rd >= 200 {
		t.Errorf("Expected uncertainty (RD) to decrease after a match, got %f", rd)
	}

	// Example 2: Player loses against a much lower rated player
	r2, _, _, err := UpdateMMR(1500, 200, vol, 1200, 50, 0.0)
	if err != nil {
		t.Fatalf("loss case failed: %v", err)
	}
	if r2 >= 1500 {
		t.Errorf("Expected rating to decrease after losing to lower rating, got %f", r2)
	}

	// Example 3: Player draws
	r3, _, _, err := UpdateMMR(1500, 50, 0.06, 1500, 50, 0.5)
	if err != nil {
		t.Fatalf("draw case failed: %v", err)
	}
	if math.Abs(r3-1500) > 10.0 { // should be close to 1500
		t.Errorf("Expected rating to stay mostly same for draw with equal, got %f", r3)
	}
}

// TestUpdateMMR_RejectsNaNGuard verifies the NaN/Inf guard. Previously, a
// corrupt persisted profile would cause the Illinois-method solver to spin
// forever, hanging the caller (P8 in SECURITY_REVIEW.md).
func TestUpdateMMR_RejectsNaNGuard(t *testing.T) {
	cases := []struct {
		name                     string
		pr, prd, pv, or, ord, sc float64
	}{
		{"NaN rating", math.NaN(), 200, 0.06, 1400, 30, 1.0},
		{"Inf RD", 1500, math.Inf(1), 0.06, 1400, 30, 1.0},
		{"NaN score", 1500, 200, 0.06, 1400, 30, math.NaN()},
		{"zero volatility", 1500, 200, 0, 1400, 30, 1.0},
		{"negative volatility", 1500, 200, -0.1, 1400, 30, 1.0},
	}
	for _, c := range cases {
		t.Run(c.name, func(t *testing.T) {
			_, _, _, err := UpdateMMR(c.pr, c.prd, c.pv, c.or, c.ord, c.sc)
			if err == nil {
				t.Fatalf("expected error for %s, got nil", c.name)
			}
		})
	}
}
