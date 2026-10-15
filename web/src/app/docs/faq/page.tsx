import Link from "next/link";
import { HelpCircle, ChevronDown } from "lucide-react";

export const metadata = {
  title: "AMP FAQ — Tournament Protocol Questions",
  description: "Answers about the AMP Tournament Protocol: prize escrows, bracket formats, pull-claims, fees, and engine support.",
};

const faqs = [
  {
    q: "What is AMP?",
    a: "AMP is an open, headless Tournament & Match Protocol built for games on Avalanche. It provides non-custodial sponsor prize pools, verifiable tournament brackets (Single Elimination, Swiss, Round Robin), and drop-in game engine SDKs for in-game prize claims.",
  },
  {
    q: "Do I need to host a backend server to run tournaments?",
    a: "No! AMP is 100% serverless. The client SDKs (Unity, Unreal, TypeScript, C#, Rust) talk directly to Avalanche RPC nodes and smart contracts. There are zero databases or cloud servers to babysit.",
  },
  {
    q: "How do sponsor prize pools work?",
    a: "Sponsors deposit AVAX or ERC-20 tokens (e.g., USDC) into the AMPTournamentCup.sol contract on Avalanche. The sponsor specifies payout percentages via basis points (e.g., 60% 1st, 30% 2nd, 10% 3rd) and designates a trusted verifier or referee.",
  },
  {
    q: "What happens if a tournament is abandoned or never finalized?",
    a: "AMPTournamentCup features built-in sponsor refund protection. When creating a cup, a finalizeDeadline is set. If the verifier fails to finalize the tournament before the deadline, the sponsor can call cancelTournament() and retrieve 100% of their deposited funds.",
  },
  {
    q: "How do winners receive their prize money?",
    a: "AMP uses a pull-payment claim architecture. Once a tournament is finalized with an EIP-712 verifier attestation, winners call claimPrize(tournamentId, placement) directly from the game client to pull their winnings straight into their Core or MetaMask wallet.",
  },
  {
    q: "Which game engines have official SDK support?",
    a: "AMP provides first-class support for: (1) Unity via com.amp.sdk, (2) Unreal Engine 5 via the AmpUnreal C++ & Blueprint plugin, (3) Web & Node.js via @amp/sdk, (4) C# / Godot via Amp.Sdk, and (5) Rust via amp-sdk.",
  },
  {
    q: "What are the protocol fees?",
    a: "A 2.5% protocol fee is deducted only when prizes are claimed by winners. 97.5% of the escrowed prize pool is delivered directly to the competitors. There are no monthly fees, subscription tiers, or hosting overhead.",
  },
  {
    q: "Can I deploy AMP to my own Avalanche Subnet / L1?",
    a: "Yes! AMPTournamentCup.sol is standard Solidity compatible with any EVM-based Avalanche L1. You can configure native gas tokens or custom Subnet ERC-20 tokens.",
  },
];

export default function FaqPage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <div className="relative z-10 max-w-3xl mx-auto px-6 py-16">
        <div className="mb-12 text-center">
          <Link href="/docs" className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors">
            ← Docs
          </Link>
          <div className="inline-flex items-center gap-2 rounded-full border border-brand-cyan/30 bg-brand-cyan/5 px-4 py-1.5 text-xs font-bold uppercase tracking-widest text-brand-cyan mt-6 mb-4">
            <HelpCircle className="w-3.5 h-3.5" />
            FAQ & Architecture
          </div>
          <h1 className="text-4xl font-black uppercase tracking-tight mb-4">
            Frequently Asked <span className="text-brand-cyan">Questions</span>
          </h1>
          <p className="text-sm text-zinc-400">Everything you need to know about the Avalanche Tournament Protocol.</p>
        </div>

        <div className="space-y-3">
          {faqs.map((faq, i) => (
            <details
              key={i}
              className="glass-panel rounded-2xl border border-white/10 overflow-hidden group"
            >
              <summary className="flex items-center justify-between cursor-pointer px-6 py-4 hover:bg-white/5 transition-colors list-none">
                <span className="font-bold text-sm md:text-base">{faq.q}</span>
                <ChevronDown className="w-4 h-4 text-zinc-500 shrink-0 ml-4 group-open:rotate-180 transition-transform" />
              </summary>
              <div className="px-6 pb-5 pt-0">
                <p className="text-sm text-zinc-400 leading-relaxed">{faq.a}</p>
              </div>
            </details>
          ))}
        </div>

        <div className="mt-12 text-center space-y-2">
          <p className="text-sm text-zinc-500">Ready to integrate?</p>
          <Link href="/docs/quickstart" className="text-sm text-brand-cyan hover:underline font-bold">
            Read the Quickstart Guide →
          </Link>
        </div>
      </div>
    </div>
  );
}
