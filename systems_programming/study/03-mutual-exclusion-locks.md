# 03 — Mutual exclusion & locks (+ deadlock)

Lesson 02 ended with a **fix landscape**: four ways to kill a race (mutual exclusion, atomic op/CAS, immutability, confinement). This lesson builds **row 1** — the general-purpose hammer, **mutual exclusion via a lock** — the tool you'll reach for in the interview whenever the critical section is more than a single-location update. We take it all the way: what a lock *is*, what it *costs* (blocking vs spinning, contention, granularity), how to *design* with it (the OO half the round grades), and then the price of using it carelessly — **deadlock**, its four necessary conditions, and how to break them.

By the end you should be able to, cold:

- Say precisely what a **lock** guarantees, and write the `lock(m){ critical section }` pseudocode that turns L02's racy `count++` into a correct one.
- Explain what happens to a thread that **can't** get the lock — *blocking* (descheduled, a context switch — the L01 cost model) vs *spinning* (busy-wait) — and when each is right.
- Argue **lock granularity**: why one coarse lock is correct-but-slow, why fine/striped locks scale, and the tradeoff between them (the *reliable* vs *fast/scalable* conversation).
- Lay out the **design rules** for locks (the lock lives with the data, private lock object, hold briefly, never call foreign code while holding it) — SRP/encapsulation, which is graded.
- Define **deadlock**, walk the canonical two-lock example, name the **four Coffman conditions**, and give the practical cure (**lock ordering**) plus its alternatives (`TryEnter`/timeout, coarser lock) and their tradeoffs. Distinguish **livelock**.

We stay in **pseudocode**; C# appears only as the optional "watch-it-happen" demo (fix the race, then force *and* fix a deadlock).

---

## Part 0 — Where we are, and the tool we need

Recall the disease from L02: a **critical section** — a region that touches shared mutable state — can be torn apart because the scheduler may interleave two threads inside it. The cure was stated as a property: **make the critical section atomic** (all-or-nothing). L02 previewed *three* ways to get that property; two of them dodge the problem (don't share → confinement; don't mutate → immutability), and one hardware trick handles a *single location* (atomic op/CAS, Lesson 04).

But most real critical sections are **not** a single-location update — they're "check the map, then insert," or "debit one account and credit another," spanning multiple lines and multiple fields (L02's Shapes 2 and 4). For those you need a tool that makes an **arbitrary block** behave as one indivisible unit. That tool is the **lock**.

> **One-line framing for the room:** *"An atomic instruction makes one location indivisible; a **lock** makes an arbitrary **region of code** indivisible — it's how you enforce a critical section that's bigger than a single word."*

---

## Part 1 — The lock: mutual exclusion, precisely

A **lock** (a.k.a. **mutex**, from *mut*ual *ex*clusion) is a coordination object with exactly two operations — **acquire** and **release** — and one guarantee:

> **At most one thread can hold the lock at any instant.** A thread that calls `acquire` while another holds it **waits** until the holder calls `release`. `[S1]`

You wrap the critical section between them:

```
   lock m                          // a lock object that GUARDS a specific piece of shared state

   acquire(m)                      // ── enter: if someone else holds m, wait here ──
       // ... critical section ...    only ONE thread is ever between acquire and release
   release(m)                      // ── exit: hand the lock to a waiter, if any ──
```

Because only one thread is ever *between* `acquire` and `release`, the block runs as if it were a single uninterruptible step **with respect to every other thread that uses the same lock**. That is exactly the "make the critical section atomic" property L02 asked for. The platform states it cleanly for the C# `lock` statement: *"the `lock` statement ensures that at most only one thread executes its body at any moment in time … any other thread is blocked from acquiring the lock and waits until the lock is released."* `[S1]`

### 1a. L02's `count++`, fixed

```
   shared integer count = 0
   lock m                          // guards `count`

   // each of the two threads runs:
   repeat 1000 times:
       acquire(m)
           count = count + 1       // the 3-step read-modify-write now runs ALONE
       release(m)
```

Walk the L02 disaster again: thread A does `acquire(m)`, gets it, and starts its load/add/store. If B now calls `acquire(m)`, B **cannot** enter — it waits. A finishes all three steps and `release`s. *Only then* does B acquire and read the fresh value. The stale-read window is gone. The final total is exactly `2000`, every run. (This is demo step 1.)

