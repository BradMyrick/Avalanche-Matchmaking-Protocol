//! Phase 2.6 — matchstore microbenchmark.
//!
//! Proves the root-cause fix for the latency regression. The server's
//! active-match store was `Arc<ArcSwap<HashMap<String, ActiveMatch>>>`, whose
//! every mutation cloned the ENTIRE map inside `rcu`. At the documented
//! `MAX_ACTIVE_MATCHES = 10_000` that is a ~1.5–2 MB allocation + memcpy per
//! insert/update/archive — on the hottest path in the protocol.
//!
//! This bench compares, at 10 000 entries, the two approaches for the three
//! mutating operations (`insert`, `update`, `remove`) that the matchmaker and
//! settlement paths exercise on every match. The perf-gate (Phase 2.7) asserts
//! the DashMap variants stay sub-50µs p99.

use amp_benches::BenchMatch;
use criterion::{BenchmarkId, Criterion, Throughput, black_box, criterion_group, criterion_main};
use dashmap::DashMap;
use rustc_hash::FxHasher;
use std::collections::HashMap;
use std::hash::BuildHasherDefault;
use std::sync::Arc;

use arc_swap::ArcSwap;

type FxMap<V> = HashMap<String, V, BuildHasherDefault<FxHasher>>;

fn bench_match(i: usize) -> BenchMatch {
    BenchMatch {
        match_id: format!("match-{i}"),
        game_id: format!("game-{}", i / 16),
        players: vec![format!("p-{}", i * 2), format!("p-{}", i * 2 + 1)],
        created_at_ms: i as u64,
        settled: false,
        settled_at_ms: None,
        expires_at_ms: Some((i as u64) + 3_600_000),
        settlement_failed: false,
        settlement_tx_hash: String::new(),
    }
}

const N: usize = 10_000;

fn bench_insert(c: &mut Criterion) {
    let mut group = c.benchmark_group("insert_at_10k");
    group.throughput(Throughput::Elements(1));

    group.bench_function(BenchmarkId::new("dashmap", N), |b| {
        let map: DashMap<String, BenchMatch> = DashMap::new();
        for i in 0..N {
            map.insert(format!("seed-{i}"), bench_match(i));
        }
        let mut i = N;
        b.iter(|| {
            let id = format!("m-{}", {
                i += 1;
                i
            });
            map.insert(id.clone(), bench_match(i));
            black_box(&map);
        });
    });

    group.bench_function(BenchmarkId::new("arcswap_clone", N), |b| {
        let mut map: FxMap<BenchMatch> = FxMap::default();
        for i in 0..N {
            map.insert(format!("seed-{i}"), bench_match(i));
        }
        let store: Arc<ArcSwap<FxMap<BenchMatch>>> = Arc::new(ArcSwap::from_pointee(map));
        let mut i = N;
        b.iter(|| {
            // Reproduces the OLD `rcu(|am| { let mut m = (**am).clone(); m.insert(...); m })`
            let id = format!("m-{}", {
                i += 1;
                i
            });
            store.rcu(|am| {
                let mut m = (**am).clone();
                m.insert(id.clone(), bench_match(i));
                m
            });
            black_box(&store);
        });
    });

    group.finish();
}

fn bench_update(c: &mut Criterion) {
    let mut group = c.benchmark_group("update_at_10k");
    group.throughput(Throughput::Elements(1));

    group.bench_function(BenchmarkId::new("dashmap", N), |b| {
        let map: DashMap<String, BenchMatch> = DashMap::new();
        for i in 0..N {
            map.insert(format!("m-{i}"), bench_match(i));
        }
        let mut i = 0;
        b.iter(|| {
            i = (i + 1) % N;
            let id = format!("m-{i}");
            if let Some(mut m) = map.get_mut(&id) {
                m.value_mut().settled = true;
                m.value_mut().settled_at_ms = Some(i as u64);
            }
            black_box(&map);
        });
    });

    group.bench_function(BenchmarkId::new("arcswap_clone", N), |b| {
        let mut map: FxMap<BenchMatch> = FxMap::default();
        for i in 0..N {
            map.insert(format!("m-{i}"), bench_match(i));
        }
        let store: Arc<ArcSwap<FxMap<BenchMatch>>> = Arc::new(ArcSwap::from_pointee(map));
        let mut i = 0;
        b.iter(|| {
            i = (i + 1) % N;
            let id = format!("m-{i}");
            store.rcu(|am| {
                let mut m = (**am).clone();
                if let Some(e) = m.get_mut(&id) {
                    e.settled = true;
                    e.settled_at_ms = Some(i as u64);
                }
                m
            });
            black_box(&store);
        });
    });

    group.finish();
}

fn bench_remove(c: &mut Criterion) {
    let mut group = c.benchmark_group("remove_at_10k");
    group.throughput(Throughput::Elements(1));

    group.bench_function(BenchmarkId::new("dashmap", N), |b| {
        let map: DashMap<String, BenchMatch> = DashMap::new();
        for i in 0..N {
            map.insert(format!("m-{i}"), bench_match(i));
        }
        let mut i = 0;
        b.iter(|| {
            i = (i + 1) % N;
            let id = format!("m-{i}");
            let v = map.remove(&id);
            // re-insert to keep the map at size ~N across iterations
            map.insert(id, bench_match(i));
            black_box(v);
        });
    });

    group.bench_function(BenchmarkId::new("arcswap_clone", N), |b| {
        let mut map: FxMap<BenchMatch> = FxMap::default();
        for i in 0..N {
            map.insert(format!("m-{i}"), bench_match(i));
        }
        let store: Arc<ArcSwap<FxMap<BenchMatch>>> = Arc::new(ArcSwap::from_pointee(map));
        let mut i = 0;
        b.iter(|| {
            i = (i + 1) % N;
            let id = format!("m-{i}");
            store.rcu(|am| {
                let mut m = (**am).clone();
                m.remove(&id);
                m.insert(id.clone(), bench_match(i));
                m
            });
            black_box(&store);
        });
    });

    group.finish();
}

criterion_group!(benches, bench_insert, bench_update, bench_remove);
criterion_main!(benches);
