// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Script.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPSettlement.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/governance/TimelockController.sol";

/// @notice Finalizes AMP governance by executing the pending Ownable2Step
/// `acceptOwnership()` calls on `AMPRegistry` and `AMPSettlement` through the
/// timelock. Run this **MIN_DELAY seconds** after `Deploy.s.sol` (i.e. once
/// the schedule window has elapsed).
///
/// Until this runs, the deployer retains owner authority on both contracts —
/// that is by design (it gives the community a window to verify the deploy
/// before ownership is locked). After this runs, the deployer key has zero
/// economic authority and only timelocked governance can change economic
/// parameters.
///
/// **Env vars:**
/// - `PRIVATE_KEY`          — must correspond to the timelock `governor` role
/// - `TIMELOCK_ADDRESS`     — the TimelockController deployed by `Deploy.s.sol`
/// - `REGISTRY_ADDRESS`     — the AMPRegistry address
/// - `SETTLEMENT_ADDRESS`   — the AMPSettlement address
contract FinalizeGovernanceScript is Script {
    function run() external {
        uint256 governorPrivateKey = vm.envUint("PRIVATE_KEY");
        address timelockAddr = vm.envAddress("TIMELOCK_ADDRESS");
        address registryAddr = vm.envAddress("REGISTRY_ADDRESS");
        address settlementAddr = vm.envAddress("SETTLEMENT_ADDRESS");

        vm.startBroadcast(governorPrivateKey);

        TimelockController timelock = TimelockController(payable(timelockAddr));
        bytes memory accept = abi.encodeWithSelector(Ownable2Step.acceptOwnership.selector);

        timelock.execute(registryAddr, 0, accept, bytes32(0), bytes32(0));
        timelock.execute(settlementAddr, 0, accept, bytes32(0), bytes32(0));

        vm.stopBroadcast();

        require(AMPRegistry(registryAddr).owner() == timelockAddr, "REGISTRY_NOT_TIMELOCKED");
        require(AMPSettlement(settlementAddr).owner() == timelockAddr, "SETTLEMENT_NOT_TIMELOCKED");

        console.log("AMPRegistry owner:", AMPRegistry(registryAddr).owner());
        console.log("AMPSettlement owner:", AMPSettlement(settlementAddr).owner());
        console.log("Governance finalized: deployer key no longer has economic authority.");
    }
}
