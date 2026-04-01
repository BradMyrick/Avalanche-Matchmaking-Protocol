#!/usr/bin/env bash
set -e

echo "=================================================="
echo " [DEMO] WAITING FOR BACKEND SERVICES"
echo "=================================================="
until nc -z amp-server 50051; do
  sleep 2
done
echo "Server is READY."

until curl -s http://amp-demo-gateway:50053/demo-submit -d '{"match_id": "999"}' -X POST > /dev/null; do
  sleep 2
done
echo "Gateway is READY."

# 1. Python Demo
echo ""
echo "--------------------------------------------------"
echo " [MATCH] RUNNING PYTHON SDK FLOW (MATCH ID: 0)"
echo "--------------------------------------------------"
python3 scripts/python_match.py http://amp-demo-gateway:50053 0

# 2. C++ Demo
echo ""
echo "--------------------------------------------------"
echo " [MATCH] RUNNING C++ SDK FLOW (MATCH ID: 1)"
echo "--------------------------------------------------"
cd /app/amp-sdk/examples/cpp/build
./amp_test amp-server:50051

# 3. C# Demo
echo ""
echo "--------------------------------------------------"
echo " [MATCH] RUNNING C# SDK FLOW (MATCH ID: 2)"
echo "--------------------------------------------------"
cd /app/amp-sdk/examples/csharp
dotnet bin/Debug/net8.0/csharp_example.dll

echo ""
echo "=================================================="
echo " ALL SDK FLOWS COMPLETED SUCCESSFULLY"
echo "=================================================="
