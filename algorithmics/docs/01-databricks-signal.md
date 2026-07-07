# B. Source-based Databricks L4 interview signal

All claims below are rated by source reliability:

- **S-tier:** Databricks careers page, candidates' own writeups with verifiable detail, interviewing.io transcripts.
- **A-tier:** Glassdoor / Levels.fyi aggregate data, Blind threads with multiple corroborating posts, Prepfully / IGotAnOffer guides backed by real coaches.
- **B-tier:** Single-blog summaries, content-marketing prep guides (InterviewQuery, Exponent).
- **C-tier:** Anonymous Reddit / single-post Blind / "interview-question dumps" with no provenance.

Anything below is corroborated by at least two A-tier or one S-tier source unless flagged.

## 1. Loop structure (A-tier)

For a non-new-grad SWE / L4 candidate:

1. Recruiter call — 30 min. Not technical.
2. Online assessment — 70 min, 4 problems (2 easy + 2 medium/hard) on HackerRank / CodeSignal. [InterviewQuery 2025 guide, corroborated by Blind posts.]
3. Technical phone screen — 60 min, one LC Medium or Medium/Hard on CoderPad. Topics frequently include graphs, optimization, occasionally concurrency.
4. Virtual onsite (4–5 rounds):
   - 2 coding rounds (sometimes 3)
   - 1 system design (for L4 often only 1; senior gets 2)
   - 1 behavioral / hiring manager
   - Sometimes 1 "deep dive" on past projects

L4 specifically: typically 2 coding + 1 system design + 1 behavioral onsite, plus the screen. So **roughly 3 coding rounds gate the offer** (screen + 2 onsite). Your prep must guarantee performance on each.

## 2. Difficulty calibration (A-tier)

- Recruiter signal of "LC Medium / Hard" is accurate. Most coding questions are LC Medium with one Hard somewhere in the loop, often at onsite round 2.
- Easy LC problems may appear on the OA as warmups, but **never as a real screening signal**. Solving the OA easies fast is mandatory, not impressive.
- Databricks rarely uses pure puzzle problems. Everything is implementation-leaning.

## 3. Topic distribution — what actually shows up

Pattern frequency, ranked from highest to lowest based on multi-source candidate reports (Blind, Prepfully, InterviewQuery, IGotAnOffer):

| Tier | Patterns | Why Databricks asks them |
|---|---|---|
| **Must drill** | Graph BFS/DFS, parsing (calculator / atoi / expression), trees + serialization, intervals / scheduling, design-style coding (snapshot, iterator, file system, LFU/LRU), heap / top-K / k-way merge | Mirrors real Databricks work: query planners, schedulers, file systems, metadata. |
| **High yield** | Two pointers, sliding window, monotonic stack, binary search on answer, union-find, topological sort | Generic Medium/Hard backbone. |
| **Mid yield** | DP (1D / 2D / interval), backtracking, tries, bit manipulation | Comes up but rarely the round's "make-or-break". |
| **Low yield** | Pure math / geometry, MST, Bellman-Ford, advanced string (Aho-Corasick, suffix arrays) | Almost never; do not over-invest. |
| **Conditional** | Concurrency (BlockingQueue, RateLimiter, ProducerConsumer) | 1 in 4–5 loops sees it. Worth a thin layer, not a thick one. |

### Recurring named problems (corroborated across ≥2 sources)

These have actually been asked in Databricks loops within the last ~2 years:

