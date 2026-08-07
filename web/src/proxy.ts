import { NextRequest, NextResponse } from "next/server";
import { Ratelimit } from "@upstash/ratelimit";
import { Redis } from "@upstash/redis";

/**
 * Edge middleware — rate-limits every /api/* request via Upstash Redis.
 *
 * Tiers (per-IP, sliding window):
 *   strict  (5/min):  /paypal/*, /finalize, /init
 *   report  (30/min): /report
 *   default (60/min): everything else under /api/*
 *
 * If Upstash env vars are missing (local dev without Redis), rate limiting is
 * skipped — the app still works, just unthrottled.
 */

let strict: Ratelimit | null = null;
let report: Ratelimit | null = null;
let def: Ratelimit | null = null;
let initialized = false;

function init() {
  if (initialized) return;
  initialized = true;
  const url = process.env.UPSTASH_REDIS_REST_URL;
  const token = process.env.UPSTASH_REDIS_REST_TOKEN;
  if (!url || !token) return;
  const redis = new Redis({ url, token });
  strict = new Ratelimit({ redis, limiter: Ratelimit.slidingWindow(5, "1 m"), prefix: "amp:strict" });
  report = new Ratelimit({ redis, limiter: Ratelimit.slidingWindow(30, "1 m"), prefix: "amp:report" });
  def = new Ratelimit({ redis, limiter: Ratelimit.slidingWindow(60, "1 m"), prefix: "amp:default" });
}

function clientIp(req: NextRequest): string {
  return (
    req.headers.get("x-forwarded-for")?.split(",")[0]?.trim() ??
    "unknown"
  );
}

export async function proxy(req: NextRequest) {
  const path = req.nextUrl.pathname;
  if (!path.startsWith("/api/")) return NextResponse.next();

  init();
  if (!def) return NextResponse.next(); // Upstash not configured — skip

  const ip = clientIp(req);

  let rl: Ratelimit | null;
  if (path.includes("/paypal/") || path.endsWith("/finalize") || path.endsWith("/init")) {
    rl = strict;
  } else if (path.endsWith("/report")) {
    rl = report;
  } else {
    rl = def;
  }

  if (!rl) return NextResponse.next();

  const { success, remaining } = await rl.limit(ip);

  if (!success) {
    return new NextResponse(
      JSON.stringify({ error: "rate limit exceeded" }),
      { status: 429, headers: { "Content-Type": "application/json", "Retry-After": "60" } }
    );
  }

  const res = NextResponse.next();
  res.headers.set("X-RateLimit-Remaining", String(remaining));
  return res;
}

export const config = {
  matcher: "/api/:path*",
};
