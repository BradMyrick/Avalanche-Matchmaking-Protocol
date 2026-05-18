#!/usr/bin/env bash
set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
ENV_FILE="$REPO_ROOT/.env"

echo "=========================================="
echo " AMP Localnet Quickstart"
echo "=========================================="

if [ ! -f "$ENV_FILE" ]; then
    echo "Creating .env from .env.example..."
    cp "$REPO_ROOT/.env.example" "$ENV_FILE"
    echo "Created .env - edit it with your keys before deploying to testnet."
fi

source "$ENV_FILE" 2>/dev/null || true

echo "[1/4] Starting Anvil (local Avalanche)..."
if command -v anvil &>/dev/null; then
    anvil --host 0.0.0.0 --port 8545 --chain-id 43113 &
    ANVIL_PID=$!
    echo "Anvil started (PID: $ANVIL_PID)"
    sleep 2
else
    echo "Anvil not found. Install with: curl -L https://foundry.paradigm.xyz | bash && foundryup"
    echo "Falling back to Fuji RPC if configured..."
fi

echo "[2/4] Deploying contracts..."
if [ -d "$REPO_ROOT/contracts" ] && command -v forge &>/dev/null; then
    cd "$REPO_ROOT/contracts"
    forge build 2>/dev/null || true
    if [ -n "$PRIVATE_KEY" ]; then
        echo "Contracts built. Deploy with: cd contracts && forge script script/Deploy.s.sol --rpc-url http://localhost:8545 --broadcast --private-key \$PRIVATE_KEY"
    else
        echo "Set PRIVATE_KEY in .env to deploy contracts."
    fi
else
    echo "Forge not found or contracts directory missing. Skipping contract deployment."
fi

echo "[3/4] Building Rust services..."
cd "$REPO_ROOT"
cargo build --release -p AMP-Server -p amp-relayer -p amp-telemetry 2>/dev/null
echo "Rust services built."

echo "[4/4] Starting services..."
export AMP_ADDR="0.0.0.0:50051"
export RELAYER_RPC_ADDR="127.0.0.1:50052"
export TELEMETRY_ADDR="127.0.0.1:4317"
export AMP_DB_PATH="/tmp/amp-data"
export RELAYER_DB_PATH="/tmp/amp-relayer-data"
export RELAYER_PRIVATE_KEY="${RELAYER_PRIVATE_KEY:-ac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80}"
export FUJI_RPC_URL="${FUJI_RPC_URL:-http://localhost:8545}"
export CONTRACT_REGISTRY="${CONTRACT_REGISTRY:-0x0000000000000000000000000000000000000000}"
export CONTRACT_SETTLEMENT="${CONTRACT_SETTLEMENT:-0x0000000000000000000000000000000000000000}"
export VERIFIER_KEY="${VERIFIER_KEY:-ac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80}"

mkdir -p /tmp/amp-data /tmp/amp-relayer-data

echo ""
echo "Starting amp-telemetry on $TELEMETRY_ADDR..."
"$REPO_ROOT/target/release/amp-telemetry" "$TELEMETRY_ADDR" /tmp/telemetry.bin &
TEL_PID=$!

sleep 1

echo "Starting amp-relayer on $RELAYER_RPC_ADDR..."
"$REPO_ROOT/target/release/amp-relayer" &
RELAY_PID=$!

sleep 1

echo "Starting amp-server on $AMP_ADDR..."
"$REPO_ROOT/target/release/AMP-Server" &
SERVER_PID=$!

sleep 2

echo ""
echo "=========================================="
echo " AMP Stack Running!"
echo "=========================================="
echo " amp-server:    $AMP_ADDR (PID: $SERVER_PID)"
echo " amp-relayer:   $RELAYER_RPC_ADDR (PID: $RELAY_PID)"
echo " amp-telemetry: $TELEMETRY_ADDR (PID: $TEL_PID)"
echo ""
echo " Press Ctrl+C to stop all services."
echo "=========================================="

cleanup() {
    echo ""
    echo "Stopping services..."
    kill $SERVER_PID $RELAY_PID $TEL_PID ${ANVIL_PID:-} 2>/dev/null || true
    wait 2>/dev/null
    echo "All services stopped."
}
trap cleanup EXIT INT TERM

wait
