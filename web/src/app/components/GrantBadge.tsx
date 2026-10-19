"use client";

import { motion } from "framer-motion";
import Image from "next/image";
import { Award } from "lucide-react";

export default function GrantBadge() {
  return (
    <motion.div
      initial={{ opacity: 0, y: 20 }}
      animate={{ opacity: 1, y: 0 }}
      transition={{ duration: 0.8, delay: 0.3 }}
      className="mb-10"
    >
      <div className="inline-flex items-center gap-3 px-5 py-3 rounded-full bg-gradient-to-r from-brand-red/20 via-black/60 to-brand-cyan/20 border border-brand-red/30 shadow-[0_0_30px_rgba(232,65,66,0.15)] hover:shadow-[0_0_40px_rgba(232,65,66,0.3)] transition-shadow duration-500">
        <div className="w-10 h-10 rounded-full overflow-hidden bg-black flex items-center justify-center border border-brand-red/40 shadow-[0_0_15px_rgba(232,65,66,0.3)]">
          <Image
            src="/grant_badge.png"
            alt="Avalanche Build Games 2026"
            width={40}
            height={40}
            className="w-full h-full object-contain"
          />
        </div>
        <div className="flex flex-col">
          <span className="text-[11px] font-bold tracking-[0.2em] uppercase text-brand-red">
            Avalanche Build Games 2026
          </span>
          <span className="text-sm font-semibold text-white/90">
            Grant Recipient - $15,000 USD
          </span>
        </div>
        <Award className="w-5 h-5 text-yellow-400 ml-1 drop-shadow-[0_0_8px_rgba(250,204,21,0.6)]" />
      </div>
    </motion.div>
  );
}
