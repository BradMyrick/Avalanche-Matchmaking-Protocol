"use client";

import { motion } from "framer-motion";
import { ShieldCheck, CheckCircle2, Boxes } from "lucide-react";

const stats = [
  { value: "16", label: "Contract tests", sublabel: "Forge · AMPTournamentCup", icon: CheckCircle2 },
  { value: "3", label: "EIP-712 impls", sublabel: "Solidity · ethers · Rust", icon: Boxes },
  { value: "100%", label: "Pull-payment", sublabel: "winners call claimPrize", icon: ShieldCheck },
];

const badges = [
  "EIP-712 Verifier Attestation",
  "Pull-Payment Prize Payouts",
  "ReentrancyGuard",
  "Pausable + Ownable2Step",
  "TimelockController Governance",
  "Fee-on-Transfer Safe",
  "Custodial Key Isolation",
  "OZ EIP712 Domain",
];

export default function SecurityAudits() {
  return (
    <section id="security" className="py-24 relative overflow-hidden scroll-mt-32">
      <div className="absolute inset-0 bg-gradient-to-b from-transparent via-brand-cyan/[0.03] to-transparent pointer-events-none" />
      <div className="max-w-7xl mx-auto px-6">
        <div className="text-center mb-16">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-green-500/10 border border-green-500/30 mb-4">
            <CheckCircle2 className="w-3 h-3 text-green-400" />
            <span className="text-xs font-bold text-green-400 tracking-widest uppercase">
              Live on Fuji · source-verified
            </span>
          </div>
          <h2 className="text-4xl md:text-5xl font-bold mb-4 text-white">
            Security Hardened
          </h2>
          <p className="text-zinc-400 max-w-2xl mx-auto text-lg">
            Funds only ever move through the contract — never the operator. Prize pools are escrowed up front,
            winners pull-claim, and every outcome is EIP-712 attested. The digest is computed identically in
            Solidity, the browser (ethers), and the isolated Rust relayer.
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-3 gap-8 mb-12">
          {stats.map((stat, idx) => (
            <motion.div
              key={stat.label}
              initial={{ opacity: 0, y: 30 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.5, delay: idx * 0.15 }}
              className="glass-panel p-8 text-center group hover:bg-green-500/5 transition-colors"
            >
              <div className="w-14 h-14 rounded-2xl bg-green-500/10 border border-green-500/30 flex items-center justify-center mx-auto mb-4 text-green-400 group-hover:scale-110 transition-transform shadow-[0_0_15px_rgba(34,197,94,0.2)]">
                <stat.icon className="w-7 h-7" />
              </div>
              <div className="text-5xl font-black text-white mb-2 drop-shadow-[0_0_15px_rgba(34,197,94,0.3)]">
                {stat.value}
              </div>
              <div className="text-lg font-bold text-zinc-200">{stat.label}</div>
              <div className="text-sm text-zinc-500">{stat.sublabel}</div>
            </motion.div>
          ))}
        </div>

        <div className="flex flex-wrap justify-center gap-3">
          {badges.map((badge, idx) => (
            <motion.span
              key={badge}
              initial={{ opacity: 0, scale: 0.9 }}
              whileInView={{ opacity: 1, scale: 1 }}
              viewport={{ once: true }}
              transition={{ duration: 0.3, delay: idx * 0.05 }}
              className="px-4 py-2 rounded-full text-xs font-bold tracking-wide uppercase bg-white/5 border border-white/10 text-zinc-300 hover:bg-brand-cyan/10 hover:border-brand-cyan/30 hover:text-brand-cyan transition-all cursor-default"
            >
              {badge}
            </motion.span>
          ))}
        </div>
      </div>
    </section>
  );
}
