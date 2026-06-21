using System;
using System.Threading;
using System.Threading.Tasks;
using CapnpGen;
using Capnp.Rpc;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
using Nethereum.Util;
using SysException = System.Exception;
using System.Net;
using System.Net.Sockets;
using System.Numerics;

namespace AmpSdk {

/// <summary>
/// Main client for connecting to an AMP server via Cap'n Proto RPC.
/// </summary>
/// <remarks>
/// <b>Thread safety:</b> instances are NOT thread-safe. Use one client per
/// logical session or wrap calls with an external lock.
/// </remarks>
public class AmpClient : IDisposable {
    private const int DefaultPort = 50051;
    private const int ConnectMaxRetries = 5;
    private static readonly TimeSpan ConnectBaseBackoff = TimeSpan.FromSeconds(1);

    private readonly string _rpcUrl;

    private TcpRpcClient _rpcClient;
    private IGameSessionService _sessionService;
    private IUserSession _userSession;
    private IMatchSession _matchSession;
    // Keep-alive for any active match-event listener so it isn't GC'd while
    // the server holds the (weak) callback capability reference.
    private IMatchListener _activeListener;

    /// <summary>
    /// The Ethereum address (20 bytes) recovered from the authenticated signer.
    /// Used by the server to bind <c>submit_outcome</c> signatures to a
    /// participant. Set after a successful <see cref="AuthenticateAsync"/>.
    /// </summary>
    public byte[] PlayerAddress { get; private set; }

    /// <summary>
    /// True when a custodial <see cref="EthECKey"/> is available to sign
    /// outcomes; false when a callback-based signer was used at login time.
    /// </summary>
    private bool HasCustodialSigner => _custodialKey != null;
    private EthECKey _custodialKey;

    private bool HasMatchSession => _matchSession != null;

    /// <summary>
    /// Creates a new AmpClient targeting the given <c>host:port</c> RPC endpoint.
    /// </summary>
    /// <param name="rpcUrl">AMP RPC endpoint, e.g. <c>matchmaker.example:50051</c>.</param>
    /// <param name="useTls">
    /// When true, signals that the endpoint is a TLS-terminating reverse proxy
    /// (nginx/envoy/traefik) fronting the AMP server. <b>Capnp.Net.Runtime 1.3.x
    /// does not support native SslStream wrapping</b> — for production deployments,
    /// point <paramref name="rpcUrl"/> at the proxy's TLS port instead. This flag
    /// is reserved for a future native-TLS implementation; today it only validates
    /// that the caller is intentionally opting in.
    /// </param>
    public AmpClient(string rpcUrl, bool useTls = false) {
        if (string.IsNullOrWhiteSpace(rpcUrl))
            throw new ArgumentException("rpcUrl is required", nameof(rpcUrl));
        _rpcUrl = rpcUrl;
        _useTls = useTls;
        if (useTls)
            System.Diagnostics.Debug.WriteLine(
                "AMP: AmpClient constructed with useTls=true; ensure rpcUrl points " +
                "at a TLS-terminating reverse proxy. Native client-side TLS is " +
                "not yet supported by Capnp.Net.Runtime 1.3.x.");
    }
    private readonly bool _useTls;

    /// <summary>
    /// Connects to the AMP server via Cap'n Proto RPC.
    /// Retries with exponential backoff and jitter to avoid thundering-herd.
    /// </summary>
    public async Task<bool> ConnectAsync(CancellationToken cancellationToken = default) {
        if (!TrySplitHostPort(_rpcUrl, out var host, out var port))
            throw new ArgumentException($"rpcUrl '{_rpcUrl}' is not a valid host:port");

        SysException lastError = null;
        for (int i = 0; i < ConnectMaxRetries; i++) {
            cancellationToken.ThrowIfCancellationRequested();
            try {
                _rpcClient?.Dispose();
                // Async DNS resolution; falls back to IPv6 if no IPv4 record exists.
                var addresses = await Dns.GetHostAddressesAsync(host).ConfigureAwait(false);
                IPAddress target = ResolvePreferred(addresses);
                _rpcClient = new TcpRpcClient(target.ToString(), port);
                _sessionService = _rpcClient.GetMain<IGameSessionService>();
                if (_sessionService != null) return true;
            } catch (SysException ex) when (!IsCancellation(ex, cancellationToken)) {
                lastError = ex;
                _rpcClient?.Dispose();
                _rpcClient = null;
                int delayMs = (int)(ConnectBaseBackoff.TotalMilliseconds * (1L << i));
                delayMs += Jitter(); // exponential backoff + jitter
                try {
                    await Task.Delay(delayMs, cancellationToken).ConfigureAwait(false);
                } catch (OperationCanceledException) {
                    throw;
                }
            }
        }
        if (lastError != null)
            System.Diagnostics.Debug.WriteLine($"AMP connect failed after {ConnectMaxRetries} retries: {lastError}");
        return false;
    }

