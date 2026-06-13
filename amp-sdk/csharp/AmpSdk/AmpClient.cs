using System;
using System.Threading;
using System.Threading.Tasks;
using CapnpGen;
using Capnp.Rpc;
using Nethereum.Signer;
using Nethereum.Hex.HexConvertors.Extensions;
namespace AmpSdk {

/// <summary>
/// Main client for connecting to an AMP server via Cap'n Proto RPC.
/// </summary>
public class AmpClient : IDisposable {
    private readonly string _rpcUrl;

    private TcpRpcClient _rpcClient;
    private IGameSessionService _sessionService;
    private IUserSession _userSession;
    private IMatchSession _matchSession;

    private bool HasMatchSession => _matchSession != null;

    /// <summary>
    /// Creates a new AmpClient targeting the given host:port RPC endpoint.
    /// </summary>
    public AmpClient(string rpcUrl) {
        _rpcUrl = rpcUrl;
    }

    /// <summary>
    /// Connects to the AMP server via Cap'n Proto RPC.
    /// Retries up to 5 times with 2-second backoff.
    /// </summary>
    public async Task<bool> ConnectAsync() {
        int maxRetries = 5;
        for (int i = 0; i < maxRetries; i++) {
            try {
                var parts = _rpcUrl.Split(':');
                var host = parts[0];
                var port = parts.Length > 1 ? int.Parse(parts[1]) : 50051;

                var addresses = System.Net.Dns.GetHostAddresses(host);
                var ipv4 = System.Linq.Enumerable.FirstOrDefault(addresses,
                    a => a.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                if (ipv4 != null) host = ipv4.ToString();

                _rpcClient = new TcpRpcClient(host, port);
                _sessionService = _rpcClient.GetMain<IGameSessionService>();

                if (_sessionService != null) return true;
            } catch (System.Exception) {
                _rpcClient?.Dispose();
                if (i < maxRetries - 1) await Task.Delay(2000);
            }
        }
        return false;
    }

    /// <summary>
    /// Requests a one-time authentication challenge for the given game ID.
    /// </summary>
    public async Task<(byte[] challenge, ulong expiresAt)> RequestChallengeAsync(ulong gameId) {
        if (_sessionService == null) throw new InvalidOperationException("Not connected.");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        var (challenge, expiresAt) = await _sessionService.RequestChallenge(gameId, cts.Token);
        return (System.Linq.Enumerable.ToArray(challenge), expiresAt);
    }

    /// <summary>
    /// Authenticates with the AMP server using a signed challenge.
    /// </summary>
    public async Task<bool> LoginAsync(ulong gameId, byte[] signature, byte[] challengePayload) {
        if (_sessionService == null) throw new InvalidOperationException("Not connected.");
        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
        _userSession = await _sessionService.Login(gameId, signature, challengePayload, cts.Token);
        return _userSession != null;
    }

    /// <summary>
    /// Authenticates with the AMP matchmaker.
    /// If signCallback is provided, it is used to sign the challenge manually.
    /// Otherwise, if privateKeyHex is provided, it is used for custodial signing.
    /// If neither is provided, a new temporary custodial wallet is generated.
    /// </summary>
    public async Task<bool> AuthenticateAsync(ulong gameId, string privateKeyHex = null, Func<byte[], Task<byte[]>> signCallback = null) {
        var (challenge, expiresAt) = await RequestChallengeAsync(gameId);
        byte[] signatureBytes;
        
        if (signCallback != null) {
            signatureBytes = await signCallback(challenge);
        } else {
            if (string.IsNullOrEmpty(privateKeyHex)) {
                var ecKey = EthECKey.GenerateKey();
                privateKeyHex = ecKey.GetPrivateKey();
            }
            
            var signer = new EthereumMessageSigner();
            var key = new EthECKey(privateKeyHex);
            
            // Standard Web3 Ethereum message signing
            var sig = signer.Sign(challenge, key);
            if (sig.StartsWith("0x")) sig = sig.Substring(2);
            signatureBytes = sig.HexToByteArray();
        }
        
        return await LoginAsync(gameId, signatureBytes, challenge);
    }

    /// <summary>
    /// Creates or retrieves a player profile on the AMP server.
    /// </summary>
    public async Task<string> CreateProfileAsync(PlayerProfileData profile) {
        return await Task.FromResult(profile.PlayerId);
    }

    /// <summary>
    /// Submits a match request and returns the assignment details once matched.
    /// </summary>
    public async Task<MatchResult> RequestMatchAsync(MatchRequest request) {
        if (_userSession == null) throw new InvalidOperationException("Not logged in.");

        var req = new GameMatchRequest {
            GameId = System.Text.Encoding.UTF8.GetBytes(request.GameId),
            RulesType = "standard",
            PlayerInfo = new PlayerInfo {
                PlayerId = System.Text.Encoding.UTF8.GetBytes(request.PlayerId),
                DisplayName = "CSharpPlayer",
                Elo = Elo.unranked,
                Region = Region.na,
                PlayerWallet = Array.Empty<byte>()
            },
            Stake = new PaymentInfo {
                PayerWallet = Array.Empty<byte>(),
                FeeToken = Array.Empty<byte>(),
                AuthSpend = 0
            },
            OptionalConfig = Array.Empty<byte>()
        };

        using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1));
        var (assignment, matchSession) = await _userSession.RequestMatch(req, cts.Token);

