@0xd47a28e3519c3621;

using Go = import "go.capnp";
$Go.package("generated");
$Go.import("amp-sdk/go/generated");

interface RelayerService {
    getGameAdmin @0 (gameId :UInt64) -> (admin :Data);
    # Returns the on-chain admin address for a given game.
    # getGameAdmin(gameId)

    getCustodialAddress @1 (gameId :UInt64) -> (address :Data);
    # Returns the derived custodial wallet address for a given game.
    # getCustodialAddress(gameId)

    submitOutcome @2 (matchId :Data, outcome :UInt8, transcriptHash :Data, signature :Data) -> (txHash :Data);
    # Submits a match outcome to the blockchain using the game's custodial wallet.
    # submitOutcome(matchId, outcome, transcriptHash, signature)
}
