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
4. **Reusable templates** — copy-ready C# skeletons for the recurring shapes (traversals, searches, etc.).
5. **Common pitfalls** — the mistakes that recur (off-by-one, missing base case, wrong complexity, language footguns).
6. **Complexity cheat sheet** — typical time/space for the standard approaches.
7. **Map to the problem set** — which `#`/LC problems in `docs/04-problem-set.md` this class equips you for.

## Workflow

1. User asks for the class on the day's topic.
2. Assistant teaches interactively (in chat) AND writes/updates `class/<topic>/<topic>.md` as the durable record.
3. Only after the class do we move on to solving the day's problems (normal solve → note → tracker → calendar/problem-set sync flow).
4. The class file is living: when a problem reveals a missing template or pitfall, fold it back into the class file.
