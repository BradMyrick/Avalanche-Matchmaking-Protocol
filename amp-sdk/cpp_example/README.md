# AMP Native Engine SDK Test (C++)

This example demonstrates how to connect two native C++ AMP clients to a local **AMP Matchmaker Service**, request a match, simulate a game, and submit a result to a verifier for settlement.  
It serves as an end‑to‑end validation of the AMP SDK’s matchmaking and verifier interaction flow.

***

## Overview

The test spawns two simulated players — **Player A** and **Player B** — that connect to the same matchmaker instance at `127.0.0.1:50051`.  
Once connected, both players request a match in game `"0"`. The AMP service pairs them and returns a canonical `MatchAssignment`.

Player A then sends a fake “win” result to a verifier, which responds with a signed attestation. The signature is printed in hexadecimal form.

This exercise verifies that:

- The native AMP client can connect and authenticate with the matchmaker  
- Match requests are successfully queued and paired  
- Match assignments return valid metadata  
- Verifier signing works end‑to‑end

***

## Build & Run

### Prerequisites
- C++17 or later  
- `cmake` (≥3.15 recommended)  
- AMP matchmaker and verifier running locally on port **50051**  
- `AmpClient.hpp` and linked library from the AMP SDK (built from the SDK root)

### Build

```bash
mkdir build && cd build
cmake ..
make
```

### Run

```bash
./amp_cpp_test
```

### Expected Output
You should see logs like:

```
Starting AMP C++ Native Engine SDK Test...
Connected & Logged in to AMP matchmaker.
Spawning Player B thread...
Requesting match in game '0'...
Got MatchAssignment! Match ID: abcd1234
Simulating game...
Submitting outcome to verifier...
Verifier provided signature: 0xdeadbeef...
C++ Native Client MVP successful. Exiting cleanly.
```

***

## File Structure

```
/examples/
  └── cpp/
      ├── AmpClient.hpp      # AMP SDK C++ client interface
      ├── AmpClient.cpp      # Implementation or linked lib
      └── test_main.cpp      # This example integration test
```

***

## Code Summary
Key components in `test_main.cpp`:
- **`AmpClient`** – AMP SDK object that manages 
  - network connection,
  - match requests, and
  - result submission.
- **`runOpponent()`** – Separate thread simulating the second player.
- **`printHex()`** – Helper utility to display verifier signatures clearly.

No gameplay logic is implemented here – it’s purely a functional connectivity test for AMP’s core matchmaking and settlement pipeline.

***

## Next Steps
1. Replace the dummy connect signatures with wallet‑based authentication (e.g., `ecrecover`‑compatible signing).  
2. Implement real **match transcripts** using Cap’n Proto serialization to test `ASYNC_VERIFIER` mode fully.  
3. Integrate with your engine’s event system — Unreal, Unity, or a custom engine — to build real gameplay‑to‑settlement loops.

