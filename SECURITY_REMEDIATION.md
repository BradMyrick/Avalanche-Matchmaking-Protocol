# P0 Security Remediation — Implementation Spec

> Status: **blocking.** Do not put real funds behind the relayer until P0-1…P0-5 are implemented and verified. The API currently has no authentication and the relayer blindly signs attacker-supplied payouts. See the audit (C1/C2/C3/H1/H2) for the threat model.

This spec is implementation-grade: file paths, signatures, DDL, and the exact checks. Execute top-to-bottom; each item lists its files.

---

## P0-1 — Relayer re-derives winners (no payout addresses in the job payload)

**Goal:** eliminate C1. A `finalize` job must carry **only** `{ tournamentId }`. The relayer loads the bracket from Postgres, re-derives the winner order deterministically, maps winner ids → wallets from the bracket's `players`, and signs. The payload never carries payout addresses.

### Files
- `relayer/src/main.rs` — `finalize_job` + new `derive_winners` module.
- `relayer/src/bracket.rs` (new) — single-elim reconstruction + winner derivation (Rust port of `web/src/lib/engine/singleElim.ts` + `Tournament::winners()`).
- `web/src/app/api/tournament/[id]/finalize/route.ts` — stop accepting `winnerWallets`; accept only an organizer signature + emit `{ tournamentId }`.

### Rust winner derivation (port from TS, single-elim only for now)
`derive_winners(bracket: &BracketState) -> Result<Vec<String>>`:
1. Parse `players` → `Vec<(id, seed, wallet)>`. Sort by `seed` asc → seeding order.
2. `bracket_size = next_pow2(n)`. `seed_slots(bracket_size)` → rank→slot (bit-reversal).
3. Build round-0 matches from seeded slots; slots beyond `n` are byes.
4. Allocate match ids round-major (start at 1) **identically** to the TS engine (`web/src/lib/engine/singleElim.ts`). This is the load-bearing invariant — the ids must match the `results` the web recorded.
5. Replay `results` (sorted by matchId asc): for each, set outcome, propagate winner into the `winnerTo` target slot, auto-complete byes.
6. `champion` = winner of the final match (the one with no `winner_to`).
7. Ordered winners = `[champion] ++ rest.sorted_by(|a,b| (elim_round(b), seed(a)).cmp((elim_round(a), seed(b))))` — elimination round desc, then seed asc (mirrors `Tournament::singleElimOrder` in `web/src/lib/engine/tournament.ts`).
8. Map the top `payoutBps.len()` winner ids → wallets via `players`.

**Hard guards in `finalize_job` before signing:**
- Tournament exists and `state == OPEN` (read `tournaments.state`; the relayer now reads Postgres, not just the queue).
- Bracket `is_complete` (all non-bye matches have a recorded result). Reject otherwise.
- Not already finalized (idempotent).
- `derive_winners` length `== payoutBps.len()`; else reject (payout/placement mismatch).

### Parity check
Because the relayer and the web both derive winners, add a one-time cross-check before sign+submit (defense against a port bug): after deriving, optionally `eth_call` the contract's `getTournament(id)` to confirm `state` is still `OPEN`. (Contract-level winner correctness is guaranteed by the derivation matching the bracket; the contract only enforces signature + structure.)

---

## P0-2 — Authorization (organizer bearer token + player EIP-191 signatures)

**Goal:** eliminate C3 + H1. Only the tournament's organizer can write bracket state or enqueue finalize; only a player who proves wallet ownership can report a result.

### Schema (see migration `migrations/0003_security.sql` below)
- `tournaments.organizer_wallet TEXT`, `tournaments.manage_token TEXT UNIQUE`.
- `report_nonces(wallet TEXT, tournament_id BIGINT, match_id BIGINT, nonce TEXT, used_at TIMESTAMPTZ, PRIMARY KEY(wallet, tournament_id, match_id, nonce))`.

### Organizer writes — bearer `manage_token`
Routes: `PUT /api/tournament/[id]/bracket`, `POST /api/tournament/[id]/finalize`, `POST /api/tournament/[id]/init` (the upsert).

- Token issuance (at creation):
  - `/init` (AVAX path): generate `manage_token = randomBytes(32).toString('hex')`, store on the `tournaments` row, return it in the response. The `/setup` page passes it to `/manage/[id]` via `?token=` (fragment `#token=` preferred — never in query logs) or `sessionStorage`.
  - `/api/paypal/capture` (custodial path): same — issue `manage_token`, return it; the setup page stores + passes it.
