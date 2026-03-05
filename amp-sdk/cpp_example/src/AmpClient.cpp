#include "AmpClient.hpp"
#include <iostream>
#include <kj/async.h>

namespace amp {

AmpClient::AmpClient(const std::string &address) : serverAddress(address) {}

AmpClient::~AmpClient() {}

bool AmpClient::connect(const std::vector<uint8_t> &playerSignature) {
  try {
    rpcClient = std::make_unique<capnp::EzRpcClient>(serverAddress);
    gameSessionService = rpcClient->getMain<GameSessionService>();

    auto req = gameSessionService.loginRequest();

    kj::ArrayPtr<const kj::byte> sigBytes(playerSignature.data(),
                                          playerSignature.size());
    req.setSignedChallenge(sigBytes);

    auto promise = req.send();
    auto response = promise.wait(rpcClient->getWaitScope());

    userSession = response.getSession();
    return true;
  } catch (const kj::Exception &e) {
    std::cerr << "AMP Connect Error: " << e.getDescription().cStr()
              << std::endl;
    return false;
  }
}

MatchAssignment AmpClient::requestMatch(const std::string &gameId) {
  try {
    auto req = userSession.requestMatchRequest();
    auto gmReq = req.initReq();

    kj::ArrayPtr<const kj::byte> gameIdBytes(
        reinterpret_cast<const kj::byte *>(gameId.data()), gameId.size());
    gmReq.setGameId(gameIdBytes);

    gmReq.setRulesType("standard");
    auto pInfo = gmReq.initPlayerInfo();
    pInfo.setPlayerId(kj::arrayPtr(reinterpret_cast<const kj::byte*>("p1"), 2));
    pInfo.setDisplayName("CppPlayer");
    pInfo.setElo(Elo::UNRANKED);
    pInfo.setRegion(Region::NA);
    pInfo.setPlayerWallet(kj::ArrayPtr<const kj::byte>());

    auto pStake = gmReq.initStake();
    pStake.setPayerWallet(kj::ArrayPtr<const kj::byte>());
    pStake.setFeeToken(kj::ArrayPtr<const kj::byte>());
    pStake.setAuthSpend(0);

    gmReq.setOptionalConfig(kj::ArrayPtr<const kj::byte>());

    auto response = req.send().wait(rpcClient->getWaitScope());
    auto assignment = response.getAssignment();

    MatchAssignment result;
    auto matchIdData = assignment.getMatchId();
    result.matchId =
        std::string(reinterpret_cast<const char *>(matchIdData.begin()),
                    matchIdData.size());

    // Save the MatchSession Capability explicitly
    matchSession = response.getSession();
    hasMatchSession = true;
    return result;
  } catch (const kj::Exception &e) {
    std::cerr << "AMP RequestMatch Error: " << e.getDescription().cStr()
              << std::endl;
    return {"", ""};
  }
}

void AmpClient::emitGameEvent(const std::string &eventType) {
  if (!hasMatchSession)
    return;
  try {
    auto req = matchSession.emitGameEventRequest();
    auto event = req.initEvent();
    event.setEventId(0);
    event.setTimestamp(0);
    event.setTriggeredBy(kj::arrayPtr(reinterpret_cast<const kj::byte*>("p1"), 2));
    event.setEventType("move");
    event.setEventData(kj::arrayPtr(reinterpret_cast<const kj::byte*>(eventType.data()), eventType.size()));
    auto promise = req.send();
    // Here we just fire and forget or could wait
  } catch (...) {
  }
}

void AmpClient::emitTelemetry(uint8_t eventTypeEnum, uint64_t timestamp) {
  if (!hasMatchSession)
    return;
  try {
    auto req = matchSession.emitTelemetryRequest();
    auto event = req.initEvent();
    event.setMatchId(kj::ArrayPtr<const kj::byte>());
    event.setTimestamp(timestamp);
    event.setEventType(static_cast<TelemetryEventType>(eventTypeEnum));
    event.setVerifierId(kj::ArrayPtr<const kj::byte>());
    event.setEventData(kj::ArrayPtr<const kj::byte>());
    auto promise = req.send();
    // Fire and forget, or collect promise
  } catch (...) {
  }
}

VerifierResult
AmpClient::submitOutcome(const std::string &matchId, uint8_t outcome,
                         const std::vector<uint8_t> &transcriptHash) {
  try {
    kj::ArrayPtr<const kj::byte> matchIdBytes(
        reinterpret_cast<const kj::byte *>(matchId.data()), matchId.size());

    if (!hasMatchSession) {
      auto reconReq = userSession.reconnectRequest();
      reconReq.setMatchId(matchIdBytes);
      auto reconRes = reconReq.send().wait(rpcClient->getWaitScope());
      matchSession = reconRes.getSession();
      hasMatchSession = true;
    }

    // Submit the outcome
    auto outReq = matchSession.submitOutcomeRequest();
    auto sub = outReq.initSubmission();
    sub.setMatchId(matchIdBytes);

    auto matchOutcome = sub.initOutcome();
    matchOutcome.setType(outcome == 0 ? OutcomeType::WIN : OutcomeType::UNKNOWN);
    auto scores = matchOutcome.initScores(2);
    scores.set(0, 1);
    scores.set(1, 0);
    matchOutcome.setVictor(outcome);
    matchOutcome.setMetadata(kj::ArrayPtr<const kj::byte>());

    if (!transcriptHash.empty()) {
      kj::ArrayPtr<const kj::byte> hashBytes(transcriptHash.data(),
                                             transcriptHash.size());
      sub.setReplayHash(hashBytes);
    }

    auto outRes = outReq.send().wait(rpcClient->getWaitScope());
    auto sig = outRes.getSignature();

    VerifierResult result;
    result.matchId = matchId;
    result.signature.assign(sig.begin(), sig.end());

    return result;
  } catch (const kj::Exception &e) {
    std::cerr << "AMP SubmitOutcome Error: " << e.getDescription().cStr()
              << std::endl;
    return {"", {}};
  }
}

} // namespace amp
