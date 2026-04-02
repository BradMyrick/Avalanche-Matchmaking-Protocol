class RuleSetBuilder:
    def __init__(self, game_id: str, name: str):
        self.game_id = game_id
        self.name = name
        self.max_skill_diff = 300.0
        self.max_ping_ms = 150
        self.timeout_ms = 60000
        self.backfill_enabled = False
        self.backfill_skill_tolerance = 100.0
        self.required_roles = []

    def with_skill_window(self, max_diff: float):
        self.max_skill_diff = max_diff
        return self

    def with_latency_limit(self, max_ping_ms: int):
        self.max_ping_ms = max_ping_ms
        return self

    def enable_backfill(self, timeout_ms: int, skill_tolerance: float):
        self.backfill_enabled = True
        self.timeout_ms = timeout_ms
        self.backfill_skill_tolerance = skill_tolerance
        return self

    def build_dict(self) -> dict:
        return {
            "gameId": self.game_id,
            "name": self.name,
            "maxSkillDiff": self.max_skill_diff,
            "maxPingMs": self.max_ping_ms,
            "timeoutMs": self.timeout_ms,
            "backfillEnabled": self.backfill_enabled,
            "backfillSkillTolerance": self.backfill_skill_tolerance,
        }
