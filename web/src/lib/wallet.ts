import { ethers } from "ethers";

/** A generated custodial wallet for a tournament winner's payout. */
export interface GeneratedWallet {
  address: string;
  privateKey: string;
}

/** Generate `n` fresh random wallets (offline; no network). */
export function generateWallets(n: number): GeneratedWallet[] {
  const out: GeneratedWallet[] = [];
  for (let i = 0; i < n; i++) {
    const w = ethers.Wallet.createRandom();
    out.push({ address: w.address, privateKey: w.privateKey });
  }
  return out;
}

/** Parse a free-text list of addresses (one per line, comma, or space). */
export function parseAddressList(raw: string): string[] {
  return raw
    .split(/[\s,]+/)
    .map((s) => s.trim())
    .filter((s) => s.length > 0 && ethers.isAddress(s));
}
