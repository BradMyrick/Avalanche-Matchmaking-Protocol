"use client";

import { useState } from "react";
import { Code2, Coins, Boxes, Terminal } from "lucide-react";

type EngineData = {
  id: string;
  name: string;
  description: string;
  icon: React.ElementType;
  iconColor: string;
  filename: string;
  code: string;
};

const engines: EngineData[] = [
  {
    id: "http",
    name: "HTTP API · any language",
    description: "Plain JSON over HTTPS. No SDK to install — call it from Unity, Godot, Python, curl, anything.",
    icon: Terminal,
    iconColor: "text-brand-cyan",
    filename: "api.sh",
    code: `# Report a match result from any client
curl -X POST https://playwithamp.xyz/api/tournament/42/report \\
  -H "Content-Type: application/json" \\
  -d '{"wallet":"0xabc...","matchId":7,"outcome":"A"}'

# Read the bracket + standings
curl https://playwithamp.xyz/api/tournament/42`,
  },
  {
    id: "onchain",
    name: "On-chain · ethers / viem",
    description: "Talk to the escrow contract directly from a game wallet. Sponsor a pool, claim a prize.",
    icon: Coins,
    iconColor: "text-brand-cyan",
    filename: "claim.ts",
    code: `import { CUP_ADDRESS, AMPCUP_ABI, connectWallet } from "amp";

const provider = await connectWallet();        // Core / MetaMask
const signer  = await provider.getSigner();
const cup     = new ethers.Contract(CUP_ADDRESS, AMPCUP_ABI, signer);

// A winner pulls their escrowed prize — pull-payment, no middleman
await cup.claimPrize(42, 0, { gasLimit: 200_000 });`,
  },
  {
    id: "engine",
    name: "Embed the bracket engine",
    description: "Pure TypeScript, zero deps. Drop the bracket logic straight into your server or client.",
    icon: Boxes,
    iconColor: "text-brand-cyan",
    filename: "bracket.ts",
    code: `import { Tournament, Outcome } from "amp/engine";

// Single-elim, round-robin, or Swiss — same surface
const cup = Tournament.new(
  { kind: "single_elimination" },
  players.map((p, i) => ({ id: i, seed: p.seed })),
);

// Record a result; the engine advances the bracket
cup.record(cup.pending()[0], Outcome.A);

const ordered = cup.winners();   // -> [champion, runner-up, ...]`,
  },
];

export default function EngineTabs() {
  const [activeTab, setActiveTab] = useState(engines[0]);

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center relative z-10">
      <div>
        <h2 className="text-5xl font-black uppercase tracking-tight mb-6 text-white drop-shadow-[0_0_15px_rgba(0,229,255,0.3)]">
          One API.<br /><span className="text-brand-cyan">Any Stack.</span>
        </h2>
        <p className="text-zinc-400 text-lg mb-8 font-medium">
          AMP is an <strong className="text-white">HTTPS JSON API</strong> and an{" "}
          <strong className="text-white">on-chain contract</strong>. No SDK to install, no binary protocol —
          call it from Unity, Godot, Python, or a shell. The bracket engine is pure TypeScript you can embed,
          and the custody path is an isolated Rust service. Integrate in minutes, in whatever you already use.
        </p>

        <div className="space-y-4">
          {engines.map((engine) => {
            const isActive = activeTab.id === engine.id;
            return (
              <button
                key={engine.id}
                onClick={() => setActiveTab(engine)}
                className={
                  "w-full text-left flex gap-4 items-start p-4 rounded-sm border transition-all cursor-pointer group hover:shadow-[0_0_20px_rgba(0,229,255,0.1)] hover:-translate-y-0.5 " +
                  (isActive ? "bg-black/60 border-brand-cyan/60 shadow-[0_0_20px_rgba(0,229,255,0.1)]" : "bg-black/40 border-white/5 hover:border-brand-cyan/30")
                }
              >
                <div className={
                  "p-3 rounded-sm transition-colors border " +
                  (isActive ? "bg-brand-cyan/20 text-brand-cyan border-brand-cyan/30" : "bg-white/5 text-zinc-400 border-transparent group-hover:bg-brand-cyan/10 group-hover:border-brand-cyan/20")
                }>
                  <engine.icon className={"w-6 h-6 " + (isActive ? "text-brand-cyan" : engine.iconColor)} />
                </div>
                <div>
                  <h4 className={"font-bold uppercase tracking-wider text-sm mb-1 " + (isActive ? "text-white" : "text-zinc-300 group-hover:text-white")}>
                    {engine.name}
                  </h4>
                  <p className={"text-sm " + (isActive ? "text-zinc-300" : "text-zinc-500 group-hover:text-zinc-400")}>
                    {engine.description}
                  </p>
                </div>
              </button>
            );
          })}
        </div>
      </div>

      <div className="glass-panel p-2 bg-gradient-to-br from-brand-cyan/20 to-transparent relative shadow-[0_0_50px_rgba(0,229,255,0.15)] h-full min-h-[400px]">
        <div className="absolute -inset-0.5 bg-gradient-to-r from-brand-cyan/50 to-transparent opacity-30 blur pointer-events-none" />
        <div className="bg-black/90 rounded-[0.75rem] p-6 overflow-hidden h-full flex flex-col border border-white/10 relative z-10 transition-all duration-300 shadow-inner">
          <div className="flex justify-between items-center mb-4 border-b border-white/10 pb-4">
            <div className="flex gap-2">
              <div className="w-3 h-3 rounded-full bg-brand-red/80 shadow-[0_0_5px_rgba(232,65,66,0.5)]" />
              <div className="w-3 h-3 rounded-full bg-yellow-500/80 shadow-[0_0_5px_rgba(234,179,8,0.5)]" />
              <div className="w-3 h-3 rounded-full bg-brand-cyan/80 shadow-[0_0_5px_rgba(0,229,255,0.5)]" />
            </div>
            <span className="text-[11px] text-zinc-500 font-bold tracking-wider">{activeTab.filename}</span>
          </div>

          <div className="text-zinc-300 font-mono text-sm leading-relaxed overflow-x-auto custom-scrollbar flex-1 relative">
            <div className="absolute top-0 right-0 w-8 h-full bg-gradient-to-l from-black/90 to-transparent pointer-events-none" />
            <div className="absolute bottom-0 left-0 w-full h-8 bg-gradient-to-t from-black/90 to-transparent pointer-events-none" />
            <code className="text-brand-cyan/70 block mb-2 opacity-50 select-none">{"// "}{activeTab.name}</code>
            <pre className="whitespace-pre">{activeTab.code}</pre>
          </div>
        </div>
      </div>
    </div>
  );
}
