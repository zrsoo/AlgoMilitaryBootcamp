# I. Final advice — what to ignore, what to drill hard

## Ignore

- **Blind 75 as a complete plan.** It's a starter set. By Day 7 you should be past it.
- **Memorizing 200+ patterns.** There are ~15 patterns that matter. Master them.
- **Problem count as a metric.** "I did 400 problems" without a clean process beats nobody. A clean 120 beats a sloppy 400.
- **Math/geometry deep dives.** Modular, gcd/lcm, basic combinatorics — yes. FFT, computational geometry, MST optimizations — no.
- **Segment trees and BITs.** You'll use a TreeMap, sorted dict, or just sort + sweep. If a problem requires a segment tree, it's the wrong problem for you to spend time on.
- **Tarjan, Kosaraju, Hopcroft-Karp.** Out of scope at L4.
- **Concurrency past 3 problems** unless your recruiter explicitly says the team is platform/infra.
- **System design content in this plan.** It's an entirely separate prep track; do not bleed coding hours into it.
- **Mock interviews with random people who can't grade you.** Better to record yourself and review than to pair with someone who can't push back.
- **"Top 100 most-asked at FAANG" lists.** Databricks ≠ Google. Use the [Databricks signal](01-databricks-signal.md) list.

## Drill hard (in priority order)

1. **The ⚑ Databricks-signal problems** in [04](04-problem-set.md). Each one twice, with timer. If you only have 40 hours left, do these and nothing else.
2. **The ★ must-redo set** (30 problems). At least 2× during 44 days.
3. **Graph BFS variants** — Bus Routes, Shortest-Path-Visiting-All, Alien Dictionary, Itinerary. These exact problems get asked. Make them muscle memory.
4. **Parsing block** — atoi, Calculator I/II/III. Build one calculator template you can adapt under pressure.
5. **Design coding under contract** — LRU, LFU, Snapshot, In-Memory File System. Each has a non-obvious DS combo; you cannot improvise these in 25 minutes the first time.
6. **N-ary tree serialization** — explicit Databricks ask. Build BFS *and* DFS variants.
7. **Interval / sweep line** — Employee Free Time, Skyline, Meeting Rooms II. Sweep-line template.
8. **Binary search on answer** — Koko, Split Array, Capacity. The predicate-monotonicity reasoning is asked across many topics.
9. **Heap top-K and streaming median** — Top K Frequent, Median Stream. Tuple comparator + two heaps.
10. **Edge case discipline.** Practice saying the checklist out loud on every problem until it's reflex.

## Behaviors that disproportionately impress Databricks interviewers

- **State the brute force first.** Then immediately give its complexity. Then say "but we can do better because...". This rhythm signals seniority.
- **Volunteer tradeoffs.** "We could also use a `TreeMap` for O(log n) deletes, but `dict` + lazy deletion is simpler here." Even when interviewer doesn't ask.
- **Use small, named helper functions** even for short scripts. `def neighbors(r, c): ...` for grid problems. It shows you think in interfaces, which is Databricks-coded.
- **Walk through one concrete example after coding** — say "Let's trace through `arr = [3,1,4,1,5]`...". Catches bugs and signals thoroughness.
- **Restate the problem in your own words** at the start. Solves 30% of the "you misread the problem" failure mode.
- **Say "I don't know but here's how I'd find out"** when stuck on detail. Better than guessing.
- **Don't apologize.** "I'm a bit rusty on this" is a silent down-vote. Just code.

## Behaviors that disproportionately tank candidates

- Coding silently for 10 minutes.
- Asking for a hint within 3 minutes.
- Saying "this is easy" out loud (it never lands well).
- Skipping the brute force because "obviously it's O(n²)".
- Hand-waving complexity ("it's basically linear-ish").
- Refusing to use a hint, then spending 25 minutes stuck.
- Not asking *any* clarifying question. Real problems have ambiguity; asking signals engineering maturity.
- Failing to handle an obvious edge case after the interviewer literally hints at it.
- Pretending to know something you don't (interviewers can tell).

## The single most important shift in the next 44 days

Stop asking *"have I done this problem?"* and start asking *"can I solve a new one in this pattern, cold, in 25 minutes, with clean code, while talking?"*. Every block-2 session is a rehearsal for that.

Train the rehearsal, not the answer.

## When you walk into the interview

- You will see 1–2 problems you haven't seen before.
- You will probably stumble somewhere in the loop. That alone does not fail you.
- The interviewer wants you to succeed. They're trying to see how you think, not catch you out.
- The signal they grade on is mostly *process + communication + correctness*, in roughly that order, with optimality as a tiebreaker.
- If you've internalized the 18 patterns in [03](03-taxonomy.md) and the ⚑ set in [04](04-problem-set.md), and you can talk through them, you will pass.

Good luck. Now close this file and start Day 1.
