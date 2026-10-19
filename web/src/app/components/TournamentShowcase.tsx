"use client";

import Image from "next/image";
import { motion } from "framer-motion";
import { Trophy, Coins, ShieldCheck, Users, MapPin } from "lucide-react";

const tiles = [
  {
    icon: MapPin,
    title: "Any Venue",
    description: "School gym, esports arena, Discord server - run a cup anywhere.",
    iconWrap: "bg-brand-cyan/15 border-brand-cyan/30 text-brand-cyan",
  },
  {
    icon: Trophy,
    title: "Live Brackets",
    description: "Seeded by on-chain Glicko-2 rating. Streamed in real time.",
    iconWrap: "bg-yellow-400/15 border-yellow-400/30 text-yellow-400",
  },
  {
    icon: Coins,
    title: "Instant Payouts",
    description: "Escrowed prize pools. Champions pull-claim on Avalanche.",
    iconWrap: "bg-brand-red/15 border-brand-red/30 text-brand-red",
  },
  {
    icon: ShieldCheck,
    title: "Portable Credentials",
    description: "Every match mints a verifiable skill record players keep.",
    iconWrap: "bg-green-400/15 border-green-400/30 text-green-400",
  },
];

export default function TournamentShowcase() {
  return (
    <section id="showcase" className="py-32 relative overflow-hidden scroll-mt-32">
      <div className="absolute top-1/2 left-0 w-[80%] h-[400px] bg-brand-cyan/5 blur-[150px] rounded-full pointer-events-none" />
      <div className="absolute bottom-0 right-0 w-[60%] h-[300px] bg-brand-red/5 blur-[130px] rounded-full pointer-events-none" />

      <div className="max-w-7xl mx-auto px-6 relative z-10">
        <div className="text-center mb-12">
          <div className="inline-flex items-center gap-2 px-3 py-1 rounded-full bg-yellow-400/10 border border-yellow-400/30 mb-4">
            <Trophy className="w-3 h-3 text-yellow-400" />
            <span className="text-xs font-bold text-yellow-400 tracking-widest uppercase">
              AMP Cup · In The Wild
            </span>
          </div>
          <h2 className="text-4xl md:text-6xl font-black mb-6 text-white uppercase tracking-tight drop-shadow-[0_0_20px_rgba(0,229,255,0.25)]">
            Where Champions Are <span className="text-brand-cyan">Crowned</span>
          </h2>
          <p className="text-zinc-400 max-w-2xl mx-auto text-lg">
            From a school gym to a global stage - any community can run a verifiable arena.
          </p>
        </div>

        {/* Cinematic feature frame */}
        <motion.div
          initial={{ opacity: 0, scale: 0.97, y: 24 }}
          whileInView={{ opacity: 1, scale: 1, y: 0 }}
          viewport={{ once: true }}
          transition={{ duration: 0.7 }}
          className="relative w-full aspect-[16/9] rounded-3xl overflow-hidden border border-brand-cyan/20 shadow-[0_40px_120px_-20px_rgba(0,0,0,1)] group"
        >
          {/* Base photograph */}
          <Image
            src="/tourny1-1920.webp"
            alt="A community gaming tournament in a school gymnasium with Avalanche branding and a projector displaying the live tournament bracket, powered by the AMP Verifiable Tournament Engine."
            fill
            sizes="(max-width: 1280px) 100vw, 1216px"
            className="object-cover transition-transform duration-[3000ms] ease-out group-hover:scale-[1.04]"
          />

          {/* Brand neon washes (screen blend adds atmospheric glow to the dark scene) */}
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_18%_22%,rgba(0,229,255,0.28),transparent_55%)] mix-blend-screen pointer-events-none" />
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_85%_80%,rgba(232,65,66,0.22),transparent_50%)] mix-blend-screen pointer-events-none" />

          {/* Faint grid overlay to match the site aesthetic */}
          <div
            className="absolute inset-0 opacity-[0.05] pointer-events-none"
            style={{
              backgroundImage:
                "linear-gradient(rgba(0,229,255,0.4) 1px, transparent 1px), linear-gradient(90deg, rgba(0,229,255,0.4) 1px, transparent 1px)",
              backgroundSize: "44px 44px",
            }}
          />

          {/* Vignette to focus the center */}
          <div className="absolute inset-0 shadow-[inset_0_0_140px_70px_rgba(0,0,0,0.75)] pointer-events-none" />

          {/* Legibility gradients */}
          <div className="absolute inset-0 bg-gradient-to-t from-black via-black/15 to-black/40 pointer-events-none" />

          {/* Corner brackets - broadcast frame feel */}
          <div className="absolute top-4 left-4 w-6 h-6 border-t-2 border-l-2 border-brand-cyan/70 rounded-tl-md pointer-events-none" />
          <div className="absolute top-4 right-4 w-6 h-6 border-t-2 border-r-2 border-brand-cyan/70 rounded-tr-md pointer-events-none" />
          <div className="absolute bottom-4 left-4 w-6 h-6 border-b-2 border-l-2 border-brand-cyan/70 rounded-bl-md pointer-events-none" />
          <div className="absolute bottom-4 right-4 w-6 h-6 border-b-2 border-r-2 border-brand-cyan/70 rounded-br-md pointer-events-none" />

          {/* Top-left live badge */}
          <div className="absolute top-6 left-6 md:top-8 md:left-8 flex items-center gap-2 px-3 py-1.5 rounded-full bg-black/60 backdrop-blur-md border border-brand-cyan/30 shadow-[0_0_20px_rgba(0,229,255,0.2)]">
            <span className="relative flex h-2 w-2">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-brand-cyan opacity-75"></span>
              <span className="relative inline-flex rounded-full h-2 w-2 bg-brand-cyan"></span>
            </span>
            <span className="text-[11px] font-bold tracking-widest uppercase text-white">Bracket Live</span>
          </div>

          {/* Lower-third caption */}
          <div className="absolute inset-x-0 bottom-0 p-6 md:p-10">
            <div className="flex items-center gap-2 mb-3">
              <div className="w-1.5 h-1.5 rounded-full bg-brand-red shadow-[0_0_8px_rgba(232,65,66,0.9)]" />
              <span className="text-[11px] font-bold tracking-[0.25em] uppercase text-brand-red">
                Community Cup · Sponsored Prize Pool
              </span>
            </div>
            <p className="text-lg md:text-2xl font-bold text-white leading-tight max-w-3xl drop-shadow-[0_2px_12px_rgba(0,0,0,0.95)]">
              <span className="text-brand-cyan">Powered by the AMP Verifiable Tournament Engine.</span>{" "}
              <br />
              Prize pool escrowed and settled on Avalanche.
            </p>
          </div>
        </motion.div>

        {/* Supporting feature tiles */}
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4 md:gap-5 mt-8">
          {tiles.map((tile, idx) => (
            <motion.div
              key={tile.title}
              initial={{ opacity: 0, y: 20 }}
              whileInView={{ opacity: 1, y: 0 }}
              viewport={{ once: true }}
              transition={{ duration: 0.4, delay: idx * 0.08 }}
              className="glass-panel p-5 md:p-6 hover:bg-white/5 transition-colors group"
            >
              <div className={`w-10 h-10 rounded-sm border flex items-center justify-center mb-4 group-hover:scale-110 transition-transform ${tile.iconWrap}`}>
                <tile.icon className="w-5 h-5" />
              </div>
              <h3 className="text-sm md:text-base font-bold uppercase tracking-wide text-white mb-1">
                {tile.title}
              </h3>
              <p className="text-xs md:text-sm text-zinc-400 leading-relaxed">{tile.description}</p>
            </motion.div>
          ))}
        </div>

        <div className="mt-10 flex items-center justify-center gap-2 text-sm text-zinc-500">
          <Users className="w-4 h-4 text-brand-cyan" />
          <span>Run by guilds, DAOs, studios, and creators worldwide.</span>
        </div>
      </div>
    </section>
  );
}
