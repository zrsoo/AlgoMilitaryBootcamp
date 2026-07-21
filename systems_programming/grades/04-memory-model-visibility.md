# Lesson 04 — Memory model & visibility (volatile, barriers, Interlocked/CAS) — Quiz grades

Quizzed 2026-07-21 (spaced recall). 10 open-ended Qs, integrative with L01–L03. Scale: Strong / Partial / Weak.

**Overall: Solid — 7 Strong / 3 Partial / 0 Weak** (strongest quiz to date). Partials = Q1 (layer taxonomy), Q2 (happens-before ≠ program order), Q9 (Debug-hang reason + passing-run≠proof). Two excellent self-driven follow-ups (two-CAS multi-location; Interlocked-vs-spinlock progress).

---

## Q1 (L04) — three layers of reordering/invisibility + why single-thread-allowed; why while(!stop) can hang
**Grade: Partial** (mechanism strong, taxonomy muddled).
- **Stop-flag mechanism — Strong.** Nailed the headline: JIT **hoists** the never-written `stop` into a register, so the worker loops on a stale `false` forever and never re-reads memory. This is the exact `[S1]` failure.
- **Taxonomy — Partial.** Canonical three layers = (1) **compiler/JIT** (reorder independent instrs OR hoist a read into a register), (2) **CPU** (out-of-order execution + store buffers reorder loads/stores), (3) **cache hierarchy** (per-core L1/L2; a write lands in the writer's cache and may not propagate promptly). User instead listed: "CPU/JIT reordering" (merged layers 1+2 into one), "hoisting" (this is a *sub-case of the compiler/JIT layer*, not a co-equal third layer), and "cache" (correct). Net: only 2 of the 3 canonical layers cleanly named — the distinct **CPU/hardware out-of-order + store-buffer** layer got folded into "reordering" rather than standing on its own. Re-drill: compiler/JIT and CPU are two *different* reorderers; hoisting lives under the compiler one.
- **Single-thread reason — mostly Strong.** Correct instinct throughout: each optimization preserves *this* thread's result; the bug only appears because *another* thread observes the reordered/stale state (reordering) or because there's no other writer to invalidate a hoisted value (hoisting) or no other reader needing the not-yet-propagated write (cache). The canonical one-liner to lock in: *"reordering/caching is legal because it preserves single-threaded meaning — only a second thread can catch the intermediate state."*

## Q2 (L04) — define memory model + happens-before; meaning of no HB edge; why it's the stop-flag root cause
**Grade: Partial** (visibility defs good; happens-before definition imprecise — the key miss).
- **Memory model — Strong.** "Defines when a write on one core is visible to other cores." Captured the visibility half; add the "**and in what order**" half to be complete.
- **Happens-before — Partial, the crux correction.** Said "if A precedes B **in-code**, B sees A's writes." That conflates **program order with happens-before**. Across threads, HB is **not** automatic from source/wall-clock order — it must be **established by a synchronizing action** (lock release→next acquire, volatile write→volatile read of same field, Interlocked). Within one thread program order gives HB; between two threads you need a sync edge. This is *exactly* why the stop-flag breaks: main's write physically precedes the worker's read in time, yet there's **no HB edge** because nothing synchronizes them.
- **No HB edge — Partial.** Said "can't say which runs first" (ordering) — but it's also (and here, mainly) **"no guarantee either sees the other's writes"** (visibility). User did recover the visibility angle in the next paragraph.
- **Root cause / L02 contrast — Partial (nice preempt, incomplete).** Good instinct: "even if the flag reads/writes are atomic, it still breaks" — correctly separates this from atomicity. Did NOT fully spell the contrast asked for: an L02 race = two threads **collide** on one location (lost/torn update); the stop-flag has **no collision** — each access is a clean single read or single write; the bug is pure **non-publication** of a finished write. (Recurring "conclusion there, contrast-half thin" — pressed.)

## Q3 (L04 + L02) — what volatile guarantees; three things it doesn't; why it doesn't fix count++
**Grade: Strong.**
- Guarantee: ordering + visibility of the one location. ✓
- Three "does nots": not atomic for RMW ✓; can't apply to multi-word types (`double`/`long`) since those aren't atomically read/written → torn read ✓; no ordering guarantee across multiple locations ✓.
- `count++` walk: exact — both threads take a *fresh* volatile read of the same value, both `+1`, both write same result → one increment lost. Visibility fixed, race untouched.
- Minor precision: the "multiple locations" point is canonically "**no single total order** of volatile writes as seen from all threads" (two threads can observe two volatile writes in different orders); user framed it as timing/simultaneity — same truth, tighten the wording.

## Q4 (L04) — memory barrier def; how volatile/Interlocked/lock relate; why not by hand
**Grade: Strong.**
- Barrier = instruction forbidding reordering across it (before-can't-move-after and vice-versa). ✓
- volatile/Interlocked/lock all built on top of barriers. ✓
- Not by hand: low-level + error-prone; higher-level tools plant the barrier at the right spot with clearer intent; MS steers to lock/Interlocked. ✓
- Answered his "do I need the exact impl?" → no, but carry the mapping: volatile write ≈ store+release-barrier, volatile read ≈ acquire-barrier+load, lock/Interlocked include barriers.

## Q5 (L04 + L02) — CAS: what/returns; retry loop + optimistic; two costs vs lock; ABA
**Grade: Strong.**
- Signature (location/newValue/comparand), conditional swap, returns old value. ✓
- Retry loop correct; `until observed == previous` right; optimistic-vs-pessimistic (do work up-front, commit if unchanged vs exclude-everyone-first) nailed.
- Costs: gave "wastes retry work" + "keeps thread busy" — but those are two faces of the SAME high-contention spin cost. Coached the cleaner distinct pair: (a) spins/wastes work under high contention, (b) single-location only (multi-field wants a lock).
- ABA: correct A→B→A definition. Supplied "where it bites" = pointer/stack structures (reused address masks change); cures (version stamps, hazard pointers) = L07. He only needs to name it — did.
- **Excellent self-driven follow-up (his strength again):** challenged "single location" by proposing a two-CAS loop. Walked him through both failure modes: (1) no atomicity across the pair → observer sees half-updated state (bank-transfer example); (2) his retry **double-applies** delta1 if CAS2 fails after CAS1 succeeds (needs rollback → racy). Landed: hardware atomic = one word; multi-location-atomic needs a lock / pack-into-one-word / STM-DCAS.

## Q6 (L04 + L03) — why a lock gives visibility free; the HB edge; when still volatile/Interlocked
**Grade: Strong.**
- HB edge: release of a lock happens-before next acquire of same lock → next holder sees all critical-section writes. ✓ (Mechanism: acquire-barrier on acquire, release-barrier on release.)
- When still volatile/Interlocked: single-location update under low/medium contention to avoid the lock's blocking + context-switch cost. ✓ Added: `volatile` for a single published flag/reference read by many.

## Q7 (L04 design) — pick the tool for counter / config-ref / multi-field stats
**Grade: Strong.**
- Counter → Interlocked (+retry loop): single-location RMW, volatile can't do atomicity, no lock needed. ✓ Refined: bare increment = `Interlocked.Increment`; CAS+retry is the general form (for a *computed* new value).
- Config ref → volatile: publish a new immutable reference; only visibility matters. ✓ **Excellent unprompted caveat: "assuming the write is atomic, else lock"** — exactly the reference-writes-are-atomic nuance.
- Stats record → lock: spans multiple fields, neither volatile nor single CAS covers it. ✓ (Bonus offered: pack into one immutable record + swap reference → collapses to the volatile case.)
- **Two more strong self-driven follow-ups (his signature strength):** (a) proposed a two-CAS multi-location loop — walked through observer-sees-half-state + retry-double-apply bugs; (b) "how is Interlocked different from a spinlock, isn't it the same cost?" — clarified the real difference is **holds-a-lock vs holds-nothing → lock-free progress guarantee**, not cost; a stalled CAS thread blocks no one, a stalled spinlock holder freezes all spinners (single-core disaster). His cost/latency intuition for the both-spinning regime was correct.
- **Process note:** I initially over-explained the 3 cases and handed him the mapping; he flagged it ("you're handing it to me, give bare use-cases only"). Corrected → re-posed as 4-word stubs, answered cold. Keep scenario prompts terse; don't pre-solve the mapping.

## Q8 (L04 demo) — predict plain++ / Interlocked / volatile-++; what volatile-++ proves
**Grade: Strong (predictions) — headline "so what" left implicit (recurring pattern).**
- (a) plain `++` → races, loses increments ✓; (b) `Interlocked.Increment` → exact ✓; (c) volatile-`++` → still loses increments ✓.
- Asked for the one-sentence proof; referred back ("we went over the scenario") instead of stating it. Supplied + must own: **visibility ≠ atomicity** — volatile publishes each read/write but `++` stays a non-atomic RMW, so updates still lost; fixing visibility ≠ fixing the race. Pressed per standing directive.

## Q9 (L04 + honesty/sourcing) — what hangs the non-volatile worker; why "it depends" is correct
**Grade: Partial.**
- **Part 1 mechanisms — Strong.** hoisting → *indefinite* hang; slow cache propagation → *temporary* staleness. Distinguished the two correctly.
- **Single-core reasoning — Strong (self-driven).** hoisting = per-thread compiler optimization, independent of core count (hoisted value in register set, preserved across ctx switch) → can hang on one core; cache-propagation needs ≥2 cores; hoisting a non-issue single-thread. All correct.
- **Debug-build reason — WRONG.** Guessed "maybe runs on a single thread." Correct: Debug **disables JIT optimizations** → no register hoist → every iteration re-reads memory → worker exits. Same source correct in Debug, buggy in Release.
- **Part 2 — Partial (recurring "so-what half thin").** Got "nondeterministic / uncontrollable (hoisting, reordering)." Missed the two-sided point: "always hangs" = false (Debug); **"it stopped when I ran it → it's fine" = the WORSE error** = the concurrency trap that **a passing run ≠ proof of correctness** (latent, env-dependent bug). Honest stance separates the guaranteed/sourced fact (volatile fix provably correct) from the env-dependent symptom. Reusable lesson: write to the memory model / right primitive, don't "test until it looks fine."
