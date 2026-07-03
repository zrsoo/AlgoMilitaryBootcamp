# G. Final 72 hours — Jul 3 (Fri) → Jul 5 (Sun), interview Mon Jul 6

**Budget:** ~3h Fri + 10h Sat + 10h Sun ≈ **23 hours**. This supersedes calendar Days 42–44 for the run-in.

## The one thesis for these 3 days

> **Whatever we solve now is Monday's frontline memory. Spend it on the highest-signal thing that is NOT yet banked, then lock it cold, then keep everything else warm.**

State audit (2026-07-03):

- **Strong & banked** (just needs warmth, do NOT re-learn): graphs (BFS/Dijkstra/topo/UF, Bus Routes, Alien Dict, Itinerary, Shortest-Visit-All), parsing (atoi, Calc I/II), trees + serialization (297, 428, 124, 236, 98), heap (23, 295, 215), binary search on answer (410, 875, 4), sliding window, monotonic stack, two-pointer, linked-list.
- **THE GAP → top priority:** the 4 design ⚑ problems with **no solution file yet** — final-advice #5, "you cannot improvise these in 25 minutes the first time":
  - **LRU #146** (hashmap + doubly-linked list) — the single most-asked design problem.
  - **LFU #460** (Hard) — builds on LRU; freq-bucket lists + minFreq.
  - **Snapshot Array #1146** — per-index `(snapId, val)` lists + binary-search-floor (you already own this search from #981).
  - **In-Memory File System #588** (Hard) — trie of dir/file nodes.

Priority order (final-advice §"Drill hard"): #1 the ⚑ set twice with timer · #3 graph BFS variants muscle-memory · #4 one calculator template · #5 design DS-combos · #6 N-ary serialize.

---

## FRI Jul 3 — ~3h — bank the two cheapest design templates

