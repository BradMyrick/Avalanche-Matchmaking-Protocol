package matchmaking

import (
	"fmt"

	"capnproto.org/go/capnp/v3"
	"github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated"
)

type RuleSetBuilder struct {
	msg     *capnp.Message
	ruleset generated.RuleSet
	rules   []generated.MatchmakingRule
}

func NewRuleSetBuilder() (*RuleSetBuilder, error) {
	msg, seg, err := capnp.NewMessage(capnp.SingleSegment(nil))
	if err != nil {
		return nil, err
	}

	ruleset, err := generated.NewRuleSet(seg)
	if err != nil {
		return nil, err
	}

	ruleset.SetVersion("1.0.0")

	return &RuleSetBuilder{
		msg:     msg,
		ruleset: ruleset,
		rules:   make([]generated.MatchmakingRule, 0),
	}, nil
}

func (b *RuleSetBuilder) WithID(id string) *RuleSetBuilder {
	_ = b.ruleset.SetRuleSetId([]byte(id))
	return b
}

func (b *RuleSetBuilder) WithName(name string) *RuleSetBuilder {
	_ = b.ruleset.SetName(name)
	return b
}

func (b *RuleSetBuilder) WithGameID(gameID []byte) *RuleSetBuilder {
	_ = b.ruleset.SetGameId(gameID)
	return b
}

func (b *RuleSetBuilder) WithMaxSkillDiff(ruleName string, diff float32) *RuleSetBuilder {
	rule, err := generated.NewMatchmakingRule(b.ruleset.Segment())
	if err != nil {
		return b
	}
	rule.SetName(ruleName)
	rule.SetType(generated.RuleType_skill)
	rule.SetWeight(1.0)
	rule.SetIsHardConstraint(true)

	rule.SetParameters([]byte(fmt.Sprintf("{\"maxDiff\":%f}", diff)))
	b.rules = append(b.rules, rule)
	return b
}

func (b *RuleSetBuilder) WithMaxLatency(ruleName string, maxPingMs uint32) *RuleSetBuilder {
	rule, err := generated.NewMatchmakingRule(b.ruleset.Segment())
	if err != nil {
		return b
	}
	rule.SetName(ruleName)
	rule.SetType(generated.RuleType_latency)
	rule.SetWeight(1.0)
	rule.SetIsHardConstraint(true)

	rule.SetParameters([]byte(fmt.Sprintf("{\"maxPingMs\":%d}", maxPingMs)))
	b.rules = append(b.rules, rule)
	return b
}

func (b *RuleSetBuilder) Build() (generated.RuleSet, error) {
	ruleList, err := generated.NewMatchmakingRule_List(b.ruleset.Segment(), int32(len(b.rules)))
	if err != nil {
		return b.ruleset, err
	}

	for i, r := range b.rules {
		if err := ruleList.Set(i, r); err != nil {
			return b.ruleset, err
		}
	}

	return b.ruleset, b.ruleset.SetRules(ruleList)
}
