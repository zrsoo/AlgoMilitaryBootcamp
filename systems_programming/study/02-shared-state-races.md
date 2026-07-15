# 02 — Shared state & race conditions

Lesson 01 ended on one sentence: **sequential within a thread, arbitrary across threads.** This lesson takes that single fact and shows the damage it does the moment two threads touch the *same* piece of memory. By the end you should be able to, cold:

- Say exactly what **shared mutable state** is, and why it — not "threads" in the abstract — is the source of every concurrency bug.
- Define a **race condition** precisely, and walk the canonical `count++` **lost update** step by step.
- Name the three words that make the diagnosis: **critical section**, **atomicity**, **interleaving**.
- Recognise the **four common shapes** a race takes (not just `count++`), so you can *spot* them in an interview problem.
- Sketch the **fix landscape** — mutual exclusion, atomic operations, don't-share — and the **tradeoffs** between them (the reliability-vs-throughput conversation the round wants).

This is the lesson the whole "make it **reliable**" axis of the interview rests on. We stay in **pseudocode** throughout — the language you'll write on the whiteboard — and only *watch* a real race happen at the end.

> **Reminder on scope (from the track's teaching reframe).** The round is language-agnostic; you'll write pseudocode. So we learn the *concepts and the primitives generically* and treat any concrete language only as a place to *see* the bug. The one concrete language fact we borrow here — which numeric reads/writes are atomic — is cited as an *illustration* of a universal hardware truth, not as something to memorise per-language.

---

## Part 0 — The one thing that carries over from Lesson 01

Recall the two facts we ended on:

1. **Each thread runs its own instructions in order** (it has its own program counter and stack).
2. **The scheduler can pause a thread between *any two* instructions** and run another thread — and you cannot predict when.

And the memory picture:

- **Locals live on each thread's private stack** → not shared → safe.
- **Heap objects and static/shared fields live in the one shared address space** → every thread with a reference sees the *same bytes* → this is where trouble lives.

Put those together and you get the subject of this lesson. If two threads only ever touch their own private data, nothing can go wrong. The instant they both touch **the same shared, mutable location**, the "arbitrary across threads" interleaving can tear the operation apart. That combination has a name.

---

## Part 1 — Shared mutable state: the actual villain

Three words, and you need **all three** present for a concurrency bug to be possible:

```
   SHARED          +      MUTABLE        +     CONCURRENT ACCESS
(≥2 threads can    (the value can        (the accesses can overlap
 reach the same     change — it's not     in time — at least one
 location)          read-only/constant)   of them is a WRITE)
```

Remove **any one** of the three and the bug is impossible — and this is your entire toolbox of fixes previewed in one diagram:

- Not **shared**? (each thread has its own copy → *confinement*) → safe.
- Not **mutable**? (the data never changes after creation → *immutability*) → safe.
- Not **concurrent**? (accesses are forced to take turns → *mutual exclusion / locks*) → safe.

So every fix in this whole course is just "attack one of those three legs." Hold that thought; we come back to it in Part 5.

> **Vocabulary.** *Mutable* = changeable (can be mutated/modified). *Immutable* = fixed once created. A `readonly` constant, or an object you never modify after building it, is immutable. *Confinement* = keeping data reachable by only one thread (e.g. a local variable, or a per-thread copy), so it's never shared.

**Why "at least one write"?** Two threads that both only *read* the same value can't corrupt it — reading doesn't change anything, so it doesn't matter who goes first or whether they overlap. You need at least one **writer** for the order to matter. *Two readers are always safe; a reader plus a writer, or two writers, are where races live.* (This exact asymmetry is why a **read-write lock** exists — Lesson 06 — it lets unlimited readers run together but forces writers to be alone.)

---

## Part 2 — What a race condition *is*

Here is the definition, from the platform's own threading guidance:

> "A **race condition** is a bug that occurs when the outcome of a program depends on **which of two or more threads reaches a particular block of code first.** Running the program many times produces different results, and the result of any given run cannot be predicted." `[S1]`

Unpack it:

- **"the outcome depends on who gets there first"** — correctness has become a function of *scheduling*, which is not under your control. The program is only *accidentally* correct — right on the interleavings you happened to hit, wrong on the ones you didn't.
- **"different results each run … cannot be predicted"** — this is the tell. A correct program gives the same answer every time for the same input. A racy one gives different answers depending on timing. That non-determinism is both the symptom and the reason races are so nasty to debug (Part 4).

### 2a. The canonical example: `count++` is a lie

Take the most innocent line of code imaginable — increment a shared counter:

```
shared integer count = 0        // lives on the heap / as a static field → SHARED

// run this on two threads at once, 1000 times each:
repeat 1000 times:
    count = count + 1           // looks atomic. it is NOT.
```

You expect `2000` at the end. You will frequently get *less*. To see why, you must know that `count = count + 1` is **not one step**. The platform docs spell out the three machine steps it compiles to `[S1]`:

```
   count = count + 1     actually means:

   (1) LOAD   read count's current value from memory into a CPU register
   (2) ADD    add 1 to the value in the register
   (3) STORE  write the register value back to count in memory
```

This is a **read-modify-write** sequence: read the old value, compute a new one, write it back. Three separate instructions — and, per Lesson 01, **the scheduler can preempt the thread between any two of them.**

### 2b. The lost update, step by step

Watch what happens when two threads, A and B, both run `count++` starting from `count = 5`, and the scheduler interleaves them at the worst moment. Time flows downward:

```
   time │ Thread A                 │ Thread B                 │ count in memory
   ─────┼──────────────────────────┼──────────────────────────┼────────────────
     t1 │ LOAD  count → regA (5)   │                          │       5
     t2 │ ADD   regA = 5 + 1 = 6   │                          │       5
     t3 │    ── preempted ──       │ LOAD  count → regB (5)   │       5   ← B reads the STALE 5
     t4 │                          │ ADD   regB = 5 + 1 = 6   │       5
     t5 │                          │ STORE regB → count (6)   │       6
     t6 │ STORE regA → count (6)   │    ── resumes ──         │       6   ← A clobbers with its own 6
   ─────┴──────────────────────────┴──────────────────────────┴────────────────
                                                    final count = 6, not 7
```

Two increments happened. The counter went up by **one**. One update was **lost** — silently overwritten — because B read `count` *after* A had read it but *before* A had written its result back. B's work was based on a **stale** value, and A's later write erased B's. That is a **lost update**, the most common race in existence `[S1]`.

The docs describe exactly this: *"a thread that has loaded and incremented the value might be preempted by another thread which performs all three steps; when the first thread resumes execution and stores its value, it overwrites [the counter] without taking into account the fact that the value has changed in the interim."* `[S1]`

> **Crucial callback to Lesson 01:** you do **not** need two cores for this. On a single core, the scheduler time-slicing between A and B produces exactly the interleaving above. Races come from **concurrency (interleaving)**, not parallelism. (This is why the demo at the end reproduces the bug on any machine.)

---

## Part 3 — The three words that name the problem

Now attach precise vocabulary to what we just saw. These are the words to *say out loud in the interview* — they signal you actually understand the mechanism.

### 3a. Critical section

A **critical section** is a region of code that accesses shared state and **must not be executed by more than one thread at a time** to stay correct. In the example, the three instructions of `count++` are a critical section: they must run as an *uninterrupted unit* with respect to any other thread touching `count`. The bug is precisely that *nothing was stopping two threads from being inside that section at once.*

> Spotting critical sections is the core skill. Ask of any line: *"does this read-then-write, or check-then-act on, shared state? Could another thread be in here at the same time?"* If yes, it's a critical section and it needs protecting.

### 3b. Atomicity

**Atomic** means **indivisible** — an operation is atomic if, from every other thread's point of view, it either **has not happened yet** or **is completely done**, with **no observable in-between state**. No thread can ever catch it "halfway."

- `count++` is **not** atomic: its halfway state (loaded-and-incremented-but-not-yet-stored) is exactly what B observed and exploited.
- A single, aligned read or write of a machine-word-sized value generally **is** atomic in hardware — nobody sees "half a value." This is a real, universal hardware property. As a concrete illustration, the C# language spec guarantees that reads and writes of `bool`, `char`, `byte`, `short`, `int`, `float`, and reference types (i.e. ≤ 32-bit and pointer-sized) are atomic — **but** reads and writes of `long`, `ulong`, `double`, and `decimal` (64-bit / wide) **need not be** atomic, and, in its own words, *"there is no guarantee of atomic read-modify-write, such as in the case of increment or decrement."* `[S2]`

Two separate lessons hide in that spec sentence:

1. **Even a plain assignment can tear for wide types** (`long`, `double`, structs). If one thread writes a 64-bit value in two 32-bit halves and another reads between the halves, the reader gets a **torn value** — half old, half new — a number that was *never actually assigned*. That's a **torn read/write** (Part 4).
2. **Read-modify-write is never atomic for free** — increment, decrement, "append to list," "if absent then put" — all are multi-step and all can be split. `count++` is just the smallest instance.

The fix, in one line: **make the critical section atomic** — force it to behave as an all-or-nothing unit. *How* you do that (a lock, a hardware atomic instruction, or not sharing at all) is Part 5 and the next three lessons.

### 3c. Interleaving (and the combinatorial explosion)

An **interleaving** is one particular order in which the scheduler weaves the instructions of several threads together. Two threads of three instructions each can be interleaved in *many* orders; most are harmless, a few corrupt the state. The scheduler picks one **non-deterministically** every run.

```
   Some interleavings of A=(a1 a2 a3) and B=(b1 b2 b3):

   a1 a2 a3 b1 b2 b3     ← A fully, then B fully  → CORRECT (+2)
   a1 b1 a2 b2 a3 b3     ← finely interleaved      → LOST UPDATE (+1)
   a1 a2 b1 b2 b3 a3     ← the Part-2b disaster     → LOST UPDATE (+1)
   ...                     dozens more, a mix of correct and buggy
```

The danger isn't that races happen *often* — it's that the buggy interleavings are a **small, timing-dependent minority**. Which brings us to why they're so uniquely awful.

---

## Part 4 — Why races are so dangerous: non-determinism & Heisenbugs

A race is not like a normal bug. A normal bug is deterministic: same input, same wrong output, every time — so you can reproduce it, step through it, and fix it. A race is **non-deterministic**: whether it fires depends on the exact scheduling, which depends on machine load, core count, timing, the phase of the moon.

Consequences you should be able to state:

- **It hides in testing.** Your tests run on a lightly loaded dev machine and hit the "correct" interleavings 999 times out of 1000. It ships. Then production — more load, more cores, more contention — hits the bad interleaving and corrupts data. These are nicknamed **Heisenbugs**: the act of observing them (attaching a debugger, adding a print, slowing things down) changes the timing and makes them vanish.
- **It can be silent.** A lost update doesn't crash — it just makes the number quietly wrong. No exception, no stack trace. The counter is off by a few; the balance doesn't reconcile; a cache entry is subtly stale. Corruption without a signal is the worst kind.
- **The guidance is to reason, not test, your way to safety.** The docs put it bluntly: *"Whenever you write a line of code, you must consider what might happen if a thread were preempted before executing the line (or before any of the individual machine instructions that make up the line), and another thread overtook it."* `[S1]` That habit — reading code and asking "what if I'm preempted *here*?" — is the skill the interview is probing.

> **Interview framing.** When you say "this is a data race, and races are non-deterministic so they won't show up reliably in tests — I need to make this critical section atomic by construction," you've just demonstrated the exact senior instinct the round grades. Naming the *danger* (silent, timing-dependent corruption) is as important as naming the fix.

---

## Part 5 — Broaden the pattern: the four shapes a race takes

`count++` is the poster child, but if that's the *only* shape you recognise you'll miss races in an interview. Here are the four common shapes — learn to *spot* each. All four are the same underlying disease (unsynchronised access to shared mutable state); they just look different on the page.

### Shape 1 — Read-modify-write (the lost update)
Already covered. Any "read it, change it, write it back" on shared state.
```
   count = count + 1        balance = balance - amount        list.append(x)
```
**Tell:** the new value depends on the old value, across a read then a write.

### Shape 2 — Check-then-act
Look at shared state, then act on what you saw — but between the *check* and the *act*, another thread changes it, so you act on a stale fact.
```
   if (cache does NOT contain key):        // CHECK
       value = expensiveCompute()
       cache.put(key, value)               // ACT  ← two threads can both pass the check,
                                           //        both compute, both put (double work / clobber)
```
The classic instances: **lazy initialisation** (`if (instance == null) instance = new ...` → two threads both see null, both construct), and **"contains? then add"** on a shared set. **Tell:** an `if` (or a lookup) on shared state whose truth another thread can invalidate before you use it.

### Shape 3 — Torn read / torn write
For values wider than the machine can read/write in one atomic step (a 64-bit `long`/`double`, or a multi-field struct), a reader can observe a value that is **half-old, half-new** — a state that was never actually written. Backed by the spec: wide types' reads/writes "need not be atomic." `[S2]`
```
   shared long x            // 64-bit; may be written as two 32-bit halves
   writer: x = 0xFFFFFFFF_FFFFFFFF
   reader: reads x  → could see 0x00000000_FFFFFFFF  (a value never assigned)
```
**Tell:** a shared field wider than a machine word, or a multi-field object, read without synchronisation.

### Shape 4 — Compound / multi-variable invariant
Two or more shared fields must stay consistent *with each other*, but you update them one at a time. A thread reads them mid-update and sees a **broken invariant**.
```
   invariant: accounts must always sum to the same total
   transfer(from, to, amt):
       from.balance -= amt        // ← another thread reading BOTH balances here
       to.balance   += amt        //   sees money that has left 'from' but not yet arrived in 'to'
```
Between the two lines, the total is temporarily wrong. **Tell:** an invariant that spans multiple fields, updated non-atomically. (This is why "make the critical section atomic" must sometimes cover *several* lines, not just one.)

> The unifying question for all four: **"Is there a moment, mid-operation, where another thread could look at this shared state and see something impossible or stale?"** If yes — race.

---

## Part 6 — The fix landscape (and the tradeoffs the round wants)

We won't *implement* the fixes yet (that's Lessons 03–07). But you must be able to lay out the options and argue between them — because "reliable, **fast**, **scalable**" means the interviewer will push on the *cost* of whatever you reach for. Recall the three legs from Part 1; each fix removes one.

