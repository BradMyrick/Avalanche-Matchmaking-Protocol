// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "openzeppelin-contracts/contracts/utils/cryptography/ECDSA.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/EIP712.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/utils/Pausable.sol";
import "openzeppelin-contracts/contracts/utils/ReentrancyGuard.sol";
import "openzeppelin-contracts/contracts/token/ERC20/IERC20.sol";
import "openzeppelin-contracts/contracts/token/ERC20/utils/SafeERC20.sol";
import "openzeppelin-contracts/contracts/utils/Address.sol";

/// @title AMPTournamentCup
/// @notice Sponsor-funded tournament prize-pool escrow. A sponsor funds a prize
///         pool (native AVAX or ERC-20), commits to a payout structure (basis
///         points per placement), and a verifier attests the final standings via
///         an EIP-712 signature. Winners pull-claim their share.
///
///         MVP model for the AMP tournament engine. Standalone — does not touch
///         the deployed `AMPRegistry` / `AMPSettlement` wagering contracts.
///
/// @dev Security model:
///      - Pull-payment only (Rule 6, anti-MEV): winners call `claimPrize`.
///      - `nonReentrant` + checks-effects-interactions on every state change.
///      - EIP-712 verifier attestation reuses the AMPSettlement pattern.
///      - `Ownable2Step` (transferable to the TimelockController post-deploy).
///      - payoutBps must sum to exactly 10000; placements bounded to MAX_PLACEMENTS.
contract AMPTournamentCup is Ownable2Step, Pausable, EIP712, ReentrancyGuard {
    using SafeERC20 for IERC20;
    using Address for address payable;

    enum TournamentState {
        OPEN, // funded, awaiting verifier finalization
        FINALIZED, // winners attested; claims open
        COMPLETE, // all winners have claimed
        CANCELLED // sponsor refunded
    }

    struct Tournament {
        address sponsor;
        address token; // address(0) == native AVAX
        uint256 prizePool; // actual received amount (fee-on-transfer safe)
        uint16[] payoutBps; // per-placement share, index 0 == 1st. Sum == 10000.
        address verifier; // sole authorized attester for this cup
        address[] winners; // attested winners, index-aligned to payoutBps
        TournamentState state;
        uint64 createdAt;
        uint64 finalizeDeadline; // after this, OPEN cups may be refunded if still unfinalized
    }

    uint16 public constant MAX_PROTOCOL_FEE_BPS = 500;
    uint8 public constant MAX_PLACEMENTS = 16;

    error ZeroAddress();
    error InvalidPayout();
    error PayoutSumInvalid();
    error TooManyPlacements();
    error DeadlineInPast();
    error NotSponsor();
    error NotOpen();
    error NotFinalized();
    error DeadlineNotPassed();
    error InvalidSignature();
    error WinnerCountMismatch();
    error NotWinner();
    error AlreadyClaimed();
    error TransferFailed();
    error NoAVAXExpected();
    error InvalidFee();
    error DeadlinePassedImplicit();

    event TournamentCreated(
        uint256 indexed tournamentId, address indexed sponsor, address token, uint256 prizePool, address verifier
    );
    event TournamentFinalized(uint256 indexed tournamentId, bytes32 indexed winnersRoot);
    event PrizeClaimed(uint256 indexed tournamentId, uint256 indexed placement, address winner, uint256 amount);
    event TournamentCancelled(uint256 indexed tournamentId, uint256 refund);
    event ProtocolFeeUpdated(uint16 feeBps);
    event ProtocolFeeRecipientUpdated(address indexed recipient);

    bytes32 public constant TOURNAMENT_RESULT_TYPEHASH =
        keccak256("TournamentResult(uint256 tournamentId,address[] winners)");

    uint256 public nextTournamentId = 1;
    uint16 public protocolFeeBps;
    address public protocolFeeRecipient;

    mapping(uint256 => Tournament) private _tournaments;
    // tournamentId => placement => claimed
    mapping(uint256 => mapping(uint256 => bool)) public claimed;

    constructor(address _feeRecipient) Ownable(msg.sender) EIP712("AMPTournamentCup", "1") {
        if (_feeRecipient == address(0)) revert ZeroAddress();
        protocolFeeRecipient = _feeRecipient;
        protocolFeeBps = 0; // MVP: no protocol fee by default
    }

    // ───────────────────────── Config (owner only) ─────────────────────────

    function updateProtocolFeeBps(uint16 feeBps) external onlyOwner {
        if (feeBps > MAX_PROTOCOL_FEE_BPS) revert InvalidFee();
        protocolFeeBps = feeBps;
        emit ProtocolFeeUpdated(feeBps);
    }

    function updateProtocolFeeRecipient(address recipient) external onlyOwner {
        if (recipient == address(0)) revert ZeroAddress();
        protocolFeeRecipient = recipient;
        emit ProtocolFeeRecipientUpdated(recipient);
    }

    function pause() external onlyOwner {
        _pause();
    }

    function unpause() external onlyOwner {
        _unpause();
    }

    // ───────────────────────── View ─────────────────────────

    function getTournament(uint256 id) external view returns (Tournament memory) {
        return _tournaments[id];
    }

    /// EIP-712 domain separator (delegates to OZ audited impl).
    function domainSeparator() external view returns (bytes32) {
        return _domainSeparatorV4();
    }

    // ───────────────────────── Sponsor: create + cancel ─────────────────────────

    /// @notice Fund a prize pool with native AVAX and commit to a payout structure.
    ///         For ERC-20 prizes, use `createTournamentERC20`.
    /// @param payoutBps    Per-placement share in bps. index 0 = 1st place. Must sum to 10000.
    /// @param verifier     Address authorized to attest winners (EIP-712 signer).
    /// @param finalizeDeadline Timestamp after which an unfinalized cup may be refunded.
    function createTournament(uint16[] calldata payoutBps, address verifier, uint64 finalizeDeadline)
        external
        payable
        whenNotPaused
        nonReentrant
        returns (uint256 tournamentId)
    {
        if (msg.value == 0) revert InvalidPayout();
        if (verifier == address(0)) revert ZeroAddress();
        if (payoutBps.length == 0 || payoutBps.length > MAX_PLACEMENTS) revert InvalidPayout();
        if (finalizeDeadline <= block.timestamp) revert DeadlineInPast();

        uint256 sum;
        for (uint256 i = 0; i < payoutBps.length; i++) {
            if (payoutBps[i] == 0 || payoutBps[i] > 10000) revert InvalidPayout();
            sum += payoutBps[i];
        }
        if (sum != 10000) revert PayoutSumInvalid();

        tournamentId = _create(msg.sender, address(0), msg.value, payoutBps, verifier, finalizeDeadline);
    }

    /// @notice ERC-20 variant: funds the pool with an explicit deposit amount.
    function createTournamentERC20(
        address token,
        uint256 amount,
        uint16[] calldata payoutBps,
        address verifier,
        uint64 finalizeDeadline
    ) external whenNotPaused nonReentrant returns (uint256 tournamentId) {
        if (token == address(0)) revert ZeroAddress();
        if (amount == 0) revert InvalidPayout();
        if (verifier == address(0)) revert ZeroAddress();
        if (payoutBps.length == 0 || payoutBps.length > MAX_PLACEMENTS) revert InvalidPayout();
        if (finalizeDeadline <= block.timestamp) revert DeadlineInPast();

        uint256 sum;
        for (uint256 i = 0; i < payoutBps.length; i++) {
            if (payoutBps[i] == 0 || payoutBps[i] > 10000) revert InvalidPayout();
            sum += payoutBps[i];
        }
        if (sum != 10000) revert PayoutSumInvalid();

        // Fee-on-transfer safe: measure the actual balance delta.
        uint256 balBefore = IERC20(token).balanceOf(address(this));
        IERC20(token).safeTransferFrom(msg.sender, address(this), amount);
        uint256 received = IERC20(token).balanceOf(address(this)) - balBefore;
        if (received == 0) revert InvalidPayout();

        tournamentId = _create(msg.sender, token, received, payoutBps, verifier, finalizeDeadline);
    }

    /// @notice Refund the sponsor. Allowed by the sponsor while OPEN and before
    ///         the deadline, OR by anyone once the deadline has passed unfinalized.
    function cancelTournament(uint256 tournamentId) external nonReentrant {
        Tournament storage t = _tournaments[tournamentId];
        if (t.state != TournamentState.OPEN) revert NotOpen();
        bool deadlinePassed = block.timestamp > t.finalizeDeadline;
        if (!deadlinePassed && msg.sender != t.sponsor) revert NotSponsor();

        t.state = TournamentState.CANCELLED;
        uint256 refund = t.prizePool;
        // C-E-I: state mutated before external transfer.
        _send(t.token, payable(t.sponsor), refund);

        emit TournamentCancelled(tournamentId, refund);
    }

    // ───────────────────────── Verifier-attested finalization ─────────────────────────

    /// @notice Attest the final standings. Anyone may submit; validity comes from
    ///         the verifier's EIP-712 signature over (tournamentId, winners).
    function finalizeTournament(uint256 tournamentId, address[] calldata winners, bytes calldata signature)
        external
        whenNotPaused
        nonReentrant
    {
        Tournament storage t = _tournaments[tournamentId];
        if (t.state != TournamentState.OPEN) revert NotOpen();
        if (block.timestamp > t.finalizeDeadline) revert DeadlinePassedImplicit();
        if (winners.length != t.payoutBps.length) revert WinnerCountMismatch();

        bytes32 winnersRoot = _hashWinners(winners);
        bytes32 structHash = keccak256(abi.encode(TOURNAMENT_RESULT_TYPEHASH, tournamentId, winnersRoot));
        bytes32 digest = _hashTypedDataV4(structHash);
        address signer = ECDSA.recover(digest, signature);
        if (signer != t.verifier) revert InvalidSignature();
        if (signer == address(0)) revert InvalidSignature();

        // Reject duplicate/dust addresses (a placement cannot point to address(0)).
        for (uint256 i = 0; i < winners.length; i++) {
            if (winners[i] == address(0)) revert ZeroAddress();
            t.winners.push(winners[i]);
        }
        t.state = TournamentState.FINALIZED;

        emit TournamentFinalized(tournamentId, winnersRoot);
    }

    // Custom error kept below finalizeTournament so the revert reads cleanly.
    // (Declared with the other errors at the top of the contract.)

    // ───────────────────────── Winner: pull-claim ─────────────────────────

    /// @notice Claim a placement's prize. Caller must be the attested winner at `placement`.
    function claimPrize(uint256 tournamentId, uint256 placement) external whenNotPaused nonReentrant {
        Tournament storage t = _tournaments[tournamentId];
        if (t.state != TournamentState.FINALIZED && t.state != TournamentState.COMPLETE) revert NotFinalized();
        if (placement >= t.winners.length) revert NotWinner();
        if (t.winners[placement] != msg.sender) revert NotWinner();
        if (claimed[tournamentId][placement]) revert AlreadyClaimed();

        // CHECKS done. EFFECTS:
        claimed[tournamentId][placement] = true;
        uint256 share = (t.prizePool * t.payoutBps[placement]) / 10000;
        uint256 fee = (share * protocolFeeBps) / 10000;
        uint256 payout = share - fee;

        // Mark COMPLETE if this was the last unclaimed placement.
        bool allClaimed = true;
        for (uint256 i = 0; i < t.winners.length; i++) {
            if (!claimed[tournamentId][i]) {
                allClaimed = false;
                break;
            }
        }
        if (allClaimed) {
            t.state = TournamentState.COMPLETE;
        }

        // INTERACTION (after all effects).
        if (payout > 0) _send(t.token, payable(msg.sender), payout);
        if (fee > 0) _send(t.token, payable(protocolFeeRecipient), fee);

        emit PrizeClaimed(tournamentId, placement, msg.sender, payout);
    }

    // ───────────────────────── Internals ─────────────────────────

    function _create(
        address sponsor,
        address token,
        uint256 received,
        uint16[] calldata payoutBps,
        address verifier,
        uint64 finalizeDeadline
    ) internal returns (uint256 tournamentId) {
        tournamentId = nextTournamentId++;
        Tournament storage t = _tournaments[tournamentId];
        t.sponsor = sponsor;
        t.token = token;
        t.prizePool = received;
        t.verifier = verifier;
        t.state = TournamentState.OPEN;
        t.createdAt = uint64(block.timestamp);
        t.finalizeDeadline = finalizeDeadline;
        for (uint256 i = 0; i < payoutBps.length; i++) {
            t.payoutBps.push(payoutBps[i]);
        }
        emit TournamentCreated(tournamentId, sponsor, token, received, verifier);
    }

    /// EIP-712 encoding of `address[]`: keccak of the concatenated 32-byte words.
    function _hashWinners(address[] calldata winners) internal pure returns (bytes32) {
        bytes32[] memory words = new bytes32[](winners.length);
        for (uint256 i = 0; i < winners.length; i++) {
            words[i] = bytes32(uint256(uint160(winners[i])));
        }
        return keccak256(abi.encodePacked(words));
    }

    function _send(address token, address payable to, uint256 amount) internal {
        if (amount == 0) return;
        if (token == address(0)) {
            to.sendValue(amount);
        } else {
            IERC20(token).safeTransfer(to, amount);
        }
    }
}
