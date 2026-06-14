//! AMP Rust SDK — Cap'n Proto RPC client for the Avalanche Matchmaking Protocol.
//!
//! Threading model: capnp-rpc's `RpcSystem` is `!Send`, so the RPC loop
//! must run on a tokio `LocalSet`. The AMP client's [`AmpClient::connect`]
//! therefore must be called from inside a `LocalSet`. The easiest way is
//! to wrap your `main` with [`run_in_localset`]:
//!
//! # Example
//! ```no_run
//! use amp_sdk::{run_in_localset, AmpClient, MatchRequest};
//!
//! #[tokio::main]
//! async fn main() -> amp_sdk::AmpResult<()> {
//!     run_in_localset(async {
//!         let client = AmpClient::connect("127.0.0.1:50051").await?;
//!         let (challenge, expires_at) = client.request_challenge(1).await?;
//!         let signed = vec![0u8; 65]; // replace with real EIP-191 signature over `challenge`
//!         let session = client.login(1, &signed, &challenge).await?;
//!         let assigned = session.request_match(MatchRequest::new(b"chess-v1")).await?;
//!         println!("matched as {}", assigned.assignment.match_id_string());
//!         Ok(())
//!     }).await
//! }
//! ```

use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_types_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_core_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod match_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod player_profile_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/player_profile_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod matchmaking_rules_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/matchmaking_rules_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_registry_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/game_registry_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod amp_telemetry_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod relayer_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod inventory_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/inventory_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod tournament_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/tournament_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod security_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/security_capnp.rs"));
}
#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod service_capnp {
    #![allow(warnings)]
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}

// --------------------------------------------------------------------------- //
// Errors
// --------------------------------------------------------------------------- //
/// Errors returned by AMP SDK operations.
#[derive(Debug, thiserror::Error)]
pub enum AmpError {
    #[error("connection failed: {0}")]
    ConnectionFailed(String),
    #[error("authentication failed: {0}")]
    AuthFailed(String),
    #[error("match not found: {0}")]
    MatchNotFound(String),
    #[error("operation timed out after {0:?}")]
    Timeout(std::time::Duration),
    #[error("RPC error: {0}")]
    Rpc(#[from] capnp::Error),
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
}

/// Result type alias for AMP SDK operations.
pub type AmpResult<T> = Result<T, AmpError>;

// --------------------------------------------------------------------------- //
// Public DTOs
// --------------------------------------------------------------------------- //
/// Parameters for a matchmaking request.
#[derive(Clone)]
pub struct MatchRequest {
    /// Opaque game identifier bytes (e.g. b"chess-v1").
    pub game_id: Vec<u8>,
    /// Optional ruleset reference (e.g. b"standard-v2").
    pub ruleset_id: Option<Vec<u8>>,
    /// Rules variant tag (e.g. "standard").
    pub rules_type: String,
    /// Opaque optional configuration bytes.
    pub optional_config: Vec<u8>,
    /// Match type (turn-based or real-time).
    pub match_type: match_capnp::MatchType,
    /// Server-side matchmaking search timeout in milliseconds.
    pub timeout_ms: u64,
}

impl MatchRequest {
    /// Build a new request for the given game id with sensible defaults.
    pub fn new(game_id: &[u8]) -> Self {
        Self {
            game_id: game_id.to_vec(),
            ruleset_id: None,
            rules_type: "standard".to_string(),
            optional_config: Vec::new(),
            match_type: match_capnp::MatchType::TurnBased,
            timeout_ms: 60_000,
        }
    }
}

/// Parameters for submitting a match outcome.
#[derive(Clone)]
pub struct OutcomeSubmission {
    /// Opaque match identifier bytes.
    pub match_id: Vec<u8>,
    /// Type of outcome (win, draw, void...).
    pub outcome_type: match_capnp::OutcomeType,
    /// Per-player scores.
    pub scores: Vec<u64>,
    /// Index of the winning player (1..=N).
    pub victor: u8,
    /// Opaque metadata bytes.
    pub metadata: Vec<u8>,
    /// 32-byte keccak256 transcript/replay hash.
    pub replay_hash: Vec<u8>,
    /// 65-byte EIP-712 submitter signature over (matchId, outcome, replayHash).
    pub signature: Vec<u8>,
}

/// Verifier-assigned match data, returned by [`UserSession::request_match`].
///
/// The `match_id` is defensively copied out of the capnp message buffer so
/// it is safe to retain after the parent client is dropped.
#[derive(Clone, Debug)]
pub struct MatchAssignment {
    /// Authoritative match identifier (owned).
    pub match_id: Vec<u8>,
    /// Match-quality score in [0.0, 1.0].
    pub quality: f32,
}

impl MatchAssignment {
    /// Best-effort UTF-8 rendering of the match id (for logging/UI only).
    pub fn match_id_string(&self) -> String {
        String::from_utf8_lossy(&self.match_id).into_owned()
    }
}

/// Result of a successful matchmaking request: both the descriptive
/// [`MatchAssignment`] and the live [`MatchSession`] capability.
pub struct AssignedMatch {
    /// Descriptive match data (id, quality, opponents, ...).
    pub assignment: MatchAssignment,
    /// Live capability for further RPCs (submit outcome, telemetry, ...).
    pub session: MatchSession,
}

// --------------------------------------------------------------------------- //
// LocalSet helper
// --------------------------------------------------------------------------- //
/// Run an async block inside a tokio `LocalSet`.
///
/// Required because capnp-rpc's `RpcSystem` is `!Send` and must execute on
/// a single thread. Use this to wrap your `main` (or any task that calls
/// `AmpClient::connect`).
pub async fn run_in_localset<F, T>(future: F) -> T
where
    F: std::future::Future<Output = T>,
{
    let local_set = tokio::task::LocalSet::new();
    local_set.run_until(future).await
}

/// Returns true if the given challenge expiry (ns since epoch) is in the past.
/// Use this before signing a challenge to avoid burning a one-time nonce.
pub fn is_challenge_expired(expires_at_ns: u64) -> bool {
    let now = std::time::SystemTime::now()
        .duration_since(std::time::UNIX_EPOCH)
        .map(|d| d.as_nanos() as u64)
        .unwrap_or(0);
    expires_at_ns <= now
}

// --------------------------------------------------------------------------- //
// AmpClient
// --------------------------------------------------------------------------- //
/// Handle to an AMP server connection.
///
/// Capnp-rpc capabilities are `!Send`, so this handle is **not `Send`**.
/// It must be created and used on a single thread, typically via
/// [`run_in_localset`]. Cloning is supported (the underlying capability
/// is reference-counted) but clones must remain on the same thread.
pub struct AmpClient {
    client: service_capnp::game_session_service::Client,
}

impl AmpClient {
    /// Connect to an AMP matchmaker server at the given `addr` (host:port).
    ///
    /// **Must be called from inside a [`run_in_localset`] block** (or any
    /// other `tokio::task::LocalSet` context).
    pub async fn connect(addr: &str) -> AmpResult<Self> {
        Self::dial_plaintext(addr).await
    }

