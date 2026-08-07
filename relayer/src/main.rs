//! AMP custodial relayer — the only process that holds the funded key.
//!
//! Polls `relayer_jobs` in Postgres, signs + submits `createTournament` and
//! `finalizeTournament` on Fuji, writes back the result. The web app never
//! touches the key; it only enqueues jobs.

use std::env;
use std::time::Duration;

use anyhow::{anyhow, Context, Result};
use ethers::contract::abigen;
use ethers::core::k256::{ecdsa::SigningKey, FieldBytes};
use ethers::core::types::{Address, U256};
use ethers::middleware::SignerMiddleware;
use ethers::providers::{Http, Middleware, Provider};
use ethers::signers::{LocalWallet, Signer};
use ethers::utils::keccak256;
use serde::Deserialize;
use serde_json::Value;
use sqlx::postgres::PgPoolOptions;
use sqlx::{PgPool, Row};
use std::sync::Arc;
use tracing::{error, info};

abigen!(
    AMPTournamentCup,
    r#"[
        function createTournament(uint16[] payoutBps, address verifier, uint64 finalizeDeadline) payable returns (uint256)
        function finalizeTournament(uint256 tournamentId, address[] winners, bytes signature)
        function nextTournamentId() view returns (uint256)
    ]"#,
);

const FUJI_CHAIN_ID: u64 = 43113;
const FUJI_RPC: &str = "https://api.avax-test.network/ext/bc/C/rpc";
const CUP_ADDRESS_HEX: &str = "0x7c743c1c9ae3e7a65d030098f2249b7787d66dff";

#[derive(Debug, Deserialize)]
struct Job {
    id: i64,
    kind: String,
    payload: Value,
}

#[derive(Debug, Deserialize)]
struct FundPayload {
    #[serde(rename = "payoutBps")]
    payout_bps: Vec<u16>,
    #[serde(rename = "winnerWallets", default)]
    winner_wallets: Vec<String>,
    #[serde(rename = "fundedAvax")]
    funded_avax: f64,
    #[serde(default)]
    mode: Option<String>,
    #[serde(rename = "finalizeDays", default = "default_finalize_days")]
    finalize_days: i64,
}

fn default_finalize_days() -> i64 {
    7
}

#[tokio::main]
async fn main() -> Result<()> {
    // Load relayer/.env for local dev (no-op in prod where env is injected).
    let _ = dotenvy::dotenv();

    tracing_subscriber::fmt()
        .with_env_filter(
            tracing_subscriber::EnvFilter::try_from_default_env()
                .unwrap_or_else(|_| tracing_subscriber::EnvFilter::new("info")),
        )
        .init();

    let db_url = env::var("DATABASE_URL").context("DATABASE_URL not set")?;
    let key_str = env::var("AMP_RELAYER_KEY").context("AMP_RELAYER_KEY not set")?;
    let wallet: LocalWallet = key_str
        .parse::<LocalWallet>()
        .context("invalid AMP_RELAYER_KEY")?
        .with_chain_id(FUJI_CHAIN_ID);
    let relayer_addr = wallet.address();
    info!(?relayer_addr, "AMP relayer started");

    let pool = PgPoolOptions::new()
        .max_connections(2)
        .connect(&db_url)
        .await
        .context("connecting to Postgres")?;

    let provider = Arc::new(SignerMiddleware::new(
        Provider::<Http>::try_from(FUJI_RPC)?,
        wallet.clone(),
    ));
    let balance = provider.get_balance(relayer_addr, None).await?;
    info!(?balance, "relayer balance");

    loop {
        match poll_once(&pool, &provider, &key_str).await {
            Ok(true) => {}
            Ok(false) => tokio::time::sleep(Duration::from_secs(3)).await,
            Err(e) => {
                error!(error = %e, "poll error; backing off");
                tokio::time::sleep(Duration::from_secs(10)).await;
            }
        }
    }
}

type SignerProvider = SignerMiddleware<Provider<Http>, LocalWallet>;