### 1b. Two properties every correct lock has

1. **Mutual exclusion (safety):** never two holders at once. Without it the lock is pointless.
2. **Progress (liveness):** if the lock is free and someone wants it, *someone* eventually gets it (no thread waits forever while the lock sits available). A lock that can wedge shut is worse than none.

Keep these two words — **safety** (nothing bad happens: no two threads inside) and **liveness** (something good eventually happens: a waiter gets in). Deadlock (Part 5) is precisely a **liveness** failure of an otherwise-safe locking scheme.

### 1c. Reentrancy (a property to *know you're relying on*)

A lock is **reentrant** (a.k.a. *recursive*) if the thread that already holds it can `acquire` it **again** without blocking itself — it just bumps a counter, and must `release` the same number of times. The C# `lock`/`Monitor` is reentrant: *"while a lock is held, the thread that holds the lock can acquire and release the lock multiple times."* `[S1]`

Why you care: if method `A()` takes the lock and calls `B()`, and `B()` *also* takes the same lock, a **non-reentrant** lock would deadlock a thread **against itself**. Reentrancy prevents that. In the interview, when you write a class where one locked method calls another, say *"I'm assuming a reentrant lock here, otherwise this self-call deadlocks."* That one sentence shows you see the trap.

> **Language note (not the point).** In C# you almost never call acquire/release by hand; you write `lock (obj) { ... }`, which the compiler expands to `Monitor.Enter/Exit` inside a `try/finally` so the lock is **always released even if the body throws** `[S1]`. `Monitor`/`lock` also has **thread affinity** — the *same* thread that entered must be the one that exits `[S2]`. In pseudocode we just write `acquire`/`release`, but "release in a finally so an exception can't leak the lock" is a real correctness point worth stating.

---

## Part 2 — What a lock actually costs: blocking vs spinning

A lock is not free, and the round grades *fast*. The cost has two very different shapes depending on **what a waiting thread does while it waits**.

### 2a. Blocking (the default): the thread steps aside

A **blocking lock** puts a thread that can't get in to **sleep**: the OS moves it from *running* to a *blocked/waiting* state, takes it off the CPU, and runs someone else. When the lock is released, the OS **wakes** a waiter, moves it back to *ready*, and reschedules it.

This is the **L01 context-switch cost model** cashing in. Blocking costs:
- a **context switch out** (save registers, swap in another thread) when the thread parks, and
- a **context switch back in** (plus the woken thread's caches/TLB are now cold) when it resumes.

So an *uncontended* `acquire` (nobody else holds it) is nearly free — just a quick atomic check. A *contended* one can cost **two context switches** and cache pollution. This is *why* L02's "don't over-synchronize" mattered: every needless contended lock buys you that round-trip. Blocking is the right default when the critical section — or the wait — is **long** relative to a context switch, because the parked thread lets the CPU do real work meanwhile.

### 2b. Spinning: the thread burns CPU waiting

A **spinlock** does the opposite: a thread that can't get in **loops, re-checking the lock** until it's free — it never sleeps. The docs describe exactly this: a spin lock *"waits in a loop, repeatedly checking until the lock becomes available."* `[S2]`

```
   spin-acquire(m):
       while not try_take(m):      // atomic test-and-set
           // spin: do nothing, check again (optionally a tiny pause)
```

- **Buys:** no context switch — if the lock frees in a few hundred nanoseconds, the spinner grabs it *immediately*, far faster than parking and being re-woken.
- **Costs:** it **burns a whole CPU core doing nothing** while it spins. If the holder won't release for a while (or, worst case, the spinner is on the *same single core* as the holder, so the holder can't even run to release it), spinning is a disaster.

**The rule:** spin when the critical section is **very short** and you likely have **spare cores**; block when it's longer or cores are scarce. Real systems often **combine** them — spin briefly (the lock usually frees fast), and only *then* fall back to blocking. (C#'s `SpinWait` does exactly this: spin a little, "and then yield … only if the condition was not met in the specified time." `[S2]`)

