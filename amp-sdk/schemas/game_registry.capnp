@0xda4edbd7866b3acf;

using Go = import "go.capnp";
$Go.package("generated");
$Go.import("amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("game_registry_capnp");

using Match = import "match.capnp";
using TimeStamp = Match.TimeStamp;
using Address = Match.Address;
using AmpId = Match.AmpId;
using Signature = Match.Signature;

# Game Studio/Developer
struct GameDeveloper {
    developerId     @0 :AmpId;
    name            @1 :Text;
    wallet          @2 :Address;     # Primary treasury/administration
    contactEmail    @3 :Text;
    website         @4 :Text;
    isVerified      @5 :Bool;
    verificationDate @6 :TimeStamp;
    reputation      @7 :Float32;     # Developer reputation score
    totalGames      @8 :UInt32;      # Number of games published
}

# Game Mode Configuration
struct GameMode {
    modeId          @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    rules           @3 :Data;        # Serialized game-specific rules
    isRanked        @4 :Bool;
    isTournament    @5 :Bool;
    isCooperative   @6 :Bool;        # Co-op vs PvP
    minPlayers      @7 :UInt8;
    maxPlayers      @8 :UInt8;
    teamCount       @9 :UInt8;       # Number of teams
    playersPerTeam  @10 :List(UInt8);# Players per team (variadic)
    estimatedTimeMs @11 :UInt64;     # Average match duration
}

# Game Configuration
struct GameConfiguration {
    gameId          @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    version         @3 :Text;
    
    # Developer Info
    developerId     @4 :AmpId;
    
    # Game Modes
    supportedModes  @5 :List(GameMode);
    
    # Economic Settings
    entryFeeModel   @6 :FeeModel;
    rewardModel     @7 :RewardModel;
    
    # Technical Requirements
    clientHash      @8 :Data;        # Expected client binary hash
    supportedRegions @9 :List(Match.Region);
    minClientVersion @10 :Text;
    serverRequirements @11 :ServerRequirements;
    
    # Metadata
    tags            @12 :List(Text); # "fps", "rpg", "strategy", etc.
    ageRating       @13 :Text;       # "E", "T", "M", etc.
    logoUrl         @14 :Text;
    bannerUrl       @15 :Text;
    website         @16 :Text;
    
    # Status & Statistics
    status          @17 :GameStatus;
    totalMatches    @18 :UInt64;
    activePlayers   @19 :UInt32;
    averageQueueTimeMs @20 :UInt64;
    
    # Smart Contract Integration
    smartContracts  @21 :SmartContractConfig;
}

# Server Requirements
struct ServerRequirements {
    cpuCores        @0 :UInt8;
    ramMB           @1 :UInt32;
    bandwidthMbps   @2 :UInt32;
    os              @3 :Text;        # "linux", "windows"
    storageMB       @4 :UInt32;
}

# Fee Structures
struct FeeModel {
    baseFee         @0 :UInt64;      # Base match fee (wei/token units)
    feeToken        @1 :Address;     # ERC-20 token address
    feeDistribution @2 :FeeDistribution;
    developerCut    @3 :Float32;     # Percentage to developer (0.0-1.0)
    protocolCut     @4 :Float32;     # Percentage to protocol (0.0-1.0)
    verifierCut     @5 :Float32;     # Percentage to verifier (0.0-1.0)
    stakingReward   @6 :Float32;     # Percentage to stakers
}

struct FeeDistribution {
    winnerPayout    @0 :Float32;     # % to winner (tournament style)
    participationReward @1 :Float32; # % to all participants
    performanceBonus @2 :Float32;    # % based on performance metrics
}

struct RewardModel {
    type            @0 :RewardType;
    tokenAddress    @1 :Address;     # Reward token contract
    amountPerWin    @2 :UInt64;
    amountPerLoss   @3 :UInt64;
    amountPerDraw   @4 :UInt64;
    bonusMultiplier @5 :Float32;     # Streak/performance multiplier
}

enum RewardType {
    none            @0;
    fixed           @1;
    dynamic         @2;  # Based on stake/performance
    tournament      @3;
    seasonal        @4;  # Seasonal rewards
}

# On-Chain Integration
struct SmartContractConfig {
    gameContract    @0 :Address;     # Main game logic contract
    nftContract     @1 :Address;     # Item/NFT contract
    tokenContract   @2 :Address;     # Reward token contract
    registryContract @3 :Address;    # Game registry contract
    chainId         @4 :UInt64;      # Avalanche chain/subnet ID
    subnetId        @5 :Text;        # Avalanche subnet ID
}

enum GameStatus {
    active          @0;
    suspended       @1;
    deprecated      @2;
    maintenance     @3;
    beta            @4;
    alpha           @5;
}

# Game Statistics
struct GameStatistics {
    gameId          @0 :AmpId;
    totalMatches    @1 :UInt64;
    totalPlayers    @2 :UInt64;
    totalVolume     @3 :UInt64;      # Total fees/rewards processed
    avgMatchQuality @4 :Float32;
    peakConcurrent  @5 :UInt32;      # Peak concurrent players
    avgQueueTimeMs  @6 :UInt64;
    regionStats     @7 :List(RegionGameStats);
}

struct RegionGameStats {
    region          @0 :Match.Region;
    playerCount     @1 :UInt32;
    matchCount      @2 :UInt64;
    avgLatencyMs    @3 :UInt32;
}

# Service Interfaces
interface GameRegistryService {
    # Game Registration & Management
    registerGame @0 (config :GameConfiguration, developerSig :Signature) -> (gameId :AmpId);
    updateGame @1 (gameId :AmpId, config :GameConfiguration, developerSig :Signature) -> ();
    suspendGame @2 (gameId :AmpId, reason :Text, adminSig :Signature) -> ();
    deprecateGame @3 (gameId :AmpId, migrationTarget :AmpId, adminSig :Signature) -> ();
    
    # Game Discovery
    getGameConfig @4 (gameId :AmpId) -> (config :GameConfiguration);
    listGames @5 (filter :GameFilter) -> (games :List(GameConfiguration));
    getGameStats @6 (gameId :AmpId) -> (stats :GameStatistics);
    
    # Developer Management
    registerDeveloper @7 (developer :GameDeveloper, proof :Data) -> (developerId :AmpId);
    verifyDeveloper @8 (developerId :AmpId, verified :Bool, adminSig :Signature) -> ();
    
    # Analytics
    getPopularGames @9 (limit :UInt32, timeRange :Match.TimeRange) -> (games :List(GameStatsEntry));
}

struct GameFilter {
    tags            @0 :List(Text);
    minPlayers      @1 :UInt8;
    maxPlayers      @2 :UInt8;
    isRanked        @3 :Bool;
    region          @4 :Match.Region;
    status          @5 :GameStatus;
    developerId     @6 :AmpId;
    hasRewards      @7 :Bool;
    freeToPlay      @8 :Bool;
}


struct GameStatsEntry {
    gameId          @0 :AmpId;
    name            @1 :Text;
    playerCount     @2 :UInt32;
    matchCount      @3 :UInt64;
    trend           @4 :Float32;     # Growth rate
}