Goal: two of the four design problems in the bank before the big days. Pick the highest-probability + lowest-cost first: **LRU** (most-asked) and **Snapshot** (reuses the #981 floor-search you already know).

| Block | Time | Activity |
|---|---|---|
| Warm-up | 10 min | Recognition drill — I give 4 one-line statements (mixed, not design); you name the tool + why. |
| Solve 1 | ~55 min | **LRU Cache #146.** ✓ solved first try (2026-07-03). <details><summary>Tool</summary>`Dictionary<key, LinkedListNode>` + a doubly-linked list of `(key,val)`. `get`/`put` → move node to front; on overflow evict the tail; store the key in the node so eviction can delete from the dict. Narrate brute force (ordered list, O(n) move) → why O(1) needs the hashmap+DLL combo.</details> |
| Solve 2 | ~45 min | **Snapshot Array #1146.** ✓ solved first try (2026-07-03). <details><summary>Tool</summary>Per index keep a list of `(snapId, val)` appended in increasing snapId; `snap()` returns then increments a global counter; `get(i, snapId)` = upper-bound-floor binary search on index `i`'s list (same shape as #981's `Get`). Only write a new pair when the value actually changes.</details> |
| Write-up | 20–30 min | 5-line debrief each → `notes/design/146_*.md`, `notes/design/1146_*.md`; solution files under `solutions/design/`. Log the hint level. |

Stop at 3h. If LRU overruns, drop Snapshot to Saturday — LRU is the one that matters most.

---

## SAT Jul 4 — 10h — the two Hard designs + full mock

Morning is fresh-brain: spend it on the hardest new material (LFU, In-Mem FS). Afternoon is the simulated loop.

| Block | Time | Activity |
|---|---|---|
| Warm-up | 10 min | Recognition drill (mixed). |
| Solve 1 | ~50 min | **LFU Cache #460** (Hard). ✓ solved first try (2026-07-04). <details><summary>Tool</summary>`Dictionary<key,(val,freq)>` + `Dictionary<freq, LinkedList<key>>` (insertion-ordered per bucket) + `minFreq`. On access: bump freq, move key from bucket `f` to `f+1`, if bucket `f` empties and `f==minFreq` then `minFreq++`. On insert at capacity: evict the tail of `buckets[minFreq]`, then set `minFreq=1`. It's LRU-within-each-frequency-bucket.</details> |
| Solve 2 | ~50 min | **In-Memory File System #588** (Hard). <details><summary>Tool</summary>One node type: `Dictionary<string,Node> children` + optional file content (`StringBuilder`/`isFile`). Split path on `/`, walk/create nodes. `ls` → if file return its name, else sorted child names; `mkdir` → create missing dirs; `addContentToFile` → append; `readContentFromFile` → return content. Small helper `Node Traverse(path)`.</details> |
| Write-ups | 30 min | Debriefs + solution files for both. |
| **Break** | 30 min | Real break — protect the mock. |
| **Full mock** | 90 min | Databricks-simulated loop, cold, narrated, recorded: **1 Medium + 1 Hard back-to-back.** Pick from cold-topic pool (e.g. Med: #692 Top K Frequent Words or #56 Merge Intervals; Hard: #253/#759 scheduling or #772 Calculator III). No editorial. |
| Post-mortem | 30 min | Replay: where you went silent >30s, complexity stated cleanly?, edge cases named before submit?, brute-force-first rhythm? |
| Keep-warm A | ~60 min | Cold re-solve **2 graph ⚑ Hards** (final-advice #3): pick from Bus Routes #815, Alien Dict #269, Itinerary #332, Shortest-Visit-All #847. Blank file, timed. |
| Keep-warm B | ~60 min | Blind reps of 3 decaying in-rotation Hards: e.g. #297 serialize, #124 max path sum, #42 trap water (or #84, #218, #4). |
| Buffer | remainder | Overflow from any block; else re-solve LRU cold to confirm it stuck. |

---

## SUN Jul 5 — 10h — lock the design set cold, sweep for breadth, rehearse, sleep early

**No new patterns today.** Consolidation only — cold re-solves and talk-through. The design problems learned Fri/Sat get their locking rep here (a next-day cold re-solve is what makes them stick).

| Block | Time | Activity |
|---|---|---|
| Warm-up | 10 min | Recognition drill. |
| Design lock | ~2.5h | **Cold re-solve all 4 design problems from blank files** — LRU #146, Snapshot #1146, LFU #460, In-Mem FS #588. 25 min each + narration. This is the rep that converts "seen once" → "frontline memory." Any that still needs notes → do it twice. |
| Break | 20 min | — |
| Breadth sweep | ~3h | **One cold representative per pattern** you haven't touched in the last two days — hit each family so nothing is cold on Monday: sliding window (#76 or #239), monotonic stack (#84), binary search on answer (#410 or #875), heap/streaming (#295), topo/Dijkstra (#207 or #743), backtracking (#79), 2-D DP (#10), parsing (one calculator, #227). ~20 min each, narrate, blank file. |
| Break | 30 min | Real break. |
| Rehearsal | ~1.5h | **Talk-track drill (no coding).** For the ⚑ set + each of the 18 taxonomy patterns, say aloud in 60–90s: brute force → complexity → "better because…" → the DS combo → edge cases. This trains the *communication* signal Databricks grades first. Also rehearse the edge-case checklist (empty / single / dup / overflow / negative / max-size) until reflex. |
| Light finish | ~1.5h | Re-read all `notes/design/*` and the calculator/graph notes. Fix any template that felt shaky in the morning. **Then stop — no new problem after this.** |
| — | evening | Close the laptop early. Sleep is a performance input on Monday. |

---

## Monday morning (interview day) — do NOT cram

- Skim only: the 4 design DS-combos, the calculator template, the graph-BFS "index of inverted map / bitmask state" one-liners. 20 min max.
- Re-read the impress/tank behavior lists in [08-final-advice.md](08-final-advice.md).
- Restate the problem · brute force first + complexity · "better because…" · narrate · trace one example · name edge cases · don't apologize.

## Bookkeeping (per repo workflow)

As each design problem is solved, create `solutions/design/<lc>_<snake>.cs` + `notes/design/<lc>_<snake>.md`, and mark the calendar + problem-set label (`✓` if clean first solve, `↻` if it needed hints and should be re-solved cold Sunday).

## If you fall behind

Non-negotiable core, in order: **LRU #146 → Snapshot #1146 → LFU #460 → In-Mem FS #588**, then keep-warm on the graph ⚑ Hards. If time collapses, a banked LRU + Snapshot + one calculator + warm graphs beats four half-learned templates. Depth over breadth on the design gap.

---

## ALTERNATIVE track for Sat/Sun — recognition-triage loop (pick one Saturday morning)

Instead of the fixed hour-by-hour blocks above, run a **recognition-driven triage loop**. Rationale: recognition (naming the tool + why, cold) is the protected skill and the fastest way to *find* what's actually weak. Solve-time is then spent only where recognition breaks — no cycles wasted re-solving what's already reflexive.

**The loop:**

1. **I pitch a set of 4** one-line problem statements, **most-important-first** (priority pool below) and **mixed across topics** — never 4 from the same family in one set. Mixing is the point: it tests recognition without topic-priming (if you know the set is "all graphs," you're not really recognizing). You name **tool + why** for each — no solving.
2. **Any miss → we drop everything and cold-solve that problem immediately** (blank file, timed, narrated), then debrief + note. A "miss" = can't name the correct tool cleanly, names the wrong tool (like #2 in today's warm-up), OR it's a known-unbanked problem you've never implemented (the design ⚑ — recognition isn't enough, the DS-combo has to be in the hands).
3. **Clean hits** get a checkmark and are considered warm — we move on.
4. Back to step 1 with the next mixed set of 4. Repeat until the priority pool is exhausted or the day ends.

**Pitch priority pool (I draw the highest-priority items first, but each pitched set of 4 is a mix of topics — I interleave across these tiers rather than emptying one before the next):**

- **Tier 1 — design ⚑ (the known gap):** LRU #146, Snapshot #1146, LFU #460, In-Mem FS #588. These "miss" by default (never implemented) → they get solved first regardless. Same top priority as the fixed track, reached via the loop.
- **Tier 2 — graph BFS ⚑ (final-advice #3):** Bus Routes #815, Alien Dict #269, Reconstruct Itinerary #332, Shortest-Visit-All #847.
- **Tier 3 — parsing + serialize (#4, #6):** Basic Calculator I #224, Calculator II #227, atoi #8, N-ary serialize #428.
- **Tier 4 — the Hard backbone:** Max Path Sum #124, Trap Water #42, Largest Rectangle #84, Skyline #218.
- **Tier 5 — search/heap/window:** Median of Two Sorted #4, Median Stream #295, Min Window #76, Split Array #410.
- **Tier 6 — remaining pattern reps:** Word Search #79, Subsets #78, Regex DP #10, Course Schedule #207, Network Delay #743, Cheapest Flights #787, k-Group Reverse #25, Validate BST #98, Serialize Tree #297, LCA #236, Word Search II #212.

A well-formed pitched set therefore looks like *"one design + one graph + one parsing + one Hard-backbone"*, not *"four designs."* Priority controls **how early** an item shows up, not that a whole tier is pitched together.

**Why this can beat the fixed track:** it self-adjusts to the actual gaps instead of a guessed allocation, and it drills the exact skill Monday grades first (recognition + "brute force → better because…" out loud). **Why it might not:** if lots of sets come back clean, you spend the day confirming strength rather than banking the design set — so if you pick this track, **still force Tier 1 (all 4 design) to be solved regardless of the recognition result.** The fixed track guarantees the design bank; this track guarantees breadth-of-recognition. Pick based on how the LRU/Snapshot solves feel tonight.
