# Grades — Lesson 01: Threads & the execution model

Quizzed **2026-07-15** (spaced-recall, next-session). Scale: **Strong / Partial / Weak** per question + overall.

---

## Q1 — Define process & thread; 3 things threads share, 2 things private; why sharing is powerful+dangerous
**Grade: Strong**
- Process = running program ✓. Thread private state (stack: locals/params/call chain; context: registers + PC) ✓. Powerful (cheap implicit shared-memory communication) + dangerous (races/coordination) both nailed ✓.
- **Gaps:** (1) missed the **third shared resource = OS resources/handles** (open files, sockets, kernel handles) — named only address space + code. (2) Thread definition slightly imprecise: sharpen to "the **basic unit to which the OS allocates processor time**" (scheduling emphasis), not just "chunk of instructions."

## Q2 — Concurrency vs parallelism; example w/o parallelism; which causes races; why race on 1 core
**Grade: Partial**
- Parallelism = same instant / ≥2 cores ✓. Concurrency-without-parallelism = one core time-slicing ✓.
- **Gaps:** (1) Concurrency definition too narrow — described only "one takes point while another waits"; should be **structure/composition** (independent tasks, overlapping lifetimes, interleaved) per Rob Pike. (2) **Conflated deadlock with race** — gave an A-waits-B / B-waits-A example (that's a **deadlock/liveness** bug) and asked if it's a race. Race = outcome depends on interleaving of **unsynchronized shared-state access** (e.g. `count++`); doesn't require waiting. (3) Missed the key **why-on-one-core** mechanism: scheduler preempts **between any two instructions**, so `count++` (read-modify-write) interleaves and loses an update — no 2nd core needed.
- **RE-DRILL:** race vs deadlock distinction; the read-modify-write interleaving story.

## Q3 — Context switch step-by-step; direct + indirect cost; why oversubscription cuts throughput
**Grade: Strong**
- Direct cost (save/restore + scheduler) ✓, indirect cold-cache cost ✓ (well explained), oversubscription → more time switching than working ✓.
- **Gap (precision):** said the switch "saves the **stack**" — it does **not**. The stack stays in RAM; only the **register snapshot (working regs + PC + SP)** is saved/restored, and the restored **stack pointer** re-points to A's still-in-place stack. Copying a multi-MB stack every switch would be ruinous. Otherwise solid.

## Q4 — Thread vs pool-thread vs Task; which rides pool; bg/fg defaults; canonical raw-Thread case
**Grade: Strong** (facts) — **the "why" was a self-flagged gap**
- All factual mappings correct: Task rides pool ✓; pool threads + Tasks background, raw Thread foreground ✓; raw Thread for long-lived background loop ✓ (canonical = TTL sweeper / logger drain).
- **Gap (understanding):** guessed the reason was "less direct/indirect cost" — **wrong lever.** Real reason = **pool starvation**: the pool is a *bounded* set of threads for *short* work items; a long-lived/blocking job on a pool thread ties up a scarce slot for the app lifetime and can **prevent other queued Tasks from starting**. Dedicated Thread keeps the pool free. (Full docs list for raw Thread: foreground / specific priority / long blocking / STA / stable identity.)
- **RE-DRILL:** "why dedicated Thread for the sweeper" = don't starve the bounded pool, NOT cost.

## Q5 — Task.Run then Main returns w/o await; what happens & why; how to avoid losing work
**Grade: Strong**
- Mechanism correct: Task is background → Main returns → last foreground thread ends → runtime stops all background threads & shuts down → work abandoned ✓. Tied to "runtime alive only while a foreground thread runs" ✓. Await as fix ✓.
- **Gap (misconception):** said "Task can be configured to be foreground" — **false.** Tasks run on pool threads, which are **always background**; no knob. For foreground behavior you must use a raw **Thread** (fg by default). Other avoidance methods missed: **block Main** via `task.Wait()`/`.Result`/`GetAwaiter().GetResult()`/`Task.WhenAll`, or dedicated foreground Thread + `Join()`.
- **RE-DRILL:** pool threads can't be made foreground; the block-Main options.

## Q6 — Two reasons to add threads; map to conc/par; three costs; the senior one-liner
**Grade: Partial**
- Mapping correct: concurrency = overlap waits (helps on 1 core) ✓; parallelism = split CPU-bound across cores ✓.
- **Gaps:** (1) **Missed all three costs** — correctness burden (races/deadlock + non-determinism), overhead (creation/stack/switch+cache), complexity. (2) **Didn't give the one-liner.** (3) Looseness: said concurrency speeds up "**any** work on a single core" — only work that **waits**; pure CPU-bound work on 1 core gets no speedup, just switch overhead.
- **RE-DRILL:** the three costs (esp. non-determinism as the #1 cost) + the senior one-liner ("add concurrency when work waits / is parallelizable, only as much as core count + coordination cost justify; every thread I owe a correctness story").
- Note: good challenge ("is that quote important?") — engaged with the *why* of the framing.

## Side-questions (not graded) — strong conceptual probing
- Asked await-vs-block difference → correctly needed: await = non-blocking (frees thread), block (.Wait/.Result) = holds thread in WaitSleepJoin, can deadlock under a sync context.
- Asked if Join needed to keep process alive → correct instinct: **foreground Thread keeps process alive on its own**; Join is for **synchronization/ordering**, not liveness. (This user catches sloppy generalizations — consistent strength.)

## Q7 — Why not build correctness on Priority; why ManagedThreadId ≠ OS thread id (+ consequence)
**Grade: Partial**
- Priority ✓ — "just a request, OS may not grant it" (docs: "not guaranteed to be honored"). ManagedThreadId = id within managed code, not OS ✓.
- **Gap (the recurring "so what" under-explanation):** gave definitions, **skipped the practical consequence** the Q asked for. Missing: (1) mechanism — host may map **many managed threads → one OS thread** or **move a managed thread between OS threads**, so no stable 1:1. (2) Rule — **use** ManagedThreadId for logging/diagnostics/identity *within managed code*; **don't** use it for OS-level assumptions (correlating to OS thread id, thread affinity, native APIs).
- **PATTERN FLAG (matches system_design notes):** right conclusion fast, under-explains the consequence half. Directive: always append "…and therefore you should/shouldn't ___."

## Q8 — Why locals private but heap/static shared; where each kind of data lives
**Grade: Partial**
- Heap conclusion ✓ — heap/static shared across threads, must coordinate. (Refinement given: it's shared **mutable** state that needs sync, not literally "everything atomic"; immutable/read-only shared data is safe.)
- **Gap (missed the core mechanism):** reached for **compile-time-size / allocation-timing** to explain locals' privacy. The real reason = **each thread has its own STACK**, locals live on the stack, so two threads in the same method get **separate copies** — no shared storage to collide on. (He literally stated "stack is private" in Q1 but didn't connect it here.) Where each lives: local→running thread's own stack (private); `new()`→heap (shared); `static`→shared process-wide storage.
- **RE-DRILL:** the safety fault line = "per-thread stack vs shared heap/static," NOT allocation timing. Connect back to Q1's own stack fact.

## Q9 — Latency ladder ordering + magnitudes; how it justifies concurrency
**Grade: Partial**
- Ordering ✓ (register<L1<RAM<SSD≈net-DC<HDD). Multipliers (×100, ×10,000 from 1ns) roughly right.
- **Gaps:** (1) unit labels slipped ~10–100×: RAM labeled "1 µs" (it's ~100 ns = 0.1 µs); SSD labeled "milliseconds" (it's ~10–100 **µs**; ms is the HDD rung); L1 "tens of ns" is more L2/L3 (L1 ~1 ns). (2) **Missed the payoff half entirely** — didn't say how the ladder justifies concurrency. Answer: disk/net are 10⁴–10⁶× slower than CPU/RAM, so a waiting thread should **get off the CPU (block/yield)**; blocked threads use no CPU → scheduler runs B/C/D → **overlap the wait** = the throughput win (ties to Q6 Reason 1).
- **RE-DRILL:** the "so a waiting thread should step aside" payoff; keep units straight (ns/µs/ms).

## Q10 — Demo's two invariants (ordered vs arbitrary) + mechanisms + why count++ race inevitable
**Grade: Strong**
- Within-thread order preserved (own PC/stack, sequential) ✓; cross-thread interleaving arbitrary (scheduler preempts between instructions) ✓; race inevitable because a thread is cut off mid read-modify-write and another runs oblivious to the intermediate state → lost update ✓. Complete, well-reasoned close.
- Carry-forward phrase: **"sequential within a thread, arbitrary across threads."**

---

## OVERALL — Lesson 01: **Strong-to-Solid** (4 Strong / 5 Partial / 0 Weak)
Strong: Q1, Q3, Q4(facts), Q5, Q10. Partial: Q2, Q6, Q7, Q8, Q9. No Weak.
- **Strengths:** context-switch cost model, Thread/pool/Task mapping, background-thread lifecycle, the demo's mental model. Deep engagement — challenged framings, asked sharp await-vs-block & Join-vs-liveness follow-ups (both resolved correctly).
- **Top re-drill items before Lesson 2:**
  1. **race vs deadlock** (Q2) — distinct bugs; race = interleaved unsynchronized shared-state access, doesn't need waiting.
  2. **locals private because of the per-thread STACK** (Q8), not allocation timing — the exact fault line L2 sits on.
  3. **the three costs of concurrency + the senior one-liner** (Q6); don't say concurrency speeds up "any" work — only work that waits.
  4. **ManagedThreadId consequence** (Q7) — managed-only identity, no OS assumptions; append the "so you should/shouldn't" half.
  5. **latency ladder units + the "waiting thread steps aside" payoff** (Q9).
- **Recurring pattern (confirmed, matches system_design grades):** correct conclusion fast, **under-explains the consequence/"so what"** half. Keep pushing for the second half.
