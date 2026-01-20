import { useState } from 'react';

// Mock types

function App() {
  const [status, setStatus] = useState<string>("Ready");
  const [lastEvent, setLastEvent] = useState<any>(null);
  const [isConnecting, setIsConnecting] = useState(false);

  // Constants
  const REGISTRY_ADDRESS = "0x897...0626";
  const SETTLEMENT_ADDRESS = "0xA43...4321";

  const connectWallet = async () => {
    setIsConnecting(true);
    setStatus("Connecting wallet...");
    await new Promise(r => setTimeout(r, 1000));
    setStatus("Connected (Mock)");
    setIsConnecting(false);
  };

  const registerDemoGame = async () => {
    setStatus("Registering game...");
    await new Promise(r => setTimeout(r, 1200));
    setStatus("Game Registered (ID: 0)");
  };

  const createMatch = async () => {
    setStatus("Creating match...");
    await new Promise(r => setTimeout(r, 1000));
    setStatus("Match Created (ID: 42)");
  };

  const joinMatch = async () => {
    setStatus("Joining match...");
    await new Promise(r => setTimeout(r, 1000));
    setStatus("Joined Match 42");
  };

  const playAndSettle = async () => {
    setStatus("Simulating duel & settlement...");
    await new Promise(r => setTimeout(r, 2000));
    const mockEvent = {
      matchId: 42,
      outcome: "WIN_A",
      payout: "0.198 AVAX",
      timestamp: new Date().toLocaleTimeString()
    };
    setLastEvent(mockEvent);
    setStatus("Match Settled!");
  };

  return (
    <div className="min-h-screen bg-black text-white selection:bg-avax-red/30">
      {/* Navigation */}
      <nav className="border-b border-white/10 px-6 py-4 flex justify-between items-center backdrop-blur-md sticky top-0 z-50">
        <div className="flex items-center gap-3">
          <div className="w-10 h-10 bg-avax-red rounded-lg flex items-center justify-center font-bold tracking-tighter text-xl">A</div>
          <h1 className="text-2xl font-black tracking-tight uppercase italic">AMP <span className="text-avax-red">MVP</span></h1>
        </div>
        <button
          onClick={connectWallet}
          className={`px-6 py-2 rounded-full font-bold transition-all ${status.includes("Connected")
            ? "bg-white/10 text-white border border-white/20"
            : "bg-avax-red hover:bg-avax-red/80 text-white shadow-lg shadow-avax-red/20"
            }`}
        >
          {isConnecting ? "Connecting..." : status.includes("Connected") ? "0x1234...5678" : "Connect Wallet"}
        </button>
      </nav>

      <main className="max-w-4xl mx-auto px-6 py-12">
        {/* Header Section */}
        <div className="mb-12">
          <h2 className="text-4xl font-extrabold mb-4">Avalanche Match Protocol</h2>
          <p className="text-white/60 text-lg max-w-2xl leading-relaxed">
            Scalable, verifiable on-chain game settlement. Powered by ZK-friendly transcripts and off-chain computation.
          </p>
        </div>

        {/* Status Dashboard */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-12">
          <div className="bg-avax-gray p-6 rounded-2xl border border-white/5">
            <p className="text-white/40 text-sm uppercase font-bold tracking-widest mb-1">Status</p>
            <p className={`text-xl font-mono ${status === "Match Settled!" ? "text-green-400" : "text-white"}`}>{status}</p>
          </div>
          <div className="bg-avax-gray p-6 rounded-2xl border border-white/5">
            <p className="text-white/40 text-sm uppercase font-bold tracking-widest mb-1">Network</p>
            <p className="text-xl font-bold">Fuji Testnet</p>
          </div>
          <div className="bg-avax-gray p-6 rounded-2xl border border-white/5">
            <p className="text-white/40 text-sm uppercase font-bold tracking-widest mb-1">Game ID</p>
            <p className="text-xl font-bold">#0 (Chess Demo)</p>
          </div>
        </div>

        {/* Action Panel */}
        <div className="bg-avax-gray rounded-3xl border border-white/5 overflow-hidden shadow-2xl">
          <div className="p-8 border-b border-white/5 bg-gradient-to-r from-avax-red/10 to-transparent">
            <h3 className="text-xl font-bold">Developer Control Console</h3>
          </div>
          <div className="p-8 grid grid-cols-1 sm:grid-cols-2 gap-4">
            <button
              onClick={registerDemoGame}
              disabled={status === "Ready"}
              className="flex items-center justify-between p-4 bg-white/5 hover:bg-white/10 rounded-xl transition-all border border-white/5 disabled:opacity-50 disabled:cursor-not-allowed group"
            >
              <span className="font-bold">Register Game Type</span>
              <span className="opacity-0 group-hover:opacity-100 transition-opacity">→</span>
            </button>
            <button
              onClick={createMatch}
              disabled={status === "Ready"}
              className="flex items-center justify-between p-4 bg-white/5 hover:bg-white/10 rounded-xl transition-all border border-white/5 disabled:opacity-50 disabled:cursor-not-allowed group"
            >
              <span className="font-bold">Create New Match</span>
              <span className="opacity-0 group-hover:opacity-100 transition-opacity">→</span>
            </button>
            <button
              onClick={joinMatch}
              disabled={status === "Ready"}
              className="flex items-center justify-between p-4 bg-white/5 hover:bg-white/10 rounded-xl transition-all border border-white/5 disabled:opacity-50 disabled:cursor-not-allowed group"
            >
              <span className="font-bold">Join Existing Match</span>
              <span className="opacity-0 group-hover:opacity-100 transition-opacity">→</span>
            </button>
            <button
              onClick={playAndSettle}
              disabled={status === "Ready"}
              className="flex items-center justify-between p-4 bg-avax-red/20 hover:bg-avax-red/30 rounded-xl transition-all border border-avax-red/30 disabled:opacity-50 disabled:cursor-not-allowed group"
            >
              <span className="font-bold text-avax-red">Play & Settle Duel</span>
              <span className="opacity-0 group-hover:opacity-100 transition-opacity text-avax-red">→</span>
            </button>
          </div>
        </div>

        {/* Event Logs */}
        {lastEvent && (
          <div className="mt-12 animate-in fade-in slide-in-from-bottom-4 duration-500">
            <h3 className="text-xl font-bold mb-4 flex items-center gap-2">
              <span className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></span>
              Live Events
            </h3>
            <div className="bg-avax-gray p-8 rounded-2xl border border-white/5 font-mono text-sm overflow-x-auto">
              <div className="flex justify-between mb-4 border-b border-white/5 pb-2">
                <span className="text-white/40">MATCH_SETTLED</span>
                <span className="text-white/40">{lastEvent.timestamp}</span>
              </div>
              <pre className="text-green-400">
                {JSON.stringify(lastEvent, null, 2)}
              </pre>
            </div>
          </div>
        )}

        {/* Footer Meta */}
        <div className="mt-12 flex flex-col sm:flex-row gap-6 justify-between text-xs text-white/30 font-mono tracking-tighter">
          <div className="flex gap-6">
            <p>REGISTRY: {REGISTRY_ADDRESS}</p>
            <p>SETTLEMENT: {SETTLEMENT_ADDRESS}</p>
          </div>
          <p>© 2026 AMP PROTOCOL</p>
        </div>
      </main>
    </div>
  );
}

export default App;
