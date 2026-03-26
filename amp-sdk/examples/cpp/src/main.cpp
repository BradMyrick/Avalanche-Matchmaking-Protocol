#include "AmpClient.hpp"
#include <iomanip>
#include <iostream>
#include <vector>
#include <string>
#include <thread>
#include <unistd.h>

const std::string GAME_ID = "0x6767676767676767";

// Utility to print hex safely
void printHex(const std::string &prefix, const std::vector<uint8_t> &data) {
  std::cout << prefix << "0x";
  for (auto b : data) {
    std::cout << std::hex << std::setw(2) << std::setfill('0') << (int)b;
  }
  std::cout << std::dec << std::endl;
}

std::vector<uint8_t> getFujiSignature() {
    std::string hex = "24e2943427fa35e48c01ba764c271a9e76d295142704648b171e8cb5272a279773a0206586e565b1fed8a916e945a39868463654f752bf6bcd6843ab06bff39e1c";
    std::vector<uint8_t> bytes;
    for (unsigned int i = 0; i < hex.length(); i += 2) {
        std::string byteString = hex.substr(i, 2);
        uint8_t byte = (uint8_t) strtol(byteString.c_str(), NULL, 16);
        bytes.push_back(byte);
    }
    return bytes;
}

static std::string g_addr = "127.0.0.1:50051";

void runOpponent() {
  usleep(500000); // Wait half a second for player A to connect
  amp::AmpClient client2(g_addr);
  std::vector<uint8_t> dummySig2 = getFujiSignature(); 
  if (client2.connect(dummySig2)) {
    std::cout << "[Player B] Connected to Matchmaker. Requesting match..."
              << std::endl;
    client2.requestMatch(GAME_ID);
  }
}

int main(int argc, char** argv) {
  if (argc > 1) g_addr = argv[1];
  std::cout << "Starting AMP C++ Native Engine SDK Test on " << g_addr << "..." << std::endl;

  amp::AmpClient client(g_addr);
  std::cout << "[Player A] Connecting to Matchmaker..." << std::endl;
  std::vector<uint8_t> dummySig = getFujiSignature();
  if (!client.connect(dummySig)) {
    std::cerr << "Failed to connect to matchmaker." << std::endl;
    return 1;
  }
  std::cout << "[PlayerA] Connected & Logged in to AMP matchmaker."
            << std::endl;

  std::cout << "Spawning Player B thread..." << std::endl;
  std::thread playerBThread(runOpponent);
  playerBThread.detach();

  std::cout << "Requesting match in game " << GAME_ID << std::endl;
  auto assignment = client.requestMatch(GAME_ID);
  if (assignment.matchId.empty()) {
    std::cerr << "Failed to get match assignment." << std::endl;
    return 1;
  }
  std::cout << "Got MatchAssignment! Match ID: " << assignment.matchId
            << std::endl;

  std::cout << "Simulating game..." << std::endl;
  // Emit match Joined telemetry (enum matchJoined = 1)
  client.emitTelemetry(1, 1000200);

  // flip a coin
  int coinFlip = rand() % 2;
  client.emitGameEvent(coinFlip == 0 ? "PlayerA_Scored" : "PlayerB_Scored");

  // submit outcome

  std::cout << "Submitting outcome to verifier..." << std::endl;
  if (coinFlip == 0) {

    std::cout << "Winner Player A" << std::endl;

  } else {

    std::cout << "Winner Player B" << std::endl;
  }

  std::vector<uint8_t> noTranscript;
  auto verifierRes =
      client.submitOutcome(assignment.matchId, coinFlip, noTranscript);

  if (verifierRes.signature.empty()) {
    std::cerr << "Failed to get verifier signature." << std::endl;
    return 1;
  }

  printHex("Verifier provided signature: ", verifierRes.signature);

  std::cout << "C++ Native Client SDK test successful. Exiting cleanly."
            << std::endl;
  exit(0);
}
