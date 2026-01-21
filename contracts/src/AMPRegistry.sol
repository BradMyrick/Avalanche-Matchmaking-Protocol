// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

import "./AMPTypes.sol";

/**
 * @title AMPRegistry
 * @notice Handles registration of games and creation/joining of match instances.
 */
contract AMPRegistry {
    address private system;
    address payable owner;
    uint256 public nextGameId;
    uint256 public nextMatchId;

    uint256 public feeBalance;

    bool private locked;

    modifier nonReentrant() {
        require(!locked, "No reentrancy");
        locked = true;
        _;
        locked = false;
    }


    mapping(uint256 => AMPTypes.Game) public games;
    mapping(uint256 => AMPTypes.Match) public matches;

    event GameRegistered(
        uint256 indexed gameId,
        address admin,
        AMPTypes.SettlementMode mode
    );
    event MatchCreated(
        uint256 indexed matchId,
        uint256 indexed gameId,
        address playerA,
        uint256 stakeAmount
    );
    event MatchJoined(uint256 indexed matchId, address playerB);
    event FeesWithdrawn(uint256 amount, address wallet);

    modifier onlySystem() {
        require(msg.sender == system);
        _;
    }

    modifier onlyOwner() {
        require(msg.sender == owner);
        _;
    }

    constructor() {
        owner = payable(msg.sender);
    }

    /**
     * @notice Registers a new game in the AMP protocol.
     * @param mode Settlement mode (Async or Real-time).
     * @param verifiers Authorized verifier addresses (for ASYNC_VERIFIER mode).
     * @param minStake Minimum stake amount required for matches.
     * @param stakeToken Token to be used for stakes (address(0) for AVAX).
     */
    function registerGame(
        AMPTypes.SettlementMode mode,
        address[] calldata verifiers,
        uint256 minStake,
        address stakeToken
    ) external returns (uint256 gameId) {
        // TODO: implement permission model if needed. Currently open to any caller.
        gameId = nextGameId++;
        games[gameId] = AMPTypes.Game({
            admin: msg.sender,
            mode: mode,
            verifiers: verifiers,
            minStake: minStake,
            stakeToken: stakeToken
        });

        emit GameRegistered(gameId, msg.sender, mode);
    }

    /**
     * @notice Updates the list of authorized verifiers for a game.
     */
    function updateGameVerifiers(
        uint256 gameId,
        address[] calldata verifiers
    ) external {
        // TODO: restrict to game admin (games[gameId].admin).
        games[gameId].verifiers = verifiers;
    }

    /**
     * @notice Creates a new match for a registered game.
     */
    function createMatch(
        uint256 gameId
    ) external payable returns (uint256 matchId) {
        AMPTypes.Game storage game = games[gameId];
        require(msg.value >= game.minStake, "Stake too low");
        // TODO: implement ERC20 safeTransferFrom if game.stakeToken != address(0).
        // This ensures the stake is escrowed in the contract.

        matchId = nextMatchId++;
        matches[matchId] = AMPTypes.Match({
            gameId: gameId,
            playerA: msg.sender,
            playerB: address(0),
            stakeAmount: msg.value,
            state: AMPTypes.MatchState.OPEN,
            createdAt: block.timestamp
        });

        emit MatchCreated(matchId, gameId, msg.sender, msg.value);
    }

    /**
     * @notice Joins an existing OPEN match.
     */
    function joinMatch(uint256 matchId) external payable {
        AMPTypes.Match storage m = matches[matchId];
        require(m.state == AMPTypes.MatchState.OPEN, "Match not open");
        require(msg.value == m.stakeAmount, "Stake mismatch");
        // TODO: check stakeToken consistency and perform transfer if ERC20.

        m.playerB = msg.sender;
        m.state = AMPTypes.MatchState.READY;

        emit MatchJoined(matchId, msg.sender);
    }

    /**
     * @notice Pays out collected fees
     */
    function withdrawFees() external onlyOwner nonReentrant {
        require(feeBalance > 0, "No fees to withdraw");
        (bool success,) = owner.call{value: feeBalance}("");
        require(success, "Transfer Failed");

        emit FeesWithdrawn(feeBalance, owner);
        feeBalance = 0;
    }
}
