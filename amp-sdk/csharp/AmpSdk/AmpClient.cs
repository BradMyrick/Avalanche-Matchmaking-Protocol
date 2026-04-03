using System;
using System.Threading.Tasks;

namespace AmpSdk {

public class AmpClient {
    private readonly string _rpcUrl;

    public AmpClient(string rpcUrl) {
        _rpcUrl = rpcUrl;
    }

    public async Task<bool> ConnectAsync(string playerId, ulong gameId) {
        // var client = new TcpClient();
        // await client.ConnectAsync(_rpcUrl.Split(':')[0], int.Parse(_rpcUrl.Split(':')[1]));
        // var stream = client.GetStream();
        // _connection = new RpcConnection(stream);
        // _gameSession = _connection.Bootstrap<GameSessionService>();
        // var req = _gameSession.LoginRequest();
        // req.GameId = gameId;
        // req.SignedChallenge = System.Text.Encoding.UTF8.GetBytes(playerId);
        // var res = await req.SendAsync();
        // _userSession = res.Session;
        return await Task.FromResult(true);
    }

    public async Task<string> CreateProfileAsync(PlayerProfileData profile) {
        // var req = _userSession.CreateOrUpdateProfileRequest();
        // req.DisplayName = profile.DisplayName;
        // req.Region = profile.Region;
        // await req.SendAsync();
        return await Task.FromResult(profile.PlayerId);
    }

    public async Task<MatchResult> RequestMatchAsync(MatchRequest request) {
        // var req = _userSession.RequestMatchRequest();
        // req.Req.RuleSetId = System.Text.Encoding.UTF8.GetBytes(request.RulesetId);
        // var res = await req.SendAsync();
        // _matchSession = res.Session;
        // return new MatchResult { MatchId = res.Assignment.MatchId, Quality = res.Assignment.MatchQuality };
        return await Task.FromResult(new MatchResult { MatchId = "tmp", Quality = 1.0f });
    }

    public async Task SubmitOutcomeAsync(string matchId, byte outcome, byte[] transcriptHash) {
        // var req = _matchSession.SubmitOutcomeRequest();
        // req.Submission.MatchId = System.Text.Encoding.UTF8.GetBytes(matchId);
        // req.Submission.Outcome.Victor = outcome;
        // req.Submission.ReplayHash = transcriptHash;
        // await req.SendAsync();
        await Task.CompletedTask;
    }
}

}
