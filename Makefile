# Avalanche Matchmaking Protocol (AMP) Makefile
# Build system for the AMP Rust workspace + Solidity contracts + multi-language SDKs

.PHONY: help setup build test clean localnet-up localnet-down deploy-local \
        lint format docs build-rust build-contracts build-sdk-go build-sdk-cpp \
        build-sdk-csharp build-sdk-python test-rust test-contracts test-integration \
        test-sdk-go test-sdk-python test-sdk-js \
        lint-rust lint-contracts format-rust check-contracts

help:
	@echo "Avalanche Matchmaking Protocol (AMP) Build System"
	@echo ""
	@echo "Targets:"
	@echo "  help           Show this help message"
	@echo "  setup          Install dependencies and set up development environment"
	@echo "  build          Build all Rust components + contracts"
	@echo "  test           Run all tests (Rust + Forge + SDKs)"
	@echo "  clean          Clean all build artifacts"
	@echo "  localnet-up    Start local Avalanche testnet (Docker)"
	@echo "  localnet-down  Stop local Avalanche testnet"
	@echo "  deploy-local   Deploy AMP contracts to local testnet"
	@echo "  lint           Run linters on all code"
	@echo "  format         Format all code"
	@echo "  check-contracts  Check Solidity formatting (forge fmt --check)"
	@echo "  docs           Generate documentation"

# ─── Setup ───────────────────────────────────────────────────────────────

setup: setup-rust setup-contracts setup-sdk-go setup-sdk-python
	@echo "Development environment setup complete"

setup-rust:
	@echo "Fetching Rust dependencies..."
	cargo fetch

setup-contracts:
	@echo "Installing Foundry libraries (pinned: OZ v5.6.1, forge-std v1.10.0)..."
	cd contracts && forge install OpenZeppelin/openzeppelin-contracts@v5.6.1 --no-git
	cd contracts && forge install foundry-rs/forge-std@v1.10.0 --no-git

setup-sdk-go:
	@echo "Downloading Go SDK dependencies..."
	cd amp-sdk/go && go mod download

setup-sdk-python:
	@echo "Installing Python SDK dependencies..."
	cd amp-sdk/python && pip install -e . 2>/dev/null || echo "Python SDK deps skipped (pip not configured)"

# ─── Build ───────────────────────────────────────────────────────────────

build: build-contracts build-rust
	@echo "Build complete"

build-contracts:
	@echo "Building Solidity contracts..."
	cd contracts && forge build

build-rust:
	@echo "Building Rust workspace..."
	cargo build --release

# ─── Test ────────────────────────────────────────────────────────────────

test: test-rust test-contracts test-sdk-go test-sdk-python test-sdk-js test-integration
	@echo "All tests passed"

test-rust:
	@echo "Running Rust workspace tests..."
	cargo test --workspace

test-contracts:
	@echo "Running Forge contract tests..."
	cd contracts && forge test -vvv

test-integration: build
	@echo "Running end-to-end integration tests..."
	cargo run --bin amp-integration-tests

test-sdk-go:
	@echo "Running Go SDK tests..."
	cd amp-sdk/go && go test ./...

test-sdk-python:
	@echo "Running Python SDK tests..."
	@cd amp-sdk/python && pip install -e ".[dev]" 2>/dev/null || echo "  (pip install skipped — ensure pycapnp, eth-account, eth-keys, eth-utils, pytest are installed in your environment)"
	@cd amp-sdk/python && pytest || (echo "  Python tests failed. If dependencies are missing, run: pip install -e amp-sdk/python[dev]" && false)

test-sdk-js:
	@echo "Running JavaScript SDK tests..."
	@cd amp-sdk/js && npm install --silent 2>/dev/null || echo "  (npm install skipped — ensure ethers is installed)"
	@cd amp-sdk/js && npm run build && npm test

# ─── Lint & Format ───────────────────────────────────────────────────────

lint: lint-rust lint-contracts
	@echo "Linting complete"

lint-rust:
	@echo "Running Clippy on Rust workspace..."
	cargo clippy --workspace --all-targets -- -D warnings

lint-contracts:
	@echo "Linting Solidity contracts..."
	cd contracts && forge fmt --check

format: format-rust format-contracts
	@echo "Formatting complete"

format-rust:
	@echo "Formatting Rust code..."
	cargo fmt --all

format-contracts:
	@echo "Formatting Solidity code..."
	cd contracts && forge fmt

# ─── Docker & Localnet ──────────────────────────────────────────────────

localnet-up:
	@echo "Starting local Avalanche testnet..."
	docker compose -f docker/localnet/docker-compose.yml up -d
	@echo "Local testnet running. Use 'make localnet-down' to stop."

localnet-down:
	@echo "Stopping local Avalanche testnet..."
	docker compose -f docker/localnet/docker-compose.yml down
	@echo "Local testnet stopped."

deploy-local:
	@echo "Deploying AMP contracts to local testnet..."
	cd contracts && ([ -f ../.env ] && . ../.env || true) && forge script script/Deploy.s.sol --rpc-url "$$ANVIL_RPC_URL" --private-key "$$ANVIL_DEPLOYER_PRIVATE_KEY" --broadcast
	@echo "Deployment complete"

# ─── Clean ───────────────────────────────────────────────────────────────

clean:
	@echo "Cleaning build artifacts..."
	cargo clean
	cd contracts && forge clean
	@echo "Clean complete"

# ─── Docs ───────────────────────────────────────────────────��────────────

docs:
	@echo "Documentation available in docs/"

# ─── Convenience ─────────────────────────────────────────────────────────

dev: localnet-up build
	@echo "Development environment ready"

all: setup build test
	@echo "Full build and test complete"