    /// Connect over plaintext TCP. See [`AmpClient::connect`].
    pub async fn dial_plaintext(addr: &str) -> AmpResult<Self> {
        let stream = tokio::net::TcpStream::connect(addr)
            .await
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
        stream
            .set_nodelay(true)
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
        Self::bootstrap(stream).await
    }

    /// Connect over TLS using a pre-built connector.
    ///
    /// `domain` is the SNI / certificate-expected hostname. Requires the
    /// `tls` feature.
    #[cfg(feature = "tls")]
    pub async fn dial_tls(
        addr: &str,
        domain: &str,
        connector: tokio_rustls::TlsConnector,
    ) -> AmpResult<Self> {
        let tcp = tokio::net::TcpStream::connect(addr)
            .await
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
        tcp.set_nodelay(true)
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
        let server_name = rustls::pki_types::ServerName::try_from(domain.to_string())
            .map_err(|e| AmpError::ConnectionFailed(format!("invalid SNI: {}", e)))?;
        let stream = connector
            .connect(server_name, tcp)
            .await
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
        Self::bootstrap(stream).await
    }

    async fn bootstrap<S>(stream: S) -> AmpResult<Self>
    where
        S: tokio::io::AsyncRead + tokio::io::AsyncWrite + 'static,
    {
        let (reader, writer) = tokio::io::split(stream);
        let network = capnp_rpc::twoparty::VatNetwork::new(
            reader.compat(),
            writer.compat_write(),
            capnp_rpc::rpc_twoparty_capnp::Side::Client,
            Default::default(),
        );
        let mut rpc_system = capnp_rpc::RpcSystem::new(Box::new(network), None);
        let client: service_capnp::game_session_service::Client =
            rpc_system.bootstrap(capnp_rpc::rpc_twoparty_capnp::Side::Server);
        // Drive the RPC system on a LocalSet task. The caller is responsible
        // for being inside a LocalSet (use run_in_localset).
        tokio::task::spawn_local(async move {
            if let Err(e) = rpc_system.await {
                tracing::warn!("RPC system error: {}", e);
            }
        });
        Ok(Self { client })
    }

