mod match_capnp {
    include!(concat!(env!("OUT_DIR"), "/match_capnp.rs"));
}
mod game_types_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_types_capnp.rs"));
}
mod service_capnp {
    include!(concat!(env!("OUT_DIR"), "/service_capnp.rs"));
}
mod game_core_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_core_capnp.rs"));
}
mod amp_telemetry_capnp {
    include!(concat!(env!("OUT_DIR"), "/amp_telemetry_capnp.rs"));
}
mod player_profile_capnp {
    include!(concat!(env!("OUT_DIR"), "/player_profile_capnp.rs"));
}
mod matchmaking_rules_capnp {
    include!(concat!(env!("OUT_DIR"), "/matchmaking_rules_capnp.rs"));
}
mod inventory_capnp {
    include!(concat!(env!("OUT_DIR"), "/inventory_capnp.rs"));
}
mod tournament_capnp {
    include!(concat!(env!("OUT_DIR"), "/tournament_capnp.rs"));
}
mod security_capnp {
    include!(concat!(env!("OUT_DIR"), "/security_capnp.rs"));
}
mod relayer_capnp {
    include!(concat!(env!("OUT_DIR"), "/relayer_capnp.rs"));
}
mod game_registry_capnp {
    include!(concat!(env!("OUT_DIR"), "/game_registry_capnp.rs"));
}
mod rust_capnp {
    include!(concat!(env!("OUT_DIR"), "/rust_capnp.rs"));
}

use anyhow::{Context, Result};
use ethers::prelude::*;
use std::process::Command;
use std::sync::Arc;
use std::time::{Duration, Instant};
use tokio::net::TcpStream;
use tokio_util::compat::{TokioAsyncReadCompatExt, TokioAsyncWriteCompatExt};

abigen!(
    AMPRegistryContract,
    "../contracts/out/AMPRegistry.sol/AMPRegistry.json"
);
abigen!(
    AMPSettlementContract,
    "../contracts/out/AMPSettlement.sol/AMPSettlement.json"
);

// Structs to read from server database
#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub struct StoredPlayerProfile {
    pub display_name: String,
    pub wallet_address: Vec<u8>,
    pub global_mmr: f32,
    pub mmr_uncertainty: f32,
    pub mmr_volatility: f32,
    pub games_played: u32,
    pub game_stats: std::collections::HashMap<String, StoredGameStats>,
    pub preferred_role: String,
    pub language: String,
    pub platform: String,
    pub region: String,
    pub max_ping_ms: u32,
    pub is_online: bool,
    pub last_login: u64,
    pub restrictions: StoredPlayerRestrictions,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub struct StoredGameStats {
    pub matches_played: u32,
    pub wins: u32,
    pub losses: u32,
    pub draws: u32,
    pub best_streak: u16,
    pub current_streak: i16,
    pub total_play_time_ms: u64,
}

#[derive(Debug, Clone, serde::Serialize, serde::Deserialize)]
pub struct StoredPlayerRestrictions {
    pub is_banned: bool,
    pub ban_expiry: u64,
    pub ban_reason: String,
    pub matchmaking_cooldown_until: u64,
    pub daily_match_limit: u32,
    pub matches_today: u32,
    pub last_match_day: u64,
}

struct KillOnDrop(std::process::Child);

impl Drop for KillOnDrop {
    fn drop(&mut self) {
        let _ = self.0.kill();
    }
}

fn find_free_port() -> u16 {
    let listener = std::net::TcpListener::bind("127.0.0.1:0").unwrap();
    listener.local_addr().unwrap().port()
}

async fn wait_for_tcp(port: u16, name: &str, timeout_secs: u64) -> Result<()> {
    let start = Instant::now();
    loop {
        if std::net::TcpStream::connect(format!("127.0.0.1:{}", port)).is_ok() {
            println!("[TEST] {} is ready on port {}", name, port);
            return Ok(());
        }
        if start.elapsed().as_secs() > timeout_secs {
            anyhow::bail!("Timed out waiting for {} on port {}", name, port);
        }
        tokio::time::sleep(Duration::from_millis(100)).await;
    }
}

