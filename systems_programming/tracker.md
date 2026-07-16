# Systems Programming — Study Tracker

**Goal:** build **single-machine concurrency in C# from scratch** to an **L5 bar** for the Databricks **Systems Programming** round. 12-lesson curriculum ([study/00-index.md](study/00-index.md)), driven by [../full_loop/study-plan.md](../full_loop/study-plan.md) (§2, §5). This is the **~60% effort** track — the one true gap.

**Window:** start **Tue 2026-07-14** → concurrency lessons land across **Weeks 1–3** (Jul 14 – Aug 3), mocks Week 4 (Aug 4–6), **loop Fri 2026-08-07**. Pace is **not** one-a-day; it's *lessons interleaved with algo upkeep, coding-round drills, behavioral, and weekend builds/mocks* per the week-by-week plan. Checkpoint every **Sunday** — if concurrency comprehension lags, extend the date rather than cram.

Each lesson: **written to full depth (sourced) → read → run the demo → Self-check**; next session opens with an **open-ended quiz on the prior lesson** (spaced recall). Grades → [grades/](grades).

**Lesson states:** `planned` → `written` (drafted to depth, sourced) → `studied` (read + demo run + self-check) → `quizzed ✓` (recalled next session).

Date format: `YYYY-MM-DD`.

---

## Week plan (from study-plan.md §5)

| Week | Dates | Lessons | Other tracks running alongside |
|------|-------|---------|-------------------------------|
| **W1** | Jul 14 – Jul 20 | **01–04** (threads, races, locks, memory model) | draft 6–8 STAR stories · algo (4 warm-up rounds + 4 cold) · math: bit manip · 2 coding-round builds |
| **W2** | Jul 21 – Jul 27 | 05–08 (condition sync, primitives, concurrent structures, pools) | algo upkeep · math: number theory · 2 coding builds · behavioral rehearse · Mock #1 |
| **W3** | Jul 28 – Aug 3 | 09–12 (thread-safe cache ⭐, durability, I/O, rate limiter) | algo upkeep · math: combinatorics/prob · 2 coding builds · Mock #2 + #3 |
| **W4** | Aug 4 – Aug 6 | mocks + patch weak spots + taper | 2 full mock loops · algo (recycle) · Thu = light taper |

---

## Lesson progress

