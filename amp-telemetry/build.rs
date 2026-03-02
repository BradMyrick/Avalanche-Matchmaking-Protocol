fn main() {
    capnpc::CompilerCommand::new()
        .src_prefix("../amp-sdk/schemas")
        .file("../amp-sdk/schemas/amp_telemetry.capnp")
        .file("../amp-sdk/schemas/match.capnp")
        .file("../amp-sdk/schemas/game_core.capnp")
        .file("../amp-sdk/schemas/rust.capnp")
        .run()
        .expect("schema compiler command");
}
