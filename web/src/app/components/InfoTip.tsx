"use client";
import { useState, type ReactNode } from "react";
import { Info } from "lucide-react";

/** Hover tooltip with styled explanation box. */
export function InfoTip({
  children,
  className = "",
}: {
  children: ReactNode;
  className?: string;
}) {
  const [show, setShow] = useState(false);
  return (
    <span
      className={`relative inline-flex ${className}`}
      onMouseEnter={() => setShow(true)}
      onMouseLeave={() => setShow(false)}
    >
      <Info className="w-3.5 h-3.5 text-zinc-500 hover:text-brand-cyan cursor-help" />
      {show && (
        <div className="absolute bottom-full left-1/2 -translate-x-1/2 mb-2 w-64 p-3 rounded-lg bg-black border border-brand-cyan/30 shadow-xl z-50 text-xs text-zinc-300 leading-relaxed">
          {children}
          <div className="absolute top-full left-1/2 -translate-x-1/2 w-0 h-0 border-l-4 border-r-4 border-t-4 border-l-transparent border-r-transparent border-t-brand-cyan/30" />
        </div>
      )}
    </span>
  );
}
