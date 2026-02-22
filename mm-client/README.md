# AMP Matchmaker Client (CLI)

## Overview
This is a lightweight Rust-based CLI client used for debugging and load-testing the AMP Matchmaker service. 

## Features
- Connects to the Matchmaker WebSocket.
- Simulates player matchmaking requests.
- Useful for validating the protocol without a browser environment.

## Usage
1. Build the client:
   ```bash
   cargo build
   ```
2. Run the client (points to default local matchmaker):
   ```bash
   cargo run -- 127.0.0.1:50051
   ```
