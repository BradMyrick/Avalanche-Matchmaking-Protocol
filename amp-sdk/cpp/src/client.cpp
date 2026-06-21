#include "amp/client.hpp"
#include <kj/async.h>
#include <chrono>
#include <cstring>
#include <iostream>
#include <stdexcept>

namespace amp {

AMPClient::~AMPClient() = default;

void AMPClient::connect(const std::string& address) {
    rpc_client_ = std::make_unique<capnp::EzRpcClient>(address);
    game_session_service_ = rpc_client_->getMain<GameSessionService>();
}

#ifdef AMP_HAS_KJ_TLS
void AMPClient::connect_tls(const std::string& address, const TlsConfig& config) {
    // Parse host:port.
    auto colon = address.find(':');
    if (colon == std::string::npos) {
        throw std::invalid_argument("AMPClient::connect_tls: address must be host:port");
    }
    std::string host = address.substr(0, colon);
    std::string port_str = address.substr(colon + 1);

    tls_io_ = kj::setupAsyncIo();
    auto& provider = tls_io_->provider;

    // Resolve and open the TCP stream.
    auto addr = provider->getNetwork().parseAddress(address.c_str()).wait(tls_io_->waitScope);
    tls_stream_ = addr->connect().wait(tls_io_->waitScope);

    // Build a TLS client context. Production verifies the peer against the
    // system trust store; dev may skip (NEVER enable in production).
    kj::TlsContext::Options opts;
    opts.useSystemTrustStore = !config.allow_self_signed;
    tls_context_ = std::make_unique<kj::TlsContext>(kj::mv(opts));

    auto server_name = config.server_name.empty() ? host : config.server_name;
    kj::Own<kj::AsyncIoStream> secured =
        tls_context_->wrapClient(kj::mv(tls_stream_), server_name)
            .wait(tls_io_->waitScope);
    tls_stream_ = kj::mv(secured);

    // TwoPartyClient takes the stream by reference; the stream must outlive
    // the client, which it does (both are members with the same lifetime).
    tls_client_ = std::make_unique<capnp::TwoPartyClient>(*tls_stream_);
    game_session_service_ = tls_client_->bootstrap().castAs<GameSessionService>();
}
#endif

kj::WaitScope& AMPClient::wait_scope() {
    // Plaintext path uses EzRpcClient's owned wait scope; TLS path uses the
    // self-owned AsyncIoSetup wait scope.
#ifdef AMP_HAS_KJ_TLS
    if (tls_client_) {
        return tls_io_->waitScope;
    }
#endif
    return rpc_client_->getWaitScope();
}

bool AMPClient::login(uint64_t game_id,
                      const std::vector<uint8_t>& signature,
                      const std::vector<uint8_t>& challenge_payload) {
    if (signature.size() != 65) {
        std::cerr << "AMP Login Error: signature must be 65 bytes, got "
                  << signature.size() << std::endl;
        return false;
    }
    try {
        auto req = game_session_service_.loginRequest();
        req.setGameId(game_id);
        kj::ArrayPtr<const kj::byte> sig_bytes(signature.data(), signature.size());
        req.setSignature(sig_bytes);
        // Pass back the challenge bytes so the server can locate the
        // outstanding nonce and verify the signature matches. The previous
        // version hardcoded this to empty, making the server's nonce lookup
        // impossible.
        kj::ArrayPtr<const kj::byte> chal_bytes(
            challenge_payload.data(), challenge_payload.size());
        req.setChallengePayload(chal_bytes);
        auto response = req.send().wait(rpc_client_->getWaitScope());
        user_session_ = response.getSession();
        return true;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP Login Error: " << e.getDescription().cStr() << std::endl;
        return false;
    }
}

bool AMPClient::authenticate(uint64_t game_id, SignChallengeCallback sign_callback) {
    try {
        if (!sign_callback) {
            std::cerr << "AMP Authenticate Error: No sign callback provided." << std::endl;
            return false;
        }

        auto chal_req = game_session_service_.requestChallengeRequest();
        chal_req.setGameId(game_id);
        auto chal_res = chal_req.send().wait(rpc_client_->getWaitScope());
        auto challenge = chal_res.getChallenge();
        std::vector<uint8_t> challenge_bytes(challenge.begin(), challenge.end());

        // Sign the EIP-191 challenge with the user's wallet.
        std::vector<uint8_t> signature = sign_callback(challenge_bytes);
        if (signature.size() != 65) {
            std::cerr << "AMP Authenticate Error: sign_callback must return 65 bytes, got "
                      << signature.size() << std::endl;
            return false;
        }

        return login(game_id, signature, challenge_bytes);
    } catch (const kj::Exception& e) {
        std::cerr << "AMP Authenticate Error: " << e.getDescription().cStr() << std::endl;
        return false;
    }
}

std::string AMPClient::create_profile(const PlayerProfileData& profile) {
    // Stub preserved for source compatibility. See header doc.
    return profile.player_id;
}

MatchResult AMPClient::request_match(const MatchRequest& request) {
    try {
        auto req = user_session_.requestMatchRequest();
        auto gm_req = req.initReq();

        kj::ArrayPtr<const kj::byte> game_id_bytes(
            reinterpret_cast<const kj::byte*>(request.game_id.data()),
            request.game_id.size());
        gm_req.setGameId(game_id_bytes);

        gm_req.setRulesType("standard");
        auto p_info = gm_req.initPlayerInfo();
        kj::ArrayPtr<const kj::byte> player_id_bytes(
            reinterpret_cast<const kj::byte*>(request.player_id.data()),
            request.player_id.size());
        p_info.setPlayerId(player_id_bytes);
        p_info.setDisplayName(request.player_id);
        p_info.setElo(Elo::UNRANKED);
        p_info.setRegion(Region::NA);
        p_info.setPlayerWallet(kj::ArrayPtr<const kj::byte>());

        auto p_stake = gm_req.initStake();
        p_stake.setPayerWallet(kj::ArrayPtr<const kj::byte>());
        p_stake.setFeeToken(kj::ArrayPtr<const kj::byte>());
        p_stake.setAuthSpend(0);

        gm_req.setOptionalConfig(kj::ArrayPtr<const kj::byte>());

        auto response = req.send().wait(rpc_client_->getWaitScope());
        auto assignment = response.getAssignment();

        MatchResult result;
        auto match_id_data = assignment.getMatchId();
        result.match_id = std::string(
            reinterpret_cast<const char*>(match_id_data.begin()),
            match_id_data.size());

        auto opponents = assignment.getOpponents();
        if (opponents.size() > 0) {
            auto opp_id = opponents[0].getPlayerId();
            result.opponent_id = std::string(
                reinterpret_cast<const char*>(opp_id.begin()),
                opp_id.size());
        }

        result.quality = assignment.getMatchQuality();

        match_session_ = response.getSession();
        has_match_session_ = true;
        return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP RequestMatch Error: " << e.getDescription().cStr() << std::endl;
        return {"", 0.0f, ""};
    }
}

void AMPClient::emit_game_event(const std::string& event_type) {
    if (!has_match_session_) return;
    try {
        auto req = match_session_.emitGameEventRequest();
        auto event = req.initEvent();
        event.setEventId(0);
        // Nanoseconds since epoch — see schema (TimeStamp = UInt64 ns).
        auto now_ns = std::chrono::duration_cast<std::chrono::nanoseconds>(
            std::chrono::system_clock::now().time_since_epoch()).count();
        event.setTimestamp(static_cast<uint64_t>(now_ns));
        event.setTriggeredBy(kj::ArrayPtr<const kj::byte>());
        event.setEventType(event_type);
        event.setEventData(kj::ArrayPtr<const kj::byte>());
        // Fire-and-forget. Capnp marks send() as warn_unused_result; we
        // explicitly drop the RemotePromise to signal intent.
        auto _ignored = req.send();
        (void)_ignored;
    } catch (const kj::Exception& e) {
        // Fire-and-forget: log and continue. The previous version used
        // catch(...) which swallowed std::bad_alloc and programming errors.
        std::cerr << "AMP EmitGameEvent (non-fatal): "
                  << e.getDescription().cStr() << std::endl;
    }
}

void AMPClient::emit_telemetry(uint8_t event_type_enum, uint64_t timestamp) {
    if (!has_match_session_) return;
    try {
        auto req = match_session_.emitTelemetryRequest();
        auto event = req.initEvent();
        event.setMatchId(kj::ArrayPtr<const kj::byte>());
        event.setTimestamp(timestamp);
        event.setEventType(static_cast<TelemetryEventType>(event_type_enum));
        event.setVerifierId(kj::ArrayPtr<const kj::byte>());
        event.setEventData(kj::ArrayPtr<const kj::byte>());
        auto _ignored = req.send();
        (void)_ignored;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP EmitTelemetry (non-fatal): "
                  << e.getDescription().cStr() << std::endl;
    }
}

VerifierResult AMPClient::submit_outcome(const std::string& match_id, uint8_t outcome,
                                          const std::vector<uint8_t>& transcript_hash,
                                          bool sign_locally) {
    // Validate inputs up front to produce a clear error rather than a
    // capnp/RPC error from the server.
    if (transcript_hash.size() != 32) {
        throw std::invalid_argument(
            "transcript_hash must be exactly 32 bytes, got " +
            std::to_string(transcript_hash.size()));
    }
    if (outcome < 1 || outcome > 4) {
        throw std::invalid_argument(
            "outcome must be 1..=4 (server invariant), got " +
            std::to_string(static_cast<int>(outcome)));
    }

    try {
        kj::ArrayPtr<const kj::byte> match_id_bytes(
            reinterpret_cast<const kj::byte*>(match_id.data()),
            match_id.size());

        // Reconnect if needed. The previous version had this branch but it
        // was unreachable because has_match_session_ was never reset; we
        // keep the branch and it now genuinely runs if the session is dropped.
        if (!has_match_session_) {
            auto recon_req = user_session_.reconnectRequest();
            recon_req.setMatchId(match_id_bytes);
            auto recon_res = recon_req.send().wait(rpc_client_->getWaitScope());
            match_session_ = recon_res.getSession();
            has_match_session_ = true;
        }

        // Compute the submitter signature via the user-supplied callback.
        // The C++ SDK intentionally does NOT bundle a Keccak-256 implementation;
        // the callback's caller is expected to use a vetted crypto library
        // (libsecp256k1, cpp-ethereum, etc.) for both digest and signature.
        std::vector<uint8_t> submitter_sig;
        if (sign_locally) {
            if (!outcome_signer_) {
                throw std::runtime_error(
                    "outcome signing requested but no signer installed; "
                    "call set_outcome_signer() with a SignOutcomeCallback that "
                    "computes the EIP-712 digest and signs it with the player's wallet");
            }
            submitter_sig = outcome_signer_(match_id, outcome, transcript_hash);
            if (submitter_sig.size() != 65) {
                throw std::runtime_error(
                    "outcome signer must return 65 bytes, got " +
                    std::to_string(submitter_sig.size()));
            }
        }

        auto out_req = match_session_.submitOutcomeRequest();
        auto sub = out_req.initSubmission();
        sub.setMatchId(match_id_bytes);

        auto match_outcome = sub.initOutcome();
        // Schema: enum OutcomeType { unknown@0, win@1, draw@2, void@3 }
        // The previous code mapped outcome==0 → WIN, which contradicts the
        // schema. Now we map by victor index.
        OutcomeType type;
        switch (outcome) {
            case 1: type = OutcomeType::WIN; break;
            case 2: type = OutcomeType::DRAW; break;
            case 3:
            case 4: type = OutcomeType::VOID; break;
            default: type = OutcomeType::UNKNOWN; break;
        }
        match_outcome.setType(type);
        auto scores = match_outcome.initScores(2);
        scores.set(0, 1);
        scores.set(1, 0);
        match_outcome.setVictor(outcome);
        match_outcome.setMetadata(kj::ArrayPtr<const kj::byte>());

        kj::ArrayPtr<const kj::byte> hash_bytes(transcript_hash.data(),
                                                  transcript_hash.size());
        sub.setReplayHash(hash_bytes);

        if (!submitter_sig.empty()) {
            kj::ArrayPtr<const kj::byte> sig_bytes(
                submitter_sig.data(), submitter_sig.size());
            sub.setSignature(sig_bytes);
        }

        auto out_res = out_req.send().wait(rpc_client_->getWaitScope());
        auto sig = out_res.getSignature();

        VerifierResult result;
        result.match_id = match_id;
        result.signature.assign(sig.begin(), sig.end());
        return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP SubmitOutcome Error: " << e.getDescription().cStr() << std::endl;
        return {"", {}};
    }
}

// ─── Async overloads ────────────────────────────────────────────────────
// These return kj::Promise<> that resolve on the AMP event loop. Game engines
// (Unreal/Unity) drive them via `promise.wait(client.wait_scope())` on a
// dedicated AMP thread instead of blocking the render thread.
//
// Requires a KJ built with coroutine support; enable via the
// `AMP_USE_COROUTINES` CMake option.
#if defined(AMP_USE_COROUTINES)
kj::Promise<bool> AMPClient::login_async(uint64_t game_id,
                                         const std::vector<uint8_t>& signature,
                                         const std::vector<uint8_t>& challenge_payload) {
    if (signature.size() != 65) {
        co_return false;
    }
    try {
        auto req = game_session_service_.loginRequest();
        req.setGameId(game_id);
        kj::ArrayPtr<const kj::byte> sig_bytes(signature.data(), signature.size());
        req.setSignature(sig_bytes);
        kj::ArrayPtr<const kj::byte> chal_bytes(challenge_payload.data(), challenge_payload.size());
        req.setChallengePayload(chal_bytes);
        auto response = co_await req.send();
        user_session_ = response.getSession();
        co_return true;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP LoginAsync Error: " << e.getDescription().cStr() << std::endl;
        co_return false;
    }
}

kj::Promise<MatchResult> AMPClient::request_match_async(const MatchRequest& request) {
    try {
        auto req = user_session_.requestMatchRequest();
        auto gm_req = req.initReq();
        kj::ArrayPtr<const kj::byte> game_id_bytes(
            reinterpret_cast<const kj::byte*>(request.game_id.data()),
            request.game_id.size());
        gm_req.setGameId(game_id_bytes);
        gm_req.setRulesType("standard");
        auto response = co_await req.send();
        auto assignment = response.getAssignment();

        MatchResult result;
        auto match_id_data = assignment.getMatchId();
        result.match_id = std::string(
            reinterpret_cast<const char*>(match_id_data.begin()),
            match_id_data.size());
        result.quality = assignment.getMatchQuality();
        match_session_ = response.getSession();
        has_match_session_ = true;
        co_return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP RequestMatchAsync Error: " << e.getDescription().cStr() << std::endl;
        co_return MatchResult{"", 0.0f, ""};
    }
}

kj::Promise<VerifierResult> AMPClient::submit_outcome_async(const std::string& match_id,
                                                            uint8_t outcome,
                                                            const std::vector<uint8_t>& transcript_hash,
                                                            bool sign_locally) {
    if (transcript_hash.size() != 32) {
        co_return VerifierResult{"", {}};
    }
    if (outcome < 1 || outcome > 4) {
        co_return VerifierResult{"", {}};
    }
    try {
        std::vector<uint8_t> submitter_sig;
        if (sign_locally) {
            if (!outcome_signer_) {
                co_return VerifierResult{"", {}};
            }
            submitter_sig = outcome_signer_(match_id, outcome, transcript_hash);
        }

        kj::ArrayPtr<const kj::byte> match_id_bytes(
            reinterpret_cast<const kj::byte*>(match_id.data()), match_id.size());

        if (!has_match_session_) {
            auto recon_req = user_session_.reconnectRequest();
            recon_req.setMatchId(match_id_bytes);
            auto recon_res = co_await recon_req.send();
            match_session_ = recon_res.getSession();
            has_match_session_ = true;
        }

        auto out_req = match_session_.submitOutcomeRequest();
        auto sub = out_req.initSubmission();
        sub.setMatchId(match_id_bytes);
        auto match_outcome = sub.initOutcome();
        OutcomeType type;
        switch (outcome) {
            case 1: type = OutcomeType::WIN; break;
            case 2: type = OutcomeType::DRAW; break;
            default: type = OutcomeType::VOID; break;
        }
        match_outcome.setType(type);
        match_outcome.setVictor(outcome);
        kj::ArrayPtr<const kj::byte> hash_bytes(transcript_hash.data(), transcript_hash.size());
        sub.setReplayHash(hash_bytes);
        if (!submitter_sig.empty()) {
            kj::ArrayPtr<const kj::byte> sig_bytes(submitter_sig.data(), submitter_sig.size());
            sub.setSignature(sig_bytes);
        }

        auto out_res = co_await out_req.send();
        auto sig = out_res.getSignature();
        VerifierResult result;
        result.match_id = match_id;
        result.signature.assign(sig.begin(), sig.end());
        co_return result;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP SubmitOutcomeAsync Error: " << e.getDescription().cStr() << std::endl;
        co_return VerifierResult{"", {}};
    }
}
#endif  // AMP_USE_COROUTINES

bool AMPClient::reconnect(const std::string& match_id) {
    try {
        kj::ArrayPtr<const kj::byte> match_id_bytes(
            reinterpret_cast<const kj::byte*>(match_id.data()), match_id.size());
        auto recon_req = user_session_.reconnectRequest();
        recon_req.setMatchId(match_id_bytes);
        auto recon_res = recon_req.send().wait(rpc_client_->getWaitScope());
        match_session_ = recon_res.getSession();
        has_match_session_ = true;
        return true;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP Reconnect Error: " << e.getDescription().cStr() << std::endl;
        return false;
    }
}

// ─── subscribeToEvents: MatchListener callback host ─────────────────────
namespace {
/// Adapts the schema's MatchListener callback interface into a
/// MatchEventListener C++ callback. Hosted on the client so the server can
/// push onMatchSettled / onOpponentDisconnected.
class MatchListenerImpl final : public MatchListener::Server {
public:
    explicit MatchListenerImpl(MatchEventListener listener)
        : listener_(kj::mv(listener)) {}