```
   ┌───────────────┬───────────────────────────┬───────────────────────────┐
   │               │  BLOCKING lock            │  SPINLOCK                 │
   ├───────────────┼───────────────────────────┼───────────────────────────┤
   │ waiter does   │  sleeps (descheduled)     │  loops, re-checking       │
   │ pays          │  ~2 context switches      │  CPU cycles while waiting │
   │ good when     │  wait is long-ish         │  wait is ultra-short      │
   │ bad when      │  wait is nanoseconds       │  holder is slow / 1 core │
   └───────────────┴───────────────────────────┴───────────────────────────┘
```

### 2c. Contention is the enemy either way

**Contention** = multiple threads wanting the same lock at the same time. An uncontended lock is cheap; a hot, contended lock **serializes** all those threads — they take turns, and your beautiful multi-core machine runs that section **one thread at a time**. Contention is where locks kill throughput, and it's the bridge to the next part.

---

## Part 3 — Lock granularity: correctness vs throughput

You've decided to use a lock. The next question — the one that separates a junior "it's thread-safe" from a senior "it's thread-safe *and* scales" — is **how much does one lock protect?**

### 3a. Coarse-grained: one big lock

A **coarse** lock guards a large amount of state (e.g. one lock around the *entire* cache). Recall L02's Q9 cache: wrapping the whole `get_or_compute` in one lock is **correct**, but it means every operation on the cache — *including reads of unrelated keys already present* — must queue on that single lock. Under load you've turned a concurrent structure into a **one-at-a-time** queue. Correct, simple, and a **scalability bottleneck**.

### 3b. Fine-grained & striped: many small locks

A **fine-grained** scheme uses *many* locks, each guarding a small slice, so threads touching *different* slices proceed **in parallel**. The workhorse pattern is **lock striping**: instead of one lock for the whole hashmap, keep an array of *N* locks and map each key to one by its hash (`lock = locks[hash(key) % N]`). Two threads hitting different stripes never contend; only same-stripe collisions serialize.

```
   COARSE:  [ ============ one lock over the whole map ============ ]   ← all ops serialize
   STRIPED: [ lock0 | lock1 | lock2 | lock3 | ... | lock15 ]           ← 16 keys → up to 16-way parallel
                └ key "cat"  └ key "dog"   (different stripes → no contention)
```

- **Buys:** throughput scales with the number of stripes — the whole point of *scalable*.
- **Costs:** **complexity and new hazards.** More locks = more chances to take two at once = **deadlock risk** (Part 5). Operations that span *multiple* stripes (e.g. "resize the whole map," or a multi-key invariant) must acquire several locks *in a consistent order* — harder to get right. Memory overhead of many lock objects.

> **The senior granularity one-liner:** *"Start with one coarse lock — it's obviously correct. Then, only where a profiler shows contention, split it into finer/striped locks to buy parallelism — accepting the extra complexity and deadlock risk that multiple locks bring."* That hits *reliable first, fast/scalable deliberately*, and shows you don't reach for complexity prematurely. (This sets up Lesson 07's concurrent structures and Lesson 09's striped cache.)

---

## Part 4 — Designing with locks (the OO half the round grades)

The round explicitly grades **SRP, cohesion, coupling, encapsulation** — not just "is it thread-safe." Locks are where that shows up. Rules, each with the *why*:

1. **The lock lives with the data it protects — and is private.** Put the lock and the state it guards inside the *same* object, and make the lock a **private, dedicated field** nobody outside can touch. Then the class alone is responsible for its own thread-safety (SRP + encapsulation). The docs warn concretely against locking on *public* or *shared* things — don't lock on `this`, on a `Type`, or on a string, because *other* code might lock the same object and cause **deadlock or contention** you can't see `[S1]`.

   ```
   class Counter:
       private lock m                 // dedicated, private — only Counter uses it
       private integer value = 0
       increment():  acquire(m); value = value + 1; release(m)
       read():       acquire(m); return value; release(m)     // read is locked too — see rule 5
   ```

2. **Use the *same* lock for all access to the *same* data, and a *different* lock for *unrelated* data.** If two methods touch the same field under two *different* locks, you have no mutual exclusion at all — *"if you use different synchronization primitive instances to protect the same resource, you'll circumvent the protection."* `[S2]` Conversely, one lock over *unrelated* fields is needless contention.

3. **Hold the lock as briefly as possible.** Do slow work (I/O, expensive compute) *outside* the critical section; take the lock only to touch the shared state. *"Hold a lock for as short time as possible to reduce lock contention."* `[S1]` This is the single biggest throughput lever after granularity.

