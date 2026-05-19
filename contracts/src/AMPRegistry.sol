// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

import "./AMPTypes.sol";
import "openzeppelin-contracts/contracts/token/ERC20/IERC20.sol";
import "openzeppelin-contracts/contracts/token/ERC20/utils/SafeERC20.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/metatx/ERC2771Context.sol";
import "openzeppelin-contracts/contracts/utils/Pausable.sol";

contract AMPRegistry is ERC2771Context, Ownable2Step, Pausable {
    using SafeERC20 for IERC20;

    address public settlement;
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
    mapping(uint256 => mapping(address => bool)) public gameVerifiers;

    event GameRegistered(uint256 indexed gameId, address indexed admin, AMPTypes.SettlementMode mode);
    event GameVerifiersUpdated(uint256 indexed gameId);
    event MatchCreated(uint256 indexed matchId, uint256 indexed gameId, address indexed playerA, uint256 stakeAmount);
    event MatchJoined(uint256 indexed matchId, address indexed playerB);
    event MatchCancelled(uint256 indexed matchId, address indexed playerA, uint256 refundAmount);
    event MatchExpired(uint256 indexed matchId);
    event FeesWithdrawn(address indexed token, uint256 amount, address indexed wallet);
    event SettlementUpdated(address indexed settlementAddress);

    modifier onlySettlement() {
        require(_msgSender() == settlement, "Not settlement");
        _;
    }

    constructor(address trustedForwarder) ERC2771Context(trustedForwarder) Ownable(_msgSender()) {}

    function registerGame(
        AMPTypes.SettlementMode mode,
        address[] calldata verifiers,
        uint256 minStake,
        address stakeToken,
        address arbiter
    ) external whenNotPaused returns (uint256 gameId) {
        require(verifiers.length <= 10, "Too many verifiers");
        if (mode == AMPTypes.SettlementMode.RT_HASH_AGREE) {
            require(arbiter != address(0), "RT mode requires arbiter");
        }
        gameId = nextGameId++;
        games[gameId] = AMPTypes.Game({
            admin: _msgSender(),
            mode: mode,
            verifiers: verifiers,
            minStake: minStake,
            stakeToken: stakeToken,
            arbiter: arbiter,
            matchTimeout: 1 hours
        });
        _syncVerifierMapping(gameId, verifiers);

        emit GameRegistered(gameId, _msgSender(), mode);
    }

    function setMatchTimeout(uint256 gameId, uint256 timeoutSeconds) external {
        require(_msgSender() == games[gameId].admin, "Not game admin");
        require(timeoutSeconds >= 5 minutes, "Timeout too short");
        require(timeoutSeconds <= 30 days, "Timeout too long");
        games[gameId].matchTimeout = timeoutSeconds;
    }

    function updateGameVerifiers(uint256 gameId, address[] calldata verifiers) external {
        require(_msgSender() == games[gameId].admin, "Not game admin");
        require(verifiers.length <= 10, "Too many verifiers");
        _clearVerifierMapping(gameId);
        games[gameId].verifiers = verifiers;
        _syncVerifierMapping(gameId, verifiers);
        emit GameVerifiersUpdated(gameId);
    }

    function getGameVerifiers(uint256 gameId) external view returns (address[] memory) {
        return games[gameId].verifiers;
    }

    function createMatch(uint256 gameId, uint256 stakeAmount)
        external
        payable
        whenNotPaused
        nonReentrant
        returns (uint256 matchId)
    {
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

    function joinMatch(uint256 matchId) external payable whenNotPaused nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        require(m.state == AMPTypes.MatchState.OPEN, "Match not open");
        require(_msgSender() != m.playerA, "Cannot join own match");

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

    function cancelMatch(uint256 matchId) external nonReentrant whenNotPaused {
        AMPTypes.Match storage m = matches[matchId];
        require(m.state == AMPTypes.MatchState.OPEN, "Match not open");
        require(_msgSender() == m.playerA, "Not player A");

        m.state = AMPTypes.MatchState.SETTLED;

        AMPTypes.Game storage game = games[m.gameId];
        uint256 refund = m.stakeAmount;
        m.stakeAmount = 0;

        if (game.stakeToken == address(0)) {
            (bool success,) = m.playerA.call{value: refund}("");
            require(success, "Refund failed");
        } else {
            IERC20(game.stakeToken).safeTransfer(m.playerA, refund);
        }

        emit MatchCancelled(matchId, m.playerA, refund);
    }

    function expireMatch(uint256 matchId) external nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        AMPTypes.Game storage game = games[m.gameId];
        if (m.state == AMPTypes.MatchState.DISPUTED) {
            require(block.timestamp >= m.createdAt + game.matchTimeout * 3, "Dispute timeout not reached");
        } else {
            require(m.state == AMPTypes.MatchState.OPEN || m.state == AMPTypes.MatchState.READY, "Match not expirable");
            require(block.timestamp >= m.createdAt + game.matchTimeout, "Not expired yet");
        }

        m.state = AMPTypes.MatchState.EXPIRED;

        address[] memory recipients = new address[](2);
        recipients[0] = m.playerA;
        recipients[1] = m.playerB;
        uint256[] memory amounts = new uint256[](2);

        amounts[0] = m.stakeAmount;
        amounts[1] = m.playerB == address(0) ? 0 : m.stakeAmount;

        for (uint256 i = 0; i < recipients.length; i++) {
            if (amounts[i] > 0) {
                if (game.stakeToken == address(0)) {
                    (bool success,) = recipients[i].call{value: amounts[i]}("");
                    require(success, "Refund failed");
                } else {
                    IERC20(game.stakeToken).safeTransfer(recipients[i], amounts[i]);
                }
            }
        }

        emit MatchExpired(matchId);
    }

    function withdrawFees(address token) external onlyOwner nonReentrant {
        uint256 amount = feeBalances[token];
        require(amount > 0, "No fees to withdraw");
        feeBalances[token] = 0;

        if (token == address(0)) {
            (bool success,) = owner().call{value: amount}("");
            require(success, "Transfer Failed");
        } else {
            IERC20(token).safeTransfer(owner(), amount);
        }

        emit FeesWithdrawn(token, amount, owner());
    }

    function setSettlement(address settlementAddress) external onlyOwner {
        settlement = settlementAddress;
        emit SettlementUpdated(settlementAddress);
    }

    function settleMatch(
        uint256 matchId,
        AMPTypes.MatchState newState,
        address[] calldata recipients,
        uint256[] calldata amounts,
        uint256 protocolFee
    ) external onlySettlement nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        require(
            m.state == AMPTypes.MatchState.READY || m.state == AMPTypes.MatchState.OPEN
                || m.state == AMPTypes.MatchState.DISPUTED,
            "Match not settlable"
        );
        m.state = newState;

        AMPTypes.Game storage game = games[m.gameId];
        feeBalances[game.stakeToken] += protocolFee;

        for (uint256 i = 0; i < recipients.length; i++) {
            if (amounts[i] > 0) {
                if (game.stakeToken == address(0)) {
                    (bool success,) = recipients[i].call{value: amounts[i]}("");
                    require(success, "Transfer failed");
                } else {
                    IERC20(game.stakeToken).safeTransfer(recipients[i], amounts[i]);
                }
            }
        }
    }

    function pause() external onlyOwner {
        _pause();
    }

    function unpause() external onlyOwner {
        _unpause();
    }

    function isVerifier(uint256 gameId, address addr) external view returns (bool) {
        return gameVerifiers[gameId][addr];
    }

    function _clearVerifierMapping(uint256 gameId) internal {
        address[] storage old = games[gameId].verifiers;
        for (uint256 i = 0; i < old.length; i++) {
            gameVerifiers[gameId][old[i]] = false;
        }
    }

    function _syncVerifierMapping(uint256 gameId, address[] calldata verifiers) internal {
        for (uint256 i = 0; i < verifiers.length; i++) {
            gameVerifiers[gameId][verifiers[i]] = true;
        }
    }

    function _msgSender() internal view virtual override(Context, ERC2771Context) returns (address) {
        return ERC2771Context._msgSender();
    }

    function _msgData() internal view virtual override(Context, ERC2771Context) returns (bytes calldata) {
        return ERC2771Context._msgData();
    }

    function _contextSuffixLength() internal view virtual override(Context, ERC2771Context) returns (uint256) {
        return ERC2771Context._contextSuffixLength();
    }
}
