# Class Session Protocol

A **class session** is a short teaching block at the **start of each day's session**, before any problem solving begins. Its purpose: load the mental model and context for the day's topic so solving isn't a cold-stare exercise.

## Why

Going into a topic cold — with only a terse taxonomy summary — leaves gaps that make problems feel unapproachable. The class session front-loads the general framework, recurring patterns, and reusable templates so that when a specific problem appears, it slots into an existing structure rather than starting from zero.

## When

- At the **beginning of each day** whose topic hasn't had a class yet.
- Triggered by the user asking to "teach me \<topic\>" / "hold class for \<topic\>".
- A topic gets **one canonical class file**, refined over time (not one per day).

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

## Workflow

1. User asks for the class on the day's topic.
2. Assistant teaches interactively (in chat) AND writes/updates `class/<topic>/<topic>.md` as the durable record.
3. Only after the class do we move on to solving the day's problems (normal solve → note → tracker → calendar/problem-set sync flow).
4. The class file is living: when a problem reveals a missing template or pitfall, fold it back into the class file.

---

## Scoped / just-in-time class (revised 2026-06-21)

The class is **scoped to the day**, not the whole topic. Front-loading 7–8 templates at once doesn't stick — you end up revisiting mid-solve anyway. So:

- **Teach only the 1–2 templates the day's problems actually use**, the morning of. (e.g. Kahn's topo-sort the day you solve Alien Dictionary; Hierholzer the day you solve Reconstruct Itinerary — not all of graphs up front.)
- **Still one canonical file per topic.** A scoped class **appends** to the existing `class/<topic>/<topic>.md` (e.g. every graph mini-class lands in `class/graphs/graphs.md`). The file grows into the full topic reference over time; it is never split per-day.
- Keep it to **15–25 min** (block 1 of the daily structure). Mental model + template + one "Picture it" trace, then go solve.

### Why this is legitimate prep, not spoon-feeding
Interviews **assume you already know the algorithms** (BFS, Dijkstra, DP, backtracking, monotonic stack, …). What's tested cold is (1) **recognizing** which known tool a novel problem reduces to, (2) **adapting** the template to its constraints, (3) **implementing** cleanly under time while narrating. Class that teaches the *tool* builds the assumed baseline. The line you must not cross: **studying the specific problem's solution/key-insight before attempting it** — that's the rep you'd be stealing from yourself.

## Hint ladder (how much help is allowed)

Use the lowest level that unblocks you, and **log which level each problem needed** — progress = needing lower levels over time.

| Level | When | What you may take |
|---|---|---|
| **L0 — Class** | Always (block 1) | Pattern vocabulary + template. Not a hint. |
| **L1 — Category nudge** | ~10–15 min stuck | "This is a graph / DP / greedy problem." Nothing more. |
| **L2 — Structural hint** | ~25–30 min stuck | The key invariant / state definition — **not** the full solution. |
| **L3 — Editorial** | Only after a genuine attempt | Read it, then type the solution from memory; it enters the tracker as a rep. |

Record the level in the problem's write-up (`notes/<topic>/<problem-id>.md`).

## Cold-first exam days

On exam days (currently Jun 27, Jul 3, Jul 4) the **first problem is attempted strictly cold** — **no class, no hints, timed, narrate aloud** — as the interview-realism test, then the normal class-then-solve flow resumes for the rest of the day. Pick the cold problem from a **pattern already classed earlier that week** (not a fresh Hard with zero encoding). The cold re-solve also counts as that problem's spaced rep.

As a topic consolidates, **taper the class**: full scoped class for the first problem in a pattern, then go straight to cold for later problems in the same pattern. Class density should fall as competence rises — that's the bridge to the all-cold interview.
