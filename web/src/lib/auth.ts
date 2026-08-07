import { getStore, type TournamentRecord } from "./store";

/**
 * Verify an organizer bearer token (constant-time compare).
 * Returns the tournament record if authorized, null otherwise.
 */
export async function requireOrganizer(
  req: Request,
  tid: number
): Promise<TournamentRecord | null> {
  const auth = req.headers.get("authorization") ?? "";
  const token = auth.startsWith("Bearer ") ? auth.slice(7).trim() : "";
  if (!token || token.length < 16) return null;

  const rec = await getStore().getTournament(tid);
  if (!rec?.manageToken) return null;

  // Constant-time comparison to prevent timing attacks.
  const a = token;
  const b = rec.manageToken;
  if (a.length !== b.length) return null;
  let diff = 0;
  for (let i = 0; i < a.length; i++) {
    diff |= a.charCodeAt(i) ^ b.charCodeAt(i);
  }
  return diff === 0 ? rec : null;
}

/** Generate a cryptographically random manage token (64 hex chars). */
export function generateManageToken(): string {
  const buf = new Uint8Array(32);
  crypto.getRandomValues(buf);
  return Array.from(buf).map((b) => b.toString(16).padStart(2, "0")).join("");
}
