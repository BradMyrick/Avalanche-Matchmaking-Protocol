#!/usr/bin/env bash
# Phase 2.6/2.7 — reproducible local load + latency run.
# Stands up anvil + contracts + telemetry + relayer + server, runs the loadtest,
# reports connect/login/match/total percentiles + settlement counts, tears down.
#
# Requires: cargo build --release (AMP-Server, amp-relayer, amp-telemetry, amp-loadtest),
# foundry (anvil + forge), and the capnp C library.
set -u
cd "$(dirname "$0")/.."
CLIENTS="${1:-200}"
DURATION="${2:-8}"
export AMP_ALLOW_UNAUTHENTICATED_RELAYER=1 RUST_LOG=error

# Deterministic CREATE addresses for anvil account 0 (nonce 0 / 1).
REG=0x5FbDB2315678afecb367f032d93F642f64180aa3
SET=0xe7f1725E7734CE288F8367e1Bb143E90bb3F0512
PIDS=()
cleanup() { for p in "${PIDS[@]}"; do kill -9 "$p" 2>/dev/null; done; }
trap cleanup EXIT INT TERM
rm -rf /tmp/lt-server /tmp/lt-relayer /tmp/lt-telemetry.bin

setsid anvil --chain-id 31337 --port 8545 --silent >/dev/null 2>&1 < /dev/null &
PIDS+=($!)
sleep 2
(cd contracts && forge script script/Deploy.s.sol --rpc-url http://127.0.0.1:8545 \
  --private-key 0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 \
  --broadcast >/dev/null 2>&1)

setsid ./target/release/amp-telemetry 127.0.0.1:4317 /tmp/lt-telemetry.bin >/dev/null 2>&1 < /dev/null &
PIDS+=($!)
RELAYER_PRIVATE_KEY=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 \
  FUJI_RPC_URL=http://127.0.0.1:8545 CONTRACT_REGISTRY=$REG CONTRACT_SETTLEMENT=$SET \
  RELAYER_DB_PATH=/tmp/lt-relayer setsid ./target/release/amp-relayer >/dev/null 2>&1 < /dev/null &
PIDS+=($!)
VERIFIER_KEY=0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80 \
  AMP_SETTLEMENT_ADDRESS=$SET AMP_ADDR=127.0.0.1:50051 AMP_WORKERS=1 \
  AMP_MAX_CONCURRENT=2000 AMP_MAX_PER_IP_PER_MIN=100000 \
  RELAYER_RPC_ADDR=127.0.0.1:50052 TELEMETRY_ADDR=127.0.0.1:4317 \
  AMP_DB_PATH=/tmp/lt-server AMP_CHAIN_ID=31337 \
  setsid ./target/release/AMP-Server >/dev/null 2>&1 < /dev/null &
PIDS+=($!)
sleep 3

echo "===== LOADTEST ($CLIENTS clients, ${DURATION}s) ====="
AMP_CHAIN_ID=31337 AMP_SETTLEMENT_ADDRESS=$SET ./target/release/amp-loadtest \
  --addr 127.0.0.1:50051 --clients "$CLIENTS" --duration "$DURATION" 2>&1 \
  | grep -iE "logins|matches|outcome|errors|connect:|login:|match:|total:|p50|p95|p99|max"
echo "===== DONE ====="
