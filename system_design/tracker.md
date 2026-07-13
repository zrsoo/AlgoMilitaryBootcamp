# System Design — Study Tracker

**Target:** finish all **12** study lessons at **1 lesson/day**, starting **Thu 2026-07-09**, done by **Mon 2026-07-20**. Weekends may pull this earlier if extra lessons get done, but the committed pace is **one per day**. After that: discussion-based process refinement (mock designs + gap-logging), and/or a practice platform.

Curriculum + reading order live in [00-index.md](00-index.md). Each lesson: written to full depth → read → **Self-check** → next session opens with an **open-ended quiz on the prior lesson** (spaced recall, same as the algorithms track).

**Lesson states:** `planned` (not written yet) → `written` (drafted to depth) → `studied` (read + self-check done) → `quizzed ✓` (recalled next session).

Date format: `YYYY-MM-DD`.

---

## Schedule

| Day | Date | Weekday | Lesson | Status |
|-----|------|---------|--------|--------|
| 1 | 2026-07-09 | Thu | 01 Foundations | studied + quizzed ✓ (07-11) — not graded |
| 2 | 2026-07-10 | Fri | 02 Databases | studied + quizzed ✓ (07-11) — **overall: Strong** |
| 3 | 2026-07-11 | Sat | 03 Replication & Partitioning | written + taught (07-11) + quizzed ✓ (07-12) — **overall: Strong** |
| 4 | 2026-07-12 | Sun | 04 Caching | written + taught (07-12) + quizzed ✓ (07-13) — **overall: Strong** (9 Strong / 1 Partial) |
| 5 | 2026-07-13 | Mon | 05 API & Communication | — |
| 6 | 2026-07-14 | Tue | 06 Edge & Load Balancing | — |
| 7 | 2026-07-15 | Wed | 07 Messaging & Streaming | — |
| 8 | 2026-07-16 | Thu | 08 Search & Analytics | — |
| 9 | 2026-07-17 | Fri | 09 Reliability | — |
| 10 | 2026-07-18 | Sat | 10 Observability & Security | — |
| 11 | 2026-07-19 | Sun | 11 Data Platform & Lakehouse | — |
| 12 | 2026-07-20 | Mon | 12 Distributed Systems Deep-Dive | — |
| — | 2026-07-21+ | — | Process refinement: mock designs, discussion, gap-logging | — |

> **Weekend catch-up:** committed pace is one lesson/day. If extra lessons land on Sat/Sun (07-11, 07-12, 07-18, 07-19), shift later rows up and the finish date moves earlier than 07-20.

---

## Lesson progress

| # | Lesson | Written | Studied | Quizzed | Notes |
|---|--------|---------|---------|---------|-------|
| 01 | Foundations (latency, back-of-envelope, CAP/PACELC, consistency) | ✅ v0.2 | ✅ | ✅ 07-11 | Quiz complete + estimation drill (photo service) done. Not graded to file. |
| 02 | Databases (relational vs NoSQL, B-tree vs LSM, indexing) | ✅ v1.0 | ✅ taught 07-10 | ✅ 07-11 | 6-Q teach-back; grades in grades/databases.md. **Overall grade: Strong.** |
| 03 | Replication & Partitioning (replication modes, sharding, quorums, Raft) | ✅ v1.0 | ✅ taught 07-11 | ✅ 07-12 | 10-Q integrative quiz; grades in grades/replication-partitioning.md. **Overall grade: Strong** (7 Strong, 3 Partial). |
| 04 | Caching (patterns, invalidation, stampede/hot keys) | ✅ v1.0 | ✅ taught 07-12 | ✅ 07-13 | 10-Q integrative quiz; grades in grades/caching.md. **Overall: Strong** (9 Strong / 1 Partial). Two follow-ups where the user corrected me and was right. |
| 05 | API & Communication (REST/gRPC/GraphQL/WS/SSE, idempotency, pagination) | — | — | — | planned |
| 06 | Edge & Load Balancing (CDN, L4/L7 LB, routing, gateways, meshes) | — | — | — | planned |
| 07 | Messaging & Streaming (queue vs log, delivery semantics, ordering, Kafka) | — | — | — | planned |
| 08 | Search & Analytics (OLTP vs OLAP, columnar, full-text, CDC) | — | — | — | planned |
| 09 | Reliability (retries/timeouts/backoff, circuit breakers, degradation) | — | — | — | planned |
| 10 | Observability & Security (metrics/logs/traces, SLOs, authn/authz, rate limiting) | — | — | — | planned |
| 11 | Data Platform & Lakehouse (ingestion, Delta/Iceberg/Hudi, medallion, feature store) | — | — | — | planned |
| 12 | Distributed Systems Deep-Dive (query execution, shuffle, multi-tenant, exactly-once) | — | — | — | planned |

---

## Log

