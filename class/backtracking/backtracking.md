# Class: Backtracking

> Canonical class for the `backtracking` topic. Living document — fold new templates/pitfalls back in as problems reveal them.

---

## 1. Context & when to reach for it

**Backtracking = DFS over the tree of partial solutions, undoing each choice on the way back up.** You build a candidate incrementally; at each step you try every legal next choice, recurse, then **un-make** the choice before trying the next one. It is exhaustive search made tractable by **pruning** branches that can't lead to a valid/optimal answer.

It is the right tool when the answer is a **combination of choices** and you must enumerate or find one that satisfies constraints — and there's **no closed-form or greedy shortcut**.

**Signals you're in backtracking territory:**
- "Find **all** …" — all subsets, all permutations, all combinations, all valid placements/partitions.
- "Generate every …" / "how many ways …" where you must construct each candidate.
- Constraint satisfaction: place N queens, color a graph, fill a Sudoku, partition a string so every piece is a palindrome.
- A search over a grid/board where a path must spell something or satisfy a rule (LC 79, LC 212).
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

The art is almost entirely in (2): *the earlier and cheaper you prune, the smaller the tree.* A trie (LC 212) and N-Queens' column/diagonal sets are both pruning machines.

---

## 3. Core patterns

### A. Subsets (include/exclude per element) — LC 78
Each element is a binary choice: in or out. Every node of the tree is a valid subset, so you record at **every** node, not just leaves. `start` index prevents revisiting earlier elements.

### B. Combinations / combination sum — LC 39, LC 77
Pick elements with a `start` cursor so combinations aren't re-emitted in different orders (`{2,3}` not also `{3,2}`). LC 39 allows reuse → recurse with the **same** index; no reuse → `i+1`. Prune when the running sum exceeds target (sort first to break early).

### C. Permutations (order matters, use-each-once) — LC 46
No `start` cursor — every unused element is a candidate at every position. Track used elements (a `used[]` bool array or swap-in-place). Record only at **leaves** (length == n).

### D. Grid / path backtracking — LC 79, LC 212
DFS from cells across the 4 neighbors; mark a cell visited before recursing, unmark after. The candidate is the path through the grid. **LC 212** drives this DFS by a **trie** of target words so dead prefixes are pruned instantly (see the tries class, pattern C) — this is the payoff that fuses both topics.

### E. Constraint placement — LC 51 N-Queens, Sudoku
Place one unit per "row" (queen per row), maintain sets of occupied columns/diagonals for **O(1) legality checks**, recurse to the next row, undo on return. Pure pruning-driven search.

### F. Partitioning — LC 131 Palindrome Partitioning
Choose where to cut next; each prefix piece must satisfy a predicate (palindrome). `start` marks the cut position; recurse on the remainder.

### G. Mapping enumeration — LC 17 Letter Combinations
A multi-way choice per position (the letters for each digit). Depth = number of digits; branch = letters at this digit.

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

### A. Subsets (LC 78)
```csharp
IList<IList<int>> Subsets(int[] nums) {
    var res = new List<IList<int>>();
    var path = new List<int>();
    void Dfs(int start) {
        res.Add(new List<int>(path));            // record EVERY node
        for (int i = start; i < nums.Length; i++) {
            path.Add(nums[i]);                   // choose
            Dfs(i + 1);                          // explore (no reuse)
            path.RemoveAt(path.Count - 1);       // un-choose
        }
    }
    Dfs(0);
    return res;
}
```

### B. Combination Sum (LC 39 — reuse allowed)
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

### C. Permutations (LC 46)
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

### D. Grid backtracking (LC 79 — single word)
```csharp
bool Exist(char[][] board, string word) {
    int R = board.Length, C = board[0].Length;
    bool Dfs(int r, int c, int k) {
        if (k == word.Length) return true;
        if (r < 0 || r >= R || c < 0 || c >= C || board[r][c] != word[k]) return false;
        char tmp = board[r][c];
        board[r][c] = '#';                       // mark visited (in place)
        bool found = Dfs(r+1,c,k+1) || Dfs(r-1,c,k+1)
                  || Dfs(r,c+1,k+1) || Dfs(r,c-1,k+1);
        board[r][c] = tmp;                       // un-mark (backtrack)
        return found;
    }
    for (int r = 0; r < R; r++)
        for (int c = 0; c < C; c++)
            if (Dfs(r, c, 0)) return true;
    return false;
}
```

