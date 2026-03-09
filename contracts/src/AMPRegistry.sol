// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

import "./AMPTypes.sol";
import "openzeppelin-contracts/contracts/token/ERC20/IERC20.sol";
import "openzeppelin-contracts/contracts/token/ERC20/utils/SafeERC20.sol";
import "openzeppelin-contracts/contracts/metatx/ERC2771Context.sol";

/**
 * @title AMPRegistry
 * @notice Handles registration of games and creation/joining of match instances.
 */
contract AMPRegistry is ERC2771Context {
    using SafeERC20 for IERC20;

    address public settlement;
    address payable public immutable owner;
    uint256 public nextGameId;
    uint256 public nextMatchId;

    mapping(address => uint256) public feeBalances;

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
        address indexed admin,
        AMPTypes.SettlementMode mode
    );
    event MatchCreated(
        uint256 indexed matchId,
        uint256 indexed gameId,
        address indexed playerA,
        uint256 stakeAmount
    );
    event MatchJoined(uint256 indexed matchId, address indexed playerB);
    event FeesWithdrawn(address indexed token, uint256 amount, address indexed wallet);

    modifier onlySettlement() {
        require(_msgSender() == settlement, "Not settlement");
        _;
    }

    modifier onlyOwner() {
        require(_msgSender() == owner);
        _;
    }

    constructor(address trustedForwarder) ERC2771Context(trustedForwarder) {
        owner = payable(_msgSender());
    }

    /**
     * @notice Registers a new game in the AMP protocol.
     * @param mode Settlement mode (Async or Real-time).
     * @param verifiers Authorized verifier addresses (for ASYNC_VERIFIER mode).
     * @param minStake Minimum stake amount required for matches.
     * @param stakeToken Token to be used for stakes (address(0) for AVAX).
     * @param arbiter Address authorized to resolve RT_HASH_AGREE disputes.
     */
    function registerGame(
        AMPTypes.SettlementMode mode,
        address[] calldata verifiers,
        uint256 minStake,
        address stakeToken,
        address arbiter
    ) external returns (uint256 gameId) {
        gameId = nextGameId++;
        games[gameId] = AMPTypes.Game({
            admin: _msgSender(),
            mode: mode,
            verifiers: verifiers,
            minStake: minStake,
            stakeToken: stakeToken,
            arbiter: arbiter
        });

        emit GameRegistered(gameId, _msgSender(), mode);
    }

    /**
     * @notice Updates the list of authorized verifiers for a game.
     */
    function updateGameVerifiers(
        uint256 gameId,
        address[] calldata verifiers
    ) external {
        require(_msgSender() == games[gameId].admin, "Not game admin");
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
        uint256 gameId,
        uint256 stakeAmount
    ) external payable nonReentrant returns (uint256 matchId) {
        AMPTypes.Game storage game = games[gameId];
        
        uint256 actualStake;
        if (game.stakeToken == address(0)) {
            require(msg.value >= game.minStake, "Stake too low");
            actualStake = msg.value;
        } else {
            require(stakeAmount >= game.minStake, "Stake too low");
            require(msg.value == 0, "Native token sent for ERC20 match");
            IERC20(game.stakeToken).safeTransferFrom(_msgSender(), address(this), stakeAmount);
            actualStake = stakeAmount;
        }

        matchId = nextMatchId++;
        matches[matchId] = AMPTypes.Match({
            gameId: gameId,
            playerA: _msgSender(),
            playerB: address(0),
            stakeAmount: actualStake,
            state: AMPTypes.MatchState.OPEN,
            createdAt: block.timestamp
        });

        emit MatchCreated(matchId, gameId, _msgSender(), actualStake);
    }

    /**
     * @notice Joins an existing OPEN match.
     */
    function joinMatch(uint256 matchId) external payable nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        require(m.state == AMPTypes.MatchState.OPEN, "Match not open");
        
        AMPTypes.Game storage game = games[m.gameId];

        if (game.stakeToken == address(0)) {
            require(msg.value == m.stakeAmount, "Stake mismatch");
        } else {
            require(msg.value == 0, "Native token sent for ERC20 match");
            IERC20(game.stakeToken).safeTransferFrom(_msgSender(), address(this), m.stakeAmount);
        }

        m.playerB = _msgSender();
        m.state = AMPTypes.MatchState.READY;

        emit MatchJoined(matchId, _msgSender());
    }

    /**
     * @notice Pays out collected fees
     */
    function withdrawFees(address token) external onlyOwner nonReentrant {
        uint256 amount = feeBalances[token];
        require(amount > 0, "No fees to withdraw");
        feeBalances[token] = 0;

        if (token == address(0)) {
            (bool success, ) = owner.call{value: amount}("");
            require(success, "Transfer Failed");
        } else {
            IERC20(token).safeTransfer(owner, amount);
        }

        emit FeesWithdrawn(token, amount, owner);
    }

    /**
     * @notice Allows owner to set the settlement contract address.
     */
    function setSettlement(address settlementAddress) external onlyOwner {
        settlement = settlementAddress;
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
                m.state == AMPTypes.MatchState.OPEN ||
                m.state == AMPTypes.MatchState.DISPUTED,
            "Match not settlable"
        );
        m.state = newState;

        AMPTypes.Game storage game = games[m.gameId];
        feeBalances[game.stakeToken] += protocolFee;

        for (uint i = 0; i < recipients.length; i++) {
            if (amounts[i] > 0) {
                if (game.stakeToken == address(0)) {
                    (bool success, ) = recipients[i].call{value: amounts[i]}("");
                    require(success, "Transfer failed");
                } else {
                    IERC20(game.stakeToken).safeTransfer(recipients[i], amounts[i]);
                }
            }
        }
    }
}
