"use client";

import Link from "next/link";
import Image from "next/image";
import { Github } from "lucide-react";

export default function SiteHeader() {
  return (
    <header className="fixed top-0 inset-x-0 z-50 h-14 border-b border-brand-cyan/20 bg-black/80 backdrop-blur-xl">
      <div className="max-w-7xl mx-auto px-6 h-full flex items-center justify-between gap-6">
        <Link href="/" className="flex items-center gap-2 shrink-0 whitespace-nowrap">
          <Image src="/amp-icon.png" alt="" width={28} height={28} className="w-7 h-7" />
          <span className="font-black tracking-widest uppercase text-white">AMP</span>
          <span className="text-[10px] uppercase tracking-widest text-zinc-400">Tournament Protocol</span>
        </Link>
        <nav className="flex items-center gap-7 text-xs font-semibold uppercase tracking-wider text-zinc-300 whitespace-nowrap">
          <Link href="/setup" className="hover:text-white">Host</Link>
          <Link href="/claim" className="hover:text-white">Claim</Link>
          <Link href="/#demo" className="hover:text-white">Demo</Link>
          <Link href="/docs" className="hover:text-white">Docs</Link>
        </nav>
        <Link
          href="https://github.com/bradmyrick/Avalanche-Matchmaking-Protocol"
          target="_blank"
          aria-label="GitHub"
          className="text-zinc-400 hover:text-brand-cyan shrink-0"
        >
          <Github className="w-4 h-4" />
        </Link>
      </div>
    </header>
  );
}
