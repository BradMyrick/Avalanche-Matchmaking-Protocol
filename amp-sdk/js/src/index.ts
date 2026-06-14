/**
 * AMP client SDK for Node.js.
 *
 * Transport: TCP via Node's `net.Socket`. Browsers cannot open raw TCP
 * sockets; a WebSocket transport will be added when the AMP server exposes
 * one (see issue tracker). For browser use today, front the AMP server with
 * a TLS-terminating WebSocket bridge.
 *
 * Threading: all methods are async; the event loop handles concurrency.
 * A single AmpClient is NOT safe to share across concurrent requests —
 * create one per active session.
 */

import * as net from "net";
import * as tls from "tls";
import { ethers } from "ethers";
import {
  computeOutcomeEip712Digest,
  type Eip712DomainParams,
} from "./eip712.js";

export { computeOutcomeEip712Digest, type Eip712DomainParams };

/**
 * Configuration for an AmpClient.
 */
export interface AmpClientOptions {
  /** host:port of the AMP matchmaker. */
  address: string;
  /** When true, use TLS (Node tls.connect). Implies SNI = host. */
  tls?: boolean;
  /** Optional TLS options (CA, cert, etc.). */
  tlsOptions?: tls.ConnectionOptions;
  /** Connect timeout in ms (default: 10_000). */
  connectTimeoutMs?: number;
  /** EIP-712 domain parameters for outcome signing. */
  domain?: Eip712DomainParams;
}

/**
 * Custom error classes for AMP failures.
 */
export class AmpError extends Error {
  constructor(message: string, public readonly cause?: unknown) {
    super(message);
    this.name = "AmpError";
  }
}
export class AmpConnectionError extends AmpError {}
export class AmpAuthError extends AmpError {}
export class AmpMatchError extends AmpError {}
export class AmpTimeoutError extends AmpError {}

/** Result of requestMatch. */
export interface MatchAssignment {
  matchId: string;
  quality: number;
}

/**
 * AMP client. Cap'n Proto RPC over TCP.
 *
 * NOTE: This SDK currently implements connect + EIP-712 outcome digest
 * helpers. Full Cap'n Proto RPC framing is in development; for now,
 * downstream tools use this module for digest computation and wallet
 * integration while the wire layer matures.
 */
export class AmpClient {
  private socket: net.Socket | tls.TLSSocket | null = null;
  private readonly options: AmpClientOptions;
  private connected = false;
  /** @internal */ playerAddress: string | null = null;
  private custodialSigner: ethers.Wallet | null = null;

  constructor(options: AmpClientOptions) {
    if (!options.address || !options.address.includes(":")) {
      throw new Error(`options.address must be host:port, got ${options.address}`);
    }
    this.options = options;
  }

  /**
   * Open the TCP/TLS connection to the AMP server.
   * Resolves once the socket is established (does NOT include any RPC
   * handshake — that happens lazily on the first RPC).
   */
  async connect(): Promise<void> {
    if (this.connected) return;
    const [host, portStr] = splitHostPort(this.options.address);
    const port = Number.parseInt(portStr, 10);
    const timeoutMs = this.options.connectTimeoutMs ?? 10_000;

    return new Promise<void>((resolve, reject) => {
      const socket = this.options.tls
        ? tls.connect({ host, port, ...(this.options.tlsOptions ?? {}) })
        : net.connect({ host, port });
      const timer = setTimeout(() => {
        socket.destroy();
        reject(new AmpTimeoutError(`connect to ${this.options.address} timed out`));
      }, timeoutMs);
      socket.once("connect", () => {
        clearTimeout(timer);
        this.socket = socket;
        this.connected = true;
        // Disable Nagle for low-latency RPC.
        socket.setNoDelay(true);
        resolve();
      });
      socket.once("error", (err) => {
        clearTimeout(timer);
        reject(new AmpConnectionError(`connect failed: ${err.message}`, err));
      });
    });
  }

  /** Close the underlying transport. Idempotent. */
  async close(): Promise<void> {
    if (!this.connected || !this.socket) return;
    await new Promise<void>((resolve) => {
      this.socket!.once("close", () => resolve());
      this.socket!.end();
    });
    this.socket = null;
    this.connected = false;
    this.playerAddress = null;
    this.custodialSigner = null;
  }

  /**
   * Authenticate via the EIP-191 challenge/response flow.
   * Either `privateKeyHex` (custodial) or `signCallback` (wallet) is required.
   * The SDK never generates a silent ephemeral identity.
   */
  async authenticate(
    _gameId: bigint,
    privateKeyHex?: string,
    signCallback?: (challenge: Uint8Array) => Promise<Uint8Array>,
  ): Promise<void> {
    // The full Cap'n Proto RPC framing for requestChallenge/login is in
    // development. This stub validates inputs and stores the signer so it
    // can be used by submitOutcome once the RPC layer is wired.
    if (!privateKeyHex && !signCallback) {
      throw new AmpAuthError(
        "authenticate requires either privateKeyHex (custodial) or signCallback (wallet). " +
          "AMP no longer generates silent ephemeral identities — see SECURITY_REVIEW.md S2.",
      );
    }
    if (privateKeyHex) {
      this.custodialSigner = new ethers.Wallet(privateKeyHex);
      this.playerAddress = await this.custodialSigner.getAddress();
    } else if (signCallback) {
      // Address recovery happens server-side; caller-supplied signers must
      // populate playerAddress via setCustodialAddress if they want outcome
      // signing via this client.
      this.playerAddress = null;
    }
    throw new AmpError(
      "authenticate() RPC framing is not yet implemented in the JS SDK; " +
        "use the Go or Python SDK for full client functionality. The JS SDK " +
        "currently supports connect() and computeOutcomeEip712Digest() / " +
        "signOutcome() helpers.",
    );
  }

  /**
   * Sign the EIP-712 outcome digest with the custodial wallet.
   * Requires `privateKeyHex` was passed to authenticate().
   */
  async signOutcome(
    matchId: string,
    outcome: number,
    transcriptHash: Uint8Array,
  ): Promise<Uint8Array> {
    if (!this.custodialSigner) {
      throw new AmpAuthError(
        "Outcome signing requires a custodial wallet (privateKeyHex at authenticate)",
      );
    }
    const digest = computeOutcomeEip712Digest(
      matchId,
      outcome,
      transcriptHash,
      this.options.domain,
    );
    // signHash signs a pre-computed digest (no EIP-191 re-prefix — EIP-712's
    // 0x1901 prefix is already in the digest).
    const sig = this.custodialSigner.signingKey.sign(digest);
    // ethers produces { r, s, v } with v in {0, 1}; ethers expects 27/28 for
    // wire-format compatibility. Use Signature.ser to get the canonical
    // 65-byte form with v already in 27/28 convention.
    return ethers.getBytes(ethers.Signature.from(sig).serialized);
  }
}

function splitHostPort(addr: string): [string, string] {
  // Handle bracketed IPv6: [::1]:50051
  if (addr.startsWith("[")) {
    const end = addr.indexOf("]");
    if (end < 0 || end + 1 >= addr.length || addr[end + 1] !== ":") {
      throw new Error(`malformed IPv6 address: ${addr}`);
    }
    return [addr.slice(1, end), addr.slice(end + 2)];
  }
  const colon = addr.lastIndexOf(":");
  if (colon < 0) return [addr, "50051"];
  return [addr.slice(0, colon), addr.slice(colon + 1)];
}
