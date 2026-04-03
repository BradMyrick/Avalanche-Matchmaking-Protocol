#pragma once
#include "types.hpp"
#include <capnp/rpc-twoparty.h>

namespace amp {

class AMPClient {
public:
    AMPClient(const std::string& rpc_url);
    ~AMPClient();

    bool connect(const std::string& player_id, uint64_t game_id);

    std::string create_profile(const PlayerProfileData& profile);
    MatchResult request_match(const MatchRequest& request);
    void submit_outcome(const std::string& match_id, uint8_t outcome, const std::vector<uint8_t>& transcript_hash);

private:
    std::string rpc_url_;
};

} // namespace amp
