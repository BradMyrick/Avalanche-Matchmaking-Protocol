fn main() {
    let schemas = [
        "../amp-sdk/schemas/amp_telemetry.capnp",
        "../amp-sdk/schemas/game_core.capnp",
        "../amp-sdk/schemas/game_types.capnp",
        "../amp-sdk/schemas/match.capnp",
        "../amp-sdk/schemas/rust.capnp",
        "../amp-sdk/schemas/service.capnp",
    ];
    let mut cmd = capnpc::CompilerCommand::new();
    cmd.src_prefix("../amp-sdk/schemas");

    for schema in schemas {
        println!("cargo:rerun-if-changed={}", schema);
        cmd.file(schema);
    }

    cmd.run().expect("schema compiler command");
}
