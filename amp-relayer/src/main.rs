use std::env;
use std::net::SocketAddr;
use std::sync::Arc;

use anyhow::Result;
use capnp_rpc::{RpcSystem, rpc_twoparty_capnp, twoparty};
use ethers::prelude::*;
use futures::FutureExt;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};
use tracing::{error, info};

mod config;
mod custodial;
mod error;
mod gas;
mod nonce;
mod settlement;

#[allow(
    clippy::all,
    clippy::nursery,
    non_snake_case,
    dead_code,
    unused_parens,
    unused_braces
)]
pub mod relayer_capnp {
    include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs"));
}

abigen!(
    AMPSettlementContract,
    "../contracts/out/AMPSettlement.sol/AMPSettlement.json"
);

use dashmap::DashSet;

abigen!(
    AMPRegistryContract,
    "../contracts/out/AMPRegistry.sol/AMPRegistry.json"
);

#[derive(Clone)]
pub struct RelayerState {
    settlement: AMPSettlementContract<SignerMiddleware<Provider<Http>, LocalWallet>>,
    registry: AMPRegistryContract<SignerMiddleware<Provider<Http>, LocalWallet>>,
    master_client: Arc<SignerMiddleware<Provider<Http>, LocalWallet>>,
    pending_topups: Arc<DashSet<Address>>,
}

struct RelayerImpl {
    state: RelayerState,
    queue: Arc<settlement::SettlementQueue>,
}

impl relayer_capnp::relayer_service::Server for RelayerImpl {
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
                Err(e) => Err(capnp::Error::failed(format!(
                    "Registry query failed: {:?}",
                    e
                ))),
            }
        })
    }

    fn get_custodial_address(
        &mut self,
        params: relayer_capnp::relayer_service::GetCustodialAddressParams,
        mut results: relayer_capnp::relayer_service::GetCustodialAddressResults,
    ) -> capnp::capability::Promise<(), capnp::Error> {
        let game_id = capnp_rpc::pry!(params.get()).get_game_id();
        let chain_id = self.state.master_client.signer().chain_id();
        let wallet = custodial::derive_custodial_signer(
            self.state.master_client.signer(),
            "settlement",
            game_id,
            chain_id,
        );

        results.get().set_address(wallet.address().as_bytes());
        capnp::capability::Promise::ok(())
    }

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

        let queue = self.queue.clone();

        capnp::capability::Promise::from_future(async move {
            let now = std::time::SystemTime::now()
                .duration_since(std::time::UNIX_EPOCH)
                .unwrap_or_default()
                .as_millis() as u64;

            let pending = settlement::PendingSettlement {
                match_id: match_id_bytes,
                outcome,
                transcript_hash: transcript_hash_bytes,
                signature: signature_bytes,
                retry_count: 0,
                enqueued_at_ms: now,
                last_attempt_at_ms: None,
                status: settlement::SettlementStatus::Queued,
            };

            queue
                .enqueue(pending)
                .map_err(|e| capnp::Error::failed(format!("Queue error: {:?}", e)))?;

            results.get().set_tx_hash(b"queued");
            Ok(())
        })
    }
}

async fn run_cli_mode(args: Vec<String>) -> Result<()> {
    let rpc_addr = env::var("RPC_PORT")
        .map(|p| format!("127.0.0.1:{}", p))
        .unwrap_or_else(|_| "127.0.0.1:50052".to_string());

    if args[1] == "query-custodial" || args[1] == "query-admin" {
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
        let client: relayer_capnp::relayer_service::Client =
            rpc_system.bootstrap(rpc_twoparty_capnp::Side::Server);

        let local = tokio::task::LocalSet::new();
        local
            .run_until(async move {
                tokio::task::spawn_local(rpc_system);
                if args[1] == "query-custodial" {
                    let mut req = client.get_custodial_address_request();
                    req.get().set_game_id(game_id);
                    let res = req.send().promise.await?;
                    println!("0x{}", hex::encode(res.get()?.get_address()?));
                } else {
                    let mut req = client.get_game_admin_request();
                    req.get().set_game_id(game_id);
                    let res = req.send().promise.await?;
                    println!("0x{}", hex::encode(res.get()?.get_admin()?));
                }
                Ok::<(), anyhow::Error>(())
            })
            .await?;
    }
    Ok(())
}

