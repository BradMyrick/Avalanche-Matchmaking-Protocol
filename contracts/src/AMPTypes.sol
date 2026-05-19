// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

library AMPTypes {
    enum MatchState {
        OPEN,
        READY,
        SETTLED,
        EXPIRED,
        DISPUTED
    }

    enum SettlementMode {
        ASYNC_VERIFIER,
        RT_HASH_AGREE
    }

    enum OutcomeCode {
        NONE,
        WIN_A,
        WIN_B,
        DRAW,
        CANCELLED
    }

    struct Game {
        address admin;
        SettlementMode mode;
        address[] verifiers;
        uint256 minStake;
        address stakeToken;
        address arbiter;
        uint256 matchTimeout;
    }

    struct Match {
        uint256 gameId;
        address playerA;
        address playerB;
        uint256 stakeAmount;
        MatchState state;
        uint256 createdAt;
    }

    struct Settlement {
        uint256 matchId;
        OutcomeCode outcome;
        bytes32 transcriptHash;
        uint256 settledAt;
    }

    struct AsyncResult {
        uint256 matchId;
        OutcomeCode outcome;
        bytes32 transcriptHash;
        bytes signature;
    }

    struct RealTimeHashResult {
        uint256 matchId;
        OutcomeCode outcome;
        bytes32 transcriptHash;
    }
}
