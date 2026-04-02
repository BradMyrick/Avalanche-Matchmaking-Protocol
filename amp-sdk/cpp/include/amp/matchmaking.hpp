#pragma once
#include <string>
#include <vector>

namespace amp {

class RuleSetBuilder {
public:
    RuleSetBuilder(const std::string& game_id, const std::string& name) 
        : game_id_(game_id), name_(name) {}

    RuleSetBuilder& with_skill_window(float max_diff) {
        max_skill_diff_ = max_diff;
        return *this;
    }

    RuleSetBuilder& with_latency_limit(uint32_t max_ping_ms) {
        max_ping_ms_ = max_ping_ms;
        return *this;
    }

    RuleSetBuilder& enable_backfill(uint64_t timeout_ms, float skill_tolerance) {
        backfill_enabled_ = true;
        timeout_ms_ = timeout_ms;
        backfill_skill_tolerance_ = skill_tolerance;
        return *this;
    }

private:
    std::string game_id_;
    std::string name_;
    float max_skill_diff_ = 300.0f;
    uint32_t max_ping_ms_ = 150;
    uint64_t timeout_ms_ = 60000;
    bool backfill_enabled_ = false;
    float backfill_skill_tolerance_ = 100.0f;
    std::vector<std::string> required_roles_;
};

} // namespace amp
