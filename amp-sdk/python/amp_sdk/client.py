"""AMP client SDK — async Python wrapper over the AMP Cap'n Proto RPC protocol.

This module is intentionally synchronous-on-import-light: pycapnp loads the
schemas at import time, so the cost is paid up front and individual RPC calls
are pure async I/O.
"""
from __future__ import annotations

import asyncio
import os
import ssl
from typing import Optional, Awaitable, Callable, Union

import capnp

# Load Cap'n Proto schemas at import time. ``generated/`` is a sibling
# directory inside the ``amp_sdk`` package — included in the wheel via
# pyproject.toml's force-include so that `pip install` works.
schema_dir = os.path.join(os.path.dirname(__file__), "generated")
service_schema = capnp.load(os.path.join(schema_dir, "service.capnp"))
profile_schema = capnp.load(os.path.join(schema_dir, "player_profile.capnp"))
registry_schema = capnp.load(os.path.join(schema_dir, "game_registry.capnp"))
match_schema = capnp.load(os.path.join(schema_dir, "match.capnp"))


# --------------------------------------------------------------------------- #
# Exceptions
# --------------------------------------------------------------------------- #
class AmpError(Exception):
    """Base class for all AMP SDK errors."""


class ConnectionError(AmpError):  # noqa: A001 - intentional shadow of builtin
    """Raised when the transport cannot be established."""


class AuthError(AmpError):
    """Raised when authentication or authorization fails."""


class MatchError(AmpError):
    """Raised for matchmaking or outcome-submission failures."""


class TimeoutError(AmpError):  # noqa: A001 - intentional shadow of builtin
    """Raised when an RPC exceeds its deadline."""


# Type alias for the user-supplied signing callback. May be sync or async.
SignCallback = Callable[[bytes], Union[bytes, Awaitable[bytes]]]