4. **Never call unknown ("foreign") code while holding a lock** — no callbacks, no virtual/overridable methods, no events, no I/O of unknown duration. You don't know what that code does; it might take *another* lock (→ deadlock, Part 5), block for a long time (→ everyone waiting on your lock stalls), or re-enter you. Compute what you need, *release*, then call out.

5. **Lock reads too, when writes aren't atomic.** It's tempting to lock only writers. But a reader that races a multi-step writer can see a **torn / half-updated** value (L02 Shapes 3 & 4). Unless the read is of a single atomic location, readers must take the lock as well — which is exactly why the **read-write lock** exists (many readers *or* one writer; Lesson 06) to make all-those-reads cheaper without dropping safety.

> These five are a ready-made checklist to *narrate* while you code in the interview — each is a small "reliable + fast + well-structured" point the grader is listening for.

---

## Part 5 — Deadlock: when locks stop the world

Locks trade one hazard for another. A race is a **safety** failure (something bad happens: corruption). A **deadlock** is a **liveness** failure: *nothing* happens — a set of threads each wait forever for a lock another holds, so none can proceed and none will ever release. The program **hangs** — no crash, no CPU use, just frozen. (Contrast livelock in 6d, where threads *are* busy but still make no progress.)

Formally: *"a deadlock occurs when a thread enters a waiting state because a requested resource is held by another waiting thread, which in turn is waiting for another resource held by [the first]."* `[S3]`

### 5a. The canonical two-lock deadlock

Two locks, `A` and `B`. Two threads that grab them in **opposite orders**:

```
   Thread 1                         Thread 2
   acquire(A)                       acquire(B)
   ... (small delay) ...            ... (small delay) ...
   acquire(B)   ← wants B           acquire(A)   ← wants A
   ...                              ...
   release(B); release(A)           release(A); release(B)
```

Watch the fatal interleaving (time downward):

```
   time │ Thread 1            │ Thread 2            │ who holds what
   ─────┼─────────────────────┼─────────────────────┼───────────────────────
    t1  │ acquire(A) ✓        │                     │ T1 holds A
    t2  │                     │ acquire(B) ✓        │ T1:A   T2:B
    t3  │ acquire(B) …blocks  │                     │ T1 waits for B (T2 has it)
    t4  │                     │ acquire(A) …blocks  │ T2 waits for A (T1 has it)
   ─────┴─────────────────────┴─────────────────────┴───────────────────────
         T1 waits for T2, T2 waits for T1 — forever. DEADLOCK.
```

Each holds what the other needs, and neither will let go until it gets the other. That's a **circular wait**, and it's frozen. (This is demo step 2 — we force exactly this, detect the hang with a timeout, then fix it.) The classic real-world shape is L02's `transfer`: `transfer(X→Y)` locks X then Y, while `transfer(Y→X)` locks Y then X — opposite orders, same trap.

### 5b. The four necessary conditions (Coffman conditions)

A deadlock can arise **only if all four** of these hold at once — established by Coffman, Elphick & Shoshani (1971) `[S3]`:

1. **Mutual exclusion** — the resource can't be shared; only one thread may hold it at a time. (That's what a lock *is*.)
2. **Hold and wait** — a thread holds at least one resource *while* requesting another.
3. **No preemption** — a resource can only be released *voluntarily* by its holder; nobody can forcibly take it away.
4. **Circular wait** — there's a cycle of threads T1→T2→…→Tn→T1, each waiting on a resource the next one holds. `[S3]`

Why this matters practically: **break *any one* of the four and deadlock is impossible.** So the whole cure menu (Part 6) is just "pick a condition to attack" — most often the fourth. `[S3]`

---

## Part 6 — Breaking deadlock (and its cousins)

### 6a. Lock ordering — attack **circular wait** (the practical default)

Impose a **global order** on locks and require every thread to acquire them in that order. If everyone takes `A` before `B`, you can never have one thread holding `A` waiting for `B` while another holds `B` waiting for `A` — the cycle is impossible by construction. This is the standard fix: *"approaches that avoid circular waits include using a hierarchy to determine a partial ordering of resources … even the memory address of resources has been used to determine ordering, and resources are requested in increasing order."* `[S3]`

