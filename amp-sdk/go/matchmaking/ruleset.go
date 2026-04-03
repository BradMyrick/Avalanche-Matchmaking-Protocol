package matchmaking

import (
	"github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated"
	"capnproto.org/go/capnp/v3"
)

// RuleSetBuilder provides a fluent interface for constructing MatchmakingRulesets.
type RuleSetBuilder struct {
	msg     *capnp.Message
	ruleset generated.MatchmakingRuleSet
	rules   []generated.MatchmakingRule
}

// NewRuleSetBuilder initializes a new RuleSetBuilder.
func NewRuleSetBuilder() (*RuleSetBuilder, error) {
	msg, seg, err := capnp.NewMessage(capnp.SingleSegment(nil))
	if err != nil {
		return nil, err
	}

	ruleset, err := generated.NewMatchmakingRuleSet(seg)
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

// WithID sets the internal ID of the RuleSet.
func (b *RuleSetBuilder) WithID(id string) *RuleSetBuilder {
	_ = b.ruleset.SetRuleSetId([]byte(id))
	return b
}

// WithMaxSkillDiff adds a skill difference rule.
func (b *RuleSetBuilder) WithMaxSkillDiff(diff float32) *RuleSetBuilder {
	rule, _ := generated.NewMatchmakingRule(b.ruleset.Segment())
	rule.SetRuleName("MaxSkillDiff")
	rule.SetType(generated.RuleType_skillDifference)
	rule.SetWeight(1.0)
	rule.SetIsHardConstraint(true)
	
	// Create Parameter for diff
	paramList, _ := generated.NewParameterList(b.ruleset.Segment(), 1)
	param := paramList.At(0)
	param.SetName("maxDiff")
	paramValue := param.Value()
	paramValue.SetFloatVal(diff)
	
	rule.SetParameters(paramList)
	b.rules = append(b.rules, rule)
	return b
}

// WithMaxLatency adds a max latency rule.
func (b *RuleSetBuilder) WithMaxLatency(maxPingMs uint32) *RuleSetBuilder {
	rule, _ := generated.NewMatchmakingRule(b.ruleset.Segment())
	rule.SetRuleName("MaxLatency")
	rule.SetType(generated.RuleType_latencyLimit)
	rule.SetWeight(1.0)
	rule.SetIsHardConstraint(true)
	
	paramList, _ := generated.NewParameterList(b.ruleset.Segment(), 1)
	param := paramList.At(0)
	param.SetName("maxPingMs")
	paramValue := param.Value()
	paramValue.SetUintVal(uint64(maxPingMs))
	
	rule.SetParameters(paramList)
	b.rules = append(b.rules, rule)
	return b
}

// Build finalizes and returns the MatchmakingRuleSet.
func (b *RuleSetBuilder) Build() (generated.MatchmakingRuleSet, error) {
	ruleList, err := generated.NewMatchmakingRuleList(b.ruleset.Segment(), int32(len(b.rules)))
	if err != nil {
		return b.ruleset, err
	}
	
	for i, r := range b.rules {
		ruleList.Set(i, r)
	}
	
	err = b.ruleset.SetRules(ruleList)
	return b.ruleset, err
}
