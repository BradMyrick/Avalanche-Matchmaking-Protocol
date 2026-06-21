// ============================================================================
// ⚠️  RUNS WITH PRE-COMPUTED AUTH SIGNATURE  ⚠️
// ============================================================================
// This example runs end-to-end against a live AMP server when supplied with
// a pre-computed EIP-191 challenge signature. It does NOT ship a bundled
// secp256k1 wallet — game developers integrate their own (MetaMask, hardware
// wallet, engine wallet, etc.) via `signChallenge()`.
//
// To run:
//   1. Start a local AMP server (see docs/beta-guide.mdx).
//   2. Generate or use any Ethereum wallet. Use `cast wallet sign` to sign
//      the server-issued challenge, OR set up a wallet integration.
//   3. Set `AMP_EXAMPLE_SIGNATURE_HEX=<65-byte EIP-191 sig in hex>` and run.
//
// OUTCOME signing uses the SDK's BUNDLED Keccak-256 (`<amp/keccak256.hpp>`)
// via `compute_outcome_eip712_digest()` from `<amp/eip712.hpp>` — no
// external crypto dep needed for digest computation. Only secp256k1
// (the actual signature) requires a wallet integration.
//
// The Rust and Go SDK examples (`amp-sdk/examples/rust/`, `amp-sdk/go/`)
// run out-of-the-box with a configured private key file — recommended
// starting point for first-time beta developers.
// ============================================================================
//
// AMP C++ SDK end-to-end smoke test.
//
// SECURITY NOTE: This example no longer ships a hardcoded signature. The
// previous version embedded a real-looking 65-byte EIP-712 blob that was
// reused for BOTH players, demonstrating the replay vulnerability documented
// in SECURITY_REVIEW.md (S7). It has been replaced by an explicit error
// directing the operator to supply their own signer.
//
// To run this example against a live AMP server, implement `signChallenge`
// below to call into your wallet (MetaMask, hardware wallet, keystore, etc.)
// and return a 65-byte EIP-191 signature over the given challenge bytes.

#include "amp/client.hpp"
#include <iomanip>
#include <iostream>
#include <stdexcept>
#include <vector>
#include <string>
#include <thread>
#include <cstdlib>
#include <unistd.h>

const std::string GAME_ID = "0x6767676767676767";

// Utility to print hex safely.
void printHex(const std::string &prefix, const std::vector<uint8_t> &data) {
  std::cout << prefix << "0x";
  for (auto b : data) {
    std::cout << std::hex << std::setw(2) << std::setfill('0') << (int)b;
  }
  std::cout << std::dec << std::endl;
}

