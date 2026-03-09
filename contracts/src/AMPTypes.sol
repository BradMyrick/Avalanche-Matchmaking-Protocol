// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

/**
 * @title AMPTypes
 * @notice Shared enums and structs for the AMP protocol.
 */
library AMPTypes {
    /**
     * @notice State of a Match.
     * OPEN: Match created, waiting for player B.
     * READY: Both players joined, waiting for result.
     * SETTLED: Result submitted and payouts processed.
     * EXPIRED: Match time limit reached without settlement.
     * DISPUTED: Players submitted conflicting real-time hashes.
     */
    enum MatchState {
        OPEN,
        READY,
        SETTLED,
        EXPIRED,
        DISPUTED
    }

    /**
     * @notice Mode of settlement used for the match.
     * ASYNC_VERIFIER: An off-chain verifier re-simulates and signs the result.
     * RT_HASH_AGREE: Players agree on a hash of the transcript in real-time.
     */
    enum SettlementMode {
        ASYNC_VERIFIER,
        RT_HASH_AGREE
    }

    /**
     * @notice Outcome code for a match.
     */
    enum OutcomeCode {
        NONE,
        WIN_A,
        WIN_B,
        DRAW,
        CANCELLED
    }

    /**
     * @notice Game registration details.
     */
    struct Game {
        address admin;
        SettlementMode mode;
        address[] verifiers; // Addresses authorized to sign results for ASYNC_VERIFIER mode
        uint256 minStake;
        address stakeToken; // address(0) for AVAX
        address arbiter; // Address authorized to resolve disputes
    }

    /**
     * @notice Match instance details.
     */
    struct Match {
        uint256 gameId;
        address playerA;
        address playerB;
        uint256 stakeAmount;
        MatchState state;
        uint256 createdAt;
    }

    /**
     * @notice Final settlement record for a match.
     */
    struct Settlement {
        uint256 matchId;
        OutcomeCode outcome;
        bytes32 transcriptHash;
        uint256 settledAt;
    }

    /**
     * @notice Result signed by an async verifier.
     */
    struct AsyncResult {
        uint256 matchId;
        OutcomeCode outcome;
        bytes32 transcriptHash;
        bytes signature;
    }

    /**
     * @notice Result submitted by a player in real-time mode.
     */
    struct RealTimeHashResult {
        uint256 matchId;
        OutcomeCode outcome;
        bytes32 transcriptHash;
    }
}
