# 01 — Threads & the execution model

This lesson assumes you know **nothing** about how a program runs on real hardware, and builds up — from the CPU to the operating system to the .NET runtime — everything the rest of the concurrency track depends on. By the end you should be able to answer, cold:

- What is a process? What is a thread? How do they differ, concretely (memory, cost, isolation)?
- What is the difference between **concurrency** and **parallelism**, and why does it matter?
- What actually happens on the CPU when "two threads run at once"? What is a **context switch** and why isn't it free?
- In C#, what is the difference between a **`Thread`**, a **thread-pool** thread, and a **`Task`**, and when do you reach for each?
- **Why** do we use concurrency at all — what problem does it solve (latency vs throughput), and what does it cost us?

Everything later in this track — races, locks, the memory model, the thread-safe cache, the rate limiter — is a consequence of the model in this lesson. Take your time.

> **How this fits the loop.** The Databricks **Systems Programming** round is single-machine concurrency: "build a thread-safe cache," "build a rate limiter," "what happens when power goes down," "synchronize these threads" `[S6]`. You cannot reason about *thread-safe* anything until you can precisely say what a thread *is* and how the machine runs several of them. That's this lesson.

---

## Part 0 — The absolute basics (the machine underneath)

Before threads, we need a shared picture of the computer your program runs on. Skip nothing here.

**CPU / processor / core.** The **CPU** (central processing unit) is the chip that actually *executes instructions* — add these two numbers, load this value from memory, jump to that instruction. A modern CPU chip contains several **cores**, and **each core is an independent instruction-executing engine.** A core does exactly one thing at a time: it fetches an instruction, executes it, moves to the next. Billions of times per second, but still *one instruction stream at a time per core*. "A 4-core CPU" means four such engines that can genuinely run four instruction streams simultaneously.

> **Hyper-threading / SMT (simultaneous multithreading).** Many CPUs advertise "8 logical processors" on a 4-core chip. Each physical core exposes **two** hardware thread slots that share the core's execution units, so the core can interleave two instruction streams very cheaply (while one waits on memory, the other computes). For our purposes treat a "logical processor" as "a place the OS can run a thread," and remember it's not quite a full extra core. `[S1]`

**Registers.** A tiny set of ultra-fast storage slots *inside* the core (a few dozen, each 64 bits). The CPU can only compute on values that are in registers. Example: to do `a + b`, the core loads `a` into a register, loads `b` into another, adds them, and the result lands in a register. Registers are the fastest storage that exists — sub-nanosecond.

**RAM / main memory.** A large pool of storage (gigabytes) that holds your program's code and data while it runs. Much bigger than registers but much slower to reach (tens to hundreds of nanoseconds). The CPU constantly shuttles values between RAM and registers. This speed gap is the reason **caches** exist (small fast copies of recently-used RAM close to the core) — and, crucially for us, the reason each thread keeps a **stack** and why the **memory model** (Lesson 4) is subtle.

**Disk / SSD.** Permanent storage that survives power loss (RAM does not). Slower still (microseconds for an SSD, milliseconds for a spinning disk). This gap is the whole subject of the durability lesson ("what happens when power goes down"), Lesson 10.

Rough mental ladder of "how far away is my data" — memorize the *ordering and the orders of magnitude*, not exact figures:

```
register     < 1 ns          (inside the core)
L1 cache     ~ 1 ns
L2/L3 cache  ~ few–tens ns
main memory  ~ 100 ns
SSD          ~ 10–100 µs     (10,000–100,000 ns)
spinning HDD ~ 1–10 ms       (1,000,000+ ns)
network RTT  ~ 0.1–100 ms    (same datacenter → cross-continent)
```
> These are the widely-taught "latency numbers every programmer should know" order-of-magnitude figures; treat them as **approximate rules of thumb**, not measured constants (they vary by hardware and are not re-benchmarked here) `[S5]`. The one thing that's exact enough to bet on: **each rung is ~one to three orders of magnitude slower than the one above it.** That ladder is *why* a thread that's waiting on disk or network should get off the CPU and let another thread run — the seed of everything concurrency buys you.

---

## Part 1 — Program, process, thread

### 1a. Program vs process

A **program** is a file on disk: the compiled instructions (`myapp.dll`, `myapp.exe`). It's inert — just bytes.

