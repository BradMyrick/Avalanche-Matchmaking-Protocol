// Blueprint wrapper implementation. Spawns a dedicated AMP background thread
// (KJ event loop is thread-affine) and marshals results to the game thread via
// AsyncTask. Signing is left to Blueprint via the OnChallengeReceived delegate.
#include "AMPClientObject.h"
#include "Async/Async.h"
#include "HAL/Runnable.h"
#include "Misc/ScopeExit.h"

#include "amp/client.hpp"

#include <atomic>
#include <condition_variable>
#include <mutex>
#include <queue>
#include <string>
#include <vector>

// ─── AMP background thread (owns the KJ event loop + amp::AMPClient) ──────
class FAMPRunnable : public FRunnable
{
public:
    amp::AMPClient Client;
    std::atomic<bool> bShouldStop{false};

    virtual bool Init() override { return true; }
    virtual uint32 Run() override
    {
        // The KJ event loop is driven by the blocking RPCs themselves
        // (EzRpcClient owns a thread-local loop). We simply service queued
        // tasks until asked to stop.
        while (!bShouldStop.load(std::memory_order_relaxed))
        {
            std::unique_lock<std::mutex> lk(QueueMx);
            Cv.wait_for(lk, std::chrono::milliseconds(50),
                        [this] { return !Queue.empty() || bShouldStop.load(); });
            while (!Queue.empty())
            {
                auto Task = std::move(Queue.front());
                Queue.pop();
                lk.unlock();
                Task();
                lk.lock();
            }
        }
        return 0;
    }
    virtual void Stop() override
    {
        bShouldStop.store(true);
        Cv.notify_all();
    }

    void Enqueue(TUniqueFunction<void()> Task)
    {
        {
            std::lock_guard<std::mutex> lk(QueueMx);
            Queue.push(std::move(Task));
        }
        Cv.notify_one();
    }

private:
    std::mutex QueueMx;
    std::condition_variable Cv;
    std::queue<TUniqueFunction<void()>> Queue;
};

struct UAMPClientObject::FImpl
{
    TUniquePtr<FAMPRunnable> Runnable;
    TUniquePtr<FRunnableThread> Thread;

    void EnsureStarted()
    {
        if (Runnable) return;
        Runnable = MakeUnique<FAMPRunnable>();
        Thread = TUniquePtr<FRunnableThread>(
            FRunnableThread::Create(Runnable.Get(), TEXT("AMPClient"), 0, TPri_Normal));
    }
};

void UAMPClientObject::RunOnAmpThread(TUniqueFunction<void()> Task)
{
    if (!Impl) Impl = MakeUnique<FImpl>();
    Impl->EnsureStarted();
    Impl->Runnable->Enqueue(std::move(Task));
}

// ─── Helpers to marshal results back to the game thread ──────────────────
static TArray<uint8> ToByteArray(const std::vector<uint8_t>& V)
{
    TArray<uint8> Out;
    Out.Append(reinterpret_cast<const uint8*>(V.data()), V.size());
    return Out;
}

static std::vector<uint8_t> ToStdVector(const TArray<uint8>& A)
{
    return std::vector<uint8_t>(A.GetData(), A.GetData() + A.Num());
}

// ─── Blueprint-callable entry points ──────────────────────────────────────
void UAMPClientObject::Connect(const FString& Address)
{
    std::string Addr(TCHAR_TO_UTF8(*Address));
    RunOnAmpThread([this, Addr]()
    {
        EAMPConnectResult Result = EAMPConnectResult::Success;
        try {
            Impl->Runnable->Client.connect(Addr);
        } catch (const std::exception&) {
            Result = EAMPConnectResult::ConnectionFailed;
        } catch (...) {
            Result = EAMPConnectResult::ConnectionFailed;
        }
        AsyncTask(ENamedThreads::GameThread, [this, Result]()
        {
            if (OnConnectResult.IsBound()) OnConnectResult.Broadcast(Result);
        });
    });
}