```
   transfer(from, to, amt):
       // ALWAYS lock the two accounts in a FIXED global order (e.g. by id),
       // regardless of which is 'from' and which is 'to'
       (first, second) = order_by_id(from, to)
       acquire(first.lock)
           acquire(second.lock)
               from.balance -= amt
               to.balance   += amt
           release(second.lock)
       release(first.lock)
```

- **Buys:** deadlock-free, keeps full fine-grained parallelism, cheap. **Costs:** you must *know* all the locks a path takes and be able to order them — hard when locks are acquired deep in unknown/foreign code (see rule 4). This is the fix to *name first* in an interview.

### 6b. `TryEnter` / timeout — attack **hold-and-wait / no-preemption**

Instead of waiting forever, try to take the second lock with a **timeout**; if it fails, **release the first lock**, back off, and retry. `Monitor.TryEnter` supports "the amount of time during which a thread attempts to acquire a lock." `[S2]`

- **Buys:** breaks the wait — no permanent freeze even if ordering is impossible. **Costs:** more complex; wasted work on abort/retry; and if everyone retries in lockstep you can get **livelock** (6d) or **starvation**.

### 6c. Coarser lock — attack the whole problem

Fewer locks = fewer chances to hold-two-at-once. In the extreme, **one** lock can't deadlock against itself. This is the deep tie to Part 3: coarsening trades **scalability for simplicity/safety**. Sometimes the right call; often too blunt.

> **The senior deadlock one-liner:** *"Two locks taken in opposite orders is the classic deadlock. My default fix is a **global lock ordering** so a cycle can't form; if I can't order them, I use **try-with-timeout and back off**; and if the contention doesn't justify multiple locks at all, I **coarsen** to one. I also keep critical sections short and never call foreign code while holding a lock, which is where surprise deadlocks come from."*

### 6d. Livelock — the busy cousin

**Livelock** looks like the opposite of deadlock but has the same result: *"the states of the processes constantly change with regard to one another, none progressing."* `[S3]` Two threads each detect a conflict, both politely back off, both retry, both collide again — like two people stepping side-to-side in a hallway forever. They're **not blocked** (CPU is busy!), yet **no progress** is made. Livelock is a special case of **starvation** (a thread perpetually denied progress) `[S3]`. The fix is usually to break the symmetry — randomized backoff, or letting only one party retry `[S3]`.

> **Say the trio cleanly:** *deadlock* = frozen, waiting forever (liveness fail, no CPU). *Livelock* = busy, retrying forever, no progress (liveness fail, wasted CPU). *Starvation* = one unlucky thread never gets a turn while others do. All three are **liveness** failures — distinct from a **race**, which is a **safety** failure.

---

## Part 7 — The "ship": fix the race, then force & fix a deadlock

The runnable demo lives at [../code/Lessons/Lesson03Locks.cs](../code/Lessons/Lesson03Locks.cs). Run it:

```bash
cd systems_programming/code
dotnet run -- 03-locks
```

Two acts, mapped to this lesson:

