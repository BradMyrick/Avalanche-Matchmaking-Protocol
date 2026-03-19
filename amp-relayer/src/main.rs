use std::env;
use std::net::SocketAddr;
use std::sync::Arc;

use anyhow::Result;
use ethers::prelude::*;
use serde::{Deserialize, Serialize};
use tracing::{info, error};

abigen!(
    AMPSettlement,
    "../contracts/out/AMPSettlement.sol/AMPSettlement.json"
);
use warp::{Filter, Reply, Rejection};

#[derive(Debug, Deserialize)]
struct SubmitOutcomeRequest {
    match_id: String,
    outcome: u8,
    transcript_hash: String,
    signature: String,
}

#[derive(Debug, Serialize)]
struct SubmitOutcomeResponse {
    tx_hash: String,
    status: String,
    match_id: String,
}

#[derive(Clone)]
struct AppState {
    settlement: AMPSettlement<SignerMiddleware<Provider<Http>, LocalWallet>>,
}

async fn handle_submit_outcome(
    req: SubmitOutcomeRequest,
    state: Arc<AppState>,
) -> Result<impl Reply, Rejection> {
    info!("Received submit-outcome request for match {}", req.match_id);

    let match_id_parsed = if req.match_id.starts_with("match-") {
        // match- uuid handling: we just hash it to uint256 if needed?
        // Wait, the client is expected to pass a string that parses to U256 or we derive it
        // The MVP probably uses a numeric matchId or we hash the UUID string
        U256::from_big_endian(&ethers::utils::keccak256(req.match_id.as_bytes()))
    } else {
        U256::from_dec_str(&req.match_id).unwrap_or(U256::zero())
    };

    let transcript_hash_bytes = hex::decode(&req.transcript_hash).unwrap_or_default();
    let signature_bytes = hex::decode(&req.signature).unwrap_or_default();
    
    let mut t_hash = [0u8; 32];
    if transcript_hash_bytes.len() == 32 {
        t_hash.copy_from_slice(&transcript_hash_bytes);
    }

    let result = AsyncResult {
        match_id: match_id_parsed,
        outcome: req.outcome,
        transcript_hash: t_hash,
        signature: Bytes::from(signature_bytes),
    };

    match state.settlement.submit_async_result(match_id_parsed, result).send().await {
        Ok(pending_tx) => {
            let tx_hash = format!("{:?}", pending_tx.tx_hash());
            info!("Submitted tx {} for match {}", tx_hash, req.match_id);
            // wait for confirmation? We can return "pending"
            Ok(warp::reply::json(&SubmitOutcomeResponse {
                tx_hash,
                status: "submitted".to_string(),
                match_id: req.match_id,
            }))
        }
        Err(e) => {
            error!("Failed to submit tx: {:?}", e);
            Ok(warp::reply::json(&SubmitOutcomeResponse {
                tx_hash: "".to_string(),
                status: format!("error: {:?}", e),
                match_id: req.match_id,
            }))
        }
    }
}

async fn handle_health() -> Result<impl Reply, Rejection> {
    Ok(warp::reply::with_status("OK", warp::http::StatusCode::OK))
}

#[tokio::main]
async fn main() -> Result<()> {
    // Initialize logging
    tracing_subscriber::fmt::init();

    dotenv::dotenv().ok();

    let port = env::var("PORT").unwrap_or_else(|_| "3000".to_string());
    let addr: SocketAddr = format!("0.0.0.0:{}", port)
        .parse()
        .expect("Invalid port");

    let fuji_rpc = env::var("FUJI_RPC_URL").unwrap_or_else(|_| "https://api.avax-test.network/ext/bc/C/rpc".to_string());
    let provider = Provider::<Http>::try_from(fuji_rpc.clone()).expect("Failed creating provider");

    let relayer_key = env::var("RELAYER_PRIVATE_KEY").unwrap_or_else(|_| "0123456789abcdef0123456789abcdef0123456789abcdef0123456789abcdef".to_string());
    let wallet: LocalWallet = relayer_key.parse().expect("Invalid private key");
    let chain_id = provider.get_chainid().await.unwrap_or(U256::from(43113)).as_u64();
    let wallet = wallet.with_chain_id(chain_id);

    let client = Arc::new(SignerMiddleware::new(provider, wallet));

    let settlement_address = env::var("CONTRACT_SETTLEMENT")
        .unwrap_or_else(|_| "0xecD9C6C1727d610A7C0Aeb3a37A6278049791a24".to_string())
        .parse::<Address>()
        .expect("Invalid address");

    let settlement = AMPSettlement::new(settlement_address, client);

    let state = Arc::new(AppState { settlement });

    let state_filter = warp::any().map(move || Arc::clone(&state));

    let submit_outcome = warp::path!("submit-outcome")
        .and(warp::post())
        .and(warp::body::json())
        .and(state_filter.clone())
        .and_then(handle_submit_outcome);

    let health = warp::path!("health")
        .and(warp::get())
        .and_then(handle_health);

    let routes = submit_outcome
        .or(health)
        .with(warp::cors().allow_any_origin());

    info!("AMP Relayer starting on {}", addr);
    warp::serve(routes).run(addr).await;

    Ok(())
}