void UAMPClientObject::ConnectTLS(const FString& Address, const FString& ServerName, bool bAllowSelfSigned)
{
#ifdef AMP_HAS_KJ_TLS
    std::string Addr(TCHAR_TO_UTF8(*Address));
    std::string Name(TCHAR_TO_UTF8(*ServerName));
    RunOnAmpThread([this, Addr, Name, bAllowSelfSigned]()
    {
        amp::TlsConfig Cfg;
        Cfg.server_name = Name;
        Cfg.allow_self_signed = bAllowSelfSigned;
        EAMPConnectResult Result = EAMPConnectResult::Success;
        try {
            Impl->Runnable->Client.connect_tls(Addr, Cfg);
        } catch (...) {
            Result = EAMPConnectResult::ConnectionFailed;
        }
        AsyncTask(ENamedThreads::GameThread, [this, Result]()
        {
            if (OnConnectResult.IsBound()) OnConnectResult.Broadcast(Result);
        });
    });
#else
    if (OnError.IsBound())
    {
        OnError.Broadcast(TEXT("TLS support requires libkj-tls linked into the AMP module"));
    }
#endif
}

void UAMPClientObject::RequestChallenge(int64 GameId)
{
    RunOnAmpThread([this, GameId]()
    {
        // challenge bytes come back from login flow — the SDK's authenticate()
        // signs internally; for Blueprint we expose the raw challenge via
        // EzRpcClient. As a minimal Blueprint path we use authenticate() with
        // a sign callback that publishes the challenge and awaits a signature
        // pushed back from Blueprint.
        // NOTE: full challenge handoff is handled in Authenticate(); this
        // stub keeps RequestChallenge callable for advanced users.
        AsyncTask(ENamedThreads::GameThread, [this]()
        {
            if (OnError.IsBound())
            {
                OnError.Broadcast(TEXT("Use Authenticate() for the Blueprint challenge flow"));
            }
        });
    });
    (void)GameId;
}

void UAMPClientObject::Authenticate(int64 GameId)
{
    RunOnAmpThread([this, GameId]()
    {
        // Install a sign callback that hands the challenge to Blueprint and
        // blocks until Blueprint supplies the signature via a promise. This
        // keeps all crypto in the game's wallet plugin.
        // For the minimal wrapper we delegate to authenticate() with a
        // callback that immediately reports the challenge; a fuller impl uses
        // a std::promise per outstanding challenge.
        bool bOk = false;
        try {
            bOk = Impl->Runnable->Client.authenticate(
                static_cast<uint64_t>(GameId),
                [this](const std::vector<uint8_t>& Challenge) -> std::vector<uint8_t>
                {
                    // Marshal challenge to game thread; signature is expected
                    // to be returned synchronously. Games that need async
                    // signing should drive RequestChallenge/LoginWithSignature
                    // manually. Return empty to signal "no inline signature".
                    TArray<uint8> Ch = ToByteArray(Challenge);
                    AsyncTask(ENamedThreads::GameThread, [this, Ch]()
                    {
                        if (OnChallengeReceived.IsBound())
                        {
                            OnChallengeReceived.Broadcast(Ch, 0);
                        }
                    });
                    return {};
                });
        } catch (...) {
            bOk = false;
        }
        bool FinalOk = bOk;
        AsyncTask(ENamedThreads::GameThread, [this, FinalOk]()
        {
            if (OnLoginResult.IsBound()) OnLoginResult.Broadcast(FinalOk);
        });
    });
}

void UAMPClientObject::LoginWithSignature(int64 GameId, const TArray<uint8>& Signature, const TArray<uint8>& ChallengePayload)
{
    auto Sig = ToStdVector(Signature);
    auto Chal = ToStdVector(ChallengePayload);
    RunOnAmpThread([this, GameId, Sig, Chal]()
    {
        bool bOk = Impl->Runnable->Client.login(
            static_cast<uint64_t>(GameId), Sig, Chal);
        AsyncTask(ENamedThreads::GameThread, [this, bOk]()
        {
            if (OnLoginResult.IsBound()) OnLoginResult.Broadcast(bOk);
        });
    });
}

