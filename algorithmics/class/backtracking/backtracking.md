# Class: Backtracking

> Canonical class for the `backtracking` topic — the **tool library**. Living document; fold new templates/pitfalls back in as problems reveal them.
> Tools here are taught on **neutral examples** (binary strings, target-sum combinations, orderings of distinct items, spelling a target in a grid, graph m-coloring). The mapping to specific problem-set #s is at the bottom — **don't read that mapping for a problem you haven't yet attempted; it's the reduction.**

---

## 1. Context & when to reach for it

**Backtracking = DFS over the tree of partial solutions, undoing each choice on the way back up.** You build a candidate incrementally; at each step you try every legal next choice, recurse, then **un-make** the choice before trying the next one. It is exhaustive search made tractable by **pruning** branches that can't lead to a valid/optimal answer.

It is the right tool when the answer is a **combination of choices** and you must enumerate or find one that satisfies constraints — and there's **no closed-form or greedy shortcut**.

**Signals you're in backtracking territory:**
- "Find **all** …" — all subsets, all permutations, all combinations, all valid placements/partitions.
- "Generate every …" / "how many ways …" where you must construct each candidate.
- Constraint satisfaction: place N queens, color a graph, fill a Sudoku, partition a string so every piece is a palindrome.
- A search over a grid/board where a path must spell something or satisfy a rule.
- The candidate is built **one element at a time** and a partial candidate can be **rejected early**.

> Rule of thumb: if the output is "all/every valid configuration" and each configuration is assembled from a sequence of choices, it's backtracking. If you only need a count or an optimum *and* subproblems overlap, it's probably **DP** instead (backtracking explores the whole tree; DP memoizes it).

---

## 2. Mental model

Picture a **decision tree**. The root is the empty candidate. Each edge is "make one choice." Each leaf (or each node, for subsets) is a complete candidate. Backtracking is a **DFS of this tree** with one discipline:

```
choose  →  explore (recurse)  →  un-choose
```

The un-choose ("backtrack") is what makes the single mutable `path` buffer correct: after a child subtree is fully explored, you restore state so the next sibling starts clean. Without the undo, choices leak across branches.

Three knobs define any backtracking problem:
1. **Choices** — at this state, what are the legal next moves? (the loop body)
2. **Constraints / pruning** — which choices are illegal or hopeless? skip them early.
3. **Goal / base case** — when is `path` a complete answer? record it (usually a **copy**).

The art is almost entirely in (2): *the earlier and cheaper you prune, the smaller the tree.* A prefix trie, or maintained occupancy sets, are both pruning machines.

---

## 3. Core patterns

### A. Include / exclude per element
Each element is a binary choice: in or out. Depending on where you record, every node of the tree is a valid partial answer (record at **every** node) or only the depth-`n` leaves count. A `start` index prevents revisiting earlier elements. (Archetype: enumerating fixed-length sequences or a power set.)

### B. Combinations with a cursor (and sum-pruned variants)
Pick elements with a `start` cursor so combinations aren't re-emitted in different orders (`{2,3}` not also `{3,2}`). Reuse allowed → recurse with the **same** index; no reuse → `i+1`. When chasing a target sum, prune the instant the running total overshoots (sort first to `break` early).

### C. Permutations (order matters, use-each-once)
No `start` cursor — every unused element is a candidate at every position. Track used elements (a `used[]` bool array or swap-in-place). Record only at **leaves** (length == n).

### D. Grid / path backtracking
DFS from cells across the 4 neighbors; mark a cell visited before recursing, unmark after. The candidate is the path through the grid. A **trie**-driven variant prunes dead prefixes instantly when matching *many* target words in one scan (see the tries class, pattern C) — the payoff that fuses both topics.

