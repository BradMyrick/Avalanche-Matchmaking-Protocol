# Fly.io Dockerfile for the AMP relayer (isolated custody service).
# Deploy from the REPO ROOT:  fly deploy
# The build context is the repo root (needs the workspace Cargo.toml + Cargo.lock).

FROM rust:1.97 AS builder
WORKDIR /amp
COPY Cargo.toml Cargo.lock ./
COPY relayer/ ./relayer/
RUN cargo build --release -p amp-relayer

FROM debian:bookworm-slim
RUN apt-get update && apt-get install -y --no-install-recommends ca-certificates && rm -rf /var/lib/apt/lists/*
COPY --from=builder /amp/target/release/amp-relayer /usr/local/bin/amp-relayer
CMD ["amp-relayer"]
