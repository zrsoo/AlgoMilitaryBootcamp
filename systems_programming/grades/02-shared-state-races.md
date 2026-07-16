# Lesson 02 — Shared state & race conditions — Quiz grades

Quizzed 2026-07-16 (spaced recall). 10 open-ended Qs, integrative with L01 where possible. Scale: Strong / Partial / Weak.

---

## Q1 (L02 + L01 memory model) — three race legs + fixes; why local is safe; why ≥1 writer
**Grade: Strong.**
- Named all three legs with correct fixes: concurrent write access → mutual exclusion; shared → confinement/isolation; mutable → immutability. Noted (correctly) the legs are interrelated.
- Local safety: correct — lives on thread's private stack, per-thread stack pointer in context → breaks the SHARED leg. Clean L01 integration.
- **Gap (minor):** the "≥1 writer" justification drifted into the stale-read (RMW) story rather than the general principle: reading changes nothing, so outcome is order-independent unless someone writes → no writer, no race. Reasoning sound but narrower than the principle. Tighten articulation.

## Q2 (L02 + L01 concurrency-vs-parallelism) — define race; walk count++ lost update; single-core repro
**Grade: Strong.**
- Definition correct: outcome depends on scheduling, non-deterministic, uncontrolled, different results per run.
- count++ = load/increment/write; interleaving walk correct (A reads 5→6, preempted, B reads stale 5→6→writes, A writes 6 → final 6). Correctly pinpointed crux: B read 5 before A wrote back → both compute from same stale value → one increment lost.
- Concurrency-vs-parallelism nailed: concurrency/interleaving is the cause, reproduces single-core via time-slicing. Strong L01 integration.
- No gaps.

## Q3 (L02) — define critical section & atomic; why count++ not atomic; general cure
**Grade: Partial.**
- Atomic: Strong — "transactional," from other threads' view either not-begun or complete, no observable in-between.
- Why count++ not atomic + halfway state + B's exploit: Strong — B observed loaded-and-incremented-but-not-stored (memory still 5, B read stale 5).
- General cure = atomicity: correct (mechanism-agnostic).
- **Gap:** critical-section DEFINITION imprecise. Said "any code that is not atomic" — too broad. Missing crux: must access SHARED state AND require mutual exclusion (non-atomic code on private locals is NOT a critical section). Slip: called count++ "a single instruction" (one statement, three instructions). Understands mechanism; must state definition precisely for interview.

## Q4 (L02 + L01 cost model) — non-determinism, why more dangerous, Heisenbug
**Grade: Partial.**
- Non-determinism (outcome depends on scheduling), test-slip (bad interleavings a minority → pass on dev, fire in prod), and Heisenbug (debugger serializes/slows → changes timing → bug vanishes): all correct.
- **Gap 1:** "more load/cores → more likely" reasoning shallow ("more executions"). Should be QUALITATIVE: more cores = genuine parallel overlap (not just time-slicing) so the both-inside window opens for real; more contention = more threads on same location simultaneously.
- **Gap 2 (the big one — recurring pattern):** missed the SILENT-corruption "so what." A lost update doesn't crash — no exception/stack trace, just quietly wrong data. Corruption-without-a-signal is the real reason it's worse than an ordinary (loud, reproducible) bug. Gave symptom, not worst consequence. Lead with this in interview.

## Q5 (L02) — torn read on shared 64-bit long
**Grade: Strong.**
- Named torn read/write. Hardware reason correct: 32-bit machine writes long as two 32-bit ops (low/high dword); reader between sees one fresh + one stale half → value never assigned.
- int: no risk, single-shot word write. Bonus nuance correct: 64-bit CPU usually one-shot but spec doesn't guarantee (long/double need not be atomic) → don't rely on it. [S2] point.
- Only add (not a gap): atomicity also assumes natural alignment. Otherwise complete.

## Q6 (L02) — four race shapes + tells; which spans multiple fields & why atomic can't cover
**Grade: Strong.**
- All four shapes correct w/ examples + tells: RMW (count++), check-then-act (two threads both see condition true; lazy-init/contains-then-add), torn read/write (wide types/structs), multi-variable invariant (transfer decrement/increment).
- Discriminator correct: Shape 4 spans multiple lines/fields. Reason correct: single hardware atomic (increment/CAS) works on ONE location/word; multi-field invariant touches several independent locations → need a lock to make block mutually exclusive as a unit.
- Cleanest answer of the set. No gaps.

## Q7 (L01 refresher — race vs deadlock) — targets prior Partial gap
**Grade: Strong.**
- Deadlock = threads waiting on each other (circular wait). Key distinction stated cleanly: race keeps running but corrupts data (safety failure); deadlock halts progress / hangs (liveness failure).
- count++ lost update = race; mutual lock-wait hang = deadlock. Correct.
- RESOLVES the L01 Partial gap (race≠deadlock). No gaps.

