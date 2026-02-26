// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "./AMPTypes.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/ECDSA.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/MessageHashUtils.sol";

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
            address stakeToken
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
contract AMPSettlement {
    address public registry;
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

    constructor(address _registry) {
        registry = _registry;
        protocolFeeBps = 100; // 1% default
        protocolFeeRecipient = msg.sender;
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
            uint256 createdAt
        ) = IAMPRegistry(registry).matches(matchId);

        (, AMPTypes.SettlementMode mode, , ) = IAMPRegistry(registry).games(
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
        rtResults[matchId][msg.sender] = result;

        // TODO: Check if both players have submitted.
        // If (rtResults[matchId][playerA] and rtResults[matchId][playerB] exist):
        //    If (outcome and transcriptHash agree):
        //        _payout(...) and finalize.
        //    Else:
        //        Mark match DISPUTED and emit MatchDisputed.
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
