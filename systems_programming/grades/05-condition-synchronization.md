# Lesson 05 — Condition synchronization (CVs, producer–consumer, async logger) — Quiz grades

Quizzed 2026-07-23 (spaced recall, ran ~2 days late — was owed since L06 taught out of order). 10 open-ended Qs, integrative with L01–L04. Scale: Strong / Partial / Weak. Opened with a 30-sec L04 re-drill (3-layer taxonomy ✓, HB-establishers ✓, Debug-disables-hoist — right direction, sharpened to "hoist not reorder").

**Overall: SOLID — 8 Strong / 2 Partial / 0 Weak** (ties/edges out L04 as strongest to date). Partials = Q7 (async logger: writer=consumer mix-up in part a) and Q8 (`close()` broadcast: consequence-halves thin). Recurring "conclusion-fast, consequence-half-thin" still surfaces but milder; excellent self-driven follow-ups again (poison-pill-vs-close reconciliation; volatile-doesn't-save-you on Q9).

---

## Q1 (L05) — busy-wait cost; single-core failure; one word for what we want
**Grade: Strong** (missed the one-word sub-part).
- (a) Cost — Strong: burns 100% of a core re-checking a condition only another thread can change. ✓
- (b) Single-core — Strong, with a sharp self-catch: corrected "deadlocked → **livelocked**." Coached the precise framing: strictly **starvation** of the producer by the spinner (consumer hogs core; producer never scheduled to make the condition true); livelock defensible (system live, zero progress). Killer line he had: *spinner starves the very thread that would unblock it.*
- (c) — **missed**: the one word is **block** (deschedule to zero CPU, be woken). Recurring skipped-sub-part; supplied.

## Q2 (L05) — three CV ops; the atomic property in wait; bug if not atomic
**Grade: Strong** (name the bug; finish the consequence).
- Three ops correct (signal=wake one, broadcast=wake all, wait=block). Added: `wait` also **re-acquires the lock on wake** before returning.
- Atomic property + interleaving: correct walk (signal fires in unlock→sleep window, wakeup vanishes).
- Two refinements: (1) name it precisely — the **lost wakeup** (he guessed "race?"; it *is* a race but has a name); (2) land the consequence half — *the consumer sleeps forever even though the condition is now true.* Recurring "conclusion-fast, so-what-thin."

## Q3 (L05) — canonical skeleton; why check under lock; what CV "remembers"
**Grade: Strong** (nailed part b, the part flagged not to skip).
- (a) Under lock: got both TOCTOU race on the predicate AND lost-wakeup window. Cleanest framing coached: holding lock across check→wait makes check-and-sleep **atomic w.r.t. the signaler.**
- (b) — Strong: **predicate over shared state = the truth; signal = a nudge / tap to re-check.** Deepest idea in the lesson, stated unprompted. Did not skip it.

## Q4 (L05) — golden rule + all three reasons wait may find condition false
**Grade: Strong.**
- `while` not `if` ✓. All three: (1) **Mesa** (named) — signaled → ready queue, not handed lock; another thread grabs lock first, re-falsifies; (2) **spurious wakeups** (POSIX/Java); (3) **broadcast wakes many for one item**. Unifier coached: *a returning `wait` means "maybe — re-check," not "yes."*

## Q5 (L05) — signal vs broadcast; thundering herd; when signal; one-CV trap + fix
**Grade: Strong** (herd slightly conflated with cache stampede).
- (a) Defs ✓. Thundering herd shape right (all wake, contend the one lock); coached the punchline (N wake, one wins, N−1 re-check false → sleep, burning ctx switches + cache traffic). Careful: the cache-miss-and-rewrite example he reached for is **cache stampede** — a different phenomenon.
- (b) signal correct when waiters **interchangeable/equivalent** + one resource ⇒ one useful wakeup. ✓
- (c) one shared CV forces broadcast (can't target type) + fix = **two CVs (notFull/notEmpty)**. ✓ Added consequence: with signal you'd wake the wrong type → **deadlock-by-lost-wakeup**, not mere inefficiency.

## Q6 (L05) — bounded queue: which CV each signals; why signal not broadcast; backpressure + unbounded risk
**Grade: Strong.**
- (a) put→notEmpty, take→notFull ✓. (b) two CVs → right-type targeting; added "put added exactly one item → wake exactly one → no herd." (c) unbounded → **OOM/crash**; backpressure = throttle producer to consumer rate; mechanism = producer **blocks in put on notFull**, released when a consumer frees a slot.

## Q7 (L05, integrative) — async logger: one writer/many loggers; batching; graceful shutdown; deferred concern
**Grade: Partial** (writer=consumer mix-up; running-flag conceded).
- (a) — **the miss.** Conflated the "writer" with a producer. Corrected: **writer = the single background CONSUMER** that drains the queue and does `writeToDisk`; the "loggers" = many app threads calling `log()` (producers). Serializing buys: no interleaved/corrupt lines, file owned by one thread (no file lock), slow I/O off the hot path. **Re-touch this.**
- (b) Batching — Strong: amortizes high **per-call I/O overhead** (syscall/fsync) across K messages.
- (c) Two shutdown jobs right (refuse producers + let blocked consumer drain-then-exit). Initially over-corrected him on "running flag"; **conceded** — a flag IS needed to refuse writes (the `closed` flag); the narrow point is only that the *blocking drain loop* needs no `while(running)` because `take` returning CLOSED exits it. Naming is flexible; his use-case was right.
- (d) Deferred = **durability** (power-loss: batched ≠ on stable disk, still in OS page cache → gone). He challenged the question's value; engaged — it's a *reported Databricks Q* ("what happens when power goes down") and the seniority signal is flagging fsync/WAL as a separate layer (L10).

## Q8 (L05) — close(): why broadcast not signal; producer re-check after wake
**Grade: Partial** (mechanics right, both consequence-halves thin — recurring).
- (a) broadcast wakes all, signal wouldn't ✓ — but land: (1) with signal, one wakes and **every other blocked consumer/producer is stranded forever**; (2) framing = the textbook *"one state change satisfies MANY distinct waiters"* case that forces broadcast (consumers exit, producers reject).
- (b) re-check **`closed`** ✓ (nudge-to-recheck principle stated) — tighten: `while(full AND not closed)` then `if closed → reject/throw` (not vague "stop all execution"); the *reason* it's while-not-if = it may have woken **for closed, not for room** → two things to re-check.
- Self-driven: asked to reconcile poison-pill vs our CLOSED-return impl; walked him through in-band sentinel (hand-rolled, N pills for N consumers, only stops consumers) vs out-of-band `closed` flag + broadcast (stops all consumers AND rejects producers). He correctly self-corrected his "no-op" model → consumer **exits** on the signal.

## Q9 (L05 + L04, integrative) — visibility on wait-return; the HB edge; why a lock-free peek is unsafe
**Grade: Strong** (part b = his signature self-driven strength).
- (a) **release→acquire HB edge**: signaler's lock release synchronizes-with waiter's re-acquire on wake → L04 unifier (lock = mutex + memory barrier, visibility free). ✓
- (b) Both halves, second unprompted: no lock → no HB → **stale read** (L04 visibility failure); AND even a **volatile** peek only fixes visibility of one field, not check-then-act atomicity → **TOCTOU race**. "volatile ≠ atomic compound action" reached for on his own.

## Q10 (L05, judgment) — block vs drop policy on a full queue; tradeoffs; when each
**Grade: Strong** (gave drop case first; supplied block case when pressed).
- (a) block = protects **completeness**, sacrifices app latency/liveness (coached: backpressure propagates INTO the app → logging can stall request threads); drop = protects liveness/throughput, sacrifices completeness (keep a dropped-counter so you *know*). ✓
- (b) drop → high-volume low-value (debug/trace, metrics, like counts) ✓; block → **money transfers / inventory / audit** — often the only record of a mutation; completeness required for reconciliation/audit/forensics/compliance. Missed the block half until pressed (recurring skipped-sub-part).

---

## Carry-forward to L06 quiz (owed next) / general
- **Re-touch #1: async logger topology** — writer = the single consumer draining the queue; loggers = producers. (Q7a mix-up.)
- **Recurring pattern STILL live (milder):** conclusion-fast, consequence/second-half thin, and skips a sub-part on multi-part Qs (Q1c one-word; Q2 "sleeps forever"; Q8 both consequences; Q10 block half). Keep pressing every missed sub-part + the "so what."
- **Strengths reconfirmed:** self-driven follow-ups (poison-pill reconciliation, volatile-doesn't-save-you), sharp self-corrections (livelock, no-op→exit), challenges sloppy framings (durability-question value) — engage, don't over-simplify.
- **Process (mine):** don't over-index on exact lesson naming when the concept is right (running/closed flag); and when he asks for clarification on a quiz Q, **re-state + clarify without leaking the answer**, hand it back — do not answer it for him. (Both flagged by user this session; corrected.)
