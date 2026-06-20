use alloy_transport::TransportError;
use std::time::SystemTimeError;

#[derive(thiserror::Error, Debug)]
pub enum RelayerError {
    #[error("Network request failed: {0}")]
    Network(#[from] TransportError),

    #[error("Contract call failed: {0}")]
    Contract(String),

    #[error("Database operation failed: {0}")]
    Database(#[from] sled::Error),

    #[error("Serialization failed: {0}")]
    Serialization(#[from] bincode::Error),

    #[error("Transaction failed: {0}")]
    Transaction(String),

    #[error("Time error: {0}")]
    Time(#[from] SystemTimeError),
}
