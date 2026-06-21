using System;
using System.Security.Cryptography;
using System.Threading.Tasks;
using AmpSdk;

namespace AmpSdkExample
{
    class Program
    {
        // Well-known anvil/hardhat test keys (NEVER use in production).
        private const string PlayerAKey = "ac0974bec39a17e36ba4a6b4d238ff944bacb478cbed5efcae784d7bf4f2ff80";
        private const string PlayerBKey = "59c6995e998f97a5a0044966f0945389dc9e86dae88c7a8412f4603b6b78690d";

        static async Task RunOpponent(string serverUrl, string gameIdStr, ulong gameId)
        {
            await Task.Delay(500); // Wait half a second for player A to connect
            try
            {
                using var client = new AmpClient(serverUrl);
                if (await client.ConnectAsync())
                {
                    // S2 fix: pass an explicit private key — the SDK no longer
                    // silently generates an ephemeral wallet.
                    if (await client.AuthenticateAsync(gameId, PlayerBKey))
                    {
                        Console.WriteLine("[Player B] Connected to Matchmaker. Requesting match...");
                        await client.RequestMatchAsync(new MatchRequest { GameId = gameIdStr, RulesetId = "standard", PlayerId = "p2" });
                    }
                }
            }
            catch (Exception)
            {
                // Ignore background errors
            }
        }

        static async Task Main(string[] args)
        {
            var serverUrl = Environment.GetEnvironmentVariable("AMP_ADDR") ?? "127.0.0.1:50051";
            Console.WriteLine($"Starting AMP C# .NET SDK example on {serverUrl}...");

            try
            {
                using var client = new AmpClient(serverUrl);

                ulong gameId = 0;
                bool connected = await client.ConnectAsync();
                if (!connected)
                {
                    Console.WriteLine("Failed to connect.");
                    Environment.Exit(1);
                }

                // Authenticate with an explicit private key.
                bool loggedIn = await client.AuthenticateAsync(gameId, PlayerAKey);
                if (!loggedIn)
                {
                    Console.WriteLine("Failed to log in.");
                    Environment.Exit(1);
                }
                Console.WriteLine("[Player A] Connected & authenticated to the AMP matchmaker.");

                string gameIdStr = "0";

                Console.WriteLine("Spawning Player B...");
                _ = Task.Run(() => RunOpponent(serverUrl, gameIdStr, gameId));

                // Request a match
                var matchResult = await client.RequestMatchAsync(new MatchRequest { GameId = gameIdStr, RulesetId = "standard", PlayerId = "p1" });
                string matchId = matchResult.MatchId;
                Console.WriteLine($"Matched! Match ID: {matchId}");

                // Simulate gameplay
                Console.WriteLine("Simulating game...");
                await client.EmitTelemetryAsync(1, 100);
                await client.EmitGameEventAsync("MatchStarted");

                await Task.Delay(500);

                await client.EmitGameEventAsync("MatchEnded");
                await client.EmitTelemetryAsync(2, 200);

                // Submit outcome. Victor must be 1..=4 (1 = Player A wins) and
                // transcript hash must be exactly 32 bytes (S1 + S5 fixes).
                Console.WriteLine("Submitting outcome (Player A wins) to verifier...");
                byte winnerOutcome = 1;
                byte[] transcriptHash = SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(matchId));
                var signatureResult = await client.SubmitOutcomeAsync(matchId, winnerOutcome, transcriptHash);
                var signature = signatureResult.Signature;

                Console.WriteLine($"Verifier countersignature: 0x{BitConverter.ToString(signature).Replace("-", "").ToLower()}");
                Console.WriteLine("C# .NET SDK example complete.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AMP example error: {ex.Message}");
                Environment.Exit(1);
            }
        }
    }
}

