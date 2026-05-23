@0xb4b48970fe9c3d22;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("matchmaking_rules_capnp");

using Match = import "match.capnp";
using PlayerProfile = import "player_profile.capnp";
using TimeStamp = Match.TimeStamp;
using AmpId = Match.AmpId;
using Signature = Match.Signature;

# Rule Definition Language (FlexMatch-style)
struct MatchmakingRule {
    ruleId          @0 :Text;        # Unique identifier
    name            @1 :Text;
    description     @2 :Text;
    type            @3 :RuleType;
    parameters      @4 :Data;        # Rule-specific parameters
    weight          @5 :Float32;     # Importance in matchmaking (0.0-1.0)
    isHardConstraint @6 :Bool;       # Must pass (true) or preference (false)
    priority        @7 :UInt8;       # Evaluation priority (0-255)
}

enum RuleType {
    latency         @0;  # Maximum latency
    skill           @1;  # Skill difference threshold
    teamBalance     @2;  # Team composition
    region          @3;  # Geographic region
    language        @4;  # Shared language
    schedule        @5;  # Time-based matching
    inventory       @6;  # Required/restricted items
    party           @7;  # Party/groups
    avoidance       @8;  # Player avoidance list
    preference      @9;  # Player preferences
    custom          @10; # Game-specific logic
    pingBased       @11; # Latency-based matching
    skillDecay      @12; # Skill decay over time
    recentMatches   @13; # Recent match history
    connectionQuality @14; # Network quality
}

# Rule Implementations
struct LatencyRule {
    maxPingMs       @0 :UInt32;
    measurementMethod @1 :Text;      # "direct", "regional", "proxy"
    regionOverride  @2 :Bool;        # Allow region override for low pop
}

struct SkillRule {
    maxDifference   @0 :Float32;     # Maximum MMR difference
    useTrueSkill    @1 :Bool;
    teamVariance    @2 :Float32;     # Maximum team skill variance
    timeDecay       @3 :Bool;        # Increase tolerance over time
    decayRate       @4 :Float32;     # Tolerance increase per minute
}

struct TeamBalanceRule {
    requiredRoles   @0 :List(Text);  # ["tank", "dps", "support"]
    maxDuplicates   @1 :UInt8;       # Max players with same role
    composition     @2 :TeamComposition;
    roleWeights     @3 :Data;        # Role importance weights
}

struct TeamComposition {
    teamCount       @0 :UInt8;
    rolesPerTeam    @1 :List(RoleRequirement);
    flexSlots       @2 :UInt8;       # Flexible/any role slots
}

struct RoleRequirement {
    role            @0 :Text;
    minPlayers      @1 :UInt8;
    maxPlayers      @2 :UInt8;
    priority        @3 :UInt8;       # Role priority (0-255)
}

struct InventoryRule {
    requiredItems   @0 :List(ItemRequirement);
    bannedItems     @1 :List(AmpId); # Item IDs not allowed
    maxItemRarity   @2 :Text;        # "common", "rare", "legendary"
    minCollectionScore @3 :UInt32;   # Minimum inventory score
}

struct ItemRequirement {
    itemId          @0 :AmpId;
    minQuantity     @1 :UInt32;
    slot            @2 :Text;        # Specific slot requirement
    alternatives    @3 :List(AmpId); # Alternative item IDs
}

struct PartyRule {
    allowMixedParties @0 :Bool;
    maxPartySize    @1 :UInt8;
    partySkillCalculation @2 :PartySkillMethod;
    partyRatingAdjustment @3 :Float32; # Rating adjustment for parties
    soloQueueBonus  @4 :Bool;        # Bonus for solo queue players
}

enum PartySkillMethod {
    highest         @0;  # Use highest player skill
    average         @1;  # Use average skill
    weighted        @2;  # Weighted by role/performance
    adjustedAverage @3;  # Average with party adjustment
}

struct AvoidanceRule {
    maxRecentOpponents @0 :UInt8;    # Avoid recent opponents
    avoidFriends      @1 :Bool;      # Don't match with friends
    avoidBlocked      @2 :Bool;      # Don't match with blocked players
    cooldownMinutes   @3 :UInt32;    # Minutes to avoid same players
}

struct ScheduleRule {
    timeWindows     @0 :List(TimeWindow);
    dayOfWeek       @1 :List(UInt8); # 0=Sunday, 6=Saturday
    timeZone        @2 :Text;        # Reference timezone
}

struct TimeWindow {
    startHour       @0 :UInt8;       # 0-23
    endHour         @1 :UInt8;       # 0-23
    priority        @2 :UInt8;       # Window priority
}

