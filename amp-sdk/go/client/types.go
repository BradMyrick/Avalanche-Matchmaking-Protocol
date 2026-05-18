package client

import "capnproto.org/go/capnp/v3"

type MatchType uint8

const (
	MatchTypeTurnBased MatchType = iota
	MatchTypeRealTime
)

type Region uint8

const (
	RegionNA Region = iota
	RegionEU
	RegionSA
	RegionAS
)

type OutcomeType uint8

const (
	OutcomeTypeUnknown OutcomeType = iota
	OutcomeTypeWin
	OutcomeTypeDraw
	OutcomeTypeVoid
)

type PaymentInfo struct {
	PayerWallet []byte
	FeeToken    []byte
	AuthSpend   uint64
}

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

type MatchConfig struct {
	GameID      []byte
	MaxPlayers  uint8
	TimeLimitMs uint64
	CustomRules []byte
}

type Outcome struct {
	Type     OutcomeType
	Scores   []uint64
	Victor   uint8
	Metadata []byte
}

type VerifierResult struct {
	Signature []byte
}

type OutcomeSubmission struct {
	MatchID    []byte
	Outcome    Outcome
	ReplayHash []byte
	Signature  []byte
}

type MatchSession struct {
	MatchID []byte
	session capnp.Client
	client  *AMPClient
}

func (ms *MatchSession) Session() capnp.Client {
	return ms.session
}

func (ms *MatchSession) Release() {
	if ms.session.IsValid() {
		ms.session.Release()
	}
}

type GameEvent struct {
	EventID     uint64
	EventType   string
	EventData   []byte
	TriggeredBy []byte
	Timestamp   uint64
}
