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

    pub fn save<T: serde::Serialize>(&self, cf: &str, key: &str, value: &T) -> Result<()> {
        let tree = self.db.open_tree(cf)?;
        let bytes = bincode::serialize(value)?;
        tree.insert(key.as_bytes(), bytes.as_slice())?;
        tree.flush()?;
        Ok(())
    }

    #[allow(dead_code)]
    pub fn load<T: DeserializeOwned>(&self, cf: &str, key: &str) -> Result<Option<T>> {
        let tree = self.db.open_tree(cf)?;
        match tree.get(key.as_bytes())? {
            Some(bytes) => Ok(Some(bincode::deserialize(&bytes)?)),
            None => Ok(None),
        }
    }

    #[allow(dead_code)]
    pub fn delete(&self, cf: &str, key: &str) -> Result<()> {
        let tree = self.db.open_tree(cf)?;
        tree.remove(key.as_bytes())?;
        tree.flush()?;
        Ok(())
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
