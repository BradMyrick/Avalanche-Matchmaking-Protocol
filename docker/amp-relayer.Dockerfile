FROM rust:1.87-slim AS builder

RUN apt-get update && \
    apt-get install -y --no-install-recommends capnproto git ca-certificates && \
    rm -rf /var/lib/apt/lists/*

ARG FOUNDRY_VERSION=latest
RUN curl -L -o /tmp/foundryup.sh https://raw.githubusercontent.com/foundry-rs/foundry/master/foundryup/foundryup && \
    chmod +x /tmp/foundryup.sh && \
    sha256sum /tmp/foundryup.sh > /tmp/foundryup.sha256 && \
    cat /tmp/foundryup.sha256 && \
    /tmp/foundryup.sh && \
    rm /tmp/foundryup.sh /tmp/foundryup.sha256
ENV PATH="/root/.foundry/bin:${PATH}"

WORKDIR /usr/src/amp

COPY . .

RUN cd contracts && forge build

RUN cargo build --release -p amp-relayer

FROM debian:bookworm-slim

RUN useradd -m -u 1000 amp

WORKDIR /app

COPY --from=builder /usr/src/amp/target/release/amp-relayer .

RUN chown -R amp:amp /app && mkdir -p /app/relayer-data && chown -R amp:amp /app/relayer-data

USER amp

VOLUME ["/app/relayer-data"]

EXPOSE 50052

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD bash -c 'echo > /dev/tcp/localhost/50052' || exit 1

ENTRYPOINT ["./amp-relayer"]
