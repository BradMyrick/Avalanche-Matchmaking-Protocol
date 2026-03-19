using System;
using System.Threading.Tasks;

namespace AmpSdkExample
{
    class Program
    {
        static async Task RunOpponent(string serverUrl, string gameIdStr)
        {
            await Task.Delay(500); // Wait half a second for player A to connect
            try
            {
                using var client = new AmpClient(serverUrl);
                var dummySig = new byte[] { 0x05, 0x06, 0x07, 0x08 };
                ulong gameId = 0; // Use game 0 as registered in e2e_verify.sh
                if (await client.ConnectAsync(gameId, dummySig))
                {
                    Console.WriteLine("[Player B] Connected to Matchmaker. Requesting match...");
                    await client.RequestMatchAsync(gameIdStr);
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
            Console.WriteLine($"Starting AMP C# .NET SDK Test on {serverUrl}...");

            try
            {
                using var client = new AmpClient(serverUrl);

                // 1. Connect and login via signature
                var pseudoSignature = new byte[] { 0x01, 0x02, 0x03 };
                ulong gameId = 0;
                bool connected = await client.ConnectAsync(gameId, pseudoSignature);
                if (!connected)
                {
                    Console.WriteLine("Failed to connect.");
                    return;
                }
                Console.WriteLine("[Player A] Connected & Logged in to AMP matchmaker.");

                string gameIdStr = "0x6767676767676767";

                Console.WriteLine("Spawning Player B thread...");
                _ = Task.Run(() => RunOpponent(serverUrl, gameIdStr));

                // 2. Request a match
                string matchId = await client.RequestMatchAsync(gameIdStr);
                Console.WriteLine($"Got MatchAssignment! Match ID: {matchId}");

                // 3. Simulate gameplay
                Console.WriteLine("Simulating game...");
                await client.EmitTelemetryAsync(1, 100);
                await client.EmitGameEventAsync("MatchStarted");

                await Task.Delay(500);

                await client.EmitGameEventAsync("MatchEnded");
                await client.EmitTelemetryAsync(2, 200);

                // 4. Submit outcome
                Console.WriteLine("Submitting outcome to verifier...");
                byte winnerOutcome = 0; // Player0 wins
                var signature = await client.SubmitOutcomeAsync(matchId, winnerOutcome);

                Console.WriteLine($"Verifier provided signature: 0x{BitConverter.ToString(signature).Replace("-", "").ToLower()}");
                Console.WriteLine("C# .NET SDK test successful. Exiting cleanly.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AMP Connect Error: {ex.Message}");
                Console.WriteLine("C# SDK Test Failed!"); 
                Environment.Exit(1);
            }
        }
    }
}
