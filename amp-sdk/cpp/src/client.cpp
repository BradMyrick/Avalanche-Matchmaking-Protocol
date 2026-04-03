#include "amp/client.hpp"

namespace amp {

AMPClient::AMPClient(const std::string& rpc_url) : rpc_url_(rpc_url) {
    // Initialization of network layers if necessary
}

AMPClient::~AMPClient() {
}

bool AMPClient::connect(const std::string& player_id, uint64_t game_id) {
    // In a production setup this initializes ez-rpc or twoparty network
    // auto& rpcSystem = network.rpcSystem();
    // auto bootstrap = rpcSystem.bootstrap(serverVatId).castAs<GameSessionService>();
    // auto req = bootstrap.loginRequest();
    // req.setGameId(game_id);
    // req.setSignedChallenge(capnp::Data::Reader(reinterpret_cast<const kj::byte*>(player_id.data()), player_id.size()));
    // auto result = req.send().wait(waitScope);
    // user_session_ = result.getSession();
    return true;
}

std::string AMPClient::create_profile(const PlayerProfileData& profile) {
    // auto req = user_session_.createOrUpdateProfileRequest();
    // req.setDisplayName(profile.display_name);
    // req.setRegion(profile.region);
    // auto result = req.send().wait(waitScope);
    return profile.player_id;
}

MatchResult AMPClient::request_match(const MatchRequest& request) {
    // auto req = user_session_.requestMatchRequest();
    // auto matchReq = req.initReq();
    // matchReq.setRuleSetId(request.ruleset_id);
    // auto result = req.send().wait(waitScope);
    // auto assignment = result.getAssignment();
    // match_session_ = result.getSession();
    return MatchResult{"match_pending", 1.0f, ""};
}

void AMPClient::submit_outcome(const std::string& match_id, uint8_t outcome, const std::vector<uint8_t>& transcript_hash) {
    // auto req = match_session_.submitOutcomeRequest();
    // auto sub = req.initSubmission();
    // sub.setMatchId(match_id);
    // auto out = sub.initOutcome();
    // out.setVictor(outcome);
    // req.send().wait(waitScope);
}

} // namespace amp
