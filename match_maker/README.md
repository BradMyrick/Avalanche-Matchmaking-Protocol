# AMP Verifier Service

Rust async verifier service using Cap'n Proto.

## Setup

1. Install Cap'n Proto compiler:
   - macOS: `brew install capnp`
   - Linux: `apt-get install capnproto`

2. Generate Rust code from schema:
   ```bash
   capnp compile -orust ../schemas/match.capnp > src/match_capnp.rs
   ```

3. Run the server:
   ```bash
   cargo run
   ```

## Development

- `src/main.rs`: TCP listener and RPC setup.
- `src/verifier_impl.rs`: Implementation of the `Verifier` interface.

TODO:
- Implement full transcript re-simulation.
- Implement EIP-712 signing of results.
- Add configuration for private keys and network endpoints.
