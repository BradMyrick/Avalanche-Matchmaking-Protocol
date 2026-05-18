package client

import (
	"context"
	"fmt"
	"net"
	"time"

	"capnproto.org/go/capnp/v3"
	"capnproto.org/go/capnp/v3/rpc"
)

type AMPClient struct {
	rpcConn     *rpc.Conn
	bootstrap   capnp.Client
	userSession capnp.Client
}

func NewClient(ctx context.Context, addr string) (*AMPClient, error) {
	conn, err := net.Dial("tcp", addr)
	if err != nil {
		return nil, err
	}

	rpcConn := rpc.NewConn(rpc.NewStreamTransport(conn), nil)

	return &AMPClient{
		rpcConn:   rpcConn,
		bootstrap: rpcConn.Bootstrap(ctx),
	}, nil
}

func (c *AMPClient) Login(ctx context.Context, gameID uint64, signedChallenge []byte) error {
	send := capnp.Send{
		Method: capnp.Method{
			InterfaceID:   0xf3a510e30737976e,
			MethodID:      0,
			InterfaceName: "GameSessionService",
			MethodName:    "login",
		},
		ArgsSize: capnp.ObjectSize{DataSize: 8, PointerCount: 1},
		PlaceArgs: func(s capnp.Struct) error {
			s.SetUint64(0, gameID)
			return s.SetData(0, signedChallenge)
		},
	}

	ans, release := c.bootstrap.SendCall(ctx, send)
	defer release()

	result, err := ans.Struct()
	if err != nil {
		return fmt.Errorf("login failed: %w", err)
	}

	ptr, err := result.Ptr(0)
	if err != nil {
		return fmt.Errorf("login: failed to read session: %w", err)
	}

	c.userSession = ptr.Interface().Client().AddRef()
	return nil
}

func (c *AMPClient) RequestMatch(ctx context.Context, req MatchRequest) (*MatchAssignment, *MatchSession, error) {
	if !c.userSession.IsValid() {
		return nil, nil, fmt.Errorf("not logged in; call Login first")
	}

	send := capnp.Send{
		Method: capnp.Method{
			InterfaceID:   0xc563821241539e14,
			MethodID:      0,
			InterfaceName: "UserSession",
			MethodName:    "requestMatch",
		},
		ArgsSize: capnp.ObjectSize{PointerCount: 1},
		PlaceArgs: func(s capnp.Struct) error {
			msg, seg, err := capnp.NewMessage(capnp.SingleSegment(nil))
			if err != nil {
				return err
			}
			_ = msg
			_ = seg

			return s.SetData(0, req.GameID)
		},
	}

	ans, release := c.userSession.SendCall(ctx, send)
	defer release()

	result, err := ans.Struct()
	if err != nil {
		return nil, nil, fmt.Errorf("requestMatch failed: %w", err)
	}

	assignment := &MatchAssignment{}

	assignPtr, err := result.Ptr(0)
	if err == nil {
		assignStruct := assignPtr.Struct()
		matchIDPtr, err := assignStruct.Ptr(0)
		if err == nil {
			assignment.MatchID = matchIDPtr.Data()
		}
	}

	sessionClient := ans.Field(1, nil).Client()
	matchSession := &MatchSession{
		MatchID: assignment.MatchID,
		session: sessionClient.AddRef(),
		client:  c,
	}

	return assignment, matchSession, nil
}

func (c *AMPClient) SubmitOutcome(ctx context.Context, matchSession *MatchSession, outcome Outcome) (*VerifierResult, error) {
	if matchSession == nil || !matchSession.Session().IsValid() {
		return nil, fmt.Errorf("matchSession is nil or invalid")
	}

	send := capnp.Send{
		Method: capnp.Method{
			InterfaceID:   0x9501d5d4f819d0e2,
			MethodID:      0,
			InterfaceName: "MatchSession",
			MethodName:    "submitOutcome",
		},
		ArgsSize: capnp.ObjectSize{PointerCount: 1},
		PlaceArgs: func(s capnp.Struct) error {
			msg, seg, err := capnp.NewMessage(capnp.SingleSegment(nil))
			if err != nil {
				return err
			}
			_ = msg
			_ = seg
			_ = outcome
			return nil
		},
	}

	ans, release := matchSession.Session().SendCall(ctx, send)
	defer release()

	result, err := ans.Struct()
	if err != nil {
		return nil, fmt.Errorf("submitOutcome failed: %w", err)
	}

	sigPtr, err := result.Ptr(0)
	if err != nil {
		return nil, fmt.Errorf("failed to read verifier signature: %w", err)
	}

	sig := sigPtr.Data()
	verifierResult := &VerifierResult{
		Signature: make([]byte, len(sig)),
	}
	copy(verifierResult.Signature, sig)

	return verifierResult, nil
}