    kj::Promise<void> onMatchSettled(OnMatchSettledContext context) override {
        auto reader = context.getParams().getOutcome();
        MatchEvent evt;
        evt.kind = MatchEvent::Kind::Settled;
        evt.victor = reader.getVictor();
        auto scores = reader.getScores();
        for (auto s : scores) {
            evt.scores.push_back(s);
        }
        if (listener_) listener_(evt);
        return kj::READY_NOW;
    }

    kj::Promise<void> onOpponentDisconnected(OnOpponentDisconnectedContext) override {
        MatchEvent evt;
        evt.kind = MatchEvent::Kind::OpponentDisconnected;
        if (listener_) listener_(evt);
        return kj::READY_NOW;
    }

private:
    MatchEventListener listener_;
};
} // namespace

bool AMPClient::subscribe_to_events(MatchEventListener listener) {
    if (!has_match_session_) {
        std::cerr << "AMP SubscribeToEvents: no active match session" << std::endl;
        return false;
    }
    try {
        auto req = match_session_.subscribeToEventsRequest();
        req.setSubscriber(kj::heap<MatchListenerImpl>(kj::mv(listener)));
        // Fire-and-forget: the server holds its own capability reference and
        // dispatches callbacks on the AMP event loop.
        auto _ignored = req.send();
        (void)_ignored;
        return true;
    } catch (const kj::Exception& e) {
        std::cerr << "AMP SubscribeToEvents Error: " << e.getDescription().cStr() << std::endl;
        return false;
    }
}

} // namespace amp
