# 04 — Memory model & visibility (volatile, barriers, Interlocked / CAS)

Lessons 02–03 assumed a comfortable lie: that when a thread *writes* a value, the write (a) happens **when** you wrote it in the source, and (b) is immediately **seen** by every other thread. Neither is guaranteed. Modern compilers, runtimes, and CPUs **reorder** memory operations and keep values in **registers/per-core caches** for speed — so a write that has fully *finished* on one core may be **invisible** to another, and two threads can even disagree about the *order* in which things happened. This is the **memory model**: the rulebook for *when one thread's writes become visible to another, and in what order*.

This lesson is the sneaky sibling of L02's race. A race is "two threads collide on one location." **Visibility/ordering bugs are worse to reason about**: each thread's own code looks obviously correct, runs correctly in isolation, and still breaks — because the *communication* between threads was never made explicit. We cover why reordering and stale reads happen, the tool that fixes visibility for a single flag (**volatile**), the tool that fixes ordering explicitly (**memory barrier/fence**), the tool that makes a read-modify-write **atomic without a lock** (**Interlocked / compare-and-swap**), and — the punchline — **why a lock quietly gave you all of this for free** in L03.

By the end you should be able to, cold:

- Explain **reordering** (compiler/JIT *and* hardware) and **stale reads** (per-core cache / register hoisting), and why a plain `stop = true` in one thread may **never** be seen by a `while (!stop)` loop in another.
- Define the **memory model** and the **happens-before** relationship — the guarantee that "*if A happens-before B, then A's writes are visible to B*."
- Say precisely what **`volatile`** buys (visibility + ordering for that one field) and, just as important, what it **does not** buy (it is *not* atomic for read-modify-write, so it does **not** fix L02's `count++`).
- Explain a **memory barrier/fence** — the instruction that forbids reordering across it — and how it's the primitive underneath everything else.
- Define **compare-and-swap (CAS)** / `Interlocked.CompareExchange`, write the **CAS retry loop**, and explain how it makes a multi-step update atomic **without blocking** — and its failure mode, the **ABA problem** (previewed for L07).
- Deliver the key unifier: **a lock provides mutual exclusion *and* a memory barrier** — acquiring/releasing it publishes writes — so once you hold a lock, you get visibility for free and rarely need `volatile`/barriers by hand.

We stay in **pseudocode**; C# (`volatile`, `Interlocked`, `Thread.MemoryBarrier`) appears only as the optional "watch-it-happen" demo (an atomic counter, and a visible stop-flag).

---

## Part 0 — Where we are, and the second hazard

L02 gave us the **race**: interleaving tears a multi-step update apart (a *safety* failure — corruption). L03 gave us the **lock** to make a region indivisible, and its own hazard, **deadlock** (a *liveness* failure — frozen). Both lessons quietly assumed something we never justified:

> When thread A executes `x = 5`, thread B — if it reads `x` afterward — sees `5`.

On a single-core machine running one instruction at a time, that's basically true. On **real hardware with multiple cores, caches, and optimizing compilers, it is not.** Three separate layers each reserve the right to bend it, *all in the name of speed* `[S1]`:

1. **The compiler / JIT** may **reorder** independent instructions, or **hoist** a repeatedly-read field into a register once and reuse it (never re-reading memory).
2. **The CPU** may **reorder** loads and stores as it executes (out-of-order execution, store buffers).
3. **The cache hierarchy** means each core has its **own L1/L2 cache**; a write lands in the writer's cache and may not be **propagated** to another core's cache immediately.

Each layer, *on its own thread*, preserves that thread's single-threaded meaning perfectly — reordering only ever touches operations that don't affect *this* thread's result. The problem is that **another thread can observe the intermediate/​reordered state**, and nobody promised it wouldn't. The memory model is the contract that says *exactly* which cross-thread observations are legal and how to forbid the dangerous ones.

> **One-line framing for the room:** *"Locks and atomicity handle threads **colliding** on data. The memory model handles threads **communicating** data — when a write on one core actually becomes visible, and in what order, on another. Without a synchronization point, the answer is 'no promises.'"*

---

## Part 1 — Visibility: the stop-flag that never stops

The cleanest way to *feel* the hazard is a flag one thread sets to tell another to stop. Microsoft's own `volatile` docs use exactly this shape `[S1]`.

```
   shared boolean stop = false           // NOT synchronized

   Worker thread:                        Main thread:
       while (not stop):                     ... do some work ...
           do_one_unit_of_work()             stop = true          // "please halt"
       // expected: exits soon after         join(worker)         // wait for it
```

Read it single-threadedly and it's obviously fine: main flips `stop`, the worker's next loop test sees `true`, it exits. Now add the real hardware:

- The worker reads `stop` **every iteration** in a tight loop. The JIT sees that *the worker never writes `stop`*, so — legally, for single-threaded semantics — it may **hoist the read out of the loop**: read `stop` **once** into a register, then loop on the register forever. Main's later write to memory changes the memory location, but the worker is no longer *looking* at memory. **The worker never stops.** `[S1]`
- Even without hoisting, main's write may sit in main's core's **store buffer / cache** and not propagate to the worker's core promptly, so the worker keeps reading a **stale** `false`.

Microsoft states the outcome precisely: without the `volatile` modifier on the flag, *"the behavior is unpredictable. The `DoWork` method might optimize the member access, resulting in reading stale data … the number of stale reads is unpredictable."* `[S1]` This is a **visibility** bug: the write *happened*, it just isn't *seen*.

```
   ┌─────────────── the visibility gap ───────────────┐
   Main core                         Worker core
   ┌──────────────┐                  ┌──────────────┐
   │ stop = true  │ ── write lands ─▶│ (cache/reg)  │  worker still holds `false`
   │  in store    │    in MY cache   │  stop=false  │  → loops forever
   │  buffer      │    …not yours    │              │
   └──────────────┘                  └──────────────┘
        "I wrote it."                    "I never saw it."
```

Note this is **not** L02's race — there's no lost update, no torn value, no two-threads-hit-the-same-line. Each thread's access to `stop` is a clean single read or single write. The bug is purely *"a finished write is not visible."* That's why it's sneakier: the usual "wrap it in a lock because of the read-modify-write" instinct doesn't even obviously apply.

---

## Part 2 — The memory model and *happens-before*

A **memory model** is the formal contract — provided by the language/runtime — that answers: *given a write in one thread and a read in another, is the read guaranteed to see that write, and are writes seen in a consistent order?* The key concept every memory model is built on is **happens-before**:

> **Happens-before:** a relationship between two actions such that if action **A** *happens-before* action **B**, then **A's memory effects (its writes) are guaranteed visible to B**, and A is ordered before B.

Two events with **no** happens-before relationship are said to race / be *unordered*: there is **no guarantee** either sees the other's writes, and they may be observed in either order. The entire job of every synchronization tool in this lesson is to **create happens-before edges** where you need them:

- **Releasing a lock happens-before the next acquire of that lock** (L03's lock, revisited in Part 6).
- **A volatile write happens-before a subsequent volatile read of the same field** (Part 3).
- **An `Interlocked` operation** establishes ordering around itself (Part 5).

The reason a plain `stop = true` fails in Part 1 is simply that there is **no happens-before edge** between main's write and the worker's read — nothing told the system "publish this write; go re-read it." Everything below is a way to *insert that edge*.

> **Say it cleanly:** *"'Thread-safe' isn't only 'no two threads at once.' It's also 'the writer's changes are actually **published** to the reader.' Happens-before is the promise of publication + ordering; without it, visibility is undefined."*

---

## Part 3 — `volatile`: visibility + ordering for one field

The narrowest fix for Part 1's bug is to mark the flag **volatile**. Declaring a field volatile *"excludes it from certain kinds of optimizations"* `[S1]`: the compiler/JIT may **not** hoist it into a register (every read re-reads memory, every write goes to memory), and reads/writes of it may **not** be reordered with surrounding memory operations in the ways that would break cross-thread visibility. Add the modifier and the stop-flag works every run `[S1]`:

```
   shared volatile boolean stop = false      // reads re-read memory; writes publish; no hoisting

   Worker:  while (not stop): work()          // now sees main's write promptly → exits
   Main:    stop = true
```

**Volatile is a one-word tool with sharp edges.** What it does *not* give you is as important as what it does:

1. **It is NOT atomicity for read-modify-write.** Volatile makes each *individual* read and each *individual* write visible and ordered — but `count++` is still **read, add, write** (three ops). Two threads can each do a *fresh* volatile read of the same value, both add one, both write back — **the L02 lost update is completely unfixed.** MS states it flatly: *"The `volatile` keyword doesn't provide atomicity for operations other than assignment. It doesn't prevent race conditions."* `[S1]` **Volatile fixes visibility, not races.**
2. **Only assignment is covered, and only for atomically-writable types.** You cannot mark `double` or `long` volatile in C#, because reads/writes of those *aren't guaranteed atomic* in the first place — you'd get a **torn read** (L02 Shape 3) on top `[S1]`.
3. **No total order across *different* fields.** *"There's no guarantee of a single total ordering of volatile writes as seen from all threads."* `[S1]` Two volatile flags can still be observed in different orders by different threads — volatile coordinates **one location**, not a protocol across several.

**When volatile is the right tool:** a **single flag/reference published once** and read by others — a stop signal, an initialized-yet? flag, swapping in a new immutable snapshot reference. The moment you need *"read-then-write based on the old value"* or *"update two fields together,"* volatile is the wrong tool — reach for `Interlocked` (Part 5) or a lock (Part 6). Microsoft itself now frames volatile as an expert-only tool: *"In most scenarios, use safer and more reliable alternatives … the `Interlocked` class, the `lock` statement, or higher-level synchronization primitives."* `[S1]`

```
   volatile fixes:  "is my finished write SEEN?"        ✔ visibility, ✔ ordering of THAT field
   volatile does NOT fix:  "is read-modify-write atomic?"  -> still a race  (use Interlocked / lock)
```

---

## Part 4 — Memory barriers: the primitive underneath

`volatile` is really a *convenience* spelling of a lower-level idea: the **memory barrier** (a.k.a. **fence**). A barrier is an instruction that **forbids reordering across it** — operations before the barrier may not be moved to after it, and (depending on barrier type) vice-versa. C#'s full fence, `Thread.MemoryBarrier()`, is defined exactly so: *"the processor executing the current thread cannot reorder instructions in such a way that memory accesses prior to the call … execute after memory accesses that follow the call."* `[S2]`

```
   store A                    │  store A
   MemoryBarrier()   ← fence  │  ─────────  nothing may cross this line
   load  B                    │  load  B          (A can't sink below; B can't rise above)
```

Why this is the foundation: "publish my writes, then read fresh" is implemented by putting a barrier between the write and the read. A **volatile write** is (roughly) "store + release-barrier"; a **volatile read** is "acquire-barrier + load." A **lock acquire/release** and every **`Interlocked`** op include the appropriate barriers too `[S2]`. So barriers are the atoms; volatile, Interlocked, and locks are molecules built from them. You rarely write `MemoryBarrier()` by hand — MS notes *"for most purposes, the `lock` statement … or the `Monitor` class provide easier ways to synchronize data"* `[S2]` — but knowing it's *there*, underneath, is what lets you explain *why* the higher-level tools give visibility.

> **Interview-grade sentence:** *"Reordering is legal because it preserves single-thread meaning; a **memory barrier** is how you say 'not here' — it pins the order at the one point where another thread is watching. Volatile, Interlocked, and locks all plant barriers for you."*

---

## Part 5 — Interlocked / compare-and-swap: atomic without a lock

Part 3 said volatile can't fix `count++`. A lock (L03) can — but a lock **blocks** (deschedule, ~2 context switches). For a **single-location** read-modify-write there's a cheaper tool: a **hardware atomic instruction**, exposed as the **`Interlocked`** class — *"atomic operations for variables that are shared by multiple threads"* `[S3]`.

### 5a. The easy ones: Increment / Add / Exchange

`Interlocked.Increment(ref count)` performs the whole load-add-store **as one indivisible hardware step** — no other thread can interleave inside it. MS spells out the exact race it prevents: incrementing normally is *"load a value … increment … store the value,"* and *"a thread can be preempted after executing the first two steps [while] another thread … executes all three steps … the effect of the [other] increment … is lost"* `[S3]`. `Interlocked.Increment` closes that window with no lock. This is **L02's `count++`, fixed a second way** — atomically instead of by mutual exclusion:

```
   shared integer count = 0
   // each thread:
   repeat N times:
       Interlocked.increment(count)     // whole read-modify-write is ONE atomic step
   // final total is exact — like the lock, but no blocking/context switch
```

`Exchange` (atomically set and return the old value) and `Add` (atomic add, return new) round out the simple set `[S3]`.

### 5b. The general one: CompareExchange (CAS) + the retry loop

The workhorse — the primitive nearly all lock-free code is built on — is **compare-and-swap (CAS)**, whose real signature is `Interlocked.CompareExchange(ref location1, value, comparand)` `[S4]`:

- **`location1`** — the destination being read and maybe overwritten.
- **`value`** — the **new** value to store *if* the compare succeeds.
- **`comparand`** — the value you **expect** to still be there (the old/snapshot value you're checking against).

> *"If `comparand` and the value in `location1` are equal, then `value` is stored in `location1`. Otherwise, no operation is performed. The compare and exchange … [is] an atomic operation. The return value … is the original value in `location1`, whether or not the exchange takes place."* `[S4]`

(Note the docs' wording is slightly overloaded: *"the value in `location1`"* means the **current contents**, whereas the bare *"value"* is the **parameter** — the new value.) In words: **"atomically — *if* the location still holds the value I last saw (`comparand`), replace it with my new `value`; either way, tell me what was actually there."** That single conditional-write lets you build *any* read-modify-write safely with the **CAS retry loop**: read the current value, compute the new one, then CAS it in — and if someone changed it underneath you (CAS reports back a value ≠ what you saw), **loop and retry** `[S4]`:

```
   atomic_add(location1, delta):
       repeat:
           observed  = read(location1)             // snapshot current value  → use as `comparand`
           computed  = observed + delta            // compute new value from it → use as `value`
           previous  = CompareExchange(location1, computed, observed)   // store IF unchanged
       until previous == observed                  // success? (nobody changed it mid-flight)
       // if previous != observed, another thread won the race → loop and retry with fresh value
```

This is **optimistic concurrency**: assume no conflict, do the work, and only *validate-and-commit* at the atomic CAS; on conflict, redo. Contrast the lock's **pessimistic** stance ("assume conflict, exclude everyone up front").

- **Buys:** no blocking, no context switch, no deadlock (a thread that stalls never holds anything others need). Under low/medium contention this is **faster and more scalable** than a lock — the core of *fast* + *scalable*.
- **Costs:** only naturally covers a **single location** (multi-field invariants still want a lock); under **high** contention the retry loop **spins and wastes work** (like a spinlock); and it's **subtle to get right** — which is why the round rewards knowing *when* to reach for it, not memorizing lock-free structures.

### 5c. The ABA problem (preview for L07)

CAS checks *"is the value still what I saw?"* — but "same value" isn't "never changed." If the location goes `A → B → A` between your read and your CAS, the CAS sees `A`, thinks nothing happened, and **succeeds — even though the world moved and back.** That's the **ABA problem**. It rarely bites simple counters (a number being equal again is usually fine) but is a real hazard for **pointer/stack** structures, where a reused address masks a change. The standard cures — version/stamp counters, hazard pointers — belong to **Lesson 07 (lock-free structures)**; here just be able to *name* ABA as CAS's blind spot.

---

## Part 6 — The unifier: a lock already gave you all of this

Here's the payoff that ties L03 and L04 together, and the single most important sentence of this lesson:

> **A lock provides mutual exclusion *and* a memory barrier.** Acquiring a lock includes an acquire-barrier; releasing it includes a release-barrier. So **releasing a lock happens-before the next acquire of the same lock** — every write you made inside the critical section is **published** to the next thread that takes the lock.

That means when you protect shared state with a lock (the L03 default), you did **not** only stop threads from *colliding* — you also guaranteed every thread that later takes the lock **sees the latest writes**. Visibility comes **free** with the lock. This is why, in ordinary lock-based code, you almost never sprinkle `volatile` or `MemoryBarrier()` by hand — MS's guidance is exactly that: prefer the `lock` statement / `Interlocked` over raw volatile/barriers `[S1][S2]`. You reach below the lock only when you're deliberately going **lock-free** for performance.

So the tool ladder for *"make shared state correct across threads"*:

```
   ┌────────────────────────┬───────────────────────────────────────────────┬─────────────────────────┐
   │ Tool                   │ Gives you                                     │ Reach for it when       │
   ├────────────────────────┼───────────────────────────────────────────────┼─────────────────────────┤
   │ volatile (one field)   │ visibility + ordering of THAT field           │ single published flag / │
   │                        │ (NOT atomic RMW, NOT multi-field)             │ reference, read by many │
   │ Interlocked / CAS      │ atomic read-modify-write of ONE location      │ counter / lock-free     │
   │                        │ + barriers, no blocking                       │ single-location update  │
   │ lock (L03)             │ mutual exclusion over a REGION + full barrier │ the default: anything   │
   │                        │ (atomicity + visibility together)             │ bigger than one word    │
   └────────────────────────┴───────────────────────────────────────────────┴─────────────────────────┘
```

> **The senior one-liner:** *"For a single published flag I use `volatile`; for a single-location counter I use `Interlocked` / CAS; for anything spanning multiple fields or steps I use a lock — and I lean on the lock because it gives me atomicity **and** visibility in one move. I only drop to volatile/CAS/barriers when I'm deliberately trading that simplicity for lock-free speed."*

---

## Part 7 — The "ship": atomic counter, and a visible stop-flag

The runnable demo lives at [../code/Lessons/Lesson04Memory.cs](../code/Lessons/Lesson04Memory.cs). Run it:

```bash
cd systems_programming/code
dotnet run -- 04-memory
```

Two acts, mapped to this lesson:

1. **Atomic counter (Part 5).** The same shared-counter workload as L02/L03, three ways: (a) plain `count++` — **loses updates** (the L02 race); (b) `Interlocked.Increment` — **exact every run**, atomic without a lock; and, to make Part 3's warning concrete, (c) a `volatile` counter *still* using `++` — **also loses updates**, proving volatile fixes *visibility*, not the read-modify-write *race*.
2. **Visible stop-flag (Parts 1 & 3).** A worker spins on a stop flag while main sets it. With a **`volatile`** flag the worker sees the write and stops promptly. The non-volatile version is run under a **watchdog timeout** (so it can't freeze the terminal) to illustrate the *possibility* of a stale-read hang — with an honest note that whether it actually hangs is **JIT/hardware-dependent** (Release-mode hoisting is what triggers it), so the demo reports what happened this run rather than claiming a guaranteed freeze.

Predict before running: (1a) plain and (1c) volatile-but-`++` both lose updates; (1b) Interlocked is exact. (2) the volatile worker always stops; the plain worker *may* stop or may have to be force-abandoned by the watchdog. Seeing the volatile-but-`++` counter *still* race is the whole "visibility ≠ atomicity" lesson in one line of output.

---

## Self-check (say the answers out loud, as if teaching)

1. Name the **three layers** that can make one thread's write invisible or reordered to another, and give the single-thread reason each is *allowed* to do it. Why does a plain `while (!stop)` loop potentially **never** see `stop = true`?
2. Define the **memory model** and **happens-before**. What does it mean for two accesses to have *no* happens-before edge between them? How is that the root cause of the stop-flag bug?
3. What exactly does **`volatile`** guarantee? State **three things it does *not* do** — and explain precisely why marking `count` volatile does **not** fix L02's `count++`.
4. What is a **memory barrier / fence**? How do volatile, Interlocked, and locks relate to it? Why do you rarely write one by hand?
5. What does **`Interlocked.CompareExchange` (CAS)** do, and what does it **return**? Write the **CAS retry loop** for an atomic add and explain the "optimistic, validate-at-commit" idea. Give two costs of CAS vs a lock, and name the **ABA** problem.
6. Deliver the unifier: **why does holding a lock also give you visibility?** State the happens-before edge involved. Given that, when would you *still* choose `volatile` or `Interlocked` over a lock?

If any answer is shaky, re-read that Part before Lesson 05 — **Condition synchronization** — where we go from "protect shared state" to "*wait for* a condition" (a thread sleeping until another signals it), and build the producer–consumer queue that underpins async loggers and thread pools.

---

## Sources

Facts above are cited inline by tag. Pulled/verified **2026-07-20** from Microsoft Learn (authoritative for .NET/C# memory-model semantics). CAS/ABA and happens-before are standard concurrency theory; the specific .NET API guarantees are cited to the docs.

- `[S1]` Microsoft Learn — *volatile keyword (C# reference)*. "For performance reasons, the compiler, the runtime system, and even hardware might rearrange reads and writes to memory locations. Declaring a field as `volatile` excludes it from certain kinds of optimizations." "There's no guarantee of a single total ordering of volatile writes as seen from all threads." Note: "On a multiprocessor system, a volatile read operation doesn't guarantee to obtain the latest value written … a volatile write … [isn't] immediately visible to other processors." "The `volatile` keyword doesn't provide atomicity for operations other than assignment. It doesn't prevent race conditions." Can't mark `double`/`long` volatile (reads/writes not guaranteed atomic). Worker/`_shouldStop` stop-flag example — without `volatile`, "the behavior is unpredictable … reading stale data … the number of stale reads is unpredictable." Guidance: prefer `Interlocked`, `lock`, or higher-level primitives over `volatile`. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/volatile (ms.date 2026-01-22; retrieved 2026-07-20).
- `[S2]` Microsoft Learn — *Thread.MemoryBarrier Method (System.Threading)*. "Synchronizes memory access as follows: The processor executing the current thread cannot reorder instructions in such a way that memory accesses prior to the call to `MemoryBarrier()` execute after memory accesses that follow the call." Remarks: "For most purposes, the C# `lock` statement … or the `Monitor` class provide easier ways to synchronize data." (Also `Interlocked.MemoryBarrier()` / `MemoryBarrierProcessWide()` per the Interlocked reference `[S3]`.) https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread.memorybarrier (ms.date 2025-07-01; retrieved 2026-07-20).
- `[S3]` Microsoft Learn — *Interlocked Class (System.Threading)*. "Provides atomic operations for variables that are shared by multiple threads." Increment/Decrement: incrementing normally requires "load a value … increment … store the value," and without Interlocked "a thread can be preempted after executing the first two steps … another thread … executes all three steps … the effect of the … increment … is lost." `Add`/`Exchange`/`CompareExchange` described as atomic; "Ensure that any write or read access to a shared variable is atomic." `MemoryBarrier()` reordering guarantee listed among methods. https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked (ms.date 2025-07-01; retrieved 2026-07-20).
- `[S4]` Microsoft Learn — *Interlocked.CompareExchange Method*. "If `comparand` and the value in `location1` are equal, then `value` is stored in `location1`. Otherwise, no operation is performed. The compare and exchange operations are performed as an atomic operation. The return value … is the original value in `location1`, whether or not the exchange takes place." Includes the canonical CAS retry-loop example (accumulate a running total: snapshot, compute, `CompareExchange`, retry while the returned original ≠ the snapshot). https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.compareexchange (ms.date 2025-07-01; retrieved 2026-07-20).

**Not independently re-benchmarked / flagged:** the "~2 context switches" cost of a blocking lock is carried qualitatively from L03, not measured here. Whether the *non-volatile* stop-flag demo actually hangs is **JIT-/hardware-/build-dependent** (register hoisting is most likely under Release-mode optimization); the demo reports the observed outcome of the run rather than asserting a guaranteed freeze — the *volatile* version's correctness is the guaranteed, sourced part `[S1]`. The mapping "volatile write ≈ store+release-barrier, volatile read ≈ acquire-barrier+load" and "lock acquire/release plant barriers" is the standard acquire/release model consistent with `[S1][S2]`; exact fence placement is implementation-defined.
