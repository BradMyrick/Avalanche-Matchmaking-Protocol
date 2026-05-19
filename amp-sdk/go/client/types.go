package client

import "capnproto.org/go/capnp/v3"

// MatchType enumerates the supported match formats.
type MatchType uint8

const (
	// MatchTypeTurnBased represents a turn-based match.
	MatchTypeTurnBased MatchType = iota
	// MatchTypeRealTime represents a real-time match.
	MatchTypeRealTime
)

// Region represents a geographic region for latency-aware matchmaking.
type Region uint8

const (
	// RegionNA represents North America.
	RegionNA Region = iota
	// RegionEU represents Europe.
	RegionEU
	// RegionSA represents South America.
	RegionSA
	// RegionAS represents Asia.
	RegionAS
)

// OutcomeType enumerates possible match outcomes.
type OutcomeType uint8

const (
	// OutcomeTypeUnknown represents an unknown or undetermined outcome.
	OutcomeTypeUnknown OutcomeType = iota
	// OutcomeTypeWin represents a win outcome.
	OutcomeTypeWin
	// OutcomeTypeDraw represents a draw outcome.
	OutcomeTypeDraw
	// OutcomeTypeVoid represents a voided match outcome.
	OutcomeTypeVoid
)

// PaymentInfo describes the payment details for a staked match.
type PaymentInfo struct {
	PayerWallet []byte
	FeeToken    []byte
	AuthSpend   uint64
}

// PlayerInfo contains player profile data used during matchmaking.
type PlayerInfo struct {
	PlayerID       []byte
	DisplayName    string
	PlayerWallet   []byte
	Elo            uint8
	Region         Region
	ProfileID      []byte
	TeamID         []byte
	LoadoutID      []byte
	MMRRating      float32
	MMRUncertainty float32
	IsReady        bool
	Preferences    []byte
}

// MatchRequest is the input for a matchmaking request.
type MatchRequest struct {
	GameID         []byte
	RulesType      string
	Stake          PaymentInfo
	PlayerInfo     PlayerInfo
	OptionalConfig []byte
	RuleSetID      []byte
	MatchType      MatchType
	QueuePriority  uint8
	CreationTime   uint64
	TimeoutMs      uint64
}

// MatchAssignment contains the details of a successfully created match.
type MatchAssignment struct {
	MatchID          []byte
	Opponents        []PlayerInfo
	GameConfig       MatchConfig
	AssignedVerifier []byte
	ServerAddress    string
	ServerPort       uint16
	ConnectionToken  []byte
	Region           Region
	RuleSetID        []byte
	MatchQuality     float32
	AssignmentTime   uint64
}

// MatchConfig holds the game-specific configuration for a match.
type MatchConfig struct {
	GameID      []byte
	MaxPlayers  uint8
	TimeLimitMs uint64
	CustomRules []byte
}

// Outcome represents the result of a completed match.
type Outcome struct {
	Type     OutcomeType
	Scores   []uint64
	Victor   uint8
	Metadata []byte
}

// VerifierResult contains the verifier's cryptographic signature over the match outcome.
type VerifierResult struct {
	Signature []byte
}

// OutcomeSubmission packages the match outcome with a transcript hash and player signature.
type OutcomeSubmission struct {
	MatchID    []byte
	Outcome    Outcome
	ReplayHash []byte
	Signature  []byte
}

// MatchSession represents an active match session obtained after matchmaking.
type MatchSession struct {
	MatchID []byte
	session capnp.Client
	client  *AMPClient
}

// Session returns the underlying Cap'n Proto client for the match session.
func (ms *MatchSession) Session() capnp.Client {
	return ms.session
}

// Release frees the Cap'n Proto capability held by this session.
func (ms *MatchSession) Release() {
	if ms.session.IsValid() {
		ms.session.Release()
	}
}

// GameEvent represents a single state-altering event during a match.
type GameEvent struct {
	EventID     uint64
	EventType   string
	EventData   []byte
	TriggeredBy []byte
	Timestamp   uint64
}
