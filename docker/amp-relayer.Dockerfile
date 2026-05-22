FROM rust:1.87-slim AS builder

RUN apt-get update && \
    apt-get install -y --no-install-recommends capnproto curl git ca-certificates && \
    rm -rf /var/lib/apt/lists/*

RUN curl -L https://foundry.paradigm.xyz | bash
ENV PATH="/root/.foundry/bin:${PATH}"
RUN foundryup

WORKDIR /usr/src/amp

COPY . .

RUN cd contracts && forge build

RUN cargo build --release -p amp-relayer

FROM debian:bookworm-slim

RUN useradd -m -u 1000 amp

WORKDIR /app

COPY --from=builder /usr/src/amp/target/release/amp-relayer .

RUN chown -R amp:amp /app

USER amp

EXPOSE 50052

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD bash -c 'echo > /dev/tcp/localhost/50052' || exit 1

ENTRYPOINT ["./amp-relayer"]
