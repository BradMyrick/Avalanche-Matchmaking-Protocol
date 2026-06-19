// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

import "./AMPTypes.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/ECDSA.sol";
import "openzeppelin-contracts/contracts/utils/cryptography/MessageHashUtils.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/utils/Pausable.sol";

interface IAMPRegistry {
    function games(uint256 id)
        external
        view
        returns (
            address admin,
            AMPTypes.SettlementMode mode,
            uint256 minStake,
            address stakeToken,
            address arbiter,
            uint256 matchTimeout
        );

    function getGameVerifiers(uint256 id) external view returns (address[] memory);

    function isVerifier(uint256 gameId, address verifier) external view returns (bool);

    function matches(uint256 id) external view returns (uint256, address, AMPTypes.MatchState, address, uint64, uint256);

    function settleMatch(
        uint256 matchId,
        AMPTypes.MatchState newState,
        address feeRecipient,
        address[] calldata recipients,
        uint256[] calldata amounts,
        uint256 protocolFee
    ) external;
}

contract AMPSettlement is Ownable2Step, Pausable {
    error FeeExceedsMax();
    error InvalidRecipient();
    error MatchIdMismatch();
    error WrongMode();
    error MatchNotSettlable();
    error NoOpponent();
    error InvalidVerifierSignature();
    error NotArbiter();
    error NotDisputed();
    error NotAPlayer();

    address public immutable registry;
    uint16 public protocolFeeBps;
    address public protocolFeeRecipient;
    bytes32 private _cachedDomainSeparator;
    uint256 private _cachedChainId;

    bytes32 public constant EIP712_DOMAIN_TYPEHASH =
        keccak256("EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)");
    bytes32 public constant ASYNC_RESULT_TYPEHASH =
        keccak256("AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)");

    mapping(uint256 => AMPTypes.Settlement) public settlements;
    mapping(uint256 => mapping(address => AMPTypes.RealTimeHashResult)) public rtResults;

    event MatchSettled(uint256 indexed matchId, AMPTypes.OutcomeCode outcome, uint256 payout);
    event MatchDisputed(uint256 indexed matchId);
    event ProtocolFeeUpdated(uint16 feeBps);
    event ProtocolFeeRecipientUpdated(address indexed recipient);
    event MatchDisputeResolved(uint256 indexed matchId, AMPTypes.OutcomeCode enforcedOutcome);

    constructor(address _registry) Ownable(msg.sender) {
        registry = _registry;
        protocolFeeBps = 100;
        protocolFeeRecipient = msg.sender;
        _cachedChainId = block.chainid;
        _cachedDomainSeparator = _buildDomainSeparator();
    }

    uint16 public constant MAX_PROTOCOL_FEE_BPS = 500;

    function updateProtocolFeeBps(uint16 feeBps) external onlyOwner {
        if (feeBps > MAX_PROTOCOL_FEE_BPS) revert FeeExceedsMax();
        protocolFeeBps = feeBps;
        emit ProtocolFeeUpdated(feeBps);
    }

    function updateProtocolFeeRecipient(address recipient) external onlyOwner {
        if (recipient == address(0)) revert InvalidRecipient();
        protocolFeeRecipient = recipient;
        emit ProtocolFeeRecipientUpdated(recipient);
    }

    function submitAsyncResult(uint256 matchId, AMPTypes.AsyncResult calldata result) external whenNotPaused {
        if (result.matchId != matchId) revert MatchIdMismatch();

        (
            uint256 gameId,
            address playerA,
            AMPTypes.MatchState state,
            address playerB,
            uint64 createdAt,
            uint256 stakeAmount
        ) = IAMPRegistry(registry).matches(matchId);

        (, AMPTypes.SettlementMode mode,,,,) = IAMPRegistry(registry).games(gameId);

        if (mode != AMPTypes.SettlementMode.ASYNC_VERIFIER) revert WrongMode();
        if (state != AMPTypes.MatchState.READY) revert MatchNotSettlable();
        if (playerB == address(0)) revert NoOpponent();

        bytes32 structHash =
            keccak256(abi.encode(ASYNC_RESULT_TYPEHASH, result.matchId, result.outcome, result.transcriptHash));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", _domainSeparatorV4(), structHash));
        address signer = ECDSA.recover(digest, result.signature);

        if (!IAMPRegistry(registry).isVerifier(gameId, signer)) revert InvalidVerifierSignature();

        settlements[matchId] = AMPTypes.Settlement({
            matchId: matchId, outcome: result.outcome, transcriptHash: result.transcriptHash, settledAt: block.timestamp
        });

        _payout(matchId, playerA, playerB, stakeAmount, result.outcome);

        emit MatchSettled(matchId, result.outcome, stakeAmount * 2);
    }

    function submitRealTimeHashResult(uint256 matchId, AMPTypes.RealTimeHashResult calldata result)
        external
        whenNotPaused
    {
        if (result.matchId != matchId) revert MatchIdMismatch();
        (
            uint256 gameId,
            address playerA,
            AMPTypes.MatchState state,
            address playerB,
            uint64 createdAt,
            uint256 stakeAmount
        ) = IAMPRegistry(registry).matches(matchId);

        (, AMPTypes.SettlementMode mode,,,,) = IAMPRegistry(registry).games(gameId);

        if (mode != AMPTypes.SettlementMode.RT_HASH_AGREE) revert WrongMode();
        if (state != AMPTypes.MatchState.READY) revert MatchNotSettlable();
        if (msg.sender != playerA && msg.sender != playerB) revert NotAPlayer();

        rtResults[matchId][msg.sender] = result;

        address otherPlayer = msg.sender == playerA ? playerB : playerA;
        AMPTypes.RealTimeHashResult memory otherResult = rtResults[matchId][otherPlayer];

        if (otherResult.outcome != AMPTypes.OutcomeCode.NONE) {
            if (result.outcome == otherResult.outcome && result.transcriptHash == otherResult.transcriptHash) {
                settlements[matchId] = AMPTypes.Settlement({
                    matchId: matchId,
                    outcome: result.outcome,
                    transcriptHash: result.transcriptHash,
                    settledAt: block.timestamp
                });

                _payout(matchId, playerA, playerB, stakeAmount, result.outcome);
                emit MatchSettled(matchId, result.outcome, stakeAmount * 2);
            } else {
                address[] memory recipients = new address[](0);
                uint256[] memory amounts = new uint256[](0);
                IAMPRegistry(registry)
                    .settleMatch(matchId, AMPTypes.MatchState.DISPUTED, address(0), recipients, amounts, 0);
                emit MatchDisputed(matchId);
            }
        }
    }

    function resolveDispute(uint256 matchId, AMPTypes.OutcomeCode enforcedOutcome) external whenNotPaused {
        (
            uint256 gameId,
            address playerA,
            AMPTypes.MatchState state,
            address playerB,
            uint64 createdAt,
            uint256 stakeAmount
        ) = IAMPRegistry(registry).matches(matchId);

        (,,,, address arbiter,) = IAMPRegistry(registry).games(gameId);

        if (state != AMPTypes.MatchState.DISPUTED) revert NotDisputed();
        if (msg.sender != arbiter) revert NotArbiter();
        if (playerB == address(0)) revert NoOpponent();

        settlements[matchId] = AMPTypes.Settlement({
            matchId: matchId, outcome: enforcedOutcome, transcriptHash: bytes32(0), settledAt: block.timestamp
        });

        _payout(matchId, playerA, playerB, stakeAmount, enforcedOutcome);
        emit MatchDisputeResolved(matchId, enforcedOutcome);
        emit MatchSettled(matchId, enforcedOutcome, stakeAmount * 2);
    }

    function pause() external onlyOwner {
        _pause();
    }

    function unpause() external onlyOwner {
        _unpause();
    }

    function _payout(
        uint256 matchId,
        address playerA,
        address playerB,
        uint256 stakeAmount,
        AMPTypes.OutcomeCode outcome
    ) internal {
        uint256 totalPool = stakeAmount * 2;
        if (playerB == address(0)) {
            totalPool = stakeAmount;
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
            protocolFee = 0;
            amounts[0] = stakeAmount;
            amounts[1] = playerB == address(0) ? 0 : stakeAmount;
        }

        IAMPRegistry(registry)
            .settleMatch(matchId, AMPTypes.MatchState.SETTLED, protocolFeeRecipient, recipients, amounts, protocolFee);
    }

    function _buildDomainSeparator() internal view returns (bytes32) {
        return keccak256(
            abi.encode(EIP712_DOMAIN_TYPEHASH, keccak256("AMPSettlement"), keccak256("1"), block.chainid, address(this))
        );
    }

    function domainSeparator() external view returns (bytes32) {
        if (block.chainid == _cachedChainId) {
            return _cachedDomainSeparator;
        }
        return _buildDomainSeparator();
    }

    function _domainSeparatorV4() internal returns (bytes32) {
        if (block.chainid != _cachedChainId) {
            _cachedChainId = block.chainid;
            _cachedDomainSeparator = _buildDomainSeparator();
        }
        return _cachedDomainSeparator;
    }
}
