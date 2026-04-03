pub mod rust_capnp {
    include!(concat!(env!("OUT_DIR"), "/rust_capnp.rs"));
}
pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
pub mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
pub mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
pub mod player_profile_capnp { include!(concat!(env!("OUT_DIR"), "/player_profile_capnp.rs")); }
pub mod matchmaking_rules_capnp { include!(concat!(env!("OUT_DIR"), "/matchmaking_rules_capnp.rs")); }
pub mod game_registry_capnp { include!(concat!(env!("OUT_DIR"), "/game_registry_capnp.rs")); }
pub mod inventory_capnp { include!(concat!(env!("OUT_DIR"), "/inventory_capnp.rs")); }
pub mod tournament_capnp { include!(concat!(env!("OUT_DIR"), "/tournament_capnp.rs")); }
pub mod security_capnp { include!(concat!(env!("OUT_DIR"), "/security_capnp.rs")); }
pub mod relayer_capnp { include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs")); }
