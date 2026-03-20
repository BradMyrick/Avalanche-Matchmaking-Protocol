#!/usr/bin/env bash
set -e

# 1. Setup PATH so we can find 'go', 'capnp', etc.
export PATH="/usr/local/go/bin:/home/kodr/local/bin:/home/kodr/.dotnet:/home/kodr/.dotnet/tools:/home/kodr/tools/cmake-3.28.1-linux-x86_64/bin:$PATH"
export DOTNET_ROOT="/home/kodr/.dotnet"
export GOPATH="$(go env GOPATH)"
export PATH="$PATH:$GOPATH/bin"

# 2. Configuration
REPO_ROOT="/home/kodr/Repos/Avax-Build-Games-2026"
SCHEMA_DIR="$REPO_ROOT/amp-sdk/schemas"
OUTPUT_BASE="$REPO_ROOT/amp-sdk"

# Core schemas to generate bindings for
CORE_SCHEMAS=(
    "$SCHEMA_DIR/match.capnp"
    "$SCHEMA_DIR/game_types.capnp"
    "$SCHEMA_DIR/service.capnp"
    "$SCHEMA_DIR/game_core.capnp"
    "$SCHEMA_DIR/amp_telemetry.capnp"
    "$SCHEMA_DIR/relayer.capnp"
)

echo "=========================================="
echo " Generating AMP SDK Bindings (Core Only)"
echo "=========================================="

# Ensure directories exist
mkdir -p "$OUTPUT_BASE/cpp/generated"
mkdir -p "$OUTPUT_BASE/csharp/generated"
mkdir -p "$OUTPUT_BASE/go/generated"
mkdir -p "$OUTPUT_BASE/python/generated"
mkdir -p "$OUTPUT_BASE/rust/src"

# 1. C++
echo "[1/5] Generating C++ bindings..."
capnp compile -I"$SCHEMA_DIR" --src-prefix="$SCHEMA_DIR" -oc++:"$OUTPUT_BASE/cpp/generated" "${CORE_SCHEMAS[@]}"

# 2. C#
echo "[2/5] Generating C# bindings..."
DOTNET_ROLL_FORWARD=Major capnp compile -I"$SCHEMA_DIR" --src-prefix="$SCHEMA_DIR" -ocsharp:"$OUTPUT_BASE/csharp/generated" "${CORE_SCHEMAS[@]}"

# 3. Go
echo "[3/5] Generating Go bindings..."
capnp compile -I"$SCHEMA_DIR" --src-prefix="$SCHEMA_DIR" -ogo:"$OUTPUT_BASE/go/generated" "${CORE_SCHEMAS[@]}"

# 4. Python
echo "[4/5] Generating Python bindings..."
for schema in "${CORE_SCHEMAS[@]}"; do
    cp "$schema" "$OUTPUT_BASE/python/generated/"
done
echo "Core Python 'bindings' (schemas) copied to $OUTPUT_BASE/python/generated"

# 5. Rust
echo "[5/5] Generating Rust bindings..."
TEMP_RUST_DIR="/tmp/amp-rust-gen"
rm -rf "$TEMP_RUST_DIR"
mkdir -p "$TEMP_RUST_DIR/src"
cat <<CARGOEOF > "$TEMP_RUST_DIR/Cargo.toml"
[package]
name = "amp-rust-gen"
version = "0.1.0"
edition = "2021"
[build-dependencies]
capnpc = "0.19"
[dependencies]
capnp = "0.19"
CARGOEOF
cat <<RUSTEOF > "$TEMP_RUST_DIR/src/main.rs"
fn main() {}
RUSTEOF
cat <<BUILDEOF > "$TEMP_RUST_DIR/build.rs"
fn main() {
    capnpc::CompilerCommand::new()
        .src_prefix("$SCHEMA_DIR")
        .output_path("$OUTPUT_BASE/rust/src")
        .file("$SCHEMA_DIR/match.capnp")
        .file("$SCHEMA_DIR/game_types.capnp")
        .file("$SCHEMA_DIR/service.capnp")
        .file("$SCHEMA_DIR/game_core.capnp")
        .file("$SCHEMA_DIR/amp_telemetry.capnp")
        .file("$SCHEMA_DIR/relayer.capnp")
        .run()
        .expect("capnp compilation failed");
}
BUILDEOF
(cd "$TEMP_RUST_DIR" && cargo build > /dev/null 2>&1)
echo "Rust bindings generated in $OUTPUT_BASE/rust/src"

echo "=========================================="
echo " Generation Complete!"
echo "=========================================="
