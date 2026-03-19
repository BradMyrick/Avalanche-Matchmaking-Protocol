use std::env;
use std::net::SocketAddr;
use std::sync::Arc;

use anyhow::Result;
use ethers::prelude::*;
use serde::{Deserialize, Serialize};
use tracing::{info, error};

abigen!(
    AMPSettlementContract,
    "../contracts/out/AMPSettlement.sol/AMPSettlement.json"
);

abigen!(
    AMPRegistryContract,
    "../contracts/out/AMPRegistry.sol/AMPRegistry.json"
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
    settlement: AMPSettlementContract<SignerMiddleware<Provider<Http>, LocalWallet>>,
    registry: AMPRegistryContract<SignerMiddleware<Provider<Http>, LocalWallet>>,
}

/// Derives a custodial wallet for a specific game_id from the master key.
fn derive_custodial_signer(master_key: &LocalWallet, game_id: u64, chain_id: u64) -> LocalWallet {
    let mut bytes = [0u8; 32];
    bytes.copy_from_slice(&master_key.signer().to_bytes());
    
    // Hash the master key with game_id to derive a unique child key
    let mut data = Vec::with_capacity(40);
    data.extend_from_slice(&bytes);
    data.extend_from_slice(&game_id.to_be_bytes());
    
    let derived_key = ethers::utils::keccak256(data);
    LocalWallet::from_bytes(&derived_key).unwrap().with_chain_id(chain_id)
}

/// Ensures the custodial wallet has enough gas, topping up from the master if needed.
async fn ensure_gas(
    custodial_addr: Address,
    state: &AppState,
    master_client: &SignerMiddleware<Provider<Http>, LocalWallet>,
) -> Result<()> {
    let balance = state.settlement.client().get_balance(custodial_addr, None).await?;
    let threshold = ethers::utils::parse_ether(0.05)?; // 0.05 AVAX threshold
    
    if balance < threshold {
        let topup = ethers::utils::parse_ether(0.2)?; // Top up with 0.2 AVAX
        info!("Custodial wallet {} low on gas ({:?}). Topping up...", custodial_addr, balance);
        
        let tx = TransactionRequest::new()
            .to(custodial_addr)
            .value(topup);
            
        master_client.send_transaction(tx, None).await?.await?;
        info!("Top-up successful for {}", custodial_addr);
    }
    
    Ok(())
}

