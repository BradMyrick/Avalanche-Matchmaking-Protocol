# Avalanche Matchmaking Protocol Makefile

.PHONY: help setup build test clean localnet-up localnet-down deploy-local

help:
	@echo "Avalanche Matchmaking Protocol (AMP) Build System"
	@echo ""
	@echo "Targets:"
	@echo "  help        Show this help message"
	@echo "  setup       Install dependencies and setup development environment"
	@echo "  build       Build all components"
	@echo "  test        Run all tests"
	@echo "  clean       Clean build artifacts"
	@echo "  localnet-up Start local Avalanche testnet"
	@echo "  localnet-down Stop local Avalanche testnet"
	@echo "  deploy-local Deploy AMP to local testnet"
	@echo "  lint        Run linters on all code"
	@echo "  format      Format all code"
	@echo "  docs        Generate documentation"

setup: setup-go setup-rust setup-js
	@echo "Development environment setup complete"

setup-go:
	@echo "Setting up Go dependencies..."
	cd amp-sdk/go && go mod download
	cd amp-core && go mod download

setup-rust:
	@echo "Setting up Rust dependencies..."
	cd amp-sdk/rust && cargo fetch

setup-js:
	@echo "Setting up JavaScript dependencies..."
	cd amp-sdk/js && npm ci

build: build-core build-sdk
	@echo "Build complete"

build-core:
	@echo "Building AMP core..."
	cd amp-core && go build -o ../bin/amp-core ./...

build-sdk: build-sdk-go build-sdk-rust build-sdk-js
	@echo "SDKs built"

build-sdk-go:
	@echo "Building Go SDK..."
	cd amp-sdk/go && go build ./...

build-sdk-rust:
	@echo "Building Rust SDK..."
	cd amp-sdk/rust && cargo build --release

build-sdk-js:
	@echo "Building JavaScript SDK..."
	cd amp-sdk/js && npm run build

test: test-core test-sdk
	@echo "All tests passed"

test-core:
	@echo "Testing AMP core..."
	cd amp-core && go test ./...

test-sdk: test-sdk-go test-sdk-rust test-sdk-js
	@echo "SDK tests complete"

test-sdk-go:
	@echo "Testing Go SDK..."
	cd amp-sdk/go && go test ./...

test-sdk-rust:
	@echo "Testing Rust SDK..."
	cd amp-sdk/rust && cargo test

test-sdk-js:
	@echo "Testing JavaScript SDK..."
	cd amp-sdk/js && npm test

lint: lint-go lint-rust lint-js
	@echo "Linting complete"

lint-go:
	@echo "Linting Go code..."
	cd amp-core && golangci-lint run
	cd amp-sdk/go && golangci-lint run

lint-rust:
	@echo "Linting Rust code..."
	cd amp-sdk/rust && cargo clippy

lint-js:
	@echo "Linting JavaScript code..."
	cd amp-sdk/js && npm run lint

format: format-go format-rust format-js
	@echo "Formatting complete"

format-go:
	@echo "Formatting Go code..."
	cd amp-core && gofmt -w .
	cd amp-sdk/go && gofmt -w .

format-rust:
	@echo "Formatting Rust code..."
	cd amp-sdk/rust && cargo fmt

format-js:
	@echo "Formatting JavaScript code..."
	cd amp-sdk/js && npm run format

clean:
	@echo "Cleaning build artifacts..."
	rm -rf bin/
	cd amp-sdk/rust && cargo clean
	cd amp-sdk/js && rm -rf dist/ node_modules/
	@echo "Clean complete"

localnet-up:
	@echo "Starting local Avalanche testnet..."
	docker-compose -f docker/localnet/docker-compose.yml up -d
	@echo "Local testnet running. Use 'make localnet-down' to stop."

localnet-down:
	@echo "Stopping local Avalanche testnet..."
	docker-compose -f docker/localnet/docker-compose.yml down
	@echo "Local testnet stopped."

deploy-local:
	@echo "Deploying AMP to local testnet..."
	@echo "TODO: Implement deployment script"
	@echo "Deployment complete"

docs:
	@echo "Generating documentation..."
	cd amp-docs && npm run docs
	@echo "Documentation generated in amp-docs/dist/"

# Development convenience targets
dev: localnet-up build
	@echo "Development environment ready"

all: setup build test
	@echo "Full build and test complete"