import requests
import json
import time
import sys

def simulate_python_match(gateway_url, match_id):
    print(f"--- AMP Python Mini-Game Demo ---")
    print(f"Connecting to Gateway: {gateway_url}")
    print(f"Starting Match ID: {match_id}")
    
    # Simulate some "game logic"
    for i in range(5):
        print(f"  [Game] Frame {i*20}: Processing state...")
        time.sleep(0.5)
    
    outcome = 1 # Player A wins
    transcript_hash = "0x" + "0" * 64
    
    payload = {
        "match_id": str(match_id),
        "outcome": outcome,
        "transcript_hash": transcript_hash
    }
    
    print(f"Submitting outcome to AMP Gateway...")
    try:
        response = requests.post(f"{gateway_url}/demo-submit", json=payload)
        if response.status_code == 200:
            print(f"SUCCESS: Match {match_id} submitted via Python SDK simulator")
            print(f"Response: {response.text}")
        else:
            print(f"ERROR: Submission failed with status {response.status_code}")
            sys.exit(1)
    except Exception as e:
        print(f"EXCEPTION: {e}")
        sys.exit(1)

if __name__ == "__main__":
    url = sys.argv[1] if len(sys.argv) > 1 else "http://localhost:50053"
    mid = sys.argv[2] if len(sys.argv) > 2 else "0"
    simulate_python_match(url, mid)
