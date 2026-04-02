using System;

namespace AmpSdk {

public record PlayerProfileData {
    public string PlayerId { get; init; } = "";
    public string DisplayName { get; init; } = "";
    public byte[] WalletAddress { get; init; } = Array.Empty<byte>();
    public float GlobalMmr { get; init; }
    public string PreferredRole { get; init; } = "";
    public string Region { get; init; } = "";
}

public record MatchRequest {
    public string GameId { get; init; } = "";
    public string RulesetId { get; init; } = "";
    public string PlayerId { get; init; } = "";
    public float Mmr { get; init; }
    public string Region { get; init; } = "";
}

public record MatchResult {
    public string MatchId { get; init; } = "";
    public float Quality { get; init; }
    public string OpponentId { get; init; } = "";
}

}
