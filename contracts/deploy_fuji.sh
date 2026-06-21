#!/bin/bash

echo "Deploying to Avalanche Fuji Testnet..."

if [ ! -f .env ]; then
  echo "Error: .env file not found. Please create one with PRIVATE_KEY and AVALANCHE_RPC_URL."
  exit 1
fi

source .env

if [ -z "$PRIVATE_KEY" ]; then
  echo "Error: PRIVATE_KEY is not set in .env"
  exit 1
fi

forge script script/Deploy.s.sol:DeployScript \
  --rpc-url fuji \
  --broadcast \
  --verify \
  --delay 10 \
  --retries 5 \
  -vvvv

echo "Deployment complete."
