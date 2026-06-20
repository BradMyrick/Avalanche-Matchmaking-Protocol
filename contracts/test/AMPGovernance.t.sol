// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Test.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPSettlement.sol";
import "../src/AMPTypes.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/governance/TimelockController.sol";

/// Phase 1.4 — owner economic controls (protocol fee bps + recipient) MUST be
/// governed by a timelock before mainnet, so a compromised owner key cannot
/// raise fees or redirect revenue in the same block as a settlement.
///
/// The pattern: deploy a TimelockController, transfer AMPSettlement ownership
/// to it (via Ownable2Step), after which every `onlyOwner` action must be
/// scheduled + delayed + executed through the timelock.
contract AMPGovernanceTest is Test {
    AMPRegistry public registry;
    AMPSettlement public settlement;
    TimelockController public timelock;
    address public governor = address(0x606);
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

        // Propose the timelock as the new owner (Ownable2Step step 1).
        settlement.transferOwnership(address(timelock));

        // Step 2: the timelock must accept ownership itself. Since the timelock
        // only acts via `execute`, schedule + advance + execute acceptOwnership().
        bytes memory accept = abi.encodeWithSelector(Ownable2Step.acceptOwnership.selector);
        vm.prank(governor);
        timelock.schedule(address(settlement), 0, accept, bytes32(0), bytes32(0), MIN_DELAY);
        vm.warp(block.timestamp + MIN_DELAY + 1);
        vm.prank(governor);
        timelock.execute(address(settlement), 0, accept, bytes32(0), bytes32(0));

        assertEq(settlement.owner(), address(timelock), "timelock must own the settlement");
    }

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
}