# --------------------------------------------------------------------------- #
# AMP Client
# --------------------------------------------------------------------------- #
class AMPClient:
    """Async client for the AMP matchmaker.

    Authentication requires either ``private_key_hex`` (custodial mode, used
    to sign both the login challenge and match outcomes) or a ``sign_callback``
    (wallet-integrated mode, requires a separate ``sign_digest_callback`` at
    ``submit_outcome`` time). The SDK never silently generates a throwaway
    identity — see SECURITY.md.
    """

    DEFAULT_CONNECT_TIMEOUT_S = 10.0
    DEFAULT_REQUEST_TIMEOUT_S = 5.0
    DEFAULT_MATCH_TIMEOUT_S = 60.0

    def __init__(
        self,
        rpc_url: str,
        *,
        tls_context: Optional[ssl.SSLContext] = None,
    ):
        """Construct an AMP client.

        :param rpc_url: ``host:port`` (IPv4) or ``[host]:port`` (IPv6 literal).
        :param tls_context: When non-None, the transport is wrapped in TLS.
            For typical use, pass ``ssl.create_default_context()``. Server
            certificate validation is on by default; supply an unpinned
            context (``ssl._create_unverified_context``) only for local
            development. The AMP server uses rustls; specify ``ServerName``
            via the hostname portion of ``rpc_url``.
        """
        if not rpc_url or ":" not in rpc_url:
            raise ValueError(f"rpc_url must be host:port, got {rpc_url!r}")
        self.rpc_url = rpc_url
        self._tls_context = tls_context
        self._conn = None
        self._bootstrap = None
        self._user_session = None
        self._match_session = None
        # The 20-byte Ethereum address recovered from the login signature.
        # Populated by ``authenticate`` and used by the server to bind
        # ``submit_outcome`` signatures to a participant.
        self.player_address: Optional[bytes] = None
        # Stored custodial key (only when private_key_hex was supplied); used
        # to sign outcomes without an explicit callback.
        self._custodial_key_hex: Optional[str] = None

    # ------------------------------------------------------------------ #
    # Connection
    # ------------------------------------------------------------------ #
    async def connect(self) -> bool:
        """Open a TCP connection to the AMP server.

        Returns ``True`` on success. Raises :class:`ConnectionError` or
        :class:`TimeoutError` on failure. The caller may pass a host:port
        string with an IPv4 or IPv6 literal (IPv6 must be bracketed).
        """
        host, port = _split_host_port(self.rpc_url)
        try:
            stream = await asyncio.wait_for(
                capnp.AsyncIoStream.create_connection(
                    host=host, port=port, ssl=self._tls_context
                ),
                timeout=self.DEFAULT_CONNECT_TIMEOUT_S,
            )
        except asyncio.TimeoutError as e:
            raise TimeoutError(f"Connection to {self.rpc_url} timed out") from e
        except OSError as e:
            raise ConnectionError(f"Failed to connect to {self.rpc_url}: {e}") from e

        self._conn = capnp.TwoPartyClient(stream)
        self._bootstrap = self._conn.bootstrap().cast_as(service_schema.GameSessionService)
        return True

    async def close(self) -> None:
        """Release the underlying transport. Idempotent."""
        # pycapnp's TwoPartyClient closes the transport on GC; we drop our
        # references here so the socket is returned promptly.
        self._match_session = None
        self._user_session = None
        self._bootstrap = None
        self._conn = None

    async def __aenter__(self) -> "AMPClient":
        await self.connect()
        return self

    async def __aexit__(self, exc_type, exc, tb) -> None:
        await self.close()

    # ------------------------------------------------------------------ #
    # Authentication
    # ------------------------------------------------------------------ #
    async def authenticate(
        self,
        game_id: int,
        private_key_hex: Optional[str] = None,
        sign_callback: Optional[SignCallback] = None,
    ) -> bool:
        """Run the EIP-191 challenge/response flow.

        Either ``private_key_hex`` (custodial) or ``sign_callback`` (wallet)
        is **required**. The SDK will not generate an ephemeral identity.
        """
        if self._bootstrap is None:
            raise AuthError("Not connected, call connect() first")
        if not private_key_hex and not sign_callback:
            raise ValueError(
                "authenticate requires either private_key_hex (custodial) or "
                "sign_callback (wallet). AMP no longer generates silent "
                "ephemeral identities — see SECURITY.md."
            )

        try:
            challenge_bytes, expires_at_ns = await self._request_challenge(game_id)
        except Exception as e:
            raise AuthError(f"Failed to request challenge: {e}") from e

        # Client-side freshness check. Server also enforces this; failing
        # early avoids burning the one-time challenge on a stale attempt.
        if is_challenge_expired(expires_at_ns):
            raise AuthError(
                f"Received challenge already expired (expiresAt={expires_at_ns})"
            )

        if sign_callback is not None:
            sig_bytes = await _maybe_await(sign_callback(challenge_bytes))
            if not isinstance(sig_bytes, (bytes, bytearray)) or len(sig_bytes) != 65:
                raise AuthError(
                    f"sign_callback must return 65 bytes; got {type(sig_bytes).__name__} "
                    f"of len {len(sig_bytes) if sig_bytes else 0}"
                )
        else:
            sig_bytes = _sign_eip191_challenge(private_key_hex, challenge_bytes)
            self._custodial_key_hex = private_key_hex
            try:
                self.player_address = _derive_address_from_key(private_key_hex)
            except Exception:
                # Non-fatal: the server will recover the address itself.
                self.player_address = None

        try:
            result = await self._login(game_id, sig_bytes, challenge_bytes)
        except Exception as e:
            raise AuthError(f"Authentication failed: {e}") from e

        self._user_session = result.session
        return True

    async def _request_challenge(self, game_id: int) -> tuple[bytes, int]:
        req = self._bootstrap.requestChallenge_request()
        req.gameId = game_id
        result = await asyncio.wait_for(
            req.send(),
            timeout=self.DEFAULT_REQUEST_TIMEOUT_S,
        )
        return bytes(result.challenge), int(result.expiresAt)

    async def _login(self, game_id: int, sig: bytes, challenge: bytes):
        req = self._bootstrap.login_request()
        req.gameId = game_id
        req.signature = sig
        req.challengePayload = challenge
        return await asyncio.wait_for(
            req.send(),
            timeout=self.DEFAULT_REQUEST_TIMEOUT_S,
        )

    # ------------------------------------------------------------------ #
    # Profile (stub)
    # ------------------------------------------------------------------ #
    async def create_profile(self, display_name: str, region: str, role: str) -> bool:
        """Create or update the player profile.

        NOTE: The AMP server does not expose ``createOrUpdateProfile`` on the
        authenticated RPC surface today. This method exists for forward
        compatibility but currently issues an empty request — do not rely on
        it to persist state.
        """
        if self._user_session is None:
            raise AuthError("Not authenticated, call connect() first")
        try:
            req = self._user_session.createOrUpdateProfile_request()
            await req.send()
            return True
        except Exception as e:
            raise AmpError(f"Failed to create profile: {e}") from e

    # ------------------------------------------------------------------ #
    # Matchmaking
    # ------------------------------------------------------------------ #
    async def request_match(
        self,
        game_id: str,
        ruleset_id: str,
        player_id: Optional[str] = None,
    ) -> dict:
        """Queue for a match. Returns ``{"match_id": str, "quality": float}``.

        ``player_id`` defaults to the authenticated Ethereum address. The
        server identifies the caller by the recovered login address, not by
        this field — but we populate it for symmetry.
        """
        if self._user_session is None:
            raise AuthError("Not authenticated, call connect() first")
        if not game_id:
            raise ValueError("game_id is required")
        if not ruleset_id:
            raise ValueError("ruleset_id is required")

        player_id = player_id or (
            "0x" + self.player_address.hex() if self.player_address else "anon"
        )

        req = self._user_session.requestMatch_request()
        # Wire the fields correctly: gameId is the game identifier,
        # ruleSetId is the ruleset identifier. Previous versions conflated
        # the two and the player_id was hardcoded to "p1".
        req.req.gameId = game_id.encode("utf-8")
        req.req.ruleSetId = ruleset_id.encode("utf-8")
        req.req.rulesType = "standard"
        req.req.playerInfo.playerId = player_id.encode("utf-8")
        req.req.playerInfo.displayName = player_id.encode("utf-8")
        if self.player_address:
            req.req.playerInfo.playerWallet = self.player_address

        try:
            result = await asyncio.wait_for(
                req.send(),
                timeout=self.DEFAULT_MATCH_TIMEOUT_S,
            )
        except asyncio.TimeoutError as e:
            raise TimeoutError("Match request timed out") from e
        except Exception as e:
            raise MatchError(f"Match request failed: {e}") from e

        assignment = result.assignment
        self._match_session = result.session
        return {
            "match_id": bytes(assignment.matchId).decode("utf-8", errors="replace"),
            "quality": float(assignment.matchQuality),
        }

    # ------------------------------------------------------------------ #
    # Outcome submission
    # ------------------------------------------------------------------ #
    async def submit_outcome(
        self,
        match_id: str,
        outcome: int,
        transcript_hash: bytes,
        sign_digest_callback: Optional[SignCallback] = None,
    ) -> bytes:
        """Submit the final match outcome.

        The server verifies the submitter signature against the participant
        address recovered at login. Either:

        * supply ``private_key_hex`` to :meth:`authenticate` (custodial mode)
          — this client will sign automatically; or
        * supply ``sign_digest_callback`` here (wallet mode) — the callback
          receives the 32-byte EIP-712 digest over (matchId, outcome,
          transcriptHash) and must return a 65-byte signature from the SAME
          wallet used at login.
        """
        if self._match_session is None:
            raise MatchError("No active match session")
        if outcome < 1 or outcome > 4:
            raise ValueError("outcome must be 1..=4 (server and relayer invariant)")
        if not isinstance(transcript_hash, (bytes, bytearray)) or len(transcript_hash) != 32:
            raise ValueError(
                f"transcript_hash must be exactly 32 bytes; got {len(transcript_hash)}"
            )

        digest = _compute_outcome_eip712_digest(match_id, outcome, bytes(transcript_hash))
        if self._custodial_key_hex:
            from eth_keys import keys  # imported lazily to keep import cost low
            pk = keys.PrivateKey(bytes.fromhex(self._custodial_key_hex.replace("0x", "")))
            sig = pk.sign_msg_hash(digest)  # r||s||v with v in {0,1}
            submitter_sig = sig.to_bytes()  # 65 bytes; convert v to 27/28 below
            # eth_keys produces v in {0,1}; ethers expects 27/28.
            submitter_sig = submitter_sig[:64] + bytes([submitter_sig[64] + 27])
        elif sign_digest_callback is not None:
            submitter_sig = await _maybe_await(sign_digest_callback(digest))
            if not isinstance(submitter_sig, (bytes, bytearray)) or len(submitter_sig) != 65:
                raise MatchError(
                    f"sign_digest_callback must return 65 bytes; got "
                    f"{type(submitter_sig).__name__} of len "
                    f"{len(submitter_sig) if submitter_sig else 0}"
                )
            submitter_sig = bytes(submitter_sig)
        else:
            raise MatchError(
                "Outcome signing requires either a custodial key (passed to "
                "authenticate) or a sign_digest_callback (passed to submit_outcome)."
            )

        try:
            req = self._match_session.submitOutcome_request()
            req.submission.matchId = match_id.encode("utf-8")
            req.submission.outcome.victor = outcome
            req.submission.outcome.type = _outcome_type_for(outcome)
            req.submission.replayHash = bytes(transcript_hash)
            req.submission.signature = submitter_sig
            result = await req.send()
        except Exception as e:
            raise MatchError(f"Failed to submit outcome: {e}") from e

        return bytes(result.signature)


