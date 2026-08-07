/**
 * PayPal helpers (server-only). Mirrors the BitVanes account integration:
 * client-credentials auth + server-side order verification.
 *
 * Env (set these in .env, names match BitVanes so credentials port directly):
 *   PAYPAL_CLIENT_ID            REST API client id
 *   PAYPAL_SECRET               REST API secret (PAYPAL_CLIENT_SECRET also accepted)
 *   PAYPAL_API_BASE             https://api-m.sandbox.paypal.com | https://api-m.paypal.com
 *   NEXT_PUBLIC_PAYPAL_CLIENT_ID  public id for the browser SDK
 */

const PAYPAL_BASE = process.env.PAYPAL_API_BASE || "https://api-m.paypal.com";
const PAYPAL_CLIENT_ID =
  process.env.PAYPAL_CLIENT_ID || process.env.PAYPAL_ID || "";
const PAYPAL_SECRET =
  process.env.PAYPAL_SECRET || process.env.PAYPAL_CLIENT_SECRET || "";

let cachedToken: { token: string; expires: number } | null = null;

export async function paypalAccessToken(): Promise<string> {
  if (!PAYPAL_CLIENT_ID || !PAYPAL_SECRET) {
    throw new Error("PayPal credentials not configured (PAYPAL_CLIENT_ID / PAYPAL_SECRET).");
  }
  const now = Date.now();
  if (cachedToken && cachedToken.expires > now + 30_000) {
    return cachedToken.token;
  }
  const auth = Buffer.from(`${PAYPAL_CLIENT_ID}:${PAYPAL_SECRET}`).toString("base64");
  const res = await fetch(`${PAYPAL_BASE}/v1/oauth2/token`, {
    method: "POST",
    headers: {
      Authorization: `Basic ${auth}`,
      "Content-Type": "application/x-www-form-urlencoded",
    },
    body: "grant_type=client_credentials",
  });
  if (!res.ok) throw new Error(`PayPal auth failed (${res.status})`);
  const json = (await res.json()) as { access_token: string; expires_in: number };
  cachedToken = { token: json.access_token, expires: now + json.expires_in * 1000 };
  return json.access_token;
}

export function paypalBase() {
  return PAYPAL_BASE;
}

export function paypalConfigured(): boolean {
  return Boolean(PAYPAL_CLIENT_ID && PAYPAL_SECRET);
}

export interface VerifiedOrder {
  id: string;
  status: string;
  amountUsd: number;
  customId?: string;
}

/** Server-side verify: fetch the order and trust ONLY the server-returned amount. */
export async function verifyOrder(orderId: string): Promise<VerifiedOrder> {
  const token = await paypalAccessToken();
  const res = await fetch(`${PAYPAL_BASE}/v2/checkout/orders/${orderId}`, {
    headers: { Authorization: `Bearer ${token}` },
  });
  if (!res.ok) throw new Error(`PayPal order fetch failed (${res.status})`);
  const order = (await res.json()) as {
    status: string;
    purchase_units?: Array<{
      payments?: { captures?: Array<{ amount: { value: string; currency_code: string } }> };
      custom_id?: string;
      amount?: { value: string; currency_code: string };
    }>;
  };
  const capture = order.purchase_units?.[0]?.payments?.captures?.[0];
  const amountSrc = capture?.amount ?? order.purchase_units?.[0]?.amount;
  if (!amountSrc) throw new Error("No amount on PayPal order");
  if (amountSrc.currency_code !== "USD") throw new Error("Non-USD order rejected");
  return {
    id: orderId,
    status: order.status,
    amountUsd: parseFloat(amountSrc.value),
    customId: order.purchase_units?.[0]?.custom_id,
  };
}