struct ConnectionQualityRule {
    minPacketLoss   @0 :Float32;     # Maximum allowed packet loss %
    maxJitterMs     @1 :UInt32;      # Maximum jitter
    minBandwidthKbps @2 :UInt32;     # Minimum bandwidth
    requireNATType  @3 :NATType;     # Required NAT type
}

enum NATType {
    any             @0;
    open            @1;
    moderate        @2;
    strict          @3;
}

# Rule Evaluation Context
struct RuleEvaluationContext {
    players         @0 :List(PlayerContext);
    gameMode        @1 :AmpId;
    region          @2 :Match.Region;
    timeOfDay       @3 :TimeStamp;
    queueDurationMs @4 :UInt64;
}

struct PlayerContext {
    playerId        @0 :AmpId;
    mmr             @1 :Float32;
    attributes      @2 :PlayerProfile.PlayerAttributes;
    inventoryScore  @3 :UInt32;
    connectionQuality @4 :ConnectionQuality;
    recentMatches   @5 :List(RecentMatch);
    partyId         @6 :AmpId;
    isPartyLeader   @7 :Bool;
}

struct ConnectionQuality {
    pingMs          @0 :UInt32;
    packetLoss      @1 :Float32;
    jitterMs        @2 :UInt32;
    natType         @3 :NATType;
    region          @4 :Match.Region;
}

struct RecentMatch {
    matchId         @0 :AmpId;
    opponentIds     @1 :List(AmpId);
    outcome         @2 :Match.OutcomeType;
    timestamp       @3 :TimeStamp;
}

# Rule Set Collection
struct RuleSet {
    ruleSetId       @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    gameId          @3 :AmpId;
    modeId          @4 :AmpId;       # Specific game mode
    rules           @5 :List(MatchmakingRule);
    evaluationOrder @6 :List(Text);  # Rule execution order
    timeoutMs       @7 :UInt64;      # Matchmaking timeout
    backfill        @8 :BackfillPolicy;
    fallbackRules   @9 :List(MatchmakingRule); # Rules after timeout
    version         @10 :Text;       # Rule set version
    isActive        @11 :Bool;
}

struct BackfillPolicy {
    enabled         @0 :Bool;
    maxTimeMs       @1 :UInt64;
    skillTolerance  @2 :Float32;     # Increased tolerance for backfill
    partialTeams    @3 :Bool;        # Allow incomplete teams
    roleFlexibility @4 :Bool;        # Relax role requirements
    connectionTolerance @5 :Bool;    # Relax connection requirements
}

# Match Quality Metrics
struct MatchQuality {
    totalScore      @0 :Float32;     # Overall match quality (0.0-1.0)
    skillBalance    @1 :Float32;     # Skill balance score
    latencyScore    @2 :Float32;     # Latency optimization score
    roleBalance     @3 :Float32;     # Role composition score
    connectionScore @4 :Float32;     # Connection quality score
    preferenceScore @5 :Float32;     # Player preference score
    ruleScores      @6 :List(RuleScore); # Individual rule scores
}

struct RuleScore {
    ruleId          @0 :Text;
    score           @1 :Float32;
    passed          @2 :Bool;
    weight          @3 :Float32;
}

# Service Interface
interface MatchmakingRuleService {
    createRuleSet @0 (ruleSet :RuleSet, signature :Signature) -> (ruleSetId :AmpId);
    updateRuleSet @1 (ruleSetId :AmpId, ruleSet :RuleSet, signature :Signature) -> ();
    deleteRuleSet @2 (ruleSetId :AmpId, signature :Signature) -> ();
    
    getRuleSet @3 (ruleSetId :AmpId) -> (ruleSet :RuleSet);
    listRuleSets @4 (gameId :AmpId, modeId :AmpId) -> (ruleSets :List(RuleSet));
    
    evaluateMatch @5 (context :RuleEvaluationContext, ruleSetId :AmpId) -> (quality :MatchQuality, passes :Bool);
    findBestMatch @6 (players :List(PlayerContext), ruleSetId :AmpId) -> (teams :List(List(AmpId)), quality :MatchQuality);
    
    # Analytics
    getRuleStatistics @7 (ruleSetId :AmpId, timeRange :Match.TimeRange) -> (stats :RuleStats);
}

struct RuleStats {
    ruleSetId       @0 :AmpId;
    totalEvaluations @1 :UInt64;
    successfulMatches @2 :UInt64;
    failedMatches   @3 :UInt64;
    avgMatchQuality @4 :Float32;
    avgQueueTimeMs  @5 :UInt64;
    commonFailures  @6 :List(RuleFailure);
}

struct RuleFailure {
    ruleId          @0 :Text;
    failureCount    @1 :UInt64;
    failureRate     @2 :Float32;
    commonReason    @3 :Text;
}