#!/usr/bin/env zsh
source ~/.zshrc 2>/dev/null || true
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

# 2.5 Start Telemetry Receiver
echo "[2.5/4] Starting Telemetry Receiver..."
cd amp-telemetry
cargo run --bin amp-telemetry > telemetry.log 2>&1 &
TELEMETRY_PID=$!
sleep 2 # wait for receiver
cd ..

# 3. Start Matchmaker
echo "[3/4] Starting Rust Matchmaker..."
cd amp-server
export VERIFIER_KEY="0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef" # fake test key
cargo run > matchmaker.log 2>&1 &
MM_PID=$!
sleep 3 # wait for server compile/boot
cd ..

# 4. Run C++ Native SDK Test
echo "[4/5] Running C++ Native SDK Matchmaker Client..."
cd amp-sdk/cpp_example
./build/amp_test || { echo "C++ SDK Test Failed!"; kill $ANVIL_PID; kill $MM_PID; kill $TELEMETRY_PID; exit 1; }
cd ../..

# 5. Run c#.NET SDK Test

echo "[5/5] Running C# .NET SDK Matchmaker Client..."

cd amp-sdk/csharp_example

# Choose framework based on installed runtimes
if dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 10\." ; then
  FRAMEWORK="net10.0"
elif dotnet --list-runtimes | grep -q "Microsoft.NETCore.App 8\." ; then
  FRAMEWORK="net8.0"
else
  echo "Skipping C# SDK test (no .NET 8.x or 10.x runtime found)"
  cd ../..
  # skip test but keep suite green
  echo "=========================================="
  echo " Tests Passed! (C# SDK test skipped)"
  echo "=========================================="
  if [ -n "$ANVIL_PID" ]; then
      kill $ANVIL_PID 2>/dev/null || true
  fi
  if [ -n "$MM_PID" ]; then
      kill $MM_PID 2>/dev/null || true
  fi
  if [ -n "$TELEMETRY_PID" ]; then
      kill $TELEMETRY_PID 2>/dev/null || true
  fi
  exit 0
fi

zsh -ic "dotnet run --framework $FRAMEWORK" || {
  echo "C# SDK Test Failed!"
  kill $ANVIL_PID 2>/dev/null || true
  kill $MM_PID 2>/dev/null || true
  kill $TELEMETRY_PID 2>/dev/null || true
  exit 1
}

cd ../..

echo "=========================================="
echo " Tests Passed!"
echo "=========================================="
if [ -n "$ANVIL_PID" ]; then
    kill $ANVIL_PID 2>/dev/null || true
fi
if [ -n "$MM_PID" ]; then
    kill $MM_PID 2>/dev/null || true
fi
if [ -n "$TELEMETRY_PID" ]; then
    kill $TELEMETRY_PID 2>/dev/null || true
fi
exit 0

