# AMP SDKs

The AMP (Avalanche Matchmaking & Protocol) SDKs provide clients for various game engines and development environments to connect to the AMP Local Matchmaker Service, join matchmaking, and submit verified game outcomes.

## Repository Setup

To compile and use the SDKs, you should setup the repository from a fresh clone.
From the root of `Avax-Build-Games-2026`, simply run:

```bash
./mvp_setup.sh
```

This will automatically:
1. Fetch foundry packages and compile the Avalanche Smart Contracts.
2. Build the AMP Matchmaker Rust Server.
3. Generate the required Cap'n Proto C# schema interfaces automatically.
4. Compile the C++ native SDK client and example executable.
5. Build the .NET 8.0 C# SDK solution.

## SDKs Provided
- [Native C++ (Unreal, Custom Engines)](cpp_example/README.md)
- [C# .NET (Unity, Godot)](csharp_example/README.md)

## E2E Testing

Once the environment is setup using `./mvp_setup.sh`, you can execute a full local end-to-end test.

From the repository root, run:
```bash
./test_mvp.sh
```

This ensures components communicate properly from end to end.