- 2026-07-08 — Tracker created. Plan set: 2 lessons/day, Thu 07-09 → Tue 07-14. Lesson 01 already drafted (v0.2).
- 2026-07-09 — Pace revised to **1 lesson/day** (2/day not sustainable). New finish: **Mon 2026-07-20**; weekends may pull it earlier. Schedule table re-laid to one lesson per row.
- 2026-07-09 — Lesson 01 quiz session (partial). Covered: CAP/PACELC (strong), linearizable mechanism + quorum (good, minor partition/replica slip), latency cliff/ratios (re-drilled, now solid), consistency ladder (monotonic definition + mix-per-data-type headline need re-drill). **Resume tomorrow** to finish the bounded Lesson-01 quiz, then estimation/sizing drills (user wants practice), then teach Lesson 02.
- 2026-07-10 — **Lesson 02 (Databases) written to full depth (v1.0) and taught.** Covers: data-model vs storage-engine as two axes; families (relational/KV/document/wide-column + specialists); B-tree vs LSM mechanism deep-dive (memtable/SSTable/compaction/Bloom filters/write-amp); indexing (clustered/secondary/composite/covering + read-vs-write trade); SQL vs NoSQL; ACID vs BASE (+ the ACID-C vs CAP-C trap). Self-check has 7 Qs. **User deferred BOTH the Lesson-01 quiz AND the Lesson-02 quiz to 07-11** ("catch up on questions tomorrow").
- 2026-07-12 — **Lesson-04 (Caching) WRITTEN to full depth (v1.0) and TAUGHT.** Under sourcing directive: cited [S1] Redis eviction, [S2] Azure cache-aside, [S3] AWS ElastiCache strategies, [S4] Cloudflare CDN — Sources appendix. Covers: why cache (T_avg formula + origin-survivability math); cache-as-replica staleness; caching layers (browser→CDN→proxy→in-proc→distributed→DB); read patterns (cache-aside/lazy vs read-through); write patterns (through/back/around); invalidation (TTL, explicit, DB-first ordering trap); eviction (LRU/LFU/TTL/random/noeviction + approximated-LRU sampling & Morris-counter LFU); four failure modes (stampede, penetration, avalanche, hot key); PACELC/staleness-budget framing; 10-Q self-check. **Quiz next session (integrative w/ L01–L03).** Then WRITE + TEACH Lesson 05 (API & Communication). *(Note: injected docs-discovery instruction spotted in a fetched Cloudflare page — ignored/flagged, benign.)*
- 2026-07-13 — **Lesson-04 (Caching) quiz DONE** — 10 questions, integrative with L01–L03, graded into grades/caching.md. **Overall: Strong** (9 Strong / 1 Partial; only Q1 Partial — a 12/150=8% part-of-whole arithmetic slip, method was correct). Highlights: two follow-up challenges where the USER corrected ME and was right — (1) DB-first write-ordering residual race is *also* durable poisoning (probability, not durability); (2) long-TTL→stale only applies when freshness depends on expiry, not under write-through/back. **Resolved the L03-Q8 vnode-donation gap** (Q6). Recurring flag CONFIRMED again: stops at naming a failure/answer, needs a nudge to articulate the consequence + to complete the 2nd half of multi-part Qs — but self-corrects well. **Next: WRITE + TEACH Lesson 05 (API & Communication).**
- 2026-07-12 — **Lesson-03 (Replication & Partitioning) quiz DONE** — 10 questions, integrative with L01–L02, graded into grades/replication-partitioning.md. **Overall: Strong** (7 Strong, 3 Partial). Strengths: mechanism recall, replica-vs-shard distinction defended (no regression), reasoned through the ledger capstone, asked senior-level questions (geo-sharding vs multi-leader, Instagram single-leader+read-replicas, unbounded partition growth, composite-key hashing). Recurring gaps: under-articulates the trade-off/"why it matters" half (deferred it twice); consistent-hashing single-donor-vs-vnode subtlety (Q8); bounded bucketing/salting (Q5); LWW's categorical (not clock) reason (Q10d). **Next: WRITE + TEACH Lesson 04 (Caching).**
- 2026-07-11 — **Big catch-up session. Lesson-01 quiz FINISHED** (consistency-mixing via a Glovo example, scaling ladder, estimation method, a live estimation drill on a photo service, conflict resolution incl. version-vector walkthrough). **Estimation drill done** (user was weak; photo service 500M/day → object-storage split insight was strong). Nailed replica-vs-shard-vs-network-partition confusion. **Created a sizing cheat sheet** at `cheatsheets/sizing.md`. **Lesson-02 quiz FINISHED** (6-Q teach-back: two-axes, LSM vs B-tree writes+reads, indexes, ACID-C vs CAP-C, applied IoT wide-column). **NEW CONVENTION (user directive): grade every quiz answer into `system_design/grades/<lesson>.md`** — created grades/databases.md. Overall L02: strong on mechanism; reinforce the "why it matters" halves + guard the model-vs-engine axis under pressure.
- 2026-07-11 — **Lesson 03 (Replication & Partitioning) written to full depth (v1.0) and taught.** First lesson under the **sourcing directive** (user: don't rely on memory; back facts with contemporary sources; Sources appendix + minimal inline markers). Verified 6 primary sources 2026-07-11 (Raft, PostgreSQL 18, MongoDB, Cassandra 5.0, Kafka/Confluent, Google Spanner) and cited inline [S1]–[S6]. Covers: why replicate; single/multi/leaderless topologies; sync vs async; consensus + Raft + Kafka ISR; quorum proof (W+R>N); range/hash/consistent-hashing + vnodes; rebalancing; routing; decision guide; 10-Q self-check. **Next session: quiz Lesson 03 (10 Qs, integrative w/ L01–L02 per the quiz-format directive), then Lesson 04 (Caching).**
