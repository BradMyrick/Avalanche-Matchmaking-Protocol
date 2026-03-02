#!/usr/bin/env zsh
source ~/.zshrc 2>/dev/null || true
set -e

echo "=========================================="
echo " Setting up AMP MVP from clean clone"
echo "=========================================="

# 1. Install Foundry dependencies and build contracts
echo "[1/4] Building Smart Contracts..."
cd contracts
forge install OpenZeppelin/openzeppelin-contracts --no-git
forge clean
forge build
cd ..

# 2. Build Rust Services (Matchmaker and Telemetry)
echo "[2/4] Building Rust Services..."
cd amp-server
cargo build
cd ..
cd amp-telemetry
cargo build
cd ..

# 3. Generate Cap'n Proto C# schemas and build SDKs
echo "[3/4] Building C++ and C# SDKs..."

# C++ SDK
cd amp-sdk/cpp_example
mkdir -p build && cd build
cmake ..
make
cd ../../..

# C# Schemas and SDK
cd amp-sdk/schemas
mkdir -p generated/csharp
DOTNET_ROLL_FORWARD=Major capnp compile -I/usr/local/include -ocsharp:generated/csharp *.capnp
cd ../..

cd amp-sdk/csharp_example
dotnet build
cd ../..



echo "=========================================="
echo " Setup complete! You can now run ./test_mvp.sh"
echo "=========================================="
exit 0