void UAMPClientObject::RequestMatch(const FString& GameId)
{
    std::string Gid(TCHAR_TO_UTF8(*GameId));
    RunOnAmpThread([this, Gid]()
    {
        amp::MatchRequest Req;
        Req.game_id = Gid;
        amp::MatchResult Res = Impl->Runnable->Client.request_match(Req);
        FString MatchId(UTF8_TO_TCHAR(Res.match_id.c_str()));
        float Q = Res.quality;
        AsyncTask(ENamedThreads::GameThread, [this, MatchId, Q]()
        {
            if (OnMatchResult.IsBound()) OnMatchResult.Broadcast(MatchId, Q);
        });
    });
}

void UAMPClientObject::SubmitOutcome(const FString& MatchId, int32 Outcome, const TArray<uint8>& TranscriptHash, const TArray<uint8>& Signature)
{
    std::string Mid(TCHAR_TO_UTF8(*MatchId));
    auto TH = ToStdVector(TranscriptHash);
    auto Sig = ToStdVector(Signature);
    uint8_t Outcome8 = static_cast<uint8_t>(Outcome);
    RunOnAmpThread([this, Mid, Outcome8, TH, Sig]()
    {
        // Install an outcome signer that returns the Blueprint-supplied
        // signature verbatim (the EIP-712 digest + signing happened in the
        // game's wallet plugin).
        Impl->Runnable->Client.set_outcome_signer(
            [Sig](const std::string&, uint8_t, const std::vector<uint8_t>&) -> std::vector<uint8_t>
            { return Sig; });
        amp::VerifierResult Res;
        try {
            Res = Impl->Runnable->Client.submit_outcome(Mid, Outcome8, TH, true);
        } catch (const std::exception&) {
            Res.match_id.clear();
            Res.signature.clear();
        }
        TArray<uint8> VSig = ToByteArray(Res.signature);
        bool bOk = !VSig.IsEmpty();
        AsyncTask(ENamedThreads::GameThread, [this, VSig, bOk]()
        {
            if (OnOutcomeSubmitted.IsBound()) OnOutcomeSubmitted.Broadcast(VSig);
            if (!bOk && OnError.IsBound()) OnError.Broadcast(TEXT("SubmitOutcome rejected"));
        });
    });
}

void UAMPClientObject::Reconnect(const FString& MatchId)
{
    std::string Mid(TCHAR_TO_UTF8(*MatchId));
    RunOnAmpThread([this, Mid]()
    {
        Impl->Runnable->Client.reconnect(Mid);
    });
}

void UAMPClientObject::SubscribeToEvents()
{
    RunOnAmpThread([this]()
    {
        Impl->Runnable->Client.subscribe_to_events(
            [this](const amp::MatchEvent& Evt)
            {
                if (Evt.kind == amp::MatchEvent::Kind::Settled)
                {
                    uint8 Victor = Evt.victor;
                    TArray<int64> Scores;
                    for (auto s : Evt.scores) Scores.Add(static_cast<int64>(s));
                    AsyncTask(ENamedThreads::GameThread, [this, Victor, Scores]()
                    {
                        if (OnMatchSettled.IsBound()) OnMatchSettled.Broadcast(Victor, Scores, true);
                    });
                }
                else // OpponentDisconnected
                {
                    AsyncTask(ENamedThreads::GameThread, [this]()
                    {
                        if (OnOpponentDisconnected.IsBound()) OnOpponentDisconnected.Broadcast();
                    });
                }
            });
    });
}

void UAMPClientObject::EmitGameEvent(const FString& EventType)
{
    std::string Et(TCHAR_TO_UTF8(*EventType));
    RunOnAmpThread([this, Et]() { Impl->Runnable->Client.emit_game_event(Et); });
}

void UAMPClientObject::Close()
{
    if (Impl && Impl->Runnable)
    {
        Impl->Runnable->Stop();
        if (Impl->Thread) Impl->Thread->Wait();
    }
    Impl.Reset();
}

void UAMPClientObject::BeginDestroy()
{
    Close();
    Super::BeginDestroy();
}
