import { ethers, NonceManager } from "ethers";
import { AMPClient } from "./src/AMPClient";

async function main() {
    console.log("Starting AMP MVP Automation Script...");

    const provider = new ethers.JsonRpcProvider("http://localhost:8545");

    // Anvil test mnemonic defaults
    const walletA = new NonceManager(new ethers.Wallet("0x59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d", provider)); // Fake Wallet A
    const walletB = new NonceManager(new ethers.Wallet("0x5de4111afa1a4b94908f83103eb1f1706367c2e68ca870fc3fb9a804cdab365a", provider)); // Fake Wallet B

    const REGISTRY_ADDR = process.env.REGISTRY_ADDR || "0x5FbDB2315678afecb367f032d93F642f64180aa3"; // Fake Registry Address
    const SETTLEMENT_ADDR = process.env.SETTLEMENT_ADDR || "0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512"; // Fake Settlement Address
    const MATCHMAKER_URL = "localhost:50051"; // Fake Matchmaker URL

    console.log("Initializing Players...");
    const clientA = new AMPClient({ provider: walletA, registryAddress: REGISTRY_ADDR, settlementAddress: SETTLEMENT_ADDR, matchmakerUrl: MATCHMAKER_URL });
    const clientB = new AMPClient({ provider: walletB, registryAddress: REGISTRY_ADDR, settlementAddress: SETTLEMENT_ADDR, matchmakerUrl: MATCHMAKER_URL });

    // Ensure game is registered (Account 0 is deployer, Account 1 is Admin for demo)
    console.log("Registering Game...");
    const stake = ethers.parseEther("1.0");
    const verifierPubKey = new ethers.Wallet(process.env.VERIFIER_KEY || "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef").address;
    const tx0 = await clientA.registry.registerGame(0, [verifierPubKey], stake, ethers.ZeroAddress);
    await tx0.wait();

    // Player A and Player B request connection to matchmaker
    console.log("Connecting players to matchmaker...");
    await clientA.connectMatchmaker(await walletA.getAddress());
    await clientB.connectMatchmaker(await walletB.getAddress());

    const matchPromiseA = clientA.requestMatch("0");
    const matchPromiseB = clientB.requestMatch("0");

    const [assignA, assignB] = await Promise.all([matchPromiseA, matchPromiseB]);
    console.log("Players matched! Internal WS Match IDs:", assignA, assignB);

    // Player A creates the match on-chain
    console.log("Player A creating match on-chain...");
    const matchId = await clientA.createMatch({ gameId: 0, stakeAmount: stake });
    console.log(`On-chain Match ID: ${matchId}`);

    // Player B joins the match
    console.log("Player B joining match on-chain...");
    await clientB.joinMatch(matchId, stake);

    // Simulate gameplay off-chain
    console.log("Simulating game...");
    await new Promise(r => setTimeout(r, 2000));

    // Player A wins
    console.log("Player A submitting outcome to Verifier...");
    const outcome = 1; // WIN_A
    const verifierResp = await clientA.submitOutcomeToMatchmaker(assignA.match_id, outcome, "");

    console.log("Verifier provided signature:", verifierResp.signature);

    console.log("Player A submitting Async Settlement to chain...");
    const balBeforeA = await provider.getBalance(await walletA.getAddress());

    // Create a promise to wait for the event
    const settlementEvent = new Promise((resolve) => {
        clientA.onMatchSettled((id: bigint, out: number, payout: bigint) => {
            console.log(`Match ${id} settled! Outcome ${out}, Total Payout: ${ethers.formatEther(payout)} AVAX`);
            resolve(true);
        });
    });

    await clientA.submitAsyncResult(matchId, {
        matchId,
        outcome,
        transcriptHash: ethers.ZeroHash,
        signature: verifierResp.signature.startsWith("0x") ? verifierResp.signature : "0x" + verifierResp.signature
    });

    await settlementEvent;

    const balAfterA = await provider.getBalance(await walletA.getAddress());
    const diff = balAfterA - balBeforeA;
    console.log(`Raw Balance Before: ${balBeforeA}`);
    console.log(`Raw Balance After: ${balAfterA}`);
    console.log(`Player A Balance Diff: ${ethers.formatEther(diff)} AVAX (Should be roughly +2.0 minus gas)`);

    console.log("E2E Test completed successfully!");
    process.exit(0);
}

main().catch(err => {
    console.error("E2E Error:", err);
    process.exit(1);
});
