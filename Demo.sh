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

$DOCKER_CMD up -d
echo "Waiting for services to initialize (Building and Installing dependencies)..."

wait_for_port() {
    local port=$1
    local name=$2
    echo "Waiting for $name on port $port..."
    while ! nc -z localhost $port >/dev/null 2>&1; do
        sleep 5
    done
    echo "$name is READY."
}

wait_for_port 50052 "Relayer"
wait_for_port 50051 "Server"
wait_for_port 50053 "Gateway"

echo "--------------------------------------------------"
echo " [DEMO] PREPARING ON-CHAIN STATE..."
echo "--------------------------------------------------"
./scripts/setup_fuji.sh

echo "--------------------------------------------------"
echo " [DEMO] RUNNING MULTI-SDK MATCH FLOW (DOCKER)"
echo "--------------------------------------------------"
# Start the demo-clients container and wait for it to exit
$DOCKER_CMD up demo-clients --exit-code-from demo-clients

echo "--------------------------------------------------"
echo " [DEMO] VERIFYING ON-CHAIN SETTLEMENT"
echo "--------------------------------------------------"
echo "Waiting for Settlement Transactions to clear..."
sleep 20

for i in {0..2}; do
    STATUS=$(cast call $AMP_REGISTRY_ADDRESS "matches(uint256)" $i --rpc-url $FUJI_RPC_URL | head -n 1)
    if [[ "$STATUS" == *"2"* ]]; then
        echo "SUCCESS: Match $i settled on-chain!"
        echo "View on Snowtrace: https://testnet.snowtrace.io/address/$AMP_REGISTRY_ADDRESS"
    else
        echo "FAILURE: Match $i status is not SETTLED (current status code: $STATUS)"
        exit 1
    fi
done

echo "--------------------------------------------------"
echo " DEMO COMPLETED - ALL MATCHES SETTLED"
echo "--------------------------------------------------"