async fn run_settlement_processor(
    state: RelayerState,
    queue: Arc<settlement::SettlementQueue>,
    cfg: config::Config,
    cancel: tokio_util::sync::CancellationToken,
) {
    let nonce_manager = Arc::new(nonce::NonceManager::new());

    loop {
        tokio::select! {
            _ = cancel.cancelled() => {
                info!("Settlement processor shutting down...");
                return;
            }
            _ = tokio::time::sleep(std::time::Duration::from_millis(500)) => {}
        }

        match queue
            .process_next(&state, &nonce_manager)
            .await
        {
            Ok(true) => {}
            Ok(false) => {}
            Err(e) => {
                error!("Settlement processor error: {:?}", e);
            }
        }
    }
}

#[tokio::main]
async fn main() -> Result<()> {
    tracing_subscriber::fmt::init();
    dotenv::dotenv().ok();

    let args: Vec<String> = env::args().collect();
    if args.len() >= 3 {
        return run_cli_mode(args).await;
    }

    let cfg = config::Config::from_env()?;
    let addr: SocketAddr = cfg.rpc_addr.parse()?;

    let provider = Provider::<Http>::try_from(&cfg.fuji_rpc_url)?;
    let wallet: LocalWallet = cfg.relayer_private_key.parse()?;
    let chain_id = provider.get_chainid().await?.as_u64();
    let wallet = wallet.with_chain_id(chain_id);

    let master_client = Arc::new(SignerMiddleware::new(provider.clone(), wallet));
    let settlement_address: Address = cfg.contract_settlement.parse()?;
    let registry_address: Address = cfg.contract_registry.parse()?;

    let settlement = AMPSettlementContract::new(settlement_address, master_client.clone());
    let registry = AMPRegistryContract::new(registry_address, master_client.clone());

    let state = RelayerState {
        settlement,
        registry,
        master_client,
        pending_topups: Arc::new(DashSet::new()),
    };

    let db = sled::open(&cfg.db_path)?;
    let queue = Arc::new(settlement::SettlementQueue::new(
        Arc::new(db),
        cfg.max_retries,
        cfg.base_retry_delay_ms,
    ));

    queue.replay_pending()?;

    let cancel = tokio_util::sync::CancellationToken::new();
    let cancel_processor = cancel.clone();

    let state_processor = state.clone();
    let queue_processor = queue.clone();
    let cfg_processor = cfg.clone();

    let relayer_impl = RelayerImpl {
        state,
        queue: queue.clone(),
    };
    let relayer_client: relayer_capnp::relayer_service::Client =
        capnp_rpc::new_client(relayer_impl);

    let listener = tokio::net::TcpListener::bind(&addr).await?;
    info!("AMP Relayer (Cap'n Proto RPC) listening on {}", addr);

    tokio::spawn(run_settlement_processor(
        state_processor,
        queue_processor,
        cfg_processor,
        cancel_processor,
    ));

    let local = tokio::task::LocalSet::new();
    local
        .run_until(async move {
            tokio::select! {
                _ = tokio::signal::ctrl_c() => {
                    info!("Received shutdown signal");
                    cancel.cancel();
                    tokio::time::sleep(std::time::Duration::from_millis(500)).await;
                    info!("Shutdown complete.");
                }
                result = accept_loop(listener, relayer_client) => {
                    result?;
                }
            }
            Ok::<(), anyhow::Error>(())
        })
        .await?;

    Ok(())
}

async fn accept_loop(
    listener: tokio::net::TcpListener,
    relayer_client: relayer_capnp::relayer_service::Client,
) -> Result<()> {
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
}
