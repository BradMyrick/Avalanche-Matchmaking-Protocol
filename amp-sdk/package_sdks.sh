#!/bin/bash
set -e

echo "=========================================="
echo " Packaging AMP SDKs for Distribution"
echo "=========================================="

DIST_DIR="dist"
rm -rf $DIST_DIR
mkdir -p $DIST_DIR

# 1. Package C++ SDK
echo "[1/2] Packaging C++ SDK..."
CPP_DIST="$DIST_DIR/cpp_sdk"
mkdir -p $CPP_DIST/include
mkdir -p $CPP_DIST/src
mkdir -p $CPP_DIST/schemas

cp amp-sdk/cpp_example/include/*.hpp $CPP_DIST/include/
cp amp-sdk/cpp_example/src/*.cpp $CPP_DIST/src/
cp amp-sdk/schemas/*.capnp $CPP_DIST/schemas/
cp amp-sdk/cpp_example/CMakeLists.txt $CPP_DIST/

cd $DIST_DIR
tar -czf amp_cpp_sdk.tar.gz cpp_sdk
rm -rf cpp_sdk
cd ..

# 2. Package C# SDK
echo "[2/2] Packaging C# SDK..."
CSHARP_DIST="$DIST_DIR/csharp_sdk"
mkdir -p $CSHARP_DIST/src
mkdir -p $CSHARP_DIST/schemas

cp -r amp-sdk/csharp_example/* $CSHARP_DIST/
cp amp-sdk/schemas/*.capnp $CSHARP_DIST/schemas/

cd $DIST_DIR
zip -r amp_csharp_sdk.zip csharp_sdk
rm -rf csharp_sdk
cd ..

echo "=========================================="
echo " Packaging Complete!"
echo " Artifacts in: $(pwd)/$DIST_DIR"
ls -lh $DIST_DIR
echo "=========================================="
