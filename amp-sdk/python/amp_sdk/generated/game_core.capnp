@0x87693cf75f375a02;



using Match = import "match.capnp";
using AmpId = Match.AmpId;
using Address = Match.Address;
using TimeStamp = Match.TimeStamp;

# Game specific settings and parameters agreed upon at match start
struct MatchConfig {
    gameId          @0 :AmpId;
    maxPlayers      @1 :UInt8;
    timeLimitMs     @2 :UInt64;
    customRules     @3 :Data;     # Game-specific configuration
}

# Defines an input sent by a player for a specific timeframe or turn
struct InputFrame {
    frameId         @0 :UInt64;
    playerId        @1 :AmpId;
    inputData       @2 :Data;     # Encoded actions (e.g. movement, skills, choices)
    timestamp       @3 :TimeStamp;
}

# Represents a significant state change in the game
struct GameEvent {
    eventId         @0 :UInt64;
    eventType       @1 :Text;     # String identifier for event type
    eventData       @2 :Data;     # Encoded effects
    triggeredBy     @3 :AmpId;    # Player or system that caused this event
    timestamp       @4 :TimeStamp;
}
