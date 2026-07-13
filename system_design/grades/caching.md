# Lesson 04 — Caching — Quiz Grades

Quiz date: 2026-07-13. Format: 10 questions, integrative with L01–L03. Scale: Strong / Partial / Weak.

---

## Q1 (integrative: L01 sizing + L04 Part 1) — hit-ratio floor, T_avg, binding benefit
- **Grade: Partial**
- (a) Method correct (misses must fit under DB capacity: 150K·(1−h) ≤ 12K), but **arithmetic slip**: computed 12/150 as 12.5% → answered 87% floor. Correct is 12/150 = 8% → **h_min = 92%**.
- (b) Formula correct. Minor: used T_miss=25 instead of 25.5, and used h=0.95 rather than the floor from (a). At floor 92%: T_avg = 2.5 ms.
- (c) Correct after rephrase: **origin/load protection** is binding — DB (12K) would be overwhelmed by 150K without ≥92% hits.
- **Gaps:** ratio arithmetic under pressure (part-of-whole percentage). Concept/method solid.

## Q2 (integrative: L03 replication + L04 Part 5–6) — write-ordering trap
- **Grade: Strong**
- (a) Precise walkthrough of the stale-repopulation race (delete→miss→read old→set old→db update lands later). Nice nuance: writer-internal ordering, delete idempotent.
- (b) Correct ordering (db.update → cache.delete) and correctly identified the stale window between update and delete. **Slightly imprecise "why"**: framed it as op-speed/×1000 window shrink; the real reason is DB-first means the delete acts on an already-correct DB, so post-delete repopulations are fresh → cache can't be *durably* poisoned (self-heals vs lasting full TTL).
- (c) Correct: update source of truth first, then invalidate copies.
- **Follow-up (user challenge, correct):** user correctly pushed back that the residual DB-first race is *also* durable poisoning (stale until TTL), not self-healing. Corrected: both orderings can durably poison; DB-first reduces **probability/window**, not durability. Strict fix = TTL backstop + versioning/CAS or write-through. **User's model is sharp here — no gap; reinforce the "probability not durability" framing.**

## Q3 (L04 Part 7 — eviction mechanism depth) — approximated LFU
- **Grade: Strong**
- (a) Understood the mechanism. **Terminology fixes:** "Morris counter" (said "Morris trees"); ~8 *bits* per key (said "bytes"); decay is **continuous** (~1/min), not only after saturation. Log-scale + ~1M saturation correct.
- (b) Correct: without decay a burst-then-idle key stays immortal; decay lets LFU adapt to shifting popularity.
- (c) Correct concept (historically-hot, recently-cold: LRU evicts on recency, LFU keeps on frequency). Reinforced canonical framing: **scan pollution**.
- **Gaps:** minor terminology only (counter vs tree, bits vs bytes, decay timing).

