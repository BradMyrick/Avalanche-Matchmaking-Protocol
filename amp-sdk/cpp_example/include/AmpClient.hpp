#pragma once

#include "amp_telemetry.capnp.h"
#include "game_core.capnp.h"
#include "match.capnp.h"
#include "service.capnp.h"
#include <capnp/ez-rpc.h>
#include <future>
#include <memory>
#include <string>
#include <vector>

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
  AmpClient(const std::string &address);
  ~AmpClient();

  // Connects to the matchmaker and performs the initial login handshake
  bool connect(const std::vector<uint8_t> &playerSignature);

  // Requests a match. This blocks until a match is found.
  MatchAssignment requestMatch(const std::string &gameId);

  // Submits the outcome of a match and blocks until the verifier signature is
  // received. Operations using MatchSession capabilities
  void emitGameEvent(const std::string &eventType);
  void emitTelemetry(uint8_t eventTypeEnum, uint64_t timestamp);
  VerifierResult submitOutcome(const std::string &matchId, uint8_t outcome,
                               const std::vector<uint8_t> &transcriptHash);

private:
  std::string serverAddress;
  std::unique_ptr<capnp::EzRpcClient> rpcClient;

  // Capabilities
  GameSessionService::Client gameSessionService = nullptr;
  UserSession::Client userSession = nullptr;
  MatchSession::Client matchSession = nullptr;
  bool hasMatchSession = false;
};

} // namespace amp
