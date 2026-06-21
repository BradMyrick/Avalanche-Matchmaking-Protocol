// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Script.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPSettlement.sol";
import "openzeppelin-contracts/contracts/access/Ownable2Step.sol";
import "openzeppelin-contracts/contracts/governance/TimelockController.sol";

/// @notice Deploys the AMP protocol with both economically-critical contracts
/// (`AMPRegistry` and `AMPSettlement`) wrapped in a `TimelockController`.
///
/// **Why timelock at deploy time:** `AMPRegistry.setSettlement` is `onlyOwner`
/// and the trusted settlement address can route arbitrary payouts from any
/// non-terminal match. Without a timelock, a compromised deployer key can
/// re-point settlement to a drainer contract and rug every staked match in a
/// single transaction. Wrapping ownership at deploy time closes that vector:
/// after the finalize step, every economic action (setSettlement, withdrawFees,
/// pause/unpause, updateProtocolFeeBps, updateProtocolFeeRecipient) must be
/// scheduled → delayed → executed through the timelock, giving users a window
/// to exit before a malicious change lands.
///
/// **Env vars:**
/// - `PRIVATE_KEY`                — deployer key (also initial timelock governor)
/// - `TIMELOCK_MIN_DELAY`         — delay in seconds (default 0 = local/anvil;
///                                   use ≥ 1 days for Fuji/mainnet)
/// - `TIMELOCK_GOVERNOR`          — future governor address (default: deployer;
///                                   transfer this to a multisig post-deploy)
contract DeployScript is Script {
    function run() external {
        uint256 deployerPrivateKey = vm.envUint("PRIVATE_KEY");
        address deployer = vm.addr(deployerPrivateKey);
        vm.startBroadcast(deployerPrivateKey);

        uint256 minDelay = vm.envOr("TIMELOCK_MIN_DELAY", uint256(0));
        address governor = vm.envOr("TIMELOCK_GOVERNOR", deployer);

        // 1. Governance timelock. Single-governor proposer+executor by default;
        //    `admin = address(0)` means no party can cancel without going
        //    through the delay (cancels come only from proposers).
        address[] memory proposers = new address[](1);
        proposers[0] = governor;
        address[] memory executors = new address[](1);
        executors[0] = governor;
        TimelockController timelock = new TimelockController(minDelay, proposers, executors, address(0));

        // 2. Protocol contracts. Deployer is initial owner so we can wire
        //    settlement before ownership transfers complete.
        AMPRegistry registry = new AMPRegistry();
        AMPSettlement settlement = new AMPSettlement(address(registry));

        // 3. Initial wiring while deployer is still registry owner. This is
        //    the ONLY economically-sensitive owner action performed outside
        //    the timelock; every subsequent owner action requires the delay.
        registry.setSettlement(address(settlement));

        // 4. Hand both contracts to the timelock via Ownable2Step. The pending
        //    owner (the timelock) must call acceptOwnership() — routed through
        //    the timelock itself.
        settlement.transferOwnership(address(timelock));
        registry.transferOwnership(address(timelock));

        bytes memory accept = abi.encodeWithSelector(Ownable2Step.acceptOwnership.selector);
        timelock.schedule(address(settlement), 0, accept, bytes32(0), bytes32(0), minDelay);
        timelock.schedule(address(registry), 0, accept, bytes32(0), bytes32(0), minDelay);

        // For local/anvil deploys (minDelay == 0), finalize in the same script.
        // For Fuji/mainnet (minDelay > 0), the operator runs
        // `FinalizeGovernance.s.sol` after the delay elapses — at which point
        // ownership is fully under the timelock and the deployer key loses
        // economic authority.
        if (minDelay == 0) {
            timelock.execute(address(settlement), 0, accept, bytes32(0), bytes32(0));
            timelock.execute(address(registry), 0, accept, bytes32(0), bytes32(0));
        }

        vm.stopBroadcast();

        console.log("AMPRegistry Deployed at:", address(registry));
        console.log("AMPSettlement Deployed at:", address(settlement));
        console.log("AMPTimelock Deployed at:", address(timelock));
        console.log("AMPRegistry owner:", registry.owner());
        console.log("AMPSettlement owner:", settlement.owner());
    }
}
