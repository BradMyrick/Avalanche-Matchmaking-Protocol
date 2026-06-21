import { test } from "node:test";
import assert from "node:assert/strict";
import { computeOutcomeEip712Digest, NativeAmpClient } from "../index.js";

const KAT = "2d2525ad5098ca8f82a2a6cabc6775c40a55df96dfa2fbb46d7c0e372b99096c";

test("native computeOutcomeEip712Digest matches the cross-language KAT", () => {
  const digest = computeOutcomeEip712Digest(
    "1",
    1,
    Buffer.from(new Uint8Array(32)),
    43113,
    Buffer.from(new Uint8Array(20)),
  );
  assert.equal(Buffer.from(digest).toString("hex"), KAT);
});

test("native digest rejects a short transcript hash", () => {
  assert.throws(
    () => computeOutcomeEip712Digest("1", 1, Buffer.from(new Uint8Array(16)), 43113, Buffer.from(new Uint8Array(20))),
    /32 bytes/,
  );
});

test("native digest differs for different transcript hashes", () => {
  const a = computeOutcomeEip712Digest("1", 1, Buffer.from(new Uint8Array(32)), 43113, Buffer.from(new Uint8Array(20)));
  const b = computeOutcomeEip712Digest("1", 1, Buffer.from(new Uint8Array(32).fill(1)), 43113, Buffer.from(new Uint8Array(20)));
  assert.notEqual(Buffer.from(a).toString("hex"), Buffer.from(b).toString("hex"));
});

test("NativeAmpClient constructs and closes the worker cleanly", async () => {
  const c = new NativeAmpClient();
  // No connect — just verify the worker thread spawned and tears down.
  await c.close();
});
