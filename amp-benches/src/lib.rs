//! Phase 2.6/2.7 — shared types for AMP hot-path benchmarks and the latency gate.

/// Representative match payload (~150–200 B, several Strings/Vecs) — mirrors
/// `amp_server::state::ActiveMatch` for data-structure benchmarking without
/// pulling the server binary crate into the bench target.
#[derive(Clone)]
pub struct BenchMatch {
    pub match_id: String,
    pub game_id: String,
    pub players: Vec<String>,
    pub created_at_ms: u64,
    pub settled: bool,
    pub settled_at_ms: Option<u64>,
    pub expires_at_ms: Option<u64>,
    pub settlement_failed: bool,
    pub settlement_tx_hash: String,
}
