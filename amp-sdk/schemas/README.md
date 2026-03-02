# AMP Protocol - Schemas

## Overview
This directory contains the [Cap'n Proto](https://capnproto.org/) definitions for the AMP protocol. These schemas serve as the canonical source of truth for the data structures and RPC interfaces used across the network.

## Files
- **`match.capnp`**: Defines core match types (ID, players, stakes, state).
- **`game_types.capnp`**: Defines various settlement styles (Turn-based, Real-time, Oracle).
- **`service.capnp`**: Defines the RPC interfaces for matchmaking and session management.

## Current Status
The Rust implementations and SDK interfaces utilize these natively. Note that while earlier MVPs used JSON compatibility workarounds, all traffic is now strictly Cap'n Proto over the wire.

For Milestone 2, we plan to fully integrate `capnp-ts` once the TypeScript generator issues are resolved for the required RPC interfaces.
