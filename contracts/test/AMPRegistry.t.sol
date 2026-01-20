// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "forge-std/Test.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPTypes.sol";

contract AMPRegistryTest is Test {
    AMPRegistry public registry;

    function setUp() public {
        registry = new AMPRegistry();
    }

    function testRegisterGame() public {
        address[] memory verifiers = new address[](1);
        verifiers[0] = address(0x123);
        uint256 gameId = registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 0.1 ether, address(0));
        assertEq(gameId, 0);
    }

    function testCreateMatch() public {
        address[] memory verifiers = new address[](1);
        uint256 gameId = registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 0.1 ether, address(0));

        uint256 matchId = registry.createMatch{value: 0.1 ether}(gameId);
        assertEq(matchId, 0);
    }
}
