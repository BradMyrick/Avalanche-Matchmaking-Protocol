FROM rust:1.87-slim AS builder

RUN apt-get update && \
    apt-get install -y --no-install-recommends capnproto && \
    rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src/amp

COPY . .

RUN cargo build --release -p amp-telemetry

FROM debian:bookworm-slim

RUN useradd -m -u 1000 amp

WORKDIR /app

COPY --from=builder /usr/src/amp/target/release/amp-telemetry .

RUN chown -R amp:amp /app

USER amp

EXPOSE 9317

HEALTHCHECK --interval=30s --timeout=5s --start-period=5s --retries=3 \
    CMD bash -c 'echo > /dev/tcp/localhost/9317' || exit 1

ENTRYPOINT ["./amp-telemetry", "0.0.0.0:9317", "/app/data/telemetry.bin"]
