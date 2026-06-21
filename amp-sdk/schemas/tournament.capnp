@0xceffa2ecb2bb4cbd;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("tournament_capnp");

using Match = import "match.capnp";
using TimeStamp = Match.TimeStamp;
using Address = Match.Address;
using AmpId = Match.AmpId;
using Signature = Match.Signature;

# Tournament Types
enum TournamentType {
    singleElimination @0;
    doubleElimination @1;
    roundRobin       @2;
    swiss            @3;
    gauntlet         @4;
    battleRoyale     @5;
    ladder           @6;
    custom           @7;
}

# Tournament Status
enum TournamentStatus {
    scheduled       @0;
    registration    @1;
    checkIn         @2;
    inProgress      @3;
    completed       @4;
    cancelled       @5;
    paused          @6;
}

# Tournament Structure
struct Tournament {
    tournamentId    @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    
    # Configuration
    type            @3 :TournamentType;
    gameId          @4 :AmpId;
    modeId          @5 :AmpId;
    ruleSetId       @6 :AmpId;       # Matchmaking rules
    
    # Schedule
    registrationStart @7 :TimeStamp;
    registrationEnd  @8 :TimeStamp;
    checkInStart    @9 :TimeStamp;
    checkInEnd      @10 :TimeStamp;
    tournamentStart @11 :TimeStamp;
    estimatedEnd    @12 :TimeStamp;
    
    # Participation
    maxParticipants @13 :UInt32;
    minParticipants @14 :UInt32;
    teamSize        @15 :UInt8;      # Players per team (0 = any)
    isTeamTournament @16 :Bool;
    
    # Entry Requirements
    entryFee        @17 :Match.PaymentInfo;
    minRating       @18 :Float32;
    maxRating       @19 :Float32;
    regionRestrictions @20 :List(Match.Region);
    itemRequirements @21 :Data;      # Serialized item requirements
    
    # Prize Pool
    prizePool       @22 :PrizePool;
    prizeDistribution @23 :List(PrizeTier);
    
    # Status & Progress
    status          @24 :TournamentStatus;
    currentRound    @25 :UInt32;
    totalRounds     @26 :UInt32;
    participants    @27 :UInt32;
    checkedIn       @28 :UInt32;
    
    # Metadata
    organizerId     @29 :AmpId;      # Game developer or community organizer
    logoUrl         @30 :Text;
    bannerUrl       @31 :Text;
    rulesUrl        @32 :Text;
    streamUrl       @33 :Text;
    
    # Smart Contracts
    tournamentContract @34 :Address; # On-chain tournament contract
    chainId         @35 :UInt64;
}

# Prize Structures
struct PrizePool {
    totalValue      @0 :UInt64;
    currencyToken   @1 :Address;     # ERC-20 token address
    isGuaranteed    @2 :Bool;        # Fixed vs percentage of entry fees
    contributionBreakdown @3 :List(PrizeContribution);
}

struct PrizeContribution {
    contributor     @0 :Address;
    amount          @1 :UInt64;
    purpose         @2 :Text;        # "sponsorship", "organizer", "crowdfund"
}

struct PrizeTier {
    placeStart      @0 :UInt32;      # Inclusive start place (1st = 1)
    placeEnd        @1 :UInt32;      # Inclusive end place
    percentage      @2 :Float32;     # % of prize pool
    fixedAmount     @3 :UInt64;      # Fixed amount (if any)
    prizeItems      @4 :List(PrizeItem);
}

struct PrizeItem {
    itemId          @0 :AmpId;       # Inventory item template
    quantity        @1 :UInt32;
    isBound         @2 :Bool;        # Tournament-bound item
}

# Tournament Registration
struct TournamentRegistration {
    registrationId  @0 :AmpId;
    tournamentId    @1 :AmpId;
    
    # Participant Info
    teamId          @2 :AmpId;       # Team registration
    playerIds       @3 :List(AmpId); # Individual players (if no team)
    captainId       @4 :AmpId;       # Team captain
    
    # Registration Details
    registeredAt    @5 :TimeStamp;
    checkedIn       @6 :Bool;
    checkedInAt     @7 :TimeStamp;
    paidEntryFee    @8 :Bool;
    paymentTxHash   @9 :Data;
    
    # Seed/Ranking
    seed            @10 :UInt32;     # Tournament seed
    initialRating   @11 :Float32;    # Rating at registration
    
    # Status
    isActive        @12 :Bool;
    disqualification @13 :Disqualification;
}

struct Disqualification {
    isDisqualified  @0 :Bool;
    reason          @1 :Text;
    disqualifiedAt  @2 :TimeStamp;
    disqualifiedBy  @3 :AmpId;       # Admin/referee ID
}

# Bracket & Match Management
struct Bracket {
    bracketId       @0 :AmpId;
    tournamentId    @1 :AmpId;
    type            @2 :TournamentType;
    
    # Structure
    rounds          @3 :List(Round);
    matches         @4 :List(TournamentMatch);
    
    # State
    currentRound    @5 :UInt32;
    isFinalized     @6 :Bool;
}

struct Round {
    roundNumber     @0 :UInt32;
    name            @1 :Text;        # "Round of 16", "Finals", etc.
    matches         @2 :List(AmpId); # Match IDs in this round
    startTime       @3 :TimeStamp;
    endTime         @4 :TimeStamp;
    isCompleted     @5 :Bool;
}

struct TournamentMatch {
    matchId         @0 :AmpId;
    tournamentId    @1 :AmpId;
    roundNumber     @2 :UInt32;
    
