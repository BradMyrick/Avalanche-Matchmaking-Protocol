@0x85d366dc47d9302e;

struct MatchConfig {
  gameId @0 :UInt64;
  rulesVersion @1 :Text;
  seed @2 :UInt64;
}

struct InputFrame {
  tick @0 :UInt32;
  playerId @1 :UInt8;
  command @2 :Text;
}

struct GameEvent {
  tick @0 :UInt32;
  eventType @1 :Text;
  actorId @2 :UInt8;
  targetId @3 :UInt8;
  damage @4 :UInt16;
}

struct MatchTranscript {
  config @0 :MatchConfig;
  events @1 :List(GameEvent);
  summary @2 :Text;
}

interface Verifier {
  # Verifies a match transcript asynchronously.
  # Returns the winner, outcome code, and a deterministic hash of the result.
  verifyAsyncReplay @0 (transcript :MatchTranscript) -> (winner :Text, outcomeCode :Text, resultHash :Data);

  # Verifies a real-time transcript.
  # Currently a placeholder for RT_HASH_AGREE mode.
  verifyRealTimeTranscript @1 (transcript :MatchTranscript) -> (winner :Text, outcomeCode :Text, resultHash :Data);
}

# TODO: Add game-specific extensions using Cap'n Proto generics or AnyPointer.
# This schema defines the base transcript format required for AMP settlement.
