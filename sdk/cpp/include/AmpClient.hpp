#pragma once

#include <string>
#include <vector>
#include <memory>
#include <future>
#include <capnp/ez-rpc.h>
#include "schemas/service.capnp.h"
#include "schemas/match.capnp.h"

namespace amp {

struct MatchAssignment {
    std::string matchId;
    std::string opponentId;
};

struct VerifierResult {
    std::string matchId;
    std::vector<uint8_t> signature;
};

class AmpClient {
public:
    AmpClient(const std::string& address);
    ~AmpClient();

    // Connects to the matchmaker and performs the initial login handshake
    bool connect(const std::vector<uint8_t>& playerSignature);

    // Requests a match. This blocks until a match is found.
    MatchAssignment requestMatch(const std::string& gameId);

    // Submits the outcome of a match and blocks until the verifier signature is received.
    VerifierResult submitOutcome(const std::string& matchId, uint8_t outcome, const std::vector<uint8_t>& transcriptHash);

private:
    std::string serverAddress;
    std::unique_ptr<capnp::EzRpcClient> rpcClient;
    
    // Capabilities
    GameSessionService::Client gameSessionService = nullptr;
    UserSession::Client userSession = nullptr;
};

} // namespace amp
