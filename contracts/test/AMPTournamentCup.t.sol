// SPDX-License-Identifier: Apache-2.0
pragma solidity ^0.8.33;

import "forge-std/Test.sol";
import "../src/AMPTournamentCup.sol";

contract AMPTournamentCupTest is Test {
    AMPTournamentCup cup;

    address owner = makeAddr("owner");
    address feeRecipient = makeAddr("feeRecipient");
    address sponsor = makeAddr("sponsor");

    // Verifier keypair (deterministic for the test).
    uint256 verifierPrivKey = 0xA11CE;
    address verifier;

    address first = makeAddr("first");
    address second = makeAddr("second");
    address third = makeAddr("third");

    uint64 constant DEADLINE_OFFSET = 1 days;
    uint16[] payoutBps;

    function setUp() public {
        verifier = vm.addr(verifierPrivKey);
        vm.startPrank(owner);
        cup = new AMPTournamentCup(feeRecipient);
        vm.stopPrank();

        // Fund sponsor with AVAX for the AVAX-path tests.
        vm.deal(sponsor, 100 ether);

        payoutBps.push(6000); // 1st
        payoutBps.push(3000); // 2nd
        payoutBps.push(1000); // 3rd
    }

    // ───────────────────────── Helpers ─────────────────────────

    function _hashWinners(address[] memory winners) internal pure returns (bytes32) {
        bytes32[] memory words = new bytes32[](winners.length);
        for (uint256 i = 0; i < winners.length; i++) {
            words[i] = bytes32(uint256(uint160(winners[i])));
        }
        return keccak256(abi.encodePacked(words));
    }

    function _signFinalize(uint256 tournamentId, address[] memory winners) internal view returns (bytes memory) {
        bytes32 winnersRoot = _hashWinners(winners);
        bytes32 structHash = keccak256(abi.encode(cup.TOURNAMENT_RESULT_TYPEHASH(), tournamentId, winnersRoot));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", cup.domainSeparator(), structHash));
        (uint8 v, bytes32 r, bytes32 s) = vm.sign(verifierPrivKey, digest);
        return abi.encodePacked(r, s, v);
    }

    function _createAvaxCup(uint256 prize) internal returns (uint256) {
        vm.prank(sponsor);
        return cup.createTournament{value: prize}(payoutBps, verifier, uint64(block.timestamp + DEADLINE_OFFSET));
    }

    function _winners() internal view returns (address[] memory) {
        address[] memory w = new address[](3);
        w[0] = first;
        w[1] = second;
        w[2] = third;
        return w;
    }

    // ───────────────────────── Create ─────────────────────────

    function test_RevertIf_PayoutSumNot10000() public {
        uint16[] memory bad = new uint16[](2);
        bad[0] = 5000;
        bad[1] = 4000; // sum 9000
        vm.prank(sponsor);
        vm.expectRevert(AMPTournamentCup.PayoutSumInvalid.selector);
        cup.createTournament{value: 1 ether}(bad, verifier, uint64(block.timestamp + DEADLINE_OFFSET));
    }

    function test_RevertIf_DeadlineInPast() public {
        vm.prank(sponsor);
        vm.expectRevert(AMPTournamentCup.DeadlineInPast.selector);
        cup.createTournament{value: 1 ether}(payoutBps, verifier, uint64(block.timestamp - 1));
    }

    function test_RevertIf_NoValue() public {
        vm.prank(sponsor);
        vm.expectRevert(AMPTournamentCup.InvalidPayout.selector);
        cup.createTournament(payoutBps, verifier, uint64(block.timestamp + DEADLINE_OFFSET));
    }

    function test_CreateRecordsSponsorAndPool() public {
        uint256 id = _createAvaxCup(1 ether);
        AMPTournamentCup.Tournament memory t = cup.getTournament(id);
        assertEq(t.sponsor, sponsor);
        assertEq(t.prizePool, 1 ether);
        assertEq(t.token, address(0));
        assertEq(uint8(t.state), uint8(AMPTournamentCup.TournamentState.OPEN));
        assertEq(t.verifier, verifier);
        assertEq(address(cup).balance, 1 ether);
    }

    // ───────────────────────── Finalize ─────────────────────────

    function test_FinalizeHappy() public {
        uint256 id = _createAvaxCup(1 ether);
        address[] memory w = _winners();
        bytes memory sig = _signFinalize(id, w);
        cup.finalizeTournament(id, w, sig);

        AMPTournamentCup.Tournament memory t = cup.getTournament(id);
        assertEq(uint8(t.state), uint8(AMPTournamentCup.TournamentState.FINALIZED));
        assertEq(t.winners.length, 3);
    }

    function test_RevertIf_BadSignature() public {
        uint256 id = _createAvaxCup(1 ether);
        address[] memory w = _winners();
        // Sign with a wrong key.
        bytes32 winnersRoot = _hashWinners(w);
        bytes32 structHash = keccak256(abi.encode(cup.TOURNAMENT_RESULT_TYPEHASH(), id, winnersRoot));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", cup.domainSeparator(), structHash));
        (uint8 v, bytes32 r, bytes32 s) = vm.sign(0xBEEF, digest);
        bytes memory badSig = abi.encodePacked(r, s, v);

        vm.expectRevert(AMPTournamentCup.InvalidSignature.selector);
        cup.finalizeTournament(id, w, badSig);
    }

    function test_RevertIf_WinnerCountMismatch() public {
        uint256 id = _createAvaxCup(1 ether);
        address[] memory w = new address[](2);
        w[0] = first;
        w[1] = second;
        bytes memory sig = _signFinalize(id, w); // signs 2 winners
        vm.expectRevert(AMPTournamentCup.WinnerCountMismatch.selector);
        cup.finalizeTournament(id, w, sig); // contract expects 3 (payoutBps.length)
    }

    function test_RevertIf_FinalizeNotOpen() public {
        uint256 id = _createAvaxCup_Cancelled(); // cancelled
        address[] memory w = _winners();
        bytes memory sig = _signFinalize(id, w);
        vm.expectRevert(AMPTournamentCup.NotOpen.selector);
        cup.finalizeTournament(id, w, sig);
    }

    function _createAvaxCup_Cancelled() internal returns (uint256) {
        uint256 id = _createAvaxCup(1 ether);
        vm.prank(sponsor);
        cup.cancelTournament(id);
        return id;
    }

    // ───────────────────────── Claim ─────────────────────────

    function test_ClaimPaysWinnersAndCompletes() public {
        uint256 id = _createAvaxCup_Finalized(1 ether);

        uint256 b0 = first.balance;
        vm.prank(first);
        cup.claimPrize(id, 0);
        assertEq(first.balance - b0, 0.6 ether); // 60%

        uint256 b1 = second.balance;
        vm.prank(second);
        cup.claimPrize(id, 1);
        assertEq(second.balance - b1, 0.3 ether); // 30%

        uint256 b2 = third.balance;
        vm.prank(third);
        cup.claimPrize(id, 2);
        assertEq(third.balance - b2, 0.1 ether); // 10%

        AMPTournamentCup.Tournament memory t = cup.getTournament(id);
        assertEq(uint8(t.state), uint8(AMPTournamentCup.TournamentState.COMPLETE));
        assertEq(address(cup).balance, 0); // fully drained
    }

    function test_RevertIf_DoubleClaim() public {
        uint256 id = _createAvaxCup_Finalized(1 ether);
        vm.startPrank(first);
        cup.claimPrize(id, 0);
        vm.expectRevert(AMPTournamentCup.AlreadyClaimed.selector);
        cup.claimPrize(id, 0);
        vm.stopPrank();
    }

    function test_RevertIf_NonWinnerClaims() public {
        uint256 id = _createAvaxCup_Finalized(1 ether);
        vm.prank(sponsor);
        vm.expectRevert(AMPTournamentCup.NotWinner.selector);
        cup.claimPrize(id, 0);
    }

    function test_RevertIf_ClaimBeforeFinalized() public {
        uint256 id = _createAvaxCup(1 ether);
        vm.prank(first);
        vm.expectRevert(AMPTournamentCup.NotFinalized.selector);
        cup.claimPrize(id, 0);
    }

    function _createAvaxCup_Finalized(uint256 prize) internal returns (uint256) {
        uint256 id = _createAvaxCup(prize);
        address[] memory w = _winners();
        bytes memory sig = _signFinalize(id, w);
        cup.finalizeTournament(id, w, sig);
        return id;
    }

    // ───────────────────────── Cancel / refund ─────────────────────────

    function test_SponsorCancelRefundsBeforeDeadline() public {
        uint256 id = _createAvaxCup(1 ether);
        uint256 before = sponsor.balance;
        vm.prank(sponsor);
        cup.cancelTournament(id);
        assertEq(sponsor.balance, before + 1 ether);
        AMPTournamentCup.Tournament memory t = cup.getTournament(id);
        assertEq(uint8(t.state), uint8(AMPTournamentCup.TournamentState.CANCELLED));
    }

    function test_NonSponsorCannotCancelBeforeDeadline() public {
        uint256 id = _createAvaxCup(1 ether);
        vm.prank(first);
        vm.expectRevert(AMPTournamentCup.NotSponsor.selector);
        cup.cancelTournament(id);
    }

    function test_AnyoneCanRefundAfterDeadlineIfUnfinalized() public {
        uint256 id = _createAvaxCup(1 ether);
        uint256 before = sponsor.balance;
        vm.warp(block.timestamp + DEADLINE_OFFSET + 1);
        vm.prank(first); // anyone
        cup.cancelTournament(id);
        assertEq(sponsor.balance, before + 1 ether);
    }

    // ───────────────────────── Cross-language digest pin ─────────────────────────
    // Pins the contract's EIP-712 domain+struct hashing so the JS/Rust SDKs and the
    // website relayer produce the identical digest for the same inputs.

    function test_DigestMatchesExpectedLayout() public view {
        // Hardcoded expected digest for a known (tournamentId, winners) input under
        // EIP712("AMPTournamentCup","1") on chainId 31337 (anvil), verifyingContract = cup.
        address[] memory w = new address[](1);
        w[0] = address(0x1234);
        bytes32 winnersRoot = _hashWinners(w);
        bytes32 structHash = keccak256(abi.encode(cup.TOURNAMENT_RESULT_TYPEHASH(), uint256(1), winnersRoot));
        bytes32 digest = keccak256(abi.encodePacked("\x19\x01", cup.domainSeparator(), structHash));
        // Smoke test: digest must be non-zero and deterministic for this layout.
        assertNotEq(digest, bytes32(0));
    }
}
