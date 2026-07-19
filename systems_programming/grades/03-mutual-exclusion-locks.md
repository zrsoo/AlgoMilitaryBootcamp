# Lesson 03 — Mutual exclusion & locks (+ deadlock) — Quiz grades

Quizzed 2026-07-19 (spaced recall). 10 open-ended Qs, integrative with L01–L02. Scale: Strong / Partial / Weak.

---

## Q1 (L03 + L04 preview) — lock guarantee; lock vs atomic; safety/liveness & which deadlock violates
**Grade: Partial** (a & b Strong, c needs correction).
- **(a) Strong.** Mutual exclusion — at most one thread (the acquirer) between acquire and release at any instant.
- **(b) Strong.** Atomics target a single memory location; a multi-instruction block isn't atomic because the thread can be preempted mid-block, so it must be guarded to prevent races. Clean lock-vs-atomic distinction.
- **(c) Partial — conflated three things.** Did not cleanly define safety/liveness. Said deadlock "hits liveness by restricting entry to one thread" and listed parallelism/latency/throughput. Corrections: safety = *nothing bad happens* = never two holders (one-thread-in is the lock working as intended, NOT a failure); liveness = *something good eventually happens* = a waiter eventually gets in. Deadlock violates liveness because threads **wait forever** (each holds what the other needs → none progresses/releases → frozen, no CPU). The parallelism/latency/throughput hit = **contention** (perf cost of a *working* lock), a distinct concept from deadlock. Re-drill: don't merge "lock slow under contention" with "lock wedged shut forever."