| # | Lesson | Ships (C# demo) | Written | Studied | Quizzed | Notes |
|---|--------|-----------------|---------|---------|---------|-------|
| 01 | Threads & execution model (process vs thread, Thread/Task/pool, scheduling, context switch, concurrency vs parallelism, why concurrency) | `01-threads` | ✅ v1.0 (07-14) | ✅ 07-14 | ✅ 07-15 **Strong-to-Solid** (4 Strong/5 Partial/0 Weak) | Taught 07-14; quizzed 07-15 (10 Qs → [grades](grades/01-threads-execution-model.md)). Re-drill: race≠deadlock, locals-private-via-stack, 3 costs+one-liner, ManagedThreadId consequence, latency-ladder units+payoff. |
| 02 | Shared state & race conditions | `02-races` | ✅ v1.0 (07-15) | ✅ 07-15 (demo run) | ✅ 07-16 **Solid** (5 Strong/5 Partial/0 Weak) | Written to full depth, sourced (MS Learn race-conditions [S1]; C# spec §9.6 atomicity [S2]). First lesson in the **new reframe**: concept-first, pseudocode primitives, 4 race-shapes + fix-landscape tradeoffs, diagrams. Demo `02-races`: 20/20 racy runs lost updates, 20/20 atomic runs exact. Quizzed 07-16 (10 Qs, integrative w/ L01 → [grades](grades/02-shared-state-races.md)). Re-drill: precise critical-section def, SILENT corruption as headline danger, confinement=often-best fix, lock granularity trap, block-on-lock→2 context switches. |
| 03 | Mutual exclusion & locks (+ deadlock) | `03-locks` | ✅ v1.0 (07-16) | ✅ 07-16 (demo run) | — | Written to full depth, sourced ([S1] MS Learn `lock` statement, [S2] sync primitives overview, [S3] Coffman deadlock conditions). Covers: lock = mutual exclusion (acquire/release, safety+liveness, reentrancy), cost model (blocking=deschedule/2 context switches vs spinlock=busy-wait), granularity (coarse vs striped), 5 lock design rules (SRP/private lock/hold briefly/no foreign calls/lock reads), deadlock + 4 Coffman conditions, lock ordering + TryEnter + coarsen, livelock/starvation. Demo `03-locks`: locked counter 20/20 exact; opposite-order deadlock detected via watchdog, consistent-order completes. **Taught 07-16.** Quiz next session. |
| 04 | Memory model & visibility (volatile, barriers, Interlocked/CAS) | planned | — | — | — | planned |
| 05 | Condition synchronization (producer–consumer, async logger) | planned | — | — | — | planned |
| 06 | Higher-level primitives (Semaphore, RW-lock, Barrier…) | planned | — | — | — | planned |
| 07 | Concurrent data structures (striping, lock-free/CAS) | planned | — | — | — | planned |
| 08 | Thread pools & task scheduling (async/await model) | planned | — | — | — | planned |
| 09 | ⭐ Thread-safe cache (LRU/LFU + TTL, striping) | planned | — | — | — | planned — the canonical Q |
| 10 | Durability & crash-safety (WAL, fsync, "power goes down") | planned | — | — | — | planned |
| 11 | Disk & network I/O (buffering, batching, backpressure) | planned | — | — | — | planned |
| 12 | Interview integration (rate limiter, SRP/cohesion/coupling, trade-off narration) | planned | — | — | — | planned |

---

## Log

- **2026-07-14** — Track scaffolded. Created `study/00-index.md`, `study/01-threads-execution-model.md` (v1.0, full depth + Sources appendix), `tracker.md`, `grades/`. **Lesson 01 written and taught.** Verified facts live against MS Learn (threads-and-threading, managed-thread-pool, Thread class, TPL) — cited [S1]–[S4],[S7]; latency figures flagged as approximate. Demo `01-threads` added to the harness (spin up threads → observe non-deterministic interleaving + Join). **Next session:** open with a spoken quiz on Lesson 01, then write + teach Lesson 02 (shared state & races).
- **2026-07-15** — **Lesson 01 quizzed** (spaced recall, 10 open-ended Qs) → graded to [grades/01-threads-execution-model.md](grades/01-threads-execution-model.md). **Overall Strong-to-Solid: 4 Strong / 5 Partial / 0 Weak.** Strong on context-switch cost model, Thread/pool/Task mapping, background lifecycle, demo mental model; deep engagement (challenged framings, sharp await-vs-block & Join-vs-liveness follow-ups). Partial gaps to re-touch: race≠deadlock; locals-private-because-of-per-thread-stack (not allocation timing); the 3 costs + senior one-liner (+ "concurrency only speeds up work that *waits*"); ManagedThreadId = managed-only identity, no OS assumptions; latency-ladder unit slips + the "waiting thread steps aside" payoff. Recurring pattern confirmed (matches system_design grades): correct conclusion fast, under-explains the consequence half. **Next: write + teach Lesson 02 (shared state & races).**
- **2026-07-15** — **Teaching REFRAME adopted** (see repo memory `systems-programming-study-style.md`; persisted into `study/00-index.md` + `../full_loop/study-plan.md`). Round = pseudocode-in-room (Alex email), graded on reliable/fast/scalable + OO design (SRP/cohesion/coupling). → Lessons now concept-first, primitives in generic pseudocode, design/tradeoff framing, C# demoted to optional watch-it-happen harness (L02/03/04/09 only). Individual lesson rows in index + study-plan re-aligned to generic primitive names.
- **2026-07-15** — **Lesson 02 written + taught** (first lesson under the reframe). `study/02-shared-state-races.md` v1.0, full depth + Sources appendix ([S1] MS Learn race conditions, [S2] C# spec §9.6 atomicity). Covers: shared+mutable+concurrent triple, `count++` lost-update walkthrough (with interleaving diagram), critical section / atomicity / interleaving, non-determinism & Heisenbugs, the **4 race shapes** (RMW, check-then-act, torn read, multi-var invariant), and the **fix landscape** (mutual exclusion / atomic-CAS / immutability / confinement) with a tradeoffs table. Demo `02-races` added to harness (uses existing `SurfaceRaces` helper): **20/20 racy runs lost updates, 20/20 atomic runs exact.** **Next session:** quiz Lesson 02 (spaced recall), then write + teach Lesson 03 (mutual exclusion & locks + deadlock).
- **2026-07-16** — **Lesson 02 quizzed** (spaced recall, 10 open-ended Qs, integrative with L01 per the carried-over quiz rule) → graded to [grades/02-shared-state-races.md](grades/02-shared-state-races.md). **Overall Solid: 5 Strong / 5 Partial / 0 Weak.** Strong on race legs+fixes, lost-update walk & concurrency-not-parallelism, torn-read hardware truth, 4 race shapes + why atomics can't cover multi-field, and race≠deadlock (resolves an L01 gap). **Recurring pattern firmly confirmed again** (L01 + system_design): correct conclusion fast, under-explains the consequence/"so what" half — cost a grade on Q3/Q4/Q8/Q9/Q10. Re-drill before/within L03: (1) precise critical-section definition (shared + mutual-exclusion, not "any non-atomic code"); (2) SILENT corruption as the headline race danger; (3) confinement = often-best fix + full buys/costs incl. deadlock as a lock cost; (4) coarse-vs-fine lock granularity / serialize-the-whole-structure trap; (5) block-on-lock → deschedule → 2 context switches cost model (L01 carry-over). **Next: write + teach Lesson 03 (mutual exclusion & locks + deadlock)** — takes fix-landscape row 1 and builds it.
- **2026-07-16** — **Lesson 03 written + taught.** `study/03-mutual-exclusion-locks.md` v1.0, full depth + Sources appendix ([S1] MS Learn `lock` statement, [S2] sync-primitives overview, [S3] Coffman deadlock conditions — all pulled/verified live this run). Covers: lock = mutual exclusion (acquire/release, at-most-one-holder, safety vs liveness, reentrancy); cost model (blocking = deschedule + ~2 context switches, tied to L01; vs spinlock = busy-wait; when each); lock granularity (coarse bottleneck vs striping, "start coarse, split only where a profiler shows contention"); 5 lock design rules (private lock with its data / same lock for same data / hold briefly / never call foreign code under lock / lock reads too) — the SRP/encapsulation half the round grades; deadlock (liveness fail, two-lock opposite-order walk + diagram); **4 Coffman conditions** + break-any-one; fixes (lock ordering default, TryEnter/timeout, coarsen) w/ tradeoffs; livelock vs starvation vs deadlock vs race. **Deliberately targets the 5 L02 re-drill gaps.** Demo `03-locks` added to harness: locked counter **20/20 exact**; opposite-order **deadlock detected via CountdownEvent watchdog timeout**, consistent-order completes cleanly (verified running). **Next session:** quiz Lesson 03 (spaced recall, integrative w/ L01–L02), then write + teach Lesson 04 (memory model & visibility).
