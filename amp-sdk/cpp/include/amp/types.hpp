#pragma once
#include <string>
#include <vector>
#include <cstdint>

namespace amp {

/// Player profile data used during registration and matchmaking.
struct PlayerProfileData {
    /// Unique player identifier.
    std::string player_id;
    /// Human-readable display name.
    std::string display_name;
    /// On-chain wallet address bytes.
    std::vector<uint8_t> wallet_address;
    /// Global matchmaking rating.
    float global_mmr;
    /// Preferred role or position in-game.
    std::string preferred_role;
    /// Geographic region for latency-based matching.
    std::string region;
};

/// Input for a matchmaking request.
struct MatchRequest {
    /// Identifier of the game to match for.
    std::string game_id;
    /// Identifier of the rule set to apply.
    std::string ruleset_id;
    /// Identifier of the requesting player.
    std::string player_id;
    /// Current matchmaking rating of the player.
    float mmr;
    /// Geographic region for latency-based matching.
    std::string region;
};

/// Result of a successful match assignment.
struct MatchResult {
    /// Unique identifier of the assigned match.
    std::string match_id;
    /// Quality score of the match (0.0–1.0).
    float quality;
    /// Identifier of the matched opponent.
    std::string opponent_id;
};

/// Contains the verifier's cryptographic signature over the match outcome.
struct VerifierResult {
    /// Identifier of the match this result covers.
    std::string match_id;
    /// Cryptographic signature bytes from the verifier.
    std::vector<uint8_t> signature;
};

} // namespace amp
