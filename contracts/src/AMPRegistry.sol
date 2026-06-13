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

    error NoReentrancy();
    error NotSettlement();
    error TooManyVerifiers();
    error RTModeRequiresArbiter();
    error NotGameAdmin();
    error TimeoutTooShort();
    error TimeoutTooLong();
    error StakeTooLow();
    error NativeTokenSentForERC20();
    error MatchNotOpen();
    error CannotJoinOwnMatch();
    error StakeMismatch();
    error NotPlayerA();
    error DisputeTimeoutNotReached();
    error MatchNotExpirable();
    error NotExpiredYet();
    error NoFeesToWithdraw();
    error TransferFailed();
    error MatchNotSettlable();
    error NoPendingWithdrawal();
    error MatchAlreadyExists();

    address public settlement;
    uint256 public nextGameId;

    mapping(address => uint256) public feeBalances;
    mapping(address => mapping(address => uint256)) public pendingWithdrawals;

    bool private locked;

    modifier nonReentrant() {
        if (locked) revert NoReentrancy();
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
    event WithdrawalClaimed(address indexed token, address indexed account, uint256 amount);

    modifier onlySettlement() {
        if (_msgSender() != settlement) revert NotSettlement();
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
        if (verifiers.length > 10) revert TooManyVerifiers();
        if (mode == AMPTypes.SettlementMode.RT_HASH_AGREE) {
            if (arbiter == address(0)) revert RTModeRequiresArbiter();
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
        if (_msgSender() != games[gameId].admin) revert NotGameAdmin();
        if (timeoutSeconds < 5 minutes) revert TimeoutTooShort();
        if (timeoutSeconds > 30 days) revert TimeoutTooLong();
        games[gameId].matchTimeout = timeoutSeconds;
    }

    function updateGameVerifiers(uint256 gameId, address[] calldata verifiers) external {
        if (_msgSender() != games[gameId].admin) revert NotGameAdmin();
        if (verifiers.length > 10) revert TooManyVerifiers();
        _clearVerifierMapping(gameId);
        games[gameId].verifiers = verifiers;
        _syncVerifierMapping(gameId, verifiers);
        emit GameVerifiersUpdated(gameId);
    }

    function getGameVerifiers(uint256 gameId) external view returns (address[] memory) {
        return games[gameId].verifiers;
    }

    function createMatch(uint256 gameId, uint256 matchId, uint256 stakeAmount)
        external
        payable
        whenNotPaused
        nonReentrant
    {
        if (matches[matchId].playerA != address(0)) revert MatchAlreadyExists();
        AMPTypes.Game storage game = games[gameId];

        uint256 actualStake;
        if (game.stakeToken == address(0)) {
            if (msg.value < game.minStake) revert StakeTooLow();
            actualStake = msg.value;
        } else {
            if (stakeAmount < game.minStake) revert StakeTooLow();
            if (msg.value != 0) revert NativeTokenSentForERC20();
            uint256 balBefore = IERC20(game.stakeToken).balanceOf(address(this));
            IERC20(game.stakeToken).safeTransferFrom(_msgSender(), address(this), stakeAmount);
            actualStake = IERC20(game.stakeToken).balanceOf(address(this)) - balBefore;
        }

        matches[matchId] = AMPTypes.Match({
            gameId: gameId,
            playerA: _msgSender(),
            playerB: address(0),
            stakeAmount: actualStake,
            state: AMPTypes.MatchState.OPEN,
            createdAt: uint64(block.timestamp)
        });

        emit MatchCreated(matchId, gameId, _msgSender(), actualStake);
    }

    function joinMatch(uint256 matchId) external payable whenNotPaused nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        if (m.state != AMPTypes.MatchState.OPEN) revert MatchNotOpen();
        if (_msgSender() == m.playerA) revert CannotJoinOwnMatch();

        AMPTypes.Game storage game = games[m.gameId];

        if (game.stakeToken == address(0)) {
            if (msg.value != m.stakeAmount) revert StakeMismatch();
        } else {
            if (msg.value != 0) revert NativeTokenSentForERC20();
            uint256 balBefore = IERC20(game.stakeToken).balanceOf(address(this));
            IERC20(game.stakeToken).safeTransferFrom(_msgSender(), address(this), m.stakeAmount);
            uint256 received = IERC20(game.stakeToken).balanceOf(address(this)) - balBefore;
            if (received != m.stakeAmount) revert StakeMismatch();
        }

        m.playerB = _msgSender();
        m.state = AMPTypes.MatchState.READY;

        emit MatchJoined(matchId, _msgSender());
    }

    function cancelMatch(uint256 matchId) external nonReentrant whenNotPaused {
        AMPTypes.Match storage m = matches[matchId];
        if (m.state != AMPTypes.MatchState.OPEN) revert MatchNotOpen();
        if (_msgSender() != m.playerA) revert NotPlayerA();

        m.state = AMPTypes.MatchState.SETTLED;

        AMPTypes.Game storage game = games[m.gameId];
        uint256 refund = m.stakeAmount;
        m.stakeAmount = 0;

        pendingWithdrawals[game.stakeToken][m.playerA] += refund;

        emit MatchCancelled(matchId, m.playerA, refund);
    }

    function expireMatch(uint256 matchId) external nonReentrant {
        AMPTypes.Match storage m = matches[matchId];
        AMPTypes.Game storage game = games[m.gameId];
        if (m.state == AMPTypes.MatchState.DISPUTED) {
            if (block.timestamp < m.createdAt + game.matchTimeout * 3) revert DisputeTimeoutNotReached();
        } else {
            if (m.state != AMPTypes.MatchState.OPEN && m.state != AMPTypes.MatchState.READY) {
                revert MatchNotExpirable();
            }
            if (block.timestamp < m.createdAt + game.matchTimeout) revert NotExpiredYet();
        }

        m.state = AMPTypes.MatchState.EXPIRED;

        address token = game.stakeToken;
        uint256 amount = m.stakeAmount;
        pendingWithdrawals[token][m.playerA] += amount;
        if (m.playerB != address(0)) {
            pendingWithdrawals[token][m.playerB] += amount;
        }

        emit MatchExpired(matchId);
    }

    function withdrawFees(address token) external onlyOwner nonReentrant {
        uint256 amount = feeBalances[token];
        if (amount == 0) revert NoFeesToWithdraw();
        feeBalances[token] = 0;

        if (token == address(0)) {
            (bool success,) = owner().call{value: amount}("");
            if (!success) revert TransferFailed();
        } else {
            IERC20(token).safeTransfer(owner(), amount);
        }

        emit FeesWithdrawn(token, amount, owner());
    }

    function withdraw(address token) external nonReentrant {
        uint256 amount = pendingWithdrawals[token][_msgSender()];
        if (amount == 0) revert NoPendingWithdrawal();
        pendingWithdrawals[token][_msgSender()] = 0;

        if (token == address(0)) {
            (bool success,) = _msgSender().call{value: amount}("");
            if (!success) revert TransferFailed();
        } else {
            IERC20(token).safeTransfer(_msgSender(), amount);
        }

        emit WithdrawalClaimed(token, _msgSender(), amount);
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
        if (
            m.state != AMPTypes.MatchState.READY && m.state != AMPTypes.MatchState.OPEN
                && m.state != AMPTypes.MatchState.DISPUTED
        ) revert MatchNotSettlable();
        m.state = newState;

        AMPTypes.Game storage game = games[m.gameId];
        address token = game.stakeToken;
        feeBalances[token] += protocolFee;

        for (uint256 i = 0; i < recipients.length; i++) {
            if (amounts[i] > 0) {
                pendingWithdrawals[token][recipients[i]] += amounts[i];
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
