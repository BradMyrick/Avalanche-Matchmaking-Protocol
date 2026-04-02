#pragma once
#include <string>
#include <vector>
#include <cstdint>

namespace amp {

struct PlayerProfileData {
    std::string player_id;
    std::string display_name;
    std::vector<uint8_t> wallet_address;
    float global_mmr;
    std::string preferred_role;
    std::string region;
};

struct MatchRequest {
    std::string game_id;
    std::string ruleset_id;
    std::string player_id;
    float mmr;
    std::string region;
};

struct MatchResult {
    std::string match_id;
    float quality;
    std::string opponent_id;
};

} // namespace amp