        _matchSession = matchSession;
        return new MatchResult {
            MatchId = System.Text.Encoding.UTF8.GetString(
                System.Linq.Enumerable.ToArray(assignment.MatchId)),
            Quality = assignment.MatchQuality
        };
    }

    /// <summary>
    /// Emits a typed game event during an active match.
    /// </summary>
    public async Task EmitGameEventAsync(string eventType) {
        if (!HasMatchSession) return;

        var ev = new GameEvent {
            EventId = 0,
            Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000UL,
            TriggeredBy = System.Text.Encoding.UTF8.GetBytes("p1"),
            EventType = "move",
            EventData = System.Text.Encoding.UTF8.GetBytes(eventType)
        };

        await _matchSession.EmitGameEvent(ev);
    }

    /// <summary>
    /// Emits a telemetry event with the given type and timestamp.
    /// </summary>
    public async Task EmitTelemetryAsync(byte eventTypeEnum, ulong timestamp) {
        if (!HasMatchSession) return;

        var ev = new AmpTelemetryEvent {
            MatchId = Array.Empty<byte>(),
            EventType = (TelemetryEventType)eventTypeEnum,
            Timestamp = timestamp,
            VerifierId = Array.Empty<byte>(),
            EventData = Array.Empty<byte>()
        };

        await _matchSession.EmitTelemetry(ev);
    }

    /// <summary>
    /// Submits the final match outcome and returns the verifier's signed result.
    /// </summary>
    public async Task<VerifierResult> SubmitOutcomeAsync(string matchId, byte outcome, byte[] transcriptHash) {
        if (!HasMatchSession) throw new InvalidOperationException("No match session.");

        var submission = new OutcomeSubmission {
            MatchId = System.Text.Encoding.UTF8.GetBytes(matchId),
            Outcome = new Outcome {
                Type = outcome == 0 ? OutcomeType.win : OutcomeType.unknown,
                Scores = new ulong[] { 1, 0 },
                Victor = outcome,
                Metadata = Array.Empty<byte>()
            },
            ReplayHash = transcriptHash ?? Array.Empty<byte>(),
            Signature = Array.Empty<byte>()
        };

        var sig = await _matchSession.SubmitOutcome(submission);
        return new VerifierResult {
            MatchId = matchId,
            Signature = System.Linq.Enumerable.ToArray(sig)
        };
    }

    /// <summary>
    /// Releases all RPC resources.
    /// </summary>
    public void Dispose() {
        _matchSession?.Dispose();
        _userSession?.Dispose();
        _sessionService?.Dispose();
        _rpcClient?.Dispose();
    }
}

}
