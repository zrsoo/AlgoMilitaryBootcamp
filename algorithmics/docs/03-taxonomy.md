# D. Algorithm / method taxonomy

This is the **planning unit** of your prep. Every problem you solve should map to one of these 18 categories. If you cannot map it, you have not understood it.

For each category:
- **Concepts** — what to study.
- **Recognition cues** — what wording in the problem statement tips you off.
- **Template** — the skeleton you write before specializing.
- **Traps** — what kills candidates.
- **Representative problems** — 5–15, labeled **C** (Core, must solve), **R** (Reinforcement, do if time), **S** (Stretch-Hard, do if you want top-quartile readiness).

Difficulty: E = LC Easy, M = Medium, H = Hard.

---

## 0. Interview fundamentals (meta, not a category)

- **Complexity:** practice stating Big-O cold for every solution. Both worst and amortized for hash structures. Mention space.
- **Brute-force-first:** *always* state the brute force and its complexity before optimizing. This is half the signal Databricks scores you on.
- **Edge-case checklist:** empty, 1-element, all-equal, sorted, reverse-sorted, duplicates, max-size, overflow, negative, single-character strings, cycles in graphs, disconnected graphs.
- **Talking model:** *Clarify (1–2 min) → restate (30 s) → brute force + complexity → optimization idea → code → walk through example → complexity restate → edge cases → "any extensions?"*. Practice this rhythm; do not improvise it on the day.
- **Debugging:** prints to stderr equivalent; track an invariant per loop; binary-search the bug if you cannot reproduce in 2 min.

---

## 1. Arrays & hashing

**Concepts:** frequency maps, prefix sums, prefix-sum-mod (subarray-sum problems), set-based dedup, sorted-key vs insertion-order maps, grouping by canonical key (sorted-string, char-count tuple).

**Recognition cues:** "count subarrays with sum K", "anagram groups", "first non-repeating", "longest substring without ...".

**Template (prefix-sum subarray):**
```csharp
var seen = new Dictionary<long, int> { [0] = 1 }; // prefix sum -> count
long psum = 0;
int ans = 0;
foreach (var x in arr)
{
    psum += x;
    if (seen.TryGetValue(psum - k, out var c)) ans += c;
    seen[psum] = seen.GetValueOrDefault(psum, 0) + 1;
}
```

**Traps:** off-by-one when initializing `seen[0] = 1`; mutating a `Dictionary` while iterating (throws `InvalidOperationException`); choosing the wrong key type (use a `ValueTuple` for composite keys — reference-type keys hash by identity unless overridden); `HashSet<T>` enumeration order is not guaranteed.

