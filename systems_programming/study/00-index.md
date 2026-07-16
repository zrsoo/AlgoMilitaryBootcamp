# Systems Programming — Study Curriculum (Concurrency, C#)

Self-contained explainers to build **single-machine concurrency from scratch** — the one true gap for the Databricks loop's **Systems Programming** round (thread-safe cache, rate limiter, durability/"power goes down", synchronization). Read in order; each is standalone but they build.

This track mirrors the `system_design/` study style: **zero prior knowledge assumed, depth over brevity, worked examples, a Self-check at the end, and the same sourcing directive** (every non-obvious technical claim is cited; each lesson ends with a Sources appendix; nothing stated from memory).

> ## ⚠️ Reframe (2026-07-15) — how this track is taught, from Lesson 02 on
> The round is **language-agnostic and the candidate writes PSEUDOCODE in the room, not C#** (per Alex's email: *"an actual pseudocode implementation that is reliable, fast, scalable … not expected to write compiling code … but ready to write sufficiently specific code to have a conversation around tradeoffs"*). So:
> - **Concept-first, language-agnostic.** Primitives are taught generically in pseudocode — `lock(m){}`, `wait(cv)`, `signal()`, CAS — **not** `System.Threading` APIs. Lesson 01 over-invested in C# quirks (`ManagedThreadId`, foreground/background, `Thread` vs `Task` config); those are demoted to short "language notes," never the subject.
> - **"Sufficiently specific pseudocode" = the middle** between system-design hand-waving and compiling code: real class structure + method bodies where the synchronization lives, specific enough that an interviewer can poke *"what if two threads hit that line at once?"* and you defend it — but syntax/compilation don't matter.
> - **Three graded quality axes** (from the email): **reliable** (correct-under-concurrency + durable), **fast** (low contention/latency), **scalable** (striping, bounded resources). Each lesson is framed as **design + tradeoffs** ("here's a race → here are the fix options → what each costs"), not dry mechanics.
> - **OO design quality is graded, not just thread-safety** — SRP, cohesion/coupling, separation of concerns. Woven through **every** design lesson (e.g. cache = separate store / eviction policy / TTL sweeper / lock strategy), not saved for Lesson 12.
> - **C# / the code harness = optional "watch-it-happen" tool only,** scoped to the ~4 lessons where seeing non-determinism matters: **races (02), deadlock (03), memory model (04), thread-safe cache (09).** Skipped elsewhere. `async`/`await` is one axis (Lesson 08), **not** the spine — the meat is shared-memory, lock-based concurrency.


Companion docs: [../../full_loop/study-plan.md](../../full_loop/study-plan.md) (§2 = this curriculum) and [../../full_loop/databricks-full-loop.md](../../full_loop/databricks-full-loop.md) (what the loop is). Progress + schedule: [../tracker.md](../tracker.md).

## Reading order

| # | Lesson | Covers | Optional demo (watch-it-happen) |
|---|--------|--------|--------------------------|
| 01 | [01-threads-execution-model.md](01-threads-execution-model.md) | process vs thread, `Thread` vs `Task` vs thread pool, scheduling, context switches, cores, concurrency vs parallelism, why concurrency (throughput/latency) | spin up threads, observe interleaving |
| 02 | 02-shared-state-races.md *(planned)* | data race, critical section, atomicity, the `count++` race, non-determinism | reproduce a race, then a lost update |
| 03 | [03-mutual-exclusion-locks.md](03-mutual-exclusion-locks.md) | mutex/lock, spinlock, lock granularity, contention; deadlock (4 conditions), livelock, lock ordering | fix L2's race; force + fix a deadlock |
| 04 | 04-memory-model-visibility.md *(planned)* | instruction/memory reordering, visibility, memory barriers/fences, atomic read-write, compare-and-swap (CAS), why locks also give visibility | atomic counter; visible stop-flag |
| 05 | 05-condition-synchronization.md *(planned)* | condition variables (wait/signal/broadcast), spurious wakeups, producer–consumer | bounded blocking queue → async logger |
| 06 | 06-higher-level-primitives.md *(planned)* | semaphore, latch / manual-reset event, countdown latch, barrier, read-write lock | rate-of-N gate; reader-writer map |
| 07 | 07-concurrent-data-structures.md *(planned)* | thread-safe queue/map, lock-free basics (Treiber stack, CAS loop, ABA), lock striping/sharding | striped-lock hashmap; CAS stack |
| 08 | 08-thread-pools-scheduling.md *(planned)* | pool model, work queues, blocking vs async, oversubscription, `async`/`await` as a model | fixed pool + work queue |
| 09 | 09-thread-safe-cache.md ⭐ *(planned)* | LRU/LFU under concurrency, lock striping/sharding, eviction, TTL expiry thread, read-heavy tuning | concurrent LRU cache w/ TTL |
| 10 | 10-durability-crash-safety.md *(planned)* | write-ahead log, `fsync`/flush, atomic rename, crash recovery, "power goes down", idempotency | append-only log + recovery |
| 11 | 11-disk-network-io.md *(planned)* | blocking vs non-blocking, buffering, batching, backpressure, page cache | batched writer w/ backpressure |
| 12 | 12-interview-integration.md *(planned)* | rate limiter (token bucket, thread-safe); driving the solution; added requirements mid-flight; SRP / cohesion / coupling; trade-off narration | thread-safe token-bucket limiter |

## How these notes are written

- **Zero prior knowledge assumed.** Every term is defined the first time it appears.
- **Depth over brevity.** These are deep-dive study notes, not cheat-sheets. Mechanisms are explained ("X is faster" always says *why*).
- **Sourcing directive.** Every non-obvious technical claim traces to a cited source; each lesson ends with a **Sources** appendix (fact → source + as-of date). Nothing asserted from memory.
- **Concept-first, pseudocode-primary (from L02, per the 2026-07-15 reframe above).** Primitives are taught generically (pseudocode), C# is only an optional demo vehicle, and every lesson is framed as design + tradeoffs with OO-design quality (SRP/cohesion/coupling) woven in.
- **Runnable (optional).** The "Ships" column is implemented in the C# harness at [../code/](../code) — `cd systems_programming/code && dotnet run -- <demo>` (no arg lists demos) — kept only as a *watch-it-happen* intuition tool for L02/03/04/09.

## How to study each lesson

1. Read it once for the map.
2. Re-read and, for each concept, **explain it back in your own words**.
3. Run the demo; predict the output *before* running, then reconcile.
4. Do the **Self-check** out loud as if teaching it.

## Cross-day workflow (spaced recall)

Every lesson ends with a **Self-check**. **When we study lesson N, the next session opens with an open-ended quiz on lesson N** — you teach it back, we log gaps to [../grades/](../grades). Same principle as the algorithms + system_design tracks.

## Status

- 01, 02, 03 written to full depth (v1.0). Rest planned — written in order, one lesson at a time, quizzing on the prior lesson first.
</invoke>
