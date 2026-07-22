# 06 — Higher-level primitives (semaphore, read-write lock, events/latches, countdown latch, barrier)

Everything so far has been **raw materials**. Lesson 03 gave you the **lock** — at most one holder, mutual exclusion. Lesson 04 gave you **visibility** — barriers, atomics, CAS. Lesson 05 gave you the **condition variable** — *wait until a predicate is true*, with `wait`/`signal`/`broadcast` and the `while`-not-`if` discipline. With just those, you can build any concurrent coordination — but you'd be hand-rolling the same three or four patterns over and over, and getting the broadcast-vs-signal and re-check details wrong each time (Lesson 05, Part 5a, was a preview of how fiddly "close the queue correctly" already is).

This lesson is the **power-tool drawer**: five named primitives that each **crystallize a recurring lock + condition-variable pattern into one object with a correct interface**. They don't add new *physics* — every one of them can be built from a lock plus a condition variable (Part 6 shows the recipes) — but they package the pattern so you (a) don't re-derive it, (b) don't get it subtly wrong, and (c) *name the pattern* in the interview so the interviewer instantly knows what you mean. By the end you should be able to, cold:

- Define a **semaphore** as a permit counter (`acquire` decrements/blocks-at-zero, `release` increments/wakes one), and state the one distinction that trips people up: a **semaphore is not a mutex** — it has **no owner / no thread affinity**, so any thread can release it.
- Use a semaphore to **bound concurrency** (throttle to *N* at once) and to model a **resource pool** (N connections), and connect it back to Lesson 05's bounded queue (two semaphores + a mutex).
- Define a **read-write lock** (many readers **xor** one writer), say exactly *when* it beats a plain lock (read-heavy **and** critical sections long enough to amortize its higher overhead), and explain **writer starvation** and the fairness knob that addresses it.
- Define an **event / latch** (a manual gate you `set` and `reset`), distinguish **manual-reset** (gate: wakes *all*, stays open) from **auto-reset** (turnstile: wakes *one*, re-closes), and use it for a "start signal."
- Define a **countdown latch** (wait until *N* things have happened — fork/join / scatter-gather) and contrast it with a **barrier** (N peers **rendezvous repeatedly** across phases).
- Given a coordination problem, **pick the right primitive** and justify it — and, if asked, sketch how it's built from a lock + condition variable.

As always we stay in **pseudocode**; the C# realizations (`SemaphoreSlim`, `ReaderWriterLockSlim`, `ManualResetEvent(Slim)` / `AutoResetEvent`, `CountdownEvent`, `Barrier`) are short language notes, and consolidated at the end.

---

## Part 0 — Why higher-level primitives at all

You *can* build all of today's tools out of Lesson 05's `lock + condition variable`. So why learn five more names? Because a raw lock + CV is **too low-level to use safely at scale**: every time you need "let at most 8 threads in at once" or "wait for all 20 workers to finish," you'd re-write the same `while (predicate) wait(cv)` loop, re-decide `signal` vs `broadcast`, and re-check the condition after every wake — and Lesson 05 showed how many ways that goes wrong (lost wakeup, thundering herd, `if`-instead-of-`while`, forgetting to hold the lock). A higher-level primitive is that pattern, **written once, correctly, by the runtime**, exposed as an object whose name *is* the intent.

Think of it as the same move as data structures. You *could* use a raw array everywhere; instead you reach for a stack, a queue, a hash map — each an array-plus-discipline with a name that announces what it's for. These primitives are the concurrency equivalent.

Here is the whole drawer at a glance — read it now, and it'll make sense in detail by the end:

| Primitive | One-line intent | The question it answers |
|---|---|---|
| **Semaphore** | *N permits* — let at most N through | "How many may be inside at once?" |
| **Read-write lock** | many readers **xor** one writer | "Is this read-mostly? Let readers share." |
| **Event / latch** | a gate you open and close | "Has the thing happened yet? Wait for the signal." |
| **Countdown latch** | wait for **N events** to occur | "Are all N sub-tasks done? (fork/join)" |
| **Barrier** | **N peers** rendezvous each phase | "Has everyone finished this round before we start the next?" |

> **Frame for the room:** *"A lock and a condition variable are the assembly language of coordination. These five are the standard library — each is a specific lock-plus-condition-variable pattern packaged so I don't re-derive it and, more importantly, so I can *name* the pattern. In an interview I'll reach for the named tool first and only drop to raw lock+CV if the tool doesn't fit."*

---

## Part 1 — The semaphore: a counter of permits

A **lock** answers a yes/no question: *is anyone inside?* (at most **one** holder). A **semaphore** generalizes that to a **number**: *how many are inside?* It holds an integer **permit count** and exposes two operations:

| Op | Classic names | Effect |
|---|---|---|
| **acquire** | `wait`, `P`, `down` | If count > 0, decrement and proceed. If count == 0, **block** until a permit is released. |
| **release** | `signal`, `V`, `up` | Increment the count; if anyone is blocked, wake **one** of them. |

