#include <iostream>
#include <iomanip>
#include "AmpClient.hpp"

#include <thread>
#include <unistd.h>


const std::string GAME_ID = "0x6767676767676767";



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
    std::vector<uint8_t> dummySig2 = {0x05, 0x06, 0x07, 0x08};
    if (client2.connect(dummySig2)) {
        std::cout << "[Player B] Connected to Matchmaker. Requesting match..." << std::endl;
        client2.requestMatch(GAME_ID);
    }
}

int main() {
    std::cout << "Starting AMP C++ Native Engine SDK Test..." << std::endl;

    amp::AmpClient client("127.0.0.1:50051");
    std::cout << "[Player A] Connecting to Matchmaker..." << std::endl;
    std::vector<uint8_t> dummySig = {0x01, 0x02, 0x03, 0x04};
    if (!client.connect(dummySig)) {
        std::cerr << "Failed to connect to matchmaker." << std::endl;
        return 1;
    }
    std::cout << "[PlayerA] Connected & Logged in to AMP matchmaker." << std::endl;

    std::cout << "Spawning Player B thread..." << std::endl;
    std::thread playerBThread(runOpponent);
    playerBThread.detach();

    std::cout << "Requesting match in game " << GAME_ID << std::endl;
    auto assignment = client.requestMatch(GAME_ID);
    if (assignment.matchId.empty()) {
        std::cerr << "Failed to get match assignment." << std::endl;
        return 1;
    }
    std::cout << "Got MatchAssignment! Match ID: " << assignment.matchId << std::endl;

    std::cout << "Simulating game..." << std::endl;

    // flip a coin

    int coinFlip = rand() % 2;
    // submit outcome

    std::cout << "Submitting outcome to verifier..." << std::endl;
    if (coinFlip == 0) {

        std::cout <<"Winner Player A" << std::endl;

    } else { 

        std::cout <<"Winner Player B" << std::endl;

    }


    std::vector<uint8_t> noTranscript;
    auto verifierRes = client.submitOutcome(assignment.matchId, coinFlip, noTranscript);
    
    if (verifierRes.signature.empty()) {
        std::cerr << "Failed to get verifier signature." << std::endl;
        return 1;
    }

    printHex("Verifier provided signature: ", verifierRes.signature);

    std::cout << "C++ Native Client SDK test successful. Exiting cleanly." << std::endl;
    exit(0);
}
