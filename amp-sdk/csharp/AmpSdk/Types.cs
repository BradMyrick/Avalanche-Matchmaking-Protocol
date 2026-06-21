using System;

namespace AmpSdk {

/// <summary>
/// Player profile data used during registration and matchmaking.
/// </summary>
public class PlayerProfileData {
    /// <summary>Unique player identifier.</summary>
    public string PlayerId { get; set; } = "";
    /// <summary>Display name shown in-game.</summary>
    public string DisplayName { get; set; } = "";
    /// <summary>On-chain wallet address of the player.</summary>
    public byte[] WalletAddress { get; set; } = Array.Empty<byte>();
    /// <summary>Global matchmaking rating.</summary>
    public float GlobalMmr { get; set; }
    /// <summary>Preferred role selection for role-based matchmaking.</summary>
    public string PreferredRole { get; set; } = "";
    /// <summary>Geographic region for latency-based matching.</summary>
    public string Region { get; set; } = "";
}

/// <summary>
/// Input for a matchmaking request.
/// </summary>
public class MatchRequest {
    /// <summary>Identifier of the game to matchmake for.</summary>
    public string GameId { get; set; } = "";
    /// <summary>Identifier of the ruleset to apply.</summary>
    public string RulesetId { get; set; } = "";
    /// <summary>Identifier of the player requesting a match.</summary>
    public string PlayerId { get; set; } = "";
    /// <summary>Current matchmaking rating of the player.</summary>
    public float Mmr { get; set; }
    /// <summary>Geographic region preference.</summary>
    public string Region { get; set; } = "";
}

/// <summary>
/// Result of a successful match assignment.
/// </summary>
public class MatchResult {
    /// <summary>Unique identifier for the matched session.</summary>
    public string MatchId { get; set; } = "";
    /// <summary>Quality score of the match (0.0 to 1.0).</summary>
    public float Quality { get; set; }
    /// <summary>Identifier of the opponent player.</summary>
    public string OpponentId { get; set; } = "";
}

/// <summary>
/// Contains the verifier's cryptographic signature over the match outcome.
/// </summary>
public class VerifierResult {
    /// <summary>Identifier of the match this signature covers.</summary>
    public string MatchId { get; set; } = "";
    /// <summary>Cryptographic signature produced by the verifier.</summary>
    public byte[] Signature { get; set; } = Array.Empty<byte>();
}

}
