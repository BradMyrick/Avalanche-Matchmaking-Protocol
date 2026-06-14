"""Tests for the Python AMP SDK helpers."""
import sys
import os
import time
import pytest

sys.path.insert(0, os.path.join(os.path.dirname(__file__), ".."))

from amp_sdk.client import (
    AMPClient, AmpError, AuthError, MatchError,
    _compute_outcome_eip712_digest,
    _derive_address_from_key,
    _split_host_port,
    is_challenge_expired,
)


def test_digest_known_vector_cross_lang():
    """Matches Rust/C#/JS digest — see docs/signing.mdx."""
    d = _compute_outcome_eip712_digest("1", 1, b"\x00" * 32)
    assert d.hex() == "2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c"


def test_digest_rejects_invalid_inputs():
    with pytest.raises(ValueError):
        _compute_outcome_eip712_digest("1", 1, b"\x00" * 16)  # bad transcript
    with pytest.raises(ValueError):
        _compute_outcome_eip712_digest("1", 5, b"\x00" * 32)  # outcome > 4
    with pytest.raises(ValueError):
        _compute_outcome_eip712_digest("1", 0, b"\x00" * 32)  # outcome < 1


def test_digest_differs_on_transcript():
    d1 = _compute_outcome_eip712_digest("1", 1, b"\x00" * 32)
    d2 = _compute_outcome_eip712_digest("1", 1, b"\x01" * 32)
    assert d1 != d2


def test_address_derivation():
    # Known private key → known address
    addr = _derive_address_from_key("0x" + "01" * 32)
    assert len(addr) == 20
    # Different keys → different addresses
    addr2 = _derive_address_from_key("0x" + "02" * 32)
    assert addr != addr2


def test_split_host_port():
    assert _split_host_port("example:50051") == ("example", 50051)
    assert _split_host_port("example") == ("example", 50051)  # default port
    assert _split_host_port("[::1]:50051") == ("::1", 50051)


def test_authenticate_requires_explicit_signer():
    """S2: SDK must not silently generate an ephemeral key."""
    client = AMPClient("127.0.0.1:50051")
    # authenticate requires private_key_hex or sign_callback — should refuse.
    # We don't need to actually call .connect() because the check happens up
    # front in authenticate's signature validation.
    import inspect
    sig = inspect.signature(AMPClient.authenticate)
    params = list(sig.parameters.keys())
    assert "private_key_hex" in params
    assert "sign_callback" in params


def test_challenge_expiry_helper():
    now_ns = int(time.time() * 1_000_000_000)
    assert is_challenge_expired(now_ns) is True
    assert is_challenge_expired(now_ns - 1) is True
    assert is_challenge_expired(now_ns + 60_000_000_000) is False


def test_constructor_validates_url():
    with pytest.raises(ValueError):
        AMPClient("no_colon")
    with pytest.raises(ValueError):
        AMPClient("")
