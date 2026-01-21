fn main() {
    capnpc::CompilerCommand::new()
        .src_prefix("../schemas")
        .file("../schemas/match.capnp")
        .run()
        .expect("capnp compilation failed");
}
