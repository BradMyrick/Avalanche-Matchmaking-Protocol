#pragma once
#include "types.hpp"
#include "amp_telemetry.capnp.h"
#include "game_core.capnp.h"
#include "match.capnp.h"
#include "service.capnp.h"
#include "player_profile.capnp.h"
#include <capnp/ez-rpc.h>
#include <memory>
#include <string>
#include <vector>
#include <functional>

namespace amp {

/// Main client for connecting to an AMP server via Cap'n Proto RPC.
class AMPClient {
public:
    AMPClient() = default;
    ~AMPClient();

    /// Connects to the AMP server at the given address (host:port).
    void connect(const std::string& address);

    /// Authenticates with the AMP server using a game ID and signed challenge.
    bool login(uint64_t game_id, const std::vector<uint8_t>& signature);

    /// Authenticates using a high-level callback for signing challenges.
    bool authenticate(uint64_t game_id, std::function<std::vector<uint8_t>(const std::vector<uint8_t>&)> sign_callback);

    /// Creates or retrieves a player profile on the AMP server.
    std::string create_profile(const PlayerProfileData& profile);

    /// Submits a match request and blocks until a match is assigned.
    MatchResult request_match(const MatchRequest& request);

    /// Emits a typed game event during an active match.
    void emit_game_event(const std::string& event_type);

    /// Emits a telemetry event with the given type and timestamp.
    void emit_telemetry(uint8_t event_type_enum, uint64_t timestamp);

    /// Submits the final match outcome and returns the verifier's signed result.
    VerifierResult submit_outcome(const std::string& match_id, uint8_t outcome, const std::vector<uint8_t>& transcript_hash);

private:
    std::unique_ptr<capnp::EzRpcClient> rpc_client_;
    GameSessionService::Client game_session_service_ = nullptr;
    UserSession::Client user_session_ = nullptr;
    MatchSession::Client match_session_ = nullptr;
    bool has_match_session_ = false;
};

} // namespace amp
