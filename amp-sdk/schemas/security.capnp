@0xd333f0d79f40965d;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("security_capnp");

using Match = import "match.capnp";
using TimeStamp = Match.TimeStamp;
using Address = Match.Address;
using AmpId = Match.AmpId;
using Signature = Match.Signature;

# Fraud Detection Categories
enum FraudCategory {
    cheating         @0;  # Game cheating/hacking
    matchFixing      @1;  # Intentional losing/throwing
    smurfing         @2;  # High skill player on low rank account
    boosting         @3;  # Account boosting services
    collusion        @4;  # Player collusion
    disconnectAbuse  @5;  # Intentional disconnects
    pingAbuse        @6;  # Lag switching
    accountSharing   @7;  # Multiple users on one account
    botting          @8;  # Automated gameplay
    exploit          @9;  # Game exploit abuse
    harassment       @10; # Toxic behavior
    paymentFraud     @11; # Fraudulent payments
    identityFraud    @12; # Fake identity
}

# Security Incident
struct SecurityIncident {
    incidentId      @0 :AmpId;
    playerId        @1 :AmpId;
    category        @2 :FraudCategory;
    
    # Incident Details
    description     @3 :Text;
    severity        @4 :SeverityLevel;
    confidence      @5 :Float32;     # 0.0-1.0 confidence score
    
    # Evidence
    evidence        @6 :List(Evidence);
    matchId         @7 :AmpId;       # Related match
    gameId          @8 :AmpId;       # Related game
    
    # Timeline
    detectedAt      @9 :TimeStamp;
    firstOccurrence @10 :TimeStamp;
    lastOccurrence  @11 :TimeStamp;
    
    # Status
    status          @12 :IncidentStatus;
    assignedTo      @13 :AmpId;      # Assigned moderator/admin
    resolution      @14 :Resolution;
    
    # Analytics
    impactScore     @15 :Float32;    # Calculated impact score
    patternId       @16 :AmpId;      # Fraud pattern ID
}

enum SeverityLevel {
    low             @0;
    medium          @1;
    high            @2;
    critical        @3;
}

enum IncidentStatus {
    open            @0;
    investigating   @1;
    resolved        @2;
    dismissed       @3;
    escalated       @4;
}

struct Resolution {
    action          @0 :SecurityAction;
    duration        @1 :UInt64;      # Duration in seconds (0 = permanent)
    reason          @2 :Text;
    resolvedBy      @3 :AmpId;       # Moderator/admin ID
    resolvedAt      @4 :TimeStamp;
    notes           @5 :Text;
}

struct Evidence {
    evidenceId      @0 :AmpId;
    type            @1 :EvidenceType;
    data            @2 :Data;        # Screenshot, log, video, etc.
    hash            @3 :Data;        # Content hash
    submittedBy     @4 :AmpId;       # Reporter ID
    submittedAt     @5 :TimeStamp;
    verified        @6 :Bool;
}

enum EvidenceType {
    screenshot      @0;
    video           @1;
    logFile         @2;
    replay          @3;
    chatLog         @4;
    performanceData @5;
    networkCapture  @6;
    memoryDump      @7;
}

# Security Actions
enum SecurityAction {
    warning         @0;  # Formal warning
    tempBan         @1;  # Temporary ban
    permBan         @2;  # Permanent ban
    rankReset       @3;  # Reset MMR/rank
    rewardRemoval   @4;  # Remove earned rewards
    itemRemoval     @5;  # Remove acquired items
    queueRestriction @6; # Restricted matchmaking
    chatRestriction @7; # Chat/voice restrictions
    accountVerification @8; # Require verification
    monitoring      @9;  # Enhanced monitoring
    noAction        @10; # No action taken
}

# Fraud Pattern Detection
struct FraudPattern {
    patternId       @0 :AmpId;
    name            @1 :Text;
    description     @2 :Text;
    
    # Detection Logic
    detectionRules  @3 :Data;        # Serialized detection rules
    indicators      @4 :List(FraudIndicator);
    threshold       @5 :Float32;     # Detection threshold
    
    # Statistics
    detectionCount  @6 :UInt64;
    accuracy        @7 :Float32;     # True positive rate
    falsePositiveRate @8 :Float32;
    
    # Metadata
    createdAt       @9 :TimeStamp;
    lastUpdated     @10 :TimeStamp;
    isActive        @11 :Bool;
    version         @12 :Text;
}

struct FraudIndicator {
    indicatorId     @0 :Text;
    name            @1 :Text;
    weight          @2 :Float32;     # Contribution weight
    dataSource      @3 :DataSource;  # Where to get data
    calculation     @4 :Data;        # Calculation logic
}

enum DataSource {
    gameplay        @0;
    network         @1;
    system          @2;
    behavioral      @3;
    economic        @4;
    social          @5;
}

