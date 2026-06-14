// Package client provides a Cap'n Proto RPC client for the AMP matchmaker.
//
// Concurrency: an *AMPClient is safe for concurrent use after construction.
// All mutations of the userSession handle are guarded by an internal mutex.
package client

import (
	"context"
	"crypto/tls"
	"errors"
	"fmt"
	"net"
	"sync"
	"time"

	"capnproto.org/go/capnp/v3"
	"capnproto.org/go/capnp/v3/rpc"
	"github.com/BradMyrick/Avalanche-Matchmaking-Protocol/amp-sdk/go/generated"
)

// DefaultDialTimeout bounds the initial TCP handshake.
const DefaultDialTimeout = 10 * time.Second

// Options controls AMPClient construction. Pass a non-nil TLSConfig to wrap
// the underlying TCP connection in TLS. The zero value is plaintext TCP
// (matching historical behavior); pass a TLSConfig to enable encryption,
// which is strongly recommended for any deployment handling real signatures.
type Options struct {
	// TLS, when non-nil, enables TLS on the transport. MinVersion is forced
	// to TLS 1.2 if unset; ServerName should typically be set to the host
	// portion of Addr so SNI/cert validation works.
	TLS *tls.Config
	// DialTimeout bounds the TCP/TLS handshake. Defaults to 10s.
	DialTimeout time.Duration
}

// AMPClient is a Cap'n Proto RPC client for the AMP matchmaker.
// Safe for concurrent use after construction.
type AMPClient struct {
	rpcConn *rpc.Conn
	service generated.GameSessionService

	mu          sync.RWMutex
	userSession generated.UserSession
	closed      bool
}

// NewClient dials the AMP server and returns a ready client.
//
// Address format: "host:port" (e.g. "matchmaker.amp.example:50051").
// The function honors Options.DialTimeout (default 10s); context cancellation
// during the dial is also respected.
func NewClient(ctx context.Context, addr string, opts Options) (*AMPClient, error) {
	dialTimeout := opts.DialTimeout
	if dialTimeout == 0 {
		dialTimeout = DefaultDialTimeout
	}

	dialer := &net.Dialer{Timeout: dialTimeout}
	var rawConn net.Conn
	var err error
	if opts.TLS != nil {
		tlsConfig := opts.TLS.Clone()
		if tlsConfig.MinVersion == 0 {
			tlsConfig.MinVersion = tls.VersionTLS12
		}
		if tlsConfig.ServerName == "" {
			host, _, _ := net.SplitHostPort(addr)
			tlsConfig.ServerName = host
		}
		rawConn, err = tls.DialWithDialer(dialer, "tcp", addr, tlsConfig)
	} else {
		rawConn, err = dialer.DialContext(ctx, "tcp", addr)
	}
	if err != nil {
		return nil, fmt.Errorf("amp dial %s: %w", addr, err)
	}

	rpcConn := rpc.NewConn(rpc.NewStreamTransport(rawConn), nil)
	bootstrap := rpcConn.Bootstrap(ctx)

	return &AMPClient{
		rpcConn: rpcConn,
		service: generated.GameSessionService(bootstrap),
	}, nil
}

// ErrClosed is returned by Close after the first call.
var ErrClosed = errors.New("amp: client already closed")

// Login runs the EIP-191 challenge/response flow with a pre-signed challenge.
// The caller is responsible for signing the bytes returned by RequestChallenge
// with their Ethereum wallet.
func (c *AMPClient) Login(ctx context.Context, gameID uint64, signature, challengePayload []byte) error {
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

	session := result.Session()
	// AddRef before the answer is released (the defer above) so the handle
	// outlives the capnp answer message. Matches the pattern in RequestMatch.
	session = session.AddRef()

	c.mu.Lock()
	c.userSession.Release() // release any prior session
	c.userSession = session
	c.mu.Unlock()
	return nil
}

// RequestChallenge returns the EIP-191 challenge bytes and the absolute
// expiry time in nanoseconds since epoch. Callers SHOULD validate that
// expiresAt > now before signing — see CheckChallengeExpiry.
func (c *AMPClient) RequestChallenge(ctx context.Context, gameID uint64) (challenge []byte, expiresAt uint64, err error) {
	future, release := c.service.RequestChallenge(ctx, func(p generated.GameSessionService_requestChallenge_Params) error {
		p.SetGameId(gameID)
		return nil
	})
	defer release()

	result, err := future.Struct()
	if err != nil {
		return nil, 0, fmt.Errorf("requestChallenge failed: %w", err)
	}

	challenge, err = result.Challenge()
	if err != nil {
		return nil, 0, fmt.Errorf("failed to read challenge: %w", err)
	}

	expiresAt = result.ExpiresAt()
	challengeCopy := make([]byte, len(challenge))
	copy(challengeCopy, challenge)

	return challengeCopy, expiresAt, nil
}