### E. Constraint placement
Assign one unit per "slot", maintain just enough committed state (sets/maps of what's already used) for **fast legality checks**, recurse to the next slot, undo on return. Pure pruning-driven search. (Neutral archetype: graph m-coloring.)

### F. Partitioning
Choose where to cut next; each prefix piece must satisfy a predicate. `start` marks the cut position; recurse on the remainder.

### G. Mapping enumeration
A multi-way choice per position (e.g. the letters mapped to each phone digit). Depth = number of positions; branch = options at this position.

---

## 4. Reusable templates (C#)

### Universal skeleton
```csharp
void Backtrack(State path /* + cursor */) {
    if (IsComplete(path)) { results.Add(Copy(path)); return; }   // goal
    foreach (var choice in ChoicesFrom(path)) {                  // choices
        if (!IsLegal(choice)) continue;                          // prune
        Make(path, choice);                                      // choose
        Backtrack(path /* advanced */);                          // explore
        Undo(path, choice);                                      // un-choose
    }
}
```

**Picture it.** Every backtracking problem is a walk down a **decision tree**. Going *down* a branch = make a choice; hitting the bottom = record an answer; coming back *up* = undo that choice so the next sibling branch starts clean:

```
                       (   )  empty path
             choose A /      \ choose B
                   (A)        (B)
          choose B /  \        \ choose C
               (A,B)  (A,C)    (B,C)
                 |
            record, then UNDO back up to try the next sibling

the four code lines map onto one edge of this tree:
    Make   = step DOWN into a child
    Backtrack = explore that child's whole subtree
    Undo   = step back UP, erasing the choice
    continue = slide to the next sibling
```

**What it does:** this is the shape *every* template below specializes. `IsComplete` checks if `path` is a finished answer (record a **copy** — see pitfalls). The `foreach` enumerates the legal next moves; `IsLegal`/`continue` prunes hopeless ones early. The trio **choose → explore → un-choose** is the heartbeat: because we reuse one shared `path` buffer, the `Undo` after recursion is what stops a choice from leaking into the sibling branches. Master this rhythm and the specific problems are just different fillings for `Choices`, `IsLegal`, and `IsComplete`.

### A. Include / exclude per element — *neutral example: all binary strings of length n*
At each position decide 0 or 1; the tree has depth `n` and `2ⁿ` leaves.
```csharp
IList<string> BinaryStrings(int n) {
    var res = new List<string>();
    var sb = new StringBuilder();
    void Dfs(int i) {
        if (i == n) { res.Add(sb.ToString()); return; }   // leaf = one full string
        foreach (char bit in "01") {
            sb.Append(bit);            // choose
            Dfs(i + 1);                // explore
            sb.Length--;               // un-choose
        }
    }
    Dfs(0);
    return res;
}
```

**Picture it.** Every position is an independent yes/no; the two branches at each level are "append 0" vs "append 1." A complete string is a leaf at depth `n`:

```
n = 3                       ( )
              0 /                      \ 1
             (0)                       (1)
        0 /      \ 1               0 /      \ 1
      (00)       (01)           (10)        (11)
     0/  \1     0/  \1         0/  \1       0/  \1
   000  001   010  011       100  101     110  111   <- 2^3 = 8 leaves
```

**What it does:** enumerates all `2ⁿ` binary strings — the canonical include/exclude tree. `sb.Append` / `sb.Length--` is the choose/un-choose around one shared buffer. **Record placement is the knob that retargets this template:** record only at the **leaf** (here) and you enumerate fixed-length sequences; move the `res.Add` to the *top* of `Dfs` and add a `start` cursor advancing `i+1` (so earlier items aren't revisited) and the same tree enumerates a **power set** — every partial path is itself a valid answer. Permutations (pattern C) flip a different knob: drop the cursor, add a `used[]` guard.

### B. Combinations with a cursor — *neutral example: target-sum combinations, reuse allowed*
```csharp
IList<IList<int>> CombinationSum(int[] cand, int target) {
    Array.Sort(cand);
    var res = new List<IList<int>>();
    var path = new List<int>();
    void Dfs(int start, int remain) {
        if (remain == 0) { res.Add(new List<int>(path)); return; }
        for (int i = start; i < cand.Length; i++) {
            if (cand[i] > remain) break;         // prune (sorted): no point continuing
            path.Add(cand[i]);
            Dfs(i, remain - cand[i]);            // same i → reuse allowed
            path.RemoveAt(path.Count - 1);
        }
    }
    Dfs(0, target);
    return res;
}
```

**Picture it.** We chase a target sum by subtracting candidates. Passing `i` (not `i+1`) into the recursion lets us **reuse** the same number; the `start` cursor still forbids going *backward*, so we never produce the same combo in a different order. Sorting lets us `break` the instant a candidate overshoots:

```
cand = [2,3,6,7]  sorted, target = 7      (remain shown at each node)

                         remain 7
          -2 /        -3 |        -6 \      -7 \
        rem 5        rem 4       rem 1      rem 0  -> [7] ✓
     -2 / -3 \        -3 |        (2>1 break)
   rem 3    rem 2    rem 1
   -2 |     (done)   (3>1 break)
  rem 1
 (2>1 break)   ... the path 2,2,3 reaches rem 0 -> [2,2,3] ✓

results: [2,2,3], [7]
```

**What it does:** finds every combination (reuse allowed) that sums to `target`. `remain` tracks how much is left; reaching exactly 0 records the path. Recursing with the **same** `i` is what permits picking a number multiple times (`[2,2,3]`); use `i + 1` instead and you get the no-reuse variant. Because `cand` is sorted, `if (cand[i] > remain) break` abandons the rest of the loop at once — every later candidate is even bigger, so they'd all overshoot too. The `start` cursor keeps combinations canonical (ascending), so `{2,3}` is never re-emitted as `{3,2}`.

### C. Permutations — *neutral example: all orderings of n distinct items*
```csharp
IList<IList<int>> Permute(int[] nums) {
    var res = new List<IList<int>>();
    var path = new List<int>();
    var used = new bool[nums.Length];
    void Dfs() {
        if (path.Count == nums.Length) { res.Add(new List<int>(path)); return; }
        for (int i = 0; i < nums.Length; i++) {
            if (used[i]) continue;               // skip used (no start cursor)
            used[i] = true; path.Add(nums[i]);
            Dfs();
            path.RemoveAt(path.Count - 1); used[i] = false;
        }
    }
    Dfs();
    return res;
}
```

**Picture it.** Permutations care about **order**, so there's no `start` cursor — at every position any *unused* element is fair game. A `used[]` array marks which numbers are already in the current path. We only record at the **leaves** (when the path is full-length):

```
nums = [1,2,3]      used[] tracks what's taken

                          ( )
          1 /          2 |           3 \
          (1)          (2)           (3)
       2 / 3 \       1 / 3 \       1 / 2 \
    (1,2) (1,3)   (2,1) (2,3)   (3,1) (3,2)
      |     |       |     |       |     |
   (1,2,3)(1,3,2)(2,1,3)(2,3,1)(3,1,2)(3,2,1)   <- 6 leaves = 3! perms

at (1,2): used=[T,T,F] -> only 3 is free -> go to (1,2,3), record, undo
```

**What it does:** generates all n! orderings. The `if (used[i]) continue` skips numbers already placed on the current path, so each appears exactly once per permutation; the loop scans *all* indices every level (not from a cursor) because order matters — `[1,2]` and `[2,1]` are both wanted. We record only when `path.Count == nums.Length` (a leaf = a complete ordering). The undo step restores **both** `path` and `used[i]` so the next sibling sees a clean slate. (An alternative avoids `used[]` by swapping elements in place.)

### D. Grid / path backtracking — *neutral example: does a grid contain a path spelling a target?*
Walk neighbor-to-neighbor; a cell may be used once per path. Mark it, recurse the 4 directions, restore it.
```csharp
bool PathSpells(char[][] grid, string target) {
    int R = grid.Length, C = grid[0].Length;
    bool Dfs(int r, int c, int k) {
        if (k == target.Length) return true;
        if (r < 0 || r >= R || c < 0 || c >= C || grid[r][c] != target[k]) return false;
        char tmp = grid[r][c];
        grid[r][c] = '#';                        // mark visited (in place)
        bool found = Dfs(r+1,c,k+1) || Dfs(r-1,c,k+1)
                  || Dfs(r,c+1,k+1) || Dfs(r,c-1,k+1);
        grid[r][c] = tmp;                        // un-mark (backtrack)
        return found;
    }
    for (int r = 0; r < R; r++)
        for (int c = 0; c < C; c++)
            if (Dfs(r, c, 0)) return true;
    return false;
}
```

**Picture it.** Trace the target by stepping between neighbours. Match the next char and fan out to the 4 neighbours, or fail. Marking the current cell `'#'` stops the path reusing it; restoring it on the way back frees it for *other* paths:

```
grid:  C O D X        target = "CODE"
       X X E X
       X X X X

start (0,0)='C' = target[0]   -> mark '#', look for 'O'
   (0,1)='O' = target[1]      -> mark '#', look for 'D'
      (0,2)='D' = target[2]   -> mark '#', look for 'E'
         (1,2)='E' = target[3] -> k==len -> TRUE

the '#' trail prevents stepping back onto C/O/D mid-path;
a failed branch restores each cell to its letter (un-choose).
```

**What it does:** decides whether `target` can be traced through adjacent cells with no cell reused. The base cases come **first and in order**: `k == target.Length` means every char matched (success); the bounds/visited/mismatch check then rejects off-grid cells, the `'#'` sentinel (already on this path), and wrong letters — this order matters so we never index `grid[r][c]` out of range. On a match we blank the cell to `'#'`, recurse all four directions (the `||` short-circuits on the first success), then **restore** the original char so a different path may legitimately use that cell. The mark/restore *is* the choose/un-choose, done in place on the grid instead of a separate visited set.

### D′. Trie-pruned grid backtracking — *neutral example: matching many dictionary words in one grid scan*
```csharp
public class Node {
    public Node[] kids = new Node[26];
    public string word;                          // full word stored on terminal node
}

public IList<string> FindWords(char[][] board, string[] words) {
    // build trie
    var root = new Node();
    foreach (var w in words) {
        var n = root;
        foreach (var ch in w) { int i = ch - 'a'; n.kids[i] ??= new Node(); n = n.kids[i]; }
        n.word = w;
    }

    int R = board.Length, C = board[0].Length;
    var res = new List<string>();

    void Dfs(int r, int c, Node node) {
        if (r < 0 || r >= R || c < 0 || c >= C) return;
        char ch = board[r][c];
        if (ch == '#') return;                    // already on the current path
        var nxt = node.kids[ch - 'a'];
        if (nxt == null) return;                  // PRUNE: prefix not in trie
        if (nxt.word != null) { res.Add(nxt.word); nxt.word = null; } // collect + dedup

        board[r][c] = '#';                        // choose
        Dfs(r+1,c,nxt); Dfs(r-1,c,nxt); Dfs(r,c+1,nxt); Dfs(r,c-1,nxt);
        board[r][c] = ch;                         // un-choose
    }

    for (int r = 0; r < R; r++)
        for (int c = 0; c < C; c++)
            Dfs(r, c, root);
    return res;
}
```

### E. Constraint placement — *neutral example: graph m-coloring*
Assign one of `m` colors to each node so adjacent nodes differ. One "slot" per node; legality checked against already-colored neighbors; undo on return.
```csharp
bool Color(List<int>[] adj, int m, int[] colorOf, int node) {
    if (node == adj.Length) return true;             // all nodes colored
    for (int col = 1; col <= m; col++) {
        bool ok = true;
        foreach (int nb in adj[node])                // legality: no neighbor shares col
            if (colorOf[nb] == col) { ok = false; break; }
        if (!ok) continue;                           // prune
        colorOf[node] = col;                         // choose
        if (Color(adj, m, colorOf, node + 1)) return true;
        colorOf[node] = 0;                           // un-choose
    }
    return false;
}
```

**Picture it.** Walk the nodes in order; at each one try every color, keep only those that clash with no already-colored neighbor, recurse, and on a dead end undo and try the next color:

```
nodes 0-1-2 in a path, m = 2 colors (A,B)

 node0: try A           -> ok            colorOf=[A,_,_]
   node1: try A -> clashes node0; try B  colorOf=[A,B,_]
     node2: try A -> ok (only adj to 1)  colorOf=[A,B,A]  -> all colored -> TRUE

if a node exhausts all m colors with no legal pick, return up and recolor the previous node.
```

**What it does:** decides whether the graph is `m`-colorable (and yields one coloring). Each node is a slot; the inner loop is the choices; the neighbor scan is the legality check that prunes illegal colors *before* recursing. `colorOf[node] = col` / `= 0` is the choose/un-choose. The general lesson this archetype teaches: **maintain just enough committed state to test legality cheaply, and undo it on the way up.** Heavier constraint problems push the idea further — encode the "already used" sets so each legality test is O(1) instead of a neighbor scan; *which* sets to maintain is the per-problem insight you derive cold.

---

## 5. Common pitfalls

- **Forgetting to undo** — the single most common bug. Every `Make` needs a matching `Undo` on the path back up, or choices leak into sibling branches. (For grid: mark `'#'` then restore the original char.)
- **Recording a reference instead of a copy** — `res.Add(path)` stores the *same* mutable list; it'll be empty/garbage by the end. Always `res.Add(new List<>(path))`.
- **Wrong cursor for the pattern** — `start` cursor for subsets/combinations (order doesn't matter), **no** cursor + `used[]` for permutations (order matters). Mixing them gives duplicates or missing answers.
- **Pruning too late or not at all** → exponential blowup / TLE. Sort then `break` (target-sum combinations), maintain occupancy sets for O(1) legality (the constraint-placement archetype), drive the DFS by a trie (trie-pruned grid).
- **Duplicates from duplicate inputs** — when the input has repeats: sort, then `if (i > start && nums[i] == nums[i-1]) continue;` to skip duplicate siblings at the same level.
- **Trie-pruned grid specifics**: array-backed trie node (a–z), store the **full word** on the terminal node (don't rebuild from a path string), and **null the word after collecting** to dedup and shrink the trie. Mark/restore the grid cell, not a separate visited set, for speed.
- **Recording at the wrong place** — power-set style records at *every* node; permutations/target-sum record only at the goal/leaf. Putting the `res.Add` in the wrong spot changes the answer.
- **Base case ordering in grid DFS** — bounds check and visited/mismatch check must come **before** indexing `grid[r][c]`, or you index out of range.

---

## 6. Complexity cheat sheet

| Pattern | Time | Space (stack/aux) | Neutral example |
|---|---|---|---|
| A — include/exclude | O(n · 2ⁿ) | O(n) | binary strings / power set |
| C — permutations | O(n · n!) | O(n) | orderings of n items |
| B — combinations / target sum | O(2^target) bounded, pruned | O(target) | target-sum combos |
| D — grid path | O(R·C · 4^L) | O(L) | spell a target in a grid |
| D′ — trie-pruned grid | O(R·C · 4^Lmax) bounded by trie | O(total chars) trie | many words, one scan |
| E — constraint placement | O(branch^slots) pruned | O(slots) | graph m-coloring |
| G — mapping enumeration | O(k^d · d) | O(d) | digit → letters |

General shape: **time ≈ (#nodes in the decision tree) × (work per node)**; `L` = word/path length. Pruning is what keeps the constant exponent (the `4^L`, `2ⁿ`, `n!`) from actually materializing in the average case.

---

## 7. Map to the problem set

> **Recognition guard:** this maps tools → problems. Reading the row for a problem you haven't attempted hands you the reduction — the exact thing you're meant to derive cold. Use it only during review / spaced reps.

| Pattern from above | Problem-set # (LC) |
|---|---|
| A — include/exclude | #88 (LC 78 Subsets) |
| C — permutations | #89 (LC 46 Permutations) |
| B — combinations with a cursor | #90 (LC 39 Combination Sum) |
| D — grid / path backtracking | #91 (LC 79 Word Search) |
| E — constraint placement | #92 (LC 51 N-Queens) |
| F — partitioning | #93 (LC 131 Palindrome Partitioning) |
| G — mapping enumeration | #94 (LC 17 Letter Combinations) |
| D′ — trie-pruned grid | #70 (LC 212 Word Search II) |
