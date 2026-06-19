//! Node.js native bindings for the AMP Rust SDK (napi-rs).
//!
//! The AMP Cap'n Proto client is `!Send` (it runs on a tokio `LocalSet`), so
//! it cannot be driven directly from napi-rs async methods (which require
//! `Send` futures). Instead we park the entire RPC stack on a dedicated worker
//! thread running its own current-thread runtime + `LocalSet`, and bridge JS
//! calls to it via a tokio channel + oneshot replies. Each `#[napi] async fn`
//! only holds a `mpsc::Sender` (Send+Sync) and a `oneshot::Receiver` (Send),
//! so the JS-facing future is `Send` as required.
//!
//! Match events use a poll model (`startEvents` + `pollEvent`) rather than a
//! ThreadsafeFunction callback, which avoids fragile const-generic TSFN wiring
//! while still letting the JS wrapper expose a clean event-emitter surface.

#![allow(clippy::all, clippy::nursery, non_snake_case, dead_code)]

use napi::bindgen_prelude::*;
use napi_derive::napi;
use std::time::Duration;
use tokio::sync::{mpsc, oneshot};

// ─── JS-facing DTOs ───────────────────────────────────────────────────────

#[napi(object)]
pub struct Challenge {
    /// Raw challenge bytes to be signed with EIP-191 (ethers `signMessage`).
    pub bytes: Buffer,
    /// Absolute expiry, nanoseconds since UNIX_EPOCH.
    pub expires_at_ns: i64,
}

#[napi(object)]
pub struct MatchAssignment {
    pub match_id: String,
    pub quality: f64,
}

#[napi(object)]
pub struct MatchEventPayload {
    /// "settled" or "opponent_disconnected".
    pub kind: String,
    pub victor: u32,
    pub scores: Vec<u32>,
}

/// Reply type crossing from the worker back to JS. Plain `Result<T, String>`
/// (fully qualified so it isn't shadowed by `napi::bindgen_prelude::Result`).
type WorkerReply<T> = std::result::Result<T, String>;

/// Commands crossing from the JS (napi) thread to the AMP worker thread.
enum Command {
    Connect {
        addr: String,
        reply: oneshot::Sender<WorkerReply<()>>,
    },
    RequestChallenge {
        game_id: u64,
        reply: oneshot::Sender<WorkerReply<Challenge>>,
    },
    Login {
        game_id: u64,
        signature: Vec<u8>,
        challenge: Vec<u8>,
        reply: oneshot::Sender<WorkerReply<()>>,
    },
    RequestMatch {
        game_id: Vec<u8>,
        reply: oneshot::Sender<WorkerReply<MatchAssignment>>,
    },
    Reconnect {
        match_id: Vec<u8>,
        reply: oneshot::Sender<WorkerReply<()>>,
    },
    SubmitOutcome {
        match_id: Vec<u8>,
        outcome: u8,
        transcript_hash: Vec<u8>,
        signature: Vec<u8>,
        reply: oneshot::Sender<WorkerReply<Vec<u8>>>,
    },
    EmitGameEvent {
        event_type: String,
        reply: oneshot::Sender<WorkerReply<()>>,
    },
    EmitTelemetry {
        event_type: u16,
        reply: oneshot::Sender<WorkerReply<()>>,
    },
    /// Begin forwarding server-pushed events; the receiver is held on the
    /// worker and drained by `PollEvent`.
    StartEvents {
        reply: oneshot::Sender<WorkerReply<()>>,
    },
    PollEvent {
        timeout_ms: u64,
        reply: oneshot::Sender<WorkerReply<Option<MatchEventPayload>>>,
    },
    Close {
        reply: oneshot::Sender<()>,
    },
}

// ─── Worker thread (owns the !Send AMP client) ────────────────────────────

