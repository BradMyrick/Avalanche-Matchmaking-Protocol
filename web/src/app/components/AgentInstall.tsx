"use client";

import { useState } from "react";
import { Bot, Check, Copy, Download } from "lucide-react";

const SKILL_URL = "/download/SKILL.md";

const ADD_SKILL_PROMPT = `Add the AMP Tournaments skill to your agent setup:

Download ${"https://playwithamp.xyz" + SKILL_URL} and save it as:
- Claude Code:  .claude/skills/amp-tournaments/SKILL.md
- Cursor:       .cursor/rules/amp-tournaments.md
- Other agents: append to AGENTS.md (or your agent's rules file)

Read the file after saving. Confirm you installed it, then wait for my next instruction.`;

const USE_SKILL_PROMPT = `Using the amp-tournaments skill you installed, add on-chain tournaments to this project:

1. Install the AMP SDK for this stack.
2. Add a "create cup" flow: fund an AVAX prize pool with a 60/30/10 payout split and a 7-day finalize deadline.
3. Run a single-elimination bracket from our match results.
4. Finalize with an EIP-712 verifier signature (key from the AMP_VERIFIER_KEY env var — never hardcode it).
5. Add a winner claim flow (getClaimablePrize → claimPrize).

Follow the skill's rules and its definition of done. Ask me for the verifier address and prize amount before writing transactions.`;

type Copied = "skill" | "add" | "use" | null;

function CopyButton({
  text,
  kind,
  copied,
  setCopied,
  label,
}: {
  text: string;
  kind: Exclude<Copied, null>;
  copied: Copied;
  setCopied: (c: Copied) => void;
  label: string;
}) {
  const [failed, setFailed] = useState(false);
  return (
    <button
      onClick={async () => {
        try {
          await navigator.clipboard.writeText(text);
          setCopied(kind);
          setTimeout(() => setCopied(null), 1600);
        } catch {
          setFailed(true);
          setTimeout(() => setFailed(false), 2000);
        }
      }}
      className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wider bg-white/10 hover:bg-white/20 border border-white/10 transition-colors"
    >
      {copied === kind ? (
        <Check className="w-3.5 h-3.5 text-green-400" />
      ) : (
        <Copy className="w-3.5 h-3.5" />
      )}
      {copied === kind ? "Copied" : failed ? "Copy failed" : label}
    </button>
  );
}

/** Agentic install: SKILL.md rulefile + the two prompts to add and use it. */
export default function AgentInstall() {
  const [copied, setCopied] = useState<Copied>(null);

  const copySkill = async () => {
    try {
      const res = await fetch(SKILL_URL);
      const text = await res.text();
      await navigator.clipboard.writeText(text);
      setCopied("skill");
      setTimeout(() => setCopied(null), 1600);
    } catch {
      /* download link remains as fallback */
    }
  };

  return (
    <div className="glass-panel rounded-3xl border border-brand-cyan/20 p-8">
      <div className="flex items-center gap-3 mb-3">
        <div className="w-10 h-10 rounded-xl bg-brand-cyan/15 border border-brand-cyan/30 flex items-center justify-center text-brand-cyan">
          <Bot className="w-5 h-5" />
        </div>
        <h2 className="text-xl font-bold uppercase tracking-tight text-white">
          Building with an AI agent?
        </h2>
      </div>
      <p className="text-sm text-zinc-400 mb-6 max-w-2xl">
        AMP ships as a <span className="text-white font-bold">SKILL.md</span> rulefile — the
        complete integration playbook your coding agent can follow: installs per engine, the
        five-step flow, security rules, tests, and a definition of done. Two prompts and
        your agent builds the whole tournament stack.
      </p>

      <div className="space-y-5">
        <div className="rounded-2xl border border-white/10 bg-black/60 p-4">
          <div className="flex items-center justify-between gap-3 mb-2 flex-wrap">
            <span className="text-xs font-black uppercase tracking-widest text-brand-cyan">
              Prompt 1 — install the skill
            </span>
            <CopyButton
              text={ADD_SKILL_PROMPT}
              kind="add"
              copied={copied}
              setCopied={setCopied}
              label="Copy prompt"
            />
          </div>
          <p className="font-mono text-[11px] sm:text-xs text-zinc-400 leading-relaxed whitespace-pre-wrap">
            {ADD_SKILL_PROMPT}
          </p>
        </div>

        <div className="rounded-2xl border border-white/10 bg-black/60 p-4">
          <div className="flex items-center justify-between gap-3 mb-2 flex-wrap">
            <span className="text-xs font-black uppercase tracking-widest text-brand-cyan">
              Prompt 2 — build with it
            </span>
            <CopyButton
              text={USE_SKILL_PROMPT}
              kind="use"
              copied={copied}
              setCopied={setCopied}
              label="Copy prompt"
            />
          </div>
          <p className="font-mono text-[11px] sm:text-xs text-zinc-400 leading-relaxed whitespace-pre-wrap">
            {USE_SKILL_PROMPT}
          </p>
        </div>
      </div>

      <div className="mt-6 flex flex-wrap items-center gap-3">
        <button
          onClick={copySkill}
          className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wider bg-brand-cyan text-black hover:bg-white transition-colors"
        >
          {copied === "skill" ? (
            <Check className="w-3.5 h-3.5" />
          ) : (
            <Copy className="w-3.5 h-3.5" />
          )}
          {copied === "skill" ? "Copied" : "Copy SKILL.md"}
        </button>
        <a
          href={SKILL_URL}
          download
          className="inline-flex items-center gap-1.5 px-3 py-1.5 rounded-lg text-xs font-bold uppercase tracking-wider bg-white/10 hover:bg-white/20 border border-white/10 transition-colors"
        >
          <Download className="w-3.5 h-3.5" />
          Download
        </a>
        <span className="text-xs text-zinc-500">
          Drop it at <code className="text-zinc-400">.claude/skills/</code> ·{" "}
          <code className="text-zinc-400">.cursor/rules/</code> · repo root
        </span>
      </div>
    </div>
  );
}
