@0xd482546b8dd0c27b;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("player_profile_capnp");

using Match = import "match.capnp";
using TimeStamp = Match.TimeStamp;
using Address = Match.Address;
using AmpId = Match.AmpId;
using Signature = Match.Signature;
using Region = Match.Region;

# Advanced Skill Rating (MMR - Match Making Rating)
struct MMR {
    rating          @0 :Float32;     # TrueSkill or Glicko-2 rating
    uncertainty     @1 :Float32;     # Rating confidence
    volatility      @2 :Float32;     # Glicko-2 volatility
    gamesPlayed     @3 :UInt32;      # Total matches
    lastUpdated     @4 :TimeStamp;
}

# Player Attributes for Rule-Based Matchmaking
struct PlayerAttributes {
    preferredRole   @0 :Text;        # "tank", "dps", "support", etc.
    playStyle       @1 :Text;        # "aggressive", "defensive", "balanced"
    language        @2 :Text;        # ISO language code
    platform        @3 :Text;        # "pc", "console", "mobile"
    inputDevice     @4 :Text;        # "keyboard_mouse", "controller", "touch"
    timeZone        @5 :Text;        # IANA timezone
    micEnabled      @6 :Bool;        # Voice chat capability
    maxPingMs       @7 :UInt32;      # Maximum acceptable latency
}

# Game-Specific Statistics
struct GameStats {
    gameId          @0 :AmpId;
    matchesPlayed   @1 :UInt32;
    wins            @2 :UInt32;
    losses          @3 :UInt32;
    draws           @4 :UInt32;
    winRate         @5 :Float32;
    totalPlayTimeMs @6 :UInt64;      # milliseconds
    bestStreak      @7 :UInt16;      # Best win streak
    currentStreak   @8 :Int16;       # Current streak (negative for losses)
    achievements    @9 :List(Achievement);
}

struct Achievement {
    achievementId   @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    unlockedAt      @3 :TimeStamp;
    rarity          @4 :Float32;     # % of players with achievement
}

# Player Preferences
struct PlayerPreferences {
    autoAccept      @0 :Bool;        # Auto-accept matches
    maxWaitTimeMs   @1 :UInt64;      # Max queue time (ms)
    minSkillDiff    @2 :Float32;     # Max acceptable skill difference
    regionPriority  @3 :List(Region);  # Ordered region preference
    languageFilter  @4 :List(Text);  # Preferred languages
    contentFilter   @5 :ContentFilter;
    notificationPrefs @6 :NotificationPrefs;
}

struct ContentFilter {
    allowViolence   @0 :Bool;
    allowLanguage   @1 :Bool;
    allowGambling   @2 :Bool;
    ageRating       @3 :Text;        # "E", "T", "M", "AO"
}

struct NotificationPrefs {
    matchReady      @0 :Bool;
    friendOnline    @1 :Bool;
    tournamentStart @2 :Bool;
    rewardReceived  @3 :Bool;
}

# Player Restrictions
struct PlayerRestrictions {
    isBanned        @0 :Bool;
    banExpiry       @1 :TimeStamp;
    banReason       @2 :Text;
    matchmakingCoolDown @3 :UInt64;  # Cannot queue until timestamp
    dailyMatchLimit @4 :UInt32;      # 0 = unlimited
    chatRestricted  @5 :Bool;
    voiceRestricted @6 :Bool;
    reportCount     @7 :UInt32;      # Number of recent reports
}

# Social Features
struct FriendInfo {
    playerId        @0 :AmpId;
    displayName     @1 :Text;
    status          @2 :FriendStatus;
    friendshipDate  @3 :TimeStamp;
    lastPlayedTogether @4 :TimeStamp;
}

enum FriendStatus {
    offline         @0;
    online          @1;
    inGame          @2;
    away            @3;
    doNotDisturb    @4;
}

struct BlockedPlayer {
    playerId        @0 :AmpId;
    displayName     @1 :Text;
    blockedAt       @2 :TimeStamp;
    reason          @3 :Text;
}

# Complete Player Profile
struct PlayerProfile {
    # Basic Information
    playerId        @0 :AmpId;
    displayName     @1 :Text;
    walletAddress   @2 :Address;
    
    # Skill & Statistics
    globalMMR       @3 :MMR;
    gameStats       @4 :List(GameStats);
    totalPlayTimeMs @5 :UInt64;
    
    # Attributes & Preferences
    attributes      @6 :PlayerAttributes;
    preferences     @7 :PlayerPreferences;
    
    # Social
    friends         @8 :List(FriendInfo);
    blockedPlayers  @9 :List(BlockedPlayer);
    
    # Restrictions & Status
    restrictions    @10 :PlayerRestrictions;
    createdAt       @11 :TimeStamp;
    lastLogin       @12 :TimeStamp;
    isOnline        @13 :Bool;
    
    # Extended Data References
    inventoryId     @14 :AmpId;      # Reference to inventory
    loadoutIds      @15 :List(AmpId);# Active loadouts
    achievements    @16 :List(Achievement);
    title           @17 :Text;       # Player title/prefix
    avatarUrl       @18 :Text;       # Profile picture URL
    bannerUrl       @19 :Text;       # Profile banner URL
    biography       @20 :Text;       # Player bio
}

interface PlayerProfileService {
    createOrUpdateProfile @0 (playerId :AmpId, displayName :Text, walletAddress :Address, preferredRole :Text, language :Text, platform :Text, region :Text, maxPingMs :UInt32, initialMmr :Float32) -> (playerId :AmpId);
    getProfile @1 (playerId :AmpId) -> (profile :PlayerProfile);
    recordMatchResult @2 (playerId :AmpId, opponentId :AmpId, gameId :AmpId, score :Float32, playTimeMs :UInt64) -> ();
    setOffline @3 (playerId :AmpId) -> ();
    listPlayers @4 () -> (players :List(PlayerProfile));
    applyRestriction @5 (playerId :AmpId, ban :Bool, cooldownMs :UInt64) -> ();
}