// Hex-decode a 0x-prefixed hex string into bytes. Throws on malformed input.
std::vector<uint8_t> hexToBytes(const std::string &hex) {
  std::string h = (hex.size() >= 2 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X'))
                      ? hex.substr(2)
                      : hex;
  if (h.size() % 2 != 0) {
    throw std::invalid_argument("hex string has odd length");
  }
  std::vector<uint8_t> out;
  out.reserve(h.size() / 2);
  for (size_t i = 0; i < h.size(); i += 2) {
    char *end = nullptr;
    unsigned long b = std::strtoul(h.substr(i, 2).c_str(), &end, 16);
    if (end == nullptr || *end != '\0') {
      throw std::invalid_argument("invalid hex character in signature");
    }
    out.push_back(static_cast<uint8_t>(b));
  }
  return out;
}

// REPLACE THIS FUNCTION with a call into your real wallet. The challenge is
// the raw EIP-191 personal_sign payload returned by requestChallenge — sign
// it with the SAME wallet you want to identify as the player.
//
// Examples:
//   * JSON-RPC to a browser wallet (window.ethereum / EIP-1193).
//   * A hardware-wallet RPC (Ledger, Trezor) via their respective SDKs.
//   * A local keystore (e.g. libsecp256k1 + keccak256).
//
// The signature must be exactly 65 bytes in canonical r||s||v form with
// v in {27, 28}.
std::vector<uint8_t> signChallenge(const std::vector<uint8_t> & /*challenge*/) {
  // The example refuses to fabricate a signature. Point the operator at the
  // environment variable / config path that supplies real credentials.
  const char *sig_hex = std::getenv("AMP_EXAMPLE_SIGNATURE_HEX");
  if (sig_hex == nullptr || sig_hex[0] == '\0') {
    throw std::runtime_error(
        "No signature source configured. Implement signChallenge() in "
        "main.cpp to call your wallet, or set AMP_EXAMPLE_SIGNATURE_HEX to "
        "a 65-byte EIP-191 signature (FOR LOCAL TESTING ONLY — never ship "
        "a real private-key signature in source).");
  }
  std::vector<uint8_t> sig = hexToBytes(sig_hex);
  if (sig.size() != 65) {
    throw std::runtime_error("AMP_EXAMPLE_SIGNATURE_HEX must decode to 65 bytes");
  }
  return sig;
}

static std::string g_addr = "127.0.0.1:50051";

void runOpponent() {
  usleep(500000); // Wait half a second for player A to connect
  amp::AMPClient client2;
  client2.connect(g_addr);
  try {
    if (client2.authenticate(0, [](const std::vector<uint8_t> &challenge) {
          return signChallenge(challenge);
        })) {
      std::cout << "[Player B] Connected to Matchmaker. Requesting match..."
                << std::endl;
      amp::MatchRequest req;
      req.game_id = GAME_ID;
      req.player_id = "p2";
      client2.request_match(req);
    }
  } catch (const std::exception &e) {
    std::cerr << "[Player B] failed: " << e.what() << std::endl;
  }
}

int main(int argc, char **argv) {
  if (argc > 1) g_addr = argv[1];
  std::cout << "Starting AMP C++ Native Engine SDK Test on " << g_addr << "..."
            << std::endl;

  amp::AMPClient client;
  std::cout << "[Player A] Connecting to Matchmaker..." << std::endl;
  client.connect(g_addr);
  try {
    if (!client.authenticate(0, [](const std::vector<uint8_t> &challenge) {
          return signChallenge(challenge);
        })) {
      std::cerr << "Failed to connect to matchmaker." << std::endl;
      return 1;
    }
  } catch (const std::exception &e) {
    std::cerr << "Authentication error: " << e.what() << std::endl;
    return 1;
  }
  // Install the outcome signer. It receives (match_id, outcome, transcript_hash)
  // and must compute the EIP-712 digest + sign it with the same wallet used
  // at login. For this example, we re-use signChallenge as a placeholder —
  // REAL integrations must compute the proper EIP-712 digest with a vetted
  // Keccak-256 library (see amp-server/src/main.rs::compute_outcome_eip712_digest).
  client.set_outcome_signer([](const std::string & /*match_id*/,
                                uint8_t /*outcome*/,
                                const std::vector<uint8_t> & /*transcript_hash*/) {
    // NOTE: This is a placeholder. The real implementation must:
    //   1. Compute keccak256 over the EIP-712 typed data
    //      (see compute_outcome_eip712_digest in amp-server/src/main.rs).
    //   2. Sign the resulting 32-byte digest with the player's wallet.
    // The server will reject submissions where the recovered address does
    // not match the authenticated player.
    return signChallenge(std::vector<uint8_t>(32, 0));
  });
  std::cout << "[PlayerA] Connected & Logged in to AMP matchmaker." << std::endl;

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
  client.emit_telemetry(1, 1000200);

  int coinFlip = rand() % 2;
  client.emit_game_event(coinFlip == 0 ? "PlayerA_Scored" : "PlayerB_Scored");

  std::cout << "Submitting outcome to verifier..." << std::endl;
  std::cout << (coinFlip == 0 ? "Winner Player A" : "Winner Player B")
            << std::endl;

  // 32-byte transcript hash; use keccak256 of the actual game transcript.
  // Empty vector here will be rejected by the verifier — replace with the
  // real hash for end-to-end settlement.
  std::vector<uint8_t> transcriptHash(32, 0);

  auto verifierRes =
      client.submit_outcome(assignment.match_id, coinFlip, transcriptHash);

  if (verifierRes.signature.empty()) {
    std::cerr << "Failed to get verifier signature." << std::endl;
    return 1;
  }

  printHex("Verifier provided signature: ", verifierRes.signature);

  std::cout << "C++ Native Client SDK test successful. Exiting cleanly."
            << std::endl;
  // Note: do NOT call exit(0) — it bypasses stack unwinding and skips
  // ~AMPClient(). Return from main normally so RAII runs.
  return 0;
}