```
   Attack "CONCURRENT"     →  MUTUAL EXCLUSION (locks)        →  Lesson 03
   Attack "MUTABLE"        →  IMMUTABILITY / atomic ops       →  Lessons 04
   Attack "SHARED"         →  CONFINEMENT (don't share)       →  design choice
```

| Fix | What it does | Buys you | Costs you | Reach for it when |
|---|---|---|---|---|
| **Mutual exclusion (lock)** | Force threads to take turns through the critical section — only one inside at a time | Correct for *any* critical section, however complex (multi-line, multi-field) | **Contention**: threads wait → less parallelism; risk of **deadlock** (L03); coarse locks throttle throughput | The critical section is non-trivial or spans multiple fields (Shapes 2 & 4) |
| **Atomic hardware op / CAS** | Do the whole read-modify-write as one indivisible instruction (e.g. atomic increment, compare-and-swap) | Very fast, **lock-free** (no blocking, no deadlock) for *simple* updates | Only works for *small* single-location updates; CAS retry loops get subtle (ABA, L07) | A single counter/flag/pointer update (Shape 1 on one variable) |
| **Immutability** | Make the data read-only after creation → no writer → no race | Zero synchronisation needed; trivially safe to share | You must build a *new* object to "change" it (allocation cost); not always natural | The value is set once and only read afterward |
| **Confinement** | Keep the data reachable by one thread only (locals, per-thread copies, or a single owning thread) | No sharing → no race → no synchronisation at all (often the *best* answer) | Requires structuring the design so state isn't shared; needs a hand-off mechanism | You can assign each piece of state a single owner (e.g. one thread owns the queue) |