## Q4 (L04 Part 8 — failure-mode diagnosis) — three scenarios
- **Grade: Strong**
- (a) Stampede/thundering herd ✓; mitigation background/early refresh ✓.
- (b) Penetration ✓; negative caching sentinel ✓. **Added nuance:** negative caching only helps on repeat same-key misses; for random never-repeating IDs it pollutes cache → bloom filter of existing IDs is stronger. (User didn't mention this; reinforce.)
- (c) Avalanche/cold cache ✓; prime cache + rate-limit DB with inverse relaxation ✓.
- **Gaps:** the random-ID → bloom-filter distinction for penetration.

## Q5 (integrative: L03 durability + L04 Part 5) — leaderboard write pattern
- **Grade: Strong** (one nudge needed on consequence articulation — recurring pattern)
- (a) Write-back ✓, bursty/frequent reasoning ✓. Reinforced the key fit: **coalescing/batching** (100 increments → 1 flush), the real reason for a counter workload.
- (b) Named what's lost (unflushed buffer). Needed a probe to articulate the **consequence**: loss is *permanent/unrecoverable* AND the write was **already acknowledged to client** → zero-loss violation (vs DB lag which self-heals). Got "permanently lost" after nudge.
- (c) Correct: replicate cache (L03 leader+replica); added AOF persistence as alternative.
- **Gaps:** RECURRING — stops at naming the failure; needs prompting to articulate "so what's the consequence / why does it matter." Reinforce ack-before-durable framing.

## Q6 (past-lesson refresher: L03 consistent hashing)
- **Grade: Strong**
- (a) Correct: avoids remapping almost all keys on node add (vs hash%N).
- (b) Correct: ~1/N keys move; reasoning about avoiding all-shard queries good.
- (c) Correct: vnodes = many ring positions per physical node → even arc distribution + **distributed relief on join/leave**. Added: heterogeneous-capacity handling.
- **NOTE:** point B **RESOLVES the L03-Q8 Partial gap** (previously missed that single token relieves only one neighbor; now correctly stated vnodes spread donation across the ring). Regression closed.

## Q7 (L04 Part 3 — caching layers, in-proc vs reverse-proxy trap)
- **Grade: Strong**
- (a) Correct: reverse-proxy hit resolves before app runs (earlier in path / closer to user), not about raw lookup speed. Sharpened: short-circuits so app never executes.
- (b) Correct: N instances = N replicas that drift (L03 divergence). Refinement: pragmatic fix is bound-the-drift (short TTL) or centralize (shared cache), not "keep in sync."
- (c) CDN long-TTL for static images ✓; property = volatility ✓. **Could not produce a short-TTL distributed example** (gave the property but not an instance). Provided: live counter, inventory, stock price, per-user session. Added shareability as second axis.
- **Gaps:** producing concrete examples on demand (had the property, not the instance).

## Q8 (integrative: L01 PACELC/consistency ladder + L04 Part 9) — staleness budget
- **Grade: Strong**
- (a) Correct: ELC axis — Else (no partition) → Latency vs Consistency. Clean.
- (b) Correct: don't cache balance (needs strong consistency, zero staleness budget); if forced, write-through (+ lazy-load for empty-node, + short TTL). Costs: extra infra + 2 writes/write. **Added senior nuance:** for money the authoritative read should hit origin at transaction time (consistent/locking read); cache only for non-critical display, never to authorize a debit.
- (c) Correct: eventual consistency, staleness bounded by TTL.
- **Gaps:** none material; strong grasp of the consistency-per-data-type framing.

## Q9 (L04 Part 6 — TTL dial + jitter)
- **Grade: Strong**
- (a) Too-short extreme: strong (frequent expiry → misses → higher T_avg). **Too-long extreme: needed a nudge** — then gave stale-values (write-around example). Both covered after prompt.
- (b) Correct mechanism (mass simultaneous expiry → load spike) and correct fix (TTL jitter 300+rand(0..60)). **Forgot the name "cache avalanche."**
- (c) Correct after sharpening: TTL backstop protects against missed/lost explicit invalidation, capping staleness at TTL.
- **Gaps:** recalling failure-mode name (avalanche); needed prompting to give the second half of a two-part question (recurring under-answering pattern).
- **Follow-up (user challenge, CORRECT):** user pushed back that long-TTL→stale only holds for cache-aside/write-around, NOT write-through/back (which update cache on every write → fresh regardless of TTL). User is right; my generalization was wrong. Precise rule: long TTL causes staleness *iff freshness depends on expiry*. Under write-through/back, TTL serves memory reclamation + out-of-band-write backstop, not freshness. **User's model is sharp — reinforce, no gap.**

## Q10 (synthesis: L01–L04 — e-commerce product-page read path)
- **Grade: Strong**
- (i) Static images/desc: CDN(+browser), long TTL, write-around + purge/invalidate, DB-first-then-delete (applied Q2 rule unprompted). ✓
- (ii) Price: distributed cache, cache-aside reads, write-through (rare writes, must be fresh), short TTL. **Missed the checkout hook:** must re-validate price against source of truth at checkout (or signed price token) — cache for display, authoritative read for the transaction (same as Q8 balance). Prompted.
- (iii) Counter: distributed cache, write-back, loss-tolerant, eventual consistency ✓. Correctly inverted Q5 durability logic (loss acceptable here). Sharp: write-back key stays warm → immune to expiry stampede (corrected: flush doesn't evict, so no blip).
- Cross-cutting failure: needed a nudge, then named **hot key + stampede** on viral product with correct mitigations (request collapsing/cache lock, background refresh). Rounded out with steady-state hot-key (replicate key, in-process cache for hottest keys).
- **Gaps:** the checkout authoritative-read hook; needed prompting for the cross-cutting failure (recurring under-answering).

---

## Lesson 04 Summary — Overall: STRONG (9 Strong / 1 Partial)
- **Grade distribution:** Q1 Partial; Q2–Q10 Strong.
- **Standout:** two follow-up challenges where the USER corrected ME and was right — (1) Q2: residual DB-first race is also durable poisoning (probability not durability); (2) Q9: long-TTL→stale only applies when freshness depends on expiry, not write-through/back. Deep, correct mechanism-level understanding.
- **Resolved from L03:** consistent-hashing vnode donation (Q6) — the L03-Q8 Partial gap is closed.
- **Recurring pattern (CONFIRMED again):** tends to stop at *naming* a failure/answer and needs a nudge to (a) articulate the consequence/"why it matters," and (b) complete the second half of multi-part questions. Improving — self-corrects well once prompted. Keep re-asking "so what's the consequence?"
- **Minor:** part-of-whole arithmetic slip (Q1: 12/150=8% not 12.5%); forgot failure-mode name "avalanche" (Q9, had mechanism); terminology on Morris counter (Q3, bits not bytes, counter not tree).
- **Areas fully solid:** write patterns + durability trade-offs, PACELC/consistency-per-data-type, eviction mechanisms, write-ordering, layered caching, per-data-type design.
