@0x8cbcd143dece677b;

using TimeStamp     =       UInt64;  # Nanoseconds since epoch
using Address       =       UInt64;
using AmpId         =       Data;

enum MatchType {
    tb @0; # turn based
    la @1; # live action
}

enum Region {
    na  @0; # North America
    eu  @1; # Europe
    sa  @2; # South America
    as  @3; # Asia
}

enum Elo {
  unranked @0;
  bronze   @1;   # ~ 0–1199
  silver   @2;   # ~ 1200–1499
  gold     @3;   # ~ 1500–1799
  platinum @4;   # ~ 1800–1999
  diamond  @5;   # ~ 2000–2199
  master   @6;   # ~ 2200–2399
  grandmaster @7;# ~ 2400+
}

enum Outcome {
  unknown   @0;
  complete  @1;
  draw      @2;
  void      @3;  # e.g. cancelled, cheater detected, refunded
}

struct MatchConfig {
    gameId          @0      :AmpId;
    rulesVersion    @1      :Float32;
    seed            @2      :UInt64;
    mode            @3      :MatchType;
}

struct InputFrame {
    timestamp       @0      :TimeStamp;
    playerId        @1      :UInt8;
    command         @2      :Text;
}

struct Event {
    timestamp       @0      :TimeStamp;
    frame           @1      :InputFrame;
}   

struct GameEvent {
    events          @0      :List(Event); # a group of events in a given time period.
    actor           @1      :Opponent;
    target          @2      :Opponent;
    damage          @3      :UInt16;
}

struct MatchTranscript {
    config          @0      :MatchConfig;
    events          @1      :List(GameEvent);
    hash            @2      :Data;
}


struct PaymentInfo {
    payerWallet     @0      :Address;
    feeToken        @1      :Address; #erc20
    authSpend       @2      :UInt64;
}

struct Opponent {
    playerId        @0      :AmpId;          # e.g. user id
    displayName     @1      :Text;           # optional, for UI
    playerWallet    @2      :PaymentInfo;
    elo             @3      :Elo;
    region          @4      :Region;
    mode            @5      :MatchType;          # queue/mode they came from
}

struct MatchSettlement {
    matchId         @0      :Text;
    ampMatchId      @1      :Text;
    outcome         @2      :Outcome;
    netAmount       @3      :Int64; # Net token delta for this round.
    token           @4      :Address;
    victor          @5      :Opponent;  
}

struct Error {
    msg             @0      :Text;
}

struct AssignmentRequest {
    registrationId    @0    :AmpId; #ie: Activision
    matchConfig       @1    :MatchConfig;
    paymentInfo       @2    :PaymentInfo;
    playerPool        @3    :List(Opponent); #static for now
}

struct MatchAssignment {
    ticketId        @0 :AmpId;
    matchId         @1 :AmpId;     #on-chain AMP match id (bytes32/uint256 hex).
    opponents       @2 :List(Opponent);
    settle          @3 :MatchMaker; # capability to call matchmaker 
}

struct AssignmentResult {
    union {
        match   @0      :MatchAssignment;
        err     @1      :Error;
    }
}

interface MatchMaker {
    # Verifies a match transcript asynchronously.
    verifyAsyncReplay @0 (transcript :MatchTranscript) -> (outcome: MatchSettlement);

    # Verifies a real-time transcript.
    # Currently a placeholder for RT_HASH_AGREE mode.
    verifyRealTimeTranscript @1 (transcript :MatchTranscript) -> (winner :Text, outcomeCode :Text, resultHash :Data);
}


interface GameConnector {
    # Requests a new game service. will respond with assignemnt, or error. 
    requestGameService @0 (assignment_request:AssignmentRequest ) -> (assignment :AssignmentResult);
}
