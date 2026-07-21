# 05 — Condition synchronization (condition variables, producer–consumer, the async logger)

Lessons 02–04 taught one verb: **protect**. A lock makes a region indivisible (L03); the memory model makes writes visible (L04). But protection only answers *"don't let two threads corrupt this."* It says nothing about the other half of real concurrent programs: **"don't run yet — wait until something is true."** A consumer must wait until the queue is **non-empty**; a producer must wait until the queue is **non-full**; a worker pool must wait until there's **work**; a shutdown must wait until all workers have **drained**. This lesson is about that missing verb — **waiting for a condition** — and the primitive built for it, the **condition variable**.

This matters because the naive way to wait is catastrophically wrong, and it's the single most common concurrency anti-pattern: **spinning** — `while (queue is empty) { /* nothing */ }` — a thread burning a whole CPU core doing nothing but re-checking. Condition synchronization replaces the busy spin with **"sleep until another thread tells me the world changed."** By the end you should be able to, cold:

- Explain why **busy-waiting** on a condition is wrong (wastes a core, starves the very thread that would make the condition true — the L03 single-core disaster again), and what we want instead: **block, then be woken**.
- Define a **condition variable** and its three operations — **`wait`**, **`signal`** (wake one), **`broadcast`** (wake all) — and the one property that makes it work: **`wait` atomically releases the lock and sleeps**, then re-acquires the lock on wake.
- State the two ironclad rules: **(1)** you use a condition variable **while holding a lock**, and **(2)** you **always re-check the condition in a `while` loop, never an `if`** — and explain *both* reasons (a woken thread isn't guaranteed the condition still holds — *Mesa semantics* — plus *spurious wakeups*).
- Explain the **lost-wakeup** bug (signal fires before the waiter waits → the wakeup vanishes) and how the lock + while-loop discipline prevents it.
- Choose **`signal` vs `broadcast`** and articulate the correctness-vs-efficiency tradeoff (the *thundering herd*).
- Design the canonical **bounded producer–consumer queue** in pseudocode (two conditions: *not-full*, *not-empty*), and explain **backpressure** — why bounding is a feature, not a limitation.
- Apply all of it to the **async logger draining a message queue** (a reported Databricks question): batching, graceful shutdown, and why the bound protects you.

We stay in **pseudocode**; the C# realization (`Monitor.Wait`/`Pulse`/`PulseAll`, and the batteries-included `BlockingCollection<T>`) is a short language note at the end.

---

## Part 0 — The wrong way: busy-waiting

Say a consumer wants the next item from a shared queue. The queue might be empty right now, so it has to wait until a producer adds something. The tempting first attempt:

```
   // consumer — WRONG
   loop:
       lock(m)
       while queue.isEmpty():        // "wait" by spinning
           unlock(m); lock(m)        // release so a producer can add, immediately re-check
       item = queue.dequeue()
       unlock(m)
       process(item)
```

This *works* in the sense of being correct, and it is a disaster in the sense of being unusable:

- **It burns a full CPU core doing nothing.** While the queue is empty, this thread loops as fast as the hardware allows, re-checking a condition that only *another* thread can change. That is 100% of a core spent on the question "is it my turn yet?"
- **On a single core it's the L03 spin disaster again.** The consumer monopolizes the CPU re-checking `isEmpty()`; the **producer that would make it non-empty can't get scheduled** to run. The spinner actively prevents the very progress it's waiting for.
- **It scales terribly.** Ten idle consumers = ten pegged cores, heat, and no work done.

What we actually want is: **"put me to sleep — deschedule me entirely, zero CPU — and wake me only when a producer has added something."** That is exactly what a **condition variable** provides. Busy-waiting asks *"is it true yet? is it true yet? is it true yet?"*; a condition variable says *"wake me when it's true."*

> **Frame for the room:** *"A lock answers 'is it safe to touch this?' A condition variable answers 'is it **time** to touch this?' Without the second, threads either corrupt data (no lock) or burn cores spinning (no condition variable). Real concurrent structures need both."*

---

## Part 1 — The condition variable and its three operations

A **condition variable** (CV) is a synchronization primitive that lets a thread **block until notified**. It is always **associated with a lock** and with some **logical condition** over shared state (e.g. "queue not empty"). It exposes three operations:

| Op | Pseudocode | Meaning |
|---|---|---|
| **wait** | `wait(cv, m)` | *Atomically* release lock `m` and put this thread to sleep on `cv`. When later woken, re-acquire `m` before returning. |
| **signal** | `signal(cv)` | Wake **one** thread waiting on `cv` (if any). |
| **broadcast** | `broadcast(cv)` | Wake **all** threads waiting on `cv` (if any). |

(C#'s names: `Monitor.Wait` = wait, `Monitor.Pulse` = signal, `Monitor.PulseAll` = broadcast `[S1][S2]`. POSIX: `pthread_cond_wait` / `signal` / `broadcast`. Java: `Object.wait` / `notify` / `notifyAll`. Same three verbs everywhere.)

The **one property that makes it all work** is inside `wait`: it **atomically** releases the lock *and* goes to sleep, as a single indivisible step. Why atomic? Consider what would happen if it were two separate steps — `unlock(m)` then `sleep()`:

```
   Consumer                              Producer
   ─────────                             ────────
   sees queue empty
   unlock(m)                             lock(m)
             ← window! →                 queue.enqueue(x)
                                         signal(cv)      // wakes... nobody; consumer isn't asleep yet
   sleep()   // sleeps FOREVER — the signal already fired
```

That gap is the **lost wakeup**: the signal fired in the instant between the consumer unlocking and actually sleeping, so it woke no one, and the consumer sleeps forever. Making `wait` **atomically release-and-sleep** closes the gap — the producer cannot acquire the lock to signal until the consumer is *already* parked on the CV. This atomicity is the whole reason `wait` takes the lock as a parameter and must be called while holding it.

```
   wait(cv, m)  ≡   ┌─ atomically ─┐
                    │ release m    │
                    │ sleep on cv  │
                    └──────────────┘
                    … (woken) …
                    re-acquire m before returning
```

Microsoft states the C# behavior precisely: `Monitor.Wait` *"releases the lock on an object and blocks the current thread until it reacquires the lock"*, and it *"is called when the caller needs to wait for a state change that will occur as a result of another thread's operations"* `[S1]`. Note the symmetry with L04's unifier: because `wait` releases and later re-acquires the lock, all the **happens-before / visibility** guarantees of the lock (L04, Part 6) still hold — when `wait` returns, you see the writes the signaling thread made.

---

## Part 2 — The canonical wait/signal shape (and why the lock is mandatory)

Every correct use of a condition variable follows the same skeleton. A **waiter** grabs the lock, checks the condition in a **loop**, and waits while it's false; a **signaler** grabs the lock, changes the state, and signals:

```
   // WAITER
   lock(m)
   while not CONDITION:          // ← while, not if (Part 3)
       wait(cv, m)               // releases m, sleeps; re-acquires m on wake, then re-checks
   // CONDITION is now true AND we hold m
   ... use / mutate shared state ...
   unlock(m)

   // SIGNALER
   lock(m)
   ... change shared state so CONDITION may now be true ...
   signal(cv)                    // or broadcast(cv)
   unlock(m)
```

Two things are non-negotiable here:

1. **The condition is checked while holding the lock.** The condition is a predicate over *shared* state (`queue.count > 0`). Reading it without the lock is a race (L02) and, worse, you could read it as false, and *then* — before you call `wait` — a producer makes it true and signals into the void (lost wakeup, Part 1). Holding the lock across "check → wait" is what makes the check-and-sleep safe.
2. **The signaler changes state, then signals, all under the same lock.** A CV carries **no memory of its own**: signaling when nobody is waiting is simply *lost*. Microsoft is explicit for `Monitor`: *"The Monitor class does not maintain state indicating that the Pulse method has been called. Thus, if you call Pulse when no threads are waiting, the next thread that calls Wait blocks as if Pulse had never been called"* `[S2]`. So the truth must live in the **shared state** (the queue's count), not in the signal. The signal is only a *nudge* saying "go re-check the state"; the state is the source of truth. This is the deepest idea in the lesson.

> **Say it cleanly:** *"The condition variable doesn't remember anything. The **shared state** is the truth; the signal is just a tap on the shoulder that says 're-check the truth.' That's why you check state under the lock, and why a signal to an empty wait-queue is harmlessly dropped — and also why you must never rely on the signal alone."*

---

## Part 3 — The golden rule: `while`, never `if`

The most important line of discipline in this entire lesson:

> **Always re-check the condition in a `while` loop after `wait` returns — never a bare `if`.**

A returning `wait` does **not** guarantee the condition you wanted is true. There are three independent reasons, and you should be able to name all three:

1. **Mesa semantics (the big one).** In the model used by essentially all real systems (C# `Monitor`, Java, POSIX, called *Mesa*-style after the Mesa language), `signal` does **not** hand the lock directly to the woken thread. It merely moves the waiter to the **ready queue**; the woken thread must still **re-acquire the lock**, and *some other thread can grab the lock and change the state first*. Microsoft says exactly this: after `Pulse`, *"when the thread that invoked Pulse releases the lock, the next thread in the ready queue (**which is not necessarily the thread that was pulsed**) acquires the lock"* `[S2]`. So between "you were signaled" and "you actually run," a third thread may have consumed the item you were woken for. If you used `if`, you'd proceed on a false condition (dequeue from an empty queue → crash/corruption). With `while`, you simply re-check, see it's false again, and go back to sleep. Harmless.
2. **Spurious wakeups.** On some platforms a `wait` can return **without any signal at all** — a wakeup from nowhere, permitted by the specs (POSIX/Java explicitly allow it) as a concession to efficient implementation. The `while` loop makes this a non-event: re-check, still false, sleep again.
3. **Broadcast wakes many for one item.** If a signaler uses `broadcast` (Part 4) but only *one* unit of work appeared, **all** woken threads race for the lock; the first wins and consumes it, and the rest must find the condition false again. Only a `while` loop lets the losers go back to sleep correctly.

All three collapse into one rule: **`wait` returning means "maybe — re-check," not "yes."** Treat every wakeup as a *hint to re-evaluate the shared state*, and the `while` loop makes your code correct under all three.

```
   while not CONDITION:   ✔ re-checks after every wake → immune to Mesa steal, spurious, broadcast
       wait(cv, m)

   if not CONDITION:      ✘ checks once → proceeds on a stale/false condition → corruption
       wait(cv, m)
```

> **Interview-grade sentence:** *"I always wait in a `while`, never an `if`, because a wakeup is only a hint to re-check — Mesa semantics means another thread can win the lock and invalidate the condition before I run, and spurious wakeups and broadcasts can wake me when it isn't my turn. The shared state, re-checked under the lock, is the only truth."*

---

## Part 4 — `signal` vs `broadcast`: correctness vs the thundering herd

When you change the state, do you wake **one** waiter (`signal`) or **all** of them (`broadcast`)? This is a real design decision with a correctness floor and an efficiency ceiling.

- **`broadcast` (wake all) is always *safe*.** If you're unsure, broadcast: every waiter re-checks its `while` condition, the ones whose condition is now true proceed, the rest go back to sleep. You can never *miss* a wakeup by broadcasting.
- **But `broadcast` can be *wasteful* — the "thundering herd."** Suppose 100 consumers are blocked on an empty queue and a producer adds **one** item, then broadcasts. All 100 wake, all 100 stampede for the lock, **one** gets the item, and **99** re-check, find the queue empty, and go back to sleep — having done nothing but burn context switches and cache traffic. That's the thundering herd: many threads woken for work only one can do.
- **`signal` (wake one) is cheaper but needs an argument that it's correct.** Waking one is right *when any single waiter can handle the event and one item ⇒ one useful wakeup* — e.g. a producer added exactly one item, so waking exactly one consumer is enough.

The decision rule:

| Use | When |
|---|---|
| **`signal`** (wake one) | One unit of resource appeared and **any one** waiter can consume it, and all waiters are **equivalent** (waiting on the same condition). Cheapest. |
| **`broadcast`** (wake all) | Waiters are waiting on **different** conditions on the same CV (you don't know *which* to wake), **or** a single state change may satisfy **several** waiters (e.g. you set a "shutdown" flag and every worker must observe it), **or** you're simply not sure — correctness first. |

A classic trap that forces `broadcast`: **multiple distinct conditions sharing one CV.** If producers and consumers both wait on the *same* condition variable (one CV for "queue changed"), then a consumer finishing (making the queue *not full*) must wake a **producer**, not another consumer — but with one CV you can't target. `signal` might wake the wrong kind of thread, which re-checks, sees its condition still false, and sleeps again — and now the thread that *could* have proceeded was never woken. **Deadlock-by-lost-wakeup.** Two clean fixes: (a) use **two separate condition variables** (one *not-full* for producers, one *not-empty* for consumers) so `signal` always targets the right group — the approach in Part 5; or (b) keep one CV but always `broadcast`. Two CVs + `signal` is the more scalable design; one CV + `broadcast` is simpler but herds.

> **Tradeoff one-liner:** *"`broadcast` is the safe default but risks a thundering herd; `signal` is efficient but only correct when any one waiter can handle the event and everyone waits on the same condition. When conditions differ — like producers vs consumers — I split into two condition variables so a single `signal` always wakes the right group."*

---

## Part 5 — The bounded producer–consumer queue (the canonical design)

Now assemble everything into the structure that underlies thread pools, pipelines, and loggers: a **bounded blocking queue**. Producers `put` items; consumers `take` items. It's **bounded** (max `capacity`) so producers can't outrun consumers without limit. Two waiting conditions, so **two condition variables**:

- **`notFull`** — producers wait on this when the queue is full.
- **`notEmpty`** — consumers wait on this when the queue is empty.

```
   class BoundedBlockingQueue(capacity):
       lock  m
       condition notFull        // producers wait here
       condition notEmpty       // consumers wait here
       queue items = []         // the shared state = the TRUTH

       put(x):                          // producer
           lock(m)
           while items.count == capacity:      // full? wait for room  (while, not if)
               wait(notFull, m)
           items.enqueue(x)                    // state change
           signal(notEmpty)                    // a consumer may now proceed — wake ONE
           unlock(m)

       take():                          // consumer
           lock(m)
           while items.count == 0:             // empty? wait for an item (while, not if)
               wait(notEmpty, m)
           x = items.dequeue()                 // state change
           signal(notFull)                     // a producer may now proceed — wake ONE
           unlock(m)
           return x
```

Walk the four cases and see every rule from Parts 1–4 doing a job:

- **Consumer arrives, queue empty** → `while count == 0` true → `wait(notEmpty)` atomically releases `m` and sleeps (Part 1). No spin, zero CPU.
- **Producer `put`s an item** → enqueues, `signal(notEmpty)` → wakes one consumer, which re-acquires `m`, re-checks `while` (Part 3 — maybe another consumer already took it), sees `count > 0`, dequeues.
- **Producer arrives, queue full** → `wait(notFull)` sleeps until a consumer `take`s and `signal(notFull)`s — this is **backpressure** (below).
- **Why `signal` and not `broadcast`?** Because we split into **two** CVs (Part 4): a `put` only ever needs to wake **one** consumer (it added one item), and `signal(notEmpty)` targets exactly the consumers. One item → one wakeup → no herd.

**Backpressure — bounding is a feature.** Because `put` blocks when the queue is full, a **fast producer is automatically throttled to the speed of the consumers.** If you left the queue *unbounded*, a producer that outruns consumers would grow the queue without limit until the process runs out of memory and dies (**OOM**) — a classic outage. The bound converts "unbounded memory growth / crash" into "producer politely waits." Microsoft describes exactly this for `BlockingCollection<T>`: bounding *"prevents the producing threads from moving too far ahead of the consuming threads … if the collection reaches its specified maximum capacity, the producing threads will block until an item is removed"* `[S3][S4]`. Choosing the bound is a real tradeoff: **too small** → producers block often, underutilizing them; **too large** → more memory buffered and more latency before backpressure kicks in. It's a buffer between burst-absorption and memory cost.

**OO design quality (graded).** Notice the **separation of concerns**: the queue owns *(a)* the storage (`items`), *(b)* the synchronization (`m`, `notFull`, `notEmpty`), and *(c)* the blocking policy (wait/signal) — and exposes a tiny, honest interface (`put`/`take`). Producers and consumers know **nothing** about locks or condition variables; they just call `put`/`take`. That's **strong cohesion** (all the concurrency lives in one place) and **weak coupling** (callers are insulated from it) — the SRP the round grades. If tomorrow you swap the internal policy (drop-oldest instead of block-when-full), no caller changes.

### 5a. Closing the queue — graceful-shutdown support (a miniature of the whole lesson)

Part 6's logger shuts down by **closing** the queue (reject new `put`s; let `take` end once drained). Adding a `close()` is *small in code* but a perfect stress-test of Parts 3–4 — it trips all their rules:

```
   // additions to BoundedBlockingQueue
   bool closed = false                    // NEW — guarded by m

   close():
       lock(m)
       closed = true
       broadcast(notEmpty)                // wake ALL consumers → observe closed+empty, exit
       broadcast(notFull)                 // wake ALL producers  → observe closed, reject
       unlock(m)

   put(x):
       lock(m)
       while items.count == capacity and not closed:   // ← also wait on "not closed"
           wait(notFull, m)
       if closed:                         // ← re-check AFTER the wait
           unlock(m); throw ClosedError
       items.enqueue(x)
       signal(notEmpty)
       unlock(m)

   takeUpTo(K):
       lock(m)
       while items.count == 0 and not closed:          // ← also wait on "not closed"
           wait(notEmpty, m)
       if items.count == 0 and closed:                 // drained AND closed → done
           unlock(m); return CLOSED
       batch = dequeue up to K items                   // still-buffered items are served even after close
       signal(notFull)
       unlock(m)
       return batch
```

Three traps, each a direct application of this lesson:

1. **`close()` must `broadcast`, not `signal` (Part 4).** Closing is a state change that affects **every** waiter — all blocked consumers must wake to see "closed & empty → exit," and all blocked producers must wake to reject. A single `signal` would wake one and strand the rest forever. This is the textbook "one state change satisfies many waiters" case that *forces* broadcast.
2. **Re-check `closed` *after* the wait (Part 3, `while`-not-`if`).** A producer blocked on a full queue can be woken by `close()`, not by space freeing up — so the reason it woke may be "closed," not "there's room." Hence `not closed` goes into the `while` predicate *and* you re-check `closed` after the loop and reject. A wakeup is only a hint; now there are **two** things to re-check (space *and* closed).
3. **Drain-then-close (don't lose buffered data).** `take` waits only while *empty and still open* (`count == 0 and not closed`); it returns `CLOSED` only when **empty *and* closed**, and keeps serving any **still-buffered** items even after close. Graceful shutdown must flush what's already queued, not drop it.

(Freebie: `close()` is **idempotent** — setting `closed = true` twice is harmless — so double-close needs no guard.)

> **Why this matters:** this is `BlockingCollection<T>.CompleteAdding()` in miniature. It's *why* the library ships it — so you don't have to get broadcast-vs-signal, the post-wait re-check, and drain-then-close all correct by hand every time.

---

## Part 6 — Application: the async logger draining a message queue

This is a reported Databricks systems-programming question `[S7]`: *"an efficient logger that processes messages in a queue."* It **is** the bounded producer–consumer, specialized. The point of an async logger is to **decouple** the hot application threads from slow log I/O: application threads should `log(msg)` in nanoseconds and get back to work; a single dedicated **background writer** thread drains the queue and does the slow disk/network writes.

```
   class AsyncLogger:
       BoundedBlockingQueue queue(capacity = N)   // put() blocks if full, take() blocks if empty (Part 5's CVs)
       Thread writer

       start():
           writer = spawn(drainLoop)

       log(msg):                             // called by MANY app threads (producers)
           queue.put(msg)                    // fast: enqueue (+ wake writer); blocks only if full (backpressure);
                                             //   throws/rejects if the queue is CLOSED (see shutdown)

       drainLoop():                          // the single consumer
           while true:
               batch = queue.takeUpTo(K)     // BLOCKS on notEmpty (no busy-wait); returns items,
                                             //   or the CLOSED-AND-EMPTY signal
               if batch == CLOSED: break     // queue closed and fully drained → exit
               writeToDisk(batch)            // ONE syscall for K messages — amortize I/O

       shutdown():
           queue.close()                     // = CompleteAdding: reject new put(), and let take() end once drained
           join(writer)                      // wait for the writer to flush everything, then exit
```

The design decisions worth narrating — each ties back to a graded axis (**reliable / fast / scalable**):

- **One writer, many loggers (fast + scalable).** App threads are *producers*; a **single** consumer serializes all disk writes. Serializing is a *feature* here: one writer means log lines don't interleave/corrupt and the file is touched by exactly one thread (no lock needed around the file itself). The expensive I/O is off the hot path.
- **Batching (fast).** `takeUpTo(K)` drains several messages and writes them in **one** I/O operation. Disk/network I/O has high *per-call* overhead (a syscall, maybe an `fsync`); amortizing it across K messages is often a 10–100× throughput win versus one write per message. Batching is the single biggest lever for logger throughput.
- **Bounded queue = backpressure (reliable).** Under a log storm (a bug spraying millions of lines), an **unbounded** queue grows until **OOM kills the process** — the logger takes down the app. **Bounded**, `put` blocks the offending app thread until the writer catches up: the app slows down instead of dying. (You can also offer a *non-blocking* variant — `tryPut` that **drops** the message when full and increments a "dropped" counter — trading completeness for never blocking app threads. Blocking vs dropping is a deliberate reliability policy choice; be ready to defend either.)
- **Graceful shutdown (reliable).** You must not lose buffered logs on exit. Shutdown has **two** jobs: (1) **stop producers** — new `log()` calls must be rejected; and (2) **let the blocked consumer exit** — but only *after* it drains what's already queued. The clean way to do **both at once** is to **close the queue** (C#'s `BlockingCollection<T>.CompleteAdding()` `[S3][S4]`): after close, `put` throws/rejects, and a consumer blocked in `takeUpTo` is released and told “closed and empty” so the loop breaks. Then `join(writer)` blocks the shutdown caller until the flush completes. A **poison-pill / sentinel** (enqueue a special “no more work” marker) is the *hand-rolled* equivalent when your queue has no `close()` — it unblocks a writer parked on `notEmpty` and signals exit. **Note there is NO separate `running` flag:** because `takeUpTo` **blocks** (using the CV, Part 5), the writer parks inside it rather than spinning, so a `while running` check would be redundant — the close/sentinel is what actually exits the loop. A `running` bool is only load-bearing in a *non-blocking poll* loop, which would reintroduce the Part 0 busy-wait; the whole point of the blocking queue is to avoid that.
- **Durability is a *separate* axis (→ L10).** "The writer batched it" only means it's in the OS's hands; if **power goes down** before the OS flushes its page cache to the physical disk, those log lines are gone. Guaranteeing they survive a crash needs `fsync`/flush and write-ahead logging — **Lesson 10**. Flag it now, don't solve it here: *"in-memory queue → batched write buys throughput; surviving a power loss is a durability problem I'd solve with fsync/WAL, separately."*

> **How to pitch it:** *"An async logger is a bounded producer–consumer: app threads produce log lines into a bounded blocking queue; one background consumer drains and batches them into few I/O calls. Bounding gives backpressure so a log storm slows the app instead of OOM-killing it; batching amortizes syscall cost; a poison pill + join drains cleanly on shutdown. Actual crash-durability (fsync) is a separate layer."*

---

## Part 7 — Language note (brief): C# `Monitor` and `BlockingCollection<T>`

Per this track's reframe, you write **pseudocode** in the room — but know the C# realization so the words are grounded:

- **Raw condition variable = `Monitor`.** `lock(obj){}` is `Monitor.Enter/Exit`. Inside the lock: `Monitor.Wait(obj)` = `wait` (*"releases the lock … and blocks the current thread until it reacquires the lock"* `[S1]`), `Monitor.Pulse(obj)` = `signal` (wake one), `Monitor.PulseAll(obj)` = `broadcast` (wake all) `[S2]`. All three **must** be called while holding the lock, and `Pulse` has **no memory** — signaling with no waiter is lost `[S2]` — which is why you re-check shared state in a `while`. Unlike pseudocode's separate CVs, `Monitor` gives each object **one** implicit condition queue, so with multiple conditions you often `PulseAll` (or move to explicit primitives).
- **Batteries-included = `BlockingCollection<T>`.** In real C# you rarely hand-roll the queue: `BlockingCollection<T>` *"provides blocking and bounding capabilities … an implementation of the producer/consumer pattern"* `[S3]`. `Add` blocks when full, `Take` blocks when empty, the constructor takes a **bounded capacity**, and `CompleteAdding()` + `IsCompleted` handle graceful shutdown `[S3][S4]`. Default backing store is a FIFO `ConcurrentQueue<T>` `[S3]`. For `async/await` producer–consumer, MS steers you to **`Channel<T>`** instead (`BlockingCollection<T>` *"was not designed with asynchronous access in mind"* `[S3]`) — that's Lesson 08's world.
- **Interview stance:** be able to build it from `lock` + `Wait`/`Pulse` (shows you understand the mechanism), *then* say "in production I'd reach for `BlockingCollection<T>` / `Channel<T>`" (shows judgment). Building it by hand is the graded skill; knowing the library is the seniority signal.

*(No watch-it-happen demo ships for this lesson — per the track's scoping, the C# harness is reserved for the ~4 lessons where seeing non-determinism matters, L02/L03/L04/L09. The producer–consumer is deterministic-by-design once correct; if you'd like a runnable `BlockingCollection<T>` logger to poke at, say so and I'll add one.)*

---

## Self-check (say the answers out loud, as if teaching)

1. Why is **busy-waiting** on a condition (`while (empty) {}`) wrong? Give the CPU cost *and* the L03 single-core failure. What do we want instead, in one word?
2. Name the **three** condition-variable operations and what each does. What is the **one atomic property** inside `wait`, and what bug appears if release-and-sleep is *not* atomic?
3. Write the **canonical waiter/signaler skeleton**. Why must the condition be checked **while holding the lock**? Why does the CV itself "remember nothing," and what is therefore the real source of truth?
4. State the **golden rule** (`while`, not `if`) and give **all three** reasons a returning `wait` may find the condition false (name *Mesa semantics* explicitly).
5. **`signal` vs `broadcast`:** define each, give the **thundering herd**, and state when you'd pick each. Why do producers-and-consumers-on-one-CV force `broadcast`, and what's the better fix?
6. Design the **bounded producer–consumer queue** (two CVs). Which CV does `put` signal, which does `take` signal, and why `signal` not `broadcast` here? Explain **backpressure** and what an *unbounded* queue risks.
7. Pitch the **async logger** as a producer–consumer: why one writer + many loggers, why **batching**, why **bounded**, how **graceful shutdown** (poison pill / `CompleteAdding`) works, and which reliability concern is deferred to Lesson 10 (and how you'd flag it).

If any answer is shaky, re-read that Part before Lesson 06 — **Higher-level primitives** — where we go from raw locks + condition variables to the ready-made tools built on top of them: **semaphores** (a CV+counter that gates *N* permits — the bounded queue in one object), **read-write locks**, **latches/manual-reset events**, **countdown latches**, and **barriers**. Condition variables are the raw material; those are the power tools.

---

## Sources

Facts above are cited inline by tag. Pulled/verified **2026-07-21** from Microsoft Learn (authoritative for .NET `Monitor`/`BlockingCollection` semantics). Condition-variable theory (Mesa vs Hoare semantics, spurious wakeups, thundering herd) is standard concurrency material; the specific .NET API guarantees are cited to the docs, and the "async logger" framing traces to the reported Databricks question `[S7]` in the loop doc.

- `[S1]` Microsoft Learn — *Monitor.Wait Method (System.Threading)*. "Releases the lock on an object and blocks the current thread until it reacquires the lock." "When a thread calls Wait, it releases the lock on the object and enters the object's waiting queue. The next thread in the object's ready queue (if there is one) acquires the lock … All threads that call Wait remain in the waiting queue until they receive a signal from Pulse or PulseAll, sent by the owner of the lock." "This method blocks indefinitely if the holder of the lock does not call Pulse or PulseAll." "Called when the caller needs to wait for a state change that will occur as a result of another thread's operations." Pulse/PulseAll/Wait "must be invoked from within a synchronized block of code" (else `SynchronizationLockException`). https://learn.microsoft.com/en-us/dotnet/api/system.threading.monitor.wait (ms.date 2025-07-01; retrieved 2026-07-21).
- `[S2]` Microsoft Learn — *Monitor.Pulse(Object) Method*. "Notifies a thread in the waiting queue of a change in the locked object's state." "Only the current owner of the lock can signal a waiting object using Pulse." "Upon receiving the pulse, the waiting thread is moved to the ready queue. When the thread that invoked Pulse releases the lock, the next thread in the ready queue (**which is not necessarily the thread that was pulsed**) acquires the lock." **Lost-wakeup fact:** "The Monitor class does not maintain state indicating that the Pulse method has been called. Thus, if you call Pulse when no threads are waiting, the next thread that calls Wait blocks as if Pulse had never been called. If two threads are using Pulse and Wait to interact, this could result in a deadlock." "To signal multiple threads, use the PulseAll method." https://learn.microsoft.com/en-us/dotnet/api/system.threading.monitor.pulse (ms.date 2025-07-01; retrieved 2026-07-21).
- `[S3]` Microsoft Learn — *BlockingCollection\<T\> Class (System.Collections.Concurrent)*. "Provides blocking and bounding capabilities for thread-safe collections that implement IProducerConsumerCollection\<T\>." "An implementation of the producer/consumer pattern." "A bounded collection that blocks Add and Take operations when the collection is full or empty." "Bounding … prevents the producing threads from moving too far ahead of the consuming threads … if the collection reaches its specified maximum capacity, the producing threads will block until an item is removed … if the collection becomes empty, the consuming threads will block until a producer adds an item." "A producing thread can call the CompleteAdding method … Consumers monitor the IsCompleted property." Default backing store = `ConcurrentQueue<T>` (FIFO). "BlockingCollection\<T\> was not designed with asynchronous access in mind … consider using Channel\<T\> instead." https://learn.microsoft.com/en-us/dotnet/api/system.collections.concurrent.blockingcollection-1 (ms.date 2025-07-01; retrieved 2026-07-21).
- `[S4]` Microsoft Learn — *BlockingCollection Overview (.NET)*. Bounding + blocking support; the bounded-capacity-100 producer/consumer example (`Add` blocks when full, `Take` blocks when empty, `CompleteAdding()` signals completion, consumer loops while `!IsCompleted`); timed `TryAdd`/`TryTake`; cancellation via `CancellationToken`. https://learn.microsoft.com/en-us/dotnet/standard/collections/thread-safe/blockingcollection-overview (ms.date 2017-03-30; retrieved 2026-07-21).
- `[S7]` interviewing.io — reported Databricks systems-programming question: "an efficient logger that processes messages in a queue" (cited in `../../full_loop/databricks-full-loop.md`; general candidate-reported texture, not authoritative for this specific loop's structure). Motivates Part 6.

**Not independently re-benchmarked / flagged:** the "10–100× throughput" batching figure and "nanoseconds to `log()`" are order-of-magnitude illustrations of amortized-syscall economics, not measured on a specific system — the *direction* (batching amortizes high per-I/O-call overhead) is the sourced, robust claim. *Mesa vs Hoare* monitor semantics and *spurious wakeups* are standard OS/concurrency theory; the concrete .NET consequence ("the pulsed thread is not necessarily the one that reacquires the lock" ⇒ re-check in a `while`) is grounded in `[S2]`. Whether a given runtime actually produces spurious wakeups is implementation-defined; the `while`-loop discipline is correct regardless.
