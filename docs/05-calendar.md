# F. 44-day calendar plan

**Window:** Sat May 23, 2026 → Sun July 5, 2026 (44 days, 132 hours).
**Daily budget:** 3 hours.
**Weekly mock:** Saturday afternoon, hard deadline.

## Phase map

| Phase | Days | Dates | Theme | Outcome |
|---|---|---|---|---|
| 1. Foundations | 1–7 | May 23 – May 29 | Arrays/hashing, two pointers, sliding window, stack, binary search | Pattern reflexes; templates internalized |
| 2. Structured patterns | 8–14 | May 30 – Jun 5 | Linked list, trees, heap, intervals | Comfortable with Medium in these areas |
| 3. Graphs & tries | 15–21 | Jun 6 – Jun 12 | Graphs (the biggest single block), tries | Can handle BFS/Dijkstra/UF + alien-dict / bus-routes / shortest-visit-all |
| 4. DP + backtracking + greedy | 22–28 | Jun 13 – Jun 19 | DP (1D/2D/interval/string), backtracking, greedy | Comfortable with Medium DP and 1 Hard DP |
| 5. Databricks-special + design + parsing + concurrency | 29–35 | Jun 20 – Jun 26 | Calculators, CIDR, snapshot, file system, LFU, autocomplete, concurrency | Inoculated against Databricks-flavor Hards |
| 6. Sharpening | 36–44 | Jun 27 – Jul 5 | Mocks, redo of ★ set, full timed simulations | Interview-day readiness |

---

## Daily structure (every day, unless noted)

| Block | Time | Activity |
|---|---|---|
| 1 | 15–20 min | **Review.** Re-read yesterday's mistake log + one template. |
| 2 | 90–120 min | **New solving.** 1 Medium + 1 Hard, or 2 Mediums + 1 design problem. Timed. |
| 3 | 30–45 min | **Write-up.** For each problem solved today: 5-line summary — pattern, brute force, optimized, key insight, edge cases. Save to `notes/<topic>/<problem-id>.md`. |
| 4 | 20–30 min | **Spaced repetition.** Redo a problem from 3, 7, or 14 days ago **without looking at solution**. If you fail, re-add to the queue. |

If a problem destroys block 2, **stop at 60 min**, read editorial, type up the solution from memory immediately after. Do not let one problem eat the whole day. See progression rules in [06](06-workflow-rubric-rescue.md).

---

## Saturdays — mock interview day

- Block 1: warm-up (one familiar Medium, 25 min).
- Block 2: **timed mock** — 45-min Hard from a *cold* topic. Use Pramp / interviewing.io / a friend / record yourself. No editorial allowed.
- Block 3: post-mortem write-up: what slowed you down, what you said vs should have said, complexity statement clarity.
- Block 4: redo of any catastrophe from the week.

If you cannot find a partner: do **self-mock** — set a 45-min timer, narrate aloud, record audio, replay it.

## Sundays — analysis day

- Block 1: re-read all write-ups from the week.
- Block 2: redo 1 Hard from the week, from scratch.
- Block 3: update mistake log; pick 2–3 patterns that are weakest; load those into next week's queue.
- Block 4: light — read one editorial, study one template, no new solve.

---

## Phase 1 — Foundations (Days 1–7, May 23–29)

**Progress marks:** ↻ = in tracker (active spaced reps) · ✓ = completed (clean first solve, or all 3 reps done).

