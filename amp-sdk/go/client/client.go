package client

import (
	"context"
	"net"

	"capnproto.org/go/capnp/v3/rpc"
	"github.com/avalanche-matchmaking-protocol/amp-sdk/go/generated"
)

// AMPClient provides a high-level wrapper around the AMP Cap'n Proto RPC system.
type AMPClient struct {
	rpcConn *rpc.Conn
	session generated.GameSessionService
}

// NewClient establishes a TCP connection to the AMP server and returns an AMPClient.
func NewClient(ctx context.Context, addr string) (*AMPClient, error) {
	conn, err := net.Dial("tcp", addr)
	if err != nil {
		return nil, err
	}

	rpcConn := rpc.NewConn(rpc.NewStreamTransport(conn))
	session := generated.GameSessionService(rpcConn.Bootstrap(ctx))

	return &AMPClient{
		rpcConn: rpcConn,
		session: session,
	}, nil
}

// Login authenticates a user for a specific game and returns a UserSession capability.
func (c *AMPClient) Login(ctx context.Context, gameID uint64, signedChallenge []byte) (generated.UserSession, error) {
	req, err := c.session.Login(ctx, func(p generated.GameSessionService_login_Params) error {
		p.SetGameId(gameID)
		p.SetSignedChallenge(signedChallenge)
		return nil
	})
	if err != nil {
		return generated.UserSession{}, err
	}
	
	res, err := req.Struct()
	if err != nil {
		return generated.UserSession{}, err
	}
	return res.Session().AddRef(), nil
}

// Close gracefully closes the RPC connection.
func (c *AMPClient) Close() error {
	return c.rpcConn.Close()
}
