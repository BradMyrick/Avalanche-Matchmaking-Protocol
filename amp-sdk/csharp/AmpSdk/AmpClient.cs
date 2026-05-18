using System;
using System.Threading;
using System.Threading.Tasks;
using CapnpGen;
using Capnp.Rpc;

namespace AmpSdk {

public class AmpClient : IDisposable {
    private readonly string _rpcUrl;

    private TcpRpcClient _rpcClient;
    private IGameSessionService _sessionService;
    private IUserSession _userSession;
    private IMatchSession _matchSession;

    private bool HasMatchSession => _matchSession != null;

    public AmpClient(string rpcUrl) {
        _rpcUrl = rpcUrl;
    }

    public async Task<bool> ConnectAsync(ulong gameId, byte[] playerSignature) {
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

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                _userSession = await _sessionService.Login(gameId, playerSignature, cts.Token);

                if (_userSession != null) return true;
            } catch (System.Exception) {
                _rpcClient?.Dispose();
                if (i < maxRetries - 1) await Task.Delay(2000);
            }
        }
        return false;
    }

    public async Task<string> CreateProfileAsync(PlayerProfileData profile) {
        return await Task.FromResult(profile.PlayerId);
    }

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

    public void Dispose() {
        _matchSession?.Dispose();
        _userSession?.Dispose();
        _sessionService?.Dispose();
        _rpcClient?.Dispose();
    }
}

}