func (c *AMPClient) EmitTelemetry(ctx context.Context, matchSession *MatchSession, data []byte) error {
	if matchSession == nil || !matchSession.Session().IsValid() {
		return fmt.Errorf("matchSession is nil or invalid")
	}

	send := capnp.Send{
		Method: capnp.Method{
			InterfaceID:   0x9501d5d4f819d0e2,
			MethodID:      3,
			InterfaceName: "MatchSession",
			MethodName:    "emitTelemetry",
		},
		ArgsSize: capnp.ObjectSize{PointerCount: 1},
		PlaceArgs: func(s capnp.Struct) error {
			return s.SetData(0, data)
		},
	}

	ans, release := matchSession.Session().SendCall(ctx, send)
	defer release()

	_, err := ans.Struct()
	if err != nil {
		return fmt.Errorf("emitTelemetry failed: %w", err)
	}
	return nil
}

func (c *AMPClient) EmitGameEvent(ctx context.Context, matchSession *MatchSession, eventType string, data []byte) error {
	if matchSession == nil || !matchSession.Session().IsValid() {
		return fmt.Errorf("matchSession is nil or invalid")
	}

	send := capnp.Send{
		Method: capnp.Method{
			InterfaceID:   0x9501d5d4f819d0e2,
			MethodID:      2,
			InterfaceName: "MatchSession",
			MethodName:    "emitGameEvent",
		},
		ArgsSize: capnp.ObjectSize{PointerCount: 2},
		PlaceArgs: func(s capnp.Struct) error {
			if err := s.SetText(0, eventType); err != nil {
				return err
			}
			return s.SetData(1, data)
		},
	}

	ans, release := matchSession.Session().SendCall(ctx, send)
	defer release()

	_, err := ans.Struct()
	if err != nil {
		return fmt.Errorf("emitGameEvent failed: %w", err)
	}
	return nil
}

func (c *AMPClient) Reconnect(ctx context.Context, matchID []byte) (*MatchSession, error) {
	if !c.userSession.IsValid() {
		return nil, fmt.Errorf("not logged in; call Login first")
	}

	send := capnp.Send{
		Method: capnp.Method{
			InterfaceID:   0xc563821241539e14,
			MethodID:      1,
			InterfaceName: "UserSession",
			MethodName:    "reconnect",
		},
		ArgsSize: capnp.ObjectSize{PointerCount: 1},
		PlaceArgs: func(s capnp.Struct) error {
			return s.SetData(0, matchID)
		},
	}

	ans, release := c.userSession.SendCall(ctx, send)
	defer release()

	_, err := ans.Struct()
	if err != nil {
		return nil, fmt.Errorf("reconnect failed: %w", err)
	}

	sessionClient := ans.Client()

	return &MatchSession{
		MatchID: matchID,
		session: sessionClient.AddRef(),
		client:  c,
	}, nil
}

func (c *AMPClient) WaitForMatch(ctx context.Context, timeout time.Duration) (*MatchAssignment, *MatchSession, error) {
	if timeout > 0 {
		var cancel context.CancelFunc
		ctx, cancel = context.WithTimeout(ctx, timeout)
		defer cancel()
	}

	return c.RequestMatch(ctx, MatchRequest{})
}

func (c *AMPClient) Close() error {
	if c.userSession.IsValid() {
		c.userSession.Release()
	}
	c.bootstrap.Release()
	return c.rpcConn.Close()
}
