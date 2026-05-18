FROM rust:1.87-slim AS builder

RUN apt-get update && \
    apt-get install -y --no-install-recommends capnproto && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/amp

COPY . .

RUN cargo build --release -p AMP-Server

FROM debian:bookworm-slim

RUN apt-get update && \
    apt-get install -y --no-install-recommends ca-certificates && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /app

COPY --from=builder /usr/src/amp/target/release/AMP-Server .

VOLUME ["/app/data"]

EXPOSE 50051

ENTRYPOINT ["./AMP-Server"]
