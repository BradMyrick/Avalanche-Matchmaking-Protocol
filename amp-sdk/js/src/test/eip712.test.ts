/**
 * Cross-language KAT for the EIP-712 outcome digest.
 * Same vector as test_outcome_digest_known_vector_cross_lang in amp-server.
 */
import { test } from "node:test";
import assert from "node:assert/strict";
import { computeOutcomeEip712Digest } from "../eip712.js";

test("outcome digest matches cross-language KAT", () => {
  const expected =
    "2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c";
  const d = computeOutcomeEip712Digest("1", 1, new Uint8Array(32));
  const hex = Buffer.from(d).toString("hex");
  assert.equal(
    hex,
    expected,
    "JS digest must match Rust/C#/Python — see test_outcome_digest_known_vector_cross_lang",
  );
});

test("outcome digest differs on different transcript", () => {
  const d1 = computeOutcomeEip712Digest("1", 1, new Uint8Array(32));
  const d2 = computeOutcomeEip712Digest("1", 1, new Uint8Array(32).fill(1));
  assert.notEqual(Buffer.from(d1).toString("hex"), Buffer.from(d2).toString("hex"));
});

test("outcome digest rejects invalid inputs", () => {
  assert.throws(() => computeOutcomeEip712Digest("1", 1, new Uint8Array(16)), /32 bytes/);
  assert.throws(() => computeOutcomeEip712Digest("1", 5, new Uint8Array(32)), /1\.\.=4/);
});
