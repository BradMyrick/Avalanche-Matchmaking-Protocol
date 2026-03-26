#!/usr/bin/env bash
set -e

echo "Waiting for Gateway (http://amp-demo-gateway:50053) to be ready..."
until curl -s http://amp-demo-gateway:50053/demo-submit -d '{"match_id": "999"}' -X POST > /dev/null; do
  sleep 5
done
echo "Gateway is READY."

# 1. Python Demo
echo "[1/3] Running Python SDK Example..."
python3 scripts/python_match.py http://amp-demo-gateway:50053 0

# 2. C++ Demo
echo "[2/3] Building & Running C++ SDK Example (Cleaning build artifacts)..."
rm -rf /app/amp-sdk/examples/cpp/build/*
cd /app/amp-sdk/examples/cpp
mkdir -p build && cd build
cmake .. && make -j
./amp_test amp-server:50051

# 3. C# Demo
echo "[3/3] Building & Running C# SDK Example..."
export AMP_ADDR=amp-server:50051
cd /app/amp-sdk/examples/csharp
dotnet run

echo "All SDK examples completed successfully inside Docker."
