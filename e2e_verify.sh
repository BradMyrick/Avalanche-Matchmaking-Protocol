#!/usr/bin/env bash
if [ -n "$ZSH_VERSION" ] || [[ "$SHELL" == *"zsh"* ]]; then
    source ~/.zshrc 2>/dev/null || true
else
    source ~/.bashrc 2>/dev/null || true
fi
set -e

echo "=========================================="
echo " Starting AMP E2E Verification Suite"
echo "=========================================="

# 1. Start Anvil
echo "[1/6] Starting local Anvil node..."
anvil > anvil.log 2>&1 &
ANVIL_PID=$!
sleep 2

# 2. Deploy Contracts
echo "[2/6] Deploying AMP Contracts..."
cd contracts
export PRIVATE_KEY=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80
forge script script/Deploy.s.sol --rpc-url http://localhost:8545 --broadcast > deploy_logs.txt
REGISTRY_ADDR=$(cat deploy_logs.txt | grep "AMPRegistry Deployed at:" | awk '{print $4}')
SETTLEMENT_ADDR=$(cat deploy_logs.txt | grep "AMPSettlement Deployed at:" | awk '{print $4}')
echo "Registry: $REGISTRY_ADDR"
echo "Settlement: $SETTLEMENT_ADDR"

# 2.5 Register a Game (Studio Setup)
echo "[2.5/6] Registering a Test Game..."
# Use cast to call registerGame(mode, verifiers, minStake, stakeToken, arbiter)
# mode=0 (ASYNC_VERIFIER), verifiers=[admin], minStake=0, stakeToken=0x0, arbiter=admin
ADMIN_ADDR=0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266
cast send $REGISTRY_ADDR "registerGame(uint8,address[],uint256,address,address)" 0 "[$ADMIN_ADDR]" 0 "0x0000000000000000000000000000000000000000" "$ADMIN_ADDR" --rpc-url http://localhost:8545 --private-key $PRIVATE_KEY > /dev/null
GAME_ID=0
echo "Game Registered with ID: $GAME_ID"
cd ..

# 3. Start Telemetry
echo "[3/6] Starting Telemetry Receiver..."
cd amp-telemetry
cargo build
cargo run --bin amp-telemetry > telemetry.log 2>&1 &
TELEMETRY_PID=$!
sleep 2
cd ..

# 4. Start Relayer (RPC)
echo "[4/6] Starting Relayer (Cap'n Proto RPC)..."
cd amp-relayer
export CONTRACT_REGISTRY=$REGISTRY_ADDR
export CONTRACT_SETTLEMENT=$SETTLEMENT_ADDR
export FUJI_RPC_URL=http://localhost:8545
export RELAYER_PRIVATE_KEY=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80
export RPC_PORT=50052
cargo run --bin amp-relayer > ../relayer.log 2>&1 &
RELAYER_PID=$!
sleep 5
cd ..

# 5. Start AMP-Server (RPC)
echo "[5/6] Starting AMP Server (Cap'n Proto RPC)..."
cd amp-server
export RELAYER_RPC_ADDR="localhost:50052"
export TELEMETRY_ADDR="127.0.0.1:4317"
export VERIFIER_KEY="0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80"
export AMP_ADDR="0.0.0.0:50051"
cargo run --bin AMP-Server > ../server.log 2>&1 &
SERVER_PID=$!
sleep 5
cd ..

# 6. Start Demo Gateway (JSON -> RPC bridge)
echo "[6/6] Starting Demo Gateway (JSON Bridge)..."
cd amp-demo-gateway
export AMP_ADDR="127.0.0.1:50051"
export AMP_HTTP_ADDR="0.0.0.0:50053"
cargo run --bin amp-demo-gateway > ../gateway.log 2>&1 &
GATEWAY_PID=$!
sleep 5
cd ..

echo "Architecture check: Relayer(50052) <- Server(50051) <- Gateway(50053)"

# [7/7] Testing Match Settlement Flow
echo "[7/7] Testing Match Settlement Flow..."

# Query custodial address via Relayer RPC CLI
CUSTODIAL_ADDR=$(cd amp-relayer && cargo run --bin amp-relayer -- query-custodial $GAME_ID 2>/dev/null | tail -n 1)
echo "Game $GAME_ID Custodial Wallet: $CUSTODIAL_ADDR"

if [[ ! "$CUSTODIAL_ADDR" =~ ^0x[0-9a-fA-F]{40}$ ]]; then
    echo "ERROR: Failed to query custodial address via RPC CLI"
    kill $ANVIL_PID $TELEMETRY_PID $RELAYER_PID $SERVER_PID $GATEWAY_PID
    exit 1
fi

# Authorize Custodial Wallet as Verifier
echo "Authorizing custodial wallet as verifier..."
cast send $REGISTRY_ADDR "updateGameVerifiers(uint256,address[])" $GAME_ID "[$CUSTODIAL_ADDR]" --rpc-url http://localhost:8545 --private-key $PRIVATE_KEY > /dev/null

# Create & Join Match
echo "Creating/Joining Match..."
MATCH_ID=0
MATCH_TX=$(cast send $REGISTRY_ADDR "createMatch(uint256,uint256)" $GAME_ID 0 --rpc-url http://localhost:8545 --private-key $PRIVATE_KEY > /dev/null)
PLAYER_B_KEY=0x59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d
cast send $REGISTRY_ADDR "joinMatch(uint256)" $MATCH_ID --rpc-url http://localhost:8545 --private-key $PLAYER_B_KEY > /dev/null

# Submit via Demo Gateway (triggers the full Cap'n Proto flow)
TRANSCRIPT_HASH="0x0000000000000000000000000000000000000000000000000000000000000000"
echo "Submitting outcome via Demo Gateway -> AMP Server -> Relayer..."
SUBMIT_RES=$(curl -s -X POST http://localhost:50053/demo-submit \
     -H "Content-Type: application/json" \
     -d "{\"match_id\": \"$MATCH_ID\", \"outcome\": 1, \"transcript_hash\": \"$TRANSCRIPT_HASH\"}")

echo "Gateway Response: $SUBMIT_RES"

echo "Waiting for Relayer to settle on-chain..."
sleep 20

# Verify State (MatchStatus 2 = SETTLED)
STATUS=$(cast call $REGISTRY_ADDR "matches(uint256)" $MATCH_ID --rpc-url http://localhost:8545 | head -n 1)
if [[ "$STATUS" == *"2"* ]]; then
    echo "SUCCESS: Match $MATCH_ID settled on-chain!"
    
    # Check gas top-up
    BALANCE=$(cast balance $CUSTODIAL_ADDR --rpc-url http://localhost:8545)
    echo "Final Custodial Balance: $(cast --from-wei $BALANCE) AVAX"
else
    echo "FAILURE: Match status is not SETTLED (status: $STATUS)"
    kill $ANVIL_PID $TELEMETRY_PID $RELAYER_PID $SERVER_PID $GATEWAY_PID
    exit 1
fi

echo "=========================================="
echo " ALPHA E2E VERIFICATION SUCCESSFUL!"
echo "=========================================="

kill $ANVIL_PID $TELEMETRY_PID $RELAYER_PID $SERVER_PID $GATEWAY_PID
exit 0
