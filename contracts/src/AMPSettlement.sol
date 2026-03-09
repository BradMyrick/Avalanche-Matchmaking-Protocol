// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "./AMPTypes.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/ECDSA.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/MessageHashUtils.sol";
import "openzeppelin-contracts/contracts/access/Ownable.sol";
import "openzeppelin-contracts/contracts/metatx/ERC2771Context.sol";

interface IAMPRegistry {
    function games(
        uint256 id
    )
        external
        view
        returns (
            address admin,
            AMPTypes.SettlementMode mode,
            uint256 minStake,
            address stakeToken,
            address arbiter
        );

    function getGameVerifiers(
        uint256 id
    ) external view returns (address[] memory);

    function matches(
        uint256 id
    )
        external
        view
        returns (
            uint256,
            address,
            address,
            uint256,
            AMPTypes.MatchState,
            uint256
        );

    function settleMatch(
        uint256 matchId,
        AMPTypes.MatchState newState,
        address[] calldata recipients,
        uint256[] calldata amounts,
        uint256 protocolFee
    ) external;
}

/**
 * @title AMPSettlement
 * @notice Handles match result submission, verification, and payouts.
 */
contract AMPSettlement is ERC2771Context, Ownable {
    address public immutable registry;
    uint16 public protocolFeeBps;
    address public protocolFeeRecipient;

    mapping(uint256 => AMPTypes.Settlement) public settlements;
    mapping(uint256 => mapping(address => AMPTypes.RealTimeHashResult))
        public rtResults;

    event MatchSettled(
        uint256 indexed matchId,
        AMPTypes.OutcomeCode outcome,
        uint256 payout
    );
    event MatchDisputed(uint256 indexed matchId);
    event ProtocolFeeUpdated(uint16 feeBps);
    event ProtocolFeeRecipientUpdated(address indexed recipient);
    event MatchDisputeResolved(uint256 indexed matchId, AMPTypes.OutcomeCode enforcedOutcome);

    constructor(address _registry, address trustedForwarder) ERC2771Context(trustedForwarder) Ownable(_msgSender()) {
        registry = _registry;
        protocolFeeBps = 100; // 1% default
        protocolFeeRecipient = _msgSender();
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

    function updateProtocolFeeBps(uint16 feeBps) external onlyOwner {
        require(feeBps <= 10000, "Invalid bps");
        protocolFeeBps = feeBps;
        emit ProtocolFeeUpdated(feeBps);
    }

    function updateProtocolFeeRecipient(address recipient) external onlyOwner {
        require(recipient != address(0), "Invalid recipient");
        protocolFeeRecipient = recipient;
        emit ProtocolFeeRecipientUpdated(recipient);
    }

    /**
     * @notice Submits a signed result from an async verifier.
     */
    function submitAsyncResult(
        uint256 matchId,
        AMPTypes.AsyncResult calldata result
    ) external {
        require(result.matchId == matchId, "Match ID mismatch");

        (
            uint256 gameId,
            address playerA,
            address playerB,
            uint256 stakeAmount,
            AMPTypes.MatchState state,
            
        ) = IAMPRegistry(registry).matches(matchId);

        (, AMPTypes.SettlementMode mode, , , ) = IAMPRegistry(registry).games(
            gameId
        );
        address[] memory verifiers = IAMPRegistry(registry).getGameVerifiers(
            gameId
        );

        require(mode == AMPTypes.SettlementMode.ASYNC_VERIFIER, "Wrong mode");
        require(
            state == AMPTypes.MatchState.READY ||
                state == AMPTypes.MatchState.OPEN,
            "Match not settlable"
        );

        // Verify signature
        bytes32 structHash = keccak256(
            abi.encode(result.matchId, result.outcome, result.transcriptHash)
        );
        bytes32 digest = MessageHashUtils.toEthSignedMessageHash(structHash);
        address signer = ECDSA.recover(digest, result.signature);

        bool isVerifier = false;
        for (uint i = 0; i < verifiers.length; i++) {
            if (verifiers[i] == signer) {
                isVerifier = true;
                break;
            }
        }
        require(isVerifier, "Invalid verifier signature");

        settlements[matchId] = AMPTypes.Settlement({
            matchId: matchId,
            outcome: result.outcome,
            transcriptHash: result.transcriptHash,
            settledAt: block.timestamp
        });

        _payout(matchId, playerA, playerB, stakeAmount, result.outcome);

        emit MatchSettled(matchId, result.outcome, stakeAmount * 2);
    }

    /**
     * @notice Submits a result hash in real-time agreement mode.
     */
    function submitRealTimeHashResult(
        uint256 matchId,
        AMPTypes.RealTimeHashResult calldata result
    ) external {
        require(result.matchId == matchId, "Match ID mismatch");
        (
            uint256 gameId,
            address playerA,
            address playerB,
            uint256 stakeAmount,
            AMPTypes.MatchState state,
            
        ) = IAMPRegistry(registry).matches(matchId);

        (, AMPTypes.SettlementMode mode, , , ) = IAMPRegistry(registry).games(
            gameId
        );

        require(mode == AMPTypes.SettlementMode.RT_HASH_AGREE, "Wrong mode");
        require(state == AMPTypes.MatchState.READY, "Match not ready");
        require(_msgSender() == playerA || _msgSender() == playerB, "Not a player");

        rtResults[matchId][_msgSender()] = result;

        address otherPlayer = _msgSender() == playerA ? playerB : playerA;
        AMPTypes.RealTimeHashResult memory otherResult = rtResults[matchId][otherPlayer];

        if (otherResult.outcome != AMPTypes.OutcomeCode.NONE) {
            // Other player has already submitted
            if (
                result.outcome == otherResult.outcome &&
                result.transcriptHash == otherResult.transcriptHash
            ) {
                // Agreement, settle match
                settlements[matchId] = AMPTypes.Settlement({
                    matchId: matchId,
                    outcome: result.outcome,
                    transcriptHash: result.transcriptHash,
                    settledAt: block.timestamp
                });

                _payout(matchId, playerA, playerB, stakeAmount, result.outcome);
                emit MatchSettled(matchId, result.outcome, stakeAmount * 2);
            } else {
                // Disagreement, dispute match
                // We fake a payout call with a DISPUTED match state, transferring 0 to lock funds
                // Or simply change state to DISPUTED via a specialized registry call.
                // However registry only uses settleMatch to change state.
                address[] memory recipients = new address[](0);
                uint256[] memory amounts = new uint256[](0);
                IAMPRegistry(registry).settleMatch(
                    matchId,
                    AMPTypes.MatchState.DISPUTED,
                    recipients,
                    amounts,
                    0
                );
                emit MatchDisputed(matchId);
            }
        }
    }

    /**
     * @notice Resolves a disputed match and forces an outcome.
     */
    function resolveDispute(uint256 matchId, AMPTypes.OutcomeCode enforcedOutcome) external {
        (
            uint256 gameId,
            address playerA,
            address playerB,
            uint256 stakeAmount,
            AMPTypes.MatchState state,
            
        ) = IAMPRegistry(registry).matches(matchId);

        (, , , , address arbiter) = IAMPRegistry(registry).games(gameId);

        require(state == AMPTypes.MatchState.DISPUTED, "Not disputed");
        require(_msgSender() == arbiter, "Not arbiter");

        settlements[matchId] = AMPTypes.Settlement({
            matchId: matchId,
            outcome: enforcedOutcome,
            transcriptHash: bytes32(0),
            settledAt: block.timestamp
        });

        _payout(matchId, playerA, playerB, stakeAmount, enforcedOutcome);
        emit MatchDisputeResolved(matchId, enforcedOutcome);
        emit MatchSettled(matchId, enforcedOutcome, stakeAmount * 2);
    }

    /**
     * @notice Internal helper to handle payouts (AVA or ERC20).
     */
    function _payout(
        uint256 matchId,
        address playerA,
        address playerB,
        uint256 stakeAmount,
        AMPTypes.OutcomeCode outcome
    ) internal {
        uint256 totalPool = stakeAmount * 2;
        if (playerB == address(0)) {
            totalPool = stakeAmount; // only Player A staked if it's OPEN and cancelled
        }

        uint256 protocolFee = (totalPool * protocolFeeBps) / 10000;
        uint256 payoutAmount = totalPool - protocolFee;

        address[] memory recipients = new address[](2);
        recipients[0] = playerA;
        recipients[1] = playerB;
        uint256[] memory amounts = new uint256[](2);

        if (outcome == AMPTypes.OutcomeCode.WIN_A) {
            amounts[0] = payoutAmount;
            amounts[1] = 0;
        } else if (outcome == AMPTypes.OutcomeCode.WIN_B) {
            amounts[0] = 0;
            amounts[1] = payoutAmount;
        } else if (outcome == AMPTypes.OutcomeCode.DRAW) {
            amounts[0] = payoutAmount / 2;
            amounts[1] = payoutAmount - amounts[0];
        } else if (outcome == AMPTypes.OutcomeCode.CANCELLED) {
            protocolFee = 0; // No fee on cancel
            amounts[0] = stakeAmount;
            amounts[1] = playerB == address(0) ? 0 : stakeAmount;
        }

        IAMPRegistry(registry).settleMatch(
            matchId,
            AMPTypes.MatchState.SETTLED,
            recipients,
            amounts,
            protocolFee
        );
    }
}
