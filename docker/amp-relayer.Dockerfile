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

RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=builder /usr/src/amp/target/release/amp-relayer .

EXPOSE 50052

ENTRYPOINT ["./amp-relayer"]
