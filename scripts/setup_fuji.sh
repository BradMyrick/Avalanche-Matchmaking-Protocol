#!/usr/bin/env bash
set -e

# Load environment variables
if [ -f .env ]; then
  export $(grep -v '^#' .env | xargs)
fi

echo "--------------------------------------------------"
echo " [SETUP] PREPARING FUJI TESTNET STATE"
echo "--------------------------------------------------"

# 1. Register Game ID 0 if not exists
echo "Checking Game ID 0..."
GAME_EXISTS=$(cast call $AMP_REGISTRY_ADDRESS "nextGameId()" --rpc-url $FUJI_RPC_URL)
if [ "$GAME_EXISTS" == "0x0000000000000000000000000000000000000000000000000000000000000000" ]; then
    echo "Registering Game ID 0..."
    # registerGame(uint8 mode, address[] verifiers, uint256 minStake, address stakeToken, address arbiter)
    # Mode 0 = ASYNC_VERIFIER
    cast send $AMP_REGISTRY_ADDRESS "registerGame(uint8,address[],uint256,address,address)" \
        0 "[$AMP_VERIFIER_ADDRESS]" 0 "0x0000000000000000000000000000000000000000" "$AMP_VERIFIER_ADDRESS" \
        --rpc-url $FUJI_RPC_URL --private-key $DEPLOYER_PRIVATE_KEY
    echo "Game 0 Registered."
else
    echo "Game ID 0 already exists or nextGameId > 0."
fi

# 2. Add Verifier if not already added (redundant if registered with it, but safe)
echo "Ensuring verifier $AMP_VERIFIER_ADDRESS is authorized..."
cast send $AMP_REGISTRY_ADDRESS "updateGameVerifiers(uint256,address[])" 0 "[$AMP_VERIFIER_ADDRESS]" \
    --rpc-url $FUJI_RPC_URL --private-key $DEPLOYER_PRIVATE_KEY

# 3. Create 3 Matches
MATCH_COUNT=$(cast call $AMP_REGISTRY_ADDRESS "nextMatchId()" --rpc-url $FUJI_RPC_URL)
MATCH_COUNT_INT=$((MATCH_COUNT))

if [ $MATCH_COUNT_INT -lt 3 ]; then
    echo "Creating missing matches (Target: 3, Current: $MATCH_COUNT_INT)..."
    for i in $(seq $MATCH_COUNT_INT 2); do
        echo "Creating Match $i..."
        cast send $AMP_REGISTRY_ADDRESS "createMatch(uint256,uint256)" 0 0 \
            --rpc-url $FUJI_RPC_URL --private-key $DEPLOYER_PRIVATE_KEY
        
        echo "Joining Match $i as Player B..."
        # Using the same key for Player B for the demo simplicity
        cast send $AMP_REGISTRY_ADDRESS "joinMatch(uint256)" $i \
            --rpc-url $FUJI_RPC_URL --private-key $DEPLOYER_PRIVATE_KEY
    done
else
    echo "At least 3 matches already exist."
fi

echo "--------------------------------------------------"
echo " [SETUP] FUJI STATE READY"
echo "--------------------------------------------------"