struct Worker {
    client: Option<amp_sdk::AmpClient>,
    user_session: Option<amp_sdk::UserSession>,
    match_session: Option<amp_sdk::MatchSession>,
    event_rx: Option<tokio::sync::mpsc::UnboundedReceiver<amp_sdk::MatchEvent>>,
}

impl Worker {
    fn new() -> Self {
        Worker {
            client: None,
            user_session: None,
            match_session: None,
            event_rx: None,
        }
    }

    async fn handle(&mut self, cmd: Command) {
        match cmd {
            Command::Connect { addr, reply } => {
                let r = amp_sdk::AmpClient::connect(&addr)
                    .await
                    .map(|c| {
                        self.client = Some(c);
                    })
                    .map_err(|e| format!("connect: {e}"));
                let _ = reply.send(r);
            }
            Command::RequestChallenge { game_id, reply } => {
                let r = match &self.client {
                    Some(c) => c
                        .request_challenge(game_id)
                        .await
                        .map(|(bytes, exp)| Challenge {
                            bytes: bytes.into(),
                            expires_at_ns: exp as i64,
                        })
                        .map_err(|e| format!("requestChallenge: {e}")),
                    None => Err("not connected".into()),
                };
                let _ = reply.send(r);
            }
            Command::Login {
                game_id,
                signature,
                challenge,
                reply,
            } => {
                let r = match &self.client {
                    Some(c) => match c.login(game_id, &signature, &challenge).await {
                        Ok(s) => {
                            self.user_session = Some(s);
                            Ok(())
                        }
                        Err(e) => Err(format!("login: {e}")),
                    },
                    None => Err("not connected".into()),
                };
                let _ = reply.send(r);
            }
            Command::RequestMatch { game_id, reply } => {
                let r = match &self.user_session {
                    Some(s) => {
                        let req = amp_sdk::MatchRequest::new(&game_id);
                        match s.request_match(req).await {
                            Ok(assigned) => {
                                let mid = assigned.assignment.match_id_string();
                                self.match_session = Some(assigned.session);
                                Ok(MatchAssignment {
                                    match_id: mid,
                                    quality: assigned.assignment.quality as f64,
                                })
                            }
                            Err(e) => Err(format!("requestMatch: {e}")),
                        }
                    }
                    None => Err("not authenticated".into()),
                };
                let _ = reply.send(r);
            }
            Command::Reconnect { match_id, reply } => {
                let r = match &self.user_session {
                    Some(s) => match s.reconnect(&match_id).await {
                        Ok(sess) => {
                            self.match_session = Some(sess);
                            Ok(())
                        }
                        Err(e) => Err(format!("reconnect: {e}")),
                    },
                    None => Err("not authenticated".into()),
                };
                let _ = reply.send(r);
            }
            Command::SubmitOutcome {
                match_id,
                outcome,
                transcript_hash,
                signature,
                reply,
            } => {
                let r = match &self.match_session {
                    Some(s) => {
                        let sub = amp_sdk::OutcomeSubmission {
                            match_id,
                            outcome_type: amp_sdk::match_capnp::OutcomeType::Win,
                            scores: vec![],
                            victor: outcome,
                            metadata: vec![],
                            replay_hash: transcript_hash,
                            signature,
                        };
                        s.submit_outcome(sub)
                            .await
                            .map_err(|e| format!("submitOutcome: {e}"))
                    }
                    None => Err("no active match session".into()),
                };
                let _ = reply.send(r);
            }
            Command::EmitGameEvent { event_type, reply } => {
                let r = match &self.match_session {
                    Some(s) => s
                        .emit_game_event(&event_type, &[])
                        .await
                        .map_err(|e| format!("emitGameEvent: {e}")),
                    None => Err("no active match session".into()),
                };
                let _ = reply.send(r);
            }
            Command::EmitTelemetry { event_type, reply } => {
                let r = match &self.match_session {
                    Some(s) => s
                        .emit_telemetry(event_type, &[])
                        .await
                        .map_err(|e| format!("emitTelemetry: {e}")),
                    None => Err("no active match session".into()),
                };
                let _ = reply.send(r);
            }
            Command::StartEvents { reply } => {
                let r = match &self.match_session {
                    Some(s) => match s.subscribe_to_events() {
                        Ok(rx) => {
                            self.event_rx = Some(rx);
                            Ok(())
                        }
                        Err(e) => Err(format!("subscribe: {e}")),
                    },
                    None => Err("no active match session".into()),
                };
                let _ = reply.send(r);
            }
            Command::PollEvent { timeout_ms, reply } => {
                let r = match &mut self.event_rx {
                    Some(rx) => {
                        match tokio::time::timeout(Duration::from_millis(timeout_ms), rx.recv())
                            .await
                        {
                            Ok(Some(evt)) => Ok(Some(match evt {
                                amp_sdk::MatchEvent::Settled { victor, scores, .. } => {
                                    MatchEventPayload {
                                        kind: "settled".into(),
                                        victor: victor as u32,
                                        scores: scores.into_iter().map(|x| x as u32).collect(),
                                    }
                                }
                                amp_sdk::MatchEvent::OpponentDisconnected => MatchEventPayload {
                                    kind: "opponent_disconnected".into(),
                                    victor: 0,
                                    scores: vec![],
                                },
                            })),
                            Ok(None) => Ok(None), // channel closed (match ended)
                            Err(_) => Ok(None),   // timed out, no event yet
                        }
                    }
                    None => Err("events not started".into()),
                };
                let _ = reply.send(r);
            }
            Command::Close { reply } => {
                self.event_rx = None;
                self.match_session = None;
                self.user_session = None;
                self.client = None;
                let _ = reply.send(());
            }
        }
    }
}

