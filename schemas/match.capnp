@0xa1b2c3d4e5f60001;

using Rust = import "rust.capnp";
$Rust.parentModule("match_capnp");

# Core Types
using TimeStamp     = UInt64;  # Nanoseconds since epoch
using Address       = Data;    # 20-byte Ethereum address
using AmpId         = Data;    # e.g., UUID or specialized ID
using Signature     = Data;    # 65-byte EIP-712 signature

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
}

# Match Request
struct GameMatchRequest {
    gameId          @0 :AmpId;     # e.g. "chess-v1"
    rulesType       @1 :Text;      # Variant tag, e.g. "standard", "blitz"
    stake           @2 :PaymentInfo;
    playerInfo      @3 :PlayerInfo;
    optionalConfig  @4 :Data;      # Game-specific config blob (serialized Cap'n Proto or JSON)
}

# Match Assignment
struct MatchAssignment {
    matchId         @0 :AmpId;     # The authoritative Match ID (e.g. on-chain UUID)
    opponents       @1 :List(PlayerInfo);
    gameConfig      @2 :Data;      # Snapshot of agreed configuration
    assignedVerifier @3 :Address;  # The public key/address of the Verifier node
    
    # Capability for the match session will be defined in service.capnp
    # session         @4 :MatchSession; 
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
    events          @1 :List(Data);   # Serialized GameEvents (opaque to generic layer)
    finalStateHash  @2 :Data;
}
