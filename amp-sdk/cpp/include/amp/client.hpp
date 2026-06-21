#pragma once
#include "types.hpp"
#include "amp_telemetry.capnp.h"
#include "game_core.capnp.h"
#include "match.capnp.h"
#include "service.capnp.h"
#include "player_profile.capnp.h"
#include <capnp/ez-rpc.h>
#include <kj/async.h>
#include <memory>
#include <string>
#include <vector>
#include <functional>

// Optional TLS support via libkj-tls. The header is detected at compile time;
// if <kj/tls.h> is unavailable, TLS methods are omitted and the SDK degrades
// to plaintext-only (documented). Unreal / production builds link libkj-tls
// via their ThirdParty module.
#if __has_include(<kj/tls.h>)
#define AMP_HAS_KJ_TLS 1
#include <kj/tls.h>
#endif

namespace amp {

/// Callback that signs the EIP-191 challenge bytes for login.
/// Must return a 65-byte secp256k1 signature (r||s||v, v in {27, 28}).
using SignChallengeCallback = std::function<std::vector<uint8_t>(const std::vector<uint8_t>&)>;

/// Callback that signs the EIP-712 outcome digest.
/// Receives (match_id_utf8, outcome, 32-byte transcript_hash) and must:
///   1. EITHER call `amp::compute_outcome_eip712_digest(...)` from
///      `<amp/eip712.hpp>` (BUNDLED — uses the SDK's own vetted Keccak-256
///      from `<amp/keccak256.hpp>`, no external crypto dep), OR compute the
///      same digest via a third-party library. Both paths produce the same
///      digest — pinned by the cross-language KAT in `cpp/tests/eip712_test.cpp`.
///   2. Sign that digest with the player's wallet (the SAME wallet used at
///      login). Use libsecp256k1, cpp-ethereum's CryptoPP, or your
///      engine's wallet integration. The SDK does NOT bundle secp256k1.
///   3. Return a 65-byte signature (r||s||v, v in {27, 28}).
using SignOutcomeCallback = std::function<std::vector<uint8_t>(
    const std::string& match_id,
    uint8_t outcome,
    const std::vector<uint8_t>& transcript_hash)>;

/// Server-pushed match event delivered to a `subscribe_to_events` listener.
struct MatchEvent {
    enum class Kind { Settled, OpponentDisconnected } kind = Kind::Settled;
    /// Populated when `kind == Settled`.
    uint8_t victor = 0;
    std::vector<uint64_t> scores;
};

/// Listener callback invoked for each server-pushed match event.
using MatchEventListener = std::function<void(const MatchEvent&)>;

/// TLS configuration for `AMPClient::connect_tls`. Defaults verify the peer
/// against the system trust store (production). `allow_self_signed` skips
/// verification for local dev (NEVER enable in production).
struct TlsConfig {
    std::string server_name;        ///< SNI / expected hostname.
    bool allow_self_signed = false; ///< Skip peer verification (dev only).
};

/// Main client for connecting to an AMP server via Cap'n Proto RPC.
///
/// Threading model: the Cap'n Proto event loop is thread-affine. The blocking
/// (`*_sync`) overloads must be called from the thread that called connect().
/// The async overloads return `kj::Promise<>` that resolve on that same loop —
/// use them from game engines (Unreal/Unity) to avoid stalling the render
/// thread. For Unreal, marshal onto a dedicated AMP async task via the
/// provided Blueprint wrapper (see `cpp/unreal/`).
class AMPClient {
public:
    AMPClient() = default;
    ~AMPClient();

    // Value type with unique_ptr member — disable copy, allow explicit move.
    AMPClient(const AMPClient&) = delete;
    AMPClient& operator=(const AMPClient&) = delete;
    AMPClient(AMPClient&&) noexcept = default;
    AMPClient& operator=(AMPClient&&) noexcept = default;

    /// Connects to the AMP server over plaintext TCP. Throws kj::Exception on
    /// the first RPC call if the address is unreachable (EzRpcClient is lazy).
    void connect(const std::string& address);

#ifdef AMP_HAS_KJ_TLS
    /// Connect over TLS (libkj-tls). Requires `AMP_HAS_KJ_TLS`. Throws
    /// kj::Exception on TLS handshake / connection failure.
    void connect_tls(const std::string& address, const TlsConfig& config);
#endif

