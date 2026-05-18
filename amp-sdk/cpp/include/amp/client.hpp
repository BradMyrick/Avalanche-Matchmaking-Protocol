#pragma once
#include "types.hpp"
#include "amp_telemetry.capnp.h"
#include "game_core.capnp.h"
#include "match.capnp.h"
#include "service.capnp.h"
#include <capnp/ez-rpc.h>
#include <memory>
#include <string>
#include <vector>

namespace amp {

class AMPClient {
public:
    AMPClient() = default;
    ~AMPClient();

    void connect(const std::string& address);
    bool login(uint64_t game_id, const std::vector<uint8_t>& signature);
    std::string create_profile(const PlayerProfileData& profile);
    MatchResult request_match(const MatchRequest& request);
    void emit_game_event(const std::string& event_type);
    void emit_telemetry(uint8_t event_type_enum, uint64_t timestamp);
    VerifierResult submit_outcome(const std::string& match_id, uint8_t outcome, const std::vector<uint8_t>& transcript_hash);

private:
    std::unique_ptr<capnp::EzRpcClient> rpc_client_;
    GameSessionService::Client game_session_service_ = nullptr;
    UserSession::Client user_session_ = nullptr;
    MatchSession::Client match_session_ = nullptr;
    bool has_match_session_ = false;
};

} // namespace amp