# Player Behavior Analysis
struct BehaviorProfile {
    playerId        @0 :AmpId;
    
    # Behavior Metrics
    playPatterns    @1 :PlayPatterns;
    socialBehavior  @2 :SocialBehavior;
    economicBehavior @3 :EconomicBehavior;
    
    # Risk Assessment
    riskScore       @4 :Float32;     # 0.0-1.0 risk score
    riskFactors     @5 :List(RiskFactor);
    confidence      @6 :Float32;     # Profile confidence
    
    # History
    incidents       @7 :List(IncidentHistory);
    warnings        @8 :UInt32;
    suspensions     @9 :UInt32;
    
    # Timestamps
    createdAt       @10 :TimeStamp;
    lastUpdated     @11 :TimeStamp;
    monitoringLevel @12 :MonitoringLevel;
}

struct PlayPatterns {
    sessionLengthStats @0 :SessionStats;
    timeOfDayPattern @1 :TimePattern;
    playFrequency   @2 :FrequencyStats;
    performanceVariance @3 :Float32; # Performance inconsistency
    disconnectRate  @4 :Float32;     # % of matches disconnected
}

struct SessionStats {
    avgDurationMs   @0 :UInt64;
    stdDevDuration  @1 :UInt64;
    totalSessions   @2 :UInt64;
}

struct TimePattern {
    usualStartHour  @0 :UInt8;       # 0-23
    usualEndHour    @1 :UInt8;
    variance        @2 :Float32;     # Schedule consistency
}

struct FrequencyStats {
    matchesPerDay   @0 :Float32;
    daysSinceLast   @1 :UInt32;
    consistency     @2 :Float32;     # Play schedule consistency
}

struct SocialBehavior {
    chatFrequency   @0 :Float32;
    reportCount     @1 :UInt32;
    wasReportedCount @2 :UInt32;
    friendCount     @3 :UInt32;
    blockedCount    @4 :UInt32;
    toxicityScore   @5 :Float32;
}

struct EconomicBehavior {
    totalSpent      @0 :UInt64;
    transactionFrequency @1 :Float32;
    chargebackRate  @2 :Float32;
    tradingActivity @3 :TradingStats;
    currencyFlow    @4 :CurrencyFlow;
}

struct TradingStats {
    totalTrades     @0 :UInt32;
    suspiciousTrades @1 :UInt32;
    avgTradeValue   @2 :UInt64;
}

struct CurrencyFlow {
    inFlow          @0 :UInt64;      # Currency received
    outFlow         @1 :UInt64;      # Currency sent
    netFlow         @2 :Int64;       # Net flow (can be negative)
}

struct RiskFactor {
    factor          @0 :Text;
    score           @1 :Float32;
    evidence        @2 :Data;        # Supporting data
}

struct IncidentHistory {
    incidentId      @0 :AmpId;
    category        @1 :FraudCategory;
    severity        @2 :SeverityLevel;
    resolvedAt      @3 :TimeStamp;
    actionTaken     @4 :SecurityAction;
}

enum MonitoringLevel {
    normal          @0;
    elevated        @1;
    high            @2;
    maximum         @3;
}

# Anti-Cheat System
struct AntiCheatReport {
    reportId        @0 :AmpId;
    playerId        @1 :AmpId;
    matchId         @2 :AmpId;
    
    # Detection Data
    cheatSignatures @3 :List(CheatSignature);
    systemChecks    @4 :SystemCheckResults;
    behaviorAnomalies @5 :List(BehaviorAnomaly);
    
    # Confidence
    detectionConfidence @6 :Float32;
    cheatType       @7 :CheatType;
    
    # Technical Details
    clientVersion   @8 :Text;
    hardwareHash    @9 :Data;
    osInfo          @10 :Text;
    
    # Status
    status          @11 :ReportStatus;
    reviewedBy      @12 :AmpId;
    reviewNotes     @13 :Text;
}

struct CheatSignature {
    signatureId     @0 :Text;
    name            @1 :Text;
    matchConfidence @2 :Float32;
    detectedAt      @3 :TimeStamp;
    details         @4 :Data;
}

struct SystemCheckResults {
    memoryScan      @0 :MemoryScanResult;
    processScan     @1 :ProcessScanResult;
    fileScan        @2 :FileScanResult;
    driverScan      @3 :DriverScanResult;
}

struct MemoryScanResult {
    suspiciousRegions @0 :UInt32;
    injectedCode    @1 :Bool;
    hooksDetected   @2 :Bool;
}

struct ProcessScanResult {
    suspiciousProcesses @0 :List(SuspiciousProcess);
    unauthorizedInjects @1 :UInt32;
}

struct SuspiciousProcess {
    processName     @0 :Text;
    confidence      @1 :Float32;
    reason          @2 :Text;
}

struct FileScanResult {
    modifiedFiles   @0 :List(ModifiedFile);
    cheatFiles      @1 :List(CheatFile);
}

