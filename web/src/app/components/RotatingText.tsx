"use client";

import React, { useState, useEffect } from "react";

const baseGames = [
  "Chess",
  "Fighters",
  "Card Games",
  "Shooters",
  "RTS",
  "Arcade",
  "Racing",
  "Strategy",
  "Auto-Battlers",
  "Esports",
  "Web3",
  "Unity",
  "Unreal",
  "Godot",
  "PyGame",
  "Rust",
];

// Combine the list twice, then end on the punchy landing word
const games = [...baseGames, ...baseGames, "ANY GAME"];

export default function RotatingText() {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [speed, setSpeed] = useState(300); // Start slow

  useEffect(() => {
    // If we reached the landing word (the last element), stop rotating
    if (currentIndex >= games.length - 1) {
      return;
    }

    const timer = setTimeout(() => {
      setCurrentIndex((prev) => prev + 1);
      // Decrease the delay to go faster and faster across the longer loop
      setSpeed((prevSpeed) => Math.max(20, prevSpeed * 0.90));
    }, speed);

    return () => clearTimeout(timer);
  }, [currentIndex, speed]);

  const isLanding = currentIndex === games.length - 1;

  return (
    <span
      className={`inline-block transition-all font-bold ${
        isLanding
          ? "duration-1000 text-brand-cyan drop-shadow-[0_0_50px_rgba(0,229,255,1)] scale-[1.3] md:scale-[1.5] animate-[pulse_2s_ease-in-out_infinite]"
          : "duration-75 text-zinc-500 drop-shadow-[0_0_10px_rgba(255,255,255,0.1)]"
      }`}
    >
      {games[currentIndex]}
    </span>
  );
}
