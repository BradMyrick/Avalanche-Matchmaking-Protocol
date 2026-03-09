// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "forge-std/Script.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPSettlement.sol";

contract DeployScript is Script {
    function run() external {
        uint256 deployerPrivateKey = vm.envUint("PRIVATE_KEY");
        vm.startBroadcast(deployerPrivateKey);

        AMPRegistry registry = new AMPRegistry(address(0));
        AMPSettlement settlement = new AMPSettlement(address(registry), address(0));
        
        registry.setSettlement(address(settlement));

        vm.stopBroadcast();

        console.log("AMPRegistry Deployed at:", address(registry));
        console.log("AMPSettlement Deployed at:", address(settlement));
    }
}