struct ModifiedFile {
    fileName        @0 :Text;
    expectedHash    @1 :Data;
    actualHash      @2 :Data;
}

struct CheatFile {
    fileName        @0 :Text;
    cheatType       @1 :CheatType;
    confidence      @2 :Float32;
}

struct DriverScanResult {
    unauthorizedDrivers @0 :List(UnauthorizedDriver);
    kernelHooks     @1 :Bool;
}

struct UnauthorizedDriver {
    driverName      @0 :Text;
    description     @1 :Text;
}

struct BehaviorAnomaly {
    anomalyType     @0 :AnomalyType;
    value           @1 :Float32;
    expectedRange   @2 :Data;        # Serialized expected range
    confidence      @3 :Float32;
}

enum AnomalyType {
    reactionTime    @0;
    accuracy        @1;
    movementPattern @2;
    decisionMaking  @3;
    resourceUsage   @4;
    networkPattern  @5;
}

enum CheatType {
    aimAssist       @0;
    wallhack        @1;
    speedhack       @2;
    teleport        @3;
    esp             @4;  # Extra sensory perception
    radar           @5;
    triggerbot      @6;
    bunnyhop        @7;
    noRecoil        @8;
    itemSpawn       @9;
    godMode         @10;
    lagSwitch       @11;
}

enum ReportStatus {
    pending         @0;
    reviewing       @1;
    confirmed       @2;
    falsePositive   @3;
    inconclusive    @4;
}

# Security Service Interface
interface SecurityService {
    # Incident Management
    reportIncident @0 (incident :SecurityIncident, signature :Signature) -> (incidentId :AmpId);
    updateIncident @1 (incidentId :AmpId, incident :SecurityIncident, signature :Signature) -> ();
    resolveIncident @2 (incidentId :AmpId, resolution :Resolution, signature :Signature) -> ();
    
    # Player Security
    getBehaviorProfile @3 (playerId :AmpId) -> (profile :BehaviorProfile);
    applySecurityAction @4 (playerId :AmpId, action :SecurityAction, duration :UInt64, reason :Text, signature :Signature) -> ();
    appealAction @5 (actionId :AmpId, appeal :Appeal, signature :Signature) -> (accepted :Bool);
    
    # Fraud Detection
    detectFraud @6 (matchId :AmpId, data :FraudDetectionData) -> (riskScore :Float32, indicators :List(FraudIndicator));
    getFraudPatterns @7 (gameId :AmpId) -> (patterns :List(FraudPattern));
    
    # Anti-Cheat
    submitAntiCheatReport @8 (report :AntiCheatReport, signature :Signature) -> (reportId :AmpId);
    reviewAntiCheatReport @9 (reportId :AmpId, review :Review, signature :Signature) -> ();
    
    # Analytics
    getSecurityStats @10 (timeRange :Match.TimeRange) -> (stats :SecurityStatistics);
}

struct FraudDetectionData {
    matchData       @0 :Data;        # Match replay/transcript
    playerData      @1 :List(PlayerSecurityData);
    networkData     @2 :NetworkData;
    systemData      @3 :SystemData;
}

struct PlayerSecurityData {
    playerId        @0 :AmpId;
    gameplayMetrics @1 :Data;
    behaviorMetrics @2 :Data;
    economicMetrics @3 :Data;
}

struct NetworkData {
    latencyHistory  @0 :List(UInt32);
    packetLoss      @1 :List(Float32);
    jitterHistory   @2 :List(UInt32);
}

struct SystemData {
    hardwareInfo    @0 :Data;
    runningProcesses @1 :List(Text);
    fileHashes      @2 :List(Data);
}

struct Appeal {
    appealId        @0 :AmpId;
    actionId        @1 :AmpId;
    reason          @2 :Text;
    evidence        @3 :Data;
    submittedAt     @4 :TimeStamp;
}

struct Review {
    reviewerId      @0 :AmpId;
    decision        @1 :ReviewDecision;
    notes           @2 :Text;
    evidence        @3 :Data;        # Supporting evidence
}

enum ReviewDecision {
    uphold          @0;
    overturn        @1;
    reduce          @2;  # Reduce severity/duration
    escalate        @3;
}


struct SecurityStatistics {
    totalIncidents  @0 :UInt64;
    resolvedIncidents @1 :UInt64;
    falsePositives  @2 :UInt64;
    avgResolutionTime @3 :UInt64;
    categoryBreakdown @4 :List(CategoryStats);
    severityBreakdown @5 :List(SeverityStats);
}

struct CategoryStats {
    category        @0 :FraudCategory;
    count           @1 :UInt64;
    percentage      @2 :Float32;
}

struct SeverityStats {
    severity        @0 :SeverityLevel;
    count           @1 :UInt64;
    percentage      @2 :Float32;
}