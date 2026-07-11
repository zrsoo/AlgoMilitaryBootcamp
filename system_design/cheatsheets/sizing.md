# Sizing cheat sheet — single box vs fleet vs shard

⚠️ **Order-of-magnitude rules of thumb, NOT benchmarked guarantees.** Real numbers swing 10× with
hardware, record size, and query shape. Use these for the "does this bust a single box?" reflex only.

## The four numbers to compute early (out loud) in any design

1. **Write QPS** — per-day writes ÷ 10⁵ (sec/day), then ×2–5 for peak. Big → **shard**.
2. **Read QPS** — write QPS × assumed read:write ratio. Big → **cache + read replicas**.
3. **Storage over retention window** — records × bytes/record × retention. Too big for one machine → **shard**.
4. **Read:write ratio + access pattern** — read-heavy → cache-friendly; key-lookup → key-value store that
   shards naturally; write-heavy → write-optimized (LSM) engine; joins/aggregations → relational (but see note).

> Tiers scale on **independent axes**: QPS sizes the **app tier** (single box vs fleet); storage sizes the
> **data tier** (single DB vs sharded). A thin app tier in front of a sharded datastore is a normal shape.

## Single well-tuned node — rough ceilings

| Resource | Single node (rough) | To exceed → |
|---|---|---|
| Stateless app server (request handling) | ~10K req/s | **fleet** (near-linear: add machines) |
| Relational DB — reads (disk/buffer) | ~10K–50K QPS | **read replicas + cache** |
| Relational DB — writes (single primary) | ~**1K–10K** QPS | **shard** (or write-optimized engine) |
| Cache node (Redis/Memcached, in-RAM) | ~**100K–1M** ops/s | add cache nodes / cluster |
| RAM per machine | ~64 GB – few TB | hot set must fit here |
| SSD per DB machine (practical) | ~**1–10 TB** | beyond → **shard** |

## What each scaling move buys (and doesn't)

| Move | Scales | Does **not** scale | Ceiling raised to |
|---|---|---|---|
| Vertical (bigger box) | everything, modestly | — | biggest single machine (still a SPOF) |
| Fleet (app tier) | request/compute QPS | DB reads/writes/storage | ~linear in app machines |
| Replicas + cache | **read** QPS | writes, storage | many× reads |
| Sharding | **write** QPS + **storage** | cross-shard ops get hard | ~linear in shards |

- **Replication** scales reads (+ reliability); does NOT scale writes (every write hits all replicas).
- **Sharding** scales writes **and data size**; cross-shard joins/aggregations/transactions get hard; watch hot shards.
- Real systems do both: **shard the data, then replicate each shard** (shards *contain* replicas, not vice versa).

## Relational vs NoSQL crossover (heuristic, not a hard line)

- **Writes too much for relational:** single primary comfortable to ~**5–10K writes/s**. Need sustained ≫10K
  *and* data shards cleanly by key → **shard relational**, or move to **write-optimized LSM/wide-column**
  (Cassandra/Scylla). LSM turns random writes into sequential appends → higher write throughput.
- **"Read QPS too much for NoSQL":** NoSQL isn't bad at reads — it scales **point** reads fine horizontally.
  The real crossover is **query shape**: rich queries / joins / aggregations / strong consistency →
  **relational + replicas + cache**; simple key lookups at massive scale → NoSQL shards more transparently.
  LSM's catch is **read amplification** (mitigated by Bloom filters + cache).

## Worked verdicts

**URL shortener (100M links/day):** 1–5K writes/s (moderate); 100–500K reads/s (heavy, read-heavy →
cache + replicas); ~90 TB / 5 yr (too big for one box → shard). Key-lookup access → sharded key-value store.

**Photo service (500M photos/day, 2 MB each, 100:1 reads, 5 yr):**
- Blob tier: ~1.8 EB → **object storage (S3) + CDN** (S3 shards for you).
- Metadata tier: 5K/25K writes/s (borderline for one relational primary); 500K/2.5M reads/s (cache + many
  replicas); realistic metadata 200–900 TB (at ~500 B/record) → **shard**. (A lean 20-byte URL-only record
  would be ~20 TB and fit a big box — the record-size assumption flips the shard decision, so state it.)
- Feed: **precomputed / denormalized (fan-out-on-write)**, NOT read-time cross-shard joins.
