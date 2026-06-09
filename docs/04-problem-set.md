# E. Final curated problem set (122 problems)

**Composition:**
- 122 problems total
- ~12% Easy (15) — warm-ups + design pattern intros
- ~58% Medium (71)
- ~30% Hard (36) — high enough to inoculate against a Hard onsite
- ~25% are explicit Databricks-flavor problems (graphs/BFS variants, parsing, design, intervals)

**Labels:**
- **C = Core** — must solve cleanly, with template internalized. Skipping = gap.
- **R = Reinforcement** — solve if on schedule; otherwise read solution + redo via spaced repetition.
- **S = Stretch-Hard** — only if you're ahead; do not stress.
- **★ = Must-redo** — these are pattern-defining; redo at least twice during the 44 days.
- **⚑ = Databricks-specific signal** — high probability of seeing this exact problem or a sibling.

**Progress marks** (appended to Label as problems are worked):
- **↻** = currently in `tracker.md` Active queue (spaced reps in flight)
- **✓** = completed (clean first solve, or all 3 reps passed)

---

## Topic 1 — Arrays & hashing (7)

| # | Problem | Diff | Label |
|---|---|---|---|
| 1 | [LC 49 Group Anagrams](https://leetcode.com/problems/group-anagrams/) | M | C ↻ |
| 2 | [LC 238 Product of Array Except Self](https://leetcode.com/problems/product-of-array-except-self/) | M | C ★ ✓ |
| 3 | [LC 560 Subarray Sum Equals K](https://leetcode.com/problems/subarray-sum-equals-k/) | M | C ★ ↻ |
| 4 | [LC 128 Longest Consecutive Sequence](https://leetcode.com/problems/longest-consecutive-sequence/) | M | C ↻ |
| 5 | [LC 974 Subarray Sums Divisible by K](https://leetcode.com/problems/subarray-sums-divisible-by-k/) | M | R ↻ |
| 6 | [LC 41 First Missing Positive](https://leetcode.com/problems/first-missing-positive/) | H | R ↻ |
| 7 | [LC 525 Contiguous Array](https://leetcode.com/problems/contiguous-array/) | M | R ↻ |

## Topic 2 — Two pointers (5)

| # | Problem | Diff | Label |
|---|---|---|---|
| 8 | [LC 15 3Sum](https://leetcode.com/problems/3sum/) | M | C ★ ↻ |
| 9 | [LC 11 Container With Most Water](https://leetcode.com/problems/container-with-most-water/) | M | C ✓ |
| 10 | [LC 75 Sort Colors](https://leetcode.com/problems/sort-colors/) | M | C ↻ |
| 11 | [LC 42 Trapping Rain Water](https://leetcode.com/problems/trapping-rain-water/) | H | C ★ ↻ |
| 12 | [LC 287 Find the Duplicate Number](https://leetcode.com/problems/find-the-duplicate-number/) | M | R ↻ |

## Topic 3 — Sliding window (7)

| # | Problem | Diff | Label |
|---|---|---|---|
| 13 | [LC 3 Longest Substring Without Repeating Characters](https://leetcode.com/problems/longest-substring-without-repeating-characters/) | M | C ✓ |
| 14 | [LC 76 Minimum Window Substring](https://leetcode.com/problems/minimum-window-substring/) | H | C ★ ↻ |
| 15 | [LC 424 Longest Repeating Character Replacement](https://leetcode.com/problems/longest-repeating-character-replacement/) | M | C ↻ |
| 16 | [LC 239 Sliding Window Maximum](https://leetcode.com/problems/sliding-window-maximum/) | H | C ★ ↻ |
| 17 | [LC 567 Permutation in String](https://leetcode.com/problems/permutation-in-string/) | M | C ↻ |
| 18 | [LC 992 Subarrays with K Different Integers](https://leetcode.com/problems/subarrays-with-k-different-integers/) | H | R |
| 19 | [LC 480 Sliding Window Median](https://leetcode.com/problems/sliding-window-median/) | H | R |

## Topic 4 — Stack / monotonic (7)

| # | Problem | Diff | Label |
|---|---|---|---|
| 20 | [LC 20 Valid Parentheses](https://leetcode.com/problems/valid-parentheses/) | E | C ✓ |
| 21 | [LC 739 Daily Temperatures](https://leetcode.com/problems/daily-temperatures/) | M | C ★ ✓ |
| 22 | [LC 84 Largest Rectangle in Histogram](https://leetcode.com/problems/largest-rectangle-in-histogram/) | H | C ★ ↻ |
| 23 | [LC 85 Maximal Rectangle](https://leetcode.com/problems/maximal-rectangle/) | H | C ↻ |
| 24 | [LC 394 Decode String](https://leetcode.com/problems/decode-string/) | M | C ↻ |
| 25 | [LC 71 Simplify Path](https://leetcode.com/problems/simplify-path/) | M | R ✓ |
| 26 | [LC 32 Longest Valid Parentheses](https://leetcode.com/problems/longest-valid-parentheses/) | H | R |

## Topic 5 — Binary search (8)

| # | Problem | Diff | Label |
|---|---|---|---|
| 27 | [LC 33 Search in Rotated Sorted Array](https://leetcode.com/problems/search-in-rotated-sorted-array/) | M | C ↻ |
| 28 | [LC 153 Find Minimum in Rotated Sorted Array](https://leetcode.com/problems/find-minimum-in-rotated-sorted-array/) | M | C ✓ |
| 29 | [LC 875 Koko Eating Bananas](https://leetcode.com/problems/koko-eating-bananas/) | M | C ★ ✓ |
| 30 | [LC 410 Split Array Largest Sum](https://leetcode.com/problems/split-array-largest-sum/) | H | C ★ ↻ |
| 31 | [LC 1011 Capacity To Ship Packages Within D Days](https://leetcode.com/problems/capacity-to-ship-packages-within-d-days/) | M | C ✓ |
| 32 | [LC 4 Median of Two Sorted Arrays](https://leetcode.com/problems/median-of-two-sorted-arrays/) | H | C ★ ↻ |
| 33 | [LC 162 Find Peak Element](https://leetcode.com/problems/find-peak-element/) | M | R |
| 34 | [LC 540 Single Element in a Sorted Array](https://leetcode.com/problems/single-element-in-a-sorted-array/) | M | R |

## Topic 6 — Intervals / sweep line (9)

| # | Problem | Diff | Label |
|---|---|---|---|
| 35 | [LC 56 Merge Intervals](https://leetcode.com/problems/merge-intervals/) | M | C ⚑ ✓ |
| 36 | [LC 57 Insert Interval](https://leetcode.com/problems/insert-interval/) | M | C ✓ |
| 37 | [LC 253 Meeting Rooms II](https://leetcode.com/problems/meeting-rooms-ii/) | M | C ★ ⚑ ✓ |
| 38 | [LC 435 Non-overlapping Intervals](https://leetcode.com/problems/non-overlapping-intervals/) | M | C ↻ |
| 39 | [LC 759 Employee Free Time](https://leetcode.com/problems/employee-free-time/) | H | C ★ ⚑ ✓ |
| 40 | [LC 218 The Skyline Problem](https://leetcode.com/problems/the-skyline-problem/) | H | C ★ ⚑ ↻ |
| 41 | [LC 731 My Calendar II](https://leetcode.com/problems/my-calendar-ii/) | M | C ⚑ |
| 42 | [LC 732 My Calendar III](https://leetcode.com/problems/my-calendar-iii/) | H | R ⚑ |
| 43 | [LC 1235 Maximum Profit in Job Scheduling](https://leetcode.com/problems/maximum-profit-in-job-scheduling/) | H | R |

## Topic 7 — Heap / priority queue (8)

| # | Problem | Diff | Label |
|---|---|---|---|
| 44 | [LC 215 Kth Largest Element in an Array](https://leetcode.com/problems/kth-largest-element-in-an-array/) | M | C ★ ✓ |
| 45 | [LC 347 Top K Frequent Elements](https://leetcode.com/problems/top-k-frequent-elements/) | M | C ✓ |
| 46 | [LC 23 Merge K Sorted Lists](https://leetcode.com/problems/merge-k-sorted-lists/) | H | C ★ ⚑ ↻ |
| 47 | [LC 295 Find Median from Data Stream](https://leetcode.com/problems/find-median-from-data-stream/) | H | C ★ ⚑ ↻ |
| 48 | [LC 1167 Minimum Cost to Connect Sticks](https://leetcode.com/problems/minimum-cost-to-connect-sticks/) | M | C |
| 49 | [LC 621 Task Scheduler](https://leetcode.com/problems/task-scheduler/) | M | R |
| 50 | [LC 692 Top K Frequent Words](https://leetcode.com/problems/top-k-frequent-words/) | M | R |
| 51 | [LC 658 Find K Closest Elements](https://leetcode.com/problems/find-k-closest-elements/) | M | R |

## Topic 8 — Linked lists (6)

| # | Problem | Diff | Label |
|---|---|---|---|
| 52 | [LC 206 Reverse Linked List](https://leetcode.com/problems/reverse-linked-list/) | E | C ✓ |
| 53 | [LC 21 Merge Two Sorted Lists](https://leetcode.com/problems/merge-two-sorted-lists/) | E | C ✓ |
| 54 | [LC 143 Reorder List](https://leetcode.com/problems/reorder-list/) | M | C |
| 55 | [LC 25 Reverse Nodes in k-Group](https://leetcode.com/problems/reverse-nodes-in-k-group/) | H | C ★ |
| 56 | [LC 138 Copy List with Random Pointer](https://leetcode.com/problems/copy-list-with-random-pointer/) | M | C |
| 57 | [LC 142 Linked List Cycle II](https://leetcode.com/problems/linked-list-cycle-ii/) | M | R |

## Topic 9 — Trees & BSTs (10)

| # | Problem | Diff | Label |
|---|---|---|---|
| 58 | [LC 104 Max Depth of Binary Tree](https://leetcode.com/problems/maximum-depth-of-binary-tree/) | E | C |
| 59 | [LC 102 Binary Tree Level Order Traversal](https://leetcode.com/problems/binary-tree-level-order-traversal/) | M | C |
| 60 | [LC 98 Validate BST](https://leetcode.com/problems/validate-binary-search-tree/) | M | C ★ |
| 61 | [LC 236 LCA of Binary Tree](https://leetcode.com/problems/lowest-common-ancestor-of-a-binary-tree/) | M | C ★ |
| 62 | [LC 124 Binary Tree Maximum Path Sum](https://leetcode.com/problems/binary-tree-maximum-path-sum/) | H | C ★ |
| 63 | [LC 297 Serialize and Deserialize Binary Tree](https://leetcode.com/problems/serialize-and-deserialize-binary-tree/) | H | C ★ ⚑ |
| 64 | [LC 428 Serialize and Deserialize N-ary Tree](https://leetcode.com/problems/serialize-and-deserialize-n-ary-tree/) | H | C ★ ⚑ |
| 65 | [LC 199 Binary Tree Right Side View](https://leetcode.com/problems/binary-tree-right-side-view/) | M | C |
| 66 | [LC 173 BST Iterator](https://leetcode.com/problems/binary-search-tree-iterator/) | M | C |
| 67 | [LC 230 Kth Smallest in BST](https://leetcode.com/problems/kth-smallest-element-in-a-bst/) | M | R |

## Topic 10 — Tries (5)

| # | Problem | Diff | Label |
|---|---|---|---|
| 68 | [LC 208 Implement Trie](https://leetcode.com/problems/implement-trie-prefix-tree/) | M | C |
| 69 | [LC 211 Design Add and Search Word](https://leetcode.com/problems/design-add-and-search-words-data-structure/) | M | C |
| 70 | [LC 212 Word Search II](https://leetcode.com/problems/word-search-ii/) | H | C ★ |
| 71 | [LC 642 Design Search Autocomplete System](https://leetcode.com/problems/design-search-autocomplete-system/) | H | C ⚑ |
| 72 | [LC 1268 Search Suggestions System](https://leetcode.com/problems/search-suggestions-system/) | M | R |

## Topic 11 — Graphs (15) — biggest investment

| # | Problem | Diff | Label |
|---|---|---|---|
| 73 | [LC 200 Number of Islands](https://leetcode.com/problems/number-of-islands/) | M | C ★ |
| 74 | [LC 133 Clone Graph](https://leetcode.com/problems/clone-graph/) | M | C |
| 75 | [LC 207 Course Schedule](https://leetcode.com/problems/course-schedule/) | M | C ★ |
| 76 | [LC 210 Course Schedule II](https://leetcode.com/problems/course-schedule-ii/) | M | C |
| 77 | [LC 994 Rotting Oranges](https://leetcode.com/problems/rotting-oranges/) | M | C ⚑ |
| 78 | [LC 261 Graph Valid Tree](https://leetcode.com/problems/graph-valid-tree/) | M | C |
| 79 | [LC 547 Number of Provinces](https://leetcode.com/problems/number-of-provinces/) | M | C |
| 80 | [LC 743 Network Delay Time](https://leetcode.com/problems/network-delay-time/) | M | C ★ |
| 81 | [LC 1631 Path With Minimum Effort](https://leetcode.com/problems/path-with-minimum-effort/) | M | C |
| 82 | [LC 815 Bus Routes](https://leetcode.com/problems/bus-routes/) | H | C ★ ⚑ |
| 83 | [LC 269 Alien Dictionary](https://leetcode.com/problems/alien-dictionary/) | H | C ★ ⚑ |
| 84 | [LC 332 Reconstruct Itinerary](https://leetcode.com/problems/reconstruct-itinerary/) | H | C ⚑ |
| 85 | [LC 847 Shortest Path Visiting All Nodes](https://leetcode.com/problems/shortest-path-visiting-all-nodes/) | H | C ★ ⚑ |
| 86 | [LC 305 Number of Islands II](https://leetcode.com/problems/number-of-islands-ii/) | H | R |
| 87 | [LC 787 Cheapest Flights Within K Stops](https://leetcode.com/problems/cheapest-flights-within-k-stops/) | M | R |

## Topic 12 — Backtracking (7)

| # | Problem | Diff | Label |
|---|---|---|---|
| 88 | [LC 78 Subsets](https://leetcode.com/problems/subsets/) | M | C |
| 89 | [LC 46 Permutations](https://leetcode.com/problems/permutations/) | M | C |
| 90 | [LC 39 Combination Sum](https://leetcode.com/problems/combination-sum/) | M | C |
| 91 | [LC 79 Word Search](https://leetcode.com/problems/word-search/) | M | C |
| 92 | [LC 51 N-Queens](https://leetcode.com/problems/n-queens/) | H | C |
| 93 | [LC 131 Palindrome Partitioning](https://leetcode.com/problems/palindrome-partitioning/) | M | R |
| 94 | [LC 17 Letter Combinations of a Phone Number](https://leetcode.com/problems/letter-combinations-of-a-phone-number/) | M | R |

## Topic 13 — Dynamic programming (12)

| # | Problem | Diff | Label |
|---|---|---|---|
| 95 | [LC 198 House Robber](https://leetcode.com/problems/house-robber/) | M | C |
| 96 | [LC 213 House Robber II](https://leetcode.com/problems/house-robber-ii/) | M | R |
| 97 | [LC 322 Coin Change](https://leetcode.com/problems/coin-change/) | M | C ★ |
| 98 | [LC 300 Longest Increasing Subsequence](https://leetcode.com/problems/longest-increasing-subsequence/) | M | C ★ |
| 99 | [LC 416 Partition Equal Subset Sum](https://leetcode.com/problems/partition-equal-subset-sum/) | M | C |
| 100 | [LC 1143 Longest Common Subsequence](https://leetcode.com/problems/longest-common-subsequence/) | M | C |
| 101 | [LC 72 Edit Distance](https://leetcode.com/problems/edit-distance/) | M | C ★ |
| 102 | [LC 5 Longest Palindromic Substring](https://leetcode.com/problems/longest-palindromic-substring/) | M | C |
| 103 | [LC 64 Minimum Path Sum](https://leetcode.com/problems/minimum-path-sum/) | M | C |
| 104 | [LC 10 Regular Expression Matching](https://leetcode.com/problems/regular-expression-matching/) | H | C ★ |
| 105 | [LC 312 Burst Balloons](https://leetcode.com/problems/burst-balloons/) | H | R |
| 106 | [LC 91 Decode Ways](https://leetcode.com/problems/decode-ways/) | M | R |

## Topic 14 — Greedy (4)

| # | Problem | Diff | Label |
|---|---|---|---|
| 107 | [LC 55 Jump Game](https://leetcode.com/problems/jump-game/) | M | C |
| 108 | [LC 45 Jump Game II](https://leetcode.com/problems/jump-game-ii/) | M | C |
| 109 | [LC 134 Gas Station](https://leetcode.com/problems/gas-station/) | M | C |
| 110 | [LC 763 Partition Labels](https://leetcode.com/problems/partition-labels/) | M | C |

## Topic 15 — Bit / math / parsing (7) — Databricks parsing emphasis

| # | Problem | Diff | Label |
|---|---|---|---|
| 111 | [LC 136 Single Number](https://leetcode.com/problems/single-number/) | E | C |
| 112 | [LC 191 Number of 1 Bits](https://leetcode.com/problems/number-of-1-bits/) | E | C |
| 113 | [LC 8 String to Integer (atoi)](https://leetcode.com/problems/string-to-integer-atoi/) | M | C ★ ⚑ |
| 114 | [LC 227 Basic Calculator II](https://leetcode.com/problems/basic-calculator-ii/) | M | C ★ ⚑ |
| 115 | [LC 224 Basic Calculator](https://leetcode.com/problems/basic-calculator/) | H | C ★ ⚑ |
| 116 | [LC 772 Basic Calculator III](https://leetcode.com/problems/basic-calculator-iii/) | H | C ⚑ |
| 117 | [LC 468 Validate IP Address](https://leetcode.com/problems/validate-ip-address/) | M | C ⚑ |

## Topic 16 — Design-style coding (12) — Databricks favorite zone

| # | Problem | Diff | Label |
|---|---|---|---|
| 118 | [LC 146 LRU Cache](https://leetcode.com/problems/lru-cache/) | M | C ★ ⚑ |
| 119 | [LC 460 LFU Cache](https://leetcode.com/problems/lfu-cache/) | H | C ★ ⚑ |
| 120 | [LC 1146 Snapshot Array](https://leetcode.com/problems/snapshot-array/) | M | C ★ ⚑ |
| 121 | [LC 380 Insert Delete GetRandom O(1)](https://leetcode.com/problems/insert-delete-getrandom-o1/) | M | C ⚑ |
| 122 | [LC 588 Design In-Memory File System](https://leetcode.com/problems/design-in-memory-file-system/) | H | C ★ ⚑ |
| 123 | [LC 1166 Design File System](https://leetcode.com/problems/design-file-system/) | M | C ⚑ |
| 124 | [LC 981 Time Based Key-Value Store](https://leetcode.com/problems/time-based-key-value-store/) | M | C ⚑ |
| 125 | [LC 359 Logger Rate Limiter](https://leetcode.com/problems/logger-rate-limiter/) | E | C |
| 126 | [LC 1396 Design Underground System](https://leetcode.com/problems/design-underground-system/) | M | C |
| 127 | [LC 528 Random Pick with Weight](https://leetcode.com/problems/random-pick-with-weight/) | M | C |
| 128 | [LC 348 Design Tic-Tac-Toe](https://leetcode.com/problems/design-tic-tac-toe/) | M | R |
| 129 | [LC 751 IP to CIDR](https://leetcode.com/problems/ip-to-cidr/) | M | R ⚑ |

## Topic 17 — Concurrency (3, thin layer)

| # | Problem | Diff | Label |
|---|---|---|---|
| 130 | [LC 1114 Print in Order](https://leetcode.com/problems/print-in-order/) | E | C |
| 131 | [LC 1115 Print FooBar Alternately](https://leetcode.com/problems/print-foobar-alternately/) | M | C |
| 132 | [LC 1188 Design Bounded Blocking Queue](https://leetcode.com/problems/design-bounded-blocking-queue/) | M | C |

> Note: actual count is 132 because design-coding deserves the extra weight. If time gets tight, drop R-labeled problems first; the C-only set is 88 problems.

---

## Must-redo set (★) — redo at least 2× during the 44 days

These are the 30 pattern-defining problems. Even if you "solved" them once, redo them in week 4 and week 6 from scratch.

LC 238, 560, 15, 42, 76, 239, 739, 84, 875, 410, 4, 253, 759, 218, 215, 23, 295, 25, 98, 236, 124, 297, 428, 212, 200, 207, 743, 815, 269, 847, 322, 300, 72, 10, 113 (atoi), 227, 224, 146, 460, 1146, 588.

## Skip-if-tight set (the first to drop if you fall behind)

LC 525, 992, 480, 32, 540, 732, 1235, 658, 142, 230, 86 (305 Islands II), 787, 131, 17, 213, 312, 91, 348, 751 — and all S-labeled problems.

## Databricks signal map (⚑) — guaranteed practice

If you skip everything else, do **at least these**:

- Trees: LC 297, 428, 124
- Graphs: LC 200, 994, 269, 332, 815, 847
- Parsing: LC 8, 227, 224, 772
- Intervals: LC 56, 253, 759, 218, 731
- Heap/streaming: LC 23, 295
- Design: LC 146, 460, 1146, 588, 1166, 981, 642, 380, 1396
- CIDR/IP: LC 468, 751
- Concurrency: LC 1114, 1115, 1188 (only if signal)

That is a 30-problem absolute minimum core. Anything else fills in the patterns around it.
