fn main() {
    // Ensure /home/kodr/local/bin is in PATH for capnp
    let path = std::env::var("PATH").unwrap_or_default();
    if !path.contains("/home/kodr/local/bin") {
        unsafe {
            std::env::set_var("PATH", format!("{}:/home/kodr/local/bin", path));
        }
    }

    let root = std::env::var("CARGO_MANIFEST_DIR").unwrap();
    let schema_dir = std::path::PathBuf::from(root).join("../amp-sdk/schemas");
    let schema_dir_str = schema_dir.to_str().unwrap();

    // Original schemas
    println!("cargo:rerun-if-changed={}/match.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/game_types.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/service.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/game_core.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/amp_telemetry.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/relayer.capnp", schema_dir_str);

    // New AMP FlexMatch-style schemas
    println!("cargo:rerun-if-changed={}/player_profile.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/matchmaking_rules.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/game_registry.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/inventory.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/tournament.capnp", schema_dir_str);
    println!("cargo:rerun-if-changed={}/security.capnp", schema_dir_str);

    capnpc::CompilerCommand::new()
        .src_prefix(schema_dir_str)
        // Original schemas
        .file(schema_dir.join("match.capnp"))
        .file(schema_dir.join("game_types.capnp"))
        .file(schema_dir.join("service.capnp"))
        .file(schema_dir.join("game_core.capnp"))
        .file(schema_dir.join("amp_telemetry.capnp"))
        .file(schema_dir.join("relayer.capnp"))
        // New AMP FlexMatch-style schemas
        .file(schema_dir.join("player_profile.capnp"))
        .file(schema_dir.join("matchmaking_rules.capnp"))
        .file(schema_dir.join("game_registry.capnp"))
        .file(schema_dir.join("inventory.capnp"))
        .file(schema_dir.join("tournament.capnp"))
        .file(schema_dir.join("security.capnp"))
        .file(schema_dir.join("rust.capnp"))
        .run()
        .expect("Failed to compile Cap'n Proto schemas. Is 'capnp' installed and in your PATH?");
}
