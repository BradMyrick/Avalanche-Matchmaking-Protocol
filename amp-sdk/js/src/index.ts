/**
 * AMP client SDK for Node.js (native, via napi-rs).
 *
 * This module wraps the native Rust SDK binding (`amp-native`) with the
 * canonical EIP-191 challenge signing and EIP-712 outcome signing (via
 * ethers). The Cap'n Proto RPC, LocalSet event loop, and all wire framing
 * run inside the native module on a dedicated worker thread.
 *
 * Crypto (challenge + outcome signing) is done in JS with ethers so games can
 * substitute their own wallet / signer. The native module handles transport.
 */

import { ethers } from "ethers";
import * as native from "../native/amp-native.js";

export { computeOutcomeEip712Digest } from "../native/amp-native.js";
export type { MatchEventPayload } from "../native/amp-native.js";

/** EIP-712 domain parameters for outcome signing. */
export interface Eip712DomainParams {
  /** Chain id (e.g. 43113 for Fuji). */
  chainId: number;
  /** 20-byte AMPSettlement address (hex or 20 bytes). */
  verifyingContract: string | Uint8Array;
}

/** Configuration for an AmpClient. */
export interface AmpClientOptions {
  /** host:port of the AMP matchmaker. */
  address: string;
  /** EIP-712 domain for outcome signing. Required before `submitOutcome`. */
  domain?: Eip712DomainParams;
}

/** Custom error classes. */
export class AmpError extends Error {
  constructor(message: string, public readonly cause?: unknown) {
    super(message);
    this.name = "AmpError";
  }
}
export class AmpConnectionError extends AmpError {}
export class AmpAuthError extends AmpError {}
export class AmpMatchError extends AmpError {}

/** Result of requestMatch. */
export interface MatchAssignment {
  matchId: string;
  quality: number;
}

/** A settled match event delivered to `subscribeToEvents`. */
export interface MatchSettledEvent {
  victor: number;
  scores: number[];
}

/** Listener handles for `subscribeToEvents`. */
export interface MatchEventListeners {
  onSettled?: (event: MatchSettledEvent) => void;
  onOpponentDisconnected?: () => void;
}

function toAddressBytes(v: string | Uint8Array): Uint8Array {
  if (typeof v === "string") {
    return ethers.getBytes(v);
  }
  return v;
}

/** Convert any byte-like value to a Node Buffer (the native binding's type). */
function toBuf(v: Uint8Array): Buffer {
  return Buffer.isBuffer(v) ? v : Buffer.from(v);
}

/**
 * AMP client. Native Cap'n Proto RPC over TCP (or TLS), driven on a worker
 * thread. One AmpClient per active session.
 */
export class AmpClient {
  private readonly native: native.AmpClient;
  private readonly options: AmpClientOptions;
  private wallet: ethers.Wallet | null = null;
  private eventLoop: Promise<void> | null = null;

  constructor(options: AmpClientOptions) {
    if (!options.address || !options.address.includes(":")) {
      throw new AmpError(`options.address must be host:port, got ${options.address}`);
    }
    this.options = options;
    this.native = new native.AmpClient();
  }

  /** Open the transport to the AMP server. */
  async connect(): Promise<void> {
    try {
      await this.native.connect(this.options.address);
    } catch (e) {
      throw new AmpConnectionError(`connect failed: ${(e as Error).message}`, e);
    }
  }

  /** Close the transport and release the worker. Idempotent. */
  async close(): Promise<void> {
    this.eventLoop = null;
    await this.native.close();
    this.wallet = null;
  }