# --------------------------------------------------------------------------- #
# Helpers
# --------------------------------------------------------------------------- #
def is_challenge_expired(expires_at_ns: int) -> bool:
    """Return True if the given challenge expiry (nanoseconds since epoch)
    is in the past. Use this before signing a challenge to avoid burning
    a one-time nonce on a stale attempt.
    """
    import time as _time
    return expires_at_ns <= _time.time_ns()


def _split_host_port(rpc_url: str) -> tuple[str, int]:
    """Parse host:port, handling bracketed IPv6 literals."""
    default_port = 50051
    if rpc_url.startswith("["):
        end = rpc_url.find("]")
        if end < 0 or end + 1 >= len(rpc_url) or rpc_url[end + 1] != ":":
            raise ValueError(f"Malformed IPv6 rpc_url: {rpc_url!r}")
        host = rpc_url[1:end]
        port = int(rpc_url[end + 2 :])
        return host, port
    if ":" not in rpc_url:
        return rpc_url, default_port
    host, _, port = rpc_url.rpartition(":")
    return host, int(port)


async def _maybe_await(value):
    return await value if asyncio.iscoroutine(value) or asyncio.isfuture(value) else value


def _sign_eip191_challenge(private_key_hex: str, challenge: bytes) -> bytes:
    """Sign the EIP-191 personal_sign prefix over challenge bytes."""
    from eth_account import Account
    from eth_account.messages import encode_defunct

    key = private_key_hex if private_key_hex.startswith("0x") else "0x" + private_key_hex
    # Validate hex by attempting the operation; ValueError propagates as a
    # clear error message from the caller.
    try:
        msg = encode_defunct(primitive=challenge)
        signed = Account.sign_message(msg, private_key=key)
        return bytes(signed.signature)
    except ValueError as e:
        raise AuthError(f"Invalid private_key_hex: {e}") from e