fn run_worker(mut rx: mpsc::Receiver<Command>) {
    let rt = tokio::runtime::Builder::new_current_thread()
        .enable_all()
        .build()
        .expect("failed to build AMP JS worker runtime");
    let local = tokio::task::LocalSet::new();
    local.block_on(&rt, async move {
        let mut worker = Worker::new();
        while let Some(cmd) = rx.recv().await {
            worker.handle(cmd).await;
        }
    });
}

// ─── Helper: send a command + await its reply ─────────────────────────────

async fn send_and_recv<T>(
    tx: &mpsc::Sender<Command>,
    make_cmd: impl FnOnce(oneshot::Sender<WorkerReply<T>>) -> Command,
) -> Result<T>
where
    T: Send + 'static,
{
    let (s, r) = oneshot::channel::<WorkerReply<T>>();
    tx.send(make_cmd(s))
        .await
        .map_err(|e| Error::from_reason(format!("AMP worker gone: {e}")))?;
    r.await
        .map_err(|e| Error::from_reason(format!("AMP worker gone: {e}")))?
        .map_err(Error::from_reason)
}

// ─── JS-facing class ──────────────────────────────────────────────────────

#[napi]
pub struct AmpClient {
    tx: mpsc::Sender<Command>,
}

#[napi]
impl AmpClient {
    #[napi(constructor)]
    pub fn new() -> Result<Self> {
        let (tx, rx) = mpsc::channel::<Command>(64);
        std::thread::Builder::new()
            .name("amp-js-worker".into())
            .spawn(move || run_worker(rx))
            .map_err(|e| Error::from_reason(format!("failed to spawn AMP worker thread: {e}")))?;
        Ok(AmpClient { tx })
    }

    #[napi]
    pub async fn connect(&self, address: String) -> Result<()> {
        send_and_recv(&self.tx, |reply| Command::Connect {
            addr: address,
            reply,
        })
        .await
    }

    #[napi]
    pub async fn request_challenge(&self, game_id: i64) -> Result<Challenge> {
        send_and_recv(&self.tx, |reply| Command::RequestChallenge {
            game_id: game_id as u64,
            reply,
        })
        .await
    }

