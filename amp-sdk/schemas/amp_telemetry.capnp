@0xa1b2c3d4e5f60010;

using Rust = import "rust.capnp";
$Rust.parentModule("amp_telemetry_capnp");

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
    eventType       @1 :TelemetryEventType;
    timestamp       @2 :TimeStamp;    # Expected to hold block number or precise timestamp
    verifierId      @3 :Address;      # Optional: address of verifier node if applicable
    eventData       @4 :Data;         # Arbitrary metric data encoded depending on eventType
}

# The Receiver interface that the `amp-telemetry` service provides
interface TelemetryReceiver {
    logEvent @0 (event :AmpTelemetryEvent) -> ();
}
