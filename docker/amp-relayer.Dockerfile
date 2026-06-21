FROM rust:1.87-slim AS builder

RUN apt-get update && \
    apt-get install -y --no-install-recommends capnproto git ca-certificates && \
    rm -rf /var/lib/apt/lists/*

# Phase 2.2 — Foundry pinned to a tagged release (no longer `master` HEAD).
# The previous Dockerfile printed a sha256 of the foundryup script but never
# checked it against anything (pure security theater). Now we install from a
# specific Foundry release tag and assert the binary version matches.
#
# To bump: pick a release from https://github.com/foundry-rs/foundry/releases,
# update FOUNDRY_VERSION below, and rebuild. Do NOT use `latest`.
ARG FOUNDRY_VERSION=v1.0.0
RUN curl -L -o /tmp/foundryup.sh https://raw.githubusercontent.com/foundry-rs/foundry/${FOUNDRY_VERSION}/foundryup/foundryup && \
    chmod +x /tmp/foundryup.sh && \
    FOUNDRY_DIR=/root/.foundry /tmp/foundryup.sh && \
    forge --version | grep -q "${FOUNDRY_VERSION#v}" || \
        (echo "ERROR: installed forge version does not match pinned ${FOUNDRY_VERSION}" && exit 1) && \
    rm /tmp/foundryup.sh
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
