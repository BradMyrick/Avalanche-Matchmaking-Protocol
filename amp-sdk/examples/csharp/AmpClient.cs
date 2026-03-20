using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

// Expected namespaces generated from Cap'n Proto schemas
using CapnpGen;
using Capnp.Rpc;

namespace AmpSdkExample
{
    public class AmpClient : IDisposable
    {
        private readonly string _serverAddress;
        
        private TcpRpcClient? _rpcClient;
        private IGameSessionService? _sessionService;
        private IUserSession? _userSession;
        private IMatchSession? _matchSession;

        // Flag for tracking whether we can emit securely
        private bool _hasMatchSession => _matchSession != null;

        public AmpClient(string address)
        {
            _serverAddress = address;
        }

        /// <summary>
        /// Connects to the matchmaker and performs the initial login handshake.
        /// Demonstrates how a capability-based session starts.
        /// </summary>
        public async Task<bool> ConnectAsync(ulong gameId, byte[] playerSignature)
        {
            Console.WriteLine($"[AmpClient] Connecting to matchmaker at {_serverAddress} for game {gameId}...");
            
            try
            {
                var parts = _serverAddress.Split(':');
                var host = parts[0];
                var port = parts.Length > 1 ? int.Parse(parts[1]) : 50051;

                _rpcClient = new TcpRpcClient(host, port);
                _sessionService = _rpcClient.GetMain<IGameSessionService>();

                using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                _userSession = await _sessionService.Login(gameId, playerSignature, cts.Token);
                return _userSession != null;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine($"[AmpClient] Connect Error: {ex.Message}");
                return false;
            }
        }

        public async Task<string> RequestMatchAsync(string gameId)
        {
            if (_userSession == null) throw new InvalidOperationException("Not logged in.");

            Console.WriteLine($"[AmpClient] Requesting match for game: {gameId}...");
            
            var req = new GameMatchRequest
            {
                GameId = System.Text.Encoding.UTF8.GetBytes(gameId),
                RulesType = "standard",
                PlayerInfo = new PlayerInfo
                {
                    PlayerId = System.Text.Encoding.UTF8.GetBytes("p1"),
                    DisplayName = "CSharpPlayer",
                    Elo = Elo.unranked,
                    Region = Region.na,
                    // Empty wallet
                    PlayerWallet = Array.Empty<byte>()
                },
                Stake = new PaymentInfo
                {
                    PayerWallet = Array.Empty<byte>(),
                    FeeToken = Array.Empty<byte>(),
                    AuthSpend = 0
                },
                OptionalConfig = Array.Empty<byte>()
            };

            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(1)); // matches might take a while
            var (assignment, matchSession) = await _userSession.RequestMatch(req, cts.Token);
            
            _matchSession = matchSession;
            return System.Text.Encoding.UTF8.GetString(System.Linq.Enumerable.ToArray(assignment.MatchId));
        }

        public async Task EmitTelemetryAsync(byte eventTypeEnum, ulong timestamp)
        {
            if (!_hasMatchSession) return;
            
            Console.WriteLine($"[AmpClient] EmitTelemetry (Secured via MatchSession): type={eventTypeEnum}, time={timestamp}");
            
            var ev = new AmpTelemetryEvent
            {
                MatchId = Array.Empty<byte>(),
                EventType = (TelemetryEventType)eventTypeEnum,
                Timestamp = timestamp,
                VerifierId = Array.Empty<byte>(),
                EventData = Array.Empty<byte>()
            };
            
            await _matchSession!.EmitTelemetry(ev);
        }

        public async Task EmitGameEventAsync(string eventType)
        {
            if (!_hasMatchSession) return;
            Console.WriteLine($"[AmpClient] EmitGameEvent (Secured via MatchSession): {eventType}");
            
            var ev = new GameEvent
            {
                EventId = 0,
                Timestamp = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() * 1000UL,
                TriggeredBy = System.Text.Encoding.UTF8.GetBytes("p1"),
                EventType = "move",
                EventData = System.Text.Encoding.UTF8.GetBytes(eventType)
            };
            
            await _matchSession!.EmitGameEvent(ev);
        }

        public async Task<byte[]> SubmitOutcomeAsync(string matchId, byte outcome)
        {
            if (!_hasMatchSession) throw new InvalidOperationException("No match session.");
            
            Console.WriteLine($"[AmpClient] SubmitOutcome: match={matchId}, outcome={outcome}");
            
            var submission = new OutcomeSubmission
            {
                MatchId = System.Text.Encoding.UTF8.GetBytes(matchId),
                Outcome = new Outcome
                {
                    Type = outcome == 0 ? OutcomeType.win : OutcomeType.unknown,
                    Scores = new ulong[] { 1, 0 },
                    Victor = outcome,
                    Metadata = Array.Empty<byte>()
                },
                ReplayHash = Array.Empty<byte>(),
                Signature = Array.Empty<byte>()
            };

            var sig = await _matchSession!.SubmitOutcome(submission);
            return System.Linq.Enumerable.ToArray(sig);
        }

        public void Dispose()
        {
            _matchSession?.Dispose();
            _userSession?.Dispose();
            _sessionService?.Dispose();
            _rpcClient?.Dispose();
        }
    }
}
