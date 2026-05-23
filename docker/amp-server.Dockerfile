FROM rust:1.87-slim AS builder

RUN apt-get update && \
    apt-get install -y --no-install-recommends capnproto && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/amp

COPY . .

RUN cargo build --release -p AMP-Server

FROM debian:bookworm-slim

RUN useradd -m -u 1000 amp

WORKDIR /app

COPY --from=builder /usr/src/amp/target/release/AMP-Server .

RUN chown -R amp:amp /app

USER amp

VOLUME ["/app/data"]

EXPOSE 50051

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD bash -c 'echo > /dev/tcp/localhost/50051' || exit 1

ENTRYPOINT ["./AMP-Server"]
