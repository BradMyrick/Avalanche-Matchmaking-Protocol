//! Per-IP token-bucket rate limiting.
//!
//! Two tiers, tuned for a game-client API behind a reverse proxy:
//! - AUTH: strict (default 10 req/min/IP) — challenge spam, per-wallet
//!   lockout fill, credential stuffing
//! - GENERAL: generous (default 300 req/min/IP) — catches runaway loops
//!   without touching healthy gameplay traffic
//!
//! Client IP resolution: `X-Forwarded-For` (first hop) is honored ONLY when
//! the socket peer is private/loopback (the Cloudflare-tunnel / LAN-proxy
//! case); otherwise the socket address is used directly, so a public client
//! cannot spoof its IP with a header.

use std::net::IpAddr;
use std::sync::Mutex;
use std::time::{Duration, Instant};

use axum::extract::ConnectInfo;
use axum::http::{HeaderValue, Request, StatusCode};
use axum::middleware::Next;
use axum::response::{IntoResponse, Response};
use dashmap::DashMap;

#[derive(Clone, Copy, PartialEq)]
pub enum Tier {
    /// /v1/auth/* — strict.
    Auth,
    /// Everything else — generous.
    General,
}

pub struct RateLimiter {
    buckets: DashMap<IpAddr, Bucket>,
    /// Periodic pruning state.
    last_prune: Mutex<Instant>,
}

struct Bucket {
    tokens: f64,
    updated: Instant,
}

impl RateLimiter {
    pub fn new() -> Self {
        Self {
            buckets: DashMap::new(),
            last_prune: Mutex::new(Instant::now()),
        }
    }

    /// Take one token; true = allowed.
    fn take(&self, ip: IpAddr, capacity: f64, refill_per_sec: f64) -> bool {
        let now = Instant::now();
        let mut b = self.buckets.entry(ip).or_insert(Bucket {
            tokens: capacity,
            updated: now,
        });
        let elapsed = now.duration_since(b.updated).as_secs_f64();
        b.tokens = (b.tokens + elapsed * refill_per_sec).min(capacity);
        b.updated = now;
        if b.tokens >= 1.0 {
            b.tokens -= 1.0;
            true
        } else {
            false
        }
    }

    fn prune(&self) {
        let should = {
            let mut last = self.last_prune.lock().unwrap();
            if last.elapsed() >= Duration::from_secs(300) {
                *last = Instant::now();
                true
            } else {
                false
            }
        };
        if should {
            // Buckets idle for an hour hold full tokens; drop them.
            self.buckets
                .retain(|_, b| b.updated.elapsed() < Duration::from_secs(3600));
        }
    }
}

impl Default for RateLimiter {
    fn default() -> Self {
        Self::new()
    }
}

fn client_ip<A>(req: &Request<A>, peer: Option<&ConnectInfo<std::net::SocketAddr>>) -> IpAddr {
    let peer_ip = peer.map(|c| c.0.ip());
    // Trust X-Forwarded-For only from a private/loopback peer (our proxy).
    if let Some(peer) = peer_ip {
        if (peer.is_loopback() || is_private(peer))
            && let Some(xff) = req
                .headers()
                .get("x-forwarded-for")
                .and_then(|v: &HeaderValue| v.to_str().ok())
                && let Some(first) = xff.split(',').next()
                    && let Ok(ip) = first.trim().parse::<IpAddr>() {
                        return ip;
                    }
        return peer;
    }
    // No connect info (unit tests): fall back to a sentinel.
    "0.0.0.0".parse().unwrap()
}

fn is_private(ip: IpAddr) -> bool {
    match ip {
        IpAddr::V4(v4) => {
            let o = v4.octets();
            o[0] == 10 || o[0] == 172 && (16..=31).contains(&o[1]) || o[0] == 192 && o[1] == 168
        }
        IpAddr::V6(v6) => {
            // fc00::/7 unique-local + fe80::/10 link-local
            (v6.segments()[0] & 0xfe00) == 0xfc00 || (v6.segments()[0] & 0xffc0) == 0xfe80
        }
    }
}

fn limits(tier: Tier) -> (f64, f64) {
    match tier {
        Tier::Auth => (10.0, 10.0 / 60.0),   // burst 10, 1 per 6s sustained
        Tier::General => (300.0, 300.0 / 60.0), // burst 300, 5/s sustained
    }
}

fn too_many(tier: Tier) -> Response {
    let (capacity, _) = limits(tier);
    let body = serde_json::json!({
        "error": "rate_limited",
        "code": "rate_limited",
        "message": format!("too many requests — burst limit {} per IP", capacity as u64),
    })
    .to_string();
    (
        StatusCode::TOO_MANY_REQUESTS,
        [("retry-after", "10"), ("content-type", "application/json")],
        body,
    )
        .into_response()
}

/// Middleware: applies the AUTH tier to /v1/auth/*, GENERAL elsewhere.
pub async fn limit(
    ConnectInfo(peer): ConnectInfo<std::net::SocketAddr>,
    req: Request<axum::body::Body>,
    next: Next,
) -> Response {
    let tier = if req.uri().path().starts_with("/v1/auth") {
        Tier::Auth
    } else {
        Tier::General
    };
    let ip = client_ip(&req, Some(&ConnectInfo(peer)));
    let limiter = req
        .extensions()
        .get::<std::sync::Arc<RateLimiter>>()
        .cloned();
    let limiter = match limiter {
        Some(l) => l,
        None => return next.run(req).await, // limiter not wired — fail open
    };
    limiter.prune();
    let (capacity, refill) = limits(tier);
    if !limiter.take(ip, capacity, refill) {
        return too_many(tier);
    }
    next.run(req).await
}

#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn bucket_refills_and_bursts() {
        let rl = RateLimiter::new();
        let ip: IpAddr = "1.2.3.4".parse().unwrap();
        for _ in 0..10 {
            assert!(rl.take(ip, 10.0, 1.0 / 6.0));
        }
        assert!(!rl.take(ip, 10.0, 1.0 / 6.0), "burst exhausted");
    }

    #[test]
    fn idle_buckets_are_prunable() {
        let rl = RateLimiter::new();
        let ip: IpAddr = "9.9.9.9".parse().unwrap();
        rl.take(ip, 10.0, 1.0);
        rl.buckets.get_mut(&ip).unwrap().updated = Instant::now() - Duration::from_secs(7200);
        *rl.last_prune.lock().unwrap() = Instant::now() - Duration::from_secs(301);
        rl.prune();
        assert!(rl.buckets.get(&ip).is_none());
    }
}
