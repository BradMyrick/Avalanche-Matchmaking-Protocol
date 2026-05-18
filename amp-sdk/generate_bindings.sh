#!/usr/bin/env bash
set -e

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SCHEMA_DIR="$REPO_ROOT/amp-sdk/schemas"
OUTPUT_BASE="$REPO_ROOT/amp-sdk"

CORE_SCHEMAS=(
    "$SCHEMA_DIR/match.capnp"
    "$SCHEMA_DIR/game_types.capnp"
    "$SCHEMA_DIR/service.capnp"
    "$SCHEMA_DIR/game_core.capnp"
    "$SCHEMA_DIR/amp_telemetry.capnp"
    "$SCHEMA_DIR/relayer.capnp"
    "$SCHEMA_DIR/player_profile.capnp"
    "$SCHEMA_DIR/matchmaking_rules.capnp"
    "$SCHEMA_DIR/game_registry.capnp"
    "$SCHEMA_DIR/inventory.capnp"
    "$SCHEMA_DIR/tournament.capnp"
    "$SCHEMA_DIR/security.capnp"
)

echo "=========================================="
echo " Generating AMP SDK Bindings"
echo "=========================================="

mkdir -p "$OUTPUT_BASE/cpp/generated"
mkdir -p "$OUTPUT_BASE/csharp/generated"
mkdir -p "$OUTPUT_BASE/go/generated"
mkdir -p "$OUTPUT_BASE/python/generated"
mkdir -p "$OUTPUT_BASE/rust/src"

echo "[1/5] Generating C++ bindings..."
capnp compile -I"$SCHEMA_DIR" --src-prefix="$SCHEMA_DIR" -oc++:"$OUTPUT_BASE/cpp/generated" "${CORE_SCHEMAS[@]}" || echo "  C++ generation skipped (capnp not found or c++ plugin missing)"

echo "[2/5] Generating C# bindings..."
capnp compile -I"$SCHEMA_DIR" --src-prefix="$SCHEMA_DIR" -ocsharp:"$OUTPUT_BASE/csharp/generated" "${CORE_SCHEMAS[@]}" || echo "  C# generation skipped (capnpc-csharp not found)"

echo "[3/5] Generating Go bindings..."
capnp compile -I"$SCHEMA_DIR" --src-prefix="$SCHEMA_DIR" -ogo:"$OUTPUT_BASE/go/generated" "${CORE_SCHEMAS[@]}" || echo "  Go generation skipped (capnpc-go not found)"

echo "[4/5] Copying schemas for Python..."
for schema in "${CORE_SCHEMAS[@]}"; do
    cp "$schema" "$OUTPUT_BASE/python/generated/"
done

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
        .file("$SCHEMA_DIR/player_profile.capnp")
        .file("$SCHEMA_DIR/matchmaking_rules.capnp")
        .file("$SCHEMA_DIR/game_registry.capnp")
        .file("$SCHEMA_DIR/inventory.capnp")
        .file("$SCHEMA_DIR/tournament.capnp")
        .file("$SCHEMA_DIR/security.capnp")
        .run()
        .expect("capnp compilation failed");
}
BUILDEOF
(cd "$TEMP_RUST_DIR" && cargo build > /dev/null 2>&1) || echo "  Rust generation skipped (cargo/capnpc not available)"
echo "Rust bindings generated in $OUTPUT_BASE/rust/src"

echo "=========================================="
echo " Generation Complete!"
echo "=========================================="