### D′. Trie-pruned grid backtracking (LC 212 — many words, the payoff)
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

### E. N-Queens (LC 51) — O(1) legality via sets
```csharp
// cols, diag (r-c), anti (r+c) as HashSet<int>; place one queen per row.
void Dfs(int r) {
    if (r == n) { res.Add(Render()); return; }
    for (int c = 0; c < n; c++) {
        if (cols.Contains(c) || diag.Contains(r-c) || anti.Contains(r+c)) continue; // prune
        cols.Add(c); diag.Add(r-c); anti.Add(r+c); place[r] = c;   // choose
        Dfs(r + 1);
        cols.Remove(c); diag.Remove(r-c); anti.Remove(r+c);        // un-choose
    }
}
```

---

## 5. Common pitfalls

- **Forgetting to undo** — the single most common bug. Every `Make` needs a matching `Undo` on the path back up, or choices leak into sibling branches. (For grid: mark `'#'` then restore the original char.)
- **Recording a reference instead of a copy** — `res.Add(path)` stores the *same* mutable list; it'll be empty/garbage by the end. Always `res.Add(new List<>(path))`.
- **Wrong cursor for the pattern** — `start` cursor for subsets/combinations (order doesn't matter), **no** cursor + `used[]` for permutations (order matters). Mixing them gives duplicates or missing answers.
- **Pruning too late or not at all** → exponential blowup / TLE. Sort then `break` (combination sum), use O(1) constraint sets (N-Queens), drive the DFS by a trie (LC 212).
- **Duplicates from duplicate inputs** — LC 90/40/47: sort, then `if (i > start && nums[i] == nums[i-1]) continue;` to skip duplicate siblings at the same level.
- **LC 212 specifics**: array-backed trie node (a–z), store the **full word** on the terminal node (don't rebuild from a path string), and **null the word after collecting** to dedup and shrink the trie. Mark/restore the grid cell, not a separate visited set, for speed.
- **Recording at the wrong place** — subsets record at *every* node; permutations/combination-sum record only at the goal/leaf. Putting the `res.Add` in the wrong spot changes the answer.
- **Base case ordering in grid DFS** — bounds check and visited/mismatch check must come **before** indexing `board[r][c]`, or you index out of range.

---

## 6. Complexity cheat sheet

| Problem | Time | Space (stack/aux) |
|---|---|---|
| Subsets (LC 78) | O(n · 2ⁿ) | O(n) |
| Permutations (LC 46) | O(n · n!) | O(n) |
| Combination Sum (LC 39) | O(2^target) bounded, pruned | O(target) |
| Word Search (LC 79) | O(R·C · 4^L) | O(L) |
| Word Search II (LC 212) | O(R·C · 4^Lmax) bounded by trie | O(total chars) trie |
| N-Queens (LC 51) | O(n!) | O(n) |
| Letter Combinations (LC 17) | O(4ⁿ · n) | O(n) |

General shape: **time ≈ (#nodes in the decision tree) × (work per node)**; `L` = word/path length. Pruning is what keeps the constant exponent (the `4^L`, `2ⁿ`, `n!`) from actually materializing in the average case.

---

## 7. Map to the problem set (Topic 12 — Backtracking)

| # | LC | Pattern from above |
|---|---|---|
| 88 | 78 Subsets | A — include/exclude, record every node |
| 89 | 46 Permutations | C — `used[]`, record at leaves |
| 90 | 39 Combination Sum | B — `start` cursor, reuse same index, prune by sum |
| 91 | 79 Word Search | D — grid DFS, mark/unmark cell |
| 92 | 51 N-Queens | E — one queen per row, O(1) constraint sets |
| 93 | 131 Palindrome Partitioning | F — cut positions, palindrome predicate (R) |
| 94 | 17 Letter Combinations | G — multi-way choice per digit (R) |

Cross-topic: **LC 212 Word Search II** (Topic 10 Tries, #70) is grid backtracking (pattern D′) pruned by a trie — the fusion of this class and the tries class. The grid/path DFS muscle here is the same one reused there.