- Verify (new helper `web/src/lib/auth.ts`):
  ```ts
  export async function requireOrganizer(req: Request, tid: number): Promise<TournamentRecord> {
    const auth = req.headers.get("authorization") ?? "";
    const token = auth.startsWith("Bearer ") ? auth.slice(7) : "";
    const rec = await getStore().getTournament(tid);
    if (!rec || !rec.manageToken || !timingSafeEqual(token, rec.manageToken)) {
      throw new Response("unauthorized", { status: 401 });
    }
    return rec;
  }
  ```
  (`timingSafeEqual` — constant-time compare, never `===` on secrets.)
- Every organizer-write route calls `requireOrganizer` first; on 401, return.
- `/manage/[id]/page.tsx` stores the token (from nav state) in `sessionStorage` and sends `Authorization: Bearer <token>` on every bracket/finalize fetch.

### Player writes — EIP-191 signature on `/report`
Body changes from `{wallet, matchId, outcome}` to:
```ts
{ wallet, matchId, outcome, nonce, ts, sig }
```
- `sig = personal_sign(wallet_privkey, "AMP-report:" + tournamentId + ":" + matchId + ":" + outcome + ":" + nonce + ":" + ts)` (EIP-191, via `signer.signMessage` in the browser).
- Server checks (`web/src/app/api/tournament/[id]/report/route.ts`):
  1. `Math.abs(Date.now()/1000 - ts) <= 60` (replay window).
  2. `nonce` is cryptographically random, ≥ 16 bytes hex.
  3. `ecrecover(message_hash, sig) === wallet` (use `ethers.verifyMessage(message, sig)`).
  4. `INSERT INTO report_nonces(...) ON CONFLICT DO NOTHING` affected exactly 1 row (else replay → 409).
  5. Then the existing side/reconciliation logic.
- `web/src/app/play/[id]/page.tsx`: after `connectWallet()`, sign each report with `signer.signMessage(...)` and include `nonce` (random) + `ts`.

### `/init` hardening (H2)
- Require a bearer `manage_token` for any subsequent `/init` upsert (the first call creates the row + token; later calls must present it).
- Validate `payoutBps` sums to exactly 10000 server-side.
- Ignore client-supplied `txHash` for state purposes; optionally `eth_getTransactionReceipt` to confirm the sponsor funded (don't trust `txHash` string).

---

## P0-3 — PayPal idempotency + real capture (eliminate C2)

### Files
- `migrations/0003_security.sql` — `funding_intents` table.
- `web/src/lib/paypal.ts` — add `captureOrder(orderId)` that calls PayPal's `POST /v2/checkout/orders/{id}/capture` with `PayPal-Request-Id` (PayPal's own idempotency) and returns the capture object.
- `web/src/app/api/paypal/capture/route.ts` — rewrite the fulfillment.

### Flow
1. `verifyOrder` (status check) — keep, but then **call `captureOrder`** to actually move the funds (or, for an already-captured order, get the existing capture). PayPal's capture is idempotent on `PayPal-Request-Id`.
2. Idempotency gate before enqueuing:
   ```sql
   INSERT INTO funding_intents (paypal_order_id, amount_usd)
   VALUES ($1, $2)
   ON CONFLICT (paypal_order_id) DO NOTHING
   RETURNING id;
   ```
   - If a row is returned → first time → enqueue the `fund` job, store its `job_id` on the `funding_intents` row, return `{ jobId }`.
   - If no row (conflict) → this order was already processed → return the existing `{ jobId, tournamentId }` (look it up) as a success. **No second job, no second funding.**

---

## P0-4 — Rate limiting + body-size caps

### Files
- `web/src/middleware.ts` (new) — Edge middleware.
- `@upstash/ratelimit` + `@upstash/redis` deps.
- Each route — early `Content-Length` / body-size check.

### Rate limit (Upstash)
- `middleware.ts` runs on `/api/*`. Per-IP sliding window:
  - `/api/paypal/*`, `/api/tournament/*/finalize`, `/api/tournament/*/init` → **5/min**.
  - `/api/tournament/*/report` → **30/min**.
  - other `/api/*` → **60/min**.
- On limit → `429`.

### Body size
In each POST/PUT handler:
```ts
const cl = Number(req.headers.get("content-length") ?? 0);
if (cl > 64 * 1024) return NextResponse.json({ error: "payload too large" }, { status: 413 });
```
And cap array lengths explicitly: `winnerWallets.length <= 16`, `players.length <= 1024`, `results.length <= 4096`, `payoutBps.length <= 16`.

