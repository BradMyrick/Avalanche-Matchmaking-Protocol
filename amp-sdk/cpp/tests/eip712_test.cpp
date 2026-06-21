// AMP C++ SDK tests — bring this SDK to tier-1 (parity with Rust/Go/C#).
//
// Coverage:
//   - Bundled Keccak-256 matches known test vectors (empty string, "abc").
//   - EIP-712 outcome digest matches the canonical cross-language KAT
//     vector `2d2525ad...b99096c` shared with Rust, Go, Python, JS, C#, Solidity.
//   - Digest rejects invalid inputs (transcript length, outcome range,
//     verifying-contract length).
//   - Digest is sensitive to match_id, outcome, transcript, chain_id,
//     verifying_contract (any change must produce a different digest).
//
// Build + run:
//   cd amp-sdk/cpp && cmake -B build && cmake --build build -j
//   ./build/amp_tests
//
// Hand-rolled test runner (no GoogleTest dep) so the test target builds in
// minimal environments (Unreal's build toolchain, restricted CI images).
#include "amp/keccak256.hpp"
#include "amp/eip712.hpp"

#include <cstdio>
#include <cstring>
#include <functional>
#include <string>
#include <vector>

namespace {

int g_tests_run = 0;
int g_tests_failed = 0;

std::string to_hex(const std::vector<uint8_t>& bytes) {
    std::string out;
    out.reserve(bytes.size() * 2);
    static const char* kHex = "0123456789abcdef";
    for (uint8_t b : bytes) {
        out.push_back(kHex[b >> 4]);
        out.push_back(kHex[b & 0x0f]);
    }
    return out;
}

void expect_hex_eq(const std::vector<uint8_t>& actual, const std::string& expected,
                  const char* what) {
    ++g_tests_run;
    std::string actual_hex = to_hex(actual);
    if (actual_hex != expected) {
        ++g_tests_failed;
        std::fprintf(stderr, "[FAIL] %s\n  expected: %s\n  actual:   %s\n",
                     what, expected.c_str(), actual_hex.c_str());
    } else {
        std::printf("[pass] %s\n", what);
    }
}

void expect_throws(const std::function<void()>& fn, const char* what) {
    ++g_tests_run;
    try {
        fn();
        ++g_tests_failed;
        std::fprintf(stderr, "[FAIL] %s: expected exception, none thrown\n", what);
    } catch (...) {
        std::printf("[pass] %s (threw as expected)\n", what);
    }
}

void expect_not_eq(const std::vector<uint8_t>& a, const std::vector<uint8_t>& b,
                   const char* what) {
    ++g_tests_run;
    if (to_hex(a) == to_hex(b)) {
        ++g_tests_failed;
        std::fprintf(stderr, "[FAIL] %s: digests should differ but match\n", what);
    } else {
        std::printf("[pass] %s\n", what);
    }
}

void test_keccak256_empty() {
    auto digest = amp::keccak256(std::string(""));
    expect_hex_eq(digest,
        "c5d2460186f7233c927e7db2dcc703c0e500b653ca82273b7bfad8045d85a470",
        "Keccak256(\"\")");
}

void test_keccak256_abc() {
    auto digest = amp::keccak256(std::string("abc"));
    expect_hex_eq(digest,
        "4e03657aea45a94fc7d47ba826c8d667c0d1e6e33a64a036ec44f58fa12d6c45",
        "Keccak256(\"abc\")");
}

void test_eip712_cross_lang_kat() {
    // The canonical AMP cross-language KAT vector. Same as:
    //   - amp-server/src/main.rs::test_outcome_digest_known_vector_cross_lang
    //   - amp-sdk/python/tests/test_client.py::test_digest_known_vector_cross_lang
    //   - amp-sdk/js/src/test/eip712.test.ts
    //   - amp-sdk/csharp/AmpSdk.Tests/OutcomeEip712Tests.cs::Digest_MatchesCrossLanguageKat
    //   - contracts/test/AMPSettlement.t.sol::testEIP712DigestMatchesCrossLangVector
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(20, 0);
    std::vector<uint8_t> transcript(32, 0);

    auto digest = amp::compute_outcome_eip712_digest("1", 1, transcript, domain);
    expect_hex_eq(digest,
        "2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c",
        "EIP-712 cross-language KAT (matchId=1, outcome=1, transcript=0)");
}

void test_eip712_rejects_bad_transcript() {
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(20, 0);
    expect_throws([&] {
        amp::compute_outcome_eip712_digest("1", 1, std::vector<uint8_t>(16, 0), domain);
    }, "Eip712 rejects 16-byte transcript");
}

void test_eip712_rejects_bad_outcome() {
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(20, 0);
    expect_throws([&] {
        amp::compute_outcome_eip712_digest("1", 0, std::vector<uint8_t>(32, 0), domain);
    }, "Eip712 rejects outcome=0");
    expect_throws([&] {
        amp::compute_outcome_eip712_digest("1", 5, std::vector<uint8_t>(32, 0), domain);
    }, "Eip712 rejects outcome=5");
}

void test_eip712_rejects_bad_verifying_contract() {
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(10, 0);
    expect_throws([&] {
        amp::compute_outcome_eip712_digest("1", 1, std::vector<uint8_t>(32, 0), domain);
    }, "Eip712 rejects 10-byte verifying_contract");
}

void test_eip712_sensitive_to_match_id() {
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(20, 0);
    std::vector<uint8_t> t(32, 0);
    auto d1 = amp::compute_outcome_eip712_digest("1", 1, t, domain);
    auto d2 = amp::compute_outcome_eip712_digest("2", 1, t, domain);
    expect_not_eq(d1, d2, "Digest sensitive to match_id");
}

void test_eip712_sensitive_to_outcome() {
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(20, 0);
    std::vector<uint8_t> t(32, 0);
    auto d1 = amp::compute_outcome_eip712_digest("1", 1, t, domain);
    auto d2 = amp::compute_outcome_eip712_digest("1", 2, t, domain);
    expect_not_eq(d1, d2, "Digest sensitive to outcome");
}

void test_eip712_sensitive_to_transcript() {
    amp::Eip712Domain domain;
    domain.chain_id = 43113;
    domain.verifying_contract = std::vector<uint8_t>(20, 0);
    std::vector<uint8_t> t1(32, 0);
    std::vector<uint8_t> t2(32, 0);
    t2[0] = 0x01;
    auto d1 = amp::compute_outcome_eip712_digest("1", 1, t1, domain);
    auto d2 = amp::compute_outcome_eip712_digest("1", 1, t2, domain);
    expect_not_eq(d1, d2, "Digest sensitive to transcript");
}

void test_eip712_sensitive_to_chain_id() {
    amp::Eip712Domain d1, d2;
    d1.verifying_contract = d2.verifying_contract = std::vector<uint8_t>(20, 0);
    d1.chain_id = 43113;
    d2.chain_id = 43114;
    std::vector<uint8_t> t(32, 0);
    auto digest1 = amp::compute_outcome_eip712_digest("1", 1, t, d1);
    auto digest2 = amp::compute_outcome_eip712_digest("1", 1, t, d2);
    expect_not_eq(digest1, digest2, "Digest sensitive to chain_id");
}

void test_eip712_sensitive_to_verifying_contract() {
    amp::Eip712Domain d1, d2;
    d1.chain_id = d2.chain_id = 43113;
    d1.verifying_contract = std::vector<uint8_t>(20, 0);
    d2.verifying_contract = std::vector<uint8_t>(20, 0);
    d2.verifying_contract[19] = 0x01;
    std::vector<uint8_t> t(32, 0);
    auto digest1 = amp::compute_outcome_eip712_digest("1", 1, t, d1);
    auto digest2 = amp::compute_outcome_eip712_digest("1", 1, t, d2);
    expect_not_eq(digest1, digest2, "Digest sensitive to verifying_contract");
}

} // namespace

int main() {
    test_keccak256_empty();
    test_keccak256_abc();
    test_eip712_cross_lang_kat();
    test_eip712_rejects_bad_transcript();
    test_eip712_rejects_bad_outcome();
    test_eip712_rejects_bad_verifying_contract();
    test_eip712_sensitive_to_match_id();
    test_eip712_sensitive_to_outcome();
    test_eip712_sensitive_to_transcript();
    test_eip712_sensitive_to_chain_id();
    test_eip712_sensitive_to_verifying_contract();

    std::printf("\n%d tests run, %d failed\n", g_tests_run, g_tests_failed);
    return g_tests_failed == 0 ? 0 : 1;
}
