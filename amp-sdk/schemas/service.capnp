@0xf3a510e30737976e;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated");

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

using PlayerProfile = import "player_profile.capnp";
using MatchmakingRules = import "matchmaking_rules.capnp";
using Inventory = import "inventory.capnp";
using Tournament = import "tournament.capnp";
using Security = import "security.capnp";

interface GameSessionService {
    # The entry point for any client connecting to the system.
    
    login @0 (gameId :UInt64, signature :Data, challengePayload :Data) -> (session :UserSession);
    # Authenticates the user for a specific game. 
    # The signature must verify against a challenge (e.g. nonce) for the game's admin address.
    # Returns a UserSession capability which holds the user's identity and game context.
    
    requestChallenge @1 (gameId :UInt64) -> (challenge :Data, expiresAt :UInt64);
    # Requests a one-time authentication challenge for the given game.
    # The client must sign the returned challenge bytes and pass them to login().
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

# Extended Services for Avalanche Matchmaking Protocol
interface ExtendedPlayerService extends(PlayerProfile.PlayerProfileService) {
    # Extended player service with additional functionality
}

interface ExtendedMatchmakingService extends(MatchmakingRules.MatchmakingRuleService) {
    # Extended matchmaking service with additional functionality
}

interface ExtendedInventoryService extends(Inventory.InventoryService) {
    # Extended inventory service with additional functionality
}

interface ExtendedTournamentService extends(Tournament.TournamentService) {
    # Extended tournament service with additional functionality
}

interface ExtendedSecurityService extends(Security.SecurityService) {
    # Extended security service with additional functionality
}

interface ProtocolRegistryService {
    # Service registry for protocol discovery and versioning
    
    registerGame @0 (gameInfo :GameRegistrationInfo, signature :Signature) -> (gameId :Match.AmpId);
    # Register a new game with the protocol
    
    getServiceEndpoints @1 (serviceType :ServiceType) -> (endpoints :List(ServiceEndpoint));
    # Get endpoints for specific service types
    
    getProtocolVersion @2 () -> (version :ProtocolVersion);
    # Get current protocol version information
}

struct GameRegistrationInfo {
    name            @0 :Text;
    description     @1 :Text;
    developer       @2 :Text;
    website         @3 :Text;
    supportedRegions @4 :List(Match.Region);
    gameModes       @5 :List(GameModeInfo);
    adminAddress    @6 :Match.Address;
}

struct GameModeInfo {
    modeId          @0 :Match.AmpId;
    name            @1 :Text;
    description     @2 :Text;
    minPlayers      @3 :UInt8;
    maxPlayers      @4 :UInt8;
    defaultRuleSet  @5 :Match.AmpId;
}

struct ServiceEndpoint {
    serviceType     @0 :ServiceType;
    url             @1 :Text;
    chainId         @2 :UInt64;
    version         @3 :Text;
    loadBalancer    @4 :Text;
}

enum ServiceType {
    matchmaking     @0;
    playerProfiles  @1;
    inventory       @2;
    tournaments     @3;
    security        @4;
    telemetry       @5;
    gameSession     @6;
}

struct ProtocolVersion {
    major           @0 :UInt16;
    minor           @1 :UInt16;
    patch           @2 :UInt16;
    supported       @3 :Bool;        # If client version is supported
    minRequired     @4 :Bool;        # If client must upgrade
    changelog       @5 :Text;
}
