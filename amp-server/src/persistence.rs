use anyhow::{Context, Result};
use serde::de::DeserializeOwned;
use std::path::Path;
use std::sync::Arc;
use tracing::{info, warn};

#[allow(dead_code)]
const CF_PLAYERS: &str = "players";
#[allow(dead_code)]
const CF_RULESETS: &str = "rulesets";
#[allow(dead_code)]
const CF_MATCHES: &str = "matches";

#[derive(Clone)]
pub struct Persistence {
    db: Arc<sled::Db>,
}

impl Persistence {
    pub fn open<P: AsRef<Path>>(path: P) -> Result<Self> {
        let db = sled::open(path).context("failed to open sled database")?;
        info!("Persistence layer opened");
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
}
