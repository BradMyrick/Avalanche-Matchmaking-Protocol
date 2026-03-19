#!/usr/bin/env bash
source ./env_setup.sh
set -e

echo "=========================================="
echo " Starting AMP E2E Verification Suite"
echo "=========================================="

# Cleanup function
cleanup() {
    echo "Cleaning up..."
    kill $ANVIL_PID $TELEMETRY_PID $RELAYER_PID $SERVER_PID $GATEWAY_PID 2>/dev/null || true
}
trap cleanup EXIT

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
ADMIN_ADDR=0xf39Fd6e51aad88F6F4ce6aB8827279cffFb92266
cast send $REGISTRY_ADDR "registerGame(uint8,address[],uint256,address,address)" 0 "[$ADMIN_ADDR]" 0 "0x0000000000000000000000000000000000000000" "$ADMIN_ADDR" --rpc-url http://localhost:8545 --private-key $PRIVATE_KEY > /dev/null
GAME_ID=0
echo "Game Registered with ID: $GAME_ID"
cd ..

# 3. Build & Start Telemetry
echo "[3/6] Starting Telemetry Receiver..."
cd amp-telemetry
cargo build > /dev/null 2>&1
cargo run --bin amp-telemetry > ../telemetry.log 2>&1 &
TELEMETRY_PID=$!
sleep 2
cd ..

# 4. Build & Start Relayer (RPC)
echo "[4/6] Starting Relayer (Cap'n Proto RPC)..."
cd amp-relayer
export CONTRACT_REGISTRY=$REGISTRY_ADDR
export CONTRACT_SETTLEMENT=$SETTLEMENT_ADDR
export FUJI_RPC_URL=http://localhost:8545
export RELAYER_PRIVATE_KEY=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80
export RPC_PORT=50052
cargo build > /dev/null 2>&1
cargo run --bin amp-relayer > ../relayer.log 2>&1 &
RELAYER_PID=$!
sleep 5
cd ..

# 5. Build & Start AMP-Server (RPC)
echo "[5/6] Starting AMP Server (Cap'n Proto RPC)..."
cd amp-server
export RELAYER_RPC_ADDR="localhost:50052"
export TELEMETRY_ADDR="127.0.0.1:4317"
export VERIFIER_KEY="0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80"
export AMP_ADDR="0.0.0.0:50051"
cargo build > /dev/null 2>&1
cargo run --bin AMP-Server > ../server.log 2>&1 &
SERVER_PID=$!
sleep 5
cd ..

# 6. Build & Start Demo Gateway
echo "[6/6] Starting Demo Gateway..."
cd amp-demo-gateway
cargo build > /dev/null 2>&1
cargo run --bin amp-demo-gateway > ../gateway.log 2>&1 &
GATEWAY_PID=$!
sleep 5
cd ..

# 7. Build C++ SDK Example
echo "[7/8] Building C++ SDK Example..."
cd amp-sdk/cpp_example
mkdir -p build && cd build
cmake .. > /dev/null 2>&1
make -j > /dev/null 2>&1
cd ../../..

# 8. Run Multi-Match Flow
echo "[8/8] Running Multi-Match Verification Flow..."

# Query custodial address via Relayer RPC CLI
CUSTODIAL_ADDR=$(cd amp-relayer && cargo run --bin amp-relayer -- query-custodial $GAME_ID 2>/dev/null | tail -n 1)
echo "Game $GAME_ID Custodial Wallet: $CUSTODIAL_ADDR"

# Authorize Custodial Wallet as Verifier
echo "Authorizing custodial wallet as verifier..."
cast send $REGISTRY_ADDR "updateGameVerifiers(uint256,address[])" $GAME_ID "[$CUSTODIAL_ADDR]" --rpc-url http://localhost:8545 --private-key $PRIVATE_KEY > /dev/null

# Pre-create 3 matches on-chain
for i in {0..2}; do
    echo "Pre-creating Match $i on-chain..."
    cast send $REGISTRY_ADDR "createMatch(uint256,uint256)" $GAME_ID 0 --rpc-url http://localhost:8545 --private-key $PRIVATE_KEY > /dev/null
    # Join Player B
    PLAYER_B_KEY=0x59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d
    cast send $REGISTRY_ADDR "joinMatch(uint256)" $i --rpc-url http://localhost:8545 --private-key $PLAYER_B_KEY > /dev/null
done

# Match 0: Gateway (curl)
echo "Testing Match 0 via Demo Gateway..."
TRANSCRIPT_HASH="0x0000000000000000000000000000000000000000000000000000000000000000"
curl -s -X POST http://localhost:50053/demo-submit \
     -H "Content-Type: application/json" \
     -d "{\"match_id\": \"0\", \"outcome\": 1, \"transcript_hash\": \"$TRANSCRIPT_HASH\"}" > /dev/null

# Match 1: C++ SDK
echo "Testing Match 1 via C++ Native SDK..."
export AMP_ADDR="127.0.0.1:50051"
./amp-sdk/cpp_example/build/amp_test > cpp_test.log 2>&1 &
CPP_PID=$!

# Match 2: C# SDK
echo "Testing Match 2 via C# .NET SDK..."
cd amp-sdk/csharp_example
dotnet run > ../../csharp_test.log 2>&1 &
CS_PID=$!
cd ../..

echo "Waiting for matches to settle..."
sleep 25

# Verification
for i in {0..2}; do
    STATUS=$(cast call $REGISTRY_ADDR "matches(uint256)" $i --rpc-url http://localhost:8545 | head -n 1)
    if [[ "$STATUS" == *"2"* ]]; then
        echo "SUCCESS: Match $i settled on-chain!"
    else
        echo "FAILURE: Match $i status is not SETTLED (status: $STATUS)"
        exit 1
    fi
done

echo "=========================================="
echo " ALL MATCHES SETTLED SUCCESSFULLY!"
echo "=========================================="
exit 0
