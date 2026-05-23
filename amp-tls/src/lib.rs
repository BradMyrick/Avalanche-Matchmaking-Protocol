use std::fs;
use std::io;
use std::sync::Arc;

pub use tokio_rustls::TlsAcceptor;

pub fn create_tls_acceptor(cert_path: &str, key_path: &str) -> io::Result<TlsAcceptor> {
    let cert_pem = fs::read(cert_path)?;
    let key_pem = fs::read(key_path)?;

    let certs: Vec<rustls::Certificate> = rustls_pemfile::certs(&mut &cert_pem[..])?
        .into_iter()
        .map(rustls::Certificate)
        .collect();

    if certs.is_empty() {
        return Err(io::Error::new(
            io::ErrorKind::NotFound,
            format!("no certificates found in {}", cert_path),
        ));
    }

    let key = rustls_pemfile::rsa_private_keys(&mut &key_pem[..])?
        .into_iter()
        .next()
        .map(rustls::PrivateKey)
        .or_else(|| {
            rustls_pemfile::ec_private_keys(&mut &key_pem[..])
                .ok()
                .and_then(|keys| keys.into_iter().next().map(rustls::PrivateKey))
        })
        .or_else(|| {
            rustls_pemfile::pkcs8_private_keys(&mut &key_pem[..])
                .ok()
                .and_then(|keys| keys.into_iter().next().map(rustls::PrivateKey))
        })
        .ok_or_else(|| io::Error::new(io::ErrorKind::NotFound, "no private key found in PEM"))?;

    let config = rustls::ServerConfig::builder()
        .with_safe_defaults()
        .with_no_client_auth()
        .with_single_cert(certs, key)
        .map_err(|e| io::Error::new(io::ErrorKind::InvalidData, e))?;

    Ok(TlsAcceptor::from(Arc::new(config)))
}
