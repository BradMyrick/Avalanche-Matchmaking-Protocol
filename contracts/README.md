# @amp-protocol/contracts — Avalanche Tournament Protocol

Source-verified smart contracts for sponsor prize pools and verifiable tournament settlement on Avalanche (C-Chain and Avalanche L1s/Subnets).

---

## Deployed Contracts (Avalanche Fuji Testnet)

| Contract | Address | Network | Description |
|:---|:---|:---|:---|
| **`AMPTournamentCup`** | [`0x7c743c1c9ae3e7a65d030098f2249b7787d66dff`](https://testnet.snowtrace.io/address/0x7c743c1c9ae3e7a65d030098f2249b7787d66dff) | Fuji (ChainID: 43113) | Sponsor prize pools, EIP-712 verified standings, winner pull-claims (AVAX & ERC-20) |

---

## 1. AMPTournamentCup.sol

`AMPTournamentCup` is the primary tournament escrow contract. It allows sponsors, DAOs, guilds, or game studios to fund automated prize cups with native AVAX or ERC-20 tokens (e.g. USDC).

### Workflow

```
┌─────────────────┐       ┌────────────────────┐       ┌─────────────────┐
│ 1. Sponsor Funds│       │ 2. Game / Bracket  │       │ 3. Winners Claim│
│ createTournament│ ────> │   finalizeTournament│ ────> │   claimPrize    │
│ (AVAX or ERC20) │       │ (EIP-712 Attested) │       │  (Pull-Payment) │
└─────────────────┘       └────────────────────┘       └─────────────────┘
```

1. **Funding & Configuration (`createTournament` / `createTournamentERC20`)**:
   - The sponsor deposits the prize pool.
   - Sets a payout split in basis points (e.g. `[7000, 3000]` for 70% 1st, 30% 2nd). Must sum to 10,000 bps.
   - Designates an authorized `verifier` address (the tournament coordinator or server key).
   - Sets a `finalizeDeadline` timestamp after which funds can be refunded if the tournament is abandoned.
2. **Attestation (`finalizeTournament`)**:
   - Once matches finish, the verifier signs an EIP-712 typed data message:
     ```solidity
     keccak256("TournamentResult(uint256 tournamentId,address[] winners)")
     ```
   - Anyone (or an automated bot) can submit the transaction with the verifier's signature.
3. **Winner Claims (`claimPrize`)**:
   - Winners pull-claim their prize directly to their wallet.
   - Anti-MEV pull-payment model: avoids failing push-transfers and reentrancy vectors.

### Safety Invariants

- **Pull-Payments Only**: Zero external calls during finalization.
- **Deadline Protection**: If the verifier fails to finalize before `finalizeDeadline`, the sponsor can cancel and reclaim 100% of their deposited funds.
- **Fee-On-Transfer Safe**: Actual received balance deltas are measured on ERC-20 deposits.
- **Zero-Address Guards**: All winner and verifier inputs are strictly validated.

---

## Installing & Importing

### Via NPM
```bash
npm install @amp-protocol/contracts
```

Import ABIs or Solidity sources directly:
```solidity
import "@amp-protocol/contracts/AMPTournamentCup.sol";
```

### Via Foundry
```bash
forge install BradMyrick/Avalanche-Matchmaking-Protocol
```

---

## Deploying to an Avalanche L1 (Subnet)

You can deploy `AMPTournamentCup` to any custom Avalanche L1 in under 60 seconds:

```bash
export RPC_URL="https://your-avalanche-l1-rpc.xyz"
export PRIVATE_KEY="0x..."
export FEE_RECIPIENT="0x..."

forge script script/DeployTournamentCup.s.sol \
  --rpc-url $RPC_URL \
  --broadcast \
  --skip-simulation
```

> **Note on Avalanche EVM**: Use `--skip-simulation` with Foundry on Avalanche C-Chain / L1s because pending block simulation can fail on certain subnet nodes.

---

## Running Contract Tests

```bash
forge test -vvv
```

All 16 unit and invariant test suites for `AMPTournamentCup` execute and pass in milliseconds.
