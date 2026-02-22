// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "forge-std/Test.sol";
import "../src/AMPSettlement.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPTypes.sol";

contract AMPSettlementTest is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    address public admin = address(0x10);
    address public playerA = address(0x20);
    address public playerB = address(0x30);
    uint256 public verifierPrivKey = 0x1234;
    address public verifierPubKey;

    function setUp() public {
        registry = new AMPRegistry();
        settlement = new AMPSettlement(address(registry));
        registry.setSettlement(address(settlement));

        verifierPubKey = vm.addr(verifierPrivKey);

        vm.deal(playerA, 10 ether);
        vm.deal(playerB, 10 ether);
    }

    function testFullAsyncFlow() public {
        // 1. Register Game
        address[] memory verifiers = new address[](1);
        verifiers[0] = verifierPubKey;

        vm.prank(admin);
        uint256 gameId = registry.registerGame(
            AMPTypes.SettlementMode.ASYNC_VERIFIER,
            verifiers,
            1 ether,
            address(0)
        );

        // 2. Create Match
        vm.prank(playerA);
        uint256 matchId = registry.createMatch{value: 1 ether}(gameId);

        // 3. Join Match
        vm.prank(playerB);
        registry.joinMatch{value: 1 ether}(matchId);

        // 4. Verifier signs outcome
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode.WIN_A;
        bytes32 transcriptHash = bytes32(uint256(0x5678));

        bytes32 structHash = keccak256(abi.encode(matchId, outcome, transcriptHash));
        bytes32 digest = keccak256(abi.encodePacked("\x19Ethereum Signed Message:\n32", structHash));

        (uint8 v, bytes32 r, bytes32 s) = vm.sign(verifierPrivKey, digest);
        bytes memory signature = abi.encodePacked(r, s, v);

        AMPTypes.AsyncResult memory result = AMPTypes.AsyncResult({
            matchId: matchId,
            outcome: outcome,
            transcriptHash: transcriptHash,
            signature: signature
        });

        // 5. Submit Async Result
        uint256 playerABalanceBefore = playerA.balance;
        uint256 playerBBalanceBefore = playerB.balance;
        uint256 feeBalanceBefore = registry.feeBalance();

        settlement.submitAsyncResult(matchId, result);

        // 6. Verify Settlement
        (, AMPTypes.OutcomeCode actualOutcome, , ) = settlement.settlements(matchId);
        assertEq(uint(actualOutcome), uint(outcome));

        // Payout: Total pool = 2 ether. Fee = 1% = 0.02 ether. Payout = 1.98 ether.
        assertEq(playerA.balance, playerABalanceBefore + 1.98 ether);
        assertEq(playerB.balance, playerBBalanceBefore); // B lost
        assertEq(registry.feeBalance(), feeBalanceBefore + 0.02 ether);
    }
}
