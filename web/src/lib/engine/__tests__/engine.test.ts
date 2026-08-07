import { describe, it, expect } from "vitest";
import { Tournament, Outcome, TournamentState, type Entrant } from "../index";

/** Entrants seeded 1..n (seed 1 = strongest). */
function seeds(n: number): Entrant<number>[] {
  return Array.from({ length: n }, (_, i) => ({ id: i + 1, seed: i + 1 }));
}

/** "Higher seed wins" for a ready match: side A if a.seed <= b.seed. */
function higherSeedWins<I>(m: { a: { seed: number } | null; b: { seed: number } | null }): Outcome {
  const a = m.a!.seed;
  const b = m.b!.seed;
  return a <= b ? Outcome.A : Outcome.B;
}

/** Play every ready match (higher seed wins) until the tournament completes. */
function playToCompletion<I>(t: Tournament<I>) {
  let guard = 0;
  while (!t.isComplete() && guard < 100_000) {
    guard++;
    const pending = t.pending();
    if (pending.length === 0) {
      t.advance();
      if (t.pending().length === 0 && !t.isComplete()) break;
      continue;
    }
    for (const id of pending) {
      const m = t.matches().find((x) => x.id === id);
      if (!m || m.outcome !== null) continue;
      t.record(id, higherSeedWins(m as { a: { seed: number } | null; b: { seed: number } | null }));
    }
  }
}

describe("single-elimination", () => {
  it("top seed is champion for any field size 2..64", () => {
    for (let n = 2; n <= 64; n++) {
      const t = Tournament.new<number>({ kind: TournamentState.SingleElimination }, seeds(n));
      playToCompletion(t);
      expect(t.isComplete(), `n=${n}`).toBe(true);
      expect(t.champion(), `n=${n}`).toBe(1);
      expect(t.winners()[0], `n=${n}`).toBe(1);
    }
  });

  it("handles non-power-of-two fields via byes", () => {
    for (const n of [3, 5, 6, 7, 9, 11, 13]) {
      const t = Tournament.new<number>({ kind: TournamentState.SingleElimination }, seeds(n));
      playToCompletion(t);
      expect(t.isComplete(), `n=${n}`).toBe(true);
      expect(t.champion(), `n=${n}`).toBe(1);
    }
  });

  it("every non-champion lost exactly once; champion lost zero", () => {
    const t = Tournament.new<number>({ kind: TournamentState.SingleElimination }, seeds(8));
    playToCompletion(t);
    for (const s of t.standings()) {
      if (s.id === t.champion()) {
        expect(s.losses).toBe(0);
        expect(s.wins).toBeGreaterThan(0);
      } else {
        expect(s.losses).toBe(1);
      }
    }
  });

  it("winners are ordered champion first, runner-up second", () => {
    const t = Tournament.new<number>({ kind: TournamentState.SingleElimination }, seeds(8));
    playToCompletion(t);
    const w = t.winners();
    expect(w[0]).toBe(1); // champion
    expect(w[1]).toBe(2); // runner-up (lost final to seed 1)
    expect(w.length).toBe(8);
  });

  it("rejects a draw outcome", () => {
    const t = Tournament.new<number>({ kind: TournamentState.SingleElimination }, seeds(2));
    const id = t.pending()[0];
    expect(() => t.record(id, Outcome.Draw)).toThrow();
  });
});

describe("round-robin", () => {
  it("every pair plays exactly once, for n=2..8", () => {
    for (let n = 2; n <= 8; n++) {
      const t = Tournament.new<number>({ kind: TournamentState.RoundRobin }, seeds(n));
      playToCompletion(t);
      expect(t.isComplete(), `n=${n}`).toBe(true);
      const played = new Set<string>();
      for (const m of t.matches()) {
        const a = m.a?.id;
        const b = m.b?.id;
        if (a == null || b == null) continue;
        const key = [a, b].sort((x, y) => x - y).join(",");
        expect(played.has(key), `n=${n}: pair ${key} played twice`).toBe(false);
        played.add(key);
      }
      expect(played.size).toBe((n * (n - 1)) / 2);
    }
  });
});

describe("swiss", () => {
  it("never rematches, completes, and top seed leads", () => {
    const n = 8;
    const rounds = 3;
    const t = Tournament.new<number>({ kind: TournamentState.Swiss, rounds }, seeds(n));
    playToCompletion(t);
    expect(t.isComplete()).toBe(true);
    const seen = new Set<string>();
    for (const m of t.matches()) {
      const a = m.a?.id;
      const b = m.b?.id;
      if (a == null || b == null) continue;
      const key = [a, b].sort((x, y) => x - y).join(",");
      expect(seen.has(key), `rematch ${key}`).toBe(false);
      seen.add(key);
    }
    expect(seen.size).toBe((n / 2) * rounds);
    expect(t.standings()[0].id).toBe(1);
    expect(t.standings()[0].wins).toBe(rounds);
  });
});

describe("validation", () => {
  it("rejects fewer than 2 entrants", () => {
    expect(() => Tournament.new<number>({ kind: TournamentState.SingleElimination }, seeds(1))).toThrow();
  });
  it("rejects duplicate ids", () => {
    expect(() =>
      Tournament.new<string>(
        { kind: TournamentState.SingleElimination },
        [
          { id: "a", seed: 1 },
          { id: "a", seed: 2 },
        ]
      )
    ).toThrow();
  });
  it("rejects swiss with zero rounds", () => {
    expect(() => Tournament.new<number>({ kind: TournamentState.Swiss, rounds: 0 }, seeds(8))).toThrow();
  });
});
