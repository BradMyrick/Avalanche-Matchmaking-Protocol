"use client";

import { useState } from "react";
import { Terminal, Gamepad2, Layers, Boxes, Coins } from "lucide-react";

type EngineData = {
  id: string;
  name: string;
  badge: string;
  description: string;
  icon: React.ElementType;
  iconColor: string;
  filename: string;
  code: string;
};

const engines: EngineData[] = [
  {
    id: "unity",
    name: "Unity Package",
    badge: "com.amp.sdk",
    description: "Drop-in Unity Package for 3-line in-game prize claims, bracket HUDs, and Avalanche wallet connections.",
    icon: Gamepad2,
    iconColor: "text-brand-cyan",
    filename: "TournamentClaim.cs",
    code: `using Amp.Sdk;
using Amp.Unity;
using UnityEngine;

public class TournamentReward : MonoBehaviour
{
    async void ClaimCupPrize(ulong tournamentId, string playerWallet)
    {
        var amp = AmpTournamentManager.Instance;

        // 3-line in-game prize pull-claim on Avalanche
        var claim = await amp.CanClaimAsync(tournamentId, playerWallet);
        if (claim.CanClaim)
        {
            var tx = await amp.ClaimPrizeAsync(tournamentId, claim.Placement);
            Debug.Log($"Prize claimed on Avalanche Fuji! Tx: {tx.Hash}");
        }
    }
}`,
  },
  {
    id: "unreal",
    name: "Unreal Engine 5",
    badge: "AmpUnreal Plugin",
    description: "C++ & Blueprint plugin for UE5. Zero external dependencies, direct async RPC calls to Fuji.",
    icon: Layers,
    iconColor: "text-brand-red",
    filename: "AmpRewardManager.cpp",
    code: `#include "AmpTournamentClient.h"

void AAmpRewardManager::ClaimTournamentPrize(int32 TournamentId, int32 Placement)
{
    UAmpTournamentClient* AmpClient = NewObject<UAmpTournamentClient>();

    // Direct non-custodial pull-claim to player's Core or MetaMask wallet
    AmpClient->ClaimPrize(TournamentId, Placement,
        FOnPrizeClaimed::CreateLambda([](bool bSuccess, FString TxHash) {
            if (bSuccess) {
                UE_LOG(LogAmp, Log, TEXT("Prize claimed! Avalanche Tx: %s"), *TxHash);
            }
        })
    );
}`,
  },
  {
    id: "typescript",
    name: "TypeScript SDK",
    badge: "@amp/sdk",
    description: "Full bracket engine (Single Elimination, Swiss, Round Robin) + contract client for Node.js and browser.",
    icon: Boxes,
    iconColor: "text-yellow-400",
    filename: "tournament.ts",
    code: `import { AMPTournamentClient, Tournament, TournamentState } from "@amp/sdk";

// 1. Run local bracket engine (zero network calls)
const cup = Tournament.new({ kind: TournamentState.SingleElimination }, [
  { id: 1, seed: 1 },
  { id: 2, seed: 2 },
]);

// 2. Query & claim escrowed prize pool on Avalanche Fuji
const client = new AMPTournamentClient({ signer });
const info = await client.canClaim(tournamentId, playerAddress);
if (info.canClaim) {
  const tx = await client.claimPrize(tournamentId, info.placement);
  console.log("Prize claimed on Avalanche! Hash:", tx.hash);
}`,
  },
  {
    id: "http",
    name: "HTTP REST API",
    badge: "Plain JSON",
    description: "Zero SDK installation required. Call from Godot, Python, Rust, Go, curl, or any backend service.",
    icon: Terminal,
    iconColor: "text-zinc-300",
    filename: "api.sh",
    code: `# Report a match result from any server or client
curl -X POST https://playwithamp.xyz/api/tournament/42/report \\
  -H "Content-Type: application/json" \\
  -d '{"wallet":"0xabc...","matchId":7,"outcome":"A"}'

# Read the live bracket + standings
curl https://playwithamp.xyz/api/tournament/42`,
  },
  {
    id: "onchain",
    name: "Smart Contract",
    badge: "AMPTournamentCup.sol",
    description: "Audited Solidity escrow contract verified on Avalanche Fuji (0x7C743c1C9AE3e7A65d030098F2249B7787d66dfF).",
    icon: Coins,
    iconColor: "text-yellow-400",
    filename: "IAMPTournamentCup.sol",
    code: `// SPDX-License-Identifier: MIT
// Verified on Avalanche Fuji: 0x7C743c1C9AE3e7A65d030098F2249B7787d66dfF
interface IAMPTournamentCup {
    function createTournament(
        uint16[] calldata payoutBps,
        address verifier,
        uint64 finalizeDeadline
    ) external payable returns (uint256);

    function finalizeTournament(
        uint256 tournamentId,
        address[] calldata winners,
        bytes calldata signature
    ) external;

    function claimPrize(uint256 tournamentId, uint256 placement) external;
}`,
  },
];

