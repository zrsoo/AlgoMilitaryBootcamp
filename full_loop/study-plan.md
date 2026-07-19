# Databricks Full Loop — Study Plan (v0.1)

Companion to [databricks-full-loop.md](databricks-full-loop.md). That doc = *what the loop is* (sourced). This doc = *how we prepare for it* (our plan). Round-requirement facts here trace to `[S1]` (Alex's email) and `[S2]` (your notes) via the loop doc; **scheduling and pedagogy below are our decisions, not sourced facts.**

---

## 0. Parameters (agreed 2026-07-13)

| Parameter | Value |
|---|---|
| **Target loop date** | **Fri 2026-08-07** (loop is 2 days per `[S2]`; treat Aug 7 as anchor, Aug 6 as taper) |
| **Study window** | **Tue 2026-07-14 → Wed 2026-08-05** (23 days), **Thu Aug 6** = light taper |
| **Daily budget** | **2–3 focused hrs/day** (alongside work); more on weekends |
| **Bar** | **Aim L5** — pass with flying colors, strengthen comp leverage |
| **Language** | **Pseudocode in the room** (language-agnostic per `[S1]`); C# used only as an optional demo/harness vehicle, **not** the subject — see the 2026-07-15 reframe in §2 |
| **Biggest gap** | **Concurrency / single-machine systems programming — from scratch** |
| **Q2 (Spark/data-platform design)** | **Assume NOT in scope** — no large-scale/data-platform design round |
| **Q5 (design tool)** | Not critical as long as the systems round is solid |

---

## 1. Strategy: spend where the gap is

The loop is four rounds `[S1]`,`[S2]`. Our starting position differs wildly across them, so effort is **not** split evenly:

| Round | Starting position | Effort weight | Why |
|---|---|---|---|
| **Systems Programming** (single-machine concurrency) | **Weak, from scratch** | **~60%** | The one true gap; highest risk; the L5 differentiator |
| **Coding** (clean, complete, tested) | Good algo base; needs code-quality/testing polish | **~15%** | Rewards finished, debugged, well-structured code — a *different* skill from LeetCode speed |
| **Algorithms** (LeetCode) | **Strong** (existing bootcamp, many graduated) | **~15%** | Active upkeep — 4 warm-up rounds (16 one-liners) + 4 cold re-solves/week so it stays *live*, not just "warm" |
| **Behavioral** (STAR + culture) | Unprepared but low-complexity | **~15%** | Front-load story drafting, then rehearse; cheap insurance for an L5 "yes" |

**Key redirect:** the large-scale `system_design/` curriculum (8 of 12 lessons remaining) is **paused** — it does *not* target this loop (single-machine, not distributed). The lessons already done still pay off: **the Caching lesson (finished 07-12/13) feeds directly into the thread-safe-cache systems question.** Resume the distributed SD track *after* the loop if desired.

---

## 2. The Systems Programming curriculum (concurrency, C#) — the core

A 12-lesson track mirroring the `system_design/` study style (verbose, from-zero, worked examples, self-check, **and the same sourcing directive** — when each lesson is written, facts get cited + a Sources appendix; nothing from memory). Proposed home: a new **`systems_programming/`** track (see [Section 6](#6-proposed-workspace-structure)).

> **⚠️ Teaching reframe (2026-07-15), from Lesson 2 on.** The round is language-agnostic and the candidate **writes pseudocode in the room, not C#** (email `[S1]`: *"an actual pseudocode implementation that is reliable, fast, scalable … not expected to write compiling code … but ready to write sufficiently specific code to have a conversation around tradeoffs, techniques, solutions … structuring of classes, strong cohesion & weak coupling, separation of concerns, SRP"*). Consequences for how lessons are taught:
> - **Concept-first, primitives in generic pseudocode** (`lock(m){}`, `wait(cv)`, `signal()`, CAS) — not `System.Threading` APIs. Lesson 1 over-invested in C# quirks; those are demoted to short "language notes."
> - **"Sufficiently specific pseudocode"** = the middle between system-design hand-waving and compiling code: real class structure + method bodies where the synchronization lives, arguable line-by-line, but syntax/compilation irrelevant.
> - **Framed as design + tradeoffs**, around the three graded axes **reliable / fast / scalable**, with **OO design quality (SRP, cohesion/coupling, separation of concerns) woven through every lesson**, not saved for Lesson 12.
> - **The C# "Ships" column below is now optional** — a *watch-it-happen* harness kept only for the ~4 lessons where seeing non-determinism matters (races L2, deadlock L3, memory model L4, thread-safe cache L9). `async`/`await` is one axis (L8), not the spine.

| # | Lesson | Core content | Optional demo (watch-it-happen) |
|---|---|---|---|
| 1 | **Threads & the execution model** | process vs thread, `Thread` vs `Task`, scheduling, context switches, cores, why concurrency (throughput/latency) | spin up threads, observe interleaving |
| 2 | **Shared state & race conditions** | data race, critical section, atomicity, the `count++` race, non-determinism | reproduce a race, then a lost update |
| 3 | **Mutual exclusion & locks** | mutex/lock, spinlock, granularity, contention; **deadlock** (4 conditions), livelock, lock ordering | fix Lesson 2's race; force + fix a deadlock |
| 4 | **Memory model & visibility** | instruction/memory reordering, visibility, memory barriers/fences, atomic read-write, compare-and-swap (CAS), why locks give visibility too | atomic counter; visible stop-flag |
| 5 | **Condition synchronization** | condition variables (wait / signal / broadcast), spurious wakeups, **producer–consumer** | bounded blocking queue → **async logger draining a message queue** `[S7]` |
| 6 | **Higher-level primitives** | semaphore, latch / manual-reset event, countdown latch, barrier, read-write lock | rate-of-N gate; reader-writer map |
| 7 | **Concurrent data structures** | thread-safe queue/map, **lock-free basics** (Treiber stack, CAS loop, ABA), lock striping/sharding | striped-lock hashmap; CAS stack |
| 8 | **Thread pools & task scheduling** | pool model, work queues, blocking vs async, oversubscription, `async/await` as a model | fixed pool + work queue |
| 9 | **Thread-safe cache** ⭐ (the canonical Q `[S1]`) | LRU/LFU **under concurrency**, lock striping/sharding, eviction, **TTL expiry thread**, read-heavy tuning — *builds on the Caching lesson* | concurrent LRU cache w/ TTL |
| 10 | **Durability & crash-safety** | write-ahead log, `fsync`/flush, atomic rename, crash recovery, **"power goes down"** `[S1]`, idempotency, checkpoints | append-only log + recovery |
| 11 | **Disk & network I/O** | blocking vs non-blocking, buffering, batching, backpressure, OS page cache, sequential vs random | batched writer w/ backpressure |
| 12 | **Interview integration** | the **rate limiter** (token bucket, thread-safe) `[S1]`,`[S3]`; driving the solution; handling **added requirements mid-flight** `[S1]`; class design — SRP, **strong cohesion / weak coupling**, separation of concerns `[S1]`; trade-off narration | thread-safe token-bucket limiter |

Each lesson: **learn (concept-first, pseudocode) → optional watch-it-happen demo → short self-check**. Lesson 9 and 12 double as full mock systems-programming problems. The **"efficient logger that processes messages in a queue"** — a reported Databricks concurrency question `[S7]`, considered particularly challenging — is built in Lesson 5 (bounded blocking queue) and revisited under load/failure in Lesson 12.

---

## 3. Behavioral prep (STAR + Databricks culture)

The email is explicit: **use the STAR method** `[S1]`, and work the **six culture principles** into answers `[S1]`,`[S4]` (customer obsessed · raise the bar · truth seeking · first principles · bias for action · company first).

**Deliverable:** ~**6–8 polished STAR stories**, each tagged to (a) the named prompts and (b) 1–2 culture principles. Target prompts from `[S1]`,`[S2]`:
- Disagreement with a teammate → resolution (Collaboration)
- Executing under varying stakeholder priorities (Collaboration)
- A project end-to-end + how you measured success (Ownership)
- A project set to miss a deadline → how you course-corrected (Ownership + bias for action)
- Something that went wrong → what you learned / would change (Growth)
- Feedback you received and applied (Growth)
- **Why Databricks** / why you left a prior company (Interest/Career)

**Cadence:** draft all stories in Week 1 (batched, ~20 min/day), rehearse aloud 2×/week Weeks 2–3, final polish Week 4. Also (from `[S1]` tips): line up the **3 references** `[S2]` early; skim a Databricks codebase (Delta Lake) and the culture page to cite specifics.

---

## 4. Algorithms upkeep, math & coding-round drills

- **Algorithms (active upkeep — EVERY week, all 4 weeks):**
  - **4 warm-up rounds/week — each round = 4 one-liners → 16 recognitions/week.** I state a one-line problem, you name the **tool + how to solve** it (no coding). Each round is mixed across families. **Any miss (wrong/uncertain tool) → drop it and cold-solve that problem immediately** (the recognition-triage loop). W1–W3 draw from a 48-problem fresh pool (16/wk); W4 recycles misses + spot-checks.
  - **4 cold re-solves/week** — **timed and compiled** (runs + tests + complexity narration, mirroring the real round `[S1]`), from a separate high-value list. Warm-up-miss re-solves count toward the 4.
  - Rationale: 1+1 lets it go stale; this keeps recall *live* under an L5 bar. Sync marks per the usual calendar/problem-set convention.
- **Math touch (small, standalone — `[S1]` explicitly names "some math concepts"):** three ~25-min reviews, one per week W1–W3, because your 84 banked problems contain almost no pure-math (genuinely uncovered surface):
  - **W1 — bit manipulation** (XOR tricks, masks, single-number, bit counting) + integer-overflow habits.
  - **W2 — number theory** (GCD/LCM via Euclid, Sieve of Eratosthenes, modular arithmetic, fast exponentiation `pow(x,n)`).
  - **W3 — combinatorics & probability basics** (nCr / permutation counting, expected value, **reservoir sampling** — which also feeds the streaming/systems round) + a few classic number problems (reverse integer, integer `sqrt` via binary search, happy number, Roman↔int).
  - Fold **one math one-liner into a warm-up round each week** for recognition. **Not a workstream — first thing to compress if time is tight.**
- **Coding round (deliberate practice — 2×/week, all 4 weeks; 8 problems total):** a **dedicated pool of implementation-heavy problems** — variable-sized games (e.g. tic-tac-toe), grid simulations, bit-level parsing (e.g. IP-to-CIDR), and graph/DFS builds — **separate from the algo pool**. This mirrors what candidates actually report for the Databricks Coding round `[S7]`,`[S8]`: LeetCode medium/hard **implementation** judged on execution quality, not a novel trick. Run each in **coding-round mode**:
  1. **Complete + compiling first** (a working solution before cleverness).
  2. **Always reach the test phase** — real edge-case tests, not eyeballing (candidates are dinged for finishing code but not testing `[S8]`).
  3. **Rehearse the follow-up** — force an optimization or an **ordering/duplicate-handling variant** and handle it cleanly (the reported failure point `[S8]`).
  4. **Clean structure** — SRP, naming, cohesion/coupling `[S1]`, narrated.
  The 8-problem pool + weekly schedule are **sealed** (kept cold, like the algo pool). Implement in the C# harness or a scratch project.

---

## 5. Week-by-week schedule

> Weekdays ≈ 2–3 hrs; weekends heavier (bigger builds + a mock). Lessons = Systems Programming curriculum in §2.

### Week 1 — Concurrency fundamentals (Tue Jul 14 – Sun Jul 20)
- **Lessons 1–4** (threads, races, locks, memory model).
- **Behavioral:** draft all 6–8 STAR stories.
- **Algo:** 4 warm-up rounds (16 one-liners) + 4 timed/compiled cold re-solves (miss → immediate re-solve).
- **Math touch:** bit manipulation (XOR/masks, single-number, bit counting) + integer-overflow habits.
- **Coding round:** 2× implementation-heavy builds in coding-round mode (complete → test → follow-up variant).
- **Setup (Jul 14):** create `systems_programming/` track + C# concurrency harness.
- **Weekend build:** re-implement the race→lock→deadlock trio cleanly; first behavioral read-through aloud.

### Week 2 — Synchronization & structures (Mon Jul 21 – Sun Jul 27)

> **↻ Re-scope 2026-07-19 (actual carry-forward).** W1 closed with **Algo ✓ and Coding ✓** done, and Systems Programming **Lessons 1–3** written/taught/quizzed — but **Lesson 4 slipped** and the **STAR stories were never drafted** (they were a W1 task). So the real, committed W2 list is **exactly three items**:
> 1. **Systems Programming Lesson 4** — Memory model & visibility (write + teach).
> 2. **The 6–8 STAR stories** → `full_loop/behavioral-stories.md` (carried over from W1).
> 3. **Math touch #2** — number theory (GCD/LCM via Euclid, Sieve, modular arithmetic, fast exponentiation `pow(x,n)`).
>
> The original W2 targets below (Lessons 5–8, mock, etc.) now shift right — treat them as the *stretch* plan, not the commitment. Lessons 4→ pushes the whole curriculum back one slot; re-baseline at the Sunday checkpoint.

*Original target (pre-slip):*
- **Lessons 5–8** (condition sync, primitives, concurrent structures, pools).
- **Algo:** 4 warm-up rounds (16 one-liners) + 4 timed/compiled cold re-solves.
- **Math touch:** number theory (GCD/LCM via Euclid, Sieve, modular arithmetic, fast exponentiation `pow(x,n)`).
- **Coding round:** 2× implementation-heavy builds in coding-round mode (complete → test → follow-up variant).
- **Behavioral:** rehearse 2× (record/critique).
- **Weekend build:** producer–consumer + striped-lock hashmap; **Mock #1** (a Lesson 5–7 problem end-to-end, narrated).

### Week 3 — Systems integration & failure modes (Mon Jul 28 – Sun Aug 3)
- **Lessons 9–12** (thread-safe cache, durability/power-loss, I/O, rate limiter + class design).
- **Algo:** 4 warm-up rounds (16 one-liners) + 4 timed/compiled cold re-solves.
- **Math touch:** combinatorics & probability (nCr / permutation counting, expected value, reservoir sampling) + classic number problems (reverse integer, integer `sqrt`, happy number, Roman↔int).
- **Coding round:** 2× implementation-heavy builds in coding-round mode (complete → test → follow-up variant).
- **Behavioral:** polish; confirm 3 references.
- **Weekend:** **Mock #2** (full thread-safe cache, incl. an interviewer-style "now add X / now power dies" curveball `[S1]`); **Mock #3** (rate limiter).

### Week 4 — Mocks, patching, taper (Mon Aug 4 – Thu Aug 6)
- **Mon–Tue:** 2 full **mock loops** (1 systems + 1 coding + behavioral run-through each); log weak spots. *(The final 2 coding-round problems from the sealed pool run as the coding leg of these mocks.)*
- **Algo:** 4 warm-up rounds (recycle misses + spot-checks) + 4 timed/compiled cold re-solves (front-load Mon–Wed so Thu stays a taper).
- **Wed:** patch the top 2–3 weak spots only.
- **Thu Aug 6:** light review, re-read own cheat-sheets, rest. **No new material.**
- **Fri Aug 7:** loop.

---

## 6. Proposed workspace structure

Mirror the existing tracks (create when we start, Jul 14):

```
systems_programming/
  tracker.md                 # schedule + status + log (like system_design/tracker.md)
  study/
    00-index.md
    01-threads-execution-model.md   … 12-interview-integration.md
  grades/                    # quiz grades per lesson (same convention)
  code/                      # C# implementations (own .csproj, net9.0) — the "ships" column
full_loop/
  databricks-full-loop.md    # (exists) the loop map
  study-plan.md              # (this file)
  behavioral-stories.md      # STAR stories tagged to prompts + culture principles
```
- Same **sourcing directive** and **quiz/grades** conventions as `system_design/`.
- `system_design/` track: **paused** (resume post-loop); Caching lesson already feeds Lesson 9.

---

## 7. Readiness checklist (the L5 bar)

By Aug 6 you should be able to, cold:
- [ ] Explain and reproduce a **data race**, then fix it with the *right* primitive (not just `lock` everything) and justify granularity `[S1]`.
- [ ] Implement a **bounded blocking queue** (producer–consumer) with condition variables from scratch.
- [ ] Design + implement a **thread-safe cache with eviction + TTL**, and scale it (lock striping) — the canonical question `[S1]`.
- [ ] Implement a **thread-safe rate limiter** (token bucket) `[S1]`,`[S3]`.
- [ ] Answer **"how do you survive power loss / crash?"** with WAL + fsync + atomic rename + recovery `[S1]`.
- [ ] Reason about **deadlock** (4 conditions) and prevention on the spot.
- [ ] Narrate **trade-offs** and structure code with **SRP / cohesion / coupling** `[S1]`.
- [ ] Deliver **6–8 STAR stories** mapped to culture principles `[S1]`,`[S4]`; 3 references confirmed `[S2]`.
- [ ] Comfortable with the **math touch** basics — bit tricks, GCD/modular/fast-pow, nCr & expected value, reservoir sampling `[S1]`.

---

## 8. Risks & adjustments

- **2–3 hrs/day is tight for concurrency-from-scratch to an L5 bar** — and the firm **algo upkeep (4 warm-up rounds ≈ 16 one-liners + 4 cold re-solves/week, ~3–4 hrs/week)** compresses the concurrency slice further. Mitigations: (1) weekends carry the heavier builds/mocks; (2) if Week 2 slips, drop Lessons 8 and 11 to "skim" (lower yield) to protect Lessons 5, 9, 10, 12; (3) the date can move — you said Aug 7 is a target, not a hard wall.
- **Algo is now active upkeep, not passive maintenance** (your call — 1+1 would go stale). It's capped at 4 rounds + 4 cold/week; resist letting it creep past that and eat concurrency time, since algo is already your strength.
- **Behavioral is cheap insurance** — a weak behavioral can cap an L5 offer; keep it on-track, don't let it slide to the final week.
- **Checkpoint every Sunday:** re-grade the plan vs reality in `systems_programming/tracker.md`; if concurrency comprehension lags, extend the date rather than cram.

---

## 9. Open items before Jul 14

- Confirm loop logistics when the **availability email** arrives `[S1]` (exact dates, round order, remote/onsite, any diagram tool).
- Decide: single `systems_programming/code` project vs per-lesson files (recommend one project, per-lesson folders).
- Line up the **3 references** `[S2]`.

*Next action (Jul 14): scaffold the `systems_programming/` track + C# harness, write Lesson 1, and draft the behavioral story list.*