    #[napi]
    pub async fn login(
        &self,
        game_id: i64,
        signature: Buffer,
        challenge_payload: Buffer,
    ) -> Result<()> {
        send_and_recv(&self.tx, |reply| Command::Login {
            game_id: game_id as u64,
            signature: signature.to_vec(),
            challenge: challenge_payload.to_vec(),
            reply,
        })
        .await
    }

    #[napi]
    pub async fn request_match(&self, game_id: String) -> Result<MatchAssignment> {
        send_and_recv(&self.tx, |reply| Command::RequestMatch {
            game_id: game_id.into_bytes(),
            reply,
        })
        .await
    }

    #[napi]
    pub async fn reconnect(&self, match_id: String) -> Result<()> {
        send_and_recv(&self.tx, |reply| Command::Reconnect {
            match_id: match_id.into_bytes(),
            reply,
        })
        .await
    }

    #[napi]
    pub async fn submit_outcome(
        &self,
        match_id: String,
        outcome: u8,
        transcript_hash: Buffer,
        signature: Buffer,
    ) -> Result<Buffer> {
        let sig: Vec<u8> = send_and_recv(&self.tx, |reply| Command::SubmitOutcome {
            match_id: match_id.into_bytes(),
            outcome,
            transcript_hash: transcript_hash.to_vec(),
            signature: signature.to_vec(),
            reply,
        })
        .await?;
        Ok(sig.into())
    }

    #[napi]
    pub async fn emit_game_event(&self, event_type: String) -> Result<()> {
        send_and_recv(&self.tx, |reply| Command::EmitGameEvent {
            event_type,
            reply,
        })
        .await
    }

    #[napi]
    pub async fn emit_telemetry(&self, event_type: u16) -> Result<()> {
        send_and_recv(&self.tx, |reply| Command::EmitTelemetry {
            event_type,
            reply,
        })
        .await
    }

    /// Begin forwarding server-pushed match events. After this resolves, call
    /// `pollEvent` in a loop to drain events.
    #[napi]
    pub async fn start_events(&self) -> Result<()> {
        send_and_recv(&self.tx, |reply| Command::StartEvents { reply }).await
    }

    /// Block (on the worker) for up to `timeout_ms` waiting for the next match
    /// event. Resolves with the event payload, or `null` on timeout / when the
    /// event stream ends.
    #[napi]
    pub async fn poll_event(&self, timeout_ms: u32) -> Result<Option<MatchEventPayload>> {
        send_and_recv(&self.tx, |reply| Command::PollEvent {
            timeout_ms: timeout_ms as u64,
            reply,
        })
        .await
    }

    #[napi]
    pub async fn close(&self) -> Result<()> {
        let (s, r) = oneshot::channel::<()>();
        let _ = self.tx.send(Command::Close { reply: s }).await;
        let _ = r.await;
        Ok(())
    }
}

/// Compute the canonical EIP-712 digest over (matchId, outcome, transcriptHash).
/// Byte-identical to the server and the other SDKs. Pure computation — no
/// worker thread needed.
#[napi]
pub fn compute_outcome_eip712_digest(
    match_id: String,
    outcome: u8,
    transcript_hash: Buffer,
    chain_id: i64,
    verifying_contract: Buffer,
) -> Result<Buffer> {
    let mut addr = [0u8; 20];
    let vb = verifying_contract.to_vec();
    if vb.len() != 20 {
        return Err(Error::from_reason(format!(
            "verifying_contract must be 20 bytes, got {}",
            vb.len()
        )));
    }
    addr.copy_from_slice(&vb);
    let digest = amp_sdk::compute_outcome_eip712_digest(
        &match_id,
        outcome,
        &transcript_hash.to_vec(),
        chain_id as u64,
        &addr,
    )
    .map_err(|e| Error::from_reason(format!("digest: {e}")))?;
    Ok(digest.to_vec().into())
}
