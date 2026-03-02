# AMP C# .NET SDK Example

This example demonstrates how to connect a C# .NET AMP client to a local **AMP Matchmaker Service**, request a match, simulate a game, and submit a result to a verifier for settlement.
It serves as an end-to-end validation of the AMP SDK's matchmaking and verifier interaction flow from C#.

***

## Overview

The test connects a simulated player to the matchmaker instance at `127.0.0.1:50051`.
Once connected, the player requests a match in game `"0x6767676767676767"`. The AMP service pairs them and returns a canonical `MatchAssignment`.

The client then emits telemetry and game events using the `MatchSession` capability, before sending a fake "win" result to a verifier, which responds with a signed attestation.

***

## Build & Run

### Prerequisites
- .NET 8.0 SDK
- The Capnp.Net.Tool command line tool (installed via `dotnet tool install -g capnpc-csharp`)

### Build

Before building the C# project, you must generate the C# Cap'n Proto schemas. Depending on your installation, the `capnpc-csharp` tool may require an older .NET runtime target, so we use `DOTNET_ROLL_FORWARD` to allow it to run on .NET 8+.

```bash
# Generate the C# code from the schemas
cd ../schemas
mkdir -p generated/csharp
DOTNET_ROLL_FORWARD=Major capnp compile -I/usr/local/include -ocsharp:generated/csharp *.capnp

# Build the C# project
cd ../csharp_example
dotnet build
```

### Run

```bash
dotnet run
```
