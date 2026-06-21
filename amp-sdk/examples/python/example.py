"""AMP Python SDK end-to-end example.

Requires a running AMP server (default 127.0.0.1:50051). Each player
authenticates with an explicit private key (the SDK no longer silently
generates ephemeral keys — see SECURITY_REVIEW.md S2) and submits a real
EIP-712 outcome signature (S1).

Set the env vars below or pass your own keys. Two distinct anvil/hardhat keys
are used by default so the example runs out-of-the-box against a local server.

    AMP_ADDR=127.0.0.1:50051 AMP_GAME_ID=0 python example.py
"""

import asyncio
import hashlib
import os
import sys

# Ensure amp_sdk is in path for example
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '../../python')))

from amp_sdk.client import AMPClient

# Well-known anvil/hardhat test keys (NEVER use in production).
PLAYER_A_KEY = os.environ.get("AMP_PLAYER_A_KEY", "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80")
PLAYER_B_KEY = os.environ.get("AMP_PLAYER_B_KEY", "0x59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d")

GAME_ID = int(os.environ.get("AMP_GAME_ID", "0"))


async def run_opponent(server_url: str) -> None:
    """Player B: authenticate and queue so Player A gets matched."""
    await asyncio.sleep(0.5)
    try:
        client = AMPClient(server_url)
        await client.connect()
        await client.authenticate(game_id=GAME_ID, private_key_hex=PLAYER_B_KEY)
        print("[Player B] Connected & authenticated. Requesting match...")
        await client.request_match(str(GAME_ID), str(GAME_ID))
    except Exception as e:
        print(f"[Player B] Error: {e}")


async def main() -> None:
    server_url = os.environ.get("AMP_ADDR", "127.0.0.1:50051")
    print(f"Starting AMP Python SDK example on {server_url}...")

    client = AMPClient(server_url)
    try:
        await client.connect()
        await client.authenticate(game_id=GAME_ID, private_key_hex=PLAYER_A_KEY)
        print("[Player A] Connected & authenticated to the AMP matchmaker.")
    except Exception as e:
        print(f"Failed to connect/authenticate: {e}")
        sys.exit(1)

    # Spawn Player B so matchmaking finds a pair.
    print("Spawning Player B task...")
    asyncio.create_task(run_opponent(server_url))

    print(f"Requesting match in game {GAME_ID}...")
    assignment = await client.request_match(str(GAME_ID), str(GAME_ID))
    match_id = assignment["match_id"]
    print(f"Matched! Match ID: {match_id}")

    print("Simulating game...")
    await asyncio.sleep(0.5)

    # Outcome = 1 (WIN_A). transcript_hash MUST be exactly 32 bytes.
    # The server treats transcript_hash as an opaque 32-byte commitment
    # (it's stored verbatim on-chain). Choice of hash function is up to the
    # game — sha256, keccak256, or any 32-byte commitment to the transcript.
    outcome = 1
    transcript_hash = hashlib.sha256(match_id.encode("utf-8")).digest()
    print("Submitting outcome (Player A wins) to verifier...")
    signature = await client.submit_outcome(match_id, outcome, transcript_hash)
    print(f"Verifier countersignature: 0x{signature.hex()}")
    print("Python SDK example complete.")


if __name__ == "__main__":
    asyncio.run(main())
