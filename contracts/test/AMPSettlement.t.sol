// SPDX-License-Identifier: MIT
pragma solidity ^0.8.33;

import "forge-std/Test.sol";
import "../src/AMPSettlement.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPTypes.sol";
import "openzeppelin-contracts/contracts/utils/Pausable.sol";

contract AMPSettlementTest is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    address public admin = address(0x10);
    address public playerA = address(0x20);
    address public playerB = address(0x30);
    address public nonPlayer = address(0x50);
    uint256 public verifierPrivKey = 0x1234;
    address public verifierPubKey;

    function setUp() public {
        registry = new AMPRegistry(address(0));
        settlement = new AMPSettlement(address(registry), address(0));
        registry.setSettlement(address(settlement));
        verifierPubKey = vm.addr(verifierPrivKey);
        vm.deal(playerA, 10 ether);
        vm.deal(playerB, 10 ether);
    }

    function _setupAsyncGameAndMatch() internal returns (uint256 matchId) {
        address[] memory verifiers = new address[](1);
        verifiers[0] = verifierPubKey;
        vm.prank(admin);
        uint256 gameId =
            registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 1 ether, address(0), address(0));
        matchId = 123;
        vm.prank(playerA);
        registry.createMatch{value: 1 ether}(gameId, matchId, 1 ether);
        vm.prank(playerB);
        registry.joinMatch{value: 1 ether}(matchId);
    }

    function _signResult(uint256 matchId, AMPTypes.OutcomeCode outcome, bytes32 transcriptHash, uint256 privKey)
        internal
        view
        returns (bytes memory)
    {
        bytes32 structHash = keccak256(abi.encode(settlement.ASYNC_RESULT_TYPEHASH(), matchId, outcome, transcriptHash));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", settlement.domainSeparator(), structHash));
        (uint8 v, bytes32 r, bytes32 s) = vm.sign(privKey, digest);
        return abi.encodePacked(r, s, v);
    }

    function _setupRTGameAndMatch() internal returns (uint256 matchId) {
        address[] memory verifiers = new address[](1);
        verifiers[0] = verifierPubKey;
        vm.prank(admin);
        uint256 gameId =
            registry.registerGame(AMPTypes.SettlementMode.RT_HASH_AGREE, verifiers, 1 ether, address(0), admin);
        matchId = 456;
        vm.prank(playerA);
        registry.createMatch{value: 1 ether}(gameId, matchId, 1 ether);
        vm.prank(playerB);
        registry.joinMatch{value: 1 ether}(matchId);
    }

    function _withdrawNative(address player, uint256 expectedAmount) internal {
        uint256 balanceBefore = player.balance;
        vm.prank(player);
        registry.withdraw(address(0));
        assertEq(player.balance, balanceBefore + expectedAmount);
    }

    function testFullAsyncFlow() public {
        uint256 matchId = _setupAsyncGameAndMatch();
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.WIN_A;
        bytes32 transcriptHash = bytes32(uint256(0x5678));
        bytes memory signature = _signResult(matchId, outcome, transcriptHash, verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId, outcome: outcome, transcriptHash: transcriptHash, signature: signature
        });
        settlement.submitAsyncResult(matchId, result);
        (, AMPTypes.OutcomeCode actualOutcome,,) = settlement.settlements(matchId);
        assertEq(uint256(actualOutcome), uint256(outcome));
        assertEq(registry.pendingWithdrawals(address(0), playerA), 1.98 ether);
        assertEq(registry.feeBalances(address(0)), 0.02 ether);
        _withdrawNative(playerA, 1.98 ether);
    }

    function testResolveDispute() public {
        uint256 matchId = _setupRTGameAndMatch();
        AMPTypes.RealTimeHashResult memory resultA = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_A, transcriptHash: bytes32(uint256(1))
        });
        AMPTypes.RealTimeHashResult memory resultB = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_B, transcriptHash: bytes32(uint256(2))
        });
        vm.prank(playerA);
        settlement.submitRealTimeHashResult(matchId, resultA);
        vm.prank(playerB);
        settlement.submitRealTimeHashResult(matchId, resultB);
        vm.prank(admin);
        settlement.resolveDispute(matchId, AMPTypes.OutcomeCode.WIN_B);
        assertEq(registry.pendingWithdrawals(address(0), playerB), 1.98 ether);
        (, AMPTypes.OutcomeCode outcome,,) = settlement.settlements(matchId);
        assertEq(uint256(outcome), uint256(AMPTypes.OutcomeCode.WIN_B));
        _withdrawNative(playerB, 1.98 ether);
    }

    function testDrawOutcome() public {
        uint256 matchId = _setupAsyncGameAndMatch();
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.DRAW;
        bytes32 transcriptHash = bytes32(uint256(0xabcd));
        bytes memory signature = _signResult(matchId, outcome, transcriptHash, verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId, outcome: outcome, transcriptHash: transcriptHash, signature: signature
        });
        settlement.submitAsyncResult(matchId, result);
        assertEq(registry.pendingWithdrawals(address(0), playerA), 0.99 ether);
        assertEq(registry.pendingWithdrawals(address(0), playerB), 0.99 ether);
        _withdrawNative(playerA, 0.99 ether);
        _withdrawNative(playerB, 0.99 ether);
    }

    function testCancelledOutcome() public {
        uint256 matchId = _setupAsyncGameAndMatch();
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.CANCELLED;
        bytes32 transcriptHash = bytes32(uint256(0xef01));
        bytes memory signature = _signResult(matchId, outcome, transcriptHash, verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId, outcome: outcome, transcriptHash: transcriptHash, signature: signature
        });
        settlement.submitAsyncResult(matchId, result);
        assertEq(registry.pendingWithdrawals(address(0), playerA), 1 ether);
        assertEq(registry.pendingWithdrawals(address(0), playerB), 1 ether);
        assertEq(registry.feeBalances(address(0)), 0);
        _withdrawNative(playerA, 1 ether);
        _withdrawNative(playerB, 1 ether);
    }

    function testInvalidVerifierSignature() public {
        uint256 matchId = _setupAsyncGameAndMatch();
        uint256 wrongPrivKey = 0xdeadbeef;
        bytes memory signature = _signResult(matchId, AMPTypes.OutcomeCode.WIN_A, bytes32(uint256(0x11)), wrongPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId,
            outcome: AMPTypes.OutcomeCode.WIN_A,
            transcriptHash: bytes32(uint256(0x11)),
            signature: signature
        });
        vm.expectRevert(AMPSettlement.InvalidVerifierSignature.selector);
        settlement.submitAsyncResult(matchId, result);
    }

    function testWrongMode() public {
        uint256 matchId = _setupRTGameAndMatch();
        bytes memory signature = _signResult(matchId, AMPTypes.OutcomeCode.WIN_A, bytes32(0), verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_A, transcriptHash: bytes32(0), signature: signature
        });
        vm.expectRevert(AMPSettlement.WrongMode.selector);
        settlement.submitAsyncResult(matchId, result);
    }

    function testRTAgreementSuccess() public {
        uint256 matchId = _setupRTGameAndMatch();
        AMPTypes.RealTimeHashResult memory resultA = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_A, transcriptHash: bytes32(uint256(0xabc))
        });
        AMPTypes.RealTimeHashResult memory resultB = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_A, transcriptHash: bytes32(uint256(0xabc))
        });
        vm.prank(playerA);
        settlement.submitRealTimeHashResult(matchId, resultA);
        vm.prank(playerB);
        settlement.submitRealTimeHashResult(matchId, resultB);
        (, AMPTypes.OutcomeCode outcome,,) = settlement.settlements(matchId);
        assertEq(uint256(outcome), uint256(AMPTypes.OutcomeCode.WIN_A));
        assertEq(registry.pendingWithdrawals(address(0), playerA), 1.98 ether);
        _withdrawNative(playerA, 1.98 ether);
    }

    function testRTNotPlayer() public {
        uint256 matchId = _setupRTGameAndMatch();
        AMPTypes.RealTimeHashResult memory result = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_A, transcriptHash: bytes32(uint256(0x1))
        });
        vm.prank(nonPlayer);
        vm.expectRevert(AMPSettlement.NotAPlayer.selector);
        settlement.submitRealTimeHashResult(matchId, result);
    }

    function testDisputeNotArbiter() public {
        uint256 matchId = _setupRTGameAndMatch();
        AMPTypes.RealTimeHashResult memory resultA = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_A, transcriptHash: bytes32(uint256(1))
        });
        AMPTypes.RealTimeHashResult memory resultB = AMPTypes.RealTimeHashResult({
            matchId: matchId, outcome: AMPTypes.OutcomeCode.WIN_B, transcriptHash: bytes32(uint256(2))
        });
        vm.prank(playerA);
        settlement.submitRealTimeHashResult(matchId, resultA);
        vm.prank(playerB);
        settlement.submitRealTimeHashResult(matchId, resultB);
        vm.prank(nonPlayer);
        vm.expectRevert(AMPSettlement.NotArbiter.selector);
        settlement.resolveDispute(matchId, AMPTypes.OutcomeCode.WIN_A);
    }

    function testFeeZeroPercent() public {
        settlement.updateProtocolFeeBps(0);
        uint256 matchId = _setupAsyncGameAndMatch();
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.WIN_A;
        bytes32 transcriptHash = bytes32(uint256(0x5678));
        bytes memory signature = _signResult(matchId, outcome, transcriptHash, verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId, outcome: outcome, transcriptHash: transcriptHash, signature: signature
        });
        settlement.submitAsyncResult(matchId, result);
        assertEq(registry.pendingWithdrawals(address(0), playerA), 2 ether);
        assertEq(registry.feeBalances(address(0)), 0);
        _withdrawNative(playerA, 2 ether);
    }

    function testFeeInvalidBps() public {
        vm.expectRevert(AMPSettlement.FeeExceedsMax.selector);
        settlement.updateProtocolFeeBps(501);
    }

    function testUpdateFeeRecipient() public {
        address newRecipient = address(0x8888);
        settlement.updateProtocolFeeRecipient(newRecipient);
        assertEq(settlement.protocolFeeRecipient(), newRecipient);
    }

    function testPauseBlocksSubmit() public {
        uint256 matchId = _setupAsyncGameAndMatch();
        settlement.pause();
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.WIN_A;
        bytes32 transcriptHash = bytes32(uint256(0x5678));
        bytes memory signature = _signResult(matchId, outcome, transcriptHash, verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId, outcome: outcome, transcriptHash: transcriptHash, signature: signature
        });
        vm.expectRevert(abi.encodeWithSelector(Pausable.EnforcedPause.selector));
        settlement.submitAsyncResult(matchId, result);
    }

    function testMatchIdMismatch() public {
        uint256 matchId = _setupAsyncGameAndMatch();
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.WIN_A;
        bytes32 transcriptHash = bytes32(uint256(0x5678));
        bytes memory signature = _signResult(999, outcome, transcriptHash, verifierPrivKey);
        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: 999, outcome: outcome, transcriptHash: transcriptHash, signature: signature
        });
        vm.expectRevert(AMPSettlement.MatchIdMismatch.selector);
        settlement.submitAsyncResult(matchId, result);
    }

    function testWithdrawNoPending() public {
        vm.prank(playerA);
        vm.expectRevert(AMPRegistry.NoPendingWithdrawal.selector);
        registry.withdraw(address(0));
    }
}
