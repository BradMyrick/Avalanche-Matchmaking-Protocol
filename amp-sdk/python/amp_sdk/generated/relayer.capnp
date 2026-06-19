@0xd47a28e3519c3621;

using Go = import "go_v3.capnp";
$Go.package("generated");
$Go.import("github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated");

using Rust = import "rust.capnp";
$Rust.parentModule("relayer_capnp");

interface RelayerService {
    authenticate @3 (apiKey :Data) -> (ok :Bool);
    # Authenticate the connection with an API key.
    # Must be called before submitOutcome or getCustodialAddress.
    # authenticate(apiKey)

    getGameAdmin @0 (gameId :UInt64) -> (admin :Data);
    # Returns the on-chain admin address for a given game.
    # getGameAdmin(gameId)

    getCustodialAddress @1 (gameId :UInt64) -> (address :Data);
    # Returns the derived custodial wallet address for a given game.
    # getCustodialAddress(gameId)

    submitOutcome @2 (matchId :Data, outcome :UInt8, transcriptHash :Data, signature :Data) -> (txHash :Data);
    # Submits a match outcome to the blockchain using the game's custodial wallet.
    # submitOutcome(matchId, outcome, transcriptHash, signature)

    getSettlementStatus @4 (matchId :Data) -> (info :SettlementStatusInfo);
    # Returns the current settlement state for a match. The server uses this
    # to reconcile its in-memory "settled" flag against the actual on-chain
    # outcome, closing the server/chain state-desync gap (release Phase 2.1).
    #
    # status codes:
    #   0 = unknown        (no record of this match anywhere)
    #   1 = queued         (accepted, not yet broadcast)
    #   2 = inFlight       (tx broadcast, awaiting receipt)
    #   3 = confirmed      (tx mined successfully — terminal)
    #   4 = reverted       (tx mined but reverted; will be retried up to maxRetries)
    #   5 = timedOut       (no receipt within the poll window; retried up to maxRetries)
    #   6 = deadLettered   (permanently failed after maxRetries — terminal)
    # getSettlementStatus(matchId)
}

struct SettlementStatusInfo {
    status @0 :UInt8;
    # See getSettlementStatus for the status code table.

    txHash @1 :Data;
    # The broadcast transaction hash once known (empty until InFlight+).

    retryCount @2 :UInt32;
    # Number of submit attempts so far.

    updatedAtMs @3 :UInt64;
    # Wall-clock millis of the last status transition.
}
