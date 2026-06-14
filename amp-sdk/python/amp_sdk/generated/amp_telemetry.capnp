@0xa5f1e39a3f915603;



using Match = import "match.capnp";
using AmpId = Match.AmpId;
using Address = Match.Address;
using TimeStamp = Match.TimeStamp;

# Telemetry Events
enum TelemetryEventType {
    matchCreated        @0;
    matchJoined         @1;
    settlementSubmitted @2;
    verifierResult      @3;
    unknown             @4;
}

struct AmpTelemetryEvent {
    matchId         @0 :AmpId;
    gameId          @1 :UInt64;
    eventType       @2 :TelemetryEventType;
    timestamp       @3 :TimeStamp;    # Expected to hold block number or precise timestamp
    verifierId      @4 :Address;      # Optional: address of verifier node if applicable
    eventData       @5 :Data;         # Arbitrary metric data encoded depending on eventType
}

# The Receiver interface that the `amp-telemetry` service provides
interface TelemetryReceiver {
    logEvent @0 (event :AmpTelemetryEvent) -> ();
}
