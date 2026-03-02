using System;
using System.Threading.Tasks;

namespace AmpSdkExample
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var serverUrl = Environment.GetEnvironmentVariable("AMP_ADDR") ?? "127.0.0.1:50051";
            Console.WriteLine($"Starting AMP C# .NET SDK Test on {serverUrl}...");

            try
            {
                var client = new AmpClient(serverUrl);

                // 1. Connect and login via signature
                var pseudoSignature = new byte[] { 0x01, 0x02, 0x03 };
                bool connected = await client.ConnectAsync(pseudoSignature);
                if (!connected)
                {
                    Console.WriteLine("Failed to connect.");
                    return;
                }
                Console.WriteLine("[Player] Connected & Logged in to AMP matchmaker.");

                // 2. Request a match
                string gameIdStr = "0x6767676767676767";
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
                Console.WriteLine("C++ SDK Test Failed!"); // (Keep format for test script matching)
                Environment.Exit(1);
            }
        }
    }
}