  /**
   * Authenticate via the EIP-191 challenge/response flow.
   * Either `privateKeyHex` (custodial) or `signCallback` (wallet) is required.
   */
  async authenticate(
    gameId: number,
    privateKeyHex?: string,
    signCallback?: (challenge: Uint8Array) => Promise<Uint8Array>,
  ): Promise<void> {
    if (!privateKeyHex && !signCallback) {
      throw new AmpAuthError(
        "authenticate requires either privateKeyHex (custodial) or signCallback (wallet).",
      );
    }
    if (privateKeyHex) {
      this.wallet = new ethers.Wallet(privateKeyHex);
    }
    const { bytes: challenge, expiresAtNs } = await this.native.requestChallenge(gameId);
    if (expiresAtNs > 0 && expiresAtNs <= Date.now() * 1_000_000) {
      throw new AmpAuthError("challenge expired before signing");
    }
    const sigHex = privateKeyHex
      ? await this.wallet!.signMessage(challenge)
      : ethers.hexlify(await signCallback!(challenge));
    const sigBytes = ethers.getBytes(sigHex);
    if (sigBytes.length !== 65) {
      throw new AmpAuthError(`signature must be 65 bytes, got ${sigBytes.length}`);
    }
    try {
      await this.native.login(gameId, toBuf(sigBytes), challenge);
    } catch (e) {
      throw new AmpAuthError(`login failed: ${(e as Error).message}`, e);
    }
  }

  /** Enter the matchmaking queue. Resolves when paired. */
  async requestMatch(gameId: string): Promise<MatchAssignment> {
    try {
      const r = await this.native.requestMatch(gameId);
      return { matchId: r.matchId, quality: r.quality };
    } catch (e) {
      throw new AmpMatchError(`requestMatch failed: ${(e as Error).message}`, e);
    }
  }

  /** Reconnect to an existing active match. */
  async reconnect(matchId: string): Promise<void> {
    try {
      await this.native.reconnect(matchId);
    } catch (e) {
      throw new AmpMatchError(`reconnect failed: ${(e as Error).message}`, e);
    }
  }

  /**
   * Submit the final outcome. Computes the EIP-712 submitter signature over
   * (matchId, outcome, transcriptHash) with the custodial wallet and sends it.
   */
  async submitOutcome(
    matchId: string,
    outcome: number,
    transcriptHash: Uint8Array,
  ): Promise<Uint8Array> {
    if (!this.wallet) {
      throw new AmpAuthError("submitOutcome requires a custodial wallet (privateKeyHex at authenticate)");
    }
    if (!(transcriptHash instanceof Uint8Array) || transcriptHash.length !== 32) {
      throw new AmpError("transcriptHash must be a 32-byte Uint8Array");
    }
    if (!this.options.domain) {
      throw new AmpError("options.domain (chainId + verifyingContract) is required for outcome signing");
    }
    const verifyingContract = toAddressBytes(this.options.domain.verifyingContract);
    if (verifyingContract.length !== 20) {
      throw new AmpError("domain.verifyingContract must be 20 bytes");
    }
    const digest = native.computeOutcomeEip712Digest(
      matchId,
      outcome,
      toBuf(transcriptHash),
      this.options.domain.chainId,
      toBuf(verifyingContract),
    );
    const sig = this.wallet.signingKey.sign(digest);
    const sigBytes = ethers.getBytes(ethers.Signature.from(sig).serialized);
    try {
      return await this.native.submitOutcome(matchId, outcome, toBuf(transcriptHash), toBuf(sigBytes));
    } catch (e) {
      throw new AmpMatchError(`submitOutcome failed: ${(e as Error).message}`, e);
    }
  }

  /** Fire-and-forget game event. */
  async emitGameEvent(eventType: string): Promise<void> {
    await this.native.emitGameEvent(eventType);
  }

  /** Fire-and-forget telemetry event. */
  async emitTelemetry(eventType: number): Promise<void> {
    await this.native.emitTelemetry(eventType);
  }

  /**
   * Subscribe to server-pushed match events. Runs an internal poll loop on the
   * JS side (the native module exposes a poll-based event API). The returned
   * promise resolves when the event stream ends (match settled or channel
   * closed); call after a match is active.
   */
  async subscribeToEvents(listeners: MatchEventListeners): Promise<void> {
    await this.native.startEvents();
    this.eventLoop = (async () => {
      for (;;) {
        const evt = await this.native.pollEvent(1000);
        if (evt === null) continue;
        if (evt.kind === "settled") {
          listeners.onSettled?.({ victor: evt.victor, scores: evt.scores });
          return;
        }
        if (evt.kind === "opponent_disconnected") {
          listeners.onOpponentDisconnected?.();
        }
      }
    })();
    await this.eventLoop;
  }
}

// Re-export the raw native client for advanced users who want the poll-based
// event API without the ethers/crypto wrapper.
export { AmpClient as NativeAmpClient } from "../native/amp-native.js";
