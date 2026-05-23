package client

import (
	"context"
	"fmt"
	"net"

	"capnproto.org/go/capnp/v3/rpc"
	"github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated"
)

type AMPClient struct {
	rpcConn     *rpc.Conn
	service     generated.GameSessionService
	userSession generated.UserSession
}

func NewClient(ctx context.Context, addr string) (*AMPClient, error) {
	conn, err := net.Dial("tcp", addr)
	if err != nil {
		return nil, err
	}

	rpcConn := rpc.NewConn(rpc.NewStreamTransport(conn), nil)
	bootstrap := rpcConn.Bootstrap(ctx)

	return &AMPClient{
		rpcConn: rpcConn,
		service: generated.GameSessionService(bootstrap),
	}, nil
}

func (c *AMPClient) Login(ctx context.Context, gameID uint64, signature []byte, challengePayload []byte) error {
	future, release := c.service.Login(ctx, func(p generated.GameSessionService_login_Params) error {
		p.SetGameId(gameID)
		if err := p.SetSignature(signature); err != nil {
			return err
		}
		return p.SetChallengePayload(challengePayload)
	})
	defer release()

	result, err := future.Struct()
	if err != nil {
		return fmt.Errorf("login failed: %w", err)
	}

	c.userSession = result.Session()
	return nil
}

func (c *AMPClient) RequestChallenge(ctx context.Context, gameID uint64) ([]byte, uint64, error) {
	future, release := c.service.RequestChallenge(ctx, func(p generated.GameSessionService_requestChallenge_Params) error {
		p.SetGameId(gameID)
		return nil
	})
	defer release()

	result, err := future.Struct()
	if err != nil {
		return nil, 0, fmt.Errorf("requestChallenge failed: %w", err)
	}

	challenge, err := result.Challenge()
	if err != nil {
		return nil, 0, fmt.Errorf("failed to read challenge: %w", err)
	}

	expiresAt := result.ExpiresAt()
	challengeCopy := make([]byte, len(challenge))
	copy(challengeCopy, challenge)

	return challengeCopy, expiresAt, nil
}

func (c *AMPClient) RequestMatch(ctx context.Context, req generated.GameMatchRequest) (*MatchSession, error) {
	if !c.userSession.IsValid() {
		return nil, fmt.Errorf("not logged in; call Login first")
	}

	future, release := c.userSession.RequestMatch(ctx, func(p generated.UserSession_requestMatch_Params) error {
		return p.SetReq(req)
	})
	defer release()

	result, err := future.Struct()
	if err != nil {
		return nil, fmt.Errorf("requestMatch failed: %w", err)
	}

	assignment, err := result.Assignment()
	if err != nil {
		return nil, fmt.Errorf("failed to read match assignment: %w", err)
	}

	matchID, _ := assignment.MatchId()
	matchSession := future.Session()

	return &MatchSession{
		MatchID: matchID,
		Session: matchSession.AddRef(),
		client:  c,
	}, nil
}

func (c *AMPClient) Reconnect(ctx context.Context, matchID []byte) (*MatchSession, error) {
	if !c.userSession.IsValid() {
		return nil, fmt.Errorf("not logged in; call Login first")
	}

	future, release := c.userSession.Reconnect(ctx, func(p generated.UserSession_reconnect_Params) error {
		return p.SetMatchId(matchID)
	})
	defer release()

	_, err := future.Struct()
	if err != nil {
		return nil, fmt.Errorf("reconnect failed: %w", err)
	}

	sessionClient := future.Session()

	return &MatchSession{
		MatchID: matchID,
		Session: sessionClient.AddRef(),
		client:  c,
	}, nil
}

func (c *AMPClient) SubmitOutcome(ctx context.Context, matchSession *MatchSession, submission generated.OutcomeSubmission) ([]byte, error) {
	if matchSession == nil || !matchSession.Session.IsValid() {
		return nil, fmt.Errorf("matchSession is nil or invalid")
	}

	future, release := matchSession.Session.SubmitOutcome(ctx, func(p generated.MatchSession_submitOutcome_Params) error {
		return p.SetSubmission(submission)
	})
	defer release()

	result, err := future.Struct()
	if err != nil {
		return nil, fmt.Errorf("submitOutcome failed: %w", err)
	}

	sig, err := result.Signature()
	if err != nil {
		return nil, fmt.Errorf("failed to read verifier signature: %w", err)
	}

	sigCopy := make([]byte, len(sig))
	copy(sigCopy, sig)
	return sigCopy, nil
}

func (c *AMPClient) EmitTelemetry(ctx context.Context, matchSession *MatchSession, event generated.AmpTelemetryEvent) error {
	if matchSession == nil || !matchSession.Session.IsValid() {
		return fmt.Errorf("matchSession is nil or invalid")
	}

	future, release := matchSession.Session.EmitTelemetry(ctx, func(p generated.MatchSession_emitTelemetry_Params) error {
		return p.SetEvent(event)
	})
	defer release()

	_, err := future.Struct()
	if err != nil {
		return fmt.Errorf("emitTelemetry failed: %w", err)
	}
	return nil
}

func (c *AMPClient) EmitGameEvent(ctx context.Context, matchSession *MatchSession, event generated.GameEvent) error {
	if matchSession == nil || !matchSession.Session.IsValid() {
		return fmt.Errorf("matchSession is nil or invalid")
	}

	future, release := matchSession.Session.EmitGameEvent(ctx, func(p generated.MatchSession_emitGameEvent_Params) error {
		return p.SetEvent(event)
	})
	defer release()

	_, err := future.Struct()
	if err != nil {
		return fmt.Errorf("emitGameEvent failed: %w", err)
	}
	return nil
}

func (c *AMPClient) Close() error {
	if c.userSession.IsValid() {
		c.userSession.Release()
	}
	c.service.Release()
	return c.rpcConn.Close()
}
