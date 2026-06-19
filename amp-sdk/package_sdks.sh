#!/usr/bin/env bash
# Package all AMP SDKs for distribution. Run from the repository root:
#   ./amp-sdk/package_sdks.sh
#
# Produces per-language archives in dist/:
#   amp_cpp_sdk.tar.gz         — C++ headers + source + Unreal plugin + schemas
#   amp_csharp_sdk.zip         — C# (.NET / Unity) sources + schemas
#   amp_go_sdk.tar.gz          — Go module sources + schemas
#   amp_rust_sdk.tar.gz        — Rust crate sources + schemas
#   amp_python_sdk.tar.gz      — Python package + schemas
#   amp_js_sdk.tar.gz          — JS/TS sources (built dist/ included)
#
# Build artifacts (bin/, obj/, node_modules/, *.pyc, __pycache__, target/) are
# excluded. The archives are reproducible from the canonical schemas in
# amp-sdk/schemas/ via ./amp-sdk/generate_bindings.sh.
set -euo pipefail

ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
SDK_DIR="$ROOT/amp-sdk"
DIST_DIR="$ROOT/dist"

echo "=========================================="
echo " Packaging AMP SDKs for Distribution"
echo "=========================================="

rm -rf "$DIST_DIR"
mkdir -p "$DIST_DIR"

# ---- C++ (headers + source + Unreal plugin + schemas) --------------------
echo "[1/6] Packaging C++ SDK..."
mkdir -p "$DIST_DIR/cpp_staging"
# Real client (NOT the examples/ copy — the old script pointed at the empty
# examples/cpp/include dir and missed client.cpp).
cp -r "$SDK_DIR/cpp/include" "$DIST_DIR/cpp_staging/"
cp -r "$SDK_DIR/cpp/src" "$DIST_DIR/cpp_staging/"
cp -r "$SDK_DIR/cpp/unreal" "$DIST_DIR/cpp_staging/"
cp -r "$SDK_DIR/schemas" "$DIST_DIR/cpp_staging/"
cp "$SDK_DIR/README.md" "$DIST_DIR/cpp_staging/"
tar -C "$DIST_DIR" -czf "$DIST_DIR/amp_cpp_sdk.tar.gz" cpp_staging
rm -rf "$DIST_DIR/cpp_staging"

# ---- C# (.NET / Unity) ---------------------------------------------------
echo "[2/6] Packaging C# SDK..."
mkdir -p "$DIST_DIR/csharp_staging/AmpSdk"
cp "$SDK_DIR/csharp/AmpSdk/"*.cs "$DIST_DIR/csharp_staging/AmpSdk/"
cp "$SDK_DIR/csharp/AmpSdk/"*.csproj "$DIST_DIR/csharp_staging/AmpSdk/"
cp "$SDK_DIR/csharp/AmpSdk/"*.asmdef "$DIST_DIR/csharp_staging/AmpSdk/" 2>/dev/null || true
cp "$SDK_DIR/csharp/AmpSdk/package.json" "$DIST_DIR/csharp_staging/AmpSdk/" 2>/dev/null || true
cp "$SDK_DIR/csharp/AmpSdk/link.xml" "$DIST_DIR/csharp_staging/AmpSdk/" 2>/dev/null || true
cp -r "$SDK_DIR/schemas" "$DIST_DIR/csharp_staging/"
( cd "$DIST_DIR" && zip -qr amp_csharp_sdk.zip csharp_staging )
rm -rf "$DIST_DIR/csharp_staging"

# ---- Go ------------------------------------------------------------------
echo "[3/6] Packaging Go SDK..."
mkdir -p "$DIST_DIR/go_staging"
cp "$SDK_DIR/go/go.mod" "$DIST_DIR/go_staging/"
cp -r "$SDK_DIR/go/client" "$DIST_DIR/go_staging/"
cp -r "$SDK_DIR/go/player" "$DIST_DIR/go_staging/"
cp -r "$SDK_DIR/go/matchmaking" "$DIST_DIR/go_staging/"
# Generated bindings are reproducible from schemas; include them so a fresh
# build doesn't require the capnp compiler.
cp -r "$SDK_DIR/go/generated" "$DIST_DIR/go_staging/"
cp -r "$SDK_DIR/schemas" "$DIST_DIR/go_staging/"
tar -C "$DIST_DIR" -czf "$DIST_DIR/amp_go_sdk.tar.gz" go_staging
rm -rf "$DIST_DIR/go_staging"

# ---- Rust ----------------------------------------------------------------
echo "[4/6] Packaging Rust SDK..."
mkdir -p "$DIST_DIR/rust_staging"
cp "$SDK_DIR/rust/Cargo.toml" "$DIST_DIR/rust_staging/"
cp -r "$SDK_DIR/rust/src" "$DIST_DIR/rust_staging/"
cp "$SDK_DIR/rust/build.rs" "$DIST_DIR/rust_staging/"
cp -r "$SDK_DIR/schemas" "$DIST_DIR/rust_staging/"
tar -C "$DIST_DIR" -czf "$DIST_DIR/amp_rust_sdk.tar.gz" rust_staging
rm -rf "$DIST_DIR/rust_staging"

# ---- Python --------------------------------------------------------------
echo "[5/6] Packaging Python SDK..."
mkdir -p "$DIST_DIR/python_staging"
cp "$SDK_DIR/python/pyproject.toml" "$DIST_DIR/python_staging/"
cp -r "$SDK_DIR/python/amp_sdk" "$DIST_DIR/python_staging/"
# Strip __pycache__ / .pyc from the staging copy.
find "$DIST_DIR/python_staging" -type d -name __pycache__ -prune -exec rm -rf {} + 2>/dev/null || true
find "$DIST_DIR/python_staging" -name '*.pyc' -delete 2>/dev/null || true
cp -r "$SDK_DIR/schemas" "$DIST_DIR/python_staging/"
tar -C "$DIST_DIR" -czf "$DIST_DIR/amp_python_sdk.tar.gz" python_staging
rm -rf "$DIST_DIR/python_staging"

# ---- JavaScript / TypeScript --------------------------------------------
echo "[6/6] Packaging JavaScript / TypeScript SDK..."
mkdir -p "$DIST_DIR/js_staging"
cp "$SDK_DIR/js/package.json" "$DIST_DIR/js_staging/"
cp "$SDK_DIR/js/tsconfig.json" "$DIST_DIR/js_staging/"
cp -r "$SDK_DIR/js/src" "$DIST_DIR/js_staging/"
cp -r "$SDK_DIR/js/native" "$DIST_DIR/js_staging/"
# Include the pre-built dist/ if present (built by `npm run build`).
[ -d "$SDK_DIR/js/dist" ] && cp -r "$SDK_DIR/js/dist" "$DIST_DIR/js_staging/" || true
cp "$SDK_DIR/js/README.md" "$DIST_DIR/js_staging/" 2>/dev/null || true
cp -r "$SDK_DIR/schemas" "$DIST_DIR/js_staging/"
tar -C "$DIST_DIR" -czf "$DIST_DIR/amp_js_sdk.tar.gz" js_staging
rm -rf "$DIST_DIR/js_staging"

echo "=========================================="
echo " Packaging Complete!"
echo " Artifacts in: $DIST_DIR"
( cd "$DIST_DIR" && ls -lh )
echo "=========================================="
