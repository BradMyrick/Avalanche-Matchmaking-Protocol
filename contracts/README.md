# AMP Protocol Smart Contracts - MVP Implementation

## Architecture & Implementation Steps
The AMP Registry and Settlement contracts have been finalized for the MVP, which focuses specifically on the turn-based `ASYNC_REPLAY` flow.

### 1. AMPRegistry.sol (Escrow & Matching)
- **`createMatch` & `joinMatch`**: Players are successfully matched and escrowed. The registry strictly checks that `msg.value` matches the `minStake` requirement for native AVAX games.
- **`settleMatch`**: A single point of exit for escrowed funds, restricted by the `onlySettlement` modifier. Iterates through recipients to handle direct value transfers and accounts for a protocol fee.
- **`getGameVerifiers`**: Added to cleanly return dynamic arrays within structs, circumnavigating a well-known Solidity getter limitation.

### 2. AMPSettlement.sol (Verification & Payout)
- **`submitAsyncResult`**: Integrated directly with `@openzeppelin/contracts/utils/cryptography/ECDSA.sol` to perform signature recovery. The `structHash` encapsulates the outcome so verifiers can produce replay-protected claims.
- **ECDSA & MessageHashUtils**: Relies on EIP-191 signatures (`\x19Ethereum Signed Message:\n32`) so the system is standard-compliant and secure.
- **`_payout`**: Distributes payouts based on the enumerator outcome. Specifically distributes proportional splits for Draws, total pools to Winners, and full refunds for Cancelled outcomes. Protocol fees are diverted to the registry.

## Security Considerations Taken
1. **Reentrancy Protection**: Used the `nonReentrant` custom modifier on functions that handle external state manipulation or payout triggers, particularly `settleMatch` and `withdrawFees`. Standard `Checks-Effects-Interactions` pattern was strictly adhered to.
2. **Access Control**: Implemented strict `onlySettlement` and `onlyOwner` modifiers. `AMPSettlement` calculates the economics but holds no funds directly, segregating risk. Only an authorized verifier (validated by the ECDSA check) can settle open matches.
3. **Array Getter Workaround**: Explicit getter (`getGameVerifiers`) implemented for struct arrays to ensure no mapping decode issues trigger unanticipated VM errors. 
4. **Stack Depth Optimization**: Enforced `via_ir` EVM compilation in `foundry.toml` to prevent `stack too deep` issues without breaking logical flow or abstracting too many variables to memory.
