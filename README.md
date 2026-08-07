# AMP — Verifiable Tournament Engine

### Trustless tournaments on Avalanche: run the bracket, escrow the prize pool, pay winners on-chain.

AMP is a verifiable tournament engine for gaming communities. An organizer funds a sponsor prize pool, the bracket runs, and winners **pull-claim** their payout from an on-chain escrow — every result attested, every payout a public Avalanche transaction. Any game, any engine, any community.

**Live on Fuji testnet:** the `AMPTournamentCup` sponsored-prize escrow is deployed, source-verified, and has run a real end-to-end tournament. Try it at **[playwithamp.xyz](https://playwithamp.xyz)** → *Host a Tournament*.

---

## The architecture (each component in the right language)

```
┌──────────────────────────┐         ┌─────────────────────────────┐
│  web/  (Next.js + TS)    │ enqueue │  Postgres                   │
│  - /setup  /manage /play │────────▶│  - tournaments, brackets    │
│    /cup    /claim        │  jobs   │  - relayer_jobs (the queue) │
│  - TS bracket engine     │         │                             │
│  - JSON API              │◀────────│                             │
└──────────────────────────┘  poll   └─────────────────────────────┘
          │  ethers (browser wallet, sponsor path)           ▲ drain
          ▼                                                   │
┌──────────────────────────┐                          ┌──────────────────┐
│  contracts/ (Solidity)   │◀──── sign + submit ──────│  relayer/ (Rust) │
│  AMPTournamentCup        │                          │  isolated custody│
│  = the security boundary │                          │  holds the ONLY  │
└──────────────────────────┘                          │  funded key      │
                                                       └──────────────────┘
```

**Why this shape:**
- **`contracts/` (Solidity)** — the on-chain escrow. This is the real security boundary: immutable, audited, holds the funds. Non-negotiable that it's Solidity/EVM.
- **`web/` (TypeScript/Next.js)** — the product surface: UI + JSON API + the bracket engine. The bracket is pure logic running in the browser/Node — not speed- or security-critical — so TS is the right tool. One implementation.
- **`relayer/` (Rust)** — the **only** process that holds the funded key. It drains `relayer_jobs`, signs EIP-712, submits on-chain, writes back the result. Single-purpose, minimal surface, memory-safe. The web app never sees the key; a full web compromise grants **no custody ability**. This is where Rust earns its place.
- **Postgres** — the single source of truth for off-chain state + the queue that decouples the web from custody.

---

## Repository layout

```
amp/
├── web/            # Next.js + TS: UI, JSON API, bracket engine, Postgres client
│   └── src/lib/engine/    # the TS bracket engine (single-elim, round-robin, Swiss)
├── relayer/        # Rust: isolated custodial relayer (job queue → sign → submit)
├── contracts/      # Solidity (Forge): AMPTournamentCup + legacy escrow
├── docs/           # docs.page source → docs.page/bradmyrick/Avalanche-Matchmaking-Protocol
├── migrations/     # Postgres schema (tournaments, brackets, relayer_jobs)
└── scripts/        # deployment utilities
```

---

## Smart contract (Fuji)

| Contract | Address | Role |
|:---|:---|:---|
| `AMPTournamentCup` | [`0x7c743c1c9ae3e7a65d030098f2249b7787d66dff`](https://testnet.snowtrace.io/address/0x7c743c1c9ae3e7a65d030098f2249b7787d66dff) | Sponsor-funded prize pool, EIP-712 verifier-attested finalization, winner pull-claims |

Deployment + end-to-end demo manifest: [`contracts/deployment-fuji-tournament.json`](contracts/deployment-fuji-tournament.json). Forge tests: 16/16.

The legacy `AMPRegistry`/`AMPSettlement` wagering contracts remain deployed + governance-finalized on Fuji but are not used by the tournament product — see [`contracts/`](contracts/).

---

## Run it

### Database (Neon Postgres)
Provision on [neon.tech](https://neon.tech), then set `DATABASE_URL` to the **pooled** connection string — the one with `-pooler` in the hostname (e.g. `ep-…-pooler…neon.tech`), **not** the direct endpoint. The pooled endpoint is required because the web runs on Vercel serverless functions (many short-lived connections); the direct endpoint exhausts connections under load. The relayer uses the same `DATABASE_URL`.

```bash
cd web && export DATABASE_URL="$(grep '^DATABASE_URL=' .env.local | cut -d= -f2-)" && npm run db:migrate
```

### Web
```bash
cd web && npm install
# requires DATABASE_URL (Neon, pooled) and, for card payments, PayPal creds
npm run dev          # http://localhost:3000
npm test             # bracket engine property tests
npm run build
```

### Relayer (the custody process — run separately)
```bash
cd relayer && cargo run --release
# requires DATABASE_URL (same Neon) + AMP_RELAYER_KEY (funded Fuji EOA)
# must run as a persistent daemon (Fly.io / Railway / Render / VPS) — NOT on Vercel
```

### Contracts
```bash
cd contracts && forge test -vvv
```

---

## Security model

- **Pull-payment only** — winners call `claimPrize`; no push transfers to untrusted addresses.
- **`AMPTournamentCup`** is `ReentrancyGuard` + `Pausable` + `Ownable2Step`; payout splits must sum to exactly 10000 bps; placements bounded.
- **Key isolation** — the funded key lives only in the Rust relayer's env (production target: KMS/HSM, so the key never exists in software). The web process has zero custody authority.
- **EIP-712 attestation** — finalization is verifier-signed; the digest is computed identically in Solidity, the browser (ethers), and the Rust relayer.
- All keys loaded via env, never committed.

For the responsible disclosure policy, see [`SECURITY.md`](SECURITY.md).

---

## License

Apache License 2.0. See [LICENSE](LICENSE).
