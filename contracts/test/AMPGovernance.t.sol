// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Test.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPSettlement.sol";
import "../src/AMPTypes.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/governance/TimelockController.sol";

/// Phase 1.4 — owner economic controls MUST be governed by a timelock before
/// mainnet, so a compromised owner key cannot:
///   - re-point `AMPRegistry.settlement` to a drainer contract and rug every
///     staked match in a single transaction (the critical rug vector — see
///     SECURITY_REVIEW.md C4 / audit finding "Operator-rug vector"),
///   - raise protocol fees or redirect revenue in the same block as a
///     settlement,
///   - pause the registry to grief users, or
///   - withdraw accrued protocol fees without notice.
///
/// The pattern: deploy a TimelockController, transfer both AMPRegistry and
/// AMPSettlement ownership to it (via Ownable2Step), after which every
/// `onlyOwner` action must be scheduled + delayed + executed through the
/// timelock. This contract verifies the pattern is enforced for the critical
/// `setSettlement` rug path, not just the fee parameters.
contract AMPGovernanceTest is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    TimelockController public timelock;
    address public governor = address(0x606);
    address public attacker = address(0xBAD);
    uint256 public constant MIN_DELAY = 1 days;

    function setUp() public {
        registry = new AMPRegistry();
        settlement = new AMPSettlement(address(registry));
        registry.setSettlement(address(settlement));

        address[] memory proposers = new address[](1);
        proposers[0] = governor;
        address[] memory executors = new address[](1);
        executors[0] = governor;
        timelock = new TimelockController(MIN_DELAY, proposers, executors, address(0));

        // Hand BOTH contracts to the timelock via Ownable2Step.
        settlement.transferOwnership(address(timelock));
        registry.transferOwnership(address(timelock));

        // Step 2: the timelock must accept ownership itself. Since the timelock
        // only acts via `execute`, schedule + advance + execute acceptOwnership()
        // on each contract.
        bytes memory accept = abi.encodeWithSelector(Ownable2Step.acceptOwnership.selector);
        vm.startPrank(governor);
        timelock.schedule(address(settlement), 0, accept, bytes32(0), bytes32(0), MIN_DELAY);
        timelock.schedule(address(registry), 0, accept, bytes32(0), bytes32(0), MIN_DELAY);
        vm.warp(block.timestamp + MIN_DELAY + 1);
        timelock.execute(address(settlement), 0, accept, bytes32(0), bytes32(0));
        timelock.execute(address(registry), 0, accept, bytes32(0), bytes32(0));
        vm.stopPrank();

        assertEq(settlement.owner(), address(timelock), "timelock must own the settlement");
        assertEq(registry.owner(), address(timelock), "timelock must own the registry");
    }

    // ---------------------------------------------------------------------
    // AMPSettlement economic controls (fee bps + recipient)
    // ---------------------------------------------------------------------

    /// A direct (un-delayed) fee change must fail — caller is not the owner.
    function testDirectFeeUpdateRevertsUnderTimelock() public {
        vm.prank(governor);
        vm.expectRevert(); // OwnableUnauthorizedAccount
        settlement.updateProtocolFeeBps(200);
    }

    /// A fee change routed through the timelock succeeds only after MIN_DELAY.
    function testFeeUpdateRequiresTimelockDelay() public {
        assertEq(settlement.protocolFeeBps(), 100);

        bytes memory call = abi.encodeWithSelector(AMPSettlement.updateProtocolFeeBps.selector, uint16(200));

        // Schedule.
        vm.prank(governor);
        timelock.schedule(address(settlement), 0, call, bytes32(0), bytes32(0), MIN_DELAY);

        // Executing immediately (before delay) reverts: "TimelockUnexpectedOperationState".
        vm.prank(governor);
        vm.expectRevert();
        timelock.execute(address(settlement), 0, call, bytes32(0), bytes32(0));

        // After the delay window, execution succeeds.
        vm.warp(block.timestamp + MIN_DELAY + 1);
        vm.prank(governor);
        timelock.execute(address(settlement), 0, call, bytes32(0), bytes32(0));
        assertEq(settlement.protocolFeeBps(), 200);
    }

    /// Fee recipient redirection is subject to the same timelock discipline.
    function testFeeRecipientUpdateRequiresTimelock() public {
        address newRecipient = address(0xFEE);
        bytes memory call = abi.encodeWithSelector(AMPSettlement.updateProtocolFeeRecipient.selector, newRecipient);
        vm.prank(governor);
        timelock.schedule(address(settlement), 0, call, bytes32(0), bytes32(0), MIN_DELAY);
        vm.warp(block.timestamp + MIN_DELAY + 1);
        vm.prank(governor);
        timelock.execute(address(settlement), 0, call, bytes32(0), bytes32(0));
        assertEq(settlement.protocolFeeRecipient(), newRecipient);
    }

    // ---------------------------------------------------------------------
    // AMPRegistry economic controls — the CRITICAL rug vector
    // ---------------------------------------------------------------------

    /// A direct call to `setSettlement` (the rug path: re-pointing settlement
    /// to a malicious contract that drains every staked match in one tx) MUST
    /// fail because the registry is no longer owned by an EOA. This is the
    /// single most important guarantee the timelock provides.
    function testDirectSetSettlementRevertsUnderTimelock() public {
        // Even the governor (who controls the timelock) cannot call
        // setSettlement directly — they must schedule+delay+execute.
        vm.prank(governor);
        vm.expectRevert(); // OwnableUnauthorizedAccount
        registry.setSettlement(attacker);

        // An unrelated attacker also cannot.
        vm.prank(attacker);
        vm.expectRevert();
        registry.setSettlement(attacker);
    }

    /// `setSettlement` can only be performed through the timelock, after the
    /// delay elapses. This means any attempt to re-point settlement is visible
    /// on-chain for at least MIN_DELAY, giving users a window to withdraw and
    /// exit before the change lands.
    function testSetSettlementRequiresTimelockDelay() public {
        address newSettlement = address(0xBEEF);

        bytes memory call = abi.encodeWithSelector(AMPRegistry.setSettlement.selector, newSettlement);

        // Schedule.
        vm.prank(governor);
        timelock.schedule(address(registry), 0, call, bytes32(0), bytes32(0), MIN_DELAY);

        // Executing immediately reverts.
        vm.prank(governor);
        vm.expectRevert();
        timelock.execute(address(registry), 0, call, bytes32(0), bytes32(0));

        // After the delay, execution succeeds — and observers have had
        // MIN_DELAY worth of blocks to react to the scheduled change.
        vm.warp(block.timestamp + MIN_DELAY + 1);
        vm.prank(governor);
        timelock.execute(address(registry), 0, call, bytes32(0), bytes32(0));
        assertEq(registry.settlement(), newSettlement);
    }

    /// `pause()` on the registry is also timelocked — operators cannot grief
    /// users with a surprise pause, but can still pause through the delay if
    /// an incident response requires it.
    function testDirectPauseRevertsUnderTimelock() public {
        vm.prank(governor);
        vm.expectRevert();
        registry.pause();
    }
}
