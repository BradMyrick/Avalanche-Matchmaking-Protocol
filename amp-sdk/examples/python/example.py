import asyncio
import os
import sys

# Ensure amp_sdk is in path for example
sys.path.insert(0, os.path.abspath(os.path.join(os.path.dirname(__file__), '../../python')))

from amp_sdk.client import AMPClient

GAME_ID = 0x6767676767676767

async def run_opponent(server_url):
    await asyncio.sleep(0.5)
    try:
        client = AMPClient(server_url)
        await client.connect()
        # Automatically generate custodial wallet and sign
        await client.authenticate(game_id=0)
        print("[Player B] Connected and authenticated. Requesting match...")
        await client.request_match(str(GAME_ID))
    except Exception as e:
        print(f"[Player B] Error: {e}")

async def main():
    server_url = os.environ.get("AMP_ADDR", "127.0.0.1:50051")
    print(f"Starting AMP Python SDK Test on {server_url}...")

    client = AMPClient(server_url)
    try:
        await client.connect()
        # Automatically generate custodial wallet
        await client.authenticate(game_id=0)
        print("[Player A] Connected & Logged in to AMP matchmaker.")
    except Exception as e:
        print(f"Failed to connect to matchmaker: {e}")
        sys.exit(1)

    print("Spawning Player B task...")
    asyncio.create_task(run_opponent(server_url))

    print(f"Requesting match in game {hex(GAME_ID)}...")
    assignment = await client.request_match(str(GAME_ID))
    match_id = assignment["match_id"]
    print(f"Got MatchAssignment! Match ID: {match_id}")

    print("Simulating game...")
    await asyncio.sleep(0.5)

    print("Submitting outcome to verifier...")
    signature = await client.submit_outcome(match_id, 0, b'')
    print(f"Verifier provided signature: 0x{signature.hex()}")
    print("Python SDK test successful. Exiting cleanly.")

if __name__ == "__main__":
    asyncio.run(main())
