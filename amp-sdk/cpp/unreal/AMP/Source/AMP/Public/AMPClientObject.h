// Blueprint-callable wrapper around the AMP C++ SDK.
//
// AMP's Cap'n Proto RPC is thread-affine, so this UObject runs all RPCs on a
// dedicated background thread and marshals results back to the game thread via
// dynamic multicast delegates. Signing (challenge + outcome) is deliberately
// left to Blueprint so games can use their own wallet / signer plugin — see
// OnChallengeReceived / OnOutcomeSignRequested.
#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "AMPClientObject.generated.h"

class UAMPClientObject;

/// Outcome of a connect attempt.
UENUM(BlueprintType)
enum class EAMPConnectResult : uint8
{
    Success UMETA(DisplayName = "Success"),
    InvalidAddress UMETA(DisplayName = "Invalid Address"),
    ConnectionFailed UMETA(DisplayName = "Connection Failed"),
};

/// Delegate fired when a Connect / ConnectTLS call completes.
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FAMPOnConnectResult, EAMPConnectResult, Result);

/// Delegate fired with a fresh authentication challenge (ns-expiry in
/// `ExpiresAtNs`). Sign the raw `Challenge` bytes with EIP-191 and call
/// LoginWithSignature().
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FAMPOnChallengeReceived, const TArray<uint8>&, Challenge, int64, ExpiresAtNs);

/// Delegate fired with the result of Login / Authenticate.
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FAMPOnLoginResult, bool, bSuccess);

/// Delegate fired when matchmaking succeeds. `MatchId` is the authoritative
/// match id to pass to SubmitOutcome / Reconnect.
DECLARE_DYNAMIC_MULTICAST_DELEGATE_TwoParams(FAMPOnMatchResult, const FString&, MatchId, float, Quality);

/// Delegate fired with the verifier's countersignature (65 bytes) after a
/// successful SubmitOutcome.
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FAMPOnOutcomeSubmitted, const TArray<uint8>&, VerifierSignature);

/// Delegate fired when the server pushes a match-settled event.
DECLARE_DYNAMIC_MULTICAST_DELEGATE_ThreeParams(FAMPOnMatchSettled, uint8, Victor, const TArray<int64>&, Scores, bool, bSuccess);

/// Delegate fired when an opponent disconnects mid-match.
DECLARE_DYNAMIC_MULTICAST_DELEGATE(FAMPOnOpponentDisconnected);

/// Delegate fired for any async error.
DECLARE_DYNAMIC_MULTICAST_DELEGATE_OneParam(FAMPOnError, const FString&, Message);

/**
 * AMP client for Blueprint. Create via Create AMPClientObject, Connect, then
 * drive the auth/match/submit flow. All RPCs run off the game thread.
 */
UCLASS(BlueprintType, Category = "AMP")
class AMP_API UAMPClientObject : public UObject
{
    GENERATED_BODY()

public:
    /** Connect over plaintext TCP. Fires OnConnectResult. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void Connect(const FString& Address);

    /** Connect over TLS (requires libkj-tls linked into the AMP module). */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void ConnectTLS(const FString& Address, const FString& ServerName, bool bAllowSelfSigned);

    /** Request an authentication challenge. Fires OnChallengeReceived. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void RequestChallenge(int64 GameId);

    /** High-level helper: request challenge → OnChallengeReceived → (you sign)
     *  → call LoginWithSignature. Provided as a convenience; equivalent to
     *  calling RequestChallenge then wiring OnChallengeReceived yourself. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void Authenticate(int64 GameId);

    /** Complete login with the signature over the challenge bytes. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void LoginWithSignature(int64 GameId, const TArray<uint8>& Signature, const TArray<uint8>& ChallengePayload);

    /** Enter the matchmaking queue. Fires OnMatchResult. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void RequestMatch(const FString& GameId);

    /** Submit the final outcome. `Signature` is the EIP-712 submitter
     *  signature over (matchId, outcome, transcriptHash). Fires
     *  OnOutcomeSubmitted with the verifier's countersignature. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void SubmitOutcome(const FString& MatchId, int32 Outcome, const TArray<uint8>& TranscriptHash, const TArray<uint8>& Signature);

    /** Reconnect to an existing active match. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void Reconnect(const FString& MatchId);

    /** Subscribe to server-pushed match events. Requires an active match. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void SubscribeToEvents();

    /** Fire-and-forget game event during a match. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void EmitGameEvent(const FString& EventType);

    /** Disconnect and release the AMP background thread. */
    UFUNCTION(BlueprintCallable, Category = "AMP")
    void Close();

    // ─── Delegates (bind in Blueprint) ───────────────────────────────
    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnConnectResult OnConnectResult;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnChallengeReceived OnChallengeReceived;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnLoginResult OnLoginResult;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnMatchResult OnMatchResult;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnOutcomeSubmitted OnOutcomeSubmitted;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnMatchSettled OnMatchSettled;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnOpponentDisconnected OnOpponentDisconnected;

    UPROPERTY(BlueprintAssignable, Category = "AMP")
    FAMPOnError OnError;

    virtual void BeginDestroy() override;

private:
    // Pimpl: hide the AMP SDK (and capnp) from the UE header.
    struct FImpl;
    TUniquePtr<FImpl> Impl;

    // The raw SDK client lives on a dedicated thread (KJ event loop is
    // thread-affine). FImpl owns it.
    void RunOnAmpThread(TUniqueFunction<void()> Task);
};