A **process** is a **program that is running** — "an executing program," in the OS's own words `[S2]`. When you launch the program, the operating system (OS) creates a process: it loads the code into RAM, hands it a chunk of memory to work in, and starts executing its instructions. The OS uses processes to **separate the applications being executed** from each other `[S2]` — one process cannot casually read or corrupt another's memory. That isolation is a safety boundary (a crash in one process doesn't scribble on another) and it's enforced by hardware + OS.

A process owns:
- **Its code** (the instructions).
- **A virtual address space** — the process's private view of memory: its heap (dynamically allocated objects), its global data, its loaded libraries. "Virtual" because the OS maps these addresses onto real physical RAM behind the scenes, giving each process the illusion of its own clean memory.
- **OS resources**: open files, network sockets, handles.
- **At least one thread** (see below).

### 1b. What a thread is

Here is the precise definition, straight from the platform docs:

> A **thread is the basic unit to which an operating system allocates processor time.** Each thread has a scheduling priority and maintains a set of structures the system uses to save the thread context when the thread's execution is paused. The thread context includes all the information the thread needs to seamlessly resume execution, including the thread's set of **CPU registers and stack**. Multiple threads can run in the context of a process. **All threads of a process share its virtual address space.** A thread can execute any part of the program code, including parts currently being executed by another thread. `[S2]`

Unpack that sentence by sentence, because *the entire concurrency track lives inside it*:

1. **"the basic unit to which an OS allocates processor time"** — the OS scheduler hands out CPU time in units of *threads*, not processes. When we say "run this on another thread," we mean "give this work its own slot the scheduler can put on a core."

2. **"maintains … the thread context … CPU registers and stack"** — each thread has:
   - **Its own stack.** The stack is the region of memory that holds a thread's *call frames*: local variables, function parameters, and return addresses, growing/shrinking as it calls and returns from methods. Because each thread has its own stack, **local variables are private to a thread** — two threads running the same method each have their own copy of the locals. This is your first and most important safety fact: *locals are not shared; only heap objects and static/shared fields are.*
   - **Its own register values** (a snapshot: the program counter saying "which instruction am I on," the stack pointer, working registers). This snapshot is what gets saved and restored on a context switch (Part 3).

3. **"All threads of a process share its virtual address space"** — this is the double-edged sword that makes concurrency both powerful and dangerous. Threads in the same process **share the heap**: if thread A allocates an object and thread B has a reference to it, they see *the same object*. That's what lets threads cooperate cheaply (no copying, no message-passing needed to share data). It's *also* exactly why **data races** (Lesson 2) exist: two threads writing the same shared field with no coordination corrupt it. Shared memory is the gift and the curse.

4. **"can execute any part of the program code, including parts currently being executed by another thread"** — two threads can be *inside the same method at the same time*. There is no automatic mutual exclusion. If that method touches shared state, you have a problem to solve.

### 1c. Process vs thread — the contrast that matters

| | **Process** | **Thread** |
|---|---|---|
| Memory | Own private virtual address space | **Shares** the process's address space with sibling threads |
| Isolation | Strong (hardware-enforced); a crash/corruption is contained | **None** between threads — they can read/write each other's shared data |
| Private state | Everything | Only its **stack + registers** (locals, call chain) |
| Cost to create | Heavy (new address space, OS bookkeeping) | Light**er** (share the parent's address space) — but still not free (see 2c) |
| Communication | Must go *through* the OS: pipes, sockets, shared-memory segments (explicit) | **Just read/write shared objects** (implicit — and therefore hazardous) |

The one-line takeaway: **processes isolate; threads share.** We use threads (not processes) for concurrency *within* an application precisely because sharing memory is cheap and immediate — and the price of that convenience is that **we, the programmers, become responsible for coordinating access to shared data.** The rest of this track is that coordination.

- By default a .NET program starts with a **single thread**, the **primary** (or *main*) thread; it can then create additional **worker** threads to run code in parallel or concurrently with the primary one `[S2]`.

---

## Part 2 — Concurrency vs parallelism (get this exactly right)

These two words are used loosely everywhere, but the distinction is the crux of the whole subject.

**Concurrency** = *dealing with* many things at once — a **structure/composition** property. A program is concurrent if it has multiple independent logical threads of control ("tasks in progress") whose lifetimes overlap. They need **not** be running at literally the same instant; they may take turns on a single core. Concurrency is about *how the work is organized*: several things are "underway" and progress is interleaved.

**Parallelism** = *doing* many things at once — a **runtime/hardware** property. Two pieces of work are parallel if they execute **at the same physical instant**, which requires **multiple cores**. Parallelism is about *how the work is executed*.

The classic framing (Rob Pike): **concurrency is about *structure*, parallelism is about *execution*.** You can have:

- **Concurrency without parallelism** — one core, many threads taking turns (time-slicing). All the tasks are "in progress," but at any instant exactly one is on the CPU. This still helps enormously when tasks *wait* (on disk, network, a lock): while one is blocked, another runs.
- **Parallelism** — genuinely at-the-same-instant execution — requires ≥2 cores.
- Real programs usually have **both**: many concurrent threads, some subset of which run in parallel across the available cores at any given moment.

A picture — three tasks A, B, C:

```
ONE CORE, concurrent (interleaved, NOT parallel):
  core0:  A A B A C C B B A C ...      ← one at a time, scheduler switches between them

TWO CORES, concurrent AND parallel:
  core0:  A A A A A A ...
  core1:  B B C C B B ...              ← A runs at the SAME INSTANT as B/C
```

Why you must keep these separate:
- **Correctness bugs (races, deadlocks) come from *concurrency*, not parallelism.** Even on a single core, the scheduler can pause thread A halfway through `count++` and run thread B — corrupting shared state. You do **not** need multiple cores to get a race; you need *interleaving*. (This is why our demos reproduce races reliably even on one core.)
- **Speedups come from *parallelism*.** Splitting a CPU-bound computation across cores makes it finish faster only because the pieces truly run at once.

Hold this: **we add threads to get either responsiveness/throughput (concurrency, even on one core, by not blocking) or raw speed (parallelism, across cores) — but every thread we add creates shared-state hazards we must then manage.**

---

## Part 3 — What the CPU actually does: scheduling and context switches

If there are more runnable threads than cores (almost always true — a laptop has 8 logical cores and hundreds of threads across all processes), how do they all make progress?

### 3a. The scheduler and time-slicing

The OS component called the **scheduler** decides, moment to moment, *which thread runs on which core*. It gives each runnable thread a small **time slice** (a **quantum**, typically on the order of milliseconds), lets it run, then **preempts** it — pauses it mid-execution — and puts another runnable thread on the core. Cycle through fast enough (many switches per second) and every thread appears to make continuous progress, even though each is really running in short bursts. This is **preemptive multitasking**: the thread does not get to choose when it's paused; the OS can stop it *between any two machine instructions*.

Threads aren't always *runnable*. A thread is in one of a few states:
- **Running** — currently on a core.
- **Ready/Runnable** — wants the CPU, waiting its turn in the scheduler's queue.
- **Blocked/Waiting** — cannot proceed right now (sleeping, waiting for a lock, or waiting on I/O like a disk read or network response). A blocked thread uses **no** CPU; the scheduler skips it until whatever it's waiting for is ready. In .NET this is captured by the `WaitSleepJoin` thread state `[S3]`.

That "**blocked threads use no CPU**" fact is the entire economic argument for concurrency in I/O-heavy code: while thread A is blocked waiting 10 ms for a disk read, the scheduler runs threads B, C, D on that core instead of idling. Nothing is wasted.

### 3b. What a context switch is

When the scheduler moves a core from thread A to thread B, it performs a **context switch**:

1. **Save A's context** — copy A's live register values (program counter, stack pointer, working registers) out of the core into A's saved-context structure in memory. This is the "set of structures the system uses to save the thread context" from the definition in Part 1 `[S2]`.
2. **Load B's context** — restore B's previously-saved registers into the core, so B resumes exactly where it left off.
3. Resume executing at B's saved program counter.

Now B runs as if it had never stopped. Later the reverse happens and A picks up mid-instruction-stream, oblivious to the interruption. *This is why a thread can be paused between any two of its own instructions and neither thread notices — which is precisely what makes shared-state access unsafe.*

### 3c. Context switches are not free (why more threads ≠ more speed)

A context switch costs real time and has second-order costs:

- **Direct cost**: saving/restoring registers and running scheduler logic — on the order of **microseconds** (again, an order-of-magnitude rule of thumb, not a benchmarked constant here) `[S5]`.
- **Indirect cost — cache pollution**: thread A had warmed the core's L1/L2 caches with *its* data. After switching to B, those caches now fill with B's data. When A resumes, its data is gone from cache and must be re-fetched from slower memory. This "cold cache" penalty is often *larger* than the direct switch cost.
- **Oversubscription**: if you spawn far more CPU-bound threads than cores, they spend an increasing fraction of time switching *between* each other rather than doing work. Throughput can actually **drop**. More threads is not more speed — past the core count (for CPU-bound work), it's usually less. This is exactly why thread **pools** exist (Part 4, Lesson 8): reuse a bounded set of threads instead of over-spawning.

**Threads are not free.** Each also reserves memory for its **stack** (on Windows the default is commonly cited as ~1 MB reserved per thread; it's configurable via a `Thread` constructor overload that takes a max stack size, and platform-dependent — the docs expose the setting without pinning a universal number) `[S3]`. Spawn 10,000 raw threads and you've committed real memory and a scheduling burden before doing any work. This cost is the reason we prefer pooled threads and `Task`s for lots of small work items.

---

## Part 4 — Threads in C#/.NET: `Thread`, the thread pool, and `Task`

.NET gives you three levels of abstraction over the same underlying OS threads. Knowing which is which — and when to use each — is standard senior-level material and a likely interview probe.

### 4a. `System.Threading.Thread` — a dedicated OS thread

The lowest-level managed abstraction: **one `Thread` object ≈ one dedicated OS thread** that you create, start, and (optionally) wait for.

```csharp
using System.Threading;

var t = new Thread(() =>
{
    Console.WriteLine($"worker on managed thread {Environment.CurrentManagedThreadId}");
});
t.Start();   // hand it to the OS to schedule; it now runs concurrently with us
t.Join();    // BLOCK the calling thread until t finishes
```

Key facts, all from the platform docs:
- You construct a `Thread` with a **delegate** representing the method to run (`ThreadStart` for no-arg, `ParameterizedThreadStart` to pass one `object`), then call **`Start()`** to begin execution `[S3]`.
- **`Join()`** blocks the *calling* thread until the target thread terminates `[S3]` — this is how you wait for work to finish. (Our harness's `RunThreads` starts N threads and `Join`s them all.)
- **`ManagedThreadId`** is a runtime-assigned id that uniquely identifies a managed thread within its process `[S3]`. Note it is **not** the OS thread id — a host can even map many managed threads onto one OS thread or move a managed thread between OS threads `[S3]`. Use `ManagedThreadId` for logging/identity, not OS-level assumptions.
- **Priority** (`Thread.Priority`) is a *request*: you can ask for higher scheduling priority, but it "is not guaranteed to be honored by the operating system" `[S3]`. Never build correctness on priority.

**Foreground vs background threads — a real gotcha.** A **foreground** thread keeps the process alive: the runtime won't exit while any foreground thread is still running. A **background** thread does not — "a background thread does not keep a process running if all foreground threads have terminated," and once the last foreground thread ends, "the runtime stops all background threads and shuts down." `[S3]` The defaults:
- **Foreground by default**: the main application thread, and **every thread you create with a `Thread` constructor** `[S3]`.
- **Background by default**: **thread-pool threads**, and therefore **`Task`s** (which run on the pool) `[S3]`.

Flip it with `t.IsBackground = true`. Consequence to remember: a `Task` doing important work will **not** keep your program from exiting — if `Main` returns, background work is abandoned mid-flight. (This is why console demos `Join`/`await`/`Wait` before returning.)

**When to reach for a raw `Thread`** — the docs give the exact list: you need a **foreground** thread; you need a **specific priority**; you have a task that **blocks for a long time** (a long block would tie up a precious pool thread); you need a **single-threaded apartment**; or you need a **stable, dedicated identity** for the thread `[S4]`. The canonical case for us: a **long-lived dedicated background loop** — e.g. the **TTL-expiry sweeper thread** in the thread-safe cache (Lesson 9), or a logger's drain thread (Lesson 5). Those run for the whole app lifetime, so a dedicated `Thread` (marked background) fits; the pool does not.

### 4b. The thread pool — reuse instead of re-create

Creating and destroying an OS thread per work item is exactly the cost we saw in Part 3c. The **managed thread pool** (`System.Threading.ThreadPool`) solves this: the runtime keeps **a pool of worker threads**; you *queue work* onto it; when a pool thread finishes a work item it is **returned to the pool and reused** for the next, so you "avoid the cost of creating a new thread for each task" `[S1]`. Facts worth memorizing:
- **One thread pool per process** `[S1]`.
- Pool threads are **background** threads, run at **default priority**, and use the **default stack size** `[S1]`.
- The pool **limits how many threads are active at once** and grows/shrinks adaptively: it has a **minimum** it will create on demand without delay, and beyond that it adds threads gradually to "optimize throughput … the number of tasks that complete per unit of time," balancing "too few threads → underuse resources" against "too many → contention" `[S1]`.
- **Gotcha — don't block pool threads for long.** Because the pool has a bounded active-thread count, a pile of pool threads blocked on long operations "might prevent tasks from starting" `[S4]` — you can starve the pool. Long/blocking work → dedicated `Thread` (4a) or true async I/O (Lesson 8), not a blocked pool thread.

You rarely call `ThreadPool.QueueUserWorkItem` directly today; you use `Task`, which sits on top of it.

### 4c. `Task` and the TPL — the modern default

The **Task Parallel Library (TPL)** — types in `System.Threading.Tasks`, chiefly **`Task`** and **`Task<TResult>`** — is "the preferred way to write multithreaded and parallel code" in modern .NET `[S1]`,`[S7]`. A **`Task`** represents **a unit of asynchronous work** (a *future* result), decoupled from the thread that runs it. By default, TPL types **"use thread-pool threads to run tasks"** `[S1]` — so `Task.Run(...)` queues your work onto the pool.

```csharp
using System.Threading.Tasks;

Task<int> t = Task.Run(() =>
{
    // runs on a POOL thread (background)
    return ExpensiveComputation();
});

int result = await t;   // asynchronously wait for the result (doesn't block a thread while waiting)
```

Why `Task` is the default instead of `Thread`:
- **It's cheap for many small work items** — it reuses pool threads instead of spawning OS threads, so you can have thousands of tasks over a bounded thread set.
- **It composes** — tasks return values (`Task<T>`), chain (`ContinueWith`, `await`), combine (`Task.WhenAll`/`WhenAny`), and carry **cancellation** and **exception** propagation. A raw `Thread` gives you none of this; the TPL "handles the partitioning of work, the scheduling of threads on the `ThreadPool`, cancellation support, state management, and other low-level details" `[S7]`.
- **`async`/`await`** (Lesson 8) builds on `Task` to express "start this, and while it's waiting, free the thread to do other work" — the ideal shape for I/O-bound concurrency, where the win is *not blocking*, not *more cores*.

The MS guidance is explicit and worth repeating for the interview: even though the TPL simplifies things, you should still "have a basic understanding of threading concepts — locks, deadlocks, and race conditions — so that you can use the TPL effectively" `[S7]`. `Task` doesn't make shared state safe; it just manages the *threads*. **Coordinating shared data is still on you** — which is the rest of this course.

### 4d. Choosing between them (decision rule)

```
Need concurrency in C#. Which abstraction?

├─ Lots of short, independent work items, want results/composition/cancellation?
│     → Task / TPL   (rides the thread pool; the default)
│
├─ I/O-bound (disk, network, DB) and want to NOT waste a thread while waiting?
│     → async/await over Task   (Lesson 8) — frees the thread during the wait
│
├─ Long-lived, dedicated background loop (TTL sweeper, logger drain, poller),
│  OR need foreground / specific priority / stable identity?
│     → a raw Thread (mark IsBackground as appropriate)   [S4]
│
└─ Direct low-level queueing with no result needed?
      → ThreadPool.QueueUserWorkItem   (rarely, Task is nicer)
```

For the **Systems Programming round**, you'll mostly *reason in raw threads and primitives* (the interviewer wants to see you handle synchronization by hand), and reach for a dedicated `Thread` for the long-lived helpers (sweeper/drain). We start the whole track on raw `Thread` (this lesson's demo, and Lesson 2's races) precisely because it makes the interleaving visible and puts the coordination burden where the interview wants it — on you.

---

## Part 5 — Why concurrency: the payoff and the price

Pulling Parts 2–4 together into the *why*. The platform docs name the two motivations directly: multithreading is used **"to increase the responsiveness of your application and to take advantage of a multiprocessor or multi-core system to increase the application's throughput."** `[S2]` Those are the two — and only two — reasons, and they map onto the concurrency/parallelism split from Part 2:

**Reason 1 — Responsiveness & throughput via *not blocking* (concurrency; helps even on one core).**
When work spends time **waiting** (disk, network, database, a lock, a queue), a single thread that does everything sequentially sits idle during each wait. Give the waiting work its own thread and the rest of the program keeps running:
- **Responsiveness**: a UI or server main loop stays free to answer new requests instead of freezing while a slow operation runs on a worker thread `[S2]`.
- **Throughput**: while thread A blocks on I/O, threads B/C/D do useful work on the same core (Part 3a: blocked threads use no CPU). You get *more requests served per second* out of the same hardware — pure win from **overlapping waits**, no extra cores needed. This is the dominant reason in server/systems code (the loop's territory).

**Reason 2 — Raw speed via *parallelism* (needs multiple cores).**
For **CPU-bound** work (a big computation, no waiting), the only way to finish faster is to split it across cores and run the pieces **at the same instant** `[S2]`. Four cores can, ideally, do four times the arithmetic per second. This is bounded by how much of the work is actually parallelizable (Amdahl's law, informally: the serial part caps your speedup) and by the core count.

**The price — what every added thread costs.** None of this is free, and a senior answer names the costs:
1. **Correctness burden**: shared mutable state across threads → **races** (Lesson 2), and coordinating it introduces **deadlock/livelock** risk (Lesson 3). Concurrency bugs are **non-deterministic** — they appear only under specific interleavings, so they hide in testing and surface in production. This is the biggest cost by far.
2. **Overhead**: thread creation, per-thread stack memory, and **context-switch + cache** costs (Part 3c). Oversubscription can make things *slower*.
3. **Complexity**: reasoning about all possible interleavings is genuinely hard; the code is harder to test, debug, and get right.

The senior framing to carry into the interview: **"Add concurrency when the work *waits* (overlap the waits for throughput/responsiveness) or when it's CPU-bound and *parallelizable* (split across cores for speed) — and only as much as the core count and the coordination cost justify. Every thread I add, I now owe a story for how its shared state stays correct."** That last clause is the entire reason this course exists.

---

## Part 6 — The "ship": spin up threads and watch them interleave

Concepts land when you *see* them. The runnable demo for this lesson lives in the harness at [../code/Lessons/Lesson01Threads.cs](../code/Lessons/Lesson01Threads.cs). Run it:

```bash
cd systems_programming/code
dotnet run -- 01-threads
```

It does three things, each illustrating one idea from above:

1. **Real OS threads exist and have identities.** It starts several `Thread`s; each prints its `ManagedThreadId`. You'll see *different* ids — proof these are distinct threads, not one thread looping (Part 4a).
2. **Interleaving is real and non-deterministic (concurrency).** Each worker prints a sequence of tagged lines (`W0-step0`, `W1-step0`, …). The output lines come out **interleaved and in a different order almost every run** — the scheduler is switching between them between instructions (Parts 2–3). No two runs need match. *This* is concurrency you can see.
3. **`Join` is how you wait.** `Main` starts the workers, then `Join`s them all before printing "all workers done" — so that line is always last. Comment the joins out (mentally) and, because these are foreground threads, the process would wait anyway; make them background and `Main` could exit first, abandoning them (Part 4a). The demo notes this.

Predict the output *before* you run it: you should be able to say "the per-worker steps stay in order within a worker (each worker's own stack/program-counter is sequential), but across workers the lines are arbitrarily interleaved, and 'all workers done' prints last." Run it a few times and confirm the cross-worker order changes while the within-worker order never does. That single observation — **sequential within a thread, arbitrary across threads** — is the mental model the entire rest of the course builds on, and it's what makes the `count++` race in Lesson 2 inevitable.

---

## Self-check (say the answers out loud, as if teaching)

1. Define **process** and **thread**, then name three concrete things a process owns that threads share, and two things each thread keeps *private*. Why does "threads share the address space" make them both powerful and dangerous?
2. **Concurrency vs parallelism**: give a one-sentence definition of each, an example of concurrency *without* parallelism, and state which one is responsible for race-condition bugs. Why can you reproduce a race on a single-core machine?
3. Walk through a **context switch** step by step. Name its **direct** cost and its **indirect** (cache) cost, and explain why spawning many more CPU-bound threads than cores can *reduce* throughput.
4. Contrast **`Thread`**, a **thread-pool** thread, and **`Task`**: which rides the pool, which are background vs foreground by default, and give the canonical case where you'd deliberately use a raw `Thread` over a `Task`. (Hint: something long-lived in the cache lesson.)
5. What is the practical consequence of `Task`s being **background** threads for a console program that returns from `Main` without awaiting? How do you avoid losing the work?
6. State the **two and only two** reasons to add threads (per the docs), map each to concurrency-vs-parallelism, and then name the **three costs** you take on. Finish the senior one-liner: "Add concurrency when …, and only as much as …; every thread I add I now owe …"
7. Priority and `ManagedThreadId`: why should you never build correctness on thread **priority**, and why is `ManagedThreadId` *not* the OS thread id?

If any answer is shaky, re-read that Part before the next lesson — Lesson 2 (shared state & races) assumes all of this cold.

---

## Sources

Facts above are cited inline by tag. Pulled/verified **2026-07-14**. Microsoft Learn is the authoritative source for .NET threading semantics; the latency-ladder figures are widely-taught order-of-magnitude rules of thumb and are **flagged as approximate, not benchmarked here**.

- `[S1]` Microsoft Learn — *The managed thread pool (.NET)*. Thread-pool reuse, one pool per process, background/default-stack/default-priority pool threads, adaptive min/throughput sizing, TPL rides the pool. https://learn.microsoft.com/en-us/dotnet/standard/threading/the-managed-thread-pool (page ms.date 2026-03-13; retrieved 2026-07-14).
- `[S2]` Microsoft Learn — *Threads and threading (.NET)*. Process = executing program; thread = basic unit of processor-time allocation; thread context = registers + stack; threads share the process's virtual address space; primary vs worker threads; multithreading for responsiveness + throughput. https://learn.microsoft.com/en-us/dotnet/standard/threading/threads-and-threading (retrieved 2026-07-14).
- `[S3]` Microsoft Learn — *Thread Class (System.Threading)*. Start/Join semantics, ThreadStart/ParameterizedThreadStart, ManagedThreadId (and its non-relationship to the OS thread id), priority "not guaranteed to be honored," foreground vs background defaults, `WaitSleepJoin` state, max-stack-size constructor. https://learn.microsoft.com/en-us/dotnet/api/system.threading.thread (page ms.date 2025-07-01; retrieved 2026-07-14).
- `[S4]` Microsoft Learn — *The managed thread pool → "When not to use thread pool threads."* Use a dedicated thread when you need foreground / specific priority / long blocking / single-threaded apartment / stable identity; blocked pool threads can prevent tasks from starting. (Same page as `[S1]`; section cited separately.) Retrieved 2026-07-14.
- `[S5]` Order-of-magnitude latency & context-switch figures — the widely-taught "latency numbers every programmer should know" tradition (register < cache < RAM < SSD < HDD < network; context switch ~µs). Used here **only as approximate rules of thumb**; not independently benchmarked in this run. (Popularized via Jeff Dean / Peter Norvig; treat as pedagogy, not a measured constant.)
- `[S6]` Databricks loop structure — the Systems Programming round is single-machine concurrency (thread-safe cache, rate limiter, power-loss/durability, synchronization). Per this workspace's loop analysis `full_loop/databricks-full-loop.md`, which traces to recruiter email `[S1]`/user notes `[S2]` therein. (Internal workspace doc; round facts sourced there.)
- `[S7]` Microsoft Learn — *Task Parallel Library (TPL) (.NET)*. TPL is the preferred way to write multithreaded/parallel code; scales concurrency across processors; handles partitioning, scheduling on the ThreadPool, cancellation, state; still understand locks/deadlocks/races to use it well. https://learn.microsoft.com/en-us/dotnet/standard/parallel-programming/task-parallel-library-tpl (page ms.date 2025-10-22; retrieved 2026-07-14).

**Not independently verified this run (flagged):** the specific "~1 MB Windows default stack per thread" figure — the `Thread` docs `[S3]` confirm a configurable max-stack-size constructor exists and that the pool uses a "default stack size," but do **not** state the 1 MB number on the retrieved page; treat 1 MB as the commonly-cited Windows default, platform-dependent, not re-confirmed here. Likewise all `[S5]` latency magnitudes.