    // ─── Authentication ────────────────────────────────────────────────
    bool login(uint64_t game_id,
               const std::vector<uint8_t>& signature,
               const std::vector<uint8_t>& challenge_payload);

    /// High-level: request challenge → sign → login.
    bool authenticate(uint64_t game_id, SignChallengeCallback sign_callback);

    /// Async login. Resolves on the AMP event loop. Requires
    /// `AMP_USE_COROUTINES` (a KJ built with coroutine support); otherwise
    /// use the blocking `login`.
#if defined(AMP_USE_COROUTINES)
    kj::Promise<bool> login_async(uint64_t game_id,
                                  const std::vector<uint8_t>& signature,
                                  const std::vector<uint8_t>& challenge_payload);
#endif

    /// Installs the callback used to sign match outcomes. Required before
    /// calling submit_outcome() with `sign_locally = true`.
    void set_outcome_signer(SignOutcomeCallback signer) { outcome_signer_ = std::move(signer); }

    // ─── Match lifecycle ───────────────────────────────────────────────
    /// Creates or retrieves a player profile on the AMP server.
    /// NOTE: stub — the AMP server does not expose createOrUpdateProfile on
    /// the authenticated RPC surface today.
    std::string create_profile(const PlayerProfileData& profile);

    /// Submits a match request and blocks until a match is assigned.
    MatchResult request_match(const MatchRequest& request);

    /// Async match request. Resolves on the AMP event loop. Requires
    /// `AMP_USE_COROUTINES`.
#if defined(AMP_USE_COROUTINES)
    kj::Promise<MatchResult> request_match_async(const MatchRequest& request);
#endif

    /// Reconnect to an existing active match by id (e.g. after a process
    /// restart). Blocks until the server responds.
    bool reconnect(const std::string& match_id);

    /// Emits a typed game event during an active match (fire-and-forget).
    void emit_game_event(const std::string& event_type);

    /// Emits a telemetry event with the given type and timestamp.
    void emit_telemetry(uint8_t event_type_enum, uint64_t timestamp);

    /// Submits the final match outcome and returns the verifier's signature.
    VerifierResult submit_outcome(const std::string& match_id,
                                  uint8_t outcome,
                                  const std::vector<uint8_t>& transcript_hash,
                                  bool sign_locally = true);

    /// Async outcome submission. Requires `AMP_USE_COROUTINES`.
#if defined(AMP_USE_COROUTINES)
    kj::Promise<VerifierResult> submit_outcome_async(const std::string& match_id,
                                                     uint8_t outcome,
                                                     const std::vector<uint8_t>& transcript_hash,
                                                     bool sign_locally = true);
#endif

    /// Subscribe to server-pushed match events (`onMatchSettled`,
    /// `onOpponentDisconnected`). The `listener` is invoked on the AMP event
    /// loop for each event until the match ends or the client is destroyed.
    /// Returns false if there is no active match session.
    bool subscribe_to_events(MatchEventListener listener);

    /// Access the underlying wait scope — needed to drive async overloads
    /// (`promise.wait(client.wait_scope())`).
    kj::WaitScope& wait_scope();
    bool is_connected() const { return rpc_client_ != nullptr; }

private:
    std::unique_ptr<capnp::EzRpcClient> rpc_client_;
    GameSessionService::Client game_session_service_ = nullptr;
    UserSession::Client user_session_ = nullptr;
    MatchSession::Client match_session_ = nullptr;
    bool has_match_session_ = false;
    SignOutcomeCallback outcome_signer_;
#ifdef AMP_HAS_KJ_TLS
    // TLS-mode owns its own event loop / stream / RPC client (EzRpcClient has
    // no stream-accepting constructor in capnp 1.3, so the plaintext and TLS
    // paths use different plumbing).
    kj::Own<kj::AsyncIoStream> tls_stream_;
    std::unique_ptr<capnp::TwoPartyClient> tls_client_;
    kj::Own<kj::AsyncIoSetup> tls_io_;
    std::unique_ptr<kj::TlsContext> tls_context_;
#endif
};

} // namespace amp
