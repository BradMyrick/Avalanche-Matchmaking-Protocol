#include <iostream>
#include <iomanip>
#include "AmpClient.hpp"

#include <thread>
#include <unistd.h>

// Utility to print hex safely
void printHex(const std::string& prefix, const std::vector<uint8_t>& data) {
    std::cout << prefix << "0x";
    for(auto b : data) {
        std::cout << std::hex << std::setw(2) << std::setfill('0') << (int)b;
    }
    std::cout << std::dec << std::endl;
}

void runOpponent() {
    usleep(500000); // Wait half a second for player A to connect
    amp::AmpClient client2("127.0.0.1:50051");
    std::vector<uint8_t> dummySig2 = {0x05, 0x06};
    if (client2.connect(dummySig2)) {
        std::cout << "[Player B] Connected to Matchmaker. Requesting match..." << std::endl;
        client2.requestMatch("0");
    }
}

int main() {
    std::cout << "Starting AMP C++ Native Engine SDK Test..." << std::endl;

    amp::AmpClient client("127.0.0.1:50051");

    std::vector<uint8_t> dummySig = {0x01, 0x02, 0x03, 0x04};
    if (!client.connect(dummySig)) {
        std::cerr << "Failed to connect to matchmaker." << std::endl;
        return 1;
    }
    std::cout << "Connected & Logged in to AMP matchmaker." << std::endl;

    std::cout << "Spawning Player B thread..." << std::endl;
    std::thread playerBThread(runOpponent);
    playerBThread.detach();

    std::cout << "Requesting match in game '0'..." << std::endl;
    auto assignment = client.requestMatch("0");
    if (assignment.matchId.empty()) {
        std::cerr << "Failed to get match assignment." << std::endl;
        return 1;
    }
    std::cout << "Got MatchAssignment! Match ID: " << assignment.matchId << std::endl;

    std::cout << "Simulating game..." << std::endl;
    
    // Player wins
    std::cout << "Submitting outcome to verifier..." << std::endl;
    std::vector<uint8_t> noTranscript;
    auto verifierRes = client.submitOutcome(assignment.matchId, 1, noTranscript);
    
    if (verifierRes.signature.empty()) {
        std::cerr << "Failed to get verifier signature." << std::endl;
        return 1;
    }

    printHex("Verifier provided signature: ", verifierRes.signature);

    std::cout << "C++ Native Client MVP successful. Exiting cleanly." << std::endl;
    exit(0);
}
