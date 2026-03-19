@0xa1b2c3d4e5f60003;

using Rust = import "rust.capnp";
$Rust.parentModule("service_capnp");

using Match = import "match.capnp";
using GameMatchRequest = Match.GameMatchRequest;
using MatchAssignment = Match.MatchAssignment;
using OutcomeSubmission = Match.OutcomeSubmission;
using Outcome = Match.Outcome;
using Signature = Match.Signature;

using GameCore = import "game_core.capnp";
using AmpTelemetry = import "amp_telemetry.capnp";

interface GameSessionService {
    # The entry point for any client connecting to the system.
    
    login @0 (gameId :UInt64, signedChallenge :Signature) -> (session :UserSession);
    # Authenticates the user for a specific game. 
    # The signature must verify against a challenge (e.g. nonce) for the game's admin address.
    # Returns a UserSession capability which holds the user's identity and game context.
}

interface UserSession {
    # Represents an authenticated user's session.
    
    requestMatch @0 (req :GameMatchRequest) -> (assignment :MatchAssignment, session :MatchSession);
    # Queues for a match. 
    # This call may hang (return a Promise) until a match is found.
    # Returns:
    #   assignment: The data describing the match (opponents, config).
    #   session: The capability to interact with this specific match.
    
    reconnect @1 (matchId :Match.AmpId) -> (session :MatchSession);
    # Reclaims a capability for an existing active match if the connection was lost.
}

interface MatchSession {
    # Represents the connection to a specific active match.
    # The implementation of this capability implicitly knows the matchId and playerId.
    
    submitOutcome @0 (submission :OutcomeSubmission) -> (signature :Signature);
    # Submits the result of the game.
    # Returns the Verifier's signature if accepted.
    
    subscribeToEvents @1 (subscriber :MatchListener) -> ();
    # Subscribes to match events (e.g. opponent disconnected, match settled).
    
    emitGameEvent @2 (event :GameCore.GameEvent) -> ();
    # Emit a game event using this capability securely.

    emitTelemetry @3 (event :AmpTelemetry.AmpTelemetryEvent) -> ();
    # Send telemetry via this verified match session capability.
}

interface MatchListener {
    # Callback interface for clients to receive updates.
    
    onMatchSettled @0 (outcome :Outcome);
    # Called when the match is officially settled on-chain or by the verifier.
    
    onOpponentDisconnected @1 ();
}
