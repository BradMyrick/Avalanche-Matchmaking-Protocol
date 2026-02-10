// SPDX-License-Identifier: MIT
pragma solidity ^0.8.19;

/**
 * @title IAMPRegistry
 * @notice Registry for valid games and their configurations on the AMP protocol.
 */
interface IAMPRegistry {
    event GameRegistered(bytes32 indexed gameId, string name, address verifier, uint256 minStake);
    event GameConfigUpdated(bytes32 indexed gameId, address verifier, uint256 minStake);

    /**
     * @notice Registers a new game.
     * @param name Human-readable name of the game.
     * @param verifierKey The address of the trusted verifier (or verifier contract) for this game.
     * @param minStake The minimum stake required to play this game (in wei/tokens).
     * @return gameId The unique identifier for the registered game.
     */
    function registerGame(
        string calldata name,
        address verifierKey,
        uint256 minStake
    ) external returns (bytes32 gameId);

    /**
     * @notice Returns the verifier address for a given game.
     */
    function getVerifier(bytes32 gameId) external view returns (address);

    /**
     * @notice Checks if a game is valid and active.
     */
    function isGameValid(bytes32 gameId) external view returns (bool);
}
