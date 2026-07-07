# 01 — Foundations

The mental primitives every design rests on: how to reason about **numbers**, and the **consistency/availability** trade-offs that decide most storage choices. Master this before the component-specific notes — everything else is an application of these.

---

## Part A — Reasoning about numbers

You can't design what you can't size. Two skills: knowing the **latency hierarchy**, and doing **back-of-envelope** estimates fast.

### The latency hierarchy (orders of magnitude, memorize the *ratios*)

Absolute numbers drift with hardware; the **ratios** are what matter for decisions.

| Operation | ~Time | Takeaway |
|---|---|---|
| L1 cache reference | ~1 ns | — |
| Main memory (RAM) reference | ~100 ns | RAM is ~100× slower than L1 |
| Read 1 MB sequentially from RAM | ~10 µs | — |
| SSD random read | ~100 µs | ~1000× slower than RAM |
| Read 1 MB from SSD | ~1 ms | — |
| Round trip within a datacenter | ~0.5 ms | Network hop ≈ same order as SSD |
| Read 1 MB from disk (HDD) | ~20 ms | Avoid |
| Round trip across continents | ~150 ms | Geography dominates; cache/colocate |

**Decisions this drives:**
- Memory beats disk by ~1000×, disk beats cross-region by another big factor → **cache hot data in RAM**, **colocate** compute with data, **avoid cross-region on the hot path**.
- A cross-continent round trip (~150 ms) blows most latency SLAs by itself → replicate near users; don't do chatty cross-region calls.
- Sequential >> random (especially on disk) → storage engines optimize for sequential writes (see LSM in note 02).

### Back-of-envelope estimation

Goal: size the system (single box? fleet? sharded?) — not precision. Round hard, state assumptions.

**Cheat sheet:**
- Seconds/day ≈ **86,400 ≈ 10⁵**. So "1M events/day" ≈ **~12/sec** (undramatic); "1B/day" ≈ **~12K/sec** (needs a fleet).
- Powers of 2 for storage: 2¹⁰ ≈ 1 K, 2²⁰ ≈ 1 M, 2³⁰ ≈ 1 B (KB/MB/GB/TB/PB step by 2¹⁰).
- Peak ≈ average **× 2–5**.

**Worked example — a URL shortener at 100M new links/day:**
- Writes: 100M / 10⁵ s ≈ **~1,000 writes/sec** (avg); ~say 5K/sec peak.
- Reads: assume 100:1 read:write → **~100K reads/sec** → this is read-heavy → **cache + replicas**.
- Storage: 100M/day × 365 × ~5 yr ≈ ~180B rows; ~500 bytes each ≈ **~90 TB** → too big for one node → **partition**, and the read path screams for **cache**.
- Conclusion from ~4 lines of math: read-heavy, needs caching, needs partitioning, KV-shaped access (lookup by short code). The numbers *chose the architecture*.

**The move in an interview:** compute read QPS, write QPS, storage/retention, and read:write ratio. Those four numbers point at cache/replica/shard decisions before you draw a single box.

---

## Part B — Consistency, availability, and the trade-offs

Most storage decisions come down to *how much consistency you actually need*. Over-asking for consistency costs latency and availability; under-asking corrupts money and inventory.

### CAP theorem (the honest version)

When a **network partition** happens (nodes can't all talk), you must choose:
- **CP** — stay **consistent**, refuse/deny some requests (reduced availability). Choose for money, inventory, anything where a wrong answer is worse than no answer.
- **AP** — stay **available**, serve possibly-stale data (reduced consistency). Choose for feeds, counts, likes, where "slightly stale" beats "down."

CAP only bites *during a partition*. The common misread is "pick 2 of 3 always" — no; **C-vs-A is only forced when P occurs.**

### PACELC (what CAP forgets)

Even with **no partition (E, else)**, there's a standing trade-off: **latency vs consistency**. Synchronously replicating to more nodes = more consistent but slower. So:
> **PAC** (if Partition: Availability or Consistency) **ELC** (Else: Latency or Consistency).

This is why "strongly consistent everywhere" is expensive even on a healthy day — every strong read/write pays coordination latency.

### Consistency models (from strong to weak)

Pick the **weakest model that's still correct** for that data — weaker = cheaper/faster/more available.

| Model | Guarantee | Use for | Cost |
|---|---|---|---|
| **Linearizable / strong** | Every read sees the latest write, as if one copy | Money, inventory, unique constraints, locks | Coordination latency; availability drops under partition |
| **Sequential** | All nodes see ops in the same order (not necessarily real-time) | Replicated logs, some consensus | Still ordered-globally cost |
| **Read-your-writes** | You see your own writes immediately | Profile edits, "post then view" | Session pinning / routing to leader |
| **Monotonic reads** | You never see time go backwards | Feeds, timelines | Sticky routing |
| **Eventual** | Replicas converge *eventually* if writes stop | Likes, view counts, DNS, caches | Temporary staleness/conflicts |

**Mixing models is normal and expected (senior tell):** in one product, the account balance is strong, the "likes" count is eventual, the user's own recent posts are read-your-writes. Don't apply one model globally.

### Where conflicts come from (and resolution)

Multi-leader / eventual systems get concurrent conflicting writes. Resolution options:
- **Last-write-wins (LWW)** — simple, lossy (drops a real write).
- **Version vectors / causal tracking** — detect concurrency, surface conflicts.
- **CRDTs** — data types that merge deterministically (counters, sets) — great for collaborative/offline.
- **Application-level merge** — domain logic decides (e.g. shopping-cart union).

---

## Part C — The trade-off vocabulary (say these out loud)

These tensions recur in every design. Naming which side you take *and why* is the graded skill.

- **Latency vs throughput** — batching raises throughput but adds latency; optimize for one.
- **Consistency vs availability** (CAP) and **consistency vs latency** (PACELC).
- **Read optimization vs write optimization** — denormalize/index/cache speeds reads, slows writes (and vice versa).
- **Normalization vs denormalization** — no duplication + write-simple vs read-fast + join-free.
- **Sync vs async** — simple/consistent vs decoupled/resilient/eventual.
- **Strong-consistency cost vs eventual-consistency risk** — pick per data type.
- **Cost vs performance vs complexity** — the eternal triangle; name the corner you sacrifice.
- **Push vs pull** — push (low latency, server-driven, fan-out cost) vs pull (simple, client-controlled, wasteful/laggy).

---

## Self-check (be able to answer without notes)

1. RAM is ~___× slower than L1 and ~___× faster than SSD random read? (100; ~1000)
2. Roughly how many seconds in a day, and why is that the handiest constant? (~86,400 ≈ 10⁵ → divide daily volume to get per-second QPS.)
3. CAP: what exactly are you forced to choose between, and *when*? (C vs A, only during a network partition.)
4. What does PACELC add? (Even with no partition: latency vs consistency.)
5. Give a single product where you'd deliberately use three different consistency models, and name them. (e.g. balance=strong, likes=eventual, own posts=read-your-writes.)
6. Two concurrent conflicting writes in an eventual system — name three resolution strategies. (LWW, version vectors, CRDT/app-merge.)

---

## Changelog

- **v0.1 — 2026-07-07:** First foundations note (style anchor). Latency hierarchy, estimation, CAP/PACELC, consistency models, conflict resolution, trade-off vocabulary, self-check.
