// Integration test: relayer fund_job (bracket mode) + finalize_job.
// Run from web/ with DATABASE_URL set. The relayer must be running.
import pg from "pg";
import { execSync } from "child_process";

const DB = process.env.DATABASE_URL;
if (!DB) { console.error("DATABASE_URL not set"); process.exit(1); }
const pool = new pg.Pool({ connectionString: DB });
const CUP = "0x7c743c1c9ae3e7a65d030098f2249b7787d66dff";
const RPC = "https://avalanche-fuji-c-chain.publicnode.com";

const cast = (sig, ...args) =>
  execSync(`cast call ${CUP} "${sig}" ${args.join(" ")} --rpc-url ${RPC}`).toString().trim();

// Two test players (deterministic for reproducibility).
const P1 = "0x1111111111111111111111111111111111111111";
const P2 = "0x2222222222222222222222222222222222222222";

let pass = 0, fail = 0;
const ok = (msg) => { console.log(`  ✓ ${msg}`); pass++; };
const bad = (msg) => { console.error(`  ✗ ${msg}`); fail++; };

console.log("=== Relayer Integration Test ===\n");

// 1. Insert a fund job (bracket mode).
const payload = {
  payoutBps: [7000, 3000],
  winnerWallets: [],
  fundedAvax: 0.01,
  mode: "bracket",
  finalizeDays: 7,
  format: "single_elimination",
  players: [
    { id: 0, wallet: P1, name: "Alice", seed: 1 },
    { id: 1, wallet: P2, name: "Bob", seed: 2 },
  ],
  manageToken: "integration-test-token-" + Date.now(),
  paypalOrderId: "inttest-" + Date.now(),
};

const { rows: [{ id: fundJobId }] } = await pool.query(
  `INSERT INTO relayer_jobs (kind, payload, status) VALUES ('fund', $1, 'pending') RETURNING id`,
  [JSON.stringify(payload)]
);
console.log(`1. Fund job #${fundJobId} inserted. Waiting for relayer (poll every 2s)...`);

let tid = null;
for (let i = 0; i < 30; i++) {
  await sleep(2000);
  const { rows: [j] } = await pool.query(`SELECT status, tournament_id, tx_hash, error FROM relayer_jobs WHERE id=$1`, [fundJobId]);
  if (j.status === "done") { tid = Number(j.tournament_id); ok(`Fund done: tid=${tid}, tx=${j.tx_hash?.slice(0,18)}…`); break; }
  if (j.status === "failed") { bad(`Fund FAILED: ${j.error}`); await pool.end(); process.exit(1); }
  process.stdout.write(".");
}
if (!tid) { bad("Fund timed out (60s)"); await pool.end(); process.exit(1); }

// 2. Verify DB rows created by the relayer.
const { rows: [t] } = await pool.query(`SELECT state, mode, manage_token FROM tournaments WHERE tournament_id=$1`, [tid]);
const { rows: b } = await pool.query(`SELECT state FROM brackets WHERE tournament_id=$1`, [tid]);
t?.state === "OPEN" ? ok(`Tournaments row: state=OPEN, mode=${t.mode}, manageToken=${t.manage_token ? "set" : "MISSING"}`) : bad("Tournaments row wrong/missing");
const bState = b.length > 0 ? (typeof b[0].state === "string" ? JSON.parse(b[0].state) : b[0].state) : null;
bState ? ok(`Brackets row: exists (${bState.players?.length} players)`) : bad("Brackets row MISSING");

// 3. Set bracket: complete results + computedWinners.
const bracket = bState;
bracket.results = [{ matchId: 1, outcome: "A" }]; // Alice wins
bracket.computedWinners = [P1, P2]; // 1st=Alice, 2nd=Bob
bracket.finalized = true;
await pool.query(`UPDATE brackets SET state=$1 WHERE tournament_id=$2`, [JSON.stringify(bracket), tid]);
ok("Bracket updated: results + computedWinners set");

// 4. Insert finalize job (tournamentId ONLY — no addresses).
const { rows: [{ id: finJobId }] } = await pool.query(
  `INSERT INTO relayer_jobs (kind, payload, status) VALUES ('finalize', $1, 'pending') RETURNING id`,
  [JSON.stringify({ tournamentId: tid })]
);
console.log(`\n2. Finalize job #${finJobId} inserted. Waiting for relayer...`);

let finDone = false;
for (let i = 0; i < 30; i++) {
  await sleep(2000);
  const { rows: [j] } = await pool.query(`SELECT status, tx_hash, error FROM relayer_jobs WHERE id=$1`, [finJobId]);
  if (j.status === "done") { ok(`Finalize done: tx=${j.tx_hash?.slice(0,18)}…`); finDone = true; break; }
  if (j.status === "failed") { bad(`Finalize FAILED: ${j.error}`); break; }
  process.stdout.write(".");
}
if (!finDone) { bad("Finalize timed out"); await pool.end(); process.exit(1); }

// 5. Verify on-chain state.
const stateRaw = cast("getTournament(uint256)(address,address,uint256,uint16[],address,address[],uint8,uint64,uint64)", tid);
// The 7th field (index 6) is the state enum: 0=OPEN, 1=FINALIZED, 2=COMPLETE, 3=CANCELLED
// Actually the contract uses: OPEN=0, FINALIZED=1, COMPLETE=2, CANCELLED=3
// cast returns multi-line; the state is the 7th return value.
const lines = stateRaw.split("\n").map(s => s.trim());
const onChainState = parseInt(lines[6]);
onChainState === 1 ? ok(`On-chain state=FINALIZED (1)`) : bad(`On-chain state=${onChainState} (expected 1=FINALIZED)`);

console.log(`\n=== ${pass} passed, ${fail} failed ===`);
await pool.end();
process.exit(fail > 0 ? 1 : 0);

function sleep(ms) { return new Promise(r => setTimeout(r, ms)); }
