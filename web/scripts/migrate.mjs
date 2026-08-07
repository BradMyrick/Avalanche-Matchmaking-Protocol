// Apply all SQL migrations in order to the configured DATABASE_URL.
// Run from web/:  DATABASE_URL='postgres://...' node scripts/migrate.mjs
import pg from "pg";
import { readFileSync, readdirSync } from "node:fs";
import { fileURLToPath } from "node:url";
import { dirname, join, resolve } from "node:path";

const here = dirname(fileURLToPath(import.meta.url));
const migrationsDir = resolve(here, "../../migrations");

if (!process.env.DATABASE_URL) {
  console.error("DATABASE_URL not set");
  process.exit(1);
}

const pool = new pg.Pool({ connectionString: process.env.DATABASE_URL });
const files = readdirSync(migrationsDir)
  .filter((f) => f.endsWith(".sql"))
  .sort();

console.log(`applying ${files.length} migration(s) from ${migrationsDir}`);
for (const f of files) {
  const sql = readFileSync(join(migrationsDir, f), "utf8");
  try {
    await pool.query(sql);
    console.log("  ✓", f);
  } catch (e) {
    console.error("  ✗", f, "-", e.message);
    throw e;
  }
}
await pool.end();
console.log("done");