(The historical names `P`/`V` are Dijkstra's; `down`/`up` are Linux's; `Wait`/`Release` are .NET's. Same two verbs.) The count starts at some **initial value** `N`, and the invariant is simply: **at most `N` threads are past a successful `acquire` at any instant.** A permit is a token; there are `N` tokens; you must hold one to be inside; you drop it on the way out.

```
   semaphore s = N          // N permits available

   acquire(s):              // "take a permit"
       while s.count == 0:  // no permits → wait  (a condition variable underneath, Lesson 05)
           wait(s.cv, s.m)
       s.count -= 1

   release(s):              // "return a permit"
       s.count += 1
       signal(s.cv)         // wake ONE waiter — one permit freed, one thread may proceed
```

Notice this **is** a Lesson 05 waiter/signaler pair: the predicate is `count > 0`, checked in a `while`, and `release` `signal`s exactly one (one permit → one wakeup → no thundering herd). The semaphore is that pattern with a friendlier face.

### 1a. The distinction that trips everyone: a semaphore is **not** a mutex

This is the single most-tested conceptual point about semaphores, so nail it. A **binary semaphore** is a semaphore with `N = 1`, so it *looks* like a lock: one thread in at a time. But it differs from a real mutex in a way that matters:

- **A mutex has an *owner* (thread affinity).** The thread that locked it is the only thread allowed to unlock it. This is what makes a mutex safe for mutual exclusion and (in C#) able to be **reentrant** — the owner can re-enter its own lock.
- **A semaphore has *no owner*.** It's just a counter. **Any** thread can `release` — including a thread that never `acquire`d. Microsoft states this explicitly: the semaphore types *"don't enforce thread identity on calls to … `Wait` … and `Release`"* — the canonical use is even *"one thread always incrementing the semaphore count and the other always decrementing it"* `[S6]`.

That "no owner" property is a **feature**, not a bug — it's exactly why a semaphore can express **producer signals, consumer waits** (the producer *releases* a permit it never *acquired*). But it also means a binary semaphore gives you **none of the safety a mutex does**:

- If you use a binary semaphore as a lock and a bug calls `release` one time too many, the count climbs to 2 and now **two** threads get in — silent mutual-exclusion failure. (C# even throws `SemaphoreFullException` if you exceed the declared max — a guard rail, but the logic bug is yours `[S6]`.)
- A binary semaphore is **not reentrant**: if the **same thread that already holds the permit** calls `acquire` again, it blocks on **itself** → instant self-deadlock. (A *different* thread calling `acquire` while the permit is held is **not** a deadlock — that's just normal contention, the intended mutual-exclusion behavior; it waits and proceeds once the holder `release`s.) The semaphore can't distinguish the two cases because it has **no owner** — it only sees `count == 0` and blocks. A `lock`/`Monitor`, which tracks the owning thread plus a recursion count, would recognize the owner and happily re-enter.

> **Say it cleanly:** *"A semaphore is a permit counter, not a lock. A binary semaphore (N=1) resembles a mutex but has no owner — any thread can release it, it's not reentrant, and an extra release silently lets two threads in. So I use a real mutex for mutual exclusion, and a semaphore when I genuinely want to count permits or hand a permit across threads."*

### 1b. What semaphores are actually for

Two everyday, interview-relevant uses:

**(1) Bound concurrency — the throttle.** You have 10 000 tasks but must not hammer a downstream service with more than **8** concurrent requests. A semaphore initialized to 8 is the throttle: each task `acquire`s before the call and `release`s after; the 9th task blocks until one of the 8 finishes. This is a *reliability/scalability* control — it caps load, memory, and connections regardless of how many tasks pile up.

```
   semaphore limit = 8                 // at most 8 concurrent downstream calls

   handleTask(t):
       acquire(limit)
       try:    callDownstream(t)        // ≤ 8 of these run at once
       finally: release(limit)          // ALWAYS release, even on exception
```

(The `try/finally` is not decoration: if `callDownstream` throws and you skip `release`, you **leak a permit** forever, and after 8 leaks the throttle is permanently jammed at zero — a slow, ugly outage. Permit accounting must be exception-safe.)

**Why holding the permit *across* the network call is fine here — unlike a lock.** L03's rule "never hold a lock across slow/blocking I/O" does **not** apply, because the throttled region is **not a critical section**: recall the precise definition (shared state + mutual exclusion), and the throttle has *neither*. The permit guards no data — it *counts in-flight operations* — and it's not mutually exclusive: up to 8 threads run their calls **simultaneously**, blocking no one but the 9th. The L03 rule is about a **mutex that serializes everyone behind you** (holding *that* across I/O queues all other threads = the contention disaster); a throttle **deliberately doesn't serialize**, it rate-limits. And holding across the call is **mandatory, not incidental** — the thing you're bounding (concurrent downstream requests / connections / memory) is consumed for the *whole duration* of the call, so the permit must span it; release it early and you'd be bounding nothing. **The one caveat:** if you *also* mutate shared state inside the throttled region, don't use the permit for that — take a **separate, brief mutex** for just that mutation and keep it off the I/O path (exactly as L03 says). Two primitives, two jobs: the semaphore *counts/gates*, the mutex *mutually excludes*.

**(2) A resource pool.** You have a pool of *N* expensive reusable resources — database connections, worker slots, buffers. A semaphore initialized to `N` gates checkout: `acquire` to take a connection, `release` to return it. The count *is* "connections currently available." (This is the throttle again, viewed as owning objects rather than call slots.)

### 1c. Tie-back: Lesson 05's bounded queue is two semaphores + a mutex

The bounded producer–consumer you built by hand in Lesson 05 (two condition variables `notFull`/`notEmpty`) has a classic textbook form using **two counting semaphores plus a mutex**:

```
   semaphore emptySlots = capacity      // permits = free space; producers acquire
   semaphore fullSlots  = 0             // permits = items ready; consumers acquire
   mutex m                              // protects the buffer itself

   put(x):                              // producer
       acquire(emptySlots)              // wait for a free slot (blocks when full → backpressure)
       lock(m); buffer.enqueue(x); unlock(m)
       release(fullSlots)               // announce: one more item to take

   take():                              // consumer
       acquire(fullSlots)               // wait for an item (blocks when empty)
       lock(m); x = buffer.dequeue(); unlock(m)
       release(emptySlots)              // announce: one more free slot
       return x
```

Two things worth seeing. First, **the two semaphores encode the two waiting conditions** (`notFull` ⇒ `emptySlots`, `notEmpty` ⇒ `fullSlots`) that were CVs in Lesson 05 — but now the *count itself* carries the state, so there's no separate predicate to re-check. Second, you **still need the mutex** for the `enqueue`/`dequeue`, because a semaphore counts permits but does **not** make the buffer mutation atomic — that's the L03 job, and it's a clean illustration of "different primitives, different jobs": semaphores for *counting/gating*, the mutex for *mutual exclusion*. `acquire(emptySlots)` when the queue is full is exactly Lesson 05's **backpressure** — the fast producer is throttled to consumer speed.

**Language note (semaphore).** C# has two: **`Semaphore`** — a thin wrapper over a Win32 kernel semaphore, can be **named/system-wide** (cross-process) `[S6]`; and **`SemaphoreSlim`** — a lightweight, single-process semaphore that's *"the recommended semaphore for synchronization within a single app"* `[S1]`. `SemaphoreSlim.Wait()`/`Release()` are `acquire`/`release`; the constructor takes `(initialCount, maxCount)`; `CurrentCount` is the live permit count `[S1]`. Like Lesson 03's hybrid lock, `SemaphoreSlim` **spins briefly before blocking** — *"during the spin-wait phase, the CPU is actively spinning"* — so it's tuned for short waits `[S6]`. There is **no FIFO guarantee**: *"if multiple threads are blocked, there is no guaranteed order … that controls when threads enter the semaphore"* `[S1]` — do not rely on wake order for fairness. (`SemaphoreSlim` also has `WaitAsync()` for the async world — Lesson 08. Named semaphores are system-wide and therefore a **DoS surface** — another process can open the same name — so protect them with access control `[S6]`.)

---

## Part 2 — The read-write lock: many readers **xor** one writer

A plain lock (L03) is **pessimistic and symmetric**: it serializes *everyone*, readers included. But two threads that only **read** shared state don't conflict — reads don't corrupt each other (L02's races need a *writer*). Serializing readers against each other is throughput left on the floor, and for **read-heavy** data (a config map, a cache, a routing table read thousands of times per write) that floor is most of your performance.

A **read-write lock** (a.k.a. shared/exclusive lock) exploits this. It has **two modes**:

- **Read (shared) mode** — **any number** of readers may hold it simultaneously.
- **Write (exclusive) mode** — **exactly one** writer, and while it's held **no readers** and no other writer may enter.

The invariant in one line: **readers XOR a writer** — either N readers and no writer, or one writer and nobody else. Microsoft: *ReaderWriterLockSlim* *"allows multiple threads to be in read mode, allows one thread to be in write mode with exclusive ownership of the lock"* `[S2]`.

```
   rwlock L

   read:                          write:
       L.enterRead()                  L.enterWrite()      // waits for all readers AND any writer to leave
       ... read shared state ...      ... mutate shared state ...
       L.exitRead()                   L.exitWrite()
```

```
   Legend: R = reader inside, W = writer inside

   [ R R R R ]      ok — readers share
   [ W ]            ok — writer alone
   [ R R  W ]       FORBIDDEN — reader/writer overlap
   [ W W ]          FORBIDDEN — two writers
```

### 2a. When it actually wins — and when it's a trap

A read-write lock is **more expensive per operation** than a plain lock: it must track a reader count, a writer flag, and two waiting sets, and coordinate the mode transitions. So it is **not** a free upgrade. It pays off only when **both** are true:

1. **Reads vastly outnumber writes** (say 90%+ reads). If writes are frequent, writers keep forcing everyone out and you get plain-lock behavior *plus* the RW-lock's extra bookkeeping — strictly worse.
2. **Critical sections are long enough** to amortize that bookkeeping. If each read is a couple of instructions (a single field read), the RW-lock's own overhead dwarfs the work, and a plain `lock` — or, better, a **lock-free `volatile` read** (L04) — wins. (Note the read tool here is `volatile`/`Volatile.Read`, *not* `Interlocked`: Interlocked is the read-modify-**write** family — `Increment`/`Exchange`/`CompareExchange` — so it's the tool for the *update* side, not for reads. `Volatile.Read` inserts the acquire barrier that gives you L04 visibility `[S7]`. The lone `Interlocked.Read` exists only for 64-bit `long` on 32-bit platforms to dodge a torn read, and is itself implemented via `CompareExchange` — i.e. a RMW trick, not a read primitive `[S8]`.) RW-locks shine when a read *holds the lock for a while* (iterating a structure, doing a multi-field consistent read).

> **The senior nuance (say the second half):** *"A read-write lock only helps when reads dominate **and** each critical section is long enough to amortize its higher overhead. For very short reads a plain lock is cheaper, and for a single value I'd skip locks entirely — a `volatile` read for a plain field (Interlocked only if I also need an atomic update). 'Read-heavy' alone isn't sufficient — the read has to be *big* enough too."*

### 2b. Writer starvation and the fairness knob

The subtle failure mode: if readers can *always* barge in whenever any reader is present, a steady stream of overlapping readers can keep the lock in read mode **forever**, and a waiting **writer never gets exclusive access** — **writer starvation** (a specific case of L03's starvation). Real implementations therefore bias toward writers. .NET's policy: *"a thread that tries to enter read mode blocks if there are threads waiting to enter write mode … Blocking new readers when writers are queued is a lock fairness policy that favors writers … to promote throughput in the most common scenarios"* `[S2]`. In other words, **once a writer is waiting, new readers queue behind it** so the in-flight readers drain and the writer gets its turn. That's the standard cure for writer starvation, and being able to name it is the graded point.

### 2c. Upgradeable read — avoiding the classic RW deadlock

A common shape: *read, and only if some condition holds, write* (check-then-act on shared state — an L02 race shape). The naive move is "take a read lock, and if I need to, upgrade to a write lock." That **deadlocks**: if two threads both hold read locks and both try to upgrade to write, each waits for the other to drop its read lock — neither will. .NET forbids the plain upgrade for exactly this reason and offers a dedicated **upgradeable read mode**: *at most one* thread may be in upgradeable mode at a time (others can still read), and from it that one thread can safely promote to write without the deadlock `[S2]`. The trade is that upgradeable mode is *semi-exclusive* — only one upgrader at a time — so use it only for the read-then-maybe-write pattern, not as a default reader mode.

### 2d. Where this is going (L09) — with a caveat that ties straight back to 2a

You'll constantly hear "use a read-write lock for a cache," but hold that against the rule from 2a. A cache **hit is a short read** (hash the key, probe a bucket, return), so a *single* RW-lock is **not** automatically justified — and for reads this short it can be **no better than, or worse than, a plain `lock`**: every `enterRead`/`exitRead` mutates the lock's **shared reader-count**, a contended cache line, so the readers serialize on the lock's *own bookkeeping* (cache-line ping-pong across cores) rather than on the data. (Mechanism argument from L02/L04 coherence, not a benchmarked figure — flagged.) A RW-lock only earns its place on a cache when the read side is genuinely *not* short: scanning/iterating entries, computing an aggregate, or taking a consistent multi-key snapshot — reads that *hold the lock for a while* (2a's second condition). For a pure short-`get` cache it's a wash.

The **scalable** answer therefore isn't a single RW-lock at all — it's **lock striping / a concurrent map** (L07): shard the keyspace into N independently-locked buckets so writes to different shards don't contend, and reads mostly go lock-free. That's what production caches use. (The L03 "granularity" theme resurfacing: one coarse lock — even a RW one — serializes everything; split it.)

**What about a miss** (the natural "but isn't a miss expensive?" objection)? The miss *lookup* is still short — you probe and find nothing. What's expensive is **loading** the value (DB/recompute), and you must **not** hold the read lock across that (the L03 / throttle rule) — you release, load *outside* the lock, then take a write lock to insert. So a miss does **not** lengthen the read critical section; it's really a **read-then-write** (exactly where **upgradeable read** from 2c fits — the `AddOrUpdate` shape `[S2]`) plus a **single-flight / cache-stampede** concern (the L03 re-drill: a per-key lock or an in-progress future so N threads don't all reload the same missing key). Neither argues for a whole-map RW-lock.

**OO/design framing (graded).** Wrapping the RW-lock inside the structure — a `SynchronizedCache` that exposes `Read`/`Add`/`AddOrUpdate` and hides the lock entirely — is the SRP/encapsulation the round grades: callers never touch `enterRead`/`enterWrite`, the locking policy lives in one place, and you can swap RW-lock → striping later with **zero caller changes**. .NET's own docs demonstrate exactly this `SynchronizedCache` shape `[S2]`.

**Language note (RW-lock).** C# = **`ReaderWriterLockSlim`** (use this, not the legacy `ReaderWriterLock` — the Slim version is faster and has *"simplified rules for recursion and for upgrading and downgrading"* and *"avoids many cases of potential deadlock"* `[S2]`). Methods: `EnterReadLock`/`ExitReadLock`, `EnterWriteLock`/`ExitWriteLock`, `EnterUpgradeableReadLock`, plus `TryEnter…Lock(timeout)` (the L03 timeout escape hatch). Default is **`NoRecursion`** — recommended, because recursion *"introduces unnecessary complications and makes your code more prone to deadlocks"* `[S2]`. It implements `IDisposable` — wrap in `using`.

---

## Part 3 — Events and latches: a gate you open and close

The next family answers a different question: not *"can I get in?"* (mutual exclusion) or *"how many?"* (semaphore), but *"has the moment arrived yet?"* — one or more threads wait for a **signal** from elsewhere. This is Lesson 05's condition variable, packaged as a standalone **boolean gate** with no lock or predicate to manage yourself. The primitive is called an **event** (or **latch**, or **manual gate**), and it has two states — **nonsignaled** (gate closed, waiters block) and **signaled** (gate open, waiters pass) — and three operations:

| Op | Who calls it | Blocks the caller? | Effect |
|---|---|---|---|
| **set** (signal) | the controller | no | Flip the gate to *signaled* — release waiter(s). |
| **reset** | the controller | no | Flip the gate back to *nonsignaled* — so *future* `wait`s will block. |
| **wait** | a waiter | **yes, if closed** | If signaled, pass immediately; else **block the calling thread** until someone `set`s it. |

**`reset` vs `wait` are near-opposites, don't conflate them:** `set`/`reset` are the **controller's** verbs — they change the gate's *state* and **never block the caller** (you flip the gate and move on); they only determine what *future* waiters will do. `wait` is the **waiter's** verb — it doesn't change the gate at all (a manual event; an auto event auto-clears as a side effect of passing), and it's the **only** op that can put the *calling* thread to sleep. Door analogy: `set` = prop the door open; `reset` = **shut it and walk away** (whoever arrives later waits); `wait` = **walk up to the door and stand there if it's shut** until someone opens it.

The critical design axis is **what happens after a `set`**, and it splits the family in two:

### 3a. Manual-reset (a broadcast gate) vs auto-reset (a turnstile)

- **Manual-reset event = a gate.** `set` opens the gate and it **stays open**: it releases *all* currently-waiting threads **and** every future `wait` passes straight through, until someone explicitly calls `reset`. Microsoft: *"Once it has been signaled, ManualResetEvent remains signaled until it is manually reset … calls to WaitOne return immediately"*, and `Set` *"releases all"* waiting threads `[S3]`. Use it for a **one-to-many "go" signal**: "initialization is done — every worker may start now."

- **Auto-reset event = a turnstile.** `set` releases **exactly one** waiting thread and then **automatically flips back** to nonsignaled. Microsoft contrasts it directly: `AutoResetEvent` *"releases threads one at a time, resetting automatically after each release"* `[S3]`. Use it to hand off to **one** worker at a time (a poor man's one-permit handoff).

```
   MANUAL-RESET (gate):                 AUTO-RESET (turnstile):
     set  → [ open ]  all pass,           set  → lets ONE waiter through,
            stays open until reset                then auto-closes behind it
     ┌───────────────┐                    ┌───┐
     │ ==>  ==>  ==>  │  (everyone)        │=> │  (one, then shut)
     └───────────────┘                    └───┘
```

```
   // "start signal" — many workers wait for one go
   event startGate = manualReset(initiallyClosed)

   worker():                            main():
       startGate.wait()                     loadConfig(); warmCaches()
       doWork()                             startGate.set()     // open once → ALL workers go
```

### 3b. The lost-wakeup trap returns

Events carry the same hazard as raw condition variables (L05, Part 1). The signaled/nonsignaled state is remembered, which *helps* — a `set` before any `wait` is **not** lost; the next waiter sees the gate already open and passes. But an **auto-reset** event remembers **at most one** pending signal, and that's where signals get lost: if two `set`s land **close together — the second before the first has released a thread** — they coalesce into one. Microsoft is explicit: *"If two calls [to Set] are too close together, so that the second call occurs before a thread has been released, only one thread is released. It's as if the second call did not happen"* `[S9]`. So the bug needs **two waiters** (two units of work): you `set` twice meaning to wake both, both sets land while the event is still signaled, only **one** waiter is released, and the second is stranded.

(The *benign* case — which is **not** a bug — is when the two `set`s are spread out in time: the first releases a waiter and the event auto-resets *before* the second arrives, so the second simply signals the next waiter, or harmlessly leaves the gate open if none is waiting. Nothing is lost there — and with only one waiter, one release is the *correct* outcome. The loss is specifically the **coalescing** of near-simultaneous sets, and it only bites when you needed more than one release.)

The lesson from L05 holds: **the boolean state is the truth; a single signal is a nudge — it does not accumulate.** For "wait for a *count* of things," don't hand-roll events — use the countdown latch (Part 4), which is built to accumulate exactly N signals.

**Language note (events).** C#: **`ManualResetEvent`** / **`AutoResetEvent`** (kernel-backed `WaitHandle`s: `Set()`, `Reset()`, `WaitOne()` `[S3]`), and the lightweight single-process **`ManualResetEventSlim`** (*"a lightweight alternative to ManualResetEvent"* `[S3]`) which spins before blocking like the other `…Slim` types. Manual vs auto is chosen by the type (or by `EventResetMode`). Reach for `…Slim` within one process; use the kernel `WaitHandle` versions when you need cross-process signaling or `WaitAll`/`WaitAny` across several handles.

---

## Part 4 — The countdown latch: wait for **N** things to finish (fork/join)

A very common shape: **fan out** work to N workers, then **wait until all N are done** before proceeding (compute partial results in parallel, then combine). A single event can't express "wait for N events" cleanly (Part 3b). The **countdown latch** is built for it: it holds a **count**, initialized to N, and exposes:

| Op | Effect |
|---|---|
| **signal** (count down) | Decrement the count by one. When it hits **zero**, the latch becomes *set* forever. |
| **wait** | Block until the count reaches zero. |

Each worker `signal`s once when it finishes; a coordinator `wait`s; when the last worker signals and the count reaches 0, the coordinator is released. It's a **one-shot** rendezvous — once it hits zero it stays open (you don't count back up). This is the **scatter-gather / fork-join** primitive.

```
   countdown done = N                   // N workers to wait for

   worker(i):                           coordinator():
       computePart(i)                       done.wait()          // blocks until all N have signaled
       done.signal()                        combineResults()     // runs exactly once, after the last worker

   // timeline:  signal signal … signal → count hits 0 → coordinator wakes
```

Two properties to state:

- **The waiter and the signalers are different threads.** The coordinator only waits; workers only signal. (Contrast Part 5's barrier, where the *participants themselves* wait for each other.)
- **The count is bidirectional *while active*, but reaching zero is terminal.** `signal` counts down and `AddCount` can count **up** — register more work mid-flight — so it's *not* strictly one-directional. But once the count hits **zero the latch is *set* and latched open**, and you can no longer add to it: `AddCount` throws and `TryAddCount` returns `false` once it's set `[S4]`. To run another round you must explicitly **`Reset()`** it (safe only when nothing is waiting). So it's effectively **one-shot per arming**; for a rendezvous that re-arms *automatically* every round, that's a **barrier** (Part 5), not a latch.

You've already seen this primitive doing real work: the **Lesson 03 deadlock demo** used a countdown-style watchdog (`CountdownEvent`) to detect that two threads *failed* to complete within a timeout — "wait for N signals, but give up after T" is the timed version of exactly this pattern.

**Language note (countdown).** C# = **`CountdownEvent`** — *"a synchronization primitive that is signaled when its count reaches zero"* `[S4]`. Constructor `CountdownEvent(count)`; `Signal()` decrements `CurrentCount`; `Wait()` blocks until it's set; `InitialCount`/`CurrentCount`/`IsSet` expose state; `AddCount()` and `Reset()` let you re-arm or grow it dynamically (e.g. add more work mid-flight) `[S4]`. In modern C# you'd often express fork/join with `Task.WhenAll` instead — but `CountdownEvent` is the explicit primitive and the one to name when the interviewer talks in threads, not tasks. (Java's equivalent is `CountDownLatch`; the "latch" name comes from there.)

---

## Part 5 — The barrier: N peers rendezvous **every phase**

The **barrier** is the countdown latch's cyclic cousin. Instead of one waiter waiting for N signalers *once*, a barrier synchronizes **N peer threads that all wait for each other, repeatedly, at the end of each phase**. Microsoft: a barrier lets *"a group of tasks cooperate by moving through a series of phases, where each in the group signals it has arrived … and implicitly waits for all others to arrive. The same Barrier can be used for multiple phases"* `[S5]`.

Each participant, at the phase boundary, calls one combined operation — **`signal-and-wait`** ("I've arrived; hold me until everyone else arrives"). When the **last** of the N arrives, the barrier **releases all of them at once** and resets for the next phase.

```
   barrier b = N participants

   participant():
       loop for each phase:
           computeThisPhase()
           b.signalAndWait()       // arrive; block until ALL N have arrived; then everyone proceeds together
```

```
   Phase 0:  T1──arrive─┐
             T2──arrive─┤ (barrier holds all)
             T3──arrive─┘ → last arrives → RELEASE all ↓
   Phase 1:  T1──arrive─┐
             T2──arrive─┤
             T3──arrive─┘ → RELEASE all ↓   … and so on
```

This is the tool for **iterative parallel algorithms** where every thread must finish round *k* before any thread starts round *k+1*: physics/particle simulations stepping time, iterative solvers, phased map/reduce, image-processing passes, game-of-life generations. The barrier guarantees no thread races ahead into the next generation reading half-updated data from the last.

**Countdown latch vs barrier — the contrast to memorize:**

| | Countdown latch | Barrier |
|---|---|---|
| Who waits | a **separate** coordinator | the **participants themselves** |
| Reusable? | **one-shot per arming** (terminal at 0; manual `Reset` to reuse) | **cyclic** — auto-resets every phase |
| Shape | **fork/join** (fan out, join once) | **rendezvous** (lockstep rounds) |
| Count meaning | "N tasks remaining to finish" | "N peers must all reach this line" |

**Language note (barrier).** C# = **`Barrier`** — *"enables multiple tasks to cooperatively work on an algorithm in parallel through multiple phases"* `[S5]`. `SignalAndWait()` is the arrive-and-block op; the constructor `Barrier(participantCount, postPhaseAction)` takes an optional **post-phase action** that runs **once, on the last-arriving thread, after all arrive but before the next phase begins** — the natural place to combine each round's results or check a convergence condition. `CurrentPhaseNumber`, `ParticipantCount`, `ParticipantsRemaining`, and `AddParticipant`/`RemoveParticipant` let you introspect and resize `[S5]`.

---

## Part 6 — Choosing the primitive (and what each is made of)

The whole point of this lesson: **name the pattern.** Given a coordination problem, this table is the decision procedure —

| You need to… | Reach for | Why |
|---|---|---|
| Let **one** thread touch shared state | **mutex / lock** (L03) | Mutual exclusion, owned, reentrant. |
| Let **at most N** through / manage a pool | **semaphore** | A permit counter — throttle, resource pool. |
| Let **many readers or one writer** on read-heavy data | **read-write lock** | Readers share; writer exclusive. Only if reads dominate *and* are long. |
| Broadcast a one-time **"go"** to many | **manual-reset event/latch** | Opens once, everyone passes. |
| Hand off to **one** waiter at a time | **auto-reset event** | Turnstile — releases one, re-closes. |
| **Fork out** N tasks, wait for **all** to finish | **countdown latch** | Fan-out/join, one-shot. |
| N peers march through **phases in lockstep** | **barrier** | Cyclic rendezvous each round. |

And the honest footnote — **none of these are magic**; each is a lock + condition variable (Lesson 05) in a costume, which is why you can always fall back to raw primitives if the tool doesn't fit:

| Primitive | Built from |
|---|---|
| Semaphore | an `int count` + a mutex + one CV (`wait` while `count == 0`; `release` bumps `count`, `signal`s one). |
| Read-write lock | reader-count + writer-flag + two CVs (readers wait while a writer holds; writers wait while readers > 0) + a fairness rule. |
| Manual/auto-reset event | a `bool signaled` + a CV (`broadcast` for manual; `signal` one + clear for auto). |
| Countdown latch | an `int count` + a CV (`signal` decrements; at 0, `broadcast`; `wait` while `count > 0`). |
| Barrier | a count + a **phase/generation** number + a CV (last arrival flips the generation and `broadcast`s; earlier arrivals wait for the generation to change). |

> **Interview-grade close:** *"These five are lock-plus-condition-variable patterns crystallized into named tools. In the room I pick by intent — counter → semaphore, read-mostly → RW-lock, one-time go → manual event, fan-out-and-join → countdown latch, lockstep phases → barrier — and if pressed I can build any of them from a lock and a condition variable, which is what the runtime is doing under the hood."*

---

## Part 7 — Consolidated language note (C# realizations)

You write **pseudocode** in the room; know the mapping so the words are grounded and you can answer "what's the C# type?" instantly.

| Concept (pseudocode) | C# type | Key methods | Note |
|---|---|---|---|
| counting semaphore | `SemaphoreSlim` (single-proc) / `Semaphore` (named, cross-proc) | `Wait()`/`Release()`, `CurrentCount` | Slim spins then blocks; no FIFO order; any thread may `Release` `[S1][S6]`. |
| read-write lock | `ReaderWriterLockSlim` | `EnterReadLock`/`EnterWriteLock`/`EnterUpgradeableReadLock` (+ `TryEnter…`) | `NoRecursion` default; favors writers; `IDisposable` `[S2]`. |
| manual gate / turnstile | `ManualResetEvent(Slim)` / `AutoResetEvent` | `Set()`/`Reset()`/`WaitOne()` | Manual = stays open & wakes all; Auto = wakes one & re-closes `[S3]`. |
| countdown latch | `CountdownEvent` | `Signal()`/`Wait()`, `AddCount()`, `CurrentCount` | Signaled at 0; one-shot (or `Reset`) `[S4]`. |
| barrier | `Barrier` | `SignalAndWait()`, post-phase action | Cyclic; `ParticipantCount`, phases `[S5]`. |

**Stance:** be able to build the primitive from `lock` + `Monitor.Wait`/`Pulse` (shows you understand the mechanism), then say "in production I'd use `SemaphoreSlim` / `ReaderWriterLockSlim` / …" (shows judgment). Building it by hand is the graded skill; naming the library type is the seniority signal.

*(No watch-it-happen demo ships for this lesson — per the track's scoping, the C# harness is reserved for the ~4 lessons where seeing non-determinism matters, L02/L03/L04/L09. These primitives are deterministic by contract; if you'd like a runnable throttle (`SemaphoreSlim`) or reader-writer cache to poke at, say so and I'll add one.)*

---

## Self-check (say the answers out loud, as if teaching)

1. Define a **semaphore** and its two operations. What exactly is the invariant with initial count `N`? Why is a **binary semaphore not a mutex** — give the ownership/affinity difference *and* two concrete consequences (extra `release`, reentrancy).
2. Give **two** real uses of a semaphore (a throttle and a pool). In the throttle code, why is the `release` in a `finally` load-bearing — what breaks if you skip it?
3. Rebuild Lesson 05's **bounded queue with two semaphores + a mutex**. What does each semaphore *count*? Why do you still need the mutex? Which line is the backpressure?
4. Define a **read-write lock** and its invariant. State the **two** conditions that must both hold for it to beat a plain lock, and what you'd use instead when reads are tiny. What is **writer starvation** and how does the fairness policy fix it?
5. **Manual-reset vs auto-reset** event: what happens after `set` in each? Which is a "gate" and which a "turnstile," and give a use for each.
6. **Countdown latch vs barrier:** who waits in each, is each reusable, and what shape (fork/join vs rendezvous) does each express? Give a use case for each.
7. Pick the primitive for: (a) cap concurrent API calls at 8; (b) a config map read 10k×/write 1×; (c) release 50 workers on a single "go"; (d) wait for 20 shards to finish loading, then serve; (e) step a simulation where every thread must finish generation *k* before any starts *k+1*.
8. For **any one** of the five, sketch how you'd build it from a lock + condition variable (Lesson 05).

If any answer is shaky, re-read that Part before Lesson 07 — **Concurrent data structures** — where these primitives stop being standalone tools and get *baked into* structures: a thread-safe queue/map, **lock striping/sharding** (the scalable answer to the L02–L06 "one coarse lock serializes everything" problem, and the real fix for the L09 cache), and the first taste of **lock-free** structures (Treiber stack, the CAS retry loop from L04, and the **ABA** problem it warned about).

---

## Sources

Facts above are cited inline by tag. Pulled/verified **2026-07-22** from Microsoft Learn (authoritative for .NET synchronization-primitive semantics). The *concepts* (counting semaphore, reader/writer lock, latch/event, countdown latch, barrier, binary-semaphore-vs-mutex, writer starvation) are standard concurrency theory; the concrete API guarantees and the .NET fairness/ownership specifics are cited to the docs.

- `[S1]` Microsoft Learn — *SemaphoreSlim Class (System.Threading)*. "Represents a lightweight alternative to Semaphore that limits the number of threads that can access a resource or pool of resources concurrently." "The count is decremented each time a thread enters the semaphore, and incremented each time a thread releases … When the count reaches zero, subsequent calls to one of the Wait methods block until other threads release." "If multiple threads are blocked, there is no guaranteed order, such as FIFO or LIFO, that controls when threads enter the semaphore." "The recommended semaphore for synchronization within a single app." Constructor `(initialCount, maxCount)`; `CurrentCount`; `Wait`/`Release`/`WaitAsync`. https://learn.microsoft.com/en-us/dotnet/api/system.threading.semaphoreslim (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S2]` Microsoft Learn — *ReaderWriterLockSlim Class (System.Threading)*. "Represents a lock that is used to manage access to a resource, allowing multiple threads for reading or exclusive access for writing." "Allows multiple threads to be in read mode, allows one thread to be in write mode with exclusive ownership of the lock, and allows one thread that has read access to be in upgradeable read mode." Fairness: "A thread that tries to enter read mode blocks if there are threads waiting to enter write mode … Blocking new readers when writers are queued is a lock fairness policy that favors writers … to promote throughput in the most common scenarios." Recursion `NoRecursion` by default ("recursion introduces unnecessary complications and makes your code more prone to deadlocks"). "ReaderWriterLockSlim … has simplified rules for recursion and for upgrading and downgrading … avoids many cases of potential deadlock … recommended for all new development." Upgradeable mode + `SynchronizedCache` example; `EnterReadLock`/`EnterWriteLock`/`EnterUpgradeableReadLock`/`TryEnter…`; `IDisposable`. https://learn.microsoft.com/en-us/dotnet/api/system.threading.readerwriterlockslim (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S3]` Microsoft Learn — *ManualResetEvent Class (System.Threading)*. "Represents a thread synchronization event that, when signaled, must be reset manually." "When the controlling thread completes the activity, it calls ManualResetEvent.Set to signal that the waiting threads can proceed. All waiting threads are released." "Once it has been signaled, ManualResetEvent remains signaled until it is manually reset by calling the Reset() method. That is, calls to WaitOne return immediately." Contrast: "the AutoResetEvent class … releases threads one at a time, resetting automatically after each release." "ManualResetEventSlim … is a lightweight alternative to ManualResetEvent." `Set()`/`Reset()`/`WaitOne()`. https://learn.microsoft.com/en-us/dotnet/api/system.threading.manualresetevent (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S4]` Microsoft Learn — *CountdownEvent Class (System.Threading)*. "Represents a synchronization primitive that is signaled when its count reaches zero." `CountdownEvent(count)`; `Signal()` "registers a signal … decrementing the value of CurrentCount"; `Wait()` "blocks the current thread until the CountdownEvent is set"; `InitialCount`/`CurrentCount`/`IsSet`; `AddCount()`, `Reset()`. **Bidirectional-while-active / terminal-at-zero facts:** `AddCount` "Increments the CountdownEvent's current count" but throws `InvalidOperationException` when "The current instance is already set" (https://learn.microsoft.com/en-us/dotnet/api/system.threading.countdownevent.addcount); `TryAddCount` returns `false` "If CurrentCount is already at zero" (https://learn.microsoft.com/en-us/dotnet/api/system.threading.countdownevent.tryaddcount); `Reset()` re-arms CurrentCount to InitialCount and un-sets the event. https://learn.microsoft.com/en-us/dotnet/api/system.threading.countdownevent (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S5]` Microsoft Learn — *Barrier Class (System.Threading)*. "Enables multiple tasks to cooperatively work on an algorithm in parallel through multiple phases." "A group of tasks cooperate by moving through a series of phases, where each in the group signals it has arrived at the Barrier in a given phase and implicitly waits for all others to arrive. The same Barrier can be used for multiple phases." `SignalAndWait()`; constructor `Barrier(participantCount, Action<Barrier> postPhaseAction)` (post-phase action runs once per phase); `CurrentPhaseNumber`/`ParticipantCount`/`ParticipantsRemaining`; `AddParticipant`/`RemoveParticipant`; `BarrierPostPhaseException`. https://learn.microsoft.com/en-us/dotnet/api/system.threading.barrier (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S6]` Microsoft Learn — *Semaphore and SemaphoreSlim (.NET)*. `Semaphore` = "a thin wrapper around the Win32 semaphore object … counting semaphores that control access to a pool of resources"; can be named/system-wide (cross-process). The types "don't enforce thread identity on calls to … Wait … and Release" — canonical use is "one thread always incrementing the semaphore count and the other always decrementing it." Over-releasing throws `SemaphoreFullException`. `SemaphoreSlim` = "lightweight, fast semaphore … for waiting within a single process when wait times are expected to be short … During the spin-wait phase, the CPU is actively spinning." Named semaphores are a DoS surface ("another process that uses the same name can enter your semaphore … basis of a denial-of-service attack"). https://learn.microsoft.com/en-us/dotnet/standard/threading/semaphore-and-semaphoreslim (ms.date 2026-03-12; retrieved 2026-07-22).
- `[S7]` Microsoft Learn — *Volatile.Read Method (System.Threading)*. "Reads the value of a field. On systems that require it, inserts a memory barrier that prevents the processor from reordering memory operations as follows: If a read or write appears after this method in the code, the processor cannot move it before this method." (The read-side acquire-fenced primitive — overloads for all primitive value types + reference types.) Cited in Part 2a to correct that a lock-free *read* uses `volatile`/`Volatile.Read`, not `Interlocked`. https://learn.microsoft.com/en-us/dotnet/api/system.threading.volatile.read (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S8]` Microsoft Learn — *Interlocked.Read Method (System.Threading)*. Overloads only for `Int64`/`UInt64` ("Returns a 64-bit value, loaded as an atomic operation"). Remarks: "The Read method is unnecessary on 64-bit systems, because 64-bit read operations are already atomic. On 32-bit systems, 64-bit read operations are not atomic unless performed using Read." "On 32-bit platforms, despite taking a readonly reference parameter, this method requires write access to the memory location because it uses CompareExchange internally to ensure atomicity." Confirms Interlocked is the read-modify-write family; the sole `Interlocked.Read` is a 64-bit-on-32-bit torn-read fix implemented via CAS, not a general read primitive. https://learn.microsoft.com/en-us/dotnet/api/system.threading.interlocked.read (ms.date 2025-07-01; retrieved 2026-07-22).
- `[S9]` Microsoft Learn — *AutoResetEvent Class (System.Threading)*. "Represents a thread synchronization event that, when signaled, releases one single waiting thread and then resets automatically." "AutoResetEvent remains signaled until Reset is called or a single waiting thread is released, at which time it automatically returns to the non-signaled state." Lost-signal fact: "There's no guarantee that every call to the Set method will release a thread. **If two calls are too close together, so that the second call occurs before a thread has been released, only one thread is released. It's as if the second call did not happen.** Also, if Set is called when there are no threads waiting and the AutoResetEvent is already signaled, the call has no effect." Cited in Part 3b to correct the coalescing (two-waiter) condition for a lost signal. https://learn.microsoft.com/en-us/dotnet/api/system.threading.autoresetevent (ms.date 2025-07-01; retrieved 2026-07-22).

**Not independently re-benchmarked / flagged:** the "90%+ reads" and "reads must be long enough to amortize" thresholds for when a read-write lock beats a plain lock are standard rules-of-thumb, not measured on a specific system — the *direction* (RW-lock has higher per-op overhead than a plain lock and only pays off under read-dominant, non-trivial critical sections) is the robust, sourced-by-mechanism claim. *Mesa-style* wakeup semantics and the "built from a lock + condition variable" recipes are standard OS/concurrency theory; the concrete .NET API guarantees (fairness-favors-writers, no-FIFO-semaphore-order, manual-vs-auto reset behavior, thread-identity-not-enforced) are each cited to the docs above.