1. **Fix L02's race with a lock (Part 1).** The same shared-counter workload from Lesson 02, but the increment now runs inside `lock (m) { ... }`. Where the unsynchronised version lost updates on most runs, the locked version is **exact every run** — mutual exclusion makes the three-step read-modify-write indivisible.
2. **Force a deadlock, then fix it (Parts 5–6).** Two threads take two locks in **opposite orders** with a tiny delay between, reliably producing the Part-5a circular wait. The demo runs them on background threads and **detects the hang with a timeout** (so it doesn't freeze your terminal), printing `DEADLOCK detected`. It then reruns with **consistent lock ordering** (Part 6a) and completes cleanly — same two locks, only the *acquisition order* changed.

Predict before running: act 1's locked counter is exact (0% races); act 2's opposite-order version hangs and is reported as a deadlock, while the ordered version finishes. Seeing the hang — no crash, no output, just *stuck* until the watchdog fires — is the whole "liveness failure" lesson in one screen.

---

## Self-check (say the answers out loud, as if teaching)

1. What single guarantee does a **lock** provide? Write the `acquire/release` pseudocode that fixes L02's `count++`, and explain why the stale-read window disappears. What are **safety** and **liveness**, and which one does deadlock violate?
2. A thread calls `acquire` on a lock someone else holds. Describe what happens under a **blocking** lock vs a **spinlock** — what each does while waiting, what each costs (tie the blocking cost to the L01 context-switch model), and when you'd pick each.
3. Define **lock granularity**. Contrast one coarse lock over a whole cache with **lock striping**: what does each buy and cost? Deliver the "start coarse, split only where a profiler shows contention" one-liner and say *why* that order.
4. Give **four design rules** for using locks well and the reason for each. Why must you never call foreign code while holding a lock? Why lock a *reader* even though it "only reads"?
5. Define **deadlock** and walk the two-lock, opposite-order example step by step. Why is it a **liveness** failure and not a safety one? How does it differ from a **race**?
6. Name the **four Coffman conditions**. For each, state the fix that removes it. Which is the one you attack in practice, and what's the concrete technique?
7. What is **livelock**, and how does it differ from deadlock (be precise about CPU use and progress)? What is **starvation**? Give the fix for livelock and why symmetry-breaking works.

If any answer is shaky, re-read that Part before Lesson 04 — **Memory model & visibility** — which explains a *second*, sneakier hazard: even a write that has fully *finished* may not be *seen* by another thread, and why a lock quietly fixes that too.

---

## Sources

Facts above are cited inline by tag. Pulled/verified **2026-07-16**. Microsoft Learn is authoritative for .NET synchronization semantics; the deadlock conditions are the standard Coffman formulation.

- `[S1]` Microsoft Learn — *The `lock` statement (C# reference)*. "The `lock` statement acquires the mutual-exclusion lock for a given object, executes a statement block, and then releases the lock … Any other thread is blocked from acquiring the lock and waits until the lock is released. The `lock` statement ensures that at most only one thread executes its body at any moment in time." Reentrancy ("while a lock is held, the thread that holds the lock can acquire and release the lock multiple times"); expansion to `Monitor.Enter/Exit` in `try/finally` (lock released even on exception); guidelines — hold the lock as short as possible to reduce contention, use a dedicated private lock object, avoid locking `this`/`Type`/`string`, avoid using one lock instance for different resources (deadlock or contention). https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/statements/lock (page ms.date 2026-01-16; retrieved 2026-07-16).
- `[S2]` Microsoft Learn — *Overview of synchronization primitives (.NET)*. `Monitor` grants mutually exclusive access; a blocked thread's `Monitor.Enter` waits until the lock is released; `Monitor`/`Mutex` have **thread affinity** (the acquiring thread must release). `SpinLock` "waits in a loop, repeatedly checking until the lock becomes available." `SpinWait` spins briefly then yields "only if the condition was not met in the specified time." `Monitor.TryEnter` can specify a timeout. `ReaderWriterLockSlim` allows multiple readers or a single writer. `Interlocked` provides atomic operations. "Use the same synchronization primitive instance to protect access of a shared resource; … different instances … circumvent the protection." https://learn.microsoft.com/en-us/dotnet/standard/threading/overview-of-synchronization-primitives (retrieved 2026-07-16).
- `[S3]` Wikipedia — *Deadlock (computer science)*, "Conditions", "Deadlock handling → Prevention", and "Livelock". Deadlock definition (a thread waits for a resource held by another waiting thread…); the four **Coffman conditions** — mutual exclusion, hold and wait, no preemption, circular wait — from Coffman, Elphick & Shoshani (1971); prevention by breaking any one condition, "especially the fourth"; circular-wait avoidance via a partial ordering/hierarchy of resources (even by memory address, acquired in increasing order); livelock ("states constantly change … none progressing"; a special case of resource starvation; fixed by ensuring only one party acts). https://en.wikipedia.org/wiki/Deadlock_(computer_science) (page last edited 2026-06-30; retrieved 2026-07-16). Primary source for the conditions: Coffman, E. G.; Elphick, M. J.; Shoshani, A. (1971), "System Deadlocks", *ACM Computing Surveys* 3 (2): 67–78.

**Not independently re-benchmarked this run (flagged):** the "~2 context switches" cost of a contended blocking acquire is the qualitative L01 model (park + wake), not a measured figure; exact costs vary by OS/hardware. "Spin when short + spare cores, block when long" is the standard rule of thumb, stated qualitatively. The five design rules synthesize the cited MS Learn guidance ([S1],[S2]) into a checklist; the individual points are sourced, the grouping is pedagogical.