// CheckChallengeExpiry returns an error if the given expiresAt (nanoseconds
// since epoch) is in the past. Helper for client-side freshness checks.
func CheckChallengeExpiry(expiresAt uint64) error {
	now := uint64(time.Now().UnixNano())
	if expiresAt <= now {
		return fmt.Errorf("challenge already expired (expiresAt=%d, now=%d)", expiresAt, now)
	}
	return nil
}

// RequestMatch queues for a match. May block until matched; supply a context
// with a deadline. The returned MatchSession.MatchID is a defensive copy and
// is safe to retain after Close.
func (c *AMPClient) RequestMatch(ctx context.Context, req generated.GameMatchRequest) (*MatchSession, error) {
	c.mu.RLock()
	session := c.userSession
	c.mu.RUnlock()
	if !session.IsValid() {
		return nil, fmt.Errorf("not logged in; call Login first")
	}

	future, release := session.RequestMatch(ctx, func(p generated.UserSession_requestMatch_Params) error {
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

	matchIDRaw, err := assignment.MatchId()
	if err != nil {
		return nil, fmt.Errorf("failed to read matchId: %w", err)
	}
	// Defensive copy: the slice returned by capnp aliases the answer buffer,
	// which is freed by the release() above. Without this copy, future
	// capnp allocations could overwrite the slice.
	matchID := make([]byte, len(matchIDRaw))
	copy(matchID, matchIDRaw)

	matchSession := future.Session().AddRef()

	return &MatchSession{
		MatchID: matchID,
		Session: matchSession,
		client:  c,
	}, nil
}

// Reconnect reclaims a capability for an existing active match.
func (c *AMPClient) Reconnect(ctx context.Context, matchID []byte) (*MatchSession, error) {
	c.mu.RLock()
	session := c.userSession
	c.mu.RUnlock()
	if !session.IsValid() {
		return nil, fmt.Errorf("not logged in; call Login first")
	}

	future, release := session.Reconnect(ctx, func(p generated.UserSession_reconnect_Params) error {
		return p.SetMatchId(matchID)
	})
	defer release()

	_, err := future.Struct()
	if err != nil {
		return nil, fmt.Errorf("reconnect failed: %w", err)
	}

	sessionClient := future.Session().AddRef()
	matchIDCopy := make([]byte, len(matchID))
	copy(matchIDCopy, matchID)

	return &MatchSession{
		MatchID: matchIDCopy,
		Session: sessionClient,
		client:  c,
	}, nil
}

// SubmitOutcome sends the signed outcome submission and returns the verifier's signature.
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

// EmitTelemetry emits an AmpTelemetryEvent on the given match session.
func (c *AMPClient) EmitTelemetry(ctx context.Context, matchSession *MatchSession, event generated.AmpTelemetryEvent) error {
	if matchSession == nil || !matchSession.Session.IsValid() {
		return fmt.Errorf("matchSession is nil or invalid")
	}

	future, release := matchSession.Session.EmitTelemetry(ctx, func(p generated.MatchSession_emitTelemetry_Params) error {
		return p.SetEvent(event)
	})
	defer release()

	if _, err := future.Struct(); err != nil {
		return fmt.Errorf("emitTelemetry failed: %w", err)
	}
	return nil
}

// EmitGameEvent emits a GameEvent on the given match session.
func (c *AMPClient) EmitGameEvent(ctx context.Context, matchSession *MatchSession, event generated.GameEvent) error {
	if matchSession == nil || !matchSession.Session.IsValid() {
		return fmt.Errorf("matchSession is nil or invalid")
	}

	future, release := matchSession.Session.EmitGameEvent(ctx, func(p generated.MatchSession_emitGameEvent_Params) error {
		return p.SetEvent(event)
	})
	defer release()

	if _, err := future.Struct(); err != nil {
		return fmt.Errorf("emitGameEvent failed: %w", err)
	}
	return nil
}

// Close releases all capabilities and the underlying transport. Idempotent —
// subsequent calls return ErrClosed. Safe for concurrent use with in-flight
// RPCs (the rpc layer drains on close).
func (c *AMPClient) Close() error {
	c.mu.Lock()
	if c.closed {
		c.mu.Unlock()
		return ErrClosed
	}
	c.closed = true
	session := c.userSession
	c.userSession = generated.UserSession{}
	c.mu.Unlock()

	if session.IsValid() {
		session.Release()
	}
	// service is always non-null after NewClient; safe to release once.
	c.service.Release()
	return c.rpcConn.Close()
}

// Compile-time assertion that capnp.Client is imported for capability.AddRef.
var _ capnp.Client
