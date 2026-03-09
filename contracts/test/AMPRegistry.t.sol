// SPDX-License-Identifier: MIT
pragma solidity ^0.8.20;

import "forge-std/Test.sol";
import "../src/AMPRegistry.sol";
import "../src/AMPTypes.sol";

contract AMPRegistryTest is Test {
    AMPRegistry public registry;
    address public playerA = address(0x10);
    address public playerB = address(0x20);

    function setUp() public {
        registry = new AMPRegistry(address(0));
        vm.deal(playerA, 10 ether);
        vm.deal(playerB, 10 ether);
    }

    function testRegisterGame() public {
        address[] memory verifiers = new address[](1);
        verifiers[0] = address(0x123);
        uint256 gameId = registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 0.1 ether, address(0), address(0));
        assertEq(gameId, 0);
    }

    function testCreateMatch() public {
        address[] memory verifiers = new address[](1);
        uint256 gameId = registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 0.1 ether, address(0), address(0));

        vm.prank(playerA);
        uint256 matchId = registry.createMatch{value: 0.1 ether}(gameId, 0.1 ether);
        assertEq(matchId, 0);
        
        (,address pA, address pB, uint256 stake,,) = registry.matches(matchId);
        assertEq(pA, playerA);
        assertEq(pB, address(0));
        assertEq(stake, 0.1 ether);
    }

    function testJoinMatch() public {
        address[] memory verifiers = new address[](1);
        uint256 gameId = registry.registerGame(AMPTypes.SettlementMode.ASYNC_VERIFIER, verifiers, 0.1 ether, address(0), address(0));

        vm.prank(playerA);
        uint256 matchId = registry.createMatch{value: 0.1 ether}(gameId, 0.1 ether);

        vm.prank(playerB);
        registry.joinMatch{value: 0.1 ether}(matchId);

        (,address pA, address pB,, AMPTypes.MatchState state,) = registry.matches(matchId);
        assertEq(pA, playerA);
        assertEq(pB, playerB);
        assertEq(uint(state), uint(AMPTypes.MatchState.READY));
    }
}
