import capnp
import asyncio
import os

schema_dir = os.path.join(os.path.dirname(__file__), '..', 'generated')
service_schema = capnp.load(os.path.join(schema_dir, 'service.capnp'))
profile_schema = capnp.load(os.path.join(schema_dir, 'player_profile.capnp'))
registry_schema = capnp.load(os.path.join(schema_dir, 'game_registry.capnp'))
match_schema = capnp.load(os.path.join(schema_dir, 'match.capnp'))


class AmpError(Exception):
    pass


class ConnectionError(AmpError):
    pass


class AuthError(AmpError):
    pass


class MatchError(AmpError):
    pass


class TimeoutError(AmpError):
    pass


class AMPClient:
    def __init__(self, rpc_url: str):
        self.rpc_url = rpc_url
        self._conn = None
        self._user_session = None
        self._match_session = None

    async def connect(self, player_id: str, game_id: int) -> bool:
        try:
            host, port = self.rpc_url.split(':')
            reader, writer = await asyncio.wait_for(
                asyncio.open_connection(host, int(port)),
                timeout=10.0,
            )
        except asyncio.TimeoutError:
            raise TimeoutError(f"Connection to {self.rpc_url} timed out")
        except OSError as e:
            raise ConnectionError(f"Failed to connect to {self.rpc_url}: {e}")

        self._conn = capnp.TwoPartyClient(reader, writer)
        self._bootstrap = self._conn.bootstrap().cast_as(
            service_schema.GameSessionService
        )

        req = self._bootstrap.login_request()
        req.gameId = game_id
        req.signedChallenge = player_id.encode('utf-8')

        try:
            result = await req.send()
        except Exception as e:
            raise AuthError(f"Authentication failed: {e}")

        self._user_session = result.session
        return True

    async def create_profile(self, display_name: str, region: str, role: str) -> bool:
        if not self._user_session:
            raise AuthError("Not authenticated, call connect() first")

        try:
            req = self._user_session.createOrUpdateProfile_request()
            await req.send()
            return True
        except Exception as e:
            raise AmpError(f"Failed to create profile: {e}")

    async def request_match(self, ruleset_id: str) -> dict:
        if not self._user_session:
            raise AuthError("Not authenticated, call connect() first")

        try:
            req = self._user_session.requestMatch_request()
            req.req.gameId = ruleset_id.encode('utf-8')
            req.req.rulesType = "standard"
            req.req.playerInfo.playerId = b"p1"
            req.req.playerInfo.displayName = "PythonPlayer"
            result = await req.send()
        except asyncio.TimeoutError:
            raise TimeoutError("Match request timed out")
        except Exception as e:
            raise MatchError(f"Match request failed: {e}")

        assignment = result.assignment
        self._match_session = result.session
        return {
            "match_id": assignment.matchId.decode('utf-8'),
            "quality": assignment.matchQuality,
        }

    async def submit_outcome(self, match_id: str, outcome: int, transcript_hash: bytes) -> bytes:
        if not self._match_session:
            raise MatchError("No active match session")

        try:
            req = self._match_session.submitOutcome_request()
            req.submission.matchId = match_id.encode('utf-8')
            req.submission.outcome.victor = outcome
            req.submission.replayHash = transcript_hash
            result = await req.send()
        except Exception as e:
            raise MatchError(f"Failed to submit outcome: {e}")

        return result.signature
