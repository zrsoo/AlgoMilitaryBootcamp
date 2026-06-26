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
| 5. Reset & graph/backtracking/DP core | 29–37 | Jun 20 – Jun 28 | Graph Hards (alien-dict / itinerary / visit-all), backtracking, DP | Distilled core after the Jun-21 re-plan |
| 6. Parsing / design / sharpening | 38–44 | Jun 29 – Jul 5 | atoi, calculators, LRU/LFU/snapshot/FS, exam cold-solves | Databricks-flavor Hards + interview readiness |

---

## Daily structure (every day, unless noted) — revised 2026-06-22

**Window: 08:00–11:00, hard stop at 11:00.** Cold-attempt-first (see [class/00-class-protocol.md](../class/00-class-protocol.md)): tools are acquired *ahead of time* in batched sessions, so solving days open with a recognition warm-up and run **cold**.

| Block | Time | Activity |
|---|---|---|
| 1 | 5–10 min | **Recognition warm-up.** Assistant gives 3–5 one-line problem statements (mixed topics, **not** today's). You name the tool + why. Trains recognition; no solving. |
| 2 | time-boxed | **Cold solve. 25 min/medium, 35 min/hard.** **2 new** problems on a medium day, **1 new** on a hard day. Straight in (tool already studied); narrate aloud. At the cap, surface what you have → hint ladder L1→L2→L3. Only a brand-new un-batched tool earns minimal neutral-example teaching first. |
| 3 | 20–30 min | **One spaced rep.** Redo a single tracker problem **blind**. If you fail, reset it. (Dig out more reps only on days with spare time.) |
| 4 | buffer → 11:00 | **Note write-up** (the debrief). 5-line summary per solved problem → `notes/<topic>/<problem-id>.md`, plus the hint level you needed. |

**Hard-stop rule:** if a problem blows past its time box, it was the *only* new problem that day and the spaced rep slides. Do not extend past 11:00.

**Exam days (Jun 27, Jul 3, Jul 4):** full solve day PLUS one extra **cold re-solve of a high-value ★/⚑ Hard** — no warm-up help, timed, narrate. This is the cold-transfer test; it also counts as that problem's spaced rep.

See the hint ladder in [class/00-class-protocol.md](../class/00-class-protocol.md) and progression rules in [06](06-workflow-rubric-rescue.md).

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
- Block 4: **tool-acquisition session** — teach/refresh the **new tools** the upcoming week's problems will need, on **neutral / already-solved examples** (no mention of which upcoming problem uses them). This is what keeps weekday solves cold. No new solve.

---

## Phase 1 — Foundations (Days 1–7, May 23–29)

**Progress marks:** ↻ = in tracker (active spaced reps) · ✓ = completed (clean first solve, or all 3 reps done).

| Day | Date | Topic | Problems (refs are #s in [04-problem-set.md](04-problem-set.md)) |
|---|---|---|---|
| 1 | Sat May 23 | Arrays/hashing | #1 ✓, #2 ✓, #3 (★) ↻ |
| 2 | Sun May 24 | Arrays/hashing + analysis | #4 ✓, #5 ✓; redo #2 |
| 3 | Mon May 25 | Two pointers | #8 (★) ↻, #9 ✓, #10 ✓ |
| 4 | Tue May 26 | Two pointers + Hard | #11 (★) ↻, #12 ✓ |
| 5 | Wed May 27 | Sliding window | #13 ✓, #15 ✓, #17 ✓ |
| 6 | Thu May 28 | Sliding window Hard | #14 (★) ↻, #16 (★) ↻ |
| 7 | Fri May 29 | Stack basics | #20 ✓, #21 (★) ✓, #24 ✓ |
| (Mock) | Sat May 30 | **Phase-1 mock** | Hard from arrays/sliding window |

Cumulative: ~16 problems. You should be done with templates for: hashmap counting, prefix sum, two-pointer triplet, sliding window, monotonic stack.

## Phase 2 — Structured patterns (Days 8–14, May 30–Jun 5)

| Day | Date | Topic | Problems |
|---|---|---|---|
| 8 | Sat May 30 | **Mock + analysis** | Mock (above) + #22 (★) ↻ histogram |
| 9 | Sun May 31 | Stack Hard + binary search | #23 ✓, #25 ✓; redo #16 |
| 10 | Mon Jun 1 | Binary search | #27 ✓, #28 ✓, #29 (★) ✓ |
| 11 | Tue Jun 2 | Binary search on answer | #30 (★) ↻, #31 ✓ |
| 12 | Wed Jun 3 | Binary search Hard | #32 (★) ↻; redo #14 |
| 13 | Thu Jun 4 | Intervals | #35 (⚑) ✓, #36 ✓, #37 (★ ⚑) ✓ |
| 14 | Fri Jun 5 | Intervals + sweep | #38 ✓, #39 (★ ⚑) ✓ |
| (Mock) | Sat Jun 6 | **Phase-2 mock** | Hard from binary search OR intervals |

Cumulative: ~32 problems.

## Phase 3 — Graphs & tries (Days 15–21, Jun 6–12)

| Day | Date | Topic | Problems |
|---|---|---|---|
| 15 | Sat Jun 6 | **Mock** + skyline | Mock + #40 (★ ⚑) ↻ |
| 16 | Sun Jun 7 | Heap | #44 (★) ✓, #45 ✓, #46 (★ ⚑) ✓ |
| 17 | Mon Jun 8 | Heap streaming + linked list | #47 (★ ⚑) ↻, #52 ✓, #53 ✓ |
| 18 | Tue Jun 9 | Linked list mid + Hard | #54 ✓, #55 (★) ↻, #56 ✓ |
| 19 | Wed Jun 10 | Trees core | #58 ✓, #59 ✓, #60 (★) ↻, #61 (★) ↻ |
| 20 | Thu Jun 11 | Trees Hard | #62 (★) ↻, #63 (★ ⚑) ↻, #65 |
| 21 | Fri Jun 12 | N-ary tree + tries | #64 (★ ⚑) ↻, #68 ✓, #69 ✓ |
| (Mock) | Sat Jun 13 | **Phase-3 mock** | Hard from trees or graphs |

Cumulative: ~52 problems.

## Phase 4 — DP + backtracking + greedy (Days 22–28, Jun 13–19)

Note: graphs slot in here on weekdays because graphs are the heaviest topic; do not delay them.

| Day | Date | Topic | Problems |
|---|---|---|---|
| 22 | Sat Jun 13 | **Mock** + trie Hard | Mock + #70 (★) Word Search II ↻ |
| 23 | Sun Jun 14 | Graphs grid + UF | #73 (★) ✓, #74 ✓, #79 ✓ |
| 24 | Mon Jun 15 | Topo sort + valid tree | #75 (★) ↻, #76 ✓, #78 ✓ |
| 25 | Tue Jun 16 | Graph BFS + Dijkstra | #77 (⚑) ✓, #80 (★) ↻, #81 ✓ |
| 26 | Wed Jun 17 | Graph Hard #1 | #82 (★ ⚑) Bus Routes ↻, #87 ↻ |
| 27 | Thu Jun 18 | Graph Hard #2 | #83 (★ ⚑) Alien Dict, #84 (⚑) Itinerary |
| 28 | Fri Jun 19 | Graph Hard #3 | #85 (★ ⚑) Shortest-visit-all (BFS+bitmask) |
| (Mock) | Sat Jun 20 | **Phase-4 mock** | Hard graph |

Cumulative: ~62 problems.

> NB: This phase is heavier on graphs than its title suggests. DP is intentionally slotted into Phase 5 to keep graphs concentrated.

## Phase 5 — Reset & graph/backtracking/DP core (Days 29–37, Jun 20–28)

> **Re-planned 2026-06-21.** Pace cut to a sustainable level (see Daily structure). Distilled to the ⚑/★ core; redundant reps dropped. Day 30 is a deliberate reset day.

| Day | Date | Tool (pre-taught) | New solves | Exam — cold ★/⚑ Hard |
|---|---|---|---|---|
| 29 | Sat Jun 20 | (past) | Phase-4 mock | — |
| 30 | Sun Jun 21 | — | **REST / RE-PLAN — no solving** | — |
| 31 | Mon Jun 22 | <details><summary>Reveal tool</summary>Topo-sort / Kahn + alien-dict</details> | #83 (★ ⚑ H) Alien Dictionary ✓ | — |
| 32 | Tue Jun 23 | <details><summary>Reveal tool</summary>Hierholzer / Euler path</details> | #84 (⚑ H) Reconstruct Itinerary ✓ | — |
| 33 | Wed Jun 24 | <details><summary>Reveal tool</summary>BFS + bitmask state</details> | #85 (★ ⚑ H) Shortest Path Visiting All Nodes ✓ | — |
| 34 | Thu Jun 25 | <details><summary>Reveal tool</summary>Backtracking template</details> | #88 Subsets ↻, #91 Word Search ↻ | — |
| 35 | Fri Jun 26 | <details><summary>Reveal tool</summary>Constraint backtracking</details> | #92 (H) N-Queens ✓ | — |
| 36 | Sat Jun 27 | <details><summary>Reveal tool</summary>DP 1-D + unbounded knapsack</details> | #95 House Robber, #97 (★) Coin Change | **#82 (★ ⚑ H) Bus Routes** |
| 37 | Sun Jun 28 | <details><summary>Reveal tool</summary>2-D string DP</details> | #104 (★ H) Regex Matching | — |

Every solve day also ends with **1 blind tracker rep** (block 3).

## Phase 6 — Parsing / design / sharpening (Days 38–44, Jun 29–Jul 5)

| Day | Date | Tool (pre-taught) | New solves | Exam — cold ★/⚑ Hard |
|---|---|---|---|---|
| 38 | Mon Jun 29 | <details><summary>Reveal tool</summary>Greedy + atoi parsing</details> | #107 Jump Game, #109 Gas Station, #113 (★ ⚑) atoi | — |
| 39 | Tue Jun 30 | <details><summary>Reveal tool</summary>Stack parsing + KV design</details> | #114 (★ ⚑) Basic Calculator II, #124 (⚑) Time-Based KV | — |
| 40 | Wed Jul 1 | <details><summary>Reveal tool</summary>Parens-recursion calculator</details> | #115 (★ ⚑ H) Basic Calculator I | — |
| 41 | Thu Jul 2 | <details><summary>Reveal tool</summary>Design (hashmap + DLL)</details> | #118 (★ ⚑) LRU, #120 (★ ⚑) Snapshot, #101 (★) Edit Distance | — |
| 42 | Fri Jul 3 | <details><summary>Reveal tool</summary>LFU buckets</details> | #119 (★ ⚑ H) LFU Cache | **#83 (★ ⚑ H) Alien Dictionary** |
| 43 | Sat Jul 4 | <details><summary>Reveal tool</summary>Trie / file-system design</details> | #122 (★ ⚑ H) In-Memory File System | **#85 (★ ⚑ H) Shortest Path Visiting All Nodes** |
| 44 | Sun Jul 5 | — | **RECAP — no new.** Re-read templates; talk through 3 ★ problems aloud; sleep early. | — |

**Optionals (slot only if ahead):** #98 (★) LIS, #116 (⚑) Basic Calculator III, #117 (⚑) Validate IP, #121 (⚑) Insert-Delete-GetRandom.

**Dropped from the original plan** (covered by representatives or low signal): redundant backtracking (#89, #90), redundant DP (#96, #99, #100, #102, #103, #105, #106), #65 Right Side View, #71 Autocomplete, design extras (#123, #125, #126, #127), CIDR #129, and **all concurrency (#130–132)**.

Cumulative target: the full ⚑ Databricks core + ★ pattern-defining set, each ★/⚑ Hard hit at least 2× (once fresh, once cold on an exam day).

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
