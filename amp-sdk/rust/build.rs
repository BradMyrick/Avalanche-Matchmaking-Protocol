fn main() {
    let schema_dir = std::path::PathBuf::from("../schemas");

    let schemas = [
        "game_types.capnp",
        "game_core.capnp",
        "match.capnp",
        "player_profile.capnp",
        "matchmaking_rules.capnp",
        "game_registry.capnp",
        "amp_telemetry.capnp",
        "relayer.capnp",
        "inventory.capnp",
        "tournament.capnp",
        "security.capnp",
        "service.capnp",
        "rust.capnp",
    ];

    for s in &schemas {
        println!("cargo:rerun-if-changed={}/{}", schema_dir.display(), s);
    }

    let mut cmd = capnpc::CompilerCommand::new();
    cmd.src_prefix(schema_dir.to_str().unwrap());
    for s in &schemas {
        if *s != "rust.capnp" {
            cmd.file(schema_dir.join(*s));
        }
    }
    cmd.run().expect("capnp compilation failed");
}