async fn poll_once(pool: &PgPool, provider: &Arc<SignerProvider>, key_str: &str) -> Result<bool> {
    let row = sqlx::query(
        r#"SELECT id, kind, payload::text as payload FROM relayer_jobs
           WHERE status = 'pending'
           ORDER BY id ASC
           FOR UPDATE SKIP LOCKED
           LIMIT 1"#,
    )
    .fetch_optional(pool)
    .await?;

    let Some(rec) = row else { return Ok(false) };
    let job = Job {
        id: rec.get("id"),
        kind: rec.get("kind"),
        payload: serde_json::from_str(&rec.get::<Option<String>, _>("payload").unwrap_or_default())?,
    };

    info!(job_id = job.id, kind = %job.kind, "processing job");

    let result = match job.kind.as_str() {
        "fund" => fund_job(provider, &job, key_str).await,
        "finalize" => finalize_job(provider, &job, key_str).await,
        other => Err(anyhow!("unknown job kind: {other}")),
    };

    match result {
        Ok((tournament_id, tx_hash)) => {
            let tx_hash = tx_hash.unwrap_or_default();
            sqlx::query(
                r#"UPDATE relayer_jobs
                   SET status = 'done', tournament_id = $2, tx_hash = $3, completed_at = now()
                   WHERE id = $1"#,
            )
            .bind(job.id)
            .bind(tournament_id)
            .bind(tx_hash.as_str())
            .execute(pool)
            .await?;
            info!(job_id = job.id, ?tournament_id, %tx_hash, "job done");
        }
        Err(e) => {
            let msg = format!("{e:#}");
            error!(job_id = job.id, error = %msg, "job failed");
            sqlx::query(
                r#"UPDATE relayer_jobs
                   SET status = 'failed', error = $2, completed_at = now()
                   WHERE id = $1"#,
            )
            .bind(job.id)
            .bind(msg.as_str())
            .execute(pool)
            .await?;
        }
    }
    Ok(true)
}

async fn fund_job(provider: &Arc<SignerProvider>, job: &Job, key_str: &str) -> Result<(Option<i64>, Option<String>)> {
    let p: FundPayload = serde_json::from_value(job.payload.clone())?;

    let cup: Address = CUP_ADDRESS_HEX.parse()?;
    let contract = AMPTournamentCup::new(cup, Arc::clone(provider));

    let deadline = U256::from(
        std::time::SystemTime::now()
            .duration_since(std::time::UNIX_EPOCH)?
            .as_secs()
            + (p.finalize_days * 86400) as u64,
    );
    let value = ethers::utils::parse_ether(p.funded_avax.to_string())?;
    let verifier = provider.signer().address();

    let create_call = contract
        .create_tournament(p.payout_bps.clone(), verifier, deadline.as_u64())
        .value(value)
        .gas(500_000);
    let pending = create_call.send().await.context("send createTournament")?;
    let receipt = pending.await?.context("createTournament reverted")?;
    let create_tx_hash = format!("{:?}", receipt.transaction_hash);

    let next = contract.next_tournament_id().call().await?;
    let tournament_id = next - U256::one();

    if p.mode.as_deref() == Some("instant") && !p.winner_wallets.is_empty() {
        let winners = parse_addresses(&p.winner_wallets)?;
        let sig = finalize_signature(key_str, tournament_id.as_u64(), &winners)?;
        let fin_call = contract.finalize_tournament(tournament_id, winners, sig.into()).gas(400_000);
        let receipt = fin_call.send().await?.await?.context("finalize reverted")?;
        return Ok((Some(tournament_id.as_u64() as i64), Some(format!("{:?}", receipt.transaction_hash))));
    }

    Ok((Some(tournament_id.as_u64() as i64), Some(create_tx_hash)))
}