export default function EngineTabs() {
  const [activeTab, setActiveTab] = useState(engines[0]);

  return (
    <div className="grid grid-cols-1 lg:grid-cols-2 gap-16 items-center relative z-10">
      <div>
        <div className="inline-flex items-center gap-2 px-3.5 py-1.5 rounded-full bg-brand-cyan/10 border border-brand-cyan/30 mb-4 shadow-[0_0_15px_rgba(0,229,255,0.2)]">
          <Boxes className="w-3.5 h-3.5 text-brand-cyan" />
          <span className="text-xs font-bold text-brand-cyan tracking-widest uppercase">
            Developer Toolkit & SDKs
          </span>
        </div>
        <h2 className="text-4xl md:text-5xl font-black uppercase tracking-tight mb-6 text-white drop-shadow-[0_0_15px_rgba(0,229,255,0.3)]">
          Drop-in SDKs.<br /><span className="text-brand-cyan">Any Game Engine.</span>
        </h2>
        <p className="text-zinc-400 text-lg mb-8 font-medium leading-relaxed">
          Integrate verifiable Avalanche tournament cups into your game in minutes. Whether you build in{" "}
          <strong className="text-white">Unity</strong>,{" "}
          <strong className="text-white">Unreal Engine 5</strong>,{" "}
          <strong className="text-white">TypeScript/Node</strong>, or custom engines via plain HTTPS,
          AMP provides first-class native libraries for bracket generation and trustless pull-claims.
        </p>

        <div className="space-y-3">
          {engines.map((engine) => {
            const isActive = activeTab.id === engine.id;
            return (
              <button
                key={engine.id}
                onClick={() => setActiveTab(engine)}
                className={
                  "w-full text-left flex gap-4 items-start p-3.5 rounded-xl border transition-all cursor-pointer group hover:shadow-[0_0_20px_rgba(0,229,255,0.1)] hover:-translate-y-0.5 " +
                  (isActive ? "bg-black/80 border-brand-cyan/60 shadow-[0_0_20px_rgba(0,229,255,0.15)]" : "bg-black/40 border-white/5 hover:border-brand-cyan/30")
                }
              >
                <div className={
                  "p-2.5 rounded-lg transition-colors border " +
                  (isActive ? "bg-brand-cyan/20 text-brand-cyan border-brand-cyan/30" : "bg-white/5 text-zinc-400 border-transparent group-hover:bg-brand-cyan/10 group-hover:border-brand-cyan/20")
                }>
                  <engine.icon className={"w-5 h-5 " + (isActive ? "text-brand-cyan" : engine.iconColor)} />
                </div>
                <div className="flex-1">
                  <div className="flex items-center justify-between mb-0.5">
                    <h4 className={"font-bold uppercase tracking-wider text-sm " + (isActive ? "text-white" : "text-zinc-300 group-hover:text-white")}>
                      {engine.name}
                    </h4>
                    <span className="text-[10px] font-mono px-2 py-0.5 rounded bg-white/5 text-zinc-400 border border-white/5">
                      {engine.badge}
                    </span>
                  </div>
                  <p className={"text-xs leading-relaxed " + (isActive ? "text-zinc-300" : "text-zinc-500 group-hover:text-zinc-400")}>
                    {engine.description}
                  </p>
                </div>
              </button>
            );
          })}
        </div>
      </div>

      <div className="glass-panel p-2 bg-gradient-to-br from-brand-cyan/20 to-transparent relative shadow-[0_0_50px_rgba(0,229,255,0.15)] h-full min-h-[440px]">
        <div className="absolute -inset-0.5 bg-gradient-to-r from-brand-cyan/50 to-transparent opacity-30 blur pointer-events-none" />
        <div className="bg-black/95 rounded-2xl p-6 overflow-hidden h-full flex flex-col border border-white/10 relative z-10 transition-all duration-300 shadow-inner">
          <div className="flex justify-between items-center mb-4 border-b border-white/10 pb-4">
            <div className="flex gap-2">
              <div className="w-3 h-3 rounded-full bg-brand-red/80 shadow-[0_0_5px_rgba(232,65,66,0.5)]" />
              <div className="w-3 h-3 rounded-full bg-yellow-500/80 shadow-[0_0_5px_rgba(234,179,8,0.5)]" />
              <div className="w-3 h-3 rounded-full bg-brand-cyan/80 shadow-[0_0_5px_rgba(0,229,255,0.5)]" />
            </div>
            <span className="text-[11px] text-zinc-400 font-mono font-bold tracking-wider">{activeTab.filename}</span>
          </div>

          <div className="text-zinc-300 font-mono text-xs sm:text-sm leading-relaxed overflow-x-auto custom-scrollbar flex-1 relative">
            <div className="absolute top-0 right-0 w-8 h-full bg-gradient-to-l from-black/90 to-transparent pointer-events-none" />
            <div className="absolute bottom-0 left-0 w-full h-8 bg-gradient-to-t from-black/90 to-transparent pointer-events-none" />
            <code className="text-brand-cyan/70 block mb-2 opacity-60 select-none">{"// "}{activeTab.name} · {activeTab.badge}</code>
            <pre className="whitespace-pre">{activeTab.code}</pre>
          </div>
        </div>
      </div>
    </div>
  );
}
