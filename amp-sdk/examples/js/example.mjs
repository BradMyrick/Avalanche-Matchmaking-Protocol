// AMP JavaScript / TypeScript SDK end-to-end example.
//
// Requires a running AMP server (default 127.0.0.1:50051) and a built native
// module (cd amp-sdk/js && npm install && npm run build).
//
// Each player authenticates with an explicit private key (the SDK never
// generates silent ephemeral keys — see SECURITY_REVIEW.md S2) and submits a
// real EIP-712 outcome signature (S1). Two distinct anvil/hardhat keys are
// used by default so the example runs out-of-the-box against a local server.
//
//   AMP_ADDR=127.0.0.1:50051 AMP_CHAIN_ID=43113 \
//   AMP_SETTLEMENT=0x... node example.mjs

import { randomBytes } from "node:crypto";
import { AmpClient } from "../../js/dist/index.js";

// Well-known anvil/hardhat test keys (NEVER use in production).
const PLAYER_A_KEY = process.env.AMP_PLAYER_A_KEY ?? "0x59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d";
const PLAYER_B_KEY = process.env.AMP_PLAYER_B_KEY ?? "0x5de4111afa1a4b94908f83103eb1f1706367c2e68ca870fc3fb9a804cdab365a";

const ADDR = process.env.AMP_ADDR ?? "127.0.0.1:50051";
const GAME_ID = Number(process.env.AMP_GAME_ID ?? "0");
const DOMAIN = {
  chainId: Number(process.env.AMP_CHAIN_ID ?? "43113"),
  verifyingContract: process.env.AMP_SETTLEMENT ?? "0x0000000000000000000000000000000000000000",
};

async function runOpponent() {
  await new Promise((r) => setTimeout(r, 500));
  try {
    const b = new AmpClient({ address: ADDR, domain: DOMAIN });
    await b.connect();
    await b.authenticate(GAME_ID, PLAYER_B_KEY);
    console.log("[Player B] Connected & authenticated. Requesting match...");
    await b.requestMatch(String(GAME_ID));
  } catch (e) {
    console.error("[Player B] Error:", e.message ?? e);
  }
}

async function main() {
  console.log(`Starting AMP JS SDK example on ${ADDR}...`);
  const a = new AmpClient({ address: ADDR, domain: DOMAIN });
  await a.connect();
  await a.authenticate(GAME_ID, PLAYER_A_KEY);
  console.log("[Player A] Connected & authenticated to the AMP matchmaker.");

  runOpponent(); // spawn Player B so matchmaking finds a pair

  console.log(`Requesting match in game ${GAME_ID}...`);
  const { matchId } = await a.requestMatch(String(GAME_ID));
  console.log(`Matched! Match ID: ${matchId}`);

  console.log("Simulating game...");
  await new Promise((r) => setTimeout(r, 500));

  // Outcome = 1 (WIN_A). transcript_hash MUST be 32 bytes.
  const transcriptHash = randomBytes(32);
  console.log("Submitting outcome (Player A wins) to verifier...");
  const sig = await a.submitOutcome(matchId, 1, transcriptHash);
  console.log(`Verifier countersignature: 0x${Buffer.from(sig).toString("hex")}`);
  console.log("JS SDK example complete.");
  await a.close();
}

main().catch((e) => {
  console.error("Example failed:", e);
  process.exit(1);
});