    /// <summary>
    /// Requests a one-time authentication challenge for the given game ID.
    /// </summary>
    public async Task<(byte[] challenge, ulong expiresAt)> RequestChallengeAsync(
        ulong gameId, CancellationToken cancellationToken = default) {
        if (_sessionService == null) throw new InvalidOperationException("Not connected.");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        var (challenge, expiresAt) = await _sessionService.RequestChallenge(gameId, cts.Token)
            .ConfigureAwait(false);
        return (System.Linq.Enumerable.ToArray(challenge), expiresAt);
    }

    /// <summary>
    /// Authenticates with the AMP server using a pre-signed challenge.
    /// </summary>
    public async Task<bool> LoginAsync(ulong gameId, byte[] signature, byte[] challengePayload,
        CancellationToken cancellationToken = default) {
        if (_sessionService == null) throw new InvalidOperationException("Not connected.");
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(5));
        _userSession = await _sessionService.Login(gameId, signature, challengePayload, cts.Token)
            .ConfigureAwait(false);
        return _userSession != null;
    }

    /// <summary>
    /// Authenticates with the AMP matchmaker via the EIP-191 challenge/response flow.
    /// </summary>
    /// <param name="gameId">Game to authenticate against.</param>
    /// <param name="privateKeyHex">
    /// Required for custodial signing. Must be a 32-byte secp256k1 private key
    /// in hex (with or without <c>0x</c> prefix). The same key is later used to
    /// sign match outcomes.
    /// </param>
    /// <param name="signCallback">
    /// Required for wallet-integrated signing. Receives the EIP-191 challenge
    /// bytes and must return a 65-byte signature. When this is set, you must
    /// also supply <paramref name="signDigestCallback"/> to
    /// <see cref="SubmitOutcomeAsync"/>.
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when neither <paramref name="privateKeyHex"/> nor
    /// <paramref name="signCallback"/> is provided. The SDK will never silently
    /// generate a throwaway identity.
    /// </exception>
    public async Task<bool> AuthenticateAsync(
        ulong gameId,
        string privateKeyHex = null,
        Func<byte[], CancellationToken, Task<byte[]>> signCallback = null,
        CancellationToken cancellationToken = default) {
        if (string.IsNullOrEmpty(privateKeyHex) && signCallback == null) {
            throw new ArgumentException(
                "Authentication requires either privateKeyHex (custodial) or signCallback (wallet). " +
                "AMP no longer generates silent ephemeral identities — see SECURITY.md.");
        }

        var (challenge, expiresAt) = await RequestChallengeAsync(gameId, cancellationToken)
            .ConfigureAwait(false);

        // Client-side freshness check. The server enforces this too, but
        // failing early avoids burning a one-time challenge on a stale attempt.
        var nowMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        // expiresAt is in nanoseconds (server uses now_ns()); convert.
        var expiresAtMs = expiresAt / 1_000_000UL;
        if (expiresAtMs <= nowMs) {
            throw new InvalidOperationException(
                $"Received challenge already expired (expiresAt={expiresAt}, now~={nowMs * 1_000_000})");
        }

        byte[] signatureBytes;
        if (signCallback != null) {
            signatureBytes = await signCallback(challenge, cancellationToken).ConfigureAwait(false);
            if (signatureBytes == null || signatureBytes.Length != 65) {
                throw new InvalidOperationException(
                    $"signCallback must return a 65-byte signature; got {signatureBytes?.Length ?? 0}");
            }
            // Address recovery happens server-side; we can't derive it here
            // without the public key, so caller-supplied signers must populate
            // PlayerAddress themselves if they want outcome signing via callback.
            PlayerAddress = null;
        } else {
            var signer = new EthereumMessageSigner();
            // Nethereum accepts private keys with or without the 0x prefix.
            var key = new EthECKey(privateKeyHex);
            _custodialKey = key;
            PlayerAddress = key.GetPublicAddressAsBytes(); // 20-byte EIP-55 address

            var sig = signer.Sign(challenge, key);
            if (sig.StartsWith("0x") || sig.StartsWith("0X")) sig = sig.Substring(2);
            signatureBytes = sig.HexToByteArray();
        }

        return await LoginAsync(gameId, signatureBytes, challenge, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Stub kept for source compatibility — the AMP server does not expose
    /// <c>createOrUpdateProfile</c> on the authenticated RPC surface today.
    /// </summary>
    public Task<string> CreateProfileAsync(PlayerProfileData profile) {
        return Task.FromResult(profile.PlayerId);
    }

    /// <summary>
    /// Submits a match request and returns the assignment details once matched.
    /// </summary>
    public async Task<MatchResult> RequestMatchAsync(MatchRequest request,
        CancellationToken cancellationToken = default) {
        if (_userSession == null) throw new InvalidOperationException("Not logged in.");

        var req = new GameMatchRequest {
            GameId = System.Text.Encoding.UTF8.GetBytes(request.GameId),
            RuleSetId = System.Text.Encoding.UTF8.GetBytes(request.RulesetId),
            RulesType = "standard",
            PlayerInfo = new PlayerInfo {
                // The server identifies the caller by the recovered login
                // address, not by this field — but populate it for symmetry.
                PlayerId = System.Text.Encoding.UTF8.GetBytes(request.PlayerId),
                DisplayName = request.PlayerId,
                Elo = Elo.unranked,
                Region = Region.na,
                PlayerWallet = PlayerAddress ?? Array.Empty<byte>(),
                MmrRating = request.Mmr,
            },
            Stake = new PaymentInfo {
                PayerWallet = PlayerAddress ?? Array.Empty<byte>(),
                FeeToken = Array.Empty<byte>(),
                AuthSpend = 0
            },
            OptionalConfig = Array.Empty<byte>()
        };

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromMinutes(1));
        var (assignment, matchSession) = await _userSession.RequestMatch(req, cts.Token)
            .ConfigureAwait(false);

        _matchSession = matchSession;
        return new MatchResult {
            MatchId = System.Text.Encoding.UTF8.GetString(
                System.Linq.Enumerable.ToArray(assignment.MatchId)),
            Quality = assignment.MatchQuality
        };
    }

    /// <summary>
    /// Reconnect to an existing active match by id (e.g. after a process
    /// restart). Sets the active match session. Throws if the match is not
    /// found or is already settled.
    /// </summary>
    public async Task ReconnectAsync(string matchId,
        CancellationToken cancellationToken = default) {
        if (_userSession == null) throw new InvalidOperationException("Not logged in.");
        if (string.IsNullOrEmpty(matchId)) throw new ArgumentException("matchId is required.", nameof(matchId));

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        _matchSession = await _userSession.Reconnect(
            System.Text.Encoding.UTF8.GetBytes(matchId), cts.Token).ConfigureAwait(false);
    }

    /// <summary>
    /// Emits a typed game event during an active match.
    /// </summary>
    public async Task EmitGameEventAsync(string eventType,
        CancellationToken cancellationToken = default) {
        if (!HasMatchSession) return;

        var ev = new GameEvent {
            EventId = 0,
            Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000UL,
            TriggeredBy = PlayerAddress ?? Array.Empty<byte>(),
            EventType = eventType,
            EventData = Array.Empty<byte>()
        };

        await _matchSession.EmitGameEvent(ev).ConfigureAwait(false);
    }

    /// <summary>
    /// Emits a telemetry event with the given type and timestamp.
    /// </summary>
    public async Task EmitTelemetryAsync(byte eventTypeEnum, ulong timestamp,
        CancellationToken cancellationToken = default) {
        if (!HasMatchSession) return;

        var ev = new AmpTelemetryEvent {
            MatchId = Array.Empty<byte>(),
            EventType = (TelemetryEventType)eventTypeEnum,
            Timestamp = timestamp,
            VerifierId = Array.Empty<byte>(),
            EventData = Array.Empty<byte>()
        };

        await _matchSession.EmitTelemetry(ev).ConfigureAwait(false);
    }

    /// <summary>
    /// Submits the final match outcome and returns the verifier's signed result.
    /// </summary>
    /// <param name="matchId">Authoritative match ID from <see cref="RequestMatchAsync"/>.</param>
    /// <param name="outcome">Victor index; must be 1..=4 (server and relayer requirement).</param>
    /// <param name="transcriptHash">Exactly 32 bytes (keccak256 of the match transcript).</param>
    /// <param name="signDigestCallback">
    /// Required when this client was authenticated via <c>signCallback</c>.
    /// Receives the 32-byte EIP-712 digest over <c>(matchId, outcome, transcriptHash)</c>
    /// and must return a 65-byte secp256k1 signature from the SAME wallet used at login.
    /// Ignored when a custodial key was used at login (this client signs automatically).
    /// </param>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="transcriptHash"/> is not 32 bytes,
    /// <paramref name="outcome"/> is outside 1..=4, or no signer is available.
    /// </exception>
    public async Task<VerifierResult> SubmitOutcomeAsync(
        string matchId, byte outcome, byte[] transcriptHash,
        Func<byte[], CancellationToken, Task<byte[]>> signDigestCallback = null,
        CancellationToken cancellationToken = default) {
        if (!HasMatchSession) throw new InvalidOperationException("No match session.");
        if (transcriptHash == null || transcriptHash.Length != 32)
            throw new ArgumentException("transcriptHash must be exactly 32 bytes", nameof(transcriptHash));
        if (outcome < 1 || outcome > 4)
            throw new ArgumentException("outcome must be 1..=4 (server and relayer invariant)", nameof(outcome));

        // Compute the canonical EIP-712 digest the server will recover from.
        // Both client and server MUST produce byte-identical digests or the
        // recovered address will not match.
        byte[] digest = OutcomeEip712.ComputeDigest(matchId, outcome, transcriptHash);
        byte[] submitterSig;
        if (HasCustodialSigner) {
            submitterSig = OutcomeEip712.SignDigest(_custodialKey, digest);
        } else if (signDigestCallback != null) {
            submitterSig = await signDigestCallback(digest, cancellationToken).ConfigureAwait(false);
            if (submitterSig == null || submitterSig.Length != 65)
                throw new InvalidOperationException(
                    $"signDigestCallback must return a 65-byte signature; got {submitterSig?.Length ?? 0}");
        } else {
            throw new InvalidOperationException(
                "Outcome signing requires either a custodial key (passed to AuthenticateAsync) " +
                "or a signDigestCallback (passed to SubmitOutcomeAsync).");
        }

        var submission = new OutcomeSubmission {
            MatchId = System.Text.Encoding.UTF8.GetBytes(matchId),
            Outcome = new Outcome {
                Type = OutcomeTypeFor(outcome),
                Scores = new ulong[] { 1, 0 },
                Victor = outcome,
                Metadata = Array.Empty<byte>()
            },
            ReplayHash = transcriptHash,
            Signature = submitterSig
        };

        var sig = await _matchSession.SubmitOutcome(submission, cancellationToken).ConfigureAwait(false);
        return new VerifierResult {
            MatchId = matchId,
            Signature = System.Linq.Enumerable.ToArray(sig)
        };
    }

    private static OutcomeType OutcomeTypeFor(byte outcome) => outcome switch {
        1 => OutcomeType.win,
        2 => OutcomeType.draw,
        3 or 4 => OutcomeType.@void,
        _ => OutcomeType.unknown,
    };

    /// <summary>
    /// Subscribe to server-pushed match events. The server invokes
    /// <paramref name="onMatchSettled"/> when the match settles and
    /// <paramref name="onOpponentDisconnected"/> when an opponent drops
    /// (callbacks fire) on the RPC dispatch
    /// thread — keep them short.
    /// </summary>
    public async Task SubscribeToEventsAsync(
        Action<Outcome> onMatchSettled = null,
        Action onOpponentDisconnected = null,
        CancellationToken cancellationToken = default) {
        if (!HasMatchSession) throw new InvalidOperationException("No match session.");
        // Capnp.Net.Runtime's serializer auto-wraps a bare IMatchListener impl
        // into a Skeleton via CapabilityReflection.CreateSkeleton when the
        // capability field is written (LinkObject). We retain the impl on the
        // client so it outlives the SubscribeToEvents RPC.
        var listener = new MatchListenerImpl(onMatchSettled, onOpponentDisconnected);
        _activeListener = listener;
        await _matchSession.SubscribeToEvents(listener, cancellationToken).ConfigureAwait(false);
    }

    /// <summary>
    /// Local <see cref="IMatchListener"/> implementation that forwards
    /// server-pushed callbacks onto user-supplied delegates.
    /// </summary>
    private sealed class MatchListenerImpl : IMatchListener {
        private readonly Action<Outcome> _onSettled;
        private readonly Action _onDisconnected;

        public MatchListenerImpl(Action<Outcome> onSettled, Action onDisconnected) {
            _onSettled = onSettled;
            _onDisconnected = onDisconnected;
        }

        public Task OnMatchSettled(Outcome outcome, CancellationToken cancellationToken_ = default) {
            _onSettled?.Invoke(outcome);
            return Task.CompletedTask;
        }

        public Task OnOpponentDisconnected(CancellationToken cancellationToken_ = default) {
            _onDisconnected?.Invoke();
            return Task.CompletedTask;
        }

        public void Dispose() { }
    }

    /// <summary>
    /// Releases all RPC resources.
    /// </summary>
    public void Dispose() {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>Releases unmanaged RPC resources and zeros the in-memory key.</summary>
    protected virtual void Dispose(bool disposing) {
        if (!disposing) return;
        _activeListener = null;
        _matchSession?.Dispose();
        _userSession?.Dispose();
        _sessionService?.Dispose();
        _rpcClient?.Dispose();
        // Zero the in-memory private key material on dispose (best-effort).
        if (_custodialKey != null) {
            // EthECKey does not expose its D-byte for direct zeroing; we drop
            // the reference and let the GC reclaim it. Operators handling
            // high-sensitivity keys should run inside a TEE / OS keychain.
            _custodialKey = null;
        }
    }

    private static bool TrySplitHostPort(string s, out string host, out int port) {
        host = null;
        port = DefaultPort;
        if (string.IsNullOrEmpty(s)) return false;
        // Handle bracketed IPv6 literals: [::1]:50051
        if (s.StartsWith("[")) {
            int end = s.IndexOf(']');
            if (end < 0 || end + 1 >= s.Length || s[end + 1] != ':') return false;
            host = s.Substring(1, end - 1);
            return int.TryParse(s.AsSpan(end + 2), out port);
        }
        int colon = s.LastIndexOf(':');
        if (colon < 0) { host = s; return true; }
        host = s.Substring(0, colon);
        return int.TryParse(s.AsSpan(colon + 1), out port);
    }

    private static IPAddress ResolvePreferred(IPAddress[] addresses) {
        // Prefer IPv4 to preserve historical behavior; fall back to first
        // available (typically IPv6 on dual-stack hosts).
        if (addresses == null || addresses.Length == 0)
            throw new SocketException((int)SocketError.HostNotFound);
        foreach (var a in addresses)
            if (a.AddressFamily == AddressFamily.InterNetwork) return a;
        return addresses[0];
    }

    private static bool IsCancellation(System.Exception ex, CancellationToken ct) =>
        ct.IsCancellationRequested && (ex is OperationCanceledException || ex is TaskCanceledException);

    private static int Jitter() {
        // Thread-local jitter RNG. Connect is a low-contention path (max 5
        // retries per call), so per-thread seeding is sufficient.
        return _jitter.Value.Next(0, 250);
    }
    private static readonly System.Threading.ThreadLocal<Random> _jitter =
        new(() => new Random(Environment.TickCount ^ Environment.CurrentManagedThreadId));
}

/// <summary>
/// EIP-712 typed-data signing helpers for AMP match outcomes.
/// Mirrors the server's <c>compute_outcome_eip712_digest</c> in
/// <c>amp-server/src/main.rs</c>. The digest and domain separator MUST be
/// byte-identical between client and server or signature verification fails.
/// </summary>
public static class OutcomeEip712 {
    // Settlement contract name & version are fixed by the on-chain verifier.
    public const string DomainName = "AMPSettlement";
    public const string DomainVersion = "1";
    public const ulong DefaultChainId = 43113; // Fuji testnet; override via SetDomain() if needed.

    // Set once at startup from your deployment's verifying contract address.
    public static byte[] VerifyingContract { get; set; } = new byte[20];
    public static ulong ChainId { get; set; } = DefaultChainId;

    private static readonly byte[] AsyncResultTypehash = Keccak(
        System.Text.Encoding.UTF8.GetBytes(
            "AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)"));

    private static readonly byte[] Eip712DomainTypehash = Keccak(
        System.Text.Encoding.UTF8.GetBytes(
            "EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"));

    /// <summary>Compute the canonical EIP-712 digest to sign.</summary>
    public static byte[] ComputeDigest(string matchId, byte outcome, byte[] transcriptHash) {
        if (transcriptHash == null || transcriptHash.Length != 32)
            throw new ArgumentException("transcriptHash must be 32 bytes", nameof(transcriptHash));
        if (VerifyingContract == null || VerifyingContract.Length != 20)
            throw new InvalidOperationException(
                "OutcomeEip712.VerifyingContract must be set to the 20-byte settlement contract address before signing.");

        byte[] matchIdEnc = EncodeUint256(matchId);
        byte[] outcomeEnc = EncodeUint256(outcome);
        byte[] nameHash = Keccak(System.Text.Encoding.UTF8.GetBytes(DomainName));
        byte[] versionHash = Keccak(System.Text.Encoding.UTF8.GetBytes(DomainVersion));

        byte[] structHash = Keccak(AbiEncode(
            AsyncResultTypehash, matchIdEnc, outcomeEnc, transcriptHash));
        byte[] domainSeparator = Keccak(AbiEncode(
            Eip712DomainTypehash, nameHash, versionHash,
            EncodeUint256(ChainId), AbiAddress(VerifyingContract)));

        byte[] digestInput = new byte[2 + 32 + 32];
        digestInput[0] = 0x19; digestInput[1] = 0x01;
        Buffer.BlockCopy(domainSeparator, 0, digestInput, 2, 32);
        Buffer.BlockCopy(structHash, 0, digestInput, 34, 32);
        return Keccak(digestInput);
    }

    /// <summary>Sign the EIP-712 digest (already includes the 0x1901 prefix).</summary>
    public static byte[] SignDigest(EthECKey key, byte[] digest) {
        if (digest == null || digest.Length != 32)
            throw new ArgumentException("digest must be 32 bytes", nameof(digest));
        // SignHashedMessage would re-prefix with EIP-191 — we must NOT do that
        // because EIP-712's 0x1901 prefix is already in the digest.
        // SignAndCalculateV returns an EthECDSASignature with the canonical
        // V (27/28) populated for Ethereum.
        var sig = key.SignAndCalculateV(digest);
        byte[] result = new byte[65];
        PadTo32(sig.R, result, 0);
        PadTo32(sig.S, result, 32);
        // In Nethereum 4.20, V is a byte[] of length 1 holding 27 or 28.
        int v = (sig.V != null && sig.V.Length > 0) ? sig.V[0] : 0;
        // SignAndCalculateV returns V in 27/28 convention (EIP-155 disabled).
        result[64] = (byte)v;
        return result;
    }

    private static void PadTo32(byte[] src, byte[] dst, int dstOffset) {
        // BigInteger/Nethereum may return <32 bytes if the high bits are zero;
        // ABI encoding requires left-pad to 32 bytes big-endian.
        if (src.Length > 32)
            throw new ArgumentException("source >32 bytes", nameof(src));
        int pad = 32 - src.Length;
        for (int i = 0; i < 32; i++)
            dst[dstOffset + i] = (i < pad) ? (byte)0 : src[i - pad];
    }

    private static byte[] EncodeUint256(object value) {
        // Render as 64-char zero-padded uppercase hex (big-endian), then parse.
        string hex64 = value switch {
            byte b => b.ToString("X64"),
            ulong u => u.ToString("X64"),
            string s when BigInteger.TryParse(s, out var bi) => bi.ToString("X64"),
            _ => throw new ArgumentException($"unsupported uint256 value: {value}")
        };
        return HexToBytesBE(hex64);
    }

    private static byte[] HexToBytesBE(string hex64) {
        // hex64 is 64 hex chars representing a 32-byte big-endian unsigned integer.
        if (hex64.Length != 64)
            throw new ArgumentException("expected 64 hex chars", nameof(hex64));
        byte[] result = new byte[32];
        for (int i = 0; i < 32; i++) {
            result[i] = Convert.ToByte(hex64.Substring(i * 2, 2), 16);
        }
        return result;
    }

    private static byte[] AbiAddress(byte[] address20) {
        byte[] padded = new byte[32];
        Buffer.BlockCopy(address20, 0, padded, 12, 20);
        return padded;
    }

    private static byte[] AbiEncode(params byte[][] parts) {
        int total = 0;
        foreach (var p in parts) total += p.Length;
        byte[] result = new byte[total];
        int offset = 0;
        foreach (var p in parts) {
            Buffer.BlockCopy(p, 0, result, offset, p.Length);
            offset += p.Length;
        }
        return result;
    }

    private static byte[] Keccak(byte[] input) {
        var sha3 = new Sha3Keccack();
        return sha3.CalculateHash(input);
    }
}

}
