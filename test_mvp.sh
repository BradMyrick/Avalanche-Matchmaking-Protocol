#!/bin/bash
set -e

echo "=========================================="
echo " Starting AMP E2E Test Suite"
echo "=========================================="

# 1. Start Anvil
echo "[1/4] Starting local Anvil node..."
anvil > anvil.log 2>&1 &
ANVIL_PID=$!
sleep 2 # wait for node to boot

# 2. Deploy Contracts
echo "[2/4] Deploying AMP Contracts..."
cd contracts
export PRIVATE_KEY=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 # fake test key
forge script script/Deploy.s.sol --rpc-url http://localhost:8545 --broadcast > deploy_logs.txt
REGISTRY_ADDR=$(cat deploy_logs.txt | grep "AMPRegistry Deployed at:" | awk '{print $4}')
SETTLEMENT_ADDR=$(cat deploy_logs.txt | grep "AMPSettlement Deployed at:" | awk '{print $4}')
echo "Registry: $REGISTRY_ADDR"
echo "Settlement: $SETTLEMENT_ADDR"
cd ..

# 3. Start Matchmaker
echo "[3/4] Starting Rust Matchmaker..."
cd match_maker
export VERIFIER_KEY="0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef" # fake test key
cargo run > matchmaker.log 2>&1 &
MM_PID=$!
sleep 3 # wait for server compile/boot
cd ..

# 4. Run C++ Native SDK Test
echo "[4/5] Running C++ Native SDK Matchmaker Client..."
cd sdk/cpp
./build/amp_test || { echo "C++ SDK Test Failed!"; kill $ANVIL_PID; kill $MM_PID; exit 1; }
cd ../..

# 5. Run JS Simulator
echo "[5/5] Running TS Headless Client Simulator..."
cd sdk/js
export REGISTRY_ADDR
export SETTLEMENT_ADDR
export VERIFIER_KEY="0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef" # fake test key

# Use ts-node to run sim.ts
npx ts-node -O '{"module":"commonjs", "moduleResolution":"node"}' sim.ts || { echo "E2E Test Failed!"; kill $ANVIL_PID; kill $MM_PID; exit 1; }

echo "=========================================="
echo " Tests Passed! Cleaning up background procs"
echo "=========================================="

kill $ANVIL_PID
kill $MM_PID
exit 0