    /// Request a one-time authentication challenge for the given `game_id`.
    ///
    /// Returns `(challenge_bytes, expires_at_ns)`. The caller SHOULD verify
    /// the challenge is not expired ([`is_challenge_expired`]) before signing.
    pub async fn request_challenge(&self, game_id: u64) -> AmpResult<(Vec<u8>, u64)> {
        let mut request = self.client.request_challenge_request();
        request.get().set_game_id(game_id);
        let response = request
            .send()
            .promise
            .await
            .map_err(|e| AmpError::AuthFailed(e.to_string()))?;
        let challenge = response.get()?.get_challenge()?.to_vec();
        let expires_at = response.get()?.get_expires_at();
        Ok((challenge, expires_at))
    }

    /// Authenticate with the server using a signed challenge.
    ///
    /// `signature` must be the 65-byte EIP-191 signature over the challenge
    /// bytes returned by [`request_challenge`](Self::request_challenge).
    /// `challenge_payload` is the raw challenge bytes that were signed.
    pub async fn login(
        &self,
        game_id: u64,
        signature: &[u8],
        challenge_payload: &[u8],
    ) -> AmpResult<UserSession> {
        if signature.len() != 65 {
            return Err(AmpError::AuthFailed(format!(
                "signature must be exactly 65 bytes (secp256k1 r||s||v), got {}",
                signature.len()
            )));
        }
        let mut request = self.client.login_request();
        request.get().set_game_id(game_id);
        request.get().set_signature(signature);
        request.get().set_challenge_payload(challenge_payload);
        let response = request
            .send()
            .promise
            .await
            .map_err(|e| AmpError::AuthFailed(e.to_string()))?;
        let session = response.get()?.get_session()?;
        Ok(UserSession { inner: session })
    }
}

// --------------------------------------------------------------------------- //
// UserSession
// --------------------------------------------------------------------------- //
/// An authenticated session for a single player. Obtain via `AmpClient::login`.
pub struct UserSession {
    inner: service_capnp::user_session::Client,
}

impl UserSession {
    /// Queue for a match using the given [`MatchRequest`].
    ///
    /// This call may block (return a pending future) until a suitable opponent
    /// is found. To avoid indefinite hangs, the SDK wraps the call in a
    /// `tokio::time::timeout` derived from `req.timeout_ms` (with 50% grace).
    pub async fn request_match(&self, req: MatchRequest) -> AmpResult<AssignedMatch> {
        let timeout = std::time::Duration::from_millis(req.timeout_ms.max(1));
        let timeout = timeout.mul_f32(1.5); // grace beyond the server's own timeout
        tokio::time::timeout(timeout, self.request_match_inner(req))
            .await
            .map_err(|_| AmpError::Timeout(timeout))?
    }

    async fn request_match_inner(&self, req: MatchRequest) -> AmpResult<AssignedMatch> {
        let mut request = self.inner.request_match_request();
        {
            let mut builder = request.get().init_req();
            builder.set_game_id(&req.game_id[..]);
            builder.set_rules_type(&req.rules_type[..]);
            builder.set_optional_config(&req.optional_config[..]);
            builder.set_match_type(req.match_type);
            builder.set_timeout_ms(req.timeout_ms);
            if let Some(rs) = &req.ruleset_id {
                builder.set_rule_set_id(rs);
            }
        }
        let response = request.send().promise.await?;
        let assignment_reader = response.get()?.get_assignment()?;
        // Defensive copy — the slice aliases the capnp answer buffer, which
        // is freed when `response` drops. Read everything we need before that.
        let match_id = assignment_reader.get_match_id()?.to_vec();
        let quality = assignment_reader.get_match_quality();
        let session = response.get()?.get_session()?;
        Ok(AssignedMatch {
            assignment: MatchAssignment { match_id, quality },
            session: MatchSession { inner: session },
        })
    }

