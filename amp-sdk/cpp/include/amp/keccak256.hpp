// Bundled Keccak-256 (Ethereum variant — padding byte 0x01, NOT SHA3's 0x06).
// Used by AMP for EIP-712 outcome digest computation. Bundling a vetted
// single-header Keccak here means Unreal / Unity-Native users get a working
// outcome-signing path WITHOUT having to find + integrate a third-party
// crypto library just to compute the digest.
//
// For secp256k1 signing itself, users still need libsecp256k1 or an
// equivalent — see amp-sdk/cpp/include/amp/client.hpp::SignOutcomeCallback.
//
// Algorithm: Keccak-f[1600] permutation with 1088-bit rate (1088/8 = 136-byte
// block), 512-bit capacity, 256-bit output. Padding: Keccak's 0x01 (the
// original NIST submission's padding; SHA-3 changed it to 0x06).
//
// Reference: Bertoni, Daemen, Peeters, Van Assche. "Keccak specification."
// Version 3, 2011. https://keccak.team/files/Keccak-specification-3.pdf
//
// License: CC0 / public domain (the algorithm is public; this implementation
// is original work contributed to AMP under Apache-2.0).
#pragma once

#include <cstdint>
#include <cstring>
#include <string>
#include <vector>

namespace amp {

/// Compute Keccak-256 (Ethereum variant) of `input`. Returns 32 bytes.
/// NOT SHA3-256 — Ethereum's "keccak256" uses the original Keccak padding
/// (0x01), while NIST's SHA-3 uses 0x06. The two produce different hashes
/// for the same input.
inline std::vector<uint8_t> keccak256(const uint8_t* input, size_t len) {
    // Keccak-f[1600] round constants (24 rounds). Standard Keccak reference
    // values from the official specification; not magic.
    static const uint64_t RC[24] = {
        0x0000000000000001ULL, 0x0000000000008082ULL, 0x800000000000808aULL,
        0x8000000080008000ULL, 0x000000000000808bULL, 0x0000000080000001ULL,
        0x8000000080008081ULL, 0x8000000000008009ULL, 0x000000000000008aULL,
        0x0000000000000088ULL, 0x0000000080008009ULL, 0x000000008000000aULL,
        0x000000008000808bULL, 0x800000000000008bULL, 0x8000000000008089ULL,
        0x8000000000008003ULL, 0x8000000000008002ULL, 0x8000000000000080ULL,
        0x000000000000800aULL, 0x800000008000000aULL, 0x8000000080008081ULL,
        0x8000000000008080ULL, 0x0000000080000001ULL, 0x8000000080008008ULL,
    };

    // Rotation offsets for the ρ step, indexed by [x][y] (5x5 lanes).
    static const unsigned ROT[5][5] = {
        { 0, 36,  3, 41, 18},
        { 1, 44, 10, 45,  2},
        {62,  6, 43, 15, 61},
        {28, 55, 25, 21, 56},
        {27, 20, 39,  8, 14},
    };

    // Keccak state: 5x5 lanes of 64 bits (1600 bits total).
    uint64_t A[5][5] = {{0}};

    // Keccak-f[1600] permutation.
    auto keccak_f = [&A]() {
        for (unsigned round = 0; round < 24; ++round) {
            // θ step
            uint64_t C[5];
            for (unsigned x = 0; x < 5; ++x) {
                C[x] = A[x][0] ^ A[x][1] ^ A[x][2] ^ A[x][3] ^ A[x][4];
            }
            uint64_t D[5];
            for (unsigned x = 0; x < 5; ++x) {
                D[x] = C[(x + 4) % 5] ^ ((C[(x + 1) % 5] << 1) | (C[(x + 1) % 5] >> 63));
            }
            for (unsigned x = 0; x < 5; ++x) {
                for (unsigned y = 0; y < 5; ++y) {
                    A[x][y] ^= D[x];
                }
            }

            // ρ + π steps (combined).
            uint64_t B[5][5];
            for (unsigned x = 0; x < 5; ++x) {
                for (unsigned y = 0; y < 5; ++y) {
                    unsigned dst_x = y;
                    unsigned dst_y = (2 * x + 3 * y) % 5;
                    uint64_t v = A[x][y];
                    unsigned r = ROT[x][y];
                    B[dst_x][dst_y] = (r == 0) ? v : ((v << r) | (v >> (64 - r)));
                }
            }

            // χ step.
            for (unsigned y = 0; y < 5; ++y) {
                for (unsigned x = 0; x < 5; ++x) {
                    A[x][y] = B[x][y] ^ ((~B[(x + 1) % 5][y]) & B[(x + 2) % 5][y]);
                }
            }

            // ι step.
            A[0][0] ^= RC[round];
        }
    };

    // Absorb phase. Rate = 136 bytes (1088 bits) for Keccak-256.
    constexpr size_t RATE = 136;
    size_t offset = 0;
    while (offset + RATE <= len) {
        for (unsigned i = 0; i < RATE / 8; ++i) {
            uint64_t lane = 0;
            std::memcpy(&lane, input + offset + i * 8, 8);
            A[i % 5][i / 5] ^= lane;
        }
        keccak_f();
        offset += RATE;
    }

    // Final block: remaining input + Keccak padding (0x01 ... 0x80).
    // The rate is 136 bytes; the padding rule for Keccak (Ethereum variant)
    // is: append 0x01, then zero-fill, then set the last byte's high bit.
    // (SHA-3 uses 0x06 instead of 0x01 — this is the only difference.)
    uint8_t block[RATE] = {0};
    size_t rem = len - offset;
    std::memcpy(block, input + offset, rem);
    block[rem] = 0x01;          // Keccak padding (NOT SHA-3's 0x06).
    block[RATE - 1] |= 0x80;    // Final-bit per the spec.
    for (unsigned i = 0; i < RATE / 8; ++i) {
        uint64_t lane = 0;
        std::memcpy(&lane, block + i * 8, 8);
        A[i % 5][i / 5] ^= lane;
    }
    keccak_f();

    // Squeeze: extract first 32 bytes (4 lanes of 8 bytes).
    std::vector<uint8_t> out(32);
    for (unsigned i = 0; i < 4; ++i) {
        uint64_t lane = A[i % 5][i / 5];
        std::memcpy(out.data() + i * 8, &lane, 8);
    }
    return out;
}

/// Convenience overload for std::vector input.
inline std::vector<uint8_t> keccak256(const std::vector<uint8_t>& input) {
    return keccak256(input.data(), input.size());
}

/// Convenience overload for null-terminated string input (the bytes of the
/// string, not including the terminator). Useful for hashing type strings
/// like "AsyncResult(uint256 matchId,uint8 outcome,bytes32 transcriptHash)".
inline std::vector<uint8_t> keccak256(const std::string& input) {
    return keccak256(reinterpret_cast<const uint8_t*>(input.data()), input.size());
}

} // namespace amp