---

## P0-5 — Strip job payloads; move side-effects off GET (eliminate L2/L3)

### Files
- `web/src/app/api/job/[id]/route.ts` — stop returning `payload`; stop doing writes on GET.
- `relayer/src/main.rs` — after a successful `fund` job (bracket mode), **the relayer** inserts the `tournaments` + `brackets` rows (it already has the payload + the new tournamentId).

### Changes
- `GET /api/job/[id]` returns **only** `{ id, status, tournamentId, txHash, error }`. Never `payload`. (Job ids are still enumerable, but no sensitive data leaks.)
- The lazy `saveTournament`/`saveBracket` provisioning currently in the GET handler moves into the relayer's `fund_job` success path (the relayer is authenticated to write these rows by virtue of processing the job). The web then just reads them via `GET /api/tournament/[id]`.
- Keep `GET /api/job/[id]` read-only.

---

## `migrations/0003_security.sql`

```sql
-- Organizer authorization
ALTER TABLE tournaments
  ADD COLUMN IF NOT EXISTS organizer_wallet TEXT,
  ADD COLUMN IF NOT EXISTS manage_token     TEXT UNIQUE;

-- Player report replay protection
CREATE TABLE IF NOT EXISTS report_nonces (
  wallet        TEXT      NOT NULL,
  tournament_id BIGINT    NOT NULL,
  match_id      BIGINT    NOT NULL,
  nonce         TEXT      NOT NULL,
  used_at       TIMESTAMPTZ NOT NULL DEFAULT now(),
  PRIMARY KEY (wallet, tournament_id, match_id, nonce)
);

-- PayPal capture idempotency
CREATE TABLE IF NOT EXISTS funding_intents (
  paypal_order_id TEXT PRIMARY KEY,
  job_id          BIGINT,
  tournament_id   BIGINT,
  amount_usd      NUMERIC(12,2) NOT NULL,
  created_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

-- Traceability on the queue
ALTER TABLE relayer_jobs ADD COLUMN IF NOT EXISTS paypal_order_id TEXT;
```

---

## Implementation order (dependency-aware)

1. **Schema** — apply `0003_security.sql`.
2. **P0-2 organizer auth** — `auth.ts` + bearer token issuance on `/init` + `/capture`; wire `/bracket`, `/finalize`, `/init` through `requireOrganizer`; update `/manage` page to send the token. (Unblocks testing the rest safely.)
3. **P0-2 player auth** — EIP-191 on `/report` + nonces; update `/play` page.
4. **P0-1 relayer re-derivation** — port single-elim + `winners()` to Rust; change `finalize` job to `{tournamentId}`; update `/finalize` route to emit that.
5. **P0-3 PayPal idempotency** — `funding_intents` + real `captureOrder`.
6. **P0-4 rate limit + body caps** — middleware + Upstash.
7. **P0-5 payload strip + relayer writes rows** — `/api/job/[id]` slimming + relayer `fund_job` row provisioning.

## Verification (must pass before declaring P0 done)
- **C1 repro → fixed:** with no token, `POST /api/tournament/N/finalize` → 401. With a stolen/absent token the relayer never signs. The relayer's `derive_winners` matches the web's `winners()` on a corpus of brackets (add a `relayer/tests` snapshot test against fixtures generated by the TS engine).
- **C2 repro → fixed:** replaying the same `orderID` to `/capture` returns the original `tournamentId`, creates exactly one `fund` job.
- **C3 repro → fixed:** `PUT /bracket` with no/wrong bearer → 401.
- **H1 repro → fixed:** `/report` with a `sig` that doesn't recover to `wallet` → 401; replayed `(wallet,nonce)` → 409.
- Load test the rate limiter (burst → 429). Confirm body-size 413 on a 1MB POST.
- End-to-end: AVAX sponsor path (fund → manage → finalize → claim) and PayPal custodial path (capture → relayer funds+provisions → manage → finalize → claim), both with valid tokens/sigs, both failing without.

## Out of scope for P0 (track separately)
- Moving organizer auth from bearer token → wallet-signed (full non-repudiation) once custodial tournaments have an organizer-wallet model.
- `AMPTournamentCup` TimelockController transfer (so the "TimelockController Governance" badge becomes truthful).
- The PayPal webhook route (reliable fulfillment fallback to capture).
- Rust relayer: round-robin / Swiss winner derivation (only single-elim is specced above; block other formats from finalize until ported).