async fn connect_rpc<C>(addr: &str) -> Result<C>
where
    C: capnp::capability::FromClientHook,
{
    let stream = TcpStream::connect(addr).await?;
    let (reader, writer) = stream.into_split();
    let network = capnp_rpc::twoparty::VatNetwork::new(
        reader.compat(),
        writer.compat_write(),
        capnp_rpc::rpc_twoparty_capnp::Side::Client,
        Default::default(),
    );
    let mut rpc_system = capnp_rpc::RpcSystem::new(Box::new(network), None);
    let client = rpc_system.bootstrap(capnp_rpc::rpc_twoparty_capnp::Side::Server);
    tokio::task::spawn_local(async move {
        let _ = rpc_system.await;
    });
    Ok(client)
}

async fn login_player(
    service: &service_capnp::game_session_service::Client,
    wallet: &LocalWallet,
) -> Result<service_capnp::user_session::Client> {
    let mut chal_req = service.request_challenge_request();
    chal_req.get().set_game_id(0);
    let chal_resp = chal_req.send().promise.await?;
    let challenge = chal_resp.get()?.get_challenge()?.to_vec();

    let msg_hash = ethers::utils::hash_message(&challenge);
    let sig = wallet.sign_hash(msg_hash)?;
    let sig_bytes = sig.to_vec();

    let mut login_req = service.login_request();
    login_req.get().set_game_id(0);
    login_req.get().set_signature(&sig_bytes);
    login_req.get().set_challenge_payload(&challenge);
    let login_resp = login_req.send().promise.await?;
    let session = login_resp.get()?.get_session()?;
    Ok(session)
}

async fn request_match(
    session: &service_capnp::user_session::Client,
    player_id: &str,
    player_wallet: Address,
) -> Result<(service_capnp::match_session::Client, String)> {
    let mut req = session.request_match_request();
    {
        let mut builder = req.get().init_req();
        builder.set_game_id(b"test-game-0");
        builder.set_rules_type("standard");
        builder.set_match_type(match_capnp::MatchType::TurnBased);
        builder.set_timeout_ms(15_000);
        builder.reborrow().init_stake();
        {
            let mut pi = builder.reborrow().init_player_info();
            pi.set_player_id(player_id.as_bytes());
            pi.set_display_name(player_id);
            pi.set_player_wallet(player_wallet.as_bytes());
        }
    }
    let resp = req.send().promise.await?;
    let response = resp.get()?;
    let assignment = response.get_assignment()?;
    let match_id = String::from_utf8(assignment.get_match_id()?.to_vec())?;
    let m_session = response.get_session()?;
    Ok((m_session, match_id))
}

#[tokio::main(flavor = "current_thread")]
async fn main() -> Result<()> {
    let local = tokio::task::LocalSet::new();
    local.run_until(run_test()).await
}

