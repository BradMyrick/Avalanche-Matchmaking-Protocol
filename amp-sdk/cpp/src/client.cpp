#include "amp/client.hpp"
#include <kj/async.h>
#include <iostream>

namespace amp {

AMPClient::~AMPClient() = default;

void AMPClient::connect(const std::string& address) {
    rpc_client_ = std::make_unique<capnp::EzRpcClient>(address);
    game_session_service_ = rpc_client_->getMain<GameSessionService>();
}

bool AMPClient::login(uint64_t game_id, const std::vector<uint8_t>& signature) {
    try {
        auto req = game_session_service_.loginRequest();
        req.setGameId(game_id);
        kj::ArrayPtr<const kj::byte> sig_bytes(signature.data(), signature.size());
        req.setSignedChallenge(sig_bytes);
        auto response = req.send().wait(rpc_client_->getWaitScope());
        user_session_ = response.getSession();
        return true;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP Login Error: " << e.getDescription().cStr() << std::endl;
        return false;
    }
}

std::string AMPClient::create_profile(const PlayerProfileData& profile) {
    return profile.player_id;
}

MatchResult AMPClient::request_match(const MatchRequest& request) {
    try {
        auto req = user_session_.requestMatchRequest();
        auto gm_req = req.initReq();

        kj::ArrayPtr<const kj::byte> game_id_bytes(
            reinterpret_cast<const kj::byte*>(request.game_id.data()),
            request.game_id.size());
        gm_req.setGameId(game_id_bytes);

        gm_req.setRulesType("standard");
        auto p_info = gm_req.initPlayerInfo();
        kj::ArrayPtr<const kj::byte> player_id_bytes(
            reinterpret_cast<const kj::byte*>(request.player_id.data()),
            request.player_id.size());
        p_info.setPlayerId(player_id_bytes);
        p_info.setDisplayName("CppPlayer");
        p_info.setElo(Elo::UNRANKED);
        p_info.setRegion(Region::NA);
        p_info.setPlayerWallet(kj::ArrayPtr<const kj::byte>());

        auto p_stake = gm_req.initStake();
        p_stake.setPayerWallet(kj::ArrayPtr<const kj::byte>());
        p_stake.setFeeToken(kj::ArrayPtr<const kj::byte>());
        p_stake.setAuthSpend(0);

        gm_req.setOptionalConfig(kj::ArrayPtr<const kj::byte>());

        auto response = req.send().wait(rpc_client_->getWaitScope());
        auto assignment = response.getAssignment();

        MatchResult result;
        auto match_id_data = assignment.getMatchId();
        result.match_id = std::string(
            reinterpret_cast<const char*>(match_id_data.begin()),
            match_id_data.size());

        auto opponents = assignment.getOpponents();
        if (opponents.size() > 0) {
            auto opp_id = opponents[0].getPlayerId();
            result.opponent_id = std::string(
                reinterpret_cast<const char*>(opp_id.begin()),
                opp_id.size());
        }

        result.quality = assignment.getMatchQuality();

        match_session_ = response.getSession();
        has_match_session_ = true;
        return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP RequestMatch Error: " << e.getDescription().cStr() << std::endl;
        return {"", 0.0f, ""};
    }
}

void AMPClient::emit_game_event(const std::string& event_type) {
    if (!has_match_session_) return;
    try {
        auto req = match_session_.emitGameEventRequest();
        auto event = req.initEvent();
        event.setEventId(0);
        event.setTimestamp(0);
        event.setTriggeredBy(kj::arrayPtr(reinterpret_cast<const kj::byte*>("p1"), 2));
        event.setEventType("move");
        event.setEventData(kj::arrayPtr(
            reinterpret_cast<const kj::byte*>(event_type.data()),
            event_type.size()));
        req.send();
    } catch (...) {
    }
}

void AMPClient::emit_telemetry(uint8_t event_type_enum, uint64_t timestamp) {
    if (!has_match_session_) return;
    try {
        auto req = match_session_.emitTelemetryRequest();
        auto event = req.initEvent();
        event.setMatchId(kj::ArrayPtr<const kj::byte>());
        event.setTimestamp(timestamp);
        event.setEventType(static_cast<TelemetryEventType>(event_type_enum));
        event.setVerifierId(kj::ArrayPtr<const kj::byte>());
        event.setEventData(kj::ArrayPtr<const kj::byte>());
        req.send();
    } catch (...) {
    }
}

VerifierResult AMPClient::submit_outcome(const std::string& match_id, uint8_t outcome,
                                          const std::vector<uint8_t>& transcript_hash) {
    try {
        kj::ArrayPtr<const kj::byte> match_id_bytes(
            reinterpret_cast<const kj::byte*>(match_id.data()),
            match_id.size());

        if (!has_match_session_) {
            auto recon_req = user_session_.reconnectRequest();
            recon_req.setMatchId(match_id_bytes);
            auto recon_res = recon_req.send().wait(rpc_client_->getWaitScope());
            match_session_ = recon_res.getSession();
            has_match_session_ = true;
        }

        auto out_req = match_session_.submitOutcomeRequest();
        auto sub = out_req.initSubmission();
        sub.setMatchId(match_id_bytes);

        auto match_outcome = sub.initOutcome();
        match_outcome.setType(outcome == 0 ? OutcomeType::WIN : OutcomeType::UNKNOWN);
        auto scores = match_outcome.initScores(2);
        scores.set(0, 1);
        scores.set(1, 0);
        match_outcome.setVictor(outcome);
        match_outcome.setMetadata(kj::ArrayPtr<const kj::byte>());

        if (!transcript_hash.empty()) {
            kj::ArrayPtr<const kj::byte> hash_bytes(transcript_hash.data(),
                                                     transcript_hash.size());
            sub.setReplayHash(hash_bytes);
        }

        auto out_res = out_req.send().wait(rpc_client_->getWaitScope());
        auto sig = out_res.getSignature();

        VerifierResult result;
        result.match_id = match_id;
        result.signature.assign(sig.begin(), sig.end());
        return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP SubmitOutcome Error: " << e.getDescription().cStr() << std::endl;
        return {"", {}};
    }
}

} // namespace amp