The senior instinct, and a line worth having ready: **"The cheapest race to fix is the one you don't create — so first I'd ask whether this state needs to be shared and mutable at all. If it must be, I protect the critical section: an atomic operation if it's a single-location update, a lock if the invariant spans more than that — and then I worry about lock granularity so the fix doesn't kill throughput."** That single sentence hits *reliable* (protect it), *fast/scalable* (granularity), and shows judgment (don't over-synchronise). The docs echo the "don't over-do it" half: unnecessary synchronisation *"decreases performance and creates the possibility of deadlocks."* `[S1]`

> **Foreshadowing.** Notice we haven't said *how* a lock is built, why it also fixes **visibility** (a separate hazard from Lesson 04, where even a *finished* write may not be *seen* by another thread), or how to keep locks from deadlocking. Those are the next three lessons. This lesson's job was only: **see the race, name it, and know the shape of the cure.**

---

## Part 7 — The "ship": watch a real race happen

Reading about a lost update is one thing; watching a counter come up short is another. The runnable demo lives in the harness at [../code/Lessons/Lesson02Races.cs](../code/Lessons/Lesson02Races.cs). Run it:

```bash
cd systems_programming/code
dotnet run -- 02-races
```

What it does, mapped to this lesson:

1. **The unsynchronised counter (the bug).** Several threads each increment a *shared* counter many times with a plain `count = count + 1`. The correct total is known by construction (threads × increments). The demo runs the trial many times via the `SurfaceRaces` helper and reports how often the final total came up **short** — i.e. how often a lost update (Part 2b) actually occurred. You'll see a large fraction of runs are wrong, and *by a different amount each time* — non-determinism (Part 4) you can see.
2. **The same code, made atomic (the fix preview).** It then runs the identical workload but performs the increment as a **single atomic operation** instead of a three-step read-modify-write. Now every run yields exactly the expected total — 0% races. Same threads, same interleaving pressure; the only change is that the critical section became **indivisible**.

Predict before you run: the unsynchronised version should *lose* updates (final < expected) on many runs and never *exceed* the expected total (you can't over-count — you can only miss some); the atomic version should be exact every time. Watching the first number come up short, and vary run to run, is the entire lesson in one screen: **shared + mutable + concurrent, with a non-atomic critical section, silently loses data.**

---

## Self-check (say the answers out loud, as if teaching)

1. Name the **three properties** that must *all* hold for a concurrency bug to be possible, and give the fix that removes each one. Why is "at least one writer" required — why are two readers always safe?
2. Define a **race condition** in one sentence. Then walk the `count++` **lost update** step by step for two threads starting at `count = 5`, and say precisely *why* the final value is 6 and not 7. Which of concurrency vs parallelism causes it, and why can you reproduce it on one core?
3. Define **critical section** and **atomic**. Why is `count++` not atomic? What does it mean for another thread to observe a "halfway" state, and what's the general cure (in one phrase)?
4. What makes races **non-deterministic**, and why does that make them *more* dangerous than an ordinary bug? Explain "it passes tests, then corrupts data in production" and what a **Heisenbug** is.
5. A shared `long` (64-bit) is written by one thread and read by another, with no synchronisation. Nobody does `++` — it's a plain assignment. Can anything go wrong? Name the phenomenon and the underlying reason.
6. Give the **four shapes** a race takes, with a one-line example of each and the "tell" that flags it. Which shape needs the fix to span *multiple* lines/fields, and why can't a single atomic instruction cover it?
7. Lay out the **fix landscape** (mutual exclusion, atomic op/CAS, immutability, confinement): what each buys and costs, and when you'd pick each. Deliver the senior one-liner about *not over-synchronising* and why unnecessary locking hurts.

If any answer is shaky, re-read that Part before Lesson 03 — **Mutual exclusion & locks** — which takes Part 6's first row and actually builds it (and then shows how locks can deadlock).

---

## Sources

Facts above are cited inline by tag. Pulled/verified **2026-07-15**. Microsoft Learn is authoritative for .NET threading semantics; the C# language specification is authoritative for the atomicity guarantees. The concepts (race condition, critical section, atomicity, interleaving, lost update) are universal and language-agnostic; the citations below anchor the specific claims and the one concrete atomicity example.

- `[S1]` Microsoft Learn — *Managed Threading Best Practices (.NET)*, "Deadlocks and race conditions → Race conditions." Definition of a race condition (outcome depends on which thread reaches a block first; different, unpredictable results each run); the increment example decomposed into load/increment/store; preemption producing a lost update; the "consider what happens if a thread is preempted before any of the individual machine instructions" guidance; and that unnecessary synchronization decreases performance and risks deadlock. https://learn.microsoft.com/en-us/dotnet/standard/threading/managed-threading-best-practices (page ms.date 2026-03-13; retrieved 2026-07-15).
- `[S2]` Microsoft Learn — *C# language specification, §9.6 "Atomicity of variable references."* Reads and writes of `bool`, `char`, `byte`, `sbyte`, `short`, `ushort`, `uint`, `int`, `nint`, `nuint`, `float`, and reference types (and enums over those) are atomic; reads and writes of `long`, `ulong`, `double`, `decimal`, and user-defined types **need not** be atomic; and "there is no guarantee of atomic read-modify-write, such as in the case of increment or decrement." Used here as the concrete illustration of the universal hardware truths behind torn reads/writes and non-atomic read-modify-write. https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/variables (spec §9.6; page ms.date 2025-09-12; retrieved 2026-07-15).

**Not independently re-benchmarked this run (flagged):** the term "Heisenbug" is standard industry jargon (a bug that disappears under observation), used here pedagogically, not as a cited platform term. The four "shapes" taxonomy is a teaching device for grouping the same underlying data-race hazard; the individual instances (lost update, check-then-act/lazy-init, torn read, multi-variable invariant) are all standard, but the four-way grouping is ours for recognition purposes.