async fn run_test() -> Result<()> {
    println!("====================================================");
    println!("  AMP END-TO-END INTEGRATION TEST SUITE");
    println!("====================================================");

    let manifest_dir = std::env::var("CARGO_MANIFEST_DIR").unwrap();
    let repo_root = std::path::PathBuf::from(manifest_dir).parent().unwrap().to_path_buf();
    let server_bin = repo_root.join("target/release/AMP-Server");
    let relayer_bin = repo_root.join("target/release/amp-relayer");
    let telemetry_bin = repo_root.join("target/release/amp-telemetry");

    // 1. Compile contracts
    println!("[TEST] Compiling smart contracts...");
    let build_output = Command::new("forge")
        .current_dir(repo_root.join("contracts"))
        .args(&["build"])
        .output()?;
    if !build_output.status.success() {
        anyhow::bail!("Failed to compile contracts with forge: {}", String::from_utf8_lossy(&build_output.stderr));
    }
    println!("[TEST] Smart contracts compiled successfully.");

    // 2. Find ports
    let anvil_port = find_free_port();
    let tel_port = find_free_port();
    let relayer_port = find_free_port();
    let server_port = find_free_port();

    println!("[TEST] Port allocations:");
    println!("  Anvil:     {}", anvil_port);
    println!("  Telemetry: {}", tel_port);
    println!("  Relayer:   {}", relayer_port);
    println!("  Server:    {}", server_port);

    // Temp dirs for DBs
    let temp_dir = tempfile::tempdir()?;
    let telemetry_log = temp_dir.path().join("telemetry.bin");
    let relayer_db = temp_dir.path().join("relayer-db");
    let server_db = temp_dir.path().join("server-db");

    // 3. Start Anvil
    println!("[TEST] Starting Anvil on port {}...", anvil_port);
    let anvil_child = Command::new("anvil")
        .args(&["--port", &anvil_port.to_string(), "--chain-id", "43113"])
        .spawn()?;
    let _anvil_guard = KillOnDrop(anvil_child);

    wait_for_tcp(anvil_port, "Anvil", 10).await?;

    // 4. Deploy contracts
    println!("[TEST] Deploying contracts via Forge script...");
    let deploy_output = Command::new("forge")
        .current_dir(repo_root.join("contracts"))
        .env("PRIVATE_KEY", "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80")
        .args(&[
            "script",
            "script/Deploy.s.sol",
            "--rpc-url",
            &format!("http://127.0.0.1:{}", anvil_port),
            "--broadcast",
            "--private-key",
            "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80",
        ])
        .output()?;

    if !deploy_output.status.success() {
        anyhow::bail!("Failed to deploy contracts: {}", String::from_utf8_lossy(&deploy_output.stderr));
    }

    let stdout_str = String::from_utf8_lossy(&deploy_output.stdout);
    let mut registry_addr = String::new();
    let mut settlement_addr = String::new();

    for line in stdout_str.lines() {
        if line.contains("AMPRegistry Deployed at:") {
            registry_addr = line.split("AMPRegistry Deployed at:").last().unwrap().trim().to_string();
        }
        if line.contains("AMPSettlement Deployed at:") {
            settlement_addr = line.split("AMPSettlement Deployed at:").last().unwrap().trim().to_string();
        }
    }

    println!("[TEST] Deployed addresses:");
    println!("  AMPRegistry:   {}", registry_addr);
    println!("  AMPSettlement: {}", settlement_addr);

    assert!(!registry_addr.is_empty(), "Failed to parse registry address");
    assert!(!settlement_addr.is_empty(), "Failed to parse settlement address");

    // 5. Start Telemetry
    println!("[TEST] Starting Telemetry Receiver...");
    let tel_child = Command::new(&telemetry_bin)
        .arg(format!("127.0.0.1:{}", tel_port))
        .arg(&telemetry_log)
        .env("RUST_LOG", "info")
        .spawn()?;
    let _tel_guard = KillOnDrop(tel_child);
    wait_for_tcp(tel_port, "Telemetry", 10).await?;

    // 6. Start Relayer
    println!("[TEST] Starting Settlement Relayer...");
    let relayer_child = Command::new(&relayer_bin)
        .env("RPC_PORT", relayer_port.to_string())
        .env("FUJI_RPC_URL", format!("http://127.0.0.1:{}", anvil_port))
        .env("CONTRACT_REGISTRY", &registry_addr)
        .env("CONTRACT_SETTLEMENT", &settlement_addr)
        .env("RELAYER_PRIVATE_KEY", "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80")
        .env("RELAYER_DB_PATH", &relayer_db)
        .env("RELAYER_API_KEY", "test-api-key")
        .env("RELAYER_MAX_RETRIES", "3")
        .env("RELAYER_RETRY_DELAY_MS", "500")
        .env("RUST_LOG", "info")
        .spawn()?;
    let _relayer_guard = KillOnDrop(relayer_child);
    wait_for_tcp(relayer_port, "Relayer", 10).await?;

    // 7. Start Server
    println!("[TEST] Starting Matchmaking Server...");
    let server_child = Command::new(&server_bin)
        .env("AMP_ADDR", format!("127.0.0.1:{}", server_port))
        .env("AMP_DB_PATH", &server_db)
        .env("VERIFIER_KEY", "0xac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80")
        .env("AMP_CHAIN_ID", "43113")
        .env("AMP_SETTLEMENT_ADDRESS", &settlement_addr)
        .env("RELAYER_RPC_ADDR", format!("127.0.0.1:{}", relayer_port))
        .env("RELAYER_API_KEY", "test-api-key")
        .env("TELEMETRY_ADDR", format!("127.0.0.1:{}", tel_port))
        .env("AMP_WORKERS", "1")
        .env("RUST_LOG", "info")
        .spawn()?;
    let server_guard = KillOnDrop(server_child);
    wait_for_tcp(server_port, "Server", 10).await?;

    // Setup wallets for players
    // Player A uses anvil key 1, Player B uses anvil key 2
    let wallet_a: LocalWallet = "59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d"
        .parse::<LocalWallet>()?
        .with_chain_id(43113u64);
    let wallet_b: LocalWallet = "5de4111afa1a4b94908f83103eb1f1706367c2e68ca870fc3fb9a804cdab365a"
        .parse::<LocalWallet>()?
        .with_chain_id(43113u64);

    let provider = Provider::<Http>::try_from(&format!("http://127.0.0.1:{}", anvil_port))?;
    let client_a = Arc::new(SignerMiddleware::new(provider.clone(), wallet_a.clone()));
    let client_b = Arc::new(SignerMiddleware::new(provider.clone(), wallet_b.clone()));

    let registry_contract_a = AMPRegistryContract::new(registry_addr.parse::<Address>()?, client_a);
    let registry_contract_b = AMPRegistryContract::new(registry_addr.parse::<Address>()?, client_b);

    // 8. Register game on-chain
    println!("[TEST] Registering game on-chain...");
    let verifier_address = Address::from_slice(&hex::decode("f39Fd6e51aad88F6F4ce6aB8827279cffFb92266")?);
    let tx = registry_contract_a.register_game(
        0, // ASYNC_VERIFIER
        vec![verifier_address],
        U256::from(1_000_000_000_000_000_000u64), // 1 ether
        Address::zero(), // Native stake token
        Address::zero(), // Arbiter
    );
    let pending_tx = tx.send().await?;
    pending_tx.await?;
    println!("[TEST] Game registered successfully on-chain.");

    // 9. Login players to Matchmaking Server
    println!("[TEST] Connecting to matchmaking server RPC...");
    let session_service: service_capnp::game_session_service::Client =
        connect_rpc(&format!("127.0.0.1:{}", server_port)).await?;

    println!("[TEST] Authenticating Player A...");
    let user_session_a = login_player(&session_service, &wallet_a).await?;
    println!("[TEST] Player A authenticated.");

    println!("[TEST] Authenticating Player B...");
    let user_session_b = login_player(&session_service, &wallet_b).await?;
    println!("[TEST] Player B authenticated.");

    // 10. Queue for matchmaking
    println!("[TEST] Entering matchmaking queue for Player A and Player B...");
    let (match_res_a, match_res_b) = tokio::try_join!(
        request_match(&user_session_a, "Player A", wallet_a.address()),
        request_match(&user_session_b, "Player B", wallet_b.address())
    )?;

    let (match_session_a, match_id_uuid) = match_res_a;
    let (_match_session_b, match_id_uuid_b) = match_res_b;

    assert_eq!(match_id_uuid, match_id_uuid_b, "Matched players must receive the same match ID");
    println!("[TEST] Match found! UUID: {}", match_id_uuid);

    // Calculate on-chain match ID
    let match_id_val = U256::from_big_endian(&ethers::utils::keccak256(match_id_uuid.as_bytes()));
    println!("[TEST] On-chain match ID hash: {}", match_id_val);

    // 11. Create and Join Match on-chain
    println!("[TEST] Player A creating match on-chain with 1 ether stake...");
    let tx = registry_contract_a.create_match(
        U256::from(0), // gameId = 0
        match_id_val,
        U256::from(1_000_000_000_000_000_000u64),
    ).value(U256::from(1_000_000_000_000_000_000u64));
    let pending_tx = tx.send().await?;
    pending_tx.await?;
    println!("[TEST] Match created on-chain.");

    println!("[TEST] Player B joining match on-chain with 1 ether stake...");
    let tx = registry_contract_b.join_match(match_id_val)
        .value(U256::from(1_000_000_000_000_000_000u64));
    let pending_tx = tx.send().await?;
    pending_tx.await?;
    println!("[TEST] Match joined on-chain. State transitioned to READY.");

    // 12. Submit match outcome
    println!("[TEST] Submitting outcome (Player A wins) to verifier...");
    let mut submit_req = match_session_a.submit_outcome_request();
    {
        let mut sub = submit_req.get().init_submission();
        sub.set_match_id(match_id_uuid.as_bytes());
        sub.set_replay_hash(&[0u8; 32]);
        sub.set_signature(&[0u8; 65]);
        {
            let mut outcome = sub.reborrow().init_outcome();
            outcome.set_type(match_capnp::OutcomeType::Win);
            outcome.set_victor(1); // Player A wins
            let mut scores = outcome.init_scores(2);
            scores.set(0, 1);
            scores.set(1, 0);
        }
    }
    let submit_resp = submit_req.send().promise.await?;
    let verifier_sig = submit_resp.get()?.get_signature()?;
    println!("[TEST] Outcome submission accepted. Verifier signature received ({} bytes).", verifier_sig.len());

    // 13. Poll on-chain settlement
    println!("[TEST] Polling on-chain Settlement contract state...");
    let settlement_contract = AMPSettlementContract::new(settlement_addr.parse::<Address>()?, Arc::new(provider.clone()));
    let start_poll = Instant::now();
    let mut settled = false;

    while start_poll.elapsed().as_secs() < 30 {
        if let Ok((_match_id, outcome, _t_hash, _settled_at)) = settlement_contract.settlements(match_id_val).call().await {
            if outcome == 1 { // WIN_A
                println!("[TEST] Outcome verified on-chain: WIN_A");
                settled = true;
                break;
            }
        }
        tokio::time::sleep(Duration::from_millis(500)).await;
    }

    assert!(settled, "Match failed to settle on-chain within timeout");

    // 14. Verify balances
    let pending_withdrawal_a = registry_contract_a.pending_withdrawals(Address::zero(), wallet_a.address()).call().await?;
    println!("[TEST] Player A pending withdrawal balance: {} Wei (expected 1.98 ether)", pending_withdrawal_a);
    // stake is 1 ether from A + 1 ether from B = 2 ether. fee is 1% (100 BPS), so payout is 1.98 ether = 1_980_000_000_000_000_000 Wei.
    assert_eq!(pending_withdrawal_a, U256::from(1_980_000_000_000_000_000u64));

    let protocol_fee = registry_contract_a.fee_balances(Address::zero()).call().await?;
    println!("[TEST] Protocol fees collected: {} Wei (expected 0.02 ether)", protocol_fee);
    assert_eq!(protocol_fee, U256::from(20_000_000_000_000_000u64));

    // 15. Verify player profile rating updates
    println!("[TEST] Shutting down matchmaking server to verify database MMR updates...");
    drop(server_guard);
    tokio::time::sleep(Duration::from_millis(500)).await;

    let db = sled::open(&server_db)?;
    let players_tree = db.open_tree("players")?;

    let player_id_a = format!("0x{}", hex::encode(wallet_a.address().as_bytes()));
    let player_id_b = format!("0x{}", hex::encode(wallet_b.address().as_bytes()));

    let profile_bytes_a = players_tree.get(player_id_a.as_bytes())?.expect("Player A profile not found in Sled");
    let profile_a: StoredPlayerProfile = bincode::deserialize(&profile_bytes_a)?;
    println!("[TEST] Player A final rating: MMR={:.2} (games={})", profile_a.global_mmr, profile_a.games_played);
    assert!(profile_a.global_mmr > 1200.0, "Player A MMR should have increased after a win");
    assert_eq!(profile_a.games_played, 1);

    let profile_bytes_b = players_tree.get(player_id_b.as_bytes())?.expect("Player B profile not found in Sled");
    let profile_b: StoredPlayerProfile = bincode::deserialize(&profile_bytes_b)?;
    println!("[TEST] Player B final rating: MMR={:.2} (games={})", profile_b.global_mmr, profile_b.games_played);
    assert!(profile_b.global_mmr < 1200.0, "Player B MMR should have decreased after a loss");
    assert_eq!(profile_b.games_played, 1);

    println!("\n====================================================");
    println!("  ALL INTEGRATION TESTS PASSED SUCCESSFULLY!");
    println!("====================================================");

    Ok(())
}
