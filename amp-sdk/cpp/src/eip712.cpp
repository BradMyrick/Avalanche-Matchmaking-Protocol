#include "amp/eip712.hpp"
#include "amp/keccak256.hpp"

#include <cstring>
#include <stdexcept>

namespace amp {

namespace {

// ABI-encode the 32-byte big-endian representation of a uint256 parsed from
// a decimal string. Throws std::invalid_argument if the string is not a
// valid non-negative integer.
std::vector<uint8_t> encode_uint256_from_decimal(const std::string& decimal) {
    if (decimal.empty()) {
        throw std::invalid_argument("match_id decimal string is empty");
    }
    // Parse decimal -> 256-bit big-endian via long division. This avoids
    // depending on a BigInteger library; we only need uint256.
    uint8_t bytes[32] = {0}; // big-endian accumulator.
    for (char c : decimal) {
        if (c < '0' || c > '9') {
            throw std::invalid_argument("match_id contains non-decimal char");
        }
        unsigned digit = c - '0';

        // bytes = bytes * 10 + digit, with carry propagation.
        uint32_t carry = digit;
        for (int i = 31; i >= 0; --i) {
            uint32_t v = static_cast<uint32_t>(bytes[i]) * 10u + carry;
            bytes[i] = static_cast<uint8_t>(v & 0xff);
            carry = v >> 8;
        }
        if (carry != 0) {
            throw std::invalid_argument("match_id overflows uint256");
        }
    }
    return std::vector<uint8_t>(bytes, bytes + 32);
}

// If match_id is a hex string of 64 chars (32 bytes), use that directly as
// the bytes32 representation (the AMP server's behavior when the string
// isn't a decimal number — see amp-server/src/main.rs:545).
bool try_parse_hex_bytes32(const std::string& s, std::vector<uint8_t>& out) {
    std::string hex = s;
    if (hex.size() >= 2 && hex[0] == '0' && (hex[1] == 'x' || hex[1] == 'X')) {
        hex = hex.substr(2);
    }
    if (hex.size() != 64) return false;
    out.resize(32);
    for (size_t i = 0; i < 32; ++i) {
        unsigned b;
        if (std::sscanf(hex.c_str() + i * 2, "%2x", &b) != 1) return false;
        out[i] = static_cast<uint8_t>(b);
    }
    return true;
}

std::vector<uint8_t> encode_match_id(const std::string& match_id) {
    std::vector<uint8_t> hex_bytes;
    if (try_parse_hex_bytes32(match_id, hex_bytes)) {
        return hex_bytes;
    }
    return encode_uint256_from_decimal(match_id);
}

// Right-padded-to-32-bytes address (20 bytes + 12 zero pad).
std::vector<uint8_t> encode_address(const std::vector<uint8_t>& addr20) {
    if (addr20.size() != 20) {
        throw std::invalid_argument("verifying_contract must be 20 bytes");
    }
    std::vector<uint8_t> out(32, 0);
    std::memcpy(out.data() + 12, addr20.data(), 20);
    return out;
}

std::vector<uint8_t> encode_uint8_padded(uint8_t v) {
    std::vector<uint8_t> out(32, 0);
    out[31] = v;
    return out;
}

std::vector<uint8_t> encode_uint64_padded(uint64_t v) {
    std::vector<uint8_t> out(32, 0);
    for (int i = 31; i >= 0 && v > 0; --i) {
        out[i] = static_cast<uint8_t>(v & 0xff);
        v >>= 8;
    }
    return out;
}

std::vector<uint8_t> concat(const std::vector<std::vector<uint8_t>>& parts) {
    std::vector<uint8_t> out;
    size_t total = 0;
    for (const auto& p : parts) total += p.size();
    out.reserve(total);
    for (const auto& p : parts) out.insert(out.end(), p.begin(), p.end());
    return out;
}

} // namespace

std::vector<uint8_t> compute_outcome_eip712_digest(
    const std::string& match_id,
    uint8_t outcome,
    const std::vector<uint8_t>& transcript_hash,
    const Eip712Domain& domain) {

    if (transcript_hash.size() != 32) {
        throw std::invalid_argument("transcript_hash must be 32 bytes");
    }
    if (domain.verifying_contract.size() != 20) {
        throw std::invalid_argument("domain.verifying_contract must be 20 bytes");
    }
    if (outcome < 1 || outcome > 4) {
        throw std::invalid_argument("outcome must be in 1..=4");
    }

    // Type hashes (canonical EIP-712 type encodings).
    static const std::vector<uint8_t> ASYNC_RESULT_TYPEHASH =
        keccak256("AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)");
    static const std::vector<uint8_t> EIP712_DOMAIN_TYPEHASH =
        keccak256("EIP712Domain(string name,string version,uint256 chainId,address verifyingContract)");
    static const std::vector<uint8_t> NAME_HASH = keccak256("AMPSettlement");
    static const std::vector<uint8_t> VERSION_HASH = keccak256("1");

    // structHash = keccak256(abi.encode(typeHash, matchId, outcome, transcriptHash))
    std::vector<uint8_t> struct_hash = keccak256(concat({
        ASYNC_RESULT_TYPEHASH,
        encode_match_id(match_id),
        encode_uint8_padded(outcome),
        transcript_hash,
    }));

    // domainSeparator = keccak256(abi.encode(typeHash, name, version, chainId, verifyingContract))
    std::vector<uint8_t> domain_separator = keccak256(concat({
        EIP712_DOMAIN_TYPEHASH,
        NAME_HASH,
        VERSION_HASH,
        encode_uint64_padded(domain.chain_id),
        encode_address(domain.verifying_contract),
    }));

    // digest = keccak256(0x1901 || domainSeparator || structHash)
    std::vector<uint8_t> digest_input;
    digest_input.push_back(0x19);
    digest_input.push_back(0x01);
    digest_input.insert(digest_input.end(), domain_separator.begin(), domain_separator.end());
    digest_input.insert(digest_input.end(), struct_hash.begin(), struct_hash.end());
    return keccak256(digest_input);
}

} // namespace amp
