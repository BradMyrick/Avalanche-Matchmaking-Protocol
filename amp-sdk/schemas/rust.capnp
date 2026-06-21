@0xa32f5e91e36709d2;

using Go = import "go.capnp";
$Go.package("generated");
$Go.import("amp-sdk/go/generated");

# This file tells the Rust code generator how to name the generated files.
# It is typically imported and used via the annotation.

# The @0... ID is arbitrary but conventionally unique.
# Rust capnp tools usually provide this file in the `capnpc` installation.
# However, to compile without relying on system-wide includes, we can define it locally.

annotation parentModule @0xa1b2c3d4e5f60004 (file) :Text;
# Specify the parent module for the generated Rust code.
