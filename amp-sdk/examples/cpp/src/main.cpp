#include "amp/client.hpp"
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
  amp::AMPClient client2;
  client2.connect(g_addr);
  if (client2.authenticate(0, [](const std::vector<uint8_t>& challenge) {
      return getFujiSignature(); 
  })) {
    std::cout << "[Player B] Connected to Matchmaker. Requesting match..."
              << std::endl;
    amp::MatchRequest req;
    req.game_id = GAME_ID;
    req.player_id = "p2";
    client2.request_match(req);
  }
}

int main(int argc, char** argv) {
  if (argc > 1) g_addr = argv[1];
  std::cout << "Starting AMP C++ Native Engine SDK Test on " << g_addr << "..." << std::endl;

  amp::AMPClient client;
  std::cout << "[Player A] Connecting to Matchmaker..." << std::endl;
  client.connect(g_addr);
  if (!client.authenticate(0, [](const std::vector<uint8_t>& challenge) {
      return getFujiSignature();
  })) {
    std::cerr << "Failed to connect to matchmaker." << std::endl;
    return 1;
  }
  std::cout << "[PlayerA] Connected & Logged in to AMP matchmaker."
            << std::endl;

  std::cout << "Spawning Player B thread..." << std::endl;
  std::thread playerBThread(runOpponent);
  playerBThread.detach();

  std::cout << "Requesting match in game " << GAME_ID << std::endl;
  amp::MatchRequest req;
  req.game_id = GAME_ID;
  req.player_id = "p1";
  auto assignment = client.request_match(req);
  if (assignment.match_id.empty()) {
    std::cerr << "Failed to get match assignment." << std::endl;
    return 1;
  }
  std::cout << "Got MatchAssignment! Match ID: " << assignment.match_id
            << std::endl;

  std::cout << "Simulating game..." << std::endl;
  // Emit match Joined telemetry (enum matchJoined = 1)
  client.emit_telemetry(1, 1000200);

  // flip a coin
  int coinFlip = rand() % 2;
  client.emit_game_event(coinFlip == 0 ? "PlayerA_Scored" : "PlayerB_Scored");

  // submit outcome

  std::cout << "Submitting outcome to verifier..." << std::endl;
  if (coinFlip == 0) {

    std::cout << "Winner Player A" << std::endl;

  } else {

    std::cout << "Winner Player B" << std::endl;
  }

  std::vector<uint8_t> noTranscript;
  auto verifierRes =
      client.submit_outcome(assignment.match_id, coinFlip, noTranscript);

  if (verifierRes.signature.empty()) {
    std::cerr << "Failed to get verifier signature." << std::endl;
    return 1;
  }

  printHex("Verifier provided signature: ", verifierRes.signature);

  std::cout << "C++ Native Client SDK test successful. Exiting cleanly."
            << std::endl;
  exit(0);
}
