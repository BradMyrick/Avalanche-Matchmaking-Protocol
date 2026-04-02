# Avalanche Matchmaking Protocol (AMP)

The on-chain version of AWS FlexMatch built on Avalanche.

## Overview

AMP is a decentralized matchmaking protocol built on Avalanche that provides game developers with enterprise-grade matchmaking capabilities similar to AWS FlexMatch, but running on-chain with full transparency, decentralization, and composability.

## Architecture

```
AMP/
├── amp-core/          # Core VM and protocol implementation
├── amp-sdk/          # Multi-language SDKs (Go, Rust, JS)
├── amp-examples/     # Example implementations
├── amp-docs/         # Documentation
└── amp-tools/        # Development and deployment tools
```

## Key Features

- **Smart Matchmaking Rules**: Define complex matchmaking rules on-chain
- **Player Skill Tracking**: Elo-based skill tracking with customizable algorithms  
- **Latency-Aware Matching**: Geographic region optimization
- **Team Balancing**: Automated team composition optimization
- **Tournament Support**: Built-in tournament bracket generation
- **Cross-Chain Compatibility**: Interoperability with other chains via Avalanche Warp Messaging
- **Gas-Efficient Operations**: Optimized for high-frequency match operations

## Getting Started

### Prerequisites
- Go 1.21+
- Rust 1.70+
- Node.js 18+
- AvalancheGo (for local development)

### Quick Start

```bash
# Clone the repository
git clone https://github.com/avalanche-matchmaking-protocol/amp.git
cd amp

# Start local testnet
make localnet-up

# Build and deploy AMP
make build
make deploy-local
```

## Documentation

Full documentation is available at [docs.amp.avalanche.dev](https://docs.amp.avalanche.dev)

## Development

See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

## License

Apache 2.0