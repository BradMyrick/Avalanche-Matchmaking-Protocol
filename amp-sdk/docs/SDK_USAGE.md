# AMP SDK Usage Guide

Welcome to the AMP SDK! This repository contains everything you need to integrate your game engine (C++ or C#) with the AMP capability-based matchmaker and verifier network.

## Core Concepts

AMP utilizes **Cap'n Proto RPC** to provide an extremely fast, capability-based security model. 
Instead of relying on global tokens or shared secrets embedded in your game client, your game requests specific **Capabilities** (like a `MatchSession`) from the matchmaker. 

This model ensures that untrusted or malicious clients cannot spoof telemetry or game events because they do not physically possess the required capability handle to invoke those RPC methods. 

## Champion Flows

### 1. Match Create & Join
The entry point to the AMP network is the `GameSessionService`.

- **Login**: Provide a cryptographic signature proving your identity. The service returns a `UserSession` capability.
- **Request Match**: Using your `UserSession`, you can call `requestMatch(gameId)`. The RPC call will return an authoritative `MatchAssignment` and a direct `MatchSession` capability specific to that match.

### 2. In-Game Telemetry & Events
Once the match starts, your game engine should utilize the `MatchSession` capability to emit verified events.

- **Game Events**: Call `emitGameEvent` for critical state changes (e.g., scoring, item pickup).
- **Telemetry**: Call `emitTelemetry` using the `AmpTelemetryEvent` schema to denote match lifecycle events like `matchJoined`, `settlementSubmitted`, etc. The capability-based design guarantees that these events are tied only to your specific assigned match.

### 3. Commit & Settle
When the match concludes, the verifier network must confirm the result.

- **Submit**: Using the `MatchSession` capability, call `submitOutcome()`. 
- **Verify**: Include the `MatchTranscript` hash if playing a turn-based or deterministic rollback game. The AMP node will respond with a signed `VerifierResult`.
- **On-Chain Settlement**: This signature can then be submitted directly to the settlement contracts on the Avalanche L1 or kept offline as a receipt.

## Examples Provided
- **Native C++ (`cpp_example`)**: Demonstrates low-level Cap'n Proto integration, managing `kj::Promise` and dynamic capabilities.
- **C# / Unity (`csharp_example`)**: Provides a drop-in reference for Unity developers leveraging the `Capnp.Net` library. 

Both examples compile cleanly and run a simulate full loop (Match Create -> Simulate -> Submit Outcome).
