use std::env;
use std::sync::Arc;
use std::net::SocketAddr;

use anyhow::Result;
use ethers::prelude::*;
use tracing::{info, error};
use capnp_rpc::{rpc_twoparty_capnp, twoparty, RpcSystem};
use futures::{FutureExt, StreamExt};
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

/// Auto-generated modules from Cap'n Proto schema
pub mod relayer_capnp {
    include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs"));
}

abigen!(
    AMPSettlementContract,
    "../contracts/out/AMPSettlement.sol/AMPSettlement.json"
);

abigen!(
    AMPRegistryContract,
    "../contracts/out/AMPRegistry.sol/AMPRegistry.json"
);

/// State shared across RPC requests.
#[derive(Clone)]
struct RelayerState {
    settlement: AMPSettlementContract<SignerMiddleware<Provider<Http>, LocalWallet>>,
    registry: AMPRegistryContract<SignerMiddleware<Provider<Http>, LocalWallet>>,
    master_client: Arc<SignerMiddleware<Provider<Http>, LocalWallet>>,
}
// RelayerState

/// Derives a custodial wallet for a specific game_id from the master key.
fn derive_custodial_signer(master_key: &LocalWallet, game_id: u64, chain_id: u64) -> LocalWallet {
    let mut bytes = [0u8; 32];
    bytes.copy_from_slice(&master_key.signer().to_bytes());
    
    let mut data = Vec::with_capacity(40);
    data.extend_from_slice(&bytes);
    data.extend_from_slice(&game_id.to_be_bytes());
    
    let derived_key = ethers::utils::keccak256(data);
    LocalWallet::from_bytes(&derived_key).unwrap().with_chain_id(chain_id)
}
// derive_custodial_signer(master_key, game_id, chain_id)

/// Ensures the custodial wallet has enough gas, topping up from the master if needed.
async fn ensure_gas(
    custodial_addr: Address,
    state: &RelayerState,
) -> Result<()> {
    let balance = state.master_client.provider().get_balance(custodial_addr, None).await?;
    let threshold = ethers::utils::parse_ether(0.05)?; 
    
    if balance < threshold {
        let topup = ethers::utils::parse_ether(0.2)?; 
        info!("Custodial wallet {} low on gas ({:?}). Topping up...", custodial_addr, balance);
        
        let tx = TransactionRequest::new()
            .to(custodial_addr)
            .value(topup);
            
        state.master_client.send_transaction(tx, None).await?.await?;
        info!("Top-up successful for {}", custodial_addr);
    }
    
    Ok(())
}
// ensure_gas(custodial_addr, state)

struct RelayerImpl {
    state: RelayerState,
}
// RelayerImpl

impl relayer_capnp::relayer_service::Server for RelayerImpl {
    /// Queries the on-chain admin for a given game ID.
    fn get_game_admin(
        &mut self,
        params: relayer_capnp::relayer_service::GetGameAdminParams,
        mut results: relayer_capnp::relayer_service::GetGameAdminResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let game_id = capnp_rpc::pry!(params.get()).get_game_id();
        let state = self.state.clone();

        capnp::capability::Promise::from_future(async move {
            match state.registry.games(U256::from(game_id)).call().await {
                Ok(game_data) => {
                    results.get().set_admin(game_data.0.as_bytes());
                    Ok(())
                }
                Err(e) => Err(capnp::Error::failed(format!("Registry query failed: {:?}", e))),
            }
        })
    }
    // get_game_admin(params, results)

    /// Returns the derived custodial address for a game ID.
    fn get_custodial_address(
        &mut self,
        params: relayer_capnp::relayer_service::GetCustodialAddressParams,
        mut results: relayer_capnp::relayer_service::GetCustodialAddressResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let game_id = capnp_rpc::pry!(params.get()).get_game_id();
        let chain_id = self.state.master_client.signer().chain_id();
        let wallet = derive_custodial_signer(self.state.master_client.signer(), game_id, chain_id);
        
        results.get().set_address(wallet.address().as_bytes());
        capnp::capability::Promise::ok(())
    }
    // get_custodial_address(params, results)

    /// Submits a match outcome to the blockchain.
    fn submit_outcome(
        &mut self,
        params: relayer_capnp::relayer_service::SubmitOutcomeParams,
        mut results: relayer_capnp::relayer_service::SubmitOutcomeResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let params_reader = capnp_rpc::pry!(params.get());
        let match_id_bytes = capnp_rpc::pry!(params_reader.get_match_id()).to_vec();
        let outcome = params_reader.get_outcome();
        let transcript_hash_bytes = capnp_rpc::pry!(params_reader.get_transcript_hash()).to_vec();
        let signature_bytes = capnp_rpc::pry!(params_reader.get_signature()).to_vec();

        let state = self.state.clone();

        capnp::capability::Promise::from_future(async move {
            let match_id_parsed = if match_id_bytes.len() == 32 {
                U256::from_big_endian(&match_id_bytes)
            } else {
                let s = std::str::from_utf8(&match_id_bytes).unwrap_or("");
                U256::from_dec_str(s).unwrap_or(U256::zero())
            };

            let (game_id, _, _, _, _, _) = state.registry.matches(match_id_parsed).call().await
                .map_err(|e| capnp::Error::failed(format!("Match lookup failed: {:?}", e)))?;

            let chain_id = state.master_client.signer().chain_id();
            let custodial_wallet = derive_custodial_signer(state.master_client.signer(), game_id.as_u64(), chain_id);
            let custodial_addr = custodial_wallet.address();

            ensure_gas(custodial_addr, &state).await
                .map_err(|e| capnp::Error::failed(format!("Gas funding failed: {:?}", e)))?;

            let provider = state.master_client.provider().clone();
            let custodial_client = SignerMiddleware::new(provider, custodial_wallet);
            let settlement_custodial = AMPSettlementContract::new(state.settlement.address(), Arc::new(custodial_client));

            let mut t_hash = [0u8; 32];
            if transcript_hash_bytes.len() == 32 {
                t_hash.copy_from_slice(&transcript_hash_bytes);
            }

            let async_result = AsyncResult {
                match_id: match_id_parsed,
                outcome,
                transcript_hash: t_hash,
                signature: Bytes::from(signature_bytes),
            };

            info!("Submitting outcome for match {} using custodial wallet {}", match_id_parsed, custodial_addr);

            match settlement_custodial.submit_async_result(match_id_parsed, async_result).send().await {
                Ok(pending_tx) => {
                    results.get().set_tx_hash(pending_tx.tx_hash().as_bytes());
                    Ok(())
                }
                Err(e) => Err(capnp::Error::failed(format!("Transaction failed: {:?}", e))),
            }
        })
    }
    // submit_outcome(params, results)
}
// relayer_capnp::relayer_service::Server for RelayerImpl

