// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Test.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPSettlement.sol";
import "../src/AMPTypes.sol";

/// Phase 1.2 — value-conservation fuzz test for the custody path.
/// Directly exercises the F1 finding: no outcome (including the NONE sentinel)
/// may trap or create funds. Every valid settlement must satisfy
/// `playerCredits + protocolFee == totalPool`.
contract AMPSettlementFuzzTest is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    address admin = address(0x10);
    address playerA = address(0x20);
    address playerB = address(0x30);
    uint256 verifierPrivKey = 0x1234;
    address verifierPubKey;

    function setUp() public {
        registry = new AMPRegistry();
        settlement = new AMPSettlement(address(registry));
        registry.setSettlement(address(settlement));
        verifierPubKey = vm.addr(verifierPrivKey);
        vm.deal(playerA, 100 ether);
        vm.deal(playerB, 100 ether);
    }

    function _setupMatch(uint256 stake) internal returns (uint256 matchId) {
        address[] memory verifiers = new address[](1);
        verifiers[0] = verifierPubKey;
        vm.prank(admin);
        uint256 gameId =
            registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 1 ether, address(0), address(0));
        matchId = uint256(keccak256(abi.encodePacked(block.timestamp, stake)));
        vm.prank(playerA);
        registry.createMatch{value: stake}(gameId, matchId, stake);
        vm.prank(playerB);
        registry.joinMatch{value: stake}(matchId);
    }

    function _sign(uint256 matchId, AMPTypes.OutcomeCode outcome, bytes32 transcript)
        internal
        view
        returns (bytes memory)
    {
        bytes32 structHash = keccak256(abi.encode(settlement.ASYNC_RESULT_TYPEHASH(), matchId, outcome, transcript));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", settlement.domainSeparator(), structHash));
        (uint8 v, bytes32 r, bytes32 s) = vm.sign(verifierPrivKey, digest);
        return abi.encodePacked(r, s, v);
    }

    /// For every outcome, either the match reverts (NONE) or the sum of player
    /// credits plus the protocol fee exactly equals the total staked pool.
    function testFuzz_SettlementConservesValue(uint8 outcomeRaw, uint256 stake) public {
        vm.assume(stake >= 1 ether && stake <= 5 ether);
        AMPTypes.OutcomeCode outcome = AMPTypes.OutcomeCode(outcomeRaw % 5);

        uint256 matchId = _setupMatch(stake);
        bytes32 transcript = bytes32(uint256(0xABCD));
        bytes memory sig = _sign(matchId, outcome, transcript);
        AMPTypes.AsyncResult memory result =
            AMPTypes.AsyncResult({matchId: matchId, outcome: outcome, transcriptHash: transcript, signature: sig});

        if (outcome == AMPTypes.OutcomeCode.NONE) {
            vm.expectRevert(AMPSettlement.InvalidOutcome.selector);
            settlement.submitAsyncResult(matchId, result);
            return;
        }

        uint256 pool = stake * 2;
        settlement.submitAsyncResult(matchId, result);

        uint256 creditA = registry.pendingWithdrawals(address(0), playerA);
        uint256 creditB = registry.pendingWithdrawals(address(0), playerB);
        // Default recipient is the deployer (address(this)); fee routes via credits.
        uint256 fee = registry.pendingWithdrawals(address(0), settlement.protocolFeeRecipient());
        assertEq(creditA + creditB + fee, pool, "value not conserved: credits+fee != pool");
    }

    /// After a full create → join → settle → withdraw cycle, the Registry holds
    /// exactly the accrued protocol fee and nothing else (no stranded funds).
    function testNoFundsStrandedAfterFullCycle() public {
        uint256 matchId = _setupMatch(1 ether);
        bytes memory sig = _sign(matchId, AMPTypes.OutcomeCode.WIN_A, bytes32(uint256(0x1)));
        settlement.submitAsyncResult(
            matchId,
            AMPTypes.AsyncResult({
                matchId: matchId,
                outcome: AMPTypes.OutcomeCode.WIN_A,
                transcriptHash: bytes32(uint256(0x1)),
                signature: sig
            })
        );

        uint256 registryBefore = address(registry).balance;
        vm.prank(playerA);
        registry.withdraw(address(0));
        // Winner withdrew; only the 1% protocol fee should remain stranded in the Registry.
        assertEq(address(registry).balance, registryBefore - 1.98 ether);
        assertEq(address(registry).balance, 0.02 ether);
    }
}