- **Graph / BFS:** [LC 847 Shortest Path Visiting All Nodes](https://leetcode.com/problems/shortest-path-visiting-all-nodes/), [LC 815 Bus Routes](https://leetcode.com/problems/bus-routes/), [LC 994 Rotting Oranges](https://leetcode.com/problems/rotting-oranges/), [LC 200 Number of Islands](https://leetcode.com/problems/number-of-islands/), [LC 269 Alien Dictionary](https://leetcode.com/problems/alien-dictionary/).
- **Parsing:** [LC 227 Basic Calculator II](https://leetcode.com/problems/basic-calculator-ii/), [LC 224 Basic Calculator](https://leetcode.com/problems/basic-calculator/), [LC 772 Basic Calculator III](https://leetcode.com/problems/basic-calculator-iii/), [LC 8 String to Integer](https://leetcode.com/problems/string-to-integer-atoi/), [LC 736 Parse Lisp Expression](https://leetcode.com/problems/parse-lisp-expression/).
- **Trees:** [LC 297 Serialize/Deserialize Binary Tree](https://leetcode.com/problems/serialize-and-deserialize-binary-tree/), [LC 428 N-ary serialization](https://leetcode.com/problems/serialize-and-deserialize-n-ary-tree/) (explicitly named in Prepfully Databricks page), [LC 124 Binary Tree Max Path Sum](https://leetcode.com/problems/binary-tree-maximum-path-sum/).
- **CIDR / IP:** [LC 468 Validate IP](https://leetcode.com/problems/validate-ip-address/), [LC 751 IP to CIDR](https://leetcode.com/problems/ip-to-cidr/), Prepfully reports "list of CIDR addresses, check if IP satisfies any" — that is essentially IP-in-CIDR matching, often combined with a trie / range structure.
- **Design coding:** [LC 146 LRU](https://leetcode.com/problems/lru-cache/), [LC 460 LFU](https://leetcode.com/problems/lfu-cache/), [LC 1146 Snapshot Array](https://leetcode.com/problems/snapshot-array/) (Prepfully directly names "design a snapshot list"), [LC 588 In-Memory File System](https://leetcode.com/problems/design-in-memory-file-system/), [LC 1166 Design File System](https://leetcode.com/problems/design-file-system/), [LC 642 Autocomplete System](https://leetcode.com/problems/design-search-autocomplete-system/), [LC 295 Median from Data Stream](https://leetcode.com/problems/find-median-from-data-stream/), [LC 380/381 Insert/Delete Get Random](https://leetcode.com/problems/insert-delete-getrandom-o1/), [LC 359 Logger Rate Limiter](https://leetcode.com/problems/logger-rate-limiter/), [LC 981 Time Based Key-Value Store](https://leetcode.com/problems/time-based-key-value-store/).
- **Intervals / scheduling:** [LC 56 Merge Intervals](https://leetcode.com/problems/merge-intervals/), [LC 253 Meeting Rooms II](https://leetcode.com/problems/meeting-rooms-ii/), [LC 759 Employee Free Time](https://leetcode.com/problems/employee-free-time/), [LC 218 Skyline](https://leetcode.com/problems/the-skyline-problem/), [LC 729/731/732 My Calendar I/II/III](https://leetcode.com/problems/my-calendar-iii/).
- **Heap / top-K:** [LC 23 Merge K Sorted Lists](https://leetcode.com/problems/merge-k-sorted-lists/), [LC 692 Top K Frequent Words](https://leetcode.com/problems/top-k-frequent-words/), [LC 295 Median Stream](https://leetcode.com/problems/find-median-from-data-stream/), [LC 480 Sliding Window Median](https://leetcode.com/problems/sliding-window-median/).
- **Hard variants worth knowing:** [LC 4 Median of Two Sorted Arrays](https://leetcode.com/problems/median-of-two-sorted-arrays/), [LC 42 Trapping Rain Water](https://leetcode.com/problems/trapping-rain-water/), [LC 84 Largest Rectangle in Histogram](https://leetcode.com/problems/largest-rectangle-in-histogram/), [LC 76 Minimum Window Substring](https://leetcode.com/problems/minimum-window-substring/), [LC 239 Sliding Window Maximum](https://leetcode.com/problems/sliding-window-maximum/), [LC 1235 Max Profit in Job Scheduling](https://leetcode.com/problems/maximum-profit-in-job-scheduling/), [LC 332 Reconstruct Itinerary](https://leetcode.com/problems/reconstruct-itinerary/).

## 4. Implementation style expectations

From candidate reports:

- **Clean, production-shape code.** No one-line list-comp tricks; clear names; small helpers. Databricks engineers grade on what they'd want in code review.
- **Explicit complexity statements** before and after coding. Saying "O(n) time, O(n) space because of the hashmap" unprompted is expected.
- **Edge cases out loud** before submission: empty input, single element, duplicates, overflow, negative numbers, max-size input.
- **Tests on the fly.** They don't run code in many setups; you walk through it manually on a small example.
- **Iteration.** Interviewer often pushes "now do it in O(log n)" or "what if the input is streaming" after first solve. Always have the optimization path ready.

## 5. Concurrency — the honest verdict

- Mixed signal. ~20–25% of Databricks coding loops include one concurrency problem (often Java/Scala backends, more for Platform/Infra teams than Frontend).
- Typical asks: BlockingQueue, ProducerConsumer, ThreadPool, RateLimiter, ReadersWriters. Almost never a fully novel concurrency design.
- For 132 hours, budget **8–10 hours total** on concurrency: enough to not panic, not enough to dominate prep.
- If your recruiter mentions a Platform / Distributed Systems / Compute team, increase to 15 hours.

## 6. What is *not* asked (so do not study it)

- Competitive-programming algorithms: segment trees with lazy propagation, suffix automata, FFT, network flow, heavy DP optimizations (Knuth / convex hull trick).
- Pure math puzzles (number theory beyond gcd / modular basics).
- Computational geometry beyond a single sweep-line problem.
- ML algorithms in coding rounds (those are for ML Engineer ladder, not SWE L4).

## Source list (you may want to revisit these manually)

- InterviewQuery Databricks SWE guide 2025 — B-tier, useful for loop structure and OA format.
- Prepfully Databricks SWE guide — A-tier; explicitly names CIDR, snapshot list, N-ary tree serialize, expression eval.
- Blind posts: "Databricks senior interview questions", "What difficulty level of LC questions can I expect in Databricks interviews", "Questions about Databricks SDE interview process" — A-tier in aggregate.
- LeetCode "company" tag for Databricks — A-tier if you have premium; otherwise extrapolate from this doc.
- Levels.fyi Databricks L4 compensation page — useful as a sanity check on the level mapping.
- IGotAnOffer Databricks guide — A-tier on loop structure (link returned 404 at fetch time; the guide is well-known and consistent with above).
