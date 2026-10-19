'use client';

import React, { useState, useEffect } from 'react';
import { motion, AnimatePresence } from 'framer-motion';
import { Trophy, Users, Swords, Coins, ArrowRight } from 'lucide-react';

const steps = [
  {
    id: 'create',
    title: 'Create Tournament',
    description: 'Organizer configures the bracket, rules, and funds the prize pool into escrow on Avalanche.',
    icon: <Trophy className="w-6 h-6" />,
    display: 'bg-brand-cyan/20 text-brand-cyan',
    list: 'bg-brand-cyan/10 text-brand-cyan',
  },
  {
    id: 'enter',
    title: 'Players Enter',
    description: 'Competitors join and are seeded by verifiable Glicko-2 skill rating. Any engine, any game.',
    icon: <Users className="w-6 h-6" />,
    display: 'bg-yellow-400/20 text-yellow-400',
    list: 'bg-yellow-400/10 text-yellow-400',
  },
  {
    id: 'compete',
    title: 'Matches Run',
    description: 'The bracket plays out. Every result is verifier-attested with EIP-712 signatures - tamper-proof.',
    icon: <Swords className="w-6 h-6" />,
    display: 'bg-brand-red/20 text-brand-red',
    list: 'bg-brand-red/10 text-brand-red',
  },
  {
    id: 'payout',
    title: 'Prize Payout',
    description: 'Champions claim instant, on-chain payouts. Winners take home escrowed prizes - no manual work.',
    icon: <Coins className="w-6 h-6" />,
    display: 'bg-green-400/20 text-green-400',
    list: 'bg-green-400/10 text-green-400',
  },
];

export default function MatchmakingDemo() {
  const [activeStep, setActiveStep] = useState(0);

  // Auto-loop the simulation continuously.
  useEffect(() => {
    const interval = setInterval(() => {
      setActiveStep((prev) => (prev + 1 >= steps.length ? 0 : prev + 1));
    }, 3000);
    return () => clearInterval(interval);
  }, []);

  // Side effect: when the simulation reaches the payout step, notify the feed.
  // Kept OUTSIDE the setState updater so we never dispatch during render.
  useEffect(() => {
    if (activeStep === steps.length - 1) {
      window.dispatchEvent(
        new CustomEvent('amp-tournament-payout', {
          detail: { tournamentId: `ampcup-${Math.floor(Math.random() * 1000)}` },
        })
      );
    }
  }, [activeStep]);

  return (
    <section id="demo" className="py-24 relative overflow-hidden scroll-mt-32">
      <div className="absolute top-0 left-1/2 -translate-x-1/2 w-[80%] h-[300px] bg-brand-cyan/5 blur-[120px] rounded-full pointer-events-none" />
      <div className="max-w-7xl mx-auto px-6 relative z-10">
        <div className="text-center mb-16">
          <h2 className="text-4xl md:text-5xl font-black mb-6 uppercase tracking-tight">
            From Signup to <span className="text-brand-cyan">Payout</span>
          </h2>
          <p className="text-zinc-400 max-w-2xl mx-auto text-lg">
            Four steps. Zero manual work. Every tournament runs end-to-end<br />
            <span className="text-yellow-400 drop-shadow-[0_0_20px_rgba(250,204,21,0.8)]">escrowed, verifiable, and trustless.</span>
          </p>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-12 items-center">
          {/* Vis Sim */}
          <div className="glass-panel p-8 min-h-[400px] flex flex-col justify-center items-center relative bg-black/40">
            <div className="absolute inset-0 bg-gradient-to-br from-brand-red/5 via-yellow-400/5 to-brand-cyan/5 pointer-events-none" />

            <div className="relative w-full max-w-md h-64">
              <AnimatePresence mode="wait">
                <motion.div
                  key={activeStep}
                  initial={{ opacity: 0, scale: 0.9, y: 20 }}
                  animate={{ opacity: 1, scale: 1, y: 0 }}
                  exit={{ opacity: 0, scale: 1.1, y: -20 }}
                  transition={{ duration: 0.5 }}
                  className="absolute inset-0 flex flex-col items-center justify-center text-center"
                >
                  <div className={`w-20 h-20 rounded-2xl flex items-center justify-center mb-6 shadow-[0_0_25px_rgba(0,229,255,0.25)] ${steps[activeStep].display}`}>
                    {steps[activeStep].icon}
                  </div>
                  <h3 className="text-2xl font-bold mb-4 text-white">{steps[activeStep].title}</h3>
                  <p className="text-zinc-300">{steps[activeStep].description}</p>
                </motion.div>
              </AnimatePresence>
            </div>

            {/* Progress dots */}
            <div className="mt-8 w-full flex justify-between items-center px-4">
              {steps.map((_, idx) => (
                <React.Fragment key={idx}>
                  <div
                    className={`w-3 h-3 rounded-full transition-all duration-500 ${idx === activeStep ? 'bg-brand-cyan scale-150 shadow-[0_0_10px_rgba(6,182,212,0.8)]' : 'bg-zinc-700'
                      }`}
                  />
                  {idx < steps.length - 1 && (
                    <div className="flex-1 h-[2px] mx-2 bg-zinc-800 overflow-hidden">
                      <motion.div
                        className="h-full bg-gradient-to-r from-yellow-400 via-brand-red to-brand-cyan"
                        initial={{ x: '-100%' }}
                        animate={idx === activeStep ? { x: '100%' } : { x: '-100%' }}
                        transition={{ duration: 2.5, repeat: idx === activeStep ? Infinity : 0, ease: "linear" }}
                      />
                    </div>
                  )}
                </React.Fragment>
              ))}
            </div>
          </div>

          {/* Step legend */}
          <div className="space-y-4">
            <div className="text-xs uppercase tracking-widest text-zinc-500 mb-2">The flow runs automatically</div>
            {steps.map((step, idx) => (
              <div
                key={step.id}
                className={`w-full text-left p-4 rounded-xl transition-all border flex items-center gap-4 ${activeStep === idx
                  ? 'bg-white/10 border-brand-cyan/50 translate-x-2'
                  : 'bg-transparent border-white/5'
                  }`}
              >
                <div className={`p-2 rounded-lg ${step.list}`}>
                  {step.icon}
                </div>
                <div>
                  <h4 className="font-bold text-white">{step.title}</h4>
                  <p className="text-xs text-zinc-500 line-clamp-1">{step.description}</p>
                </div>
                {activeStep === idx && <ArrowRight className="ml-auto w-5 h-5 text-brand-cyan" />}
              </div>
            ))}
          </div>
        </div>
      </div>
    </section>
  );
}
