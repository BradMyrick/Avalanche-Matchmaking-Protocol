fn main() {
    let schema_dir = std::path::PathBuf::from("../schemas");
    capnpc::CompilerCommand::new()
        .src_prefix(schema_dir.to_str().unwrap())
        .file(schema_dir.join("match.capnp"))
        .file(schema_dir.join("player_profile.capnp"))
        .file(schema_dir.join("matchmaking_rules.capnp"))
        .file(schema_dir.join("game_registry.capnp"))
        .run()
        .expect("capnp compilation failed");
}
