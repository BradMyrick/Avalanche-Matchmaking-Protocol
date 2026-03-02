#!/usr/bin/env bash
# dependencies.sh - Verifies prerequisites for the AMP SDK MVP setup

echo "=========================================="
echo " AMP MVP Dependency Check"
echo "=========================================="
echo ""

verify_command() {
    local cmd_name=$1
    local human_name=$2
    shift 2
    local version_cmd=("$@")
    
    printf "%-25s" "Checking $human_name..."
    
    if command -v "$cmd_name" >/dev/null 2>&1; then
        echo "✅ Installed"
        # Print version if a command was provided
        if [ ${#version_cmd[@]} -gt 0 ]; then
            local version_out
            version_out=$("${version_cmd[@]}" 2>&1 | head -n 1)
            echo "  -> Found: $version_out"
        fi
        return 0
    else
        echo "❌ Missing"
        return 1
    fi
}

ALL_PASSED=1

# System Tools
verify_command "cmake" "CMake" cmake --version || ALL_PASSED=0
verify_command "make" "Make" make --version || ALL_PASSED=0

# Core Languages/Runtimes
verify_command "rustc" "Rust (rustc)" rustc --version || ALL_PASSED=0
verify_command "cargo" "Cargo" cargo --version || ALL_PASSED=0
verify_command "dotnet" ".NET SDK (8.0+)" dotnet --version || ALL_PASSED=0


# Web3/Crypto Tools
verify_command "forge" "Foundry (forge)" forge --version || ALL_PASSED=0
verify_command "anvil" "Foundry (anvil)" anvil --version || ALL_PASSED=0

# Cap'n Proto
verify_command "capnp" "Cap'n Proto (compiler)" capnp --version || ALL_PASSED=0

# Check Capnp C# specifically
printf "%-25s" "Checking capnpc-csharp..."
if command -v capnpc-csharp >/dev/null 2>&1; then
    echo "✅ Installed"
else
    echo "❌ Missing"
    echo "  -> Recommendation: Run 'dotnet tool install -g capnpc-csharp'"
    ALL_PASSED=0
fi

echo ""
echo "=========================================="
if [ $ALL_PASSED -eq 1 ]; then
    echo "✅ All dependencies are met! You can proceed with ./mvp_setup.sh"
    exit 0
else
    echo "❌ Some dependencies are missing. Please install them before proceeding."
    exit 1
fi
