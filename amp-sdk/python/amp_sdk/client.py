import capnp
import asyncio
import os

# Load Cap'n Proto schemas
schema_dir = os.path.join(os.path.dirname(__file__), '../generated')
service_schema = capnp.load(os.path.join(schema_dir, 'service.capnp'))
profile_schema = capnp.load(os.path.join(schema_dir, 'player_profile.capnp'))
registry_schema = capnp.load(os.path.join(schema_dir, 'game_registry.capnp'))
match_schema = capnp.load(os.path.join(schema_dir, 'match.capnp'))

class AMPClient:
    def __init__(self, rpc_url: str):
        self.rpc_url = rpc_url
        self._conn = None
        self._user_session = None

    async def connect(self, player_id: str, game_id: int):
        host, port = self.rpc_url.split(':')
        reader, writer = await asyncio.open_connection(host, int(port))
        
        self._conn = capnp.TwoPartyClient(reader, writer)
        self._bootstrap = self._conn.bootstrap().cast_as(service_schema.GameSessionService)
        
        # Call login to get the UserSession capability
        req = self._bootstrap.login_request()
        req.gameId = game_id
        req.signedChallenge = player_id.encode('utf-8') # Using player_id as signature for demo purposes
        result = await req.send()
        self._user_session = result.session
        return True

    async def create_profile(self, display_name: str, region: str, role: str):
        # NOTE: Assumes ExtendedPlayerService is part of ProtocolRegistryService in production
        # For SDK completeness, we'd hit PlayerProfileService. 
        # For now, this is a placeholder interface definition mapping to real capnp types
        req = self._user_session.createOrUpdateProfile_request()
        # capnp populations
        return True

    async def request_match(self, ruleset_id: str):
        if not self._user_session:
            raise Exception("Not connected")
            
        req = self._user_session.requestMatch_request()
        req.req.gameId = ruleset_id.encode('utf-8') # Using it as gameId for now as placeholder
        req.req.rulesType = "standard"
        req.req.playerInfo.playerId = b"p1"
        req.req.playerInfo.displayName = "PythonPlayer"
        result = await req.send()
        
        # The result contains a MatchAssignment and a MatchSession
        assignment = result.assignment
        self._match_session = result.session
        return {
            "match_id": assignment.matchId.decode('utf-8'),
            "quality": assignment.matchQuality
        }

    async def submit_outcome(self, match_id: str, outcome: int, transcript_hash: bytes):
        if not hasattr(self, '_match_session'):
            raise Exception("No active match session")
            
        req = self._match_session.submitOutcome_request()
        req.submission.matchId = match_id.encode('utf-8')
        req.submission.outcome.victor = outcome
        req.submission.replayHash = transcript_hash
        
        result = await req.send()
        return result.signature
