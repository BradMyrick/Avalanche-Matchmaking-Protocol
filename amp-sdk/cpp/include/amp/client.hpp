#pragma once
#include "types.hpp"
#include "amp_telemetry.capnp.h"
#include "game_core.capnp.h"
#include "match.capnp.h"
#include "service.capnp.h"
#include "player_profile.capnp.h"
#include <capnp/ez-rpc.h>
#include <memory>
#include <string>
#include <vector>
#include <functional>

namespace amp {

/// Callback that signs the EIP-191 challenge bytes for login.
/// Must return a 65-byte secp256k1 signature (r||s||v, v in {27, 28}).
using SignChallengeCallback = std::function<std::vector<uint8_t>(const std::vector<uint8_t>&)>;

/// Callback that signs the EIP-712 outcome digest.
/// Receives (match_id_utf8, outcome, 32-byte transcript_hash) and must:
///   1. Compute the canonical EIP-712 digest over (matchId, outcome, transcriptHash)
///      using a VETTED Keccak-256 library (libkeccak, cpp-ethereum's CryptoPP,
///      libsecp256k1's keccak, etc.). See amp-server/src/main.rs and the
///      C# OutcomeEip712 / Python _compute_outcome_eip712_digest for the
///      exact digest construction.
///   2. Sign that digest with the player's wallet (the SAME wallet used at login).
///   3. Return a 65-byte signature (r||s||v, v in {27, 28}).
using SignOutcomeCallback = std::function<std::vector<uint8_t>(
    const std::string& match_id,
    uint8_t outcome,
    const std::vector<uint8_t>& transcript_hash)>;

/// Main client for connecting to an AMP server via Cap'n Proto RPC.
///
/// Thread safety: NOT thread-safe. All methods must be called from the same
/// thread that called connect() (KJ's event loop is thread-affine).
/// For game engine integration, marshal calls to a dedicated AMP thread.
class AMPClient {
public:
    AMPClient() = default;
    ~AMPClient();

    // Value type with unique_ptr member — disable copy, allow explicit move.
    AMPClient(const AMPClient&) = delete;
    AMPClient& operator=(const AMPClient&) = delete;
    AMPClient(AMPClient&&) noexcept = default;
    AMPClient& operator=(AMPClient&&) noexcept = default;

    /// Connects to the AMP server at the given address (host:port).
    /// Throws kj::Exception on connection failure (observable on the first
    /// RPC call rather than here — EzRpcClient is lazy).
    void connect(const std::string& address);

    /// Authenticates with a pre-signed challenge. The challenge bytes must
    /// be passed back as `challenge_payload` so the server can match the
    /// signature to its outstanding nonce. The signature must be 65 bytes.
    bool login(uint64_t game_id,
               const std::vector<uint8_t>& signature,
               const std::vector<uint8_t>& challenge_payload);

    /// High-level: request challenge → sign → login.
    /// The sign_callback receives the EIP-191 challenge bytes and must
    /// return a 65-byte signature.
    bool authenticate(uint64_t game_id, SignChallengeCallback sign_callback);

    /// Installs the callback used to sign match outcomes. Required before
    /// calling submit_outcome() with `sign_locally = true`.
    void set_outcome_signer(SignOutcomeCallback signer) { outcome_signer_ = std::move(signer); }

    /// Creates or retrieves a player profile on the AMP server.
    /// NOTE: stub — the AMP server does not expose createOrUpdateProfile on
    /// the authenticated RPC surface today.
    std::string create_profile(const PlayerProfileData& profile);

    /// Submits a match request and blocks until a match is assigned.
    MatchResult request_match(const MatchRequest& request);

    /// Emits a typed game event during an active match.
    /// Errors are logged to stderr but do not throw (fire-and-forget).
    void emit_game_event(const std::string& event_type);

    /// Emits a telemetry event with the given type and timestamp.
    /// Errors are logged to stderr but do not throw.
    void emit_telemetry(uint8_t event_type_enum, uint64_t timestamp);

    /// Submits the final match outcome and returns the verifier's signature.
    ///
    /// @param match_id Authoritative match ID from request_match.
    /// @param outcome Victor index; must be 1..=4 (server invariant).
    /// @param transcript_hash EXACTLY 32 bytes (keccak256 of the transcript).
    /// @param sign_locally When true, signs via the callback installed by
    ///                     set_outcome_signer(). When false, caller must
    ///                     pre-compute and the server will reject (use this
    ///                     only when an external signer will be wired up later).
    /// @throws std::invalid_argument if transcript_hash is not 32 bytes,
    ///         outcome is outside 1..=4, or no outcome signer is installed.
    VerifierResult submit_outcome(const std::string& match_id,
                                  uint8_t outcome,
                                  const std::vector<uint8_t>& transcript_hash,
                                  bool sign_locally = true);

private:
    std::unique_ptr<capnp::EzRpcClient> rpc_client_;
    GameSessionService::Client game_session_service_ = nullptr;
    UserSession::Client user_session_ = nullptr;
    MatchSession::Client match_session_ = nullptr;
    bool has_match_session_ = false;
    SignOutcomeCallback outcome_signer_;
};

} // namespace amp
