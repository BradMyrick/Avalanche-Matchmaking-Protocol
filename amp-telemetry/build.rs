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

    capnpc::CompilerCommand::new()
        .src_prefix(schema_dir_str)
        .file(schema_dir.join("amp_telemetry.capnp"))
        .file(schema_dir.join("match.capnp"))
        .file(schema_dir.join("game_core.capnp"))
        .file(schema_dir.join("rust.capnp"))
        .run()
        .expect("Failed to compile Cap'n Proto schemas. Is 'capnp' installed and in your PATH?");
}
