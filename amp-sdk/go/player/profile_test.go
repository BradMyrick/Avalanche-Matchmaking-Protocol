package player

import (
	"testing"
	"math"
)

func TestUpdateMMR(t *testing.T) {
	// A player rated 1500 with RD 200 plays an opponent rated 1400 with RD 30
	// For testing Glicko-2, using step-by-step paper example
	
	// Example 1: Player wins
	r, rd, vol := UpdateMMR(1500, 200, 0.06, 1400, 30, 1.0)
	
	if r <= 1500 {
		t.Errorf("Expected rating to increase after a win, got %f", r)
	}
	
	if rd >= 200 {
		t.Errorf("Expected uncertainty (RD) to decrease after a match, got %f", rd)
	}
	
	// Example 2: Player loses against a much lower rated player
	r2, _, _ := UpdateMMR(1500, 200, vol, 1200, 50, 0.0)
	if r2 >= 1500 {
		t.Errorf("Expected rating to decrease after losing to lower rating, got %f", r2)
	}
	
	// Example 3: Player draws
	r3, _, _ := UpdateMMR(1500, 50, 0.06, 1500, 50, 0.5)
	if math.Abs(r3 - 1500) > 10.0 { // should be close to 1500
		t.Errorf("Expected rating to stay mostly same for draw with equal, got %f", r3)
	}
}