async fn handle_submit_outcome(
    req: SubmitOutcomeRequest,
    state: Arc<AppState>,
    master_client: Arc<SignerMiddleware<Provider<Http>, LocalWallet>>,
) -> Result<impl Reply, Rejection> {
    info!("Received submit-outcome request for match {}", req.match_id);

    let match_id_parsed = if req.match_id.starts_with("match-") {
        U256::from_big_endian(&ethers::utils::keccak256(req.match_id.as_bytes()))
    } else {
        U256::from_dec_str(&req.match_id).unwrap_or(U256::zero())
    };

    // 1. Resolve game_id for this match
    let (game_id, _, _, _, _, _) = match state.registry.matches(match_id_parsed).call().await {
        Ok(m) => m,
        Err(e) => {
            error!("Failed to fetch match info: {:?}", e);
            return Ok(warp::reply::json(&SubmitOutcomeResponse {
                tx_hash: "".to_string(),
                status: format!("error fetching match: {:?}", e),
                match_id: req.match_id,
            }));
        }
    };

    // 2. Get custodial signer
    let chain_id = master_client.signer().chain_id();
    let custodial_wallet = derive_custodial_signer(master_client.signer(), game_id.as_u64(), chain_id);
    let custodial_addr = custodial_wallet.address();

    // 3. Ensure gas for custodial wallet
    if let Err(e) = ensure_gas(custodial_addr, &state, &master_client).await {
        error!("Gas funding failed: {:?}", e);
    }

    // 4. Create client for custodial signer
    let provider = master_client.provider().clone();
    let custodial_client = SignerMiddleware::new(provider, custodial_wallet);
    let settlement_custodial = AMPSettlementContract::new(state.settlement.address(), Arc::new(custodial_client));

    let transcript_hash_clean = req.transcript_hash.trim_start_matches("0x");
    let signature_clean = req.signature.trim_start_matches("0x");

    let transcript_hash_bytes = hex::decode(transcript_hash_clean).unwrap_or_default();
    let signature_bytes = hex::decode(signature_clean).unwrap_or_default();
    
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

    info!("Using custodial wallet {} for game_id {}", custodial_addr, game_id);

    match settlement_custodial.submit_async_result(match_id_parsed, result).send().await {
        Ok(pending_tx) => {
            let tx_hash = format!("{:?}", pending_tx.tx_hash());
            info!("Submitted tx {} for match {}", tx_hash, req.match_id);
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

async fn handle_get_custodial_address(
    game_id: u64,
    master_client: Arc<SignerMiddleware<Provider<Http>, LocalWallet>>,
) -> Result<impl Reply, Rejection> {
    let chain_id = master_client.signer().chain_id();
    let wallet = derive_custodial_signer(master_client.signer(), game_id, chain_id);
    Ok(warp::reply::json(&serde_json::json!({
        "game_id": game_id,
        "custodial_address": format!("{:?}", wallet.address())
    })))
}

async fn handle_get_game_admin(
    game_id: u64,
    state: Arc<AppState>,
) -> Result<impl Reply, Rejection> {
    info!("Querying admin for game {}", game_id);
    
    // Call the AMPRegistry contract (linked in state.settlement?)
    // Wait, state.settlement is AMPSettlement. I need AMPRegistry.
    // I specify the registry in the deployment, but AMPSettlement has a reference to it?
    // Let's check AMPSettlement.sol or just add registry to AppState.
    
    // Actually, I can use the registry address from env.
    let registry_addr = env::var("CONTRACT_REGISTRY")
        .unwrap_or_else(|_| "0x0000000000000000000000000000000000000000".to_string())
        .parse::<Address>()
        .expect("Invalid registry address");
    
    // We need the registry contract abigen or just call it manually.
    // For simplicity, let's assume we can get it from the settlement contract if it's there,
    // or just abigen it here too.
    
    // I'll add the registry to AppState and main.
    
    match state.registry.games(U256::from(game_id)).call().await {
        Ok(game_data) => {
            let admin_addr = format!("{:?}", game_data.0); // games(id).admin is the first return val
            Ok(warp::reply::json(&serde_json::json!({
                "game_id": game_id,
                "admin": admin_addr
            })))
        }
        Err(e) => {
            error!("Failed to query registry: {:?}", e);
            Ok(warp::reply::json(&serde_json::json!({
                "error": format!("{:?}", e)
            })))
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
 
    let fuji_rpc = env::var("FUJI_RPC_URL").unwrap_or_else(|_| "http://localhost:8545".to_string());
    let provider = Provider::<Http>::try_from(fuji_rpc.clone()).expect("Failed creating provider");
 
    let relayer_key = env::var("RELAYER_PRIVATE_KEY").unwrap_or_else(|_| "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80".to_string());
    let wallet: LocalWallet = relayer_key.parse().expect("Invalid private key");
    let chain_id = provider.get_chainid().await.unwrap_or(U256::from(31337)).as_u64();
    let wallet = wallet.with_chain_id(chain_id);
 
    let master_client = Arc::new(SignerMiddleware::new(provider, wallet));
 
    let settlement_address = env::var("CONTRACT_SETTLEMENT")
        .unwrap_or_else(|_| "0x0000000000000000000000000000000000000000".to_string())
        .parse::<Address>()
        .expect("Invalid settlement address");
    
    let registry_address = env::var("CONTRACT_REGISTRY")
        .unwrap_or_else(|_| "0x0000000000000000000000000000000000000000".to_string())
        .parse::<Address>()
        .expect("Invalid registry address");
 
    let settlement = AMPSettlementContract::new(settlement_address, master_client.clone());
    let registry = AMPRegistryContract::new(registry_address, master_client.clone());
 
    let state = Arc::new(AppState { settlement, registry });
 
    let state_filter = warp::any().map(move || Arc::clone(&state));
    let master_client_filter = warp::any().map(move || Arc::clone(&master_client));
 
    let submit_outcome = warp::path!("submit-outcome")
        .and(warp::post())
        .and(warp::body::json())
        .and(state_filter.clone())
        .and(master_client_filter.clone())
        .and_then(handle_submit_outcome);
 
    let get_custodial_address = warp::path!("custodial-address" / u64)
        .and(warp::get())
        .and(master_client_filter.clone())
        .and_then(handle_get_custodial_address);

    let get_game_admin = warp::path!("game-admin" / u64)
        .and(warp::get())
        .and(state_filter.clone())
        .and_then(handle_get_game_admin);
 
    let health = warp::path!("health")
        .and(warp::get())
        .and_then(handle_health);
 
    let routes = submit_outcome
        .or(get_custodial_address)
        .or(get_game_admin)
        .or(health)
        .with(warp::cors().allow_any_origin());
 
    info!("AMP Relayer starting on {}", addr);
    warp::serve(routes).run(addr).await;
 
    Ok(())
}