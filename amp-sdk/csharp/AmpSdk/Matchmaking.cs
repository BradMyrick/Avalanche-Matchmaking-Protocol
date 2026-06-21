using System.Collections.Generic;

namespace AmpSdk {

/// <summary>
/// Fluent builder for constructing matchmaking rule sets.
/// </summary>
public class RuleSetBuilder {
    private string gameId;
    private string name;
    private float maxSkillDiff = 300.0f;
    private uint maxPingMs = 150;
    private ulong timeoutMs = 60000;
    private bool backfillEnabled = false;
    private float backfillSkillTolerance = 100.0f;
    private List<string> requiredRoles = new List<string>();

    public RuleSetBuilder(string gameId, string name) {
        this.gameId = gameId;
        this.name = name;
    }

    /// <summary>
    /// Sets the maximum MMR difference allowed between matched players.
    /// </summary>
    public RuleSetBuilder WithSkillWindow(float maxDiff) {
        maxSkillDiff = maxDiff;
        return this;
    }

    /// <summary>
    /// Sets the maximum acceptable ping in milliseconds.
    /// </summary>
    public RuleSetBuilder WithLatencyLimit(uint maxPingMs) {
        this.maxPingMs = maxPingMs;
        return this;
    }

    /// <summary>
    /// Enables backfill matching after the initial timeout with relaxed skill tolerance.
    /// </summary>
    public RuleSetBuilder EnableBackfill(ulong timeoutMs, float skillTolerance) {
        backfillEnabled = true;
        this.timeoutMs = timeoutMs;
        backfillSkillTolerance = skillTolerance;
        return this;
    }
}

}
