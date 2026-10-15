import Link from "next/link";
import { ChevronDown } from "lucide-react";
import SiteHeader from "@/app/components/SiteHeader";
import { PROTOCOL_FEE_LABEL } from "@/lib/ampCup";

export const metadata = {
  title: "AMP FAQ — Tournament Protocol Questions",
  description: "Answers about the AMP Tournament Protocol: prize escrows, bracket formats, pull-claims, fees, and engine support.",
};

const faqs = [
  {
    q: "What is AMP?",
    a: "The protocol is the SDK and AMPTournamentCup: local brackets, on-chain escrow, pull-claims.",
  },
  {
    q: "Do I need to host a server?",
    a: "Not for the SDK and contract. This site’s cup launcher is an optional hosted console.",
  },
  {
    q: "How do prize pools work?",
    a: "A sponsor deposits AVAX into AMPTournamentCup, sets the split, and names a verifier.",
  },
  {
    q: "What if a cup is never finalized?",
    a: "After finalizeDeadline the sponsor can call cancelTournament() and take the deposit back.",
  },
  {
    q: "How do winners get paid?",
    a: "After the verifier signs, winners call claimPrize from their wallet.",
  },
  {
    q: "Which SDKs exist?",
    a: "Unity, Unreal, TypeScript, C#, and Rust.",
  },
  {
    q: "What is the protocol fee?",
    a: `${PROTOCOL_FEE_LABEL} of each prize payout. That is protocolFeeBps on AMPTournamentCup.`,
  },
  {
    q: "Can I deploy the contract myself?",
    a: "Yes. AMPTournamentCup is Solidity and runs on any EVM chain, including an Avalanche L1.",
  },
];

export default function FaqPage() {
  return (
    <div className="min-h-screen bg-black text-white antialiased">
      <div className="absolute top-0 -left-1/4 w-[150%] h-[300px] bg-brand-cyan/10 blur-[120px] rounded-full pointer-events-none" />

      <SiteHeader />
      <div className="relative z-10 max-w-3xl mx-auto px-6 pt-24 pb-16">
        <div className="mb-10 text-center">
          <Link href="/docs" className="text-sm text-zinc-500 hover:text-brand-cyan transition-colors">
            ← Docs
          </Link>
          <h1 className="text-4xl font-black uppercase tracking-tight mt-4 mb-2">
            Questions
          </h1>
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
