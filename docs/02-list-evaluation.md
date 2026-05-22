# C. Evaluation of existing curated problem lists

Bluntly: **none of the famous lists are right for Databricks L4 in 132 hours.** They are all calibrated either for new grads or for generic FAANG. Below is what each gets right, what it wastes time on, and what is missing.

## Side-by-side

| List | Size | Hard ratio | Coverage of Databricks-flavor topics | Verdict |
|---|---|---|---|---|
| **Blind 75** | 75 | ~10% | Misses design, intervals beyond Meeting Rooms, parsing, CIDR, advanced graph BFS, file-system design | **Insufficient alone.** Good first-week skeleton, nothing more. |
| **Blind 150 / NeetCode 150** | 150 | ~20% | Better breadth; still light on design-coding and parsing; over-invests in easy reps | **Closer**, but ~30 problems are filler for L4. |
| **Grind 75** | 75 | ~12% | Same gaps as Blind 75; problem selection slightly more modern | Skip. |
| **Grind 169** | 169 | ~18% | Reasonable coverage; weaker on Databricks specials | Use as a *source pool*, not a checklist. |
| **NeetCode All / NC250** | 250+ | ~25% | Most complete generic coverage | Too big for 132 h. Use selectively. |
| **LC Databricks company tag (top 50 by frequency)** | ~50 | ~30% | Highest signal for actual Databricks asks | **Mandatory.** Forms the spine of the final set. |
| **AlgoMonster / SeanPrashad / Curated company sheets** | varies | varies | The "company-tagged" curated sheets (SeanPrashad's `leetcode-patterns`, AlgoMonster patterns) are useful pattern indexes | Use AlgoMonster's pattern templates, ignore the rest. |

## Specific waste-of-time problems in those lists for *Databricks L4*

Skip or skim these even if Blind / NeetCode include them — they teach nothing new at L4 level:

- LC 1 Two Sum, LC 121 Best Time to Buy and Sell Stock, LC 217 Contains Duplicate, LC 242 Valid Anagram, LC 125 Valid Palindrome — you should already solve these in <3 min cold. If not, do them once in week 1 and move on.
- LC 70 Climbing Stairs — only useful as a 5-minute DP-intro warmup.
- LC 226 Invert Binary Tree — trivial; useful only as a recursion warmup.
- LC 543 Diameter, LC 110 Balanced BST — useful but redo only via spaced repetition.
- LC 252 Meeting Rooms I — subsumed by LC 253. Solve once.

## Specific *missing* problems in Blind 75 / NeetCode 150 that you must add for Databricks

These do not appear (or only barely appear) in the popular lists, yet they map directly to Databricks-flavor rounds:

- LC 224 / 227 / 772 Basic Calculator I / II / III
- LC 736 Parse Lisp Expression
- LC 468 Validate IP, LC 751 IP to CIDR
- LC 1146 Snapshot Array
- LC 588 In-Memory File System, LC 1166 Design File System
- LC 642 Design Search Autocomplete System
- LC 981 Time Based Key-Value Store
- LC 460 LFU Cache (Blind has LRU, not LFU)
- LC 759 Employee Free Time
- LC 731 / 732 My Calendar II / III
- LC 815 Bus Routes
- LC 847 Shortest Path Visiting All Nodes
- LC 1235 Maximum Profit in Job Scheduling
- LC 1396 Design Underground System
- LC 432 All O(1) Data Structure
- LC 528 Random Pick with Weight
- LC 1268 Search Suggestions System

These are the high-yield additions. **Adding them to a NeetCode 150 base gives you something close to a Databricks-shaped problem set.** That is exactly what [the final set](04-problem-set.md) does.

## Recommendation

Do **not** "complete" any famous list. Use NeetCode 150 (or Grind 169) as a *source of templates and reference solutions*, and follow [the curated 122-problem set](04-problem-set.md) as your actual checklist. It is built specifically for Databricks L4 in 132 hours.

## Better current resources to use *alongside* problems

- **NeetCode.io video walkthroughs** — best free explanations of templates. Use only after attempting a problem.
- **AlgoMonster pattern guide** — pattern recognition cheat sheet. Free patterns are enough; paid not required.
- **Hello Interview** (system design) — for the side system-design round (not in scope here but worth bookmarking).
- **interviewing.io free recordings** — watch 2–3 Databricks-comparable mocks to calibrate communication style.
- **Codeforces problem 1200–1600 rating** — only if you finish the set early and want to harden DP/graph speed.

Avoid: anything claiming to "memorize 200 patterns in 30 days", paid mass-problem trackers, AI question banks with no provenance.