| Day | Date | Topic | Problems (refs are #s in [04-problem-set.md](04-problem-set.md)) |
|---|---|---|---|
| 1 | Sat May 23 | Arrays/hashing | #1 ↻, #2 ✓, #3 (★) ↻ |
| 2 | Sun May 24 | Arrays/hashing + analysis | #4 ↻, #5 ↻; redo #2 |
| 3 | Mon May 25 | Two pointers | #8 (★) ↻, #9 ✓, #10 ↻ |
| 4 | Tue May 26 | Two pointers + Hard | #11 (★) ↻, #12 ↻ |
| 5 | Wed May 27 | Sliding window | #13 ✓, #15 ↻, #17 ↻ |
| 6 | Thu May 28 | Sliding window Hard | #14 (★) ↻, #16 (★) ↻ |
| 7 | Fri May 29 | Stack basics | #20 ✓, #21 (★) ✓, #24 ↻ |
| (Mock) | Sat May 30 | **Phase-1 mock** | Hard from arrays/sliding window |

Cumulative: ~16 problems. You should be done with templates for: hashmap counting, prefix sum, two-pointer triplet, sliding window, monotonic stack.

## Phase 2 — Structured patterns (Days 8–14, May 30–Jun 5)

| Day | Date | Topic | Problems |
|---|---|---|---|
| 8 | Sat May 30 | **Mock + analysis** | Mock (above) + #22 (★) ↻ histogram |
| 9 | Sun May 31 | Stack Hard + binary search | #23 ↻, #25 ✓; redo #16 |
| 10 | Mon Jun 1 | Binary search | #27 ↻, #28 ✓, #29 (★) ✓ |
| 11 | Tue Jun 2 | Binary search on answer | #30 (★) ↻, #31 ✓ |
| 12 | Wed Jun 3 | Binary search Hard | #32 (★) ↻; redo #14 |
| 13 | Thu Jun 4 | Intervals | #35 (⚑) ✓, #36 ✓, #37 (★ ⚑) ✓ |
| 14 | Fri Jun 5 | Intervals + sweep | #38 ↻, #39 (★ ⚑) ✓ |
| (Mock) | Sat Jun 6 | **Phase-2 mock** | Hard from binary search OR intervals |

Cumulative: ~32 problems.

## Phase 3 — Graphs & tries (Days 15–21, Jun 6–12)

| Day | Date | Topic | Problems |
|---|---|---|---|
| 15 | Sat Jun 6 | **Mock** + skyline | Mock + #40 (★ ⚑) ↻ |
| 16 | Sun Jun 7 | Heap | #44 (★) ✓, #45 ✓, #46 (★ ⚑) ↻ |
| 17 | Mon Jun 8 | Heap streaming + linked list | #47 (★ ⚑) ↻, #52 ✓, #53 ✓ |
| 18 | Tue Jun 9 | Linked list mid + Hard | #54 ↻, #55 (★) ↻, #56 |
| 19 | Wed Jun 10 | Trees core | #58, #59, #60 (★), #61 (★) |
| 20 | Thu Jun 11 | Trees Hard | #62 (★), #63 (★ ⚑), #65 |
| 21 | Fri Jun 12 | N-ary tree + tries | #64 (★ ⚑), #68, #69 |
| (Mock) | Sat Jun 13 | **Phase-3 mock** | Hard from trees or graphs |

Cumulative: ~52 problems.

## Phase 4 — DP + backtracking + greedy (Days 22–28, Jun 13–19)

Note: graphs slot in here on weekdays because graphs are the heaviest topic; do not delay them.

| Day | Date | Topic | Problems |
|---|---|---|---|
| 22 | Sat Jun 13 | **Mock** + trie Hard | Mock + #70 (★) Word Search II |
| 23 | Sun Jun 14 | Graphs grid + UF | #73 (★), #74, #79 |
| 24 | Mon Jun 15 | Topo sort + valid tree | #75 (★), #76, #78 |
| 25 | Tue Jun 16 | Graph BFS + Dijkstra | #77 (⚑), #80 (★), #81 |
| 26 | Wed Jun 17 | Graph Hard #1 | #82 (★ ⚑) Bus Routes, #87 |
| 27 | Thu Jun 18 | Graph Hard #2 | #83 (★ ⚑) Alien Dict, #84 (⚑) Itinerary |
| 28 | Fri Jun 19 | Graph Hard #3 | #85 (★ ⚑) Shortest-visit-all (BFS+bitmask) |
| (Mock) | Sat Jun 20 | **Phase-4 mock** | Hard graph |

Cumulative: ~62 problems.

> NB: This phase is heavier on graphs than its title suggests. DP is intentionally slotted into Phase 5 to keep graphs concentrated.

## Phase 5 — DP / parsing / design / concurrency (Days 29–35, Jun 20–26)

| Day | Date | Topic | Problems |
|---|---|---|---|
| 29 | Sat Jun 20 | **Mock** + backtracking | Mock + #88, #89, #91 |
| 30 | Sun Jun 21 | Backtracking Hard + DP 1D | #92, #95, #97 (★) |
| 31 | Mon Jun 22 | DP knapsack + LIS | #99, #98 (★) |
| 32 | Tue Jun 23 | DP string | #100, #101 (★), #102 |
| 33 | Wed Jun 24 | DP Hard | #104 (★) Regex, #103 |
| 34 | Thu Jun 25 | Greedy + design intro | #107, #109, #110, #118 (★ ⚑) LRU |
| 35 | Fri Jun 26 | Design block #1 | #119 (★ ⚑) LFU, #120 (★ ⚑) Snapshot |
| (Mock) | Sat Jun 27 | **Phase-5 mock** | Hard design or DP |

Cumulative: ~80 problems.

## Phase 6 — Sharpening (Days 36–44, Jun 27–Jul 5) — 9 days

This is where you stop "learning new patterns" and start "operating like an interview candidate". 50% of these hours are mocks and redo, not new problems.

| Day | Date | Topic | Problems / Activity |
|---|---|---|---|
| 36 | Sat Jun 27 | **Mock #1** | Mock + parsing #113 (★ ⚑) atoi, #114 (★ ⚑) calc II |
| 37 | Sun Jun 28 | Parsing Hard | #115 (★ ⚑) calc I, #116 (⚑) calc III |
| 38 | Mon Jun 29 | Databricks-special design | #122 (★ ⚑) In-Mem FS, #123 (⚑) Design FS |
| 39 | Tue Jun 30 | Databricks-special design | #124 (⚑) Time KV, #125 Rate Limiter, #126 |
| 40 | Wed Jul 1 | Design + autocomplete + CIDR | #71 (⚑) Autocomplete, #117 (⚑) Validate IP, #121 (⚑) RandGetRand |
| 41 | Thu Jul 2 | Concurrency thin | #130, #131, #132 + redo 2 ★ |
| 42 | Fri Jul 3 | **Full 3-problem timed simulation** | 90-min block: 1 E (10m) + 1 M (25m) + 1 H (45m), cold-pick from set |
| 43 | Sat Jul 4 | **Final mock** + post-mortem | 60-min Hard mock + write-up |
| 44 | Sun Jul 5 | **Light recap** | Re-read all template files; pick 3 ★ problems and *talk through them aloud* without coding; sleep early. **Do not solve a new problem today.** |

Cumulative: ~95 fresh problems + ~20 redo + spaced repetition coverage = ~115 unique touched, with the ★ set hit at least 2×.

---

## Weekly mock interview schedule (summary)

| Week | Mock date | Mock topic bias |
|---|---|---|
| 1 | Sat May 30 | Sliding window or arrays Hard |
| 2 | Sat Jun 6 | Binary search on answer Hard |
| 3 | Sat Jun 13 | Trees Hard (esp. serialize / path-sum) |
| 4 | Sat Jun 20 | Graph BFS Hard (Bus Routes / Alien Dict / Shortest-Visit-All) |
| 5 | Sat Jun 27 | DP Hard or design Hard (LFU / Snapshot) |
| 6 | Sat Jul 4 | **Full Databricks-simulated mock:** 1 Medium + 1 Hard back-to-back, 90 min total, design-flavored. |

**Mock protocol:** explain out loud constantly. If you go silent for more than 30 seconds, that's a fail signal — practice talking through pauses ("Let me think about this differently...").

---

## What slips first if you fall behind

If by the end of Phase 3 (Day 21) you have solved fewer than ~45 of the planned problems, you are in the **red zone**. Trigger the rescue plan in [06-workflow-rubric-rescue.md](06-workflow-rubric-rescue.md). In short: cut all R/S problems, keep only C + ⚑, drop concurrency entirely, and re-audit time spent per problem.