    # Participants
    team1Id         @3 :AmpId;       # Team ID or player ID
    team2Id         @4 :AmpId;
    team1Name       @5 :Text;
    team2Name       @6 :Text;
    
    # Match Details
    scheduledTime   @7 :TimeStamp;
    estimatedDuration @8 :UInt64;    # milliseconds
    gameServer      @9 :GameServer;
    streamUrl       @10 :Text;
    
    # Results
    winnerId        @11 :AmpId;
    loserId         @12 :AmpId;
    isDraw          @13 :Bool;
    scores          @14 :List(UInt64);
    completedAt     @15 :TimeStamp;
    
    # Status
    status          @16 :MatchStatus;
    forfeit         @17 :Forfeit;
    dispute         @18 :Dispute;
}

enum MatchStatus {
    scheduled       @0;
    inProgress      @1;
    completed       @2;
    cancelled       @3;
    postponed       @4;
}

struct Forfeit {
    forfeitedBy     @0 :AmpId;
    reason          @1 :Text;
    approved        @2 :Bool;
}

struct Dispute {
    disputedBy      @0 :AmpId;
    reason          @1 :Text;
    evidence        @2 :Data;        # Screenshots, logs, etc.
    status          @3 :DisputeStatus;
    resolution      @4 :Text;
}

enum DisputeStatus {
    open            @0;
    underReview     @1;
    resolved        @2;
    dismissed       @3;
}

struct GameServer {
    address         @0 :Text;        # IP/hostname
    port            @1 :UInt16;
    region          @2 :Match.Region;
    password        @3 :Text;        # Server password
    spectatorSlot   @4 :Bool;        # Spectator slot available
}

# Tournament Statistics
struct TournamentStats {
    tournamentId    @0 :AmpId;
    totalMatches    @1 :UInt32;
    completedMatches @2 :UInt32;
    averageMatchDuration @3 :UInt64;
    totalViewers    @4 :UInt64;      # Total stream viewers
    peakViewers     @5 :UInt32;
    prizeDistribution @6 :List(PrizeDistribution);
    regionBreakdown @7 :List(TournamentRegionStats);
}

struct PrizeDistribution {
    place           @0 :UInt32;
    participantId   @1 :AmpId;
    amount          @2 :UInt64;
    distributedAt   @3 :TimeStamp;
    txHash          @4 :Data;
}

struct TournamentRegionStats {
    region          @0 :Match.Region;
    participants    @1 :UInt32;
    averageRating   @2 :Float32;
    bestPlace       @3 :UInt32;
}

# Service Interfaces
interface TournamentService {
    # Tournament Management
    createTournament @0 (tournament :Tournament, signature :Signature) -> (tournamentId :AmpId);
    updateTournament @1 (tournamentId :AmpId, tournament :Tournament, signature :Signature) -> ();
    cancelTournament @2 (tournamentId :AmpId, reason :Text, signature :Signature) -> ();
    
    # Registration
    register @3 (tournamentId :AmpId, registration :TournamentRegistration, signature :Signature) -> (registrationId :AmpId);
    checkIn @4 (registrationId :AmpId, signature :Signature) -> ();
    withdraw @5 (registrationId :AmpId, reason :Text, signature :Signature) -> ();
    
    # Bracket Management
    generateBracket @6 (tournamentId :AmpId, bracketType :TournamentType, signature :Signature) -> (bracketId :AmpId);
    advanceRound @7 (tournamentId :AmpId, roundNumber :UInt32, signature :Signature) -> ();
    
    # Match Management
    scheduleMatch @8 (match :TournamentMatch, signature :Signature) -> (matchId :AmpId);
    reportMatchResult @9 (matchId :AmpId, result :MatchResult, signature :Signature) -> ();
    disputeMatch @10 (matchId :AmpId, dispute :Dispute, signature :Signature) -> ();
    
    # Information
    getTournament @11 (tournamentId :AmpId) -> (tournament :Tournament);
    listTournaments @12 (filter :TournamentFilter) -> (tournaments :List(Tournament));
    getBracket @13 (tournamentId :AmpId) -> (bracket :Bracket);
    getStandings @14 (tournamentId :AmpId) -> (standings :List(Standing));
    
    # Administration
    disqualifyParticipant @15 (registrationId :AmpId, reason :Text, signature :Signature) -> ();
    distributePrizes @16 (tournamentId :AmpId, signature :Signature) -> (distributionTxHash :Data);
}

struct MatchResult {
    winnerId        @0 :AmpId;
    loserId         @1 :AmpId;
    isDraw          @2 :Bool;
    scores          @3 :List(UInt64);
    replayHash      @4 :Data;
    evidence        @5 :Data;        # Screenshots, logs
}

struct TournamentFilter {
    gameId          @0 :AmpId;
    status          @1 :TournamentStatus;
    region          @2 :Match.Region;
    tournamentType  @3 :TournamentType;
    hasEntryFee     @4 :Bool;
    minPrizePool    @5 :UInt64;
    registrationOpen @6 :Bool;       # Currently accepting registrations
}

struct Standing {
    rank            @0 :UInt32;
    participantId   @1 :AmpId;
    name            @2 :Text;
    wins            @3 :UInt32;
    losses          @4 :UInt32;
    draws           @5 :UInt32;
    points          @6 :UInt32;
    ratingChange    @7 :Float32;
    prizeEarned     @8 :UInt64;
}