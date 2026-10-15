"use client";

import React, { useState, useEffect } from "react";

const cycle = [
  "Brackets",
  "Escrow",
  "Swiss",
  "Claims",
  "EIP-712",
  "Unity",
  "Unreal",
  "Web",
];

const words = [...cycle, ...cycle, "ANY ENGINE"];

export default function RotatingText() {
  const [currentIndex, setCurrentIndex] = useState(0);
  const [speed, setSpeed] = useState(280);

  useEffect(() => {
    if (currentIndex >= words.length - 1) return;

    const timer = setTimeout(() => {
      setCurrentIndex((prev) => prev + 1);
      setSpeed((prevSpeed) => Math.max(20, prevSpeed * 0.9));
    }, speed);

    return () => clearTimeout(timer);
  }, [currentIndex, speed]);

  const isLanding = currentIndex === words.length - 1;

  return (
    <span
      className={`inline-block whitespace-nowrap transition-colors font-black ${
        isLanding
          ? "duration-700 text-brand-cyan drop-shadow-[0_0_28px_rgba(0,229,255,0.85)]"
          : "duration-75 text-zinc-500"
      }`}
    >
      {words[currentIndex]}
    </span>
  );
}
