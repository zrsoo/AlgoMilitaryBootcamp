# Lesson 06 — Higher-level primitives (semaphore, RW-lock, events/latches, countdown, barrier) — Quiz grades

Quizzed 2026-07-23→24 (spaced recall; taught out of order 07-22, quizzed after L05). 10 open-ended Qs, integrative with L01–L05. Scale: Strong / Partial / Weak. Opened with an L05 re-touch (async-logger topology).

**Overall: SOLID — 5 Strong / 5 Partial / 0 Weak** (a notch below L05's 8/2 — L06 is denser, five primitives, and ran late). Strong on the recall/structure Qs (throttle, bounded-queue-from-semaphores, RW-lock, countdown-vs-barrier, pick-the-primitive). Partials cluster in: binary-≠-mutex consequences (Q1), auto-reset lost-signal mechanism (Q5), building a semaphore from lock+CV — lock scoping (Q8), no-FIFO→starvation (Q9), lock striping as the scalable cache answer (Q10).

**⚠️ Warm-up miss (3rd time):** async-logger topology STILL answered backwards — said "writer = producer, loggers = consumers." Correct = **writer is the single CONSUMER draining the queue; loggers are the producers.** Persistent; re-touch every session until it sticks.

---

## Q1 (L06) — semaphore def + invariant; why binary semaphore ≠ mutex (+ 2 consequences)
**Grade: Partial.**
- Definition imprecise: said "allows N threads in the **critical section**." Corrected: a semaphore is a **permit counter** (acquire = decrement/block-at-0, release = increment/wake-one); invariant = **at most N threads past a successful `acquire`**. The "critical section" framing is exactly what the lesson warns against — a throttle gates N *simultaneous* in-flight ops that guard no data (not a CS).
- Ownership difference — **Strong**: binary semaphore has no owner, any thread can release; mutex is owned, only the holder releases ("safer"). ✓
- The two consequences: (1) extra `release` — he said "exception thrown" (that's only the C# `SemaphoreFullException` guardrail *if* maxCount declared); **missed the conceptual headline = count climbs to 2 → two threads silently admitted → mutual-exclusion failure, no crash.** (2) reentrancy — **Strong**: same thread re-`acquire` → self-deadlock; a mutex is reentrant (owner + recursion count). Recurring skipped-sub-part: gave "safer" instead of enumerating both consequences until pressed.

## Q2 (L06 + L03) — throttle: why `release` in `finally`; why holding permit across the call isn't the L03 violation
**Grade: Strong.**
- (a) Skip `finally` → throw skips `release` → **leaked permit**. Coached the consequence half (recurring): leaks **accumulate** → count stuck at 0 → throttle **wedged/outage**.
- (b) Correct instinct (throttling, not mutual exclusion). Enriched: a semaphore **doesn't serialize** (N run simultaneously, no "queue behind the holder" disaster the L03 rule targets — that rule is about a *mutex*); and holding across is **mandatory** (you bound in-flight duration). Caveat: separate brief mutex if you also mutate shared state.

## Q3 (L06 + L05) — bounded queue = 2 semaphores + a mutex
**Grade: Strong** (naming inverted; (a) framing imprecise).
- Code structurally correct (acquire slot → enqueue under mutex → release item; mirror for take; init N and 0).
- **Naming flag (not pedantry — inverts meaning):** he named `queueFull(N)` for what counts **free slots** (=`emptySlots`) and `queueEmpty(0)` for **items ready** (=`fullSlots`) — opposite of what they count. Rename to what they count.
- (a) precision: a semaphore counts the **resource** (slots/items), not "producers/workers that can run"; the "binds messages to N" conclusion is right.
- (b) **Strong**: still need the mutex — semaphore counts but doesn't make enqueue/dequeue atomic; the buffer mutation is the L03 critical section. ✓
- (c) **Strong**: `acquire(emptySlots)` is the backpressure to the producer (full → count 0 → block). ✓

## Q4 (L06 + L04) — RW-lock def + invariant; 2 conditions to beat plain lock; tiny-read tool; writer starvation + fairness
**Grade: Strong** (cleanest of the quiz).
- Invariant: readers **xor** writer. ✓
- Two conditions: reads dominate (90%+) **AND** critical sections long enough. ✓ Tiny reads → **volatile read** (correctly *not* Interlocked — that's the RMW/write family). ✓
- Writer starvation: unbroken reader stream keeps read mode forever → writer never gets exclusive access. Fix = **favor-writers fairness** (once a writer queues, new readers block behind it). ✓

## Q5 (L06) — manual vs auto reset; auto-reset lost-signal condition
**Grade: Partial.**
- (a) Definitions **Strong** (manual = gate, wakes all, stays open; auto = turnstile, wakes one, auto-closes) — but **missed the use cases** ("idk"): manual = one-time "go"/start signal to many; auto = hand off to one worker at a time.
- (b) Got the two load-bearing facts (need ≥2 waiters; two close `set`s → one lost) but the **mechanism was muddled** ("two threads wake and compete; signal before the loser sleeps"). Precise: auto-reset `set` releases exactly one + re-closes; loss = the **second `set` lands while the event is still signaled (before #1 released anyone)** → second set is a no-op ("as if it never happened"). Only bites with ≥2 waiters both needing release.
- Follow-up: he asked "what does *the event is still signaled* mean?" → explained signaled/nonsignaled boolean state + the uncashed-open window. Good instinct.

## Q6 (L06) — countdown latch vs barrier (who waits / reusable / shape + use)
**Grade: Strong** (didn't name the shapes).
- (a) coordinator vs participants ✓; (b) one-shot-per-arming vs cyclic ✓; use cases correct. **Missed naming the shapes** (asked explicitly): countdown = **fork/join** (scatter-gather); barrier = **rendezvous** (lockstep).

## Q7 (L06 recognition) — pick the primitive ×5
**Grade: Strong** (4 clean, (b) tool-right-reason-off).
- (a) semaphore/throttle ✓; (c) manual-reset gate ✓; (d) countdown latch ✓; (e) barrier ✓.
- (b) config map read-heavy → said RW-lock (textbook-acceptable) but **rationale imported from wrong scenario** ("a miss can be long" — that's the L09 *cache*, not a config *map*; a map has no expensive miss). Correct rationale = reads dominate; **senior nuance** = a config get is a *short* read, so **copy-on-write behind a `volatile` immutable reference** (lock-free reads) beats a RW-lock whose edge is marginal for tiny reads.

## Q8 (L06 + L05) — build a counting semaphore from lock + CV
**Grade: Partial** (concepts right; construction has the core L05 bug).
- Call-outs correct: **`while` not `if`** ✓; **`signal` not `broadcast`** ✓ (stated reasoning right, though he wrote `broadcast` in code — reconcile).
- **Construction bug = the L05 rule he knew (L05 Q3):** put check + `wait` **outside** the lock (locked only around `curr++`). Must hold the lock across the **whole** check→wait→claim; `wait(cv,m)` must hold `m` (atomic release-and-sleep), and reading `curr` unlocked reopens the lost-wakeup window.
- "the something that should happen here = idk" → it's **`curr++` AFTER the gate** (claim the permit once there's room), not before. Gave the corrected up-counting skeleton.

## Q9 (L06 + L03) — spin-then-block; no-FIFO / no wake order
**Grade: Partial.**
- (a) **Strong**: spin for short waits to dodge L03 blocking cost (deschedule → ~2 context switches + cold caches) for a nanosecond wait. ✓
- (b) **Wrong concept**: conflated "no FIFO wake order" with the L05 two-conditions-one-CV targeting problem. Correct: no-FIFO = **no guarantee the longest-waiting thread gets the freed permit**; wake order arbitrary → can't rely on fairness/FCFS. Failure name (missed) = **starvation** (L03) — a waiter repeatedly passed over. Build fairness yourself (ticket/queue) if needed.

## Q10 (L06 → L07/L09 design) — why not a single RW-lock on a cache; the scalable design
**Grade: Partial.**
- (a) **Strong**: for a short `get`, RW-lock overhead dominates; specifically every enterRead/exitRead **mutates the shared reader-count = a contended cache line** → readers serialize on the lock's own bookkeeping (cache-line ping-pong). ✓
- (b) **Missed the design**: gave "plain lock or volatile read" (that's the single-*value* answer; a cache is a whole map — can't volatile-read mutable entries). Scalable answer = **lock striping / sharding (concurrent map)**: N independently-locked shards → different keys hit different locks, reads mostly lock-free; the one hot cache line becomes N independent ones. L03 granularity theme; this is where L07 opens.

---

## Carry-forward to L07 (write + teach next) / general
- **Re-touch #1 (HIGH PRIORITY, 3rd miss): async-logger topology** — writer = the single **consumer** draining the queue; loggers = producers. Drill at the L07 opener.
- **Re-touch #2: building primitives from lock+CV** — hold the lock across the entire check→wait→claim; `wait` needs the lock (Q8 dropped the L05 Q3 rule under construction).
- **Re-touch #3: no-FIFO semaphore → starvation** (Q9), distinct from waiter-*type* targeting.
- **Re-touch #4: lock striping / sharding** = scalable cache answer (Q10) — L07 material, so it lands naturally, but flag it up front.
- **Recurring "conclusion-fast, skips a sub-part / consequence-half"** STILL live (Q1 consequences, Q5 use-cases, Q6 shape-names). Keep pressing every sub-part.
- **Strengths:** recall/structure Qs (2,3,4,6,7) fast and clean; self-driven clarifying questions ("what does *still signaled* mean?") — the right instinct, engage them.
- **Process (mine, from L05, held this session):** grade the concept not the label (flagged his inverted semaphore names as a *communication* issue, not marked wrong); when he asks to clarify, re-state + clarify without leaking. Both honored.