async fn run_cli_mode(args: Vec<String>) -> Result<()> {
    let rpc_addr = env::var("RPC_PORT").map(|p| format!("127.0.0.1:{}", p))
        .unwrap_or_else(|_| "127.0.0.1:50052".to_string());

    if args[1] == "query-custodial" {
        let game_id: u64 = args[2].parse()?;
        let stream = tokio::net::TcpStream::connect(&rpc_addr).await?;
        let (reader, writer) = stream.into_split();
        let network = twoparty::VatNetwork::new(
            reader.compat(),
            writer.compat_write(),
            rpc_twoparty_capnp::Side::Client,
            Default::default(),
        );
        let mut rpc_system = RpcSystem::new(Box::new(network), None);
        let client: relayer_capnp::relayer_service::Client = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
        
        let local = tokio::task::LocalSet::new();
        local.run_until(async move {
            tokio::task::spawn_local(rpc_system);
            let mut req = client.get_custodial_address_request();
            req.get().set_game_id(game_id);
            let res = req.send().promise.await?;
            println!("0x{}", hex::encode(res.get()?.get_address()?));
            Ok::<(), anyhow::Error>(())
        }).await?;
    } else if args[1] == "query-admin" {
        let game_id: u64 = args[2].parse()?;
        let stream = tokio::net::TcpStream::connect(&rpc_addr).await?;
        let (reader, writer) = stream.into_split();
        let network = twoparty::VatNetwork::new(
            reader.compat(),
            writer.compat_write(),
            rpc_twoparty_capnp::Side::Client,
            Default::default(),
        );
        let mut rpc_system = RpcSystem::new(Box::new(network), None);
        let client: relayer_capnp::relayer_service::Client = rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);
        
        let local = tokio::task::LocalSet::new();
        local.run_until(async move {
            tokio::task::spawn_local(rpc_system);
            let mut req = client.get_game_admin_request();
            req.get().set_game_id(game_id);
            let res = req.send().promise.await?;
            println!("0x{}", hex::encode(res.get()?.get_admin()?));
            Ok::<(), anyhow::Error>(())
        }).await?;
    }
    Ok(())
}
// run_cli_mode(args)

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let args: Vec<String> = env::args().collect();
    if args.len() >= 3 {
        return run_cli_mode(args).await;
    }

    let rpc_port = env::var("RPC_PORT").unwrap_or_else(|_| "50052".to_string());
    let addr: SocketAddr = format!("0.0.0.0:{}", rpc_port).parse()?;

    let fuji_rpc = env::var("FUJI_RPC_URL").unwrap_or_else(|_| "http://localhost:8545".to_string());
    let provider = Provider::<Http>::try_from(fuji_rpc)?;

    let relayer_key = env::var("RELAYER_PRIVATE_KEY")?;
    let wallet: LocalWallet = relayer_key.parse()?;
    let chain_id = provider.get_chainid().await?.as_u64();
    let wallet = wallet.with_chain_id(chain_id);

    let master_client = Arc::new(SignerMiddleware::new(provider, wallet));
    let settlement_address = env::var("CONTRACT_SETTLEMENT")? .parse::<Address>()?;
    let registry_address = env::var("CONTRACT_REGISTRY")? .parse::<Address>()?;

    let settlement = AMPSettlementContract::new(settlement_address, master_client.clone());
    let registry = AMPRegistryContract::new(registry_address, master_client.clone());

    let state = RelayerState { settlement, registry, master_client };
    let relayer_impl = RelayerImpl { state };
    let relayer_client: relayer_capnp::relayer_service::Client = capnp_rpc::new_client(relayer_impl);

    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("AMP Relayer (Cap'n Proto RPC) listening on {}", addr);

    let local = tokio::task::LocalSet::new();
    local.run_until(async move {
        loop {
            let (stream, _) = listener.accept().await?;
            stream.set_nodelay(true)?;
            let (reader, writer) = stream.into_split();
            
            let network = twoparty::VatNetwork::new(
                reader.compat(),
                writer.compat_write(),
                rpc_twoparty_capnp::Side::Server,
                Default::default(),
            );

            let rpc_system = RpcSystem::new(Box::new(network), Some(relayer_client.clone().client));
            tokio::task::spawn_local(rpc_system.map(|_| ()));
        }
        #[allow(unreachable_code)]
        Ok::<(), anyhow::Error>(())
    }).await?;

    Ok(())
}
// main()