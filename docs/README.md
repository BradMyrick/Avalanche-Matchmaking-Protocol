# AMP (Async Match Protocol)

AMP is a protocol for trustless game settlement on Avalanche. It supports both async verifiers (re-simulation) and real-time agreement modes.

## Monorepo Structure

- `/contracts`: Foundry Solidity project for `AMPRegistry` and `AMPSettlement`.
- `/verifier`: Rust async verifier service using Cap'n Proto.
- `/sdk/js`: TypeScript `AMPClient` SDK.
- `/web`: Minimal React web demo.
- `/schemas`: Cap'n Proto schemas for transcripts.
- `/docs`: High-level design + integration docs.

## Quickstart

### Contracts
```bash
cd contracts
# forge install (requires foundry)
forge test
```

### Verifier
```bash
cd verifier
# Generate capnp code (see README.md in /verifier)
cargo run
```

### SDK
```bash
cd sdk/js
npm install
npm run build
```

### Web Demo
```bash
cd web
npm install
npm run dev
```

## Integration: "Integrate AMP in a day"

1. **Define Transcript**: Update `schemas/match.capnp` with your game events.
2. **Implement Verifier**: Add your game logic to `verifier/src/verifier_impl.rs`.
3. **Register Game**: Use `AMPClient` to register your game on-chain with authorized verifiers.
4. **Settle Matches**: Submit signed results from your verifier to `AMPSettlement` to unlock payouts.

TODO:
- Complete economic logic in `AMPSettlement.sol`.
- Implement EIP-712 signing in the Rust verifier.
- Add full test coverage for the SDK and Web UI.
