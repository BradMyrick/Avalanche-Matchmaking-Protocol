fn main() {
    println!("cargo:rerun-if-changed=../schemas/match.capnp");
    println!("cargo:rerun-if-changed=../schemas/game_types.capnp");
    println!("cargo:rerun-if-changed=../schemas/service.capnp");
    println!("cargo:rerun-if-changed=../schemas/rust.capnp");

    capnpc::CompilerCommand::new()
        .src_prefix("../schemas")
        .file("../schemas/match.capnp")
        .file("../schemas/game_types.capnp")
        .file("../schemas/service.capnp")
        .run()
        .expect("capnp compilation failed");
}