**Problems:**
- **C** [LC 49 Group Anagrams](https://leetcode.com/problems/group-anagrams/) (M)
- **C** [LC 238 Product of Array Except Self](https://leetcode.com/problems/product-of-array-except-self/) (M)
- **C** [LC 560 Subarray Sum Equals K](https://leetcode.com/problems/subarray-sum-equals-k/) (M)
- **C** [LC 128 Longest Consecutive Sequence](https://leetcode.com/problems/longest-consecutive-sequence/) (M)
- **C** [LC 974 Subarray Sums Divisible by K](https://leetcode.com/problems/subarray-sums-divisible-by-k/) (M)
- **R** [LC 41 First Missing Positive](https://leetcode.com/problems/first-missing-positive/) (H) — array-as-hashmap trick.
- **R** [LC 525 Contiguous Array](https://leetcode.com/problems/contiguous-array/) (M)
- **S** [LC 1330 Reverse Subarray to Maximize Array Value](https://leetcode.com/problems/reverse-subarray-to-maximize-array-value/) (H)

---

## 2. Two pointers

**Concepts:** sorted two-pointer for pair/triplet sums; fast/slow on linked lists or arrays; partitioning (Dutch national flag); shrinking window from both ends.

**Recognition cues:** sorted array + target; remove duplicates in place; partition by predicate; "in O(1) extra space".

**Template (triplet sum):**
```csharp
Array.Sort(arr);
var res = new List<int[]>();
for (int i = 0; i < arr.Length - 2; i++)
{
    if (i > 0 && arr[i] == arr[i - 1]) continue;
    int l = i + 1, r = arr.Length - 1;
    while (l < r)
    {
        int s = arr[i] + arr[l] + arr[r];
        if (s < 0) l++;
        else if (s > 0) r--;
        else
        {
            res.Add(new[] { arr[i], arr[l], arr[r] });
            l++; r--;
            while (l < r && arr[l] == arr[l - 1]) l++;
            while (l < r && arr[r] == arr[r + 1]) r--;
        }
    }
}
```

**Traps:** dedup after appending, not before; integer overflow on `arr[l] + arr[r]` (cast to `long` when values can be near `int.MaxValue`); forgetting that fast/slow cycle detection needs Floyd's two-phase to find the cycle start.

**Problems:**
- **C** [LC 15 3Sum](https://leetcode.com/problems/3sum/) (M)
- **C** [LC 11 Container With Most Water](https://leetcode.com/problems/container-with-most-water/) (M)
- **C** [LC 75 Sort Colors](https://leetcode.com/problems/sort-colors/) (M)
- **C** [LC 42 Trapping Rain Water](https://leetcode.com/problems/trapping-rain-water/) (H)
- **R** [LC 18 4Sum](https://leetcode.com/problems/4sum/) (M)
- **R** [LC 287 Find the Duplicate Number](https://leetcode.com/problems/find-the-duplicate-number/) (M) — Floyd's.

---

## 3. Sliding window

**Concepts:** fixed window (size k), variable window (shrink while invariant violated), frequency-counter windows, "at most K" → minus "at most K-1" trick for "exactly K".

**Recognition cues:** "longest / shortest substring such that ...", "at most K distinct", "window of size k", "max sum subarray length k".

**Template (variable window with counter):**
```csharp
var need = new Dictionary<char, int>();
foreach (var ch in t) need[ch] = need.GetValueOrDefault(ch, 0) + 1;
int missing = t.Length;
int l = 0, bestL = 0, bestLen = int.MaxValue;
for (int r = 0; r < s.Length; r++)
{
    char c = s[r];
    if (need.GetValueOrDefault(c, 0) > 0) missing--;
    need[c] = need.GetValueOrDefault(c, 0) - 1;
    while (missing == 0)
    {
        if (r - l + 1 < bestLen) { bestLen = r - l + 1; bestL = l; }
        char lc = s[l];
        need[lc]++;
        if (need[lc] > 0) missing++;
        l++;
    }
}
return bestLen == int.MaxValue ? "" : s.Substring(bestL, bestLen);
```

**Traps:** updating `missing` only when crossing zero (not every char); forgetting to shrink after every right-move; off-by-one on result length.

**Problems:**
- **C** [LC 3 Longest Substring Without Repeating Characters](https://leetcode.com/problems/longest-substring-without-repeating-characters/) (M)
- **C** [LC 76 Minimum Window Substring](https://leetcode.com/problems/minimum-window-substring/) (H)
- **C** [LC 424 Longest Repeating Character Replacement](https://leetcode.com/problems/longest-repeating-character-replacement/) (M)
- **C** [LC 239 Sliding Window Maximum](https://leetcode.com/problems/sliding-window-maximum/) (H) — monotonic deque.
- **C** [LC 567 Permutation in String](https://leetcode.com/problems/permutation-in-string/) (M)
- **R** [LC 992 Subarrays with K Different Integers](https://leetcode.com/problems/subarrays-with-k-different-integers/) (H) — at-most-K trick.
- **R** [LC 480 Sliding Window Median](https://leetcode.com/problems/sliding-window-median/) (H)

---

## 4. Stack & monotonic structures

**Concepts:** plain stack parsing (paired brackets, expression eval), monotonic stack (next greater / smaller), monotonic deque (sliding window max).

**Recognition cues:** "next greater element", "histogram", "remove k digits to make smallest", "valid parentheses", "asteroid collision", "stock span".

**Template (next greater element):**
```csharp
int n = arr.Length;
var res = new int[n];
Array.Fill(res, -1);
var stack = new Stack<int>();
for (int i = 0; i < n; i++)
{
    while (stack.Count > 0 && arr[stack.Peek()] < arr[i])
        res[stack.Pop()] = arr[i];
    stack.Push(i);
}
```

**Traps:** forgetting `<` vs `<=` (strict vs non-strict); not draining the stack at the end; in histogram, push a sentinel `0` at the end to flush.

**Problems:**
- **C** [LC 20 Valid Parentheses](https://leetcode.com/problems/valid-parentheses/) (E) — speed only.
- **C** [LC 739 Daily Temperatures](https://leetcode.com/problems/daily-temperatures/) (M)
- **C** [LC 84 Largest Rectangle in Histogram](https://leetcode.com/problems/largest-rectangle-in-histogram/) (H)
- **C** [LC 85 Maximal Rectangle](https://leetcode.com/problems/maximal-rectangle/) (H)
- **C** [LC 394 Decode String](https://leetcode.com/problems/decode-string/) (M)
- **R** [LC 71 Simplify Path](https://leetcode.com/problems/simplify-path/) (M)
- **R** [LC 32 Longest Valid Parentheses](https://leetcode.com/problems/longest-valid-parentheses/) (H)

---

## 5. Binary search

**Concepts:** classic search, lower_bound / upper_bound, binary search on the **answer** with a monotonic predicate, search in rotated arrays.

**Recognition cues:** "minimum X such that ...", "maximum X such that ...", sorted-or-rotated input, "find Kth", time complexity hint `O(log n)`.

**Template (binary search on answer):**
```csharp
bool Feasible(int x) { /* monotonic in x */ return true; }
int lo = lo0, hi = hi0;
while (lo < hi)
{
    int mid = lo + (hi - lo) / 2;
    if (Feasible(mid)) hi = mid;
    else lo = mid + 1;
}
return lo;
```

**Traps:** integer overflow in `(lo + hi) / 2` — always write `lo + (hi - lo) / 2`; the "feasible" predicate must be **monotonic** (verify on paper); off-by-one between `hi = mid` vs `hi = mid - 1`; rotated array with duplicates needs the linear fallback.

**Problems:**
- **C** [LC 33 Search in Rotated Sorted Array](https://leetcode.com/problems/search-in-rotated-sorted-array/) (M)
- **C** [LC 153 Find Minimum in Rotated Sorted Array](https://leetcode.com/problems/find-minimum-in-rotated-sorted-array/) (M)
- **C** [LC 875 Koko Eating Bananas](https://leetcode.com/problems/koko-eating-bananas/) (M) — canonical BSoA.
- **C** [LC 410 Split Array Largest Sum](https://leetcode.com/problems/split-array-largest-sum/) (H)
- **C** [LC 1011 Capacity To Ship Packages Within D Days](https://leetcode.com/problems/capacity-to-ship-packages-within-d-days/) (M)
- **C** [LC 4 Median of Two Sorted Arrays](https://leetcode.com/problems/median-of-two-sorted-arrays/) (H)
- **R** [LC 162 Find Peak Element](https://leetcode.com/problems/find-peak-element/) (M)
- **R** [LC 540 Single Element in a Sorted Array](https://leetcode.com/problems/single-element-in-a-sorted-array/) (M)

---

## 6. Sorting, intervals, sweep line

**Concepts:** sort + scan; merge intervals; meeting-rooms heap pattern; line sweep with events `(t, +1/-1)`; custom comparator (esp. Java/C++).

**Recognition cues:** "intervals", "meetings", "skyline", "calendar", "free time", "min number of X to cover", "Employee Free Time".

**Template (line sweep):**
```csharp
var events = new List<(int t, int delta)>();
foreach (var iv in intervals)
{
    events.Add((iv[0], +1));
    events.Add((iv[1], -1));
}
// ties: -1 before +1 if endpoints touch
events.Sort((a, b) => a.t != b.t ? a.t.CompareTo(b.t) : a.delta.CompareTo(b.delta));
int active = 0, best = 0;
foreach (var (_, d) in events)
{
    active += d;
    best = Math.Max(best, active);
}
```

**Traps:** start-vs-end tie-breaking (does a meeting ending at t conflict with one starting at t?); using `<` vs `<=` for merging; in Skyline, sorting `(x, -h, type)` to handle simultaneous starts/ends correctly.

**Problems:**
- **C** [LC 56 Merge Intervals](https://leetcode.com/problems/merge-intervals/) (M)
- **C** [LC 253 Meeting Rooms II](https://leetcode.com/problems/meeting-rooms-ii/) (M)
- **C** [LC 435 Non-overlapping Intervals](https://leetcode.com/problems/non-overlapping-intervals/) (M)
- **C** [LC 759 Employee Free Time](https://leetcode.com/problems/employee-free-time/) (H) — Databricks favorite.
- **C** [LC 218 The Skyline Problem](https://leetcode.com/problems/the-skyline-problem/) (H)
- **C** [LC 731 My Calendar II](https://leetcode.com/problems/my-calendar-ii/) (M)
- **R** [LC 732 My Calendar III](https://leetcode.com/problems/my-calendar-iii/) (H)
- **R** [LC 57 Insert Interval](https://leetcode.com/problems/insert-interval/) (M)
- **S** [LC 1235 Maximum Profit in Job Scheduling](https://leetcode.com/problems/maximum-profit-in-job-scheduling/) (H) — DP + binary search.

---

## 7. Heap / priority queue

**Concepts:** top-K; k-way merge; streaming median (two heaps); scheduling with deadlines/priority; lazy deletion (heap with "removed" set or counter).

**Recognition cues:** "K largest / smallest", "merge K sorted ...", "median of stream", "minimum cost to ...", "process tasks by ...".

**Template (k-way merge):**
```csharp
// PriorityQueue<TElement, TPriority> is a min-heap by priority.
var pq = new PriorityQueue<(int listIdx, int pos), int>();
for (int i = 0; i < lists.Count; i++)
    if (lists[i].Count > 0) pq.Enqueue((i, 0), lists[i][0]);

var outList = new List<int>();
while (pq.Count > 0)
{
    var (i, j) = pq.Dequeue();
    outList.Add(lists[i][j]);
    if (j + 1 < lists[i].Count)
        pq.Enqueue((i, j + 1), lists[i][j + 1]);
}
```

**Traps:** `PriorityQueue<T,TPriority>` is min-heap — negate the priority for a max-heap; it has no `Remove(x)` operation, so use lazy deletion or switch to `SortedSet<T>` / `SortedDictionary<TKey,TValue>` when you need ordered removal; equal priorities are **not** dequeued in insertion order — bake a tiebreaker into the priority if order matters.

**Problems:**
- **C** [LC 215 Kth Largest Element in an Array](https://leetcode.com/problems/kth-largest-element-in-an-array/) (M) — also quickselect.
- **C** [LC 347 Top K Frequent Elements](https://leetcode.com/problems/top-k-frequent-elements/) (M)
- **C** [LC 23 Merge K Sorted Lists](https://leetcode.com/problems/merge-k-sorted-lists/) (H)
- **C** [LC 295 Find Median from Data Stream](https://leetcode.com/problems/find-median-from-data-stream/) (H)
- **C** [LC 1167 Minimum Cost to Connect Sticks](https://leetcode.com/problems/minimum-cost-to-connect-sticks/) (M)
- **R** [LC 621 Task Scheduler](https://leetcode.com/problems/task-scheduler/) (M)
- **R** [LC 692 Top K Frequent Words](https://leetcode.com/problems/top-k-frequent-words/) (M)
- **R** [LC 658 Find K Closest Elements](https://leetcode.com/problems/find-k-closest-elements/) (M)
- **S** [LC 1882 Process Tasks Using Servers](https://leetcode.com/problems/process-tasks-using-servers/) (M) — two-heap scheduling.

---

## 8. Linked lists

**Concepts:** reversal in place, dummy node, fast/slow, merge, split at midpoint, cycle detection.

**Recognition cues:** explicit `ListNode`; "in-place"; "without extra space".

**Template (reverse):**
```csharp
ListNode prev = null, cur = head;
while (cur != null)
{
    var nxt = cur.next;
    cur.next = prev;
    prev = cur;
    cur = nxt;
}
return prev;
```

**Traps:** losing `head.next` before saving it; creating `var dummy = new ListNode(0)` and forgetting to return `dummy.next`; in cycle problems, not handling `head == null`.

**Problems:**
- **C** [LC 206 Reverse Linked List](https://leetcode.com/problems/reverse-linked-list/) (E)
- **C** [LC 21 Merge Two Sorted Lists](https://leetcode.com/problems/merge-two-sorted-lists/) (E)
- **C** [LC 143 Reorder List](https://leetcode.com/problems/reorder-list/) (M)
- **C** [LC 25 Reverse Nodes in k-Group](https://leetcode.com/problems/reverse-nodes-in-k-group/) (H)
- **C** [LC 138 Copy List with Random Pointer](https://leetcode.com/problems/copy-list-with-random-pointer/) (M)
- **R** [LC 142 Linked List Cycle II](https://leetcode.com/problems/linked-list-cycle-ii/) (M)
- **R** [LC 19 Remove Nth Node From End](https://leetcode.com/problems/remove-nth-node-from-end-of-list/) (M)

---

## 9. Trees & BSTs

**Concepts:** DFS recursion (pre/in/post), BFS by level, divide-and-conquer return-tuple pattern, path problems, LCA, serialization, BST ordered traversal, iterative traversal (stack).

**Recognition cues:** "binary tree", "BST", "path", "ancestor", "level", "serialize", "validate".

**Template (return-tuple DFS):**
```csharp
(int depth, int best) Dfs(TreeNode node)
{
    if (node == null) return (0, 0);
    var L = Dfs(node.left);
    var R = Dfs(node.right);
    // combine
    return (/* depth */ 0, /* best */ 0);
}
```

**Traps:** confusing in-order vs pre-order for BST validation (in-order must be strictly increasing); deep recursion blows the default ~1 MB thread stack on Windows — for very deep trees, run on a dedicated `new Thread(..., maxStackSize: 16 << 20)` or convert to an explicit stack; LCA assumptions about both nodes existing.

**Problems:**
- **C** [LC 104 Max Depth of Binary Tree](https://leetcode.com/problems/maximum-depth-of-binary-tree/) (E)
- **C** [LC 102 Binary Tree Level Order Traversal](https://leetcode.com/problems/binary-tree-level-order-traversal/) (M)
- **C** [LC 98 Validate BST](https://leetcode.com/problems/validate-binary-search-tree/) (M)
- **C** [LC 236 LCA of Binary Tree](https://leetcode.com/problems/lowest-common-ancestor-of-a-binary-tree/) (M)
- **C** [LC 124 Binary Tree Maximum Path Sum](https://leetcode.com/problems/binary-tree-maximum-path-sum/) (H)
- **C** [LC 297 Serialize and Deserialize Binary Tree](https://leetcode.com/problems/serialize-and-deserialize-binary-tree/) (H)
- **C** [LC 428 Serialize and Deserialize N-ary Tree](https://leetcode.com/problems/serialize-and-deserialize-n-ary-tree/) (H) — explicit Databricks ask.
- **C** [LC 199 Binary Tree Right Side View](https://leetcode.com/problems/binary-tree-right-side-view/) (M)
- **R** [LC 173 BST Iterator](https://leetcode.com/problems/binary-search-tree-iterator/) (M) — links to design.
- **R** [LC 230 Kth Smallest in BST](https://leetcode.com/problems/kth-smallest-element-in-a-bst/) (M)
- **S** [LC 968 Binary Tree Cameras](https://leetcode.com/problems/binary-tree-cameras/) (H) — only if you finish early.

---

## 10. Tries

**Concepts:** node = `{children: dict, end: bool}`; prefix search; dictionary with wildcard; word search on board (trie-pruned DFS).

**Recognition cues:** "prefix", "dictionary", "autocomplete", "find all words on board".

**Template:**
```csharp
public class Trie
{
    private class Node
    {
        public Dictionary<char, Node> Children = new();
        public bool End;
    }
    private readonly Node root = new();

    public void Insert(string w)
    {
        var n = root;
        foreach (var c in w)
        {
            if (!n.Children.TryGetValue(c, out var nxt))
                n.Children[c] = nxt = new Node();
            n = nxt;
        }
        n.End = true;
    }

    public bool Search(string w)
    {
        var n = root;
        foreach (var c in w)
        {
            if (!n.Children.TryGetValue(c, out n!)) return false;
        }
        return n.End;
    }
}
```

**Traps:** confusing prefix-exists vs word-exists; not pruning leaf nodes after DFS pop (memory); using a fixed `Node[26]` array can be 5–10× faster than `Dictionary<char, Node>` for lowercase-only inputs — know both.

**Problems:**
- **C** [LC 208 Implement Trie](https://leetcode.com/problems/implement-trie-prefix-tree/) (M)
- **C** [LC 211 Design Add and Search Word](https://leetcode.com/problems/design-add-and-search-words-data-structure/) (M)
- **C** [LC 212 Word Search II](https://leetcode.com/problems/word-search-ii/) (H)
- **R** [LC 642 Design Search Autocomplete System](https://leetcode.com/problems/design-search-autocomplete-system/) (H) — Databricks signal.
- **R** [LC 1268 Search Suggestions System](https://leetcode.com/problems/search-suggestions-system/) (M)

---

## 11. Graphs

**Concepts:** BFS/DFS on adjacency list and grid; connected components; topological sort (Kahn / DFS); cycle detection (directed / undirected); bipartite check (2-coloring); union-find (path compression + union by rank); shortest path (BFS for unweighted, Dijkstra for nonneg weights, 0-1 BFS, occasionally Bellman-Ford); BFS with state bitmask for Hamiltonian-like shortest paths.

**Recognition cues:** graph or grid; "shortest path", "course schedule", "alien dictionary", "redundant edge", "valid tree", "minimum cost path", "visit all nodes".

**Templates:**

Dijkstra:
```csharp
var dist = new Dictionary<int, int> { [s] = 0 };
var pq = new PriorityQueue<int, int>();
pq.Enqueue(s, 0);
while (pq.TryDequeue(out int u, out int d))
{
    if (d > dist[u]) continue; // stale entry
    foreach (var (v, w) in adj[u])
    {
        int nd = d + w;
        if (nd < dist.GetValueOrDefault(v, int.MaxValue))
        {
            dist[v] = nd;
            pq.Enqueue(v, nd);
        }
    }
}
```

Union-Find:
```csharp
int[] parent = Enumerable.Range(0, n).ToArray();
int[] rank = new int[n];

int Find(int x)
{
    while (parent[x] != x)
    {
        parent[x] = parent[parent[x]]; // path compression (halving)
        x = parent[x];
    }
    return x;
}

bool Union(int x, int y)
{
    int rx = Find(x), ry = Find(y);
    if (rx == ry) return false;
    if (rank[rx] < rank[ry]) (rx, ry) = (ry, rx);
    parent[ry] = rx;
    if (rank[rx] == rank[ry]) rank[rx]++;
    return true;
}
```

**Traps:** forgetting visited marking in BFS (TLE / infinite loop); deep DFS recursion on big grids blows the thread stack — switch to an explicit `Stack<T>` or a larger-stack `Thread`; Dijkstra with negative weights (broken — use Bellman-Ford or rethink); topological sort failing silently when the graph has a cycle (check final count == n); union-find without path compression (TLE); `PriorityQueue` has no decrease-key, so push duplicates and skip stale entries via the `d > dist[u]` check above.

**Problems:**
- **C** [LC 200 Number of Islands](https://leetcode.com/problems/number-of-islands/) (M)
- **C** [LC 207 Course Schedule](https://leetcode.com/problems/course-schedule/) (M)
- **C** [LC 210 Course Schedule II](https://leetcode.com/problems/course-schedule-ii/) (M)
- **C** [LC 994 Rotting Oranges](https://leetcode.com/problems/rotting-oranges/) (M)
- **C** [LC 133 Clone Graph](https://leetcode.com/problems/clone-graph/) (M)
- **C** [LC 261 Graph Valid Tree](https://leetcode.com/problems/graph-valid-tree/) (M)
- **C** [LC 547 Number of Provinces](https://leetcode.com/problems/number-of-provinces/) (M) — Union-Find.
- **C** [LC 743 Network Delay Time](https://leetcode.com/problems/network-delay-time/) (M) — Dijkstra.
- **C** [LC 815 Bus Routes](https://leetcode.com/problems/bus-routes/) (H) — BFS w/ stop-set trick.
- **C** [LC 269 Alien Dictionary](https://leetcode.com/problems/alien-dictionary/) (H)
- **C** [LC 332 Reconstruct Itinerary](https://leetcode.com/problems/reconstruct-itinerary/) (H) — Hierholzer.
- **C** [LC 847 Shortest Path Visiting All Nodes](https://leetcode.com/problems/shortest-path-visiting-all-nodes/) (H) — BFS + bitmask, Databricks signal.
- **R** [LC 305 Number of Islands II](https://leetcode.com/problems/number-of-islands-ii/) (H) — Union-Find with rank.
- **R** [LC 787 Cheapest Flights Within K Stops](https://leetcode.com/problems/cheapest-flights-within-k-stops/) (M) — Bellman-Ford or modified Dijkstra.
- **R** [LC 1631 Path With Minimum Effort](https://leetcode.com/problems/path-with-minimum-effort/) (M) — Dijkstra on grid.
- **S** [LC 1192 Critical Connections in a Network](https://leetcode.com/problems/critical-connections-in-a-network/) (H) — Tarjan bridges.

---

## 12. Backtracking

**Concepts:** subsets, permutations, combinations, constrained search with pruning, board search.

**Recognition cues:** "all possible ...", "generate all ...", "N-Queens", "Sudoku", word ladder DFS variant.

**Template (subsets):**
```csharp
Array.Sort(nums);
var res = new List<IList<int>>();
var path = new List<int>();

void Bt(int start)
{
    res.Add(new List<int>(path)); // copy!
    for (int i = start; i < nums.Length; i++)
    {
        if (i > start && nums[i] == nums[i - 1]) continue; // dedup if sorted
        path.Add(nums[i]);
        Bt(i + 1);
        path.RemoveAt(path.Count - 1);
    }
}
Bt(0);
```

**Traps:** forgetting to copy `path` when appending (you'll get N references to the same mutating list); dedup logic only works on sorted input; mutating the result inadvertently.

**Problems:**
- **C** [LC 78 Subsets](https://leetcode.com/problems/subsets/) (M)
- **C** [LC 46 Permutations](https://leetcode.com/problems/permutations/) (M)
- **C** [LC 39 Combination Sum](https://leetcode.com/problems/combination-sum/) (M)
- **C** [LC 79 Word Search](https://leetcode.com/problems/word-search/) (M)
- **C** [LC 51 N-Queens](https://leetcode.com/problems/n-queens/) (H)
- **R** [LC 131 Palindrome Partitioning](https://leetcode.com/problems/palindrome-partitioning/) (M)
- **R** [LC 37 Sudoku Solver](https://leetcode.com/problems/sudoku-solver/) (H)
- **R** [LC 17 Letter Combinations of a Phone Number](https://leetcode.com/problems/letter-combinations-of-a-phone-number/) (M)

---

## 13. Dynamic programming

**Concepts:** 1D DP (Fibonacci-shape, house robber); 2D DP (grid paths, edit distance); knapsack (0/1 and unbounded); LIS (DP O(n^2) and patience-sort O(n log n)); interval DP (matrix chain, burst balloons); string DP (LCS, edit distance, regex, palindrome partitioning); memoization (top-down) vs tabulation (bottom-up).

**Recognition cues:** "min / max ways", "number of ways", "longest", asks for an optimum over many overlapping choices, constraints are small (n ≤ 1000 for O(n²) or n ≤ 100 for O(n³)).

**Template (top-down memo):**
```csharp
var memo = new Dictionary<(int i, int state), int>();

int Dp(int i, int state)
{
    if (/* base */ false) return 0;
    if (memo.TryGetValue((i, state), out var cached)) return cached;
    int best = Math.Min(Dp(i + 1, /* ... */ state), Dp(i + 1, /* ... */ state));
    memo[(i, state)] = best;
    return best;
}
```

**Traps:** using reference-type keys (arrays, lists) in the memo dictionary — they hash by identity; storing 2D state when a 1D rolling array suffices; off-by-one between length and index; forgetting to reset the memo between test cases when it captures outer state.

**Problems:**
- **C** [LC 198 House Robber](https://leetcode.com/problems/house-robber/) (M)
- **C** [LC 322 Coin Change](https://leetcode.com/problems/coin-change/) (M)
- **C** [LC 300 Longest Increasing Subsequence](https://leetcode.com/problems/longest-increasing-subsequence/) (M) — both O(n²) and patience O(n log n).
- **C** [LC 416 Partition Equal Subset Sum](https://leetcode.com/problems/partition-equal-subset-sum/) (M) — 0/1 knapsack.
- **C** [LC 1143 Longest Common Subsequence](https://leetcode.com/problems/longest-common-subsequence/) (M)
- **C** [LC 72 Edit Distance](https://leetcode.com/problems/edit-distance/) (M)
- **C** [LC 5 Longest Palindromic Substring](https://leetcode.com/problems/longest-palindromic-substring/) (M)
- **C** [LC 64 Minimum Path Sum](https://leetcode.com/problems/minimum-path-sum/) (M)
- **C** [LC 10 Regular Expression Matching](https://leetcode.com/problems/regular-expression-matching/) (H)
- **C** [LC 312 Burst Balloons](https://leetcode.com/problems/burst-balloons/) (H) — interval DP.
- **R** [LC 354 Russian Doll Envelopes](https://leetcode.com/problems/russian-doll-envelopes/) (H)
- **R** [LC 213 House Robber II](https://leetcode.com/problems/house-robber-ii/) (M)
- **R** [LC 416 Partition Equal Subset Sum](https://leetcode.com/problems/partition-equal-subset-sum/) — already listed.
- **R** [LC 91 Decode Ways](https://leetcode.com/problems/decode-ways/) (M)
- **S** [LC 887 Super Egg Drop](https://leetcode.com/problems/super-egg-drop/) (H) — only if comfortable.

---

## 14. Greedy

**Concepts:** sort + scan; exchange argument; interval greedy; heap-assisted greedy.

**Recognition cues:** "minimum number of ...", optimal local choice obviously leads to global optimum, asks for a *feasibility* with greedy schedule.

**Template (interval greedy "minimum X"):**
```csharp
Array.Sort(intervals, (a, b) => a[1].CompareTo(b[1])); // by end
int count = 0, end = int.MinValue;
foreach (var iv in intervals)
{
    if (iv[0] >= end)
    {
        count++;
        end = iv[1];
    }
}
```

**Traps:** assuming greedy works without justifying via exchange argument; sorting by the wrong field (start vs end vs length); breaking ties incorrectly.

**Problems:**
- **C** [LC 55 Jump Game](https://leetcode.com/problems/jump-game/) (M)
- **C** [LC 45 Jump Game II](https://leetcode.com/problems/jump-game-ii/) (M)
- **C** [LC 134 Gas Station](https://leetcode.com/problems/gas-station/) (M)
- **C** [LC 763 Partition Labels](https://leetcode.com/problems/partition-labels/) (M)
- **R** [LC 406 Queue Reconstruction by Height](https://leetcode.com/problems/queue-reconstruction-by-height/) (M)
- **R** [LC 1167 Minimum Cost to Connect Sticks](https://leetcode.com/problems/minimum-cost-to-connect-sticks/) — heap greedy.

---

## 15. Bit manipulation

**Concepts:** XOR identities (`a^a = 0`, `a^0 = a`), masking, subset enumeration `for sub = mask; sub; sub = (sub-1) & mask`, popcount, bit DP, IP packing.

**Recognition cues:** "single number", "subsets via bitmask", "find missing", "max XOR", numbers within 32-bit constraint.

**Template (subset of mask enumeration):**
```csharp
int sub = mask;
while (sub != 0)
{
    Process(sub);
    sub = (sub - 1) & mask;
}
Process(0);
```

**Traps:** precedence of `&` is lower than `==` in C# — always parenthesize `(x & mask) == 0`; use `uint`/`ulong` for unsigned shifts, or `>>>` (C# 11+) for logical right shift on signed types; `BitOperations.PopCount` / `TrailingZeroCount` from `System.Numerics` is your friend.

**Problems:**
- **C** [LC 136 Single Number](https://leetcode.com/problems/single-number/) (E)
- **C** [LC 191 Number of 1 Bits](https://leetcode.com/problems/number-of-1-bits/) (E)
- **C** [LC 338 Counting Bits](https://leetcode.com/problems/counting-bits/) (E)
- **R** [LC 137 Single Number II](https://leetcode.com/problems/single-number-ii/) (M)
- **R** [LC 421 Maximum XOR of Two Numbers in an Array](https://leetcode.com/problems/maximum-xor-of-two-numbers-in-an-array/) (M) — trie + bits.

---

## 16. Math / parsing / numbers

**Concepts:** modular arithmetic, gcd/lcm, integer parsing with edge cases, expression evaluation, base conversion.

**Recognition cues:** "implement atoi", "evaluate expression", "decode ways" (overlaps with DP), "next greater number with same digits".

**Templates (calculator with stack):**
```csharp
int Calculate(string s)
{
    s += "+"; // sentinel
    var stack = new Stack<int>();
    int num = 0;
    char op = '+';
    foreach (var c in s)
    {
        if (char.IsDigit(c)) { num = num * 10 + (c - '0'); }
        else if (c == '+' || c == '-' || c == '*' || c == '/')
        {
            switch (op)
            {
                case '+': stack.Push(num); break;
                case '-': stack.Push(-num); break;
                case '*': stack.Push(stack.Pop() * num); break;
                case '/': stack.Push(stack.Pop() / num); break; // truncates toward 0
            }
            op = c; num = 0;
        }
    }
    return stack.Sum();
}
```

**Traps:** C# integer `/` truncates toward 0 (good — matches LC's expected behavior); watch for `int` overflow on intermediate products — promote to `long` when in doubt; whitespace and signs in atoi; nested parens in Calculator II → III; `checked { }` blocks help catch overflow during testing.

**Problems:**
- **C** [LC 8 String to Integer (atoi)](https://leetcode.com/problems/string-to-integer-atoi/) (M)
- **C** [LC 227 Basic Calculator II](https://leetcode.com/problems/basic-calculator-ii/) (M) — Databricks signal.
- **C** [LC 224 Basic Calculator](https://leetcode.com/problems/basic-calculator/) (H)
- **C** [LC 772 Basic Calculator III](https://leetcode.com/problems/basic-calculator-iii/) (H)
- **R** [LC 736 Parse Lisp Expression](https://leetcode.com/problems/parse-lisp-expression/) (H)
- **R** [LC 50 Pow(x, n)](https://leetcode.com/problems/powx-n/) (M)
- **R** [LC 415 Add Strings](https://leetcode.com/problems/add-strings/) (E)

---

## 17. Design-style coding (Databricks favorite)

**Concepts:** combining data structures to meet a contract: hashmap+linked list (LRU), buckets+linked list (LFU), versioned arrays (snapshot), trie+heap (autocomplete), tree of paths (file system), interval tree-of-arrays (booking), two heaps (median).

**Recognition cues:** problem statement is a class definition with multiple methods and amortized complexity targets. Often phrased as "Design X".

**Templates:** see the dedicated solutions when you do each problem. Build a reusable LRU template and a reusable bucket-pattern template by week 4.

**Traps:** failing to hit the stated time complexity ("each call O(1)" is contract, not guideline); thread safety asked as a follow-up; treating the design problem as a one-shot algo problem.

**Problems:**
- **C** [LC 146 LRU Cache](https://leetcode.com/problems/lru-cache/) (M)
- **C** [LC 460 LFU Cache](https://leetcode.com/problems/lfu-cache/) (H)
- **C** [LC 295 Find Median from Data Stream](https://leetcode.com/problems/find-median-from-data-stream/) (H)
- **C** [LC 1146 Snapshot Array](https://leetcode.com/problems/snapshot-array/) (M) — Databricks signal.
- **C** [LC 380 Insert Delete GetRandom O(1)](https://leetcode.com/problems/insert-delete-getrandom-o1/) (M)
- **C** [LC 381 Insert Delete GetRandom O(1) - Duplicates allowed](https://leetcode.com/problems/insert-delete-getrandom-o1-duplicates-allowed/) (H)
- **C** [LC 588 Design In-Memory File System](https://leetcode.com/problems/design-in-memory-file-system/) (H) — Databricks signal.
- **C** [LC 1166 Design File System](https://leetcode.com/problems/design-file-system/) (M)
- **C** [LC 981 Time Based Key-Value Store](https://leetcode.com/problems/time-based-key-value-store/) (M)
- **C** [LC 642 Design Search Autocomplete System](https://leetcode.com/problems/design-search-autocomplete-system/) (H)
- **C** [LC 359 Logger Rate Limiter](https://leetcode.com/problems/logger-rate-limiter/) (E) — quick win.
- **C** [LC 1396 Design Underground System](https://leetcode.com/problems/design-underground-system/) (M)
- **R** [LC 432 All O(1) Data Structure](https://leetcode.com/problems/all-oone-data-structure/) (H)
- **R** [LC 528 Random Pick with Weight](https://leetcode.com/problems/random-pick-with-weight/) (M) — prefix sum + binary search.
- **R** [LC 348 Design Tic-Tac-Toe](https://leetcode.com/problems/design-tic-tac-toe/) (M)
- **S** [LC 1622 Fancy Sequence](https://leetcode.com/problems/fancy-sequence/) (H) — only if you've crushed everything.

---

## 18. Concurrency (thin layer)

**Only invest here if** (a) you have free hours, or (b) your recruiter signaled Platform / Compute / Infra team. Otherwise: do 2 problems and move on.

**Concepts (C#):** `lock` statement (syntactic sugar over `Monitor.Enter/Exit`), `Monitor.Wait/Pulse/PulseAll`, `SemaphoreSlim` (prefer over `Semaphore` for in-process), `ManualResetEventSlim` / `AutoResetEvent`, `CountdownEvent`, `Barrier`, `BlockingCollection<T>` (the BCL's bounded blocking queue), `Channel<T>` from `System.Threading.Channels` (modern producer-consumer), `Interlocked` (atomic ops), `volatile` keyword and `Volatile.Read/Write`, `CancellationToken`. For async-style problems: `Task`, `async`/`await`, `TaskCompletionSource<T>`.
**Concepts (Java, for reference if the interviewer asks):** `synchronized`, `ReentrantLock`, `Semaphore`, `Condition`, `BlockingQueue`, `CountDownLatch`, `CyclicBarrier`, `volatile` semantics, `AtomicInteger`.

**Templates:** memorize a single producer-consumer using `BlockingCollection<T>` (or `Channel<T>`) and a single rate limiter using a token bucket backed by `SemaphoreSlim` + a refill timer. That covers ~80% of likely asks.

**Traps:** spurious wakeups (always `while (!cond) Monitor.Wait(...)`, never `if`); deadlock by lock ordering; locking on `this` or on a public object while exposing the reference to outside code \u2014 always `lock` on a `private readonly object _gate = new();`; releasing in `finally` (or just use `lock` / `using`); forgetting that `SemaphoreSlim.WaitAsync` accepts a `CancellationToken` \u2014 use it; `Task.Result` / `.Wait()` deadlocks in UI contexts \u2014 stay `async` end to end.

**Problems:**
- **C** [LC 1114 Print in Order](https://leetcode.com/problems/print-in-order/) (E)
- **C** [LC 1115 Print FooBar Alternately](https://leetcode.com/problems/print-foobar-alternately/) (M)
- **R** [LC 1188 Design Bounded Blocking Queue](https://leetcode.com/problems/design-bounded-blocking-queue/) (M)
- **R** [LC 1117 Building H2O](https://leetcode.com/problems/building-h2o/) (M)
- **R** [LC 1226 The Dining Philosophers](https://leetcode.com/problems/the-dining-philosophers/) (M)
- **S** Implement a token-bucket rate limiter from scratch (no LC link; write it yourself).
