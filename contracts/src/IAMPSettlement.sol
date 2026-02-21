// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

/**
 * @title IAMPSettlement
 * @notice Handles match creation, settlement, and stake distribution for AMP games.
 */
interface IAMPSettlement {
    event MatchCreated(bytes32 indexed matchId, bytes32 indexed gameId, address[] players, uint256 timestamp);
    event MatchSettled(bytes32 indexed matchId, uint8 victorIndex, uint256 outcomeType); 
    event SettlementFailed(bytes32 indexed matchId, string reason);

    /**
     * @notice Creates a new on-chain match record.
     * @dev This is usually called by the Matchmaker, or via meta-transaction.
     *      Players must have approved the contract to spend the required feeToken.
     * @param gameId The game being played.
     * @param players List of players participating.
     * @param configData The opaque game configuration data (for hash verification).
     * @param requiredStake Amount each player is staking.
     * @param feeToken The token used for staking (address(0) for native).
     * @return matchId The generated unique match ID.
     */
    function createMatch(
        bytes32 gameId,
        address[] calldata players,
        bytes calldata configData,
        uint256 requiredStake,
        address feeToken
    ) external payable returns (bytes32 matchId);

    /**
     * @notice Submits the final outcome of a match.
     * @dev Only the authorized verifier for the game can submit settlement. 
     *      Or it verifies signatures from the authorized verifier.
     * @param matchId The unique match ID.
     * @param outcomeData The serialized outcome structure (OutcomeType, Scores, Victor).
     * @param signatures The signatures authorizing this settlement (Verifier or players).
     */
    function submitSettlement(
        bytes32 matchId,
        bytes calldata outcomeData,
        bytes calldata signatures
    ) external;

    /**
     * @notice Checks the status of a match.
     * @return status 0: Pending, 1: Active, 2: Settled, 3: Voided.
     */
    function getMatchStatus(bytes32 matchId) external view returns (uint8);
}
