// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "./AMPTypes.sol";

interface IAMPRegistry {
    function games(uint256 id)
        external
        view
        returns (address, AMPTypes.SettlementMode, address[] memory, uint256, address);
    function matches(uint256 id)
        external
        view
        returns (uint256, address, address, uint256, AMPTypes.MatchState, uint256);
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
    mapping(uint256 => mapping(address => AMPTypes.RealTimeHashResult)) public rtResults;

    event MatchSettled(uint256 indexed matchId, AMPTypes.OutcomeCode outcome, uint256 payout);
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
    function submitAsyncResult(uint256 matchId, AMPTypes.AsyncResult calldata result) external {
        // TODO: fetch Match and Game from registry via interface.
        // TODO: verify game.mode == ASYNC_VERIFIER.
        // TODO: recover signer from result.signature and check against game.verifiers.
        // TODO: check match.state is READY and not already SETTLED.

        settlements[matchId] = AMPTypes.Settlement({
            matchId: matchId, outcome: result.outcome, transcriptHash: result.transcriptHash, settledAt: block.timestamp
        });

        // TODO: implement _payout logic based on outcome.
        // Payout = (playerA_stake + playerB_stake) * (10000 - protocolFeeBps) / 10000;

        emit MatchSettled(matchId, result.outcome, 0); // Placeholder payout
    }

    /**
     * @notice Submits a result hash in real-time agreement mode.
     */
    function submitRealTimeHashResult(uint256 matchId, AMPTypes.RealTimeHashResult calldata result) external {
        rtResults[matchId][msg.sender] = result;

        // TODO: check if both players have submitted.
        // If (rtResults[matchId][playerA] and rtResults[matchId][playerB] exist):
        //    If (outcome and transcriptHash agree):
        //        _payout(...) and finalize.
        //    Else:
        //        Mark match DISPUTED and emit MatchDisputed.
    }

    /**
     * @notice Internal helper to handle payouts (AVA or ERC20).
     */
    function _payout(uint256 matchId, AMPTypes.OutcomeCode outcome) internal {
        // TODO: implement logic to distribute pooled stakes minus protocol fee.
        // WIN_A -> playerA gets pool.
        // WIN_B -> playerB gets pool.
        // DRAW -> split 50/50.
        // CANCELLED -> refund both.
    }
}