## Q8 (L02 fix landscape) — four fixes: leg attacked / buys / costs / which is best
**Grade: Partial** (needed a push for the 2nd/3rd parts, then details drifted).
- Leg mapping: all four correct (mutex→concurrent; atomic/CAS→concurrent[not shared]; immutability→mutable; confinement→shared). Immutability buys/costs correct.
- **Gap 1:** named MUTUAL EXCLUSION as "often best"; lesson says CONFINEMENT (cheapest race = one you never create → zero sync/contention/deadlock). "Mutex most versatile" is a valid separate point but ≠ best.
- **Gap 2:** mutual-exclusion cost missed DEADLOCK risk; missed its strength = handles arbitrary multi-line/multi-field critical sections.
- **Gap 3:** atomic/CAS cost wrong ("can't control provider"); real cost = only single small location, can't cover multi-field invariant (Shape 4 — which he nailed in Q6!); CAS retry/ABA subtlety.
- **Gap 4:** confinement cost = design cost (single-owner + hand-off mechanism), not "stack space."
- RECURRING PATTERN again: conclusion (leg map) fast & correct; consequence half (buys/costs) needed push + drifted on tradeoff-relevant details.

## Q9 (L02 applied — spot the race in a get-or-compute cache)
**Grade: Partial.**
- (a) Check-then-act: correct — both pass "not contained" check, both proceed. Strong.
- (b) Wasted work (double expensiveCompute): correct. Correctness DRIFTED: said "expensiveCompute internally racy → stale"; intended bug = double-PUT CLOBBER — if computed values differ, one put overwrites other → two callers get different values, cache holds last-writer (inconsistency). (Also torn put / line-4 get null possible.)
- (c) Fix (lock whole block) correct, but cost generic ("lose speed"). Missed scenario-specific "so what": single coarse lock SERIALIZES whole hot cache incl. unrelated present keys → contention bottleneck, throughput collapse → why you'd stripe/per-key lock or double-checked/future placeholder (L07/L09).
- RECURRING PATTERN: sharp on (a), consequence halves (inconsistency + scalability cost) needed spelling out.

## Q10 (L02 + L01 synthesis — senior narration & over-sync cost)
**Grade: Partial.**
- (a) Atomic-vs-lock decision rule correct (single location→atomic; multi-field→lock). MISSED: confinement-first opener ("cheapest race is one you don't create; does it need to be shared/mutable?") AND granularity (question asked for it explicitly).
- (b) Two harms correct: deadlock + reduced performance/throughput (matches docs).
- **Gap (L01 tie-in — under-explained AGAIN):** "it waits / it's blocked" just restates Q. Intended: blocked thread is DESCHEDULED (running→blocked), context switch to run another; on lock release must be woken→ready→rescheduled = SECOND context switch + cold caches/TLB (L01 context-switch cost model). Real cost = 2 context switches + cold caches + the wait. This is the L01 "waiting thread steps aside isn't free" payoff — a known L01 Partial gap, resurfaced.

---

## Overall — Lesson 02: **Solid (5 Strong / 5 Partial / 0 Weak)**

**Strong on the core mechanics:** race legs + fixes (Q1), lost-update walk & concurrency-not-parallelism (Q2), atomic/torn-read hardware truth (Q5), the four race shapes + why atomics can't cover multi-field (Q6), and race-vs-deadlock — a resolved L01 gap (Q7).

**The recurring pattern is now VERY firmly confirmed** (matches L01 + system_design): gives the correct CONCLUSION fast, under-explains the CONSEQUENCE / "so what" half — and it cost a grade on Q3, Q4, Q8, Q9, Q10. Specifics:
- Q3: critical-section definition loose ("any non-atomic code") — must include SHARED + mutual-exclusion.
- Q4: missed the headline danger = SILENT corruption (no crash/exception); "more cores" reasoning shallow.
- Q8: named mutex (not confinement) as "often best"; buys/costs needed a push and drifted (missed deadlock as a lock cost; wrong atomic cost; wrong confinement cost).
- Q9: correctness outcome drifted (clobber/inconsistency, not "expensiveCompute racy"); missed coarse-lock-serializes-whole-cache scalability cost.
- Q10: missed granularity + confinement-first in the one-liner; L01 tie-in stopped at "it's blocked" instead of descheduling + 2 context switches + cold caches.

**Re-drill before/within L03:** (1) precise critical-section definition; (2) SILENT corruption as the headline race danger; (3) confinement = often-best fix + full buys/costs table incl. deadlock as lock cost; (4) coarse-vs-fine lock granularity & the serialize-the-whole-structure trap (sets up L03/L07/L09); (5) the block-on-lock → deschedule → 2 context switches cost model (L01 carry-over, still not automatic). Strengths to keep engaging: challenges framings, asks sharp follow-ups (e.g. "can atomic attack the shared leg?").

