@0xbd245c6130b9589d;



using import "match.capnp".TimeStamp;
using import "match.capnp".Signature;

# Turn-Based Replay
struct TurnBasedReplay {
    initialState    @0 :Data;              # Snapshot of the game start (optional if implicit from config)
    moves           @1 :List(SignedMove);
}

struct SignedMove {
    playerId        @0 :UInt8;             # Index in player list (0 or 1)
    moveData        @1 :Data;              # Game-specific move serialization
    timestamp       @2 :TimeStamp;
    signature       @3 :Signature;         # Signature of (moveData + timestamp + matchId)
}

# Real-Time Transcript
struct RealtimeTranscript {
    # Used for verifying continuous games like RTS or fighting games
    snapshots       @0 :List(StateSnapshot);
    inputs          @1 :Data;              # Compressed input stream from all players
}

struct StateSnapshot {
    frameId         @0 :UInt64;
    stateHash       @1 :Data;              # Merkle root or hash of game state
    signatures      @2 :List(Signature);   # Signatures from players agreeing to this state
}

# Oracle Result
struct OracleResult {
    source          @0 :Text;              # "Chainlink", "API-XYZ"
    queryId         @1 :Data;              # Request correlation ID
    result          @2 :Data;              # ABI-encoded result (e.g. winner uint8)
    proof           @3 :Data;              # Cryptographic proof or signature
}
