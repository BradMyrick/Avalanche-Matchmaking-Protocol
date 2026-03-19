// use amp_telemetry::{self, TelemetryConfig};
use anyhow::Result;
use capnp::capability::Promise;
use capnp_rpc::{RpcSystem, pry, rpc_twoparty_capnp, twoparty};
use ethers_core::types::H256;
use ethers_core::utils::hash_message;
use ethers_signers::{LocalWallet, Signer};
use futures_util::AsyncReadExt;
use lazy_static::lazy_static;
use std::env;
use std::sync::Arc;
use tokio::sync::{Mutex, oneshot};
use tracing::info;
use serde::Serialize;

use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use uuid::Uuid;

pub mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
pub mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
pub mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}
pub mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
pub mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}

use match_capnp::*;
use service_capnp::*;

lazy_static! {
    static ref MATCH_QUEUE: Arc<Mutex<Vec<QueueEntry>>> = Arc::new(Mutex::new(Vec::new()));
}

struct QueueEntry {
    player_id: String,
    game_id: String,
    sender: oneshot::Sender<(String, String)>,
}

async fn verify_and_sign(_match_id: &str, outcome: u8, transcript_hash: &[u8]) -> Result<Vec<u8>> {
    let key = env::var("VERIFIER_KEY")
        .unwrap_or_else(|_| "0123456789TEST0123456789TEST0123456789TEST0123456789TEST".to_string());
    let wallet: LocalWallet = key.parse()?;

    let t_hash = if transcript_hash.is_empty() {
        H256::zero()
    } else {
        H256::from_slice(&transcript_hash[..32])
    };

    let match_id_val = ethers_core::types::U256::zero();

    let encoded = ethers_core::abi::encode(&[
        ethers_core::abi::Token::Uint(match_id_val),
        ethers_core::abi::Token::Uint(ethers_core::types::U256::from(outcome)),
        ethers_core::abi::Token::FixedBytes(t_hash.as_bytes().to_vec()),
    ]);
    let struct_hash = ethers_core::utils::keccak256(&encoded);
    let digest = hash_message(struct_hash);

    let signature = wallet.sign_hash(H256::from(digest))?;
    Ok(signature.to_vec())
}

#[derive(Serialize)]
struct RelayPayload {
    match_id: String,
    outcome: u8,
    transcript_hash: String,
    signature: String,
}

async fn notify_relayer(match_id: &str, outcome: u8, transcript_hash: &[u8], signature: &[u8]) {
    let relayer_url = env::var("RELAYER_URL").unwrap_or_else(|_| "http://localhost:3000".to_string());
    let payload = RelayPayload {
        match_id: match_id.to_string(),
        outcome,
        transcript_hash: ethers_core::utils::hex::encode(transcript_hash),
        signature: ethers_core::utils::hex::encode(signature),
    };
    
    let client = reqwest::Client::new();
    match client.post(format!("{}/submit-outcome", relayer_url))
        .json(&payload)
        .send()
        .await 
    {
        Ok(res) => {
            if !res.status().is_success() {
                tracing::error!("Relayer returned error: {:?}", res.status());
            } else {
                tracing::info!("Successfully queued outcome to relayer for {}", match_id);
            }
        }
        Err(e) => tracing::error!("Failed to notify relayer: {:?}", e),
    }
}

struct MatchSessionImpl {
    match_id: String,
    _player_id: String,
}

impl match_session::Server for MatchSessionImpl {
    fn submit_outcome(
        &mut self,
        params: match_session::SubmitOutcomeParams,
        mut results: match_session::SubmitOutcomeResults,
    ) -> Promise<(), ::capnp::Error> {
        let submission = capnp_rpc::pry!(capnp_rpc::pry!(params.get()).get_submission());

        let match_id_bytes = capnp_rpc::pry!(submission.get_match_id());
        let match_id = String::from_utf8_lossy(match_id_bytes).to_string();
        let outcome = capnp_rpc::pry!(submission.get_outcome());
        let outcome_val = outcome.get_victor();

        let replay_hash = capnp_rpc::pry!(submission.get_replay_hash());

        let r_hash = replay_hash.to_vec();

        Promise::from_future(async move {
            match verify_and_sign(&match_id, outcome_val, &r_hash).await {
                Ok(sig) => {
                    results.get().set_signature(&sig);
                    
                    // Spawn task to notify relayer in the background
                    let m_id = match_id.clone();
                    tokio::spawn(async move {
                        notify_relayer(&m_id, outcome_val, &r_hash, &sig).await;
                    });
                    
                    Ok(())
                }
                Err(e) => Err(::capnp::Error::failed(format!("Verifier error: {}", e))),
            }
        })
    }

    fn subscribe_to_events(
        &mut self,
        _params: match_session::SubscribeToEventsParams,
        _results: match_session::SubscribeToEventsResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }

    fn emit_game_event(
        &mut self,
        _params: match_session::EmitGameEventParams,
        _results: match_session::EmitGameEventResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }

    fn emit_telemetry(
        &mut self,
        _params: match_session::EmitTelemetryParams,
        _results: match_session::EmitTelemetryResults,
    ) -> Promise<(), ::capnp::Error> {
        Promise::ok(())
    }
}

struct UserSessionImpl {
    player_id: String,
}

impl user_session::Server for UserSessionImpl {
    fn request_match(
        &mut self,
        params: user_session::RequestMatchParams,
        mut results: user_session::RequestMatchResults,
    ) -> Promise<(), ::capnp::Error> {
        let req = capnp_rpc::pry!(capnp_rpc::pry!(params.get()).get_req());
        let game_id_bytes = capnp_rpc::pry!(req.get_game_id());
        let game_id = String::from_utf8_lossy(game_id_bytes).to_string();
        let player_id_clone = self.player_id.clone();

        Promise::from_future(async move {
            let (tx, rx) = oneshot::channel();
            let mut queue = MATCH_QUEUE.lock().await;

            if let Some(pos) = queue.iter().position(|e| e.game_id == game_id) {
                let entry = queue.remove(pos);
                let match_id = format!("match-{}", Uuid::new_v4());

                let _ = entry
                    .sender
                    .send((match_id.clone(), player_id_clone.clone()));

                let mut assignment = results.get().init_assignment();
                assignment.set_match_id(match_id.as_bytes());

                let session = capnp_rpc::new_client(MatchSessionImpl {
                    match_id,
                    _player_id: player_id_clone,
                });
                results.get().set_session(session);

                Ok(())
            } else {
                queue.push(QueueEntry {
                    player_id: player_id_clone.clone(),
                    game_id: game_id.clone(),
                    sender: tx,
                });
                drop(queue);
                println!("Player {} queued for game {}", player_id_clone, game_id);

                if let Ok((assign_match_id, _)) = rx.await {
                    let mut assignment = results.get().init_assignment();
                    assignment.set_match_id(assign_match_id.as_bytes());

                    let session = capnp_rpc::new_client(MatchSessionImpl {
                        match_id: assign_match_id,
                        _player_id: player_id_clone,
                    });
                    results.get().set_session(session);

                    Ok(())
                } else {
                    Err(::capnp::Error::failed("Matchmaking cancelled".to_string()))
                }
            }
        })
    }

    fn reconnect(
        &mut self,
        params: user_session::ReconnectParams,
        mut results: user_session::ReconnectResults,
    ) -> Promise<(), ::capnp::Error> {
        let match_id_bytes = capnp_rpc::pry!(capnp_rpc::pry!(params.get()).get_match_id());
        let match_id = String::from_utf8_lossy(match_id_bytes).to_string();
        let player_id_clone = self.player_id.clone();

        let session = capnp_rpc::new_client(MatchSessionImpl {
            match_id,
            _player_id: player_id_clone,
        });
        results.get().set_session(session);

        Promise::ok(())
    }
}

struct GameSessionServiceImpl;

impl game_session_service::Server for GameSessionServiceImpl {
    fn login(
        &mut self,
        params: game_session_service::LoginParams,
        mut results: game_session_service::LoginResults,
    ) -> Promise<(), ::capnp::Error> {
        let sig = capnp_rpc::pry!(capnp_rpc::pry!(params.get()).get_signed_challenge());
        let player_id = String::from_utf8_lossy(sig).to_string();
        println!("Player {} logged in", player_id);

        let session = capnp_rpc::new_client(UserSessionImpl { player_id });
        results.get().set_session(session);

        Promise::ok(())
    }
}

#[tokio::main]
async fn main() -> Result<()> {
    dotenv::dotenv().ok();

    //TODO: check this
    // Telemetry relies on the MatchSession capnp interface now.

    let addr = env::var("AMP_ADDR").unwrap_or_else(|_| "0.0.0.0:50051".to_string());
    let listener = tokio::net::TcpListener::bind(&addr).await?;
    println!("AMP WS Matchmaker RPC starting on {}", addr);

    let local = tokio::task::LocalSet::new();

    local
        .run_until(async move {
            while let Ok((stream, _)) = listener.accept().await {
                tokio::task::spawn_local(async move {
                    let (read_half, write_half) = stream.into_split();

                    let network = twoparty::VatNetwork::new(
                        read_half.compat(),
                        write_half.compat_write(),
                        rpc_twoparty_capnp::Side::Server,
                        Default::default(),
                    );

                    let service: game_session_service::Client =
                        capnp_rpc::new_client(GameSessionServiceImpl);
                    let rpc_system = RpcSystem::new(Box::new(network), Some(service.client));

                    if let Err(e) = rpc_system.await {
                        println!("RPC error: {}", e);
                    }
                    // Capnp telemetry handled via events.
                    info!("processed message from player");

                    // Shutdown handler
                });
            }
        })
        .await;

    Ok(())
}
