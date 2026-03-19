fn main() {
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/relayer.capnp");

    capnpc::CompilerCommand::new()
        .src_prefix("../amp-sdk/schemas")
        .file("../amp-sdk/schemas/relayer.capnp")
        .run()
        .expect("compiling schema");
}
