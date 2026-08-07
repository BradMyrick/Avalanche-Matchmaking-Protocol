// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Script.sol";
import "../src/AMPTournamentCup.sol";

/// @notice Deploys AMPTournamentCup. Must be run with --skip-simulation --broadcast
///         because Avalanche's C-Chain does not serve state for the pending block
///         (forge's default simulation fails with "state not available for pending block").
///
///         env: FEE_RECIPIENT (address), PRIVATE_KEY (deployer)
contract DeployTournamentCupScript is Script {
    function run() external returns (AMPTournamentCup cup) {
        address feeRecipient = vm.envAddress("FEE_RECIPIENT");
        vm.startBroadcast();
        cup = new AMPTournamentCup(feeRecipient);
        vm.stopBroadcast();
    }
}
