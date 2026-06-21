@0xd5b014276707324c;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("match_capnp");
using GameCore = import "game_core.capnp";

# Core Types
using TimeStamp     = UInt64;  # Nanoseconds since epoch
using Address       = Data;    # 20-byte Ethereum address
using AmpId         = Data;    # e.g., UUID or specialized ID
using Signature     = Data;    # 65-byte secp256k1+r+s+v signature. For EIP-191
                               # auth signatures (hash_message), use 27/28 for v.
                               # For EIP-712 outcome signatures, the 0x1901
                               # prefix is built into the digest; sign the raw
                               # 32-byte digest and serialize as r||s||v with
                               # v in {27, 28}.

enum MatchType {
    turnBased   @0; 
    realTime    @1; 
}

enum Region {
    na  @0; 
    eu  @1; 
    sa  @2; 
    as  @3; 
}

enum Elo {
    unranked    @0;
    bronze      @1;   # ~ 0–1199
    silver      @2;   # ~ 1200–1499
    gold        @3;   # ~ 1500–1799
    platinum    @4;   # ~ 1800–1999
    diamond     @5;   # ~ 2000–2199
    master      @6;   # ~ 2200–2399
    grandmaster @7;   # ~ 2400+
}

struct PaymentInfo {
    payerWallet     @0 :Address;
    feeToken        @1 :Address; # usage: ERC20 address (0x0 for native AVAX?)
    authSpend       @2 :UInt64;  # amount authorized/staked
}

struct PlayerInfo {
    playerId        @0 :AmpId;
    displayName     @1 :Text;
    playerWallet    @2 :Address; # The wallet that will sign verify moves
    elo             @3 :Elo;
    region          @4 :Region;
    # Enhanced fields for FlexMatch-style system
    profileId       @5 :AmpId;       # Reference to full PlayerProfile
    teamId          @6 :AmpId;       # Current party/team ID (if any)
    loadoutId       @7 :AmpId;       # Active loadout ID
    mmrRating       @8 :Float32;     # Current MMR (Match Making Rating)
    mmrUncertainty  @9 :Float32;     # Rating confidence (TrueSkill/Glicko)
    isReady         @10 :Bool;       # Ready status for match
    preferences     @11 :Data;       # Serialized player preferences
}

# Match Request
struct GameMatchRequest {
    gameId          @0 :AmpId;     # e.g. "chess-v1"
    rulesType       @1 :Text;      # Variant tag, e.g. "standard", "blitz"
    stake           @2 :PaymentInfo;
    playerInfo      @3 :PlayerInfo;
    optionalConfig  @4 :Data;      # Game-specific config blob (serialized Cap'n Proto)
    # Enhanced fields for FlexMatch-style system
    ruleSetId       @5 :AmpId;     # Reference to MatchmakingRuleSet
    matchType       @6 :MatchType; # turnBased or realTime
    queuePriority   @7 :UInt8;     # Queue priority (0-255)
    creationTime    @8 :TimeStamp; # When request was created
    timeoutMs       @9 :UInt64;    # Max wait time in milliseconds
}

# Match Assignment
struct MatchAssignment {
    matchId         @0 :AmpId;     # The authoritative Match ID (e.g. on-chain UUID)
    opponents       @1 :List(PlayerInfo);
    gameConfig      @2 :GameCore.MatchConfig;      # Snapshot of agreed configuration
    assignedVerifier @3 :Address;  # The public key/address of the Verifier node
    # Enhanced fields for FlexMatch-style system
    serverAddress   @4 :Text;      # Game server IP/URL
    serverPort      @5 :UInt16;    # Game server port
    connectionToken @6 :Data;      # Authentication token for game server
    region          @7 :Region;    # Assigned region for match
    ruleSetId       @8 :AmpId;     # Rule set used for matchmaking
    matchQuality    @9 :Float32;   # Match quality score (0.0-1.0)
    assignmentTime  @10 :TimeStamp;# When match was assigned
    # Capability for the match session will be defined in service.capnp
    # session         @11 :MatchSession; 
}

# Outcome
enum OutcomeType {
    unknown     @0;
    win         @1;
    draw        @2;
    void        @3; # Cancelled/Refunded
}

struct Outcome {
    type            @0 :OutcomeType;
    scores          @1 :List(UInt64); # e.g. [1, 0] or points
    victor          @2 :UInt8;        # Index in `opponents` list, meaningful if type is `win`
    metadata        @3 :Data;         # Arbitrary game-end data (stats, etc)
}

struct OutcomeSubmission {
    matchId         @0 :AmpId;
    outcome         @1 :Outcome;
    replayHash      @2 :Data;         # Hash of the full replay/transcript
    signature       @3 :Signature;    # Signature of (matchId, outcome, replayHash) by the submitter
}

# Verifier Logic Types
struct MatchTranscript {
    matchId         @0 :AmpId;
    events          @1 :List(GameCore.GameEvent);   # Serialized GameEvents
    finalStateHash  @2 :Data;
}

# Matchmaking Queue Statistics
struct QueueStatistics {
    gameId          @0 :AmpId;
    ruleSetId       @1 :AmpId;
    playersInQueue  @2 :UInt32;
    estimatedWaitMs @3 :UInt64;
    avgQueueTimeMs  @4 :UInt64;
    matchesPerHour  @5 :UInt32;
    successRate     @6 :Float32;    # % of successful matches
    avgMatchQuality @7 :Float32;    # Average match quality score
    regionBreakdown @8 :List(RegionStats);
}

struct RegionStats {
    region          @0 :Region;
    playerCount     @1 :UInt32;
    avgLatencyMs    @2 :UInt32;
}

# Time Range for queries
struct TimeRange {
    startTime       @0 :TimeStamp;
    endTime         @1 :TimeStamp;
}

# Team/Party Structure
struct TeamInfo {
    teamId          @0 :AmpId;
    name            @1 :Text;
    members         @2 :List(PlayerInfo);
    captainId       @3 :AmpId;
    rating          @4 :Float32;    # Team MMR
    formationTime   @5 :TimeStamp;
}
