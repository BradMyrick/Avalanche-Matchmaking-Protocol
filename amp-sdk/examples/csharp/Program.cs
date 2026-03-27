using System;
using System.Threading.Tasks;

namespace AmpSdkExample
{
    class Program
    {
        private static byte[] HexToBytes(string hex) {
            byte[] bytes = new byte[hex.Length / 2];
            for (int i = 0; i < bytes.Length; i++) {
                bytes[i] = Convert.ToByte(hex.Substring(i * 2, 2), 16);
            }
            return bytes;
        }

        static async Task RunOpponent(string serverUrl, string gameIdStr)
        {
            await Task.Delay(500); // Wait half a second for player A to connect
            try
            {
                using var client = new AmpClient(serverUrl);
                var dummySig = HexToBytes("24e2943427fa35e48c01ba764c271a9e76d295142704648b171e8cb5272a279773a0206586e565b1fed8a916e945a39868463654f752bf6bcd6843ab06bff39e1c");
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
                var pseudoSignature = HexToBytes("24e2943427fa35e48c01ba764c271a9e76d295142704648b171e8cb5272a279773a0206586e565b1fed8a916e945a39868463654f752bf6bcd6843ab06bff39e1c");
                ulong gameId = 0;
                bool connected = await client.ConnectAsync(gameId, pseudoSignature);
                if (!connected)
                {
                    Console.WriteLine("Failed to connect.");
                    Environment.Exit(1);
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
