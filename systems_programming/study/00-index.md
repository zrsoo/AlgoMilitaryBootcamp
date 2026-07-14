# Systems Programming — Study Curriculum (Concurrency, C#)

Self-contained explainers to build **single-machine concurrency from scratch** — the one true gap for the Databricks loop's **Systems Programming** round (thread-safe cache, rate limiter, durability/"power goes down", synchronization). Read in order; each is standalone but they build.

This track mirrors the `system_design/` study style: **zero prior knowledge assumed, depth over brevity, worked examples, a Self-check at the end, and the same sourcing directive** (every non-obvious technical claim is cited; each lesson ends with a Sources appendix; nothing stated from memory).

Companion docs: [../../full_loop/study-plan.md](../../full_loop/study-plan.md) (§2 = this curriculum) and [../../full_loop/databricks-full-loop.md](../../full_loop/databricks-full-loop.md) (what the loop is). Progress + schedule: [../tracker.md](../tracker.md).

## Reading order

| # | Lesson | Covers | Ships (C# you implement) |
|---|--------|--------|--------------------------|
| 01 | [01-threads-execution-model.md](01-threads-execution-model.md) | process vs thread, `Thread` vs `Task` vs thread pool, scheduling, context switches, cores, concurrency vs parallelism, why concurrency (throughput/latency) | spin up threads, observe interleaving |
| 02 | 02-shared-state-races.md *(planned)* | data race, critical section, atomicity, the `count++` race, non-determinism | reproduce a race, then a lost update |
| 03 | 03-mutual-exclusion-locks.md *(planned)* | `lock`/`Monitor`, mutex, spinlock, granularity, contention; deadlock (4 conditions), livelock, lock ordering | fix L2's race; force + fix a deadlock |
| 04 | 04-memory-model-visibility.md *(planned)* | reordering, `volatile`, memory barriers, `Interlocked` (CAS), why locks give visibility | Interlocked counter; volatile stop flag |
| 05 | 05-condition-synchronization.md *(planned)* | `Monitor.Wait`/`Pulse`, condition variables, spurious wakeups, producer–consumer | bounded blocking queue → async logger |
| 06 | 06-higher-level-primitives.md *(planned)* | `SemaphoreSlim`, `ManualResetEventSlim`, `CountdownEvent`, `Barrier`, `ReaderWriterLockSlim` | rate-of-N gate; reader-writer map |
| 07 | 07-concurrent-data-structures.md *(planned)* | thread-safe queue/map, `ConcurrentDictionary`, lock-free basics (Treiber stack, CAS loop, ABA), lock striping | striped-lock hashmap; CAS stack |
| 08 | 08-thread-pools-scheduling.md *(planned)* | pool model, work queues, blocking vs async, oversubscription, `async`/`await` as a model | fixed pool + work queue |
| 09 | 09-thread-safe-cache.md ⭐ *(planned)* | LRU/LFU under concurrency, lock striping/sharding, eviction, TTL expiry thread, read-heavy tuning | concurrent LRU cache w/ TTL |
| 10 | 10-durability-crash-safety.md *(planned)* | write-ahead log, `fsync`/flush, atomic rename, crash recovery, "power goes down", idempotency | append-only log + recovery |
| 11 | 11-disk-network-io.md *(planned)* | blocking vs non-blocking, buffering, batching, backpressure, page cache | batched writer w/ backpressure |
| 12 | 12-interview-integration.md *(planned)* | rate limiter (token bucket, thread-safe); driving the solution; added requirements mid-flight; SRP / cohesion / coupling; trade-off narration | thread-safe token-bucket limiter |

## How these notes are written

- **Zero prior knowledge assumed.** Every term is defined the first time it appears.
- **Depth over brevity.** These are deep-dive study notes, not cheat-sheets. Mechanisms are explained ("X is faster" always says *why*).
- **Sourcing directive.** Every non-obvious technical claim traces to a cited source; each lesson ends with a **Sources** appendix (fact → source + as-of date). Nothing asserted from memory.
- **Runnable.** The "Ships" column is implemented in the C# harness at [../code/](../code) — `cd systems_programming/code && dotnet run -- <demo>` (no arg lists demos).

## How to study each lesson

1. Read it once for the map.
2. Re-read and, for each concept, **explain it back in your own words**.
3. Run the demo; predict the output *before* running, then reconcile.
4. Do the **Self-check** out loud as if teaching it.

## Cross-day workflow (spaced recall)

Every lesson ends with a **Self-check**. **When we study lesson N, the next session opens with an open-ended quiz on lesson N** — you teach it back, we log gaps to [../grades/](../grades). Same principle as the algorithms + system_design tracks.

## Status

- 01 written to full depth (v1.0). Rest planned — written in order, one lesson at a time, quizzing on the prior lesson first.
</invoke>
