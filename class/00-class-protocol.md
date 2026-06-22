# Class Session Protocol

This file defines how teaching is structured around the daily solve. **Current regime (2026-06-22): cold-attempt-first.** Tools are acquired *ahead of time* in batched sessions (never the morning of the problem that needs them); each solving day opens with a recognition warm-up and then a **cold** attempt. The authoritative flow is **[Morning flow & tool acquisition](#morning-flow--tool-acquisition--current-regime-revised-2026-06-22)** below — it supersedes the earlier "scoped class" regime.

## Why

You can't recognize or apply a tool you've never seen — so tools must be taught. But teaching them *attached to the problem that needs them* hands over the **reduction** (the highest-value, most transferable step). The resolution: teach the **tools** decoupled — ahead of time, on neutral examples — then make each problem a **cold** test of recognizing + adapting + implementing the right tool. Teaching builds the assumed baseline; the cold attempt protects the skill interviews actually test.

## When

- **Tool-acquisition** happens in **batched sessions ahead of the days that need the tools** (Sunday analysis day / a short pre-week slot) — *not* at the start of the solving day. Triggered by the user asking to "teach me \<tool\>" / "hold a tooling session", or by the assistant flagging that an upcoming stretch needs a tool not yet studied.
- **Solving days open cold** with a recognition warm-up, then a time-boxed attempt — no teaching, unless a new tool slipped through un-batched (block 2 fallback).
- A topic gets **one canonical class file** (the tool library), refined over time (not one per day).

## Where

```
class/
  <topic>/
    <topic>.md        ← the canonical class for that topic
```

Topic folder names mirror the solving topics (snake_case), e.g.:
`arrays_hashing/`, `two_pointers/`, `sliding_window/`, `stack/`, `binary_search/`,
`intervals/`, `heap/`, `linked_list/`, `trees/`, `tries/`, `graphs/`,
`backtracking/`, `dynamic_programming/`, `greedy/`, `parsing/`, `design/`, `concurrency/`.

## What a class file contains

1. **Context & when to reach for it** — what the topic is, what problem signals point to it.
2. **Mental model** — the core way to *think* about problems in this topic (the framing that makes them tractable).
3. **Core patterns / variants** — the handful of sub-patterns that cover most problems, each with its key idea.
4. **Reusable templates** — copy-ready C# skeletons for the recurring shapes (traversals, searches, etc.). Each template should be **self-contained for a reader with no prior knowledge**: pair the code with a small ASCII **"Picture it"** diagram that traces a tiny concrete example, then a plain-language "What it does" walkthrough. Don't assume the reader already knows the algorithm.
5. **Common pitfalls** — the mistakes that recur (off-by-one, missing base case, wrong complexity, language footguns).
6. **Complexity cheat sheet** — typical time/space for the standard approaches.
7. **Map to the problem set** — which `#`/LC problems in `docs/04-problem-set.md` this class equips you for.

## Workflow — tool-acquisition session (when learning a NEW tool)

Tool-acquisition is now **decoupled** from the daily solve (see [Morning flow & tool acquisition](#morning-flow--tool-acquisition--current-regime-revised-2026-06-22)). When picking up a new tool:

1. Teach the tool interactively (in chat) on a **neutral / already-solved example**, **ahead of** the day(s) that will need it — never the morning of its target problem.
2. Write/update `class/<topic>/<topic>.md` (the **tool library**) with the mechanism + template.
3. The class file is living: fold in a cleaner template or a new pitfall opportunistically when a solve reveals one — not a required step.

The per-problem **reduction is not recorded here** — it goes in the note (`notes/<topic>/<lc>.md`) after solving.

---

## Morning flow & tool acquisition — CURRENT REGIME (revised 2026-06-22)

**Why this replaces the 2026-06-21 scoped class.** A class *scoped to the day's problem* can't help but be the editorial: choosing which template to teach **is** the reduction. Teaching "Kahn's, for Alien Dictionary" pre-solves the single highest-value step — recognizing that a sorted word list is a topo-sort problem. So we decouple the class from the day's problem.

### The three skills (only one must be earned cold)
1. **Tool knowledge** — *what* topo sort / Dijkstra / Hierholzer is and how it runs. Must be taught (you can't recognize a tool you've never seen). Fine to "spoon-feed."
2. **Recognition / reduction** — mapping *this novel problem* → the right tool. **This is what interviews test, and what we protect.**
3. **Adaptation / implementation** — wiring the template under time, edge cases.

The old scoped class did all three. It must do only #1 (and #3 generically), **never #2 for the day's target.**

### Two artifacts, two roles
- **`notes/<topic>/<lc>.md`** = the per-problem **debrief** (reduction, key insight, where stuck, hint level). Already in the flow — this *is* the reduction record.
- **`class/<topic>/<topic>.md`** = the **tool library** — mechanisms + templates only. Grows **when a new tool is learned**, not after every solve. (Fold in a cleaner template opportunistically; not a required step.)

### Daily morning flow
| Block | What | Time |
|---|---|---|
| 1 | **Recognition warm-up.** Assistant gives 3–5 one-line problem statements (mixed topics, **not** today's problem). User names the tool + why. Trains the recognition skill directly. | ~5–10 min |
| 2 | **Cold solve.** If today's tool is already studied (the normal case) → straight in, no teaching. Only if a genuinely-new tool slipped through un-batched → minimal teaching on a **neutral** example first (mechanism only, never mapped to today's problem). | — |
| 3 | **Time-boxed attempt: 25 min medium / 35 min hard.** Narrate aloud. At the cap, surface what you have; if unsolved, walk the **hint ladder** (L1→L2→L3). | 25 / 35 min |
| 4 | **Note write-up** — the existing debrief + the hint level needed. | ~15 min |

### When do we teach new tools?
**Ahead of time, in batches, on neutral examples — never the morning of the problem that needs it** (same-morning teaching tells you the answer). By solve day the tool is "already studied," so the day is cold (block 2, normal case).

- Use the existing low-intensity windows: **Sunday analysis days** ("study one template" → promote to a proper tool-acquisition session) and/or a short pre-week slot.
- Teach on **neutral / abstract examples or already-solved problems**, with no mention of which upcoming problem uses the tool.
- Same-morning neutral-example teaching (block 2 fallback) is the **rare exception**, only for a new tool that wasn't pre-batched.
- Caveat: for a brand-new tool, recognition is barely testable on first contact anyway — the distance mainly ensures the day isn't a 1:1 giveaway; the protected value is the reduction + implementation, which cold-solving preserves regardless.

**The line you must not cross:** writing the day's problem's reduction / key-insight into the class file (or saying it aloud) *before* you've attempted it. The reduction is recorded **after**, in the note.

## Hint ladder (how much help is allowed)

Use the lowest level that unblocks you, and **log which level each problem needed** — progress = needing lower levels over time.

| Level | When | What you may take |
|---|---|---|
| **L0 — Tool library** | Studied ahead of time (batched) | Mechanism + template for tools you already learned. A standing reference, **not** a per-problem hint. |
| **L1 — Category nudge** | ~⅓ of the time box stuck | "This is a graph / DP / greedy problem." Nothing more. |
| **L2 — Structural hint** | ~⅔ of the time box stuck | The key invariant / state definition — **not** the full solution. |
| **L3 — Editorial** | At the cap, only after a genuine attempt | Read it, then type the solution from memory; it enters the tracker as a rep. |

Record the level in the problem's write-up (`notes/<topic>/<problem-id>.md`).

## Cold-first exam days

On exam days (currently Jun 27, Jul 3, Jul 4) the **first problem is attempted strictly cold** — **no warm-up help, no hints, timed, narrate aloud** — as the interview-realism test, then the normal cold-solve flow resumes for the rest of the day. Pick the cold problem from a **pattern whose tool was already studied earlier** (not a fresh Hard with a tool you've never seen). The cold re-solve also counts as that problem's spaced rep.

As a topic consolidates, **taper tool-teaching**: once a tool is studied, every later problem in that pattern is cold. Teaching density falls as competence rises — that's the bridge to the all-cold interview.
