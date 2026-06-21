use anyhow::{Context, Result};
use serde::de::DeserializeOwned;
use std::path::Path;
use std::sync::Arc;
use tracing::{info, warn};

/// Bump whenever a persisted struct's serialization changes in a way
/// `#[serde(default)]` cannot paper over. On open, a recorded version that
/// differs from this value is treated as incompatible.
const SCHEMA_VERSION: u8 = 2;
const VERSION_KEY: &[u8] = b"_schema_version";

#[derive(Clone)]
pub struct Persistence {
    db: Arc<sled::Db>,
}

impl Persistence {
    pub fn open<P: AsRef<Path>>(path: P) -> Result<Self> {
        let db = sled::open(path).context("failed to open sled database")?;

        // Reconcile the on-disk schema version. A mismatch is a hard stop
        // unless the operator explicitly opts into a wipe — better a loud
        // refusal than silent data corruption.
        match db.get(VERSION_KEY)? {
            None => {
                // Fresh database (or first run on a pre-versioned DB).
                db.insert(VERSION_KEY, &[SCHEMA_VERSION][..])?;
            }
            Some(v) if v.first() == Some(&SCHEMA_VERSION) => {
                // Compatible — nothing to do.
            }
            Some(v) => {
                let recorded = v.first().copied().unwrap_or(0);
                let wipe = std::env::var("AMP_PERSISTENCE_WIPE_ON_VERSION_MISMATCH")
                    .map(|s| s == "1" || s.eq_ignore_ascii_case("true"))
                    .unwrap_or(false);
                if wipe {
                    warn!(
                        recorded_version = recorded,
                        current_version = SCHEMA_VERSION,
                        "Persistence schema mismatch — wiping all trees (operator opted in via \
                         AMP_PERSISTENCE_WIPE_ON_VERSION_MISMATCH=1). Back up the DB directory \
                         before upgrading in the future."
                    );
                    for name in db.tree_names() {
                        let _ = db.drop_tree(&name);
                    }
                    db.insert(VERSION_KEY, &[SCHEMA_VERSION][..])?;
                } else {
                    anyhow::bail!(
                        "Persistence schema version mismatch: DB recorded v{recorded}, this \
                         binary expects v{current}. Back up the DB directory (the on-disk data \
                         is NOT readable by this build), then either downgrade the binary or \
                         restart with AMP_PERSISTENCE_WIPE_ON_VERSION_MISMATCH=1 to reset \
                         (DATA LOSS).",
                        recorded = recorded,
                        current = SCHEMA_VERSION,
                    );
                }
            }
        }

        info!("Persistence layer opened (schema v{})", SCHEMA_VERSION);
        Ok(Self { db: Arc::new(db) })
    }

    pub async fn save<T: serde::Serialize + Send + 'static>(
        &self,
        cf: &str,
        key: &str,
        value: &T,
    ) -> Result<()> {
        let db = self.db.clone();
        let cf = cf.to_string();
        let key = key.to_string();
        let bytes = bincode::serialize(value)?;
        tokio::task::spawn_blocking(move || {
            let tree = db.open_tree(&cf)?;
            tree.insert(key.as_bytes(), bytes.as_slice())?;
            Ok(())
        })
        .await?
    }

    #[allow(dead_code)]
    pub async fn load<T: DeserializeOwned + Send + 'static>(
        &self,
        cf: &str,
        key: &str,
    ) -> Result<Option<T>> {
        let db = self.db.clone();
        let cf = cf.to_string();
        let key = key.to_string();
        let result: Option<T> = tokio::task::spawn_blocking(move || {
            let tree = db.open_tree(&cf)?;
            match tree.get(key.as_bytes())? {
                Some(bytes) => Ok::<Option<T>, anyhow::Error>(Some(bincode::deserialize(&bytes)?)),
                None => Ok::<Option<T>, anyhow::Error>(None),
            }
        })
        .await??;
        Ok(result)
    }

    #[allow(dead_code)]
    pub async fn delete(&self, cf: &str, key: &str) -> Result<()> {
        let db = self.db.clone();
        let cf = cf.to_string();
        let key = key.to_string();
        tokio::task::spawn_blocking(move || {
            let tree = db.open_tree(&cf)?;
            tree.remove(key.as_bytes())?;
            Ok(())
        })
        .await?
    }

    pub fn load_all<T: DeserializeOwned>(&self, cf: &str) -> Result<Vec<(String, T)>> {
        let tree = self.db.open_tree(cf)?;
        let mut results = Vec::new();
        for item in tree.iter() {
            let (key, value) = item?;
            let key_str = String::from_utf8_lossy(&key).to_string();
            match bincode::deserialize(&value) {
                Ok(v) => results.push((key_str, v)),
                Err(e) => {
                    warn!("Skipping corrupt key {} in {}: {}", key_str, cf, e);
                    continue;
                }
            }
        }
        Ok(results)
    }

    /// Synchronously flush all pending writes to disk. Call on shutdown so
    /// terminal settlement state survives process exit.
    pub fn flush(&self) -> Result<()> {
        self.db.flush().context("sled flush failed")?;
        Ok(())
    }
}
