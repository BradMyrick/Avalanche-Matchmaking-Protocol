#!/usr/bin/env bash
# ==============================================================================
# AMP PROTOCOL - 2 MINUTE DEMO SCRIPT (FUJI TESTNET)
# ==============================================================================
set -e

# Setup environment paths
export PATH=$PATH:/home/kodr/local/bin:/home/kodr/.dotnet:/home/kodr/.dotnet/tools:/home/kodr/tools/cmake-3.28.1-linux-x86_64/bin
export DOTNET_ROOT=/home/kodr/.dotnet
export DOTNET_TOOLS=/home/kodr/.dotnet/tools

# Load environment variables
if [ -f .env ]; then
  echo "Loading environment from .env..."
  export $(grep -v '^#' .env | xargs)
fi

echo "--------------------------------------------------"
echo " [DEMO] STARTING BACKEND SERVICES (DOCKER)..."
echo "--------------------------------------------------"

# Check for docker permissions
DOCKER_CMD="docker compose"
if ! docker ps > /dev/null 2>&1; then
    echo "Running with sudo (group change pending relog)..."
    DOCKER_CMD="sudo docker compose"
fi

echo "=================================================="
echo " AMP PROTOCOL - LIVE MATCH SETTLEMENT DEMO"
echo "=================================================="
$DOCKER_CMD up -d > /dev/null
echo "Backend Services: STARTED"

echo "--------------------------------------------------"
echo " [PREP] INITIALIZING ON-CHAIN STATE..."
echo "--------------------------------------------------"
./scripts/setup_fuji.sh > /dev/null
echo "Fuji Testnet State: INITIALIZED"

echo "--------------------------------------------------"
echo " [MATCH] STARTING MULTI-SDK WORKLOADS"
echo "--------------------------------------------------"
# Start the demo-clients container and wait for it to exit
$DOCKER_CMD up demo-clients --exit-code-from demo-clients

echo ""
echo "=================================================="
echo " [FINAL] VERIFYING ON-CHAIN SETTLEMENT"
echo "=================================================="
echo "Settlement Transactions: PROCESSING..."
sleep 20

for i in {0..2}; do
    STATUS=$(cast call $AMP_REGISTRY_ADDRESS "matches(uint256)" $i --rpc-url $FUJI_RPC_URL | head -n 1)
    if [[ "$STATUS" == *"2"* ]]; then
        echo "Match $i: SETTLED (Registry: $AMP_REGISTRY_ADDRESS)"
        echo "   -> https://testnet.snowtrace.io/address/$AMP_REGISTRY_ADDRESS"
    else
        echo "Match $i: FAILED (Current Status: $STATUS)"
        exit 1
    fi
done

echo ""
echo "=================================================="
echo " DEMO COMPLETED SUCCESSFULLY"
echo "=================================================="
