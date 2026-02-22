// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

import "./AMPTypes.sol";

/**
 * @title AMPRegistry
 * @notice Handles registration of games and creation/joining of match instances.
 */
contract AMPRegistry {
    address public settlement;
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

    modifier onlySettlement() {
        require(msg.sender == settlement, "Not settlement");
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
        // TODO: Implement permission model if needed. Currently open to any caller.
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
        // TODO: Restrict to game admin (games[gameId].admin).
        games[gameId].verifiers = verifiers;
    }

    /**
     * @notice Returns the authorized verifiers for a game.
     */
    function getGameVerifiers(
        uint256 gameId
    ) external view returns (address[] memory) {
        return games[gameId].verifiers;
    }

    /**
     * @notice Creates a new match for a registered game.
     */
    function createMatch(
        uint256 gameId
    ) external payable returns (uint256 matchId) {
        AMPTypes.Game storage game = games[gameId];
        require(msg.value >= game.minStake, "Stake too low");
        // TODO: Implement ERC20 safeTransferFrom if game.stakeToken != address(0).
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
        // TODO: Check stakeToken consistency and perform transfer if ERC20.

        m.playerB = msg.sender;
        m.state = AMPTypes.MatchState.READY;

        emit MatchJoined(matchId, msg.sender);
    }

    /**
     * @notice Pays out collected fees
     */
    function withdrawFees() external onlyOwner nonReentrant {
        require(feeBalance > 0, "No fees to withdraw");
        (bool success, ) = owner.call{value: feeBalance}("");
        require(success, "Transfer Failed");

        emit FeesWithdrawn(feeBalance, owner);
        feeBalance = 0;
    }

    /**
     * @notice Allows owner to set the settlement contract address.
     */
    function setSettlement(address _settlement) external onlyOwner {
        settlement = _settlement;
    }

    /**
     * @notice Settles a match and distributes payouts. Only callable by the Settlement contract.
     */
    function settleMatch(
        uint256 matchId,
        AMPTypes.MatchState newState,
        address[] calldata recipients,
        uint256[] calldata amounts,
        uint256 protocolFee
    ) external onlySettlement nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        require(
            m.state == AMPTypes.MatchState.READY ||
                m.state == AMPTypes.MatchState.OPEN,
            "Match not settlable"
        );
        m.state = newState;

        feeBalance += protocolFee;

        for (uint i = 0; i < recipients.length; i++) {
            if (amounts[i] > 0) {
                (bool success, ) = recipients[i].call{value: amounts[i]}("");
                require(success, "Transfer failed");
            }
        }
    }
}