## Q2 (L03 + L01 cost model) — blocking vs spinlock: behavior, cost, when to pick, spin disaster
**Grade: Strong.**
- **(a) Strong.** Blocking → thread sleeps/descheduled, context switch places another thread on the core. Spinlock → actively loops re-checking the lock, keeps the core busy.
- **(b) Strong.** Blocking cost = context switches (direct) + cold caches (indirect); correctly counted **~2** (one to park + swap another in, one to wake/reschedule when lock frees). Tied cleanly to L01. Minor: stated the spinlock's cost (burns a full core doing nothing) in (a)/(c) rather than (b), but covered it.
- **(c) Strong.** Disaster nailed: spinner and holder on the **same single core** → holder can't run to release → stuck forever. Decision map correct (block when wait long/I/O/expensive; spin when ultra-short + spare cores). Bonus: named the **hybrid** (spin briefly then fall back to blocking, = SpinWait).
- **Follow-up (own question):** asked how block-vs-spin translates to pseudocode (what's guarded / who blocks / blocks-on-what). Resolved: shared state guarded via acquire/release; the LATE thread (not the holder) blocks; blocks on the lock's wait queue (OS parks it until release wakes it). Landed the key reframe himself — *critical-section length drives the choice indirectly via contention*: short → spin, long → block.

## Q3 (L03 + L05 deadlock preview) — granularity; coarse vs striping; start-coarse one-liner; new hazard
**Grade: Partial.**
- **(a) Strong (after self-correction).** Coarse = lock whole cache even for single-key op → buys simplicity/correctness, costs latency/throughput (all ops serialize incl. unrelated reads). Striping: started imprecise ("lock the exact key" = per-key locking) but self-corrected to the fixed **array of N locks, `hash(key) % N`**. Bonus: sophisticated resize/rehash insight — when N changes the key→lock mapping shifts and a lock held across resize can be orphaned.
- **(b) Partial — SKIPPED until nudged.** Then delivered the one-liner + "coarse first = safe/simple, split only on measured contention = no premature optimization." Missing piece added by me: you also don't know WHERE the hot lock is until you profile, so starting fine sprays complexity+deadlock risk on locks that may never contend.
- **(c) Partial — MISLABELED.** Called the new hazard "races more likely" — wrong; striping is equally race-safe done right. Correct answer = **deadlock** (≥2 locks → opposite-order acquire → circular wait; a single coarse lock can't deadlock itself). Fix = global lock ordering (Part 6).
- **Recurring pattern again:** answered only part of a multi-part Q (skipped b), and the consequence/label needed precision (races vs deadlock). User asked me to keep pressing on missed sub-parts — doing so.

## Q4 (L03 + L02 torn read) — four lock design rules; foreign-code hazards; why lock readers
**Grade: Strong.**
- **(a) Strong.** Named all five rules with reasons: lock-lives-with-data-and-private (SRP/encapsulation, avoid locking shared/public objects); same lock for same data; hold briefly; no foreign code under lock; lock reads too when writes non-atomic.
- **(c) Strong.** Lock readers because a non-atomic (e.g. 64-bit long/struct) write can be **torn-read** (L02 Shape 3) mid-update; use a **R/W lock** (many readers or one writer) to keep reads cheap without dropping safety.
- **Needed nudges (recurring pattern):** (1) rule-2 "why" — left as "(why?)" then answered correctly on prompt: two different locks on the same data = threads never wait for each other = no mutual exclusion = race, protection circumvented. (2) (b) asked for \u22652 foreign-code hazards, gave only deadlock; on prompt added the duration hazard (long I/O under lock stalls all waiters). Third (reentry/callback into your class) supplied by me.
## Q5 (L03 + L01/L02 safety-vs-liveness) — define deadlock; two-lock walk; deadlock vs race symptoms
**Grade: Strong.**
- **(a) Strong.** Reciprocal waiting — each thread waits for the lock the other holds.
- **(b) Strong.** Correct opposite-order interleaving walk (both hold first lock, each waits on the other's, neither releases → frozen). Did not recall the `transfer` mapping (asked) — supplied: transfer(X→Y) locks X-then-Y vs transfer(Y→X) locks Y-then-X → same circular wait; fix = fixed global lock order by id.
- **(c) Strong — RESOLVES the recurring L02 silent-corruption gap.** Led with it unprompted: deadlock = liveness fail → process hangs/frozen, no progress, no crash, no CPU; race = safety fail → execution continues as if fine, data silently corrupted, no signal. Clean contrast.

## Q6 (L03) — four Coffman conditions; per-condition fix; which to attack + transfer by-construction
**Grade: Strong.**
- **(a) Strong.** All four named correctly (mutual exclusion, hold-and-wait, no preemption, circular wait).
- **(b) Strong.** Circular wait → global lock ordering, transfer by-construction argument nailed ("X precedes Y so we never acquire in the other order → no cycle"). Try-enter + **jittered** backoff for hold-and-wait/no-preemption. Correctly noted mutual exclusion usually can't be attacked. Additions supplied: hold-and-wait also breakable by acquiring all locks up-front all-or-nothing; attacking mutual exclusion = go lock-free/immutable/confined.
- **(c) Strong.** Correct ladder: ordering first (simplest, correct, keeps parallelism) → try-enter+backoff when locks can't be ordered → coarsen as blunt last resort.

## Q7 (L03) — livelock vs deadlock; starvation; livelock fix; safety/liveness map
**Grade: Partial.**
- **(a) Strong.** Livelock = CPU actively burning, no progress; deadlock = no CPU (blocked), no progress. Precise contrast (same outcome, opposite CPU behavior).
- **(c) Strong.** Jitter/randomized backoff; mechanism = symmetry-breaking (both threads react identically in lockstep → randomize so one wins).
- **(b) Weak/GAP — didn't know starvation** ("I don't think we covered it" — it's in Part 6d). Taught: starvation = one thread perpetually denied a resource/turn while others progress; **livelock is a special case of starvation** (everyone starves).
- **(d) Partial.** Got the structure — race = safety = **odd one out**; deadlock/livelock all liveness — but was unsure on livelock ("I guess") and couldn't place starvation. Full map reinforced: race=safety; deadlock/livelock/starvation=liveness.
- **Re-drill: starvation definition + it being the general case of livelock.**

## Q8 (L03) — reentrancy: definition, self-call scenario, interview one-liner
**Grade: Strong.**
- **(a) Strong.** Reentrant = thread already holding it can re-acquire without blocking; tracks a hold/recursion **count**, releases at 0. Addition: also tracks **owning thread identity** (reentry only for same thread; others still block).
- **(b) Strong.** Reentrant → count bumps, B() proceeds; non-reentrant → B()'s acquire blocks on a lock its own thread holds and can't release → **self-deadlock**.
- **(c) Strong (needed the consequence clause).** Gave the assumption ("we're assuming the lock is reentrant") but stopped there \u2014 recurring pattern. Complete sentence: "...otherwise this self-call would deadlock the thread against itself." He'd already stated the consequence in (b).
- **Follow-up (own question) resolved:** jitter+backoff is PROBABILISTIC (livelock, self-healing, not permanent deadlock since holders release), vs lock ordering which is DETERMINISTIC/by-construction \u2014 reinforced why ordering is preferred and backoff is the fallback.
## Q9 (L03 + L02 check-then-act) — thread-safe get_or_compute cache design
**Grade: Partial (strong reasoning process; needed heavy pushing + blunt final fix).**
- **(a) Strong.** Coarse-lock version correct-but-slow; self-corrected the violated rule from "no foreign code" to **"hold lock as briefly as possible"** (expensive compute runs inside the CS).
- **(b) Strong.** Pull compute out of CS (lock only the cache touch), then stripe on profiled hot keys, then spin since cache ops are short. Correctly conditioned on compute needing no shared state.
- **(c) Partial — initially "I don't see any trade-off"; found it only after a pointed walk.** The tradeoff: moving compute outside the lock = check-then-act, so two threads missing the SAME key both compute → **duplicate/wasted work; thundering herd on a hot expensive key**. Correctly reasoned that block-on-read alone doesn't fix it (both still miss). Landed on **global coarse lock** = blunt (reintroduces holding lock across compute + serializes all keys). Taught the better fixes: **per-key/striped lock** or **in-progress future (single-flight)** to preserve cross-key parallelism; acceptable-to-waste iff compute is pure/idempotent + cheap/rare, must-prevent if side-effecting/exactly-once/stampede. Connected to system-design caching stampede lesson.
- **Idempotency sub-question (his own):** clarified pure fn of key (deterministic, no side effects) → duplicate compute is only wasted CPU, safe; side effects/non-determinism → must be exactly-once.

## Q10 (synthesis L01→L03) — critique "wrap every field in a lock, optimize later"
**Grade: Partial (landed all correct after pushing).**
- **(a) Strong.** Coarse first (correct, no deadlock surface) → profile (don't know which spots are hot) → stripe only contended spots. Disciplined "optimize later."
- **(b) Partial → correct.** First gave only the L01 cost (context switches + cold caches) twice; on push named the distinct **L03 serialization/throughput** cost (hot lock = threads take turns = no parallelism, independent of switch cost). Fix = striping / split data across locks.
- **(c) Partial → correct.** First two examples were "slow" not "wrong"; on push gave the two *correctness* failures: over-locking → **deadlock** (broad lock + nested foreign lock → hang); two different locks on same field → **as if unguarded → race → silent corruption**.
- **Recurring pattern once more:** defaulted to the perf framing, needed pushing to produce the *distinct* second cost and to separate slow-vs-wrong.

---

## Overall: SOLID — 5 Strong / 5 Partial / 0 Weak

**Strengths (Strong):** blocking-vs-spinlock cost model incl. ~2 context switches + the single-core disaster + hybrid (Q2); four lock design rules with reasons + torn-read→R/W lock (Q4); deadlock definition + two-lock walk + **silent-corruption-vs-frozen contrast led unprompted** (Q5, resolves a long-standing L02 gap); four Coffman conditions + per-condition fixes + by-construction lock-ordering (Q6); reentrancy (Q8). Sharp own-questions throughout: block-vs-spin→pseudocode, jitter PROBABILISTIC vs ordering DETERMINISTIC, striping resize-orphan hazard, idempotency of compute. Strong self-correction habit (per-key vs striping; foreign-code vs hold-briefly rule).

**Recurring pattern — STILL LIVE (flag):** "conclusion-fast, second-half-thin / answers only part of a multi-part Q." Skipped sub-parts on Q3(b), Q4(rule-2 why + 2nd hazard), and defaulted to perf framing on Q10 until pushed. Per user directive this session, I pressed every missed sub-part. Progress: the *silent-corruption* consequence (the poster-child of this pattern from L02) is now automatic.

**Gaps to re-drill before L04:**
1. **Safety vs liveness precise defs** (Q1): safety = never two holders; liveness = a waiter eventually gets in; deadlock = liveness fail (wait forever) — NOT "restricting entry to one thread."
2. **Deadlock vs contention** — keep distinct: contention = a *working* lock is slow (serialization); deadlock = lock *wedged forever*. (Conflated in Q1, mislabeled striping's new hazard as "races" in Q3 — correct = deadlock.)
3. **Starvation** (Q7): one thread perpetually denied its turn while others progress; livelock is the special case where everyone starves. (Didn't know it.)
4. **Cache stampede / single-flight** (Q9): moving compute outside the lock → duplicate compute on a hot key; fix with **per-key lock or in-progress future**, not a global coarse lock.

**Recommendation:** L03 comfortably passed. Proceed to Lesson 04 (memory model & visibility); open its quiz with a 1-min refresher on the 4 re-drill items above (esp. safety/liveness defs + starvation).