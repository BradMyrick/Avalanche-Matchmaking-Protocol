fn main() {
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/match.capnp");
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/game_types.capnp");
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/service.capnp");
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/game_core.capnp");
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/amp_telemetry.capnp");
    println!("cargo:rerun-if-changed=../amp-sdk/schemas/relayer.capnp");

    capnpc::CompilerCommand::new()
        .src_prefix("../amp-sdk/schemas")
        .file("../amp-sdk/schemas/match.capnp")
        .file("../amp-sdk/schemas/game_types.capnp")
        .file("../amp-sdk/schemas/service.capnp")
        .file("../amp-sdk/schemas/game_core.capnp")
        .file("../amp-sdk/schemas/amp_telemetry.capnp")
        .file("../amp-sdk/schemas/relayer.capnp")
        .run()
        .expect("capnp compilation failed");
}
