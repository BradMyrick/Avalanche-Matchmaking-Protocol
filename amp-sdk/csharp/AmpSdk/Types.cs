using System;

namespace AmpSdk {

/// <summary>
/// Player profile data used during registration and matchmaking.
/// </summary>
public record PlayerProfileData {
    /// <summary>Unique player identifier.</summary>
    public string PlayerId { get; init; } = "";
    /// <summary>Display name shown in-game.</summary>
    public string DisplayName { get; init; } = "";
    /// <summary>On-chain wallet address of the player.</summary>
    public byte[] WalletAddress { get; init; } = Array.Empty<byte>();
    /// <summary>Global matchmaking rating.</summary>
    public float GlobalMmr { get; init; }
    /// <summary>Preferred role selection for role-based matchmaking.</summary>
    public string PreferredRole { get; init; } = "";
    /// <summary>Geographic region for latency-based matching.</summary>
    public string Region { get; init; } = "";
}

/// <summary>
/// Input for a matchmaking request.
/// </summary>
public record MatchRequest {
    /// <summary>Identifier of the game to matchmake for.</summary>
    public string GameId { get; init; } = "";
    /// <summary>Identifier of the ruleset to apply.</summary>
    public string RulesetId { get; init; } = "";
    /// <summary>Identifier of the player requesting a match.</summary>
    public string PlayerId { get; init; } = "";
    /// <summary>Current matchmaking rating of the player.</summary>
    public float Mmr { get; init; }
    /// <summary>Geographic region preference.</summary>
    public string Region { get; init; } = "";
}

/// <summary>
/// Result of a successful match assignment.
/// </summary>
public record MatchResult {
    /// <summary>Unique identifier for the matched session.</summary>
    public string MatchId { get; init; } = "";
    /// <summary>Quality score of the match (0.0 to 1.0).</summary>
    public float Quality { get; init; }
    /// <summary>Identifier of the opponent player.</summary>
    public string OpponentId { get; init; } = "";
}

/// <summary>
/// Contains the verifier's cryptographic signature over the match outcome.
/// </summary>
public record VerifierResult {
    /// <summary>Identifier of the match this signature covers.</summary>
    public string MatchId { get; init; } = "";
    /// <summary>Cryptographic signature produced by the verifier.</summary>
    public byte[] Signature { get; init; } = Array.Empty<byte>();
}

}