async fn finalize_job(provider: &Arc<SignerProvider>, job: &Job, key_str: &str) -> Result<(Option<i64>, Option<String>)> {
    #[derive(Deserialize)]
    struct FinPayload {
        #[serde(rename = "tournamentId")]
        tournament_id: i64,
        #[serde(rename = "winnerWallets")]
        winner_wallets: Vec<String>,
    }
    let p: FinPayload = serde_json::from_value(job.payload.clone())?;
    let tid = U256::from(p.tournament_id);
    let winners = parse_addresses(&p.winner_wallets)?;

    let cup: Address = CUP_ADDRESS_HEX.parse()?;
    let contract = AMPTournamentCup::new(cup, Arc::clone(provider));
    let sig = finalize_signature(key_str, p.tournament_id as u64, &winners)?;
    let fin_call = contract.finalize_tournament(tid, winners, sig.into()).gas(400_000);
    let receipt = fin_call.send().await?.await?.context("finalize reverted")?;
    Ok((Some(p.tournament_id), Some(format!("{:?}", receipt.transaction_hash))))
}

/// Hand-rolled EIP-712 TournamentResult signature, byte-identical to the contract
/// and the browser (ethers signTypedData). Kept explicit so the digest encoding
/// is unambiguous and version-independent.
fn finalize_signature(key_str: &str, tournament_id: u64, winners: &[Address]) -> Result<Vec<u8>> {
    let digest = eip712_finalize_digest(tournament_id, winners);
    let secret_hex = key_str.trim_start_matches("0x");
    let secret = hex::decode(secret_hex).context("bad key hex")?;
    let arr: [u8; 32] = secret
        .as_slice()
        .try_into()
        .map_err(|_| anyhow!("relayer key must be 32 bytes"))?;
    let fb: FieldBytes = arr.into();
    let signing_key = SigningKey::from_bytes(&fb).context("invalid key bytes")?;
    let (sig, rid) = signing_key
        .sign_prehash_recoverable(&digest)
        .context("sign prehash")?;
    let r = sig.r().to_bytes();
    let s = sig.s().to_bytes();
    let mut out = Vec::with_capacity(65);
    out.extend_from_slice(&r);
    out.extend_from_slice(&s);
    out.push(27 + rid.to_byte());
    Ok(out)
}

/// EIP-712 digest for TournamentResult(uint256 tournamentId, address[] winners).
fn eip712_finalize_digest(tournament_id: u64, winners: &[Address]) -> [u8; 32] {
    let cup: Address = CUP_ADDRESS_HEX.parse().unwrap();

    // Domain separator
    let mut bs = Vec::new();
    bs.extend_from_slice(&keccak256(b"EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"));
    bs.extend_from_slice(&keccak256(b"AMPTournamentCup"));
    bs.extend_from_slice(&keccak256(b"1"));
    bs.extend_from_slice(&u256_bytes(U256::from(FUJI_CHAIN_ID)));
    bs.extend_from_slice(&addr_bytes(&cup));
    let domain_sep = keccak256(&bs);

    let type_hash = keccak256(b"TournamentResult(uint256 tournamentId,address[] winners)");

    // winners root: keccak of concatenated 32-byte-padded addresses (EIP-712 array encoding)
    let mut w = Vec::new();
    for a in winners {
        w.extend_from_slice(&addr_bytes(a));
    }
    let winners_root = keccak256(&w);

    // struct hash
    let mut sh = Vec::new();
    sh.extend_from_slice(&type_hash);
    sh.extend_from_slice(&u256_bytes(U256::from(tournament_id)));
    sh.extend_from_slice(&winners_root);
    let struct_hash = keccak256(&sh);

    let mut d = vec![0x19, 0x01];
    d.extend_from_slice(&domain_sep);
    d.extend_from_slice(&struct_hash);
    keccak256(&d)
}

fn u256_bytes(v: U256) -> [u8; 32] {
    let mut buf = [0u8; 32];
    v.to_big_endian(&mut buf);
    buf
}

fn addr_bytes(a: &Address) -> [u8; 32] {
    let mut buf = [0u8; 32];
    buf[12..].copy_from_slice(a.as_bytes());
    buf
}

fn parse_addresses(strs: &[String]) -> Result<Vec<Address>> {
    strs.iter()
        .map(|s| s.parse::<Address>().with_context(|| format!("bad address: {s}")))
        .collect()
}