def _derive_address_from_key(private_key_hex: str) -> bytes:
    """Return the 20-byte Ethereum address derived from a private key."""
    from eth_keys import keys

    raw = private_key_hex.replace("0x", "")
    pk = keys.PrivateKey(bytes.fromhex(raw))
    return pk.public_key.to_canonical_address()  # 20 bytes


def _outcome_type_for(outcome: int) -> int:
    """Map victor index to OutcomeType enum value."""
    # See amp-sdk/schemas/match.capnp: enum OutcomeType { unknown@0, win@1, draw@2, void@3 }
    if outcome == 1:
        return match_schema.OutcomeType.win
    if outcome == 2:
        return match_schema.OutcomeType.draw
    if outcome in (3, 4):
        return match_schema.OutcomeType.void
    return match_schema.OutcomeType.unknown


def _compute_outcome_eip712_digest(match_id: str, outcome: int, transcript_hash: bytes) -> bytes:
    """Compute the canonical EIP-712 digest over (matchId, outcome, transcriptHash).

    Mirrors ``compute_outcome_eip712_digest`` in amp-server/src/main.rs.
    The digest MUST be byte-identical between client and server or signature
    verification fails.
    """
    from eth_utils import keccak

    if not isinstance(transcript_hash, (bytes, bytearray)) or len(transcript_hash) != 32:
        raise ValueError(
            f"transcript_hash must be exactly 32 bytes, got {len(transcript_hash) if transcript_hash else 'None'}"
        )
    if outcome < 1 or outcome > 4:
        raise ValueError(f"outcome must be 1..=4 (server invariant), got {outcome}")

    # matchId → uint256 (parse as decimal, else keccak of the UTF-8 bytes)
    try:
        match_id_int = int(match_id)
        match_id_enc = match_id_int.to_bytes(32, "big", signed=False)
    except ValueError:
        match_id_enc = keccak(match_id.encode("utf-8"))

    outcome_enc = outcome.to_bytes(32, "big", signed=False)

    async_result_typehash = keccak(
        b"AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)"
    )
    struct_hash = keccak(
        async_result_typehash + match_id_enc + outcome_enc + bytes(transcript_hash)
    )

    # Domain separator — chainId and verifyingContract must match the on-chain
    # deployment. Defaults mirror the server's defaults; override at the
    # application level if needed by editing these constants.
    chain_id = _EIP712_CHAIN_ID
    verifying_contract = _EIP712_VERIFYING_CONTRACT
    if len(verifying_contract) != 20:
        raise ValueError(
            "OutcomeEip712 verifying contract must be set to a 20-byte address"
        )

    domain_typehash = keccak(
        b"EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)"
    )
    name_hash = keccak(b"AMPSettlement")
    version_hash = keccak(b"1")
    domain_separator = keccak(
        domain_typehash
        + name_hash
        + version_hash
        + chain_id.to_bytes(32, "big", signed=False)
        + b"\x00" * 12
        + bytes(verifying_contract)
    )

    return keccak(b"\x19\x01" + domain_separator + struct_hash)


# Defaults — override at application startup if your deployment differs.
_EIP712_CHAIN_ID: int = 43113  # Fuji testnet
_EIP712_VERIFYING_CONTRACT: bytes = b"\x00" * 20


def set_eip712_domain(chain_id: int, verifying_contract: bytes) -> None:
    """Override the EIP-712 domain parameters used for outcome signing.

    Call this once at startup with your settlement contract's chain ID and
    20-byte address; both must match the on-chain contract.
    """
    global _EIP712_CHAIN_ID, _EIP712_VERIFYING_CONTRACT
    if not isinstance(chain_id, int) or chain_id <= 0:
        raise ValueError("chain_id must be a positive integer")
    if not isinstance(verifying_contract, (bytes, bytearray)) or len(verifying_contract) != 20:
        raise ValueError("verifying_contract must be exactly 20 bytes")
    _EIP712_CHAIN_ID = chain_id
    _EIP712_VERIFYING_CONTRACT = bytes(verifying_contract)
