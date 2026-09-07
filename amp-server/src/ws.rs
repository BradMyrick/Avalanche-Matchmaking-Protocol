//! Player notification hub. Every connected client holds one WebSocket;
//! the server pushes lifecycle events the moment they happen so the queue
//! never feels like a black box:
//!
//! - `queue_status`  — depth, your wait, current skill window (throttled)
//! - `match_found`   — opponent card + connect info
//! - `match_result`  — outcome, rating deltas, attestation when signed
//! - `match_update`  — state transitions (disputed, cancelled, settling…)
//!
//! Hardening (v0.3): per-connection channels are BOUNDED. A client that
//! stops reading gets at most `CHANNEL_CAPACITY` queued events before its
//! connection is dropped — a slow or malicious reader can no longer grow
//! server memory without bound. Each wallet may hold a small number of
//! simultaneous connections (multi-tab), capped.

use dashmap::DashMap;
use serde_json::{Value, json};
use tokio::sync::mpsc;

/// Queued events per connection before the hub drops it.
const CHANNEL_CAPACITY: usize = 64;
/// Simultaneous connections per wallet (multi-tab support, DoS bound).
pub const MAX_CONNS_PER_WALLET: usize = 3;

#[derive(Default)]
struct WalletConnections {
    next_id: u64,
    senders: Vec<(u64, mpsc::Sender<String>)>,
}

pub struct WsHub {
    senders: DashMap<String, WalletConnections>,
}

/// A registered connection; drop the returned guard semantics are manual —
/// call [`WsHub::unregister`] with the id when the socket closes.
pub struct Connection {
    pub id: u64,
    pub rx: mpsc::Receiver<String>,
}

impl std::fmt::Debug for Connection {
    fn fmt(&self, f: &mut std::fmt::Formatter<'_>) -> std::fmt::Result {
        f.debug_struct("Connection").field("id", &self.id).finish()
    }
}

#[derive(Debug, PartialEq)]
pub enum RegisterError {
    /// Wallet already holds MAX_CONNS_PER_WALLET sockets.
    TooManyConnections,
}

impl WsHub {
    pub fn new() -> Self {
        Self {
            senders: DashMap::new(),
        }
    }

    pub fn register(&self, wallet: &str) -> Result<Connection, RegisterError> {
        let mut conns = self.senders.entry(wallet.to_string()).or_default();
        conns.senders.retain(|(_, tx)| !tx.is_closed());
        if conns.senders.len() >= MAX_CONNS_PER_WALLET {
            return Err(RegisterError::TooManyConnections);
        }
        let (tx, rx) = mpsc::channel(CHANNEL_CAPACITY);
        let id = conns.next_id;
        conns.next_id += 1;
        conns.senders.push((id, tx));
        Ok(Connection { id, rx })
    }

    pub fn unregister(&self, wallet: &str, id: u64) {
        if let Some(mut conns) = self.senders.get_mut(wallet) {
            conns.senders.retain(|(cid, _)| *cid != id);
            if conns.senders.is_empty() {
                drop(conns);
                self.senders.remove_if(wallet, |_, c| c.senders.is_empty());
            }
        }
    }

    /// Best-effort send. A full or closed queue drops that connection —
    /// the matchmaker never blocks on a slow reader.
    pub fn send(&self, wallet: &str, event_type: &str, payload: Value) {
        let msg = json!({ "type": event_type, "data": payload }).to_string();
        if let Some(mut conns) = self.senders.get_mut(wallet) {
            conns
                .senders
                .retain(|(_, tx)| tx.try_send(msg.clone()).is_ok());
        }
    }

    #[allow(dead_code)] // reserved for global announcements (maintenance, seasons)
    pub fn broadcast(&self, event_type: &str, payload: Value) {
        let msg = json!({ "type": event_type, "data": payload }).to_string();
        for mut entry in self.senders.iter_mut() {
            entry
                .value_mut()
                .senders
                .retain(|(_, tx)| tx.try_send(msg.clone()).is_ok());
        }
    }

    pub fn connected_count(&self) -> usize {
        self.senders.len()
    }
}

impl Default for WsHub {
    fn default() -> Self {
        Self::new()
    }
}

#[cfg(test)]
mod tests {
    use super::*;

    #[tokio::test]
    async fn events_reach_all_sockets_for_wallet() {
        let hub = WsHub::new();
        let mut c1 = hub.register("0xa").unwrap();
        let mut c2 = hub.register("0xa").unwrap();
        hub.send("0xa", "match_found", json!({ "matchId": "m1" }));
        assert_eq!(
            c1.rx.recv().await.unwrap(),
            r#"{"data":{"matchId":"m1"},"type":"match_found"}"#
        );
        assert_eq!(
            c2.rx.recv().await.unwrap(),
            r#"{"data":{"matchId":"m1"},"type":"match_found"}"#
        );
    }

    #[tokio::test]
    async fn other_wallets_do_not_receive() {
        let hub = WsHub::new();
        let mut c_a = hub.register("0xa").unwrap();
        let c_b = hub.register("0xb").unwrap();
        let mut c_b = c_b;
        hub.send("0xa", "queue_status", json!({ "depth": 2 }));
        assert!(c_a.rx.recv().await.is_some());
        assert!(c_b.rx.try_recv().is_err());
    }

    #[tokio::test]
    async fn per_wallet_connection_cap() {
        let hub = WsHub::new();
        let _a = hub.register("0xa").unwrap();
        let _b = hub.register("0xa").unwrap();
        let _c = hub.register("0xa").unwrap();
        assert_eq!(
            hub.register("0xa").unwrap_err(),
            RegisterError::TooManyConnections
        );
    }

    #[tokio::test]
    async fn closed_sockets_free_capacity() {
        let hub = WsHub::new();
        let a = hub.register("0xa").unwrap();
        let _b = hub.register("0xa").unwrap();
        let _c = hub.register("0xa").unwrap();
        assert!(hub.register("0xa").is_err());
        hub.unregister("0xa", a.id);
        assert!(hub.register("0xa").is_ok());
    }

    #[tokio::test]
    async fn slow_reader_is_dropped_not_buffered_forever() {
        let hub = WsHub::new();
        let conn = hub.register("0xa").unwrap();
        let mut conn = conn;
        // Never recv() — fill far beyond capacity; the hub must shed us.
        for i in 0..(CHANNEL_CAPACITY * 4) {
            hub.send("0xa", "queue_status", json!({ "i": i }));
        }
        // Our sender must have been dropped from the hub.
        assert!(hub.senders.get("0xa").unwrap().senders.is_empty());
        // Drain at most CHANNEL_CAPACITY events came through.
        let mut got = 0;
        while conn.rx.try_recv().is_ok() {
            got += 1;
        }
        assert!(got <= CHANNEL_CAPACITY);
    }
}