/// Phase 1.2 — real Foundry invariant test with a handler. After each fuzzed
/// handler call (open+settle, or claim), the Registry must remain solvent:
/// its native balance must back every outstanding credit and every accrued fee.
contract AMPRegistryInvariantTest is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    RegistryHandler public handler;

    function setUp() public {
        registry = new AMPRegistry();
        settlement = new AMPSettlement(address(registry));
        registry.setSettlement(address(settlement));
        handler = new RegistryHandler(registry, settlement);

        bytes4[] memory selectors = new bytes4[](2);
        selectors[0] = RegistryHandler.openAndSettle.selector;
        selectors[1] = RegistryHandler.claimSome.selector;
        targetSelector(FuzzSelector({addr: address(handler), selectors: selectors}));
        targetContract(address(handler));
    }

    function invariant_registryNativeBacksAllCredits() public view {
        assertGe(
            address(registry).balance,
            handler.totalOutstandingCredits() + registry.feeBalances(address(0)),
            "Registry native balance must back all pending credits + fees"
        );
    }
}

contract RegistryHandler is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    uint256 public verifierPrivKey = 0x9999;
    address public verifierPubKey;
    uint256 public nextMatchId = 1;
    uint256 public totalOutstandingCredits;
    address[5] public players;

    constructor(AMPRegistry _registry, AMPSettlement _settlement) {
        registry = _registry;
        settlement = _settlement;
        verifierPubKey = vm.addr(verifierPrivKey);
        for (uint160 i = 0; i < 5; i++) {
            players[i] = address(uint160(0xA000 + i));
        }
        address[] memory verifiers = new address[](1);
        verifiers[0] = verifierPubKey;
        registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 0.01 ether, address(0), address(0));
    }

    function openAndSettle() external {
        uint256 mid = nextMatchId++;
        address a = players[mid % 5];
        address b = players[(mid + 1) % 5];
        uint256 stake = 0.01 ether;

        vm.deal(a, stake);
        vm.deal(b, stake);
        vm.prank(a);
        registry.createMatch{value: stake}(0, mid, stake);
        vm.prank(b);
        registry.joinMatch{value: stake}(mid);

        bytes32 structHash =
            keccak256(abi.encode(settlement.ASYNC_RESULT_TYPEHASH(), mid, AMPTypes.OutcomeCode.WIN_A, bytes32(0)));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", settlement.domainSeparator(), structHash));
        (uint8 v, bytes32 r, bytes32 s) = vm.sign(verifierPrivKey, digest);

        // Track only the delta credited to the winner (a may have won before).
        uint256 creditBefore = registry.pendingWithdrawals(address(0), a);
        settlement.submitAsyncResult(
            mid,
            AMPTypes.AsyncResult({
                matchId: mid,
                outcome: AMPTypes.OutcomeCode.WIN_A,
                transcriptHash: bytes32(0),
                signature: abi.encodePacked(r, s, v)
            })
        );
        uint256 creditAfter = registry.pendingWithdrawals(address(0), a);
        totalOutstandingCredits += (creditAfter - creditBefore);
    }

    function claimSome() external {
        uint256 mid = (nextMatchId == 1) ? 1 : (nextMatchId - 1);
        address p = players[mid % 5];
        uint256 owed = registry.pendingWithdrawals(address(0), p);
        if (owed == 0) return;
        vm.prank(p);
        registry.withdraw(address(0));
        totalOutstandingCredits -= owed;
    }
}