    /// Reconnect to an existing active match by its `match_id`.
    ///
    /// Returns a [`MatchSession`] if the match is still active and unsettled.
    /// Use this when you have only the match_id (e.g. after a process restart)
    /// and don't need a fresh [`MatchAssignment`].
    pub async fn reconnect(&self, match_id: &[u8]) -> AmpResult<MatchSession> {
        let mut request = self.inner.reconnect_request();
        request.get().set_match_id(match_id);
        let response = request
            .send()
            .promise
            .await
            .map_err(|e| AmpError::MatchNotFound(e.to_string()))?;
        let session = response.get()?.get_session()?;
        Ok(MatchSession { inner: session })
    }
}

// --------------------------------------------------------------------------- //
// MatchSession
// --------------------------------------------------------------------------- //
/// An active match session for a single player.
pub struct MatchSession {
    inner: service_capnp::match_session::Client,
}

impl MatchSession {
    /// Submit the final outcome of the match.
    ///
    /// Returns the verifier's 65-byte EIP-712 signature attesting to the
    /// outcome. This signature is submitted on-chain by the relayer.
    pub async fn submit_outcome(&self, outcome: OutcomeSubmission) -> AmpResult<Vec<u8>> {
        if outcome.replay_hash.len() != 32 {
            return Err(AmpError::Rpc(capnp::Error::failed(format!(
                "replay_hash must be exactly 32 bytes, got {}",
                outcome.replay_hash.len()
            ))));
        }
        if outcome.signature.len() != 65 {
            return Err(AmpError::Rpc(capnp::Error::failed(format!(
                "submitter signature must be exactly 65 bytes, got {}",
                outcome.signature.len()
            ))));
        }
        if !(1..=4).contains(&outcome.victor) {
            return Err(AmpError::Rpc(capnp::Error::failed(format!(
                "victor must be 1..=4, got {}",
                outcome.victor
            ))));
        }
        let mut request = self.inner.submit_outcome_request();
        {
            let mut sub = request.get().init_submission();
            sub.set_match_id(&outcome.match_id[..]);
            sub.set_replay_hash(&outcome.replay_hash[..]);
            sub.set_signature(&outcome.signature[..]);
            {
                let mut out = sub.init_outcome();
                out.set_type(outcome.outcome_type);
                out.set_victor(outcome.victor);
                out.set_metadata(&outcome.metadata[..]);
                let mut scores = out.init_scores(outcome.scores.len() as u32);
                for (i, &s) in outcome.scores.iter().enumerate() {
                    scores.set(i as u32, s);
                }
            }
        }
        let response = request.send().promise.await?;
        let sig = response.get()?.get_signature()?;
        Ok(sig.to_vec())
    }

    /// Emit a telemetry event.
    ///
    /// `data` is an opaque payload forwarded to the telemetry service.
    /// The full `AmpTelemetryEvent` schema requires `matchId`, `gameId`,
    /// `eventType`, `timestamp`, `verifierId`, `eventData`; this helper
    /// populates `eventType` from `event_type_enum` and `timestamp` from
    /// the current time. Other fields default to empty/zero — wire up the
    /// full event if your server relies on them.
    pub async fn emit_telemetry(&self, event_type_enum: u16, data: &[u8]) -> AmpResult<()> {
        let mut request = self.inner.emit_telemetry_request();
        {
            let mut event = request.get().init_event();
            event.set_event_data(data);
            // The schema has a dedicated TelemetryEventType enum; event_type_enum
            // is forwarded as-is once we wire that enum in. For now we set
            // timestamp + data only.
            let _ = event_type_enum;
            let now_ns = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .map(|d| d.as_nanos() as u64)
                .unwrap_or(0);
            event.set_timestamp(now_ns);
        }
        // Fire-and-forget: drop the returned RemotePromise to avoid blocking
        // the caller on telemetry acknowledgement.
        drop(request.send());
        Ok(())
    }

    /// Emit an in-game event.
    pub async fn emit_game_event(&self, event_type: &str, data: &[u8]) -> AmpResult<()> {
        let mut request = self.inner.emit_game_event_request();
        {
            let mut event = request.get().init_event();
            event.set_event_type(event_type);
            event.set_event_data(data);
            let now_ns = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .map(|d| d.as_nanos() as u64)
                .unwrap_or(0);
            event.set_timestamp(now_ns);
        }
        // Fire-and-forget: drop the returned RemotePromise to avoid blocking
        // the caller on telemetry acknowledgement. Network errors are logged
        // by capnp-rpc internally; callers cannot observe them.
        let _ = request.send();
        Ok(())
    }
}

// --------------------------------------------------------------------------- //
// Tests
// --------------------------------------------------------------------------- //
#[cfg(test)]
mod tests {
    use super::*;

    #[test]
    fn challenge_expiry_helper() {
        let now = std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)
            .unwrap()
            .as_nanos() as u64;
        assert!(is_challenge_expired(now));
        assert!(is_challenge_expired(now - 1));
        assert!(!is_challenge_expired(now + 60_000_000_000));
    }

    #[test]
    fn match_request_builder_defaults() {
        let r = MatchRequest::new(b"chess-v1");
        assert_eq!(r.game_id, b"chess-v1");
        assert_eq!(r.rules_type, "standard");
        assert_eq!(r.timeout_ms, 60_000);
        assert!(r.ruleset_id.is_none());
    }

    #[test]
    fn match_assignment_string_lossy() {
        let a = MatchAssignment {
            match_id: b"abc-123".to_vec(),
            quality: 0.5,
        };
        assert_eq!(a.match_id_string(), "abc-123");
    }
}
