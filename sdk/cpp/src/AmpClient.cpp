#include "AmpClient.hpp"
#include <iostream>
#include <kj/async.h>

namespace amp {

AmpClient::AmpClient(const std::string& address) : serverAddress(address) {
}

AmpClient::~AmpClient() {
}

bool AmpClient::connect(const std::vector<uint8_t>& playerSignature) {
    try {
        rpcClient = std::make_unique<capnp::EzRpcClient>(serverAddress);
        gameSessionService = rpcClient->getMain<GameSessionService>();

        auto req = gameSessionService.loginRequest();
        
        kj::ArrayPtr<const kj::byte> sigBytes(playerSignature.data(), playerSignature.size());
        req.setSignedChallenge(sigBytes);

        auto promise = req.send();
        auto response = promise.wait(rpcClient->getWaitScope());
        
        userSession = response.getSession();
        return true;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP Connect Error: " << e.getDescription().cStr() << std::endl;
        return false;
    }
}

MatchAssignment AmpClient::requestMatch(const std::string& gameId) {
    try {
        auto req = userSession.requestMatchRequest();
        auto gmReq = req.initReq();
        
        kj::ArrayPtr<const kj::byte> gameIdBytes(reinterpret_cast<const kj::byte*>(gameId.data()), gameId.size());
        gmReq.setGameId(gameIdBytes);

        auto response = req.send().wait(rpcClient->getWaitScope());
        auto assignment = response.getAssignment();
        
        MatchAssignment result;
        auto matchIdData = assignment.getMatchId();
        result.matchId = std::string(reinterpret_cast<const char*>(matchIdData.begin()), matchIdData.size());
        
        // We do not store the matchSession capability in this simple synchronous example,
        // but it is available as response.getSession() if we needed to keep it.
        return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP RequestMatch Error: " << e.getDescription().cStr() << std::endl;
        return {"", ""};
    }
}

VerifierResult AmpClient::submitOutcome(const std::string& matchId, uint8_t outcome, const std::vector<uint8_t>& transcriptHash) {
    try {
        // Reconnect to get the match session capability
        auto reconReq = userSession.reconnectRequest();
        
        kj::ArrayPtr<const kj::byte> matchIdBytes(reinterpret_cast<const kj::byte*>(matchId.data()), matchId.size());
        reconReq.setMatchId(matchIdBytes);
        auto reconRes = reconReq.send().wait(rpcClient->getWaitScope());
        auto matchSession = reconRes.getSession();

        // Submit the outcome
        auto outReq = matchSession.submitOutcomeRequest();
        auto sub = outReq.initSubmission();
        sub.setMatchId(matchIdBytes);
        
        auto matchOutcome = sub.initOutcome();
        matchOutcome.setVictor(outcome);

        if (!transcriptHash.empty()) {
            kj::ArrayPtr<const kj::byte> hashBytes(transcriptHash.data(), transcriptHash.size());
            sub.setReplayHash(hashBytes);
        }

        auto outRes = outReq.send().wait(rpcClient->getWaitScope());
        auto sig = outRes.getSignature();
        
        VerifierResult result;
        result.matchId = matchId;
        result.signature.assign(sig.begin(), sig.end());
        
        return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP SubmitOutcome Error: " << e.getDescription().cStr() << std::endl;
        return {"", {}};
    }
}

} // namespace amp
