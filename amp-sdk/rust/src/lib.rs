use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod game_core_capnp {
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
pub mod inventory_capnp {
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
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}

/// Errors returned by AMP SDK operations.
#[derive(Debug, thiserror::Error)]
pub enum AmpError {
    #[error("connection failed: {0}")]
    ConnectionFailed(String),
    #[error("authentication failed: {0}")]
    AuthFailed(String),
    #[error("match not found: {0}")]
    MatchNotFound(String),
    #[error("timeout")]
    Timeout,
    #[error("RPC error: {0}")]
    Rpc(#[from] capnp::Error),
    #[error("IO error: {0}")]
    Io(#[from] std::io::Error),
}

/// Result type alias for AMP SDK operations.
pub type AmpResult<T> = Result<T, AmpError>;

/// Parameters for a matchmaking request.
pub struct MatchRequest {
    /// Opaque game identifier bytes.
    pub game_id: Vec<u8>,
    /// Rules type string (e.g. "standard").
    pub rules_type: String,
    /// Opaque optional configuration bytes.
    pub optional_config: Vec<u8>,
    /// Match type (ranked, casual, etc.).
    pub match_type: match_capnp::MatchType,
    /// Timeout in milliseconds for the matchmaking search.
    pub timeout_ms: u64,
}

/// Parameters for submitting a match outcome.
pub struct OutcomeSubmission {
    /// Opaque match identifier bytes.
    pub match_id: Vec<u8>,
    /// Type of outcome (win, loss, draw, cancelled).
    pub outcome_type: match_capnp::OutcomeType,
    /// Per-player scores.
    pub scores: Vec<u64>,
    /// Index of the winning player (1 or 2, 0 for draw).
    pub victor: u8,
    /// Opaque metadata bytes.
    pub metadata: Vec<u8>,
    /// SHA-256 hash of the match transcript/replay.
    pub replay_hash: Vec<u8>,
    /// Player signature over the outcome.
    pub signature: Vec<u8>,
}

/// Top-level client for connecting to an AMP matchmaker server.
///
/// Connect over TCP using Cap'n Proto RPC, then call [`request_challenge`]
/// followed by [`login`] to authenticate before using session methods.
///
/// # Example
/// ```ignore
/// use amp_sdk::AmpClient;
/// let client = AmpClient::connect("127.0.0.1:50051").await?;
/// let (challenge, _expires) = client.request_challenge(1).await?;
/// // sign challenge with your wallet, then:
/// let session = client.login(1, &signature).await?;
/// ```
pub struct AmpClient {
    client: service_capnp::game_session_service::Client,
}

impl AmpClient {
    /// Connect to an AMP matchmaker server at the given `addr` (host:port).
    pub async fn connect(addr: &str) -> AmpResult<Self> {
        let stream = tokio::net::TcpStream::connect(addr)
            .await
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
        stream
            .set_nodelay(true)
            .map_err(|e| AmpError::ConnectionFailed(e.to_string()))?;
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
        tokio::task::spawn_local(async move {
            let _ = rpc_system.await;
        });
        Ok(Self { client })
    }

    /// Request a one-time authentication challenge for the given `game_id`.
    ///
    /// Returns `(challenge_bytes, expires_at_ns)` where `challenge_bytes` must be
    /// signed by the player's Ethereum wallet. The signed bytes are then passed to
    /// [`login`](Self::login).
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
    /// `signature` must be the 65-byte ECDSA signature over the challenge bytes
    /// returned by [`request_challenge`](Self::request_challenge), with the
    /// challenge bytes appended after the 65-byte secp256k1 signature.
    ///
    /// On success, returns a [`UserSession`] for matchmaking operations.
    pub async fn login(&self, game_id: u64, signature: &[u8]) -> AmpResult<UserSession> {
        let mut request = self.client.login_request();
        request.get().set_game_id(game_id);
        request.get().set_signed_challenge(signature);
        let response = request
            .send()
            .promise
            .await
            .map_err(|e| AmpError::AuthFailed(e.to_string()))?;
        let session = response.get()?.get_session()?;
        Ok(UserSession { inner: session })
    }
}

/// An authenticated session for a single player.
///
/// Obtain via [`AmpClient::login`]. Use this to request matches or reconnect
/// to an existing match.
pub struct UserSession {
    inner: service_capnp::user_session::Client,
}

impl UserSession {
    /// Queue for a match using the given [`MatchRequest`].
    ///
    /// This call blocks (returns a pending future) until a suitable opponent
    /// is found and a match is created. On success, returns a [`MatchSession`]
    /// for interacting with the match.
    pub async fn request_match(&self, req: MatchRequest) -> AmpResult<MatchSession> {
        let mut request = self.inner.request_match_request();
        {
            let mut builder = request.get().init_req();
            builder.set_game_id(&req.game_id[..]);
            builder.set_rules_type(&req.rules_type[..]);
            builder.set_optional_config(&req.optional_config[..]);
            builder.set_match_type(req.match_type);
            builder.set_timeout_ms(req.timeout_ms);
        }
        let response = request.send().promise.await?;
        let _assignment = response.get()?.get_assignment()?;
        let session = response.get()?.get_session()?;
        Ok(MatchSession { inner: session })
    }

    /// Reconnect to an existing active match by its `match_id`.
    ///
    /// Returns a [`MatchSession`] if the match is still active and unsettled.
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

/// An active match session for a single player.
///
/// Obtain via [`UserSession::request_match`] or [`UserSession::reconnect`].
/// Use this to submit outcomes, emit telemetry, or send game events.
pub struct MatchSession {
    inner: service_capnp::match_session::Client,
}

impl MatchSession {
    /// Submit the final outcome of the match.
    ///
    /// Returns the verifier's 65-byte ECDSA signature attesting to the outcome.
    /// This signature is submitted on-chain by the relayer for settlement.
    pub async fn submit_outcome(&self, outcome: OutcomeSubmission) -> AmpResult<Vec<u8>> {
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

    /// Emit a telemetry event (e.g. ping, fps, connection quality).
    ///
    /// `data` is an opaque payload forwarded to the telemetry service.
    pub async fn emit_telemetry(&self, data: &[u8]) -> AmpResult<()> {
        let mut request = self.inner.emit_telemetry_request();
        request.get().init_event().set_event_data(data);
        request.send().promise.await?;
        Ok(())
    }

    /// Emit an in-game event (e.g. player action, state change).
    ///
    /// `event_type` is a human-readable event label.
    /// `data` is an opaque payload describing the event.
    pub async fn emit_game_event(&self, event_type: &str, data: &[u8]) -> AmpResult<()> {
        let mut request = self.inner.emit_game_event_request();
        {
            let mut event = request.get().init_event();
            event.set_event_type(event_type);
            event.set_event_data(data);
        }
        request.send().promise.await?;
        Ok(())
    }
}
