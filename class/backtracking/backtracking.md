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

**Picture it.** For subsets, each element is a simple yes/no: include it or skip it. The `start` cursor means "only consider elements from here onward," which stops us from re-emitting `{1,2}` and `{2,1}` as different answers. **Every node** of the tree is a valid subset, so we record on entry, not just at leaves:

```
nums = [1,2,3]      record path at EVERY node (shown in [])

                         [ ]
            +1 /      +2 |        +3 \
           [1]        [2]          [3]
        +2 / +3 \      | +3
     [1,2]    [1,3]   [2,3]
      | +3
   [1,2,3]

all recorded: [], [1], [1,2], [1,2,3], [1,3], [2], [2,3], [3]   = 2^3 = 8 subsets
```

After visiting `[1,2,3]` we `RemoveAt` to undo back to `[1,2]`, then up to `[1]`, etc. — each removal is the "un-choose."

**What it does:** generates all 2ⁿ subsets. We `res.Add` a copy at the **top of every call** because every partial path is itself a complete subset (unlike permutations, which only count at the leaves). The loop starts at `start` and recurses with `i + 1` so each element is considered once and in order, which is what prevents duplicate subsets. The `path.Add` / `path.RemoveAt` pair is the choose/un-choose that lets the single `path` list serve the whole tree.

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

**Picture it.** We try to spell the word by walking neighbor-to-neighbor across the grid. At each step we either match the next letter and fan out to the 4 neighbors, or fail. Marking the current cell `'#'` stops the path from reusing it; restoring it on the way back frees it for *other* paths:

```
board:  A B C        word = "ABCC"
        S F C
        A D E

start (0,0)='A' matches word[0]='A'  -> mark '#', try neighbors for 'B'
   (0,1)='B' matches word[1]        -> mark '#', try neighbors for 'C'
      (0,2)='C' matches word[2]     -> mark '#', try neighbors for 'C'
         (1,2)='C' matches word[3]  -> k==len -> TRUE

the '#' trail prevents stepping back onto A or B mid-path;
when a branch fails, each cell is restored to its letter (un-choose).
```

**What it does:** decides whether `word` can be traced through adjacent cells (no cell reused). The base cases come **first and in order**: `k == word.Length` means we matched every letter (success); the bounds/mismatch check rejects off-grid cells, the `'#'` sentinel (already on this path), and wrong letters — this ordering matters so we never index `board[r][c]` out of range. On a match we temporarily blank the cell to `'#'`, recurse into all four directions (the `||` short-circuits on the first success), then **restore the original letter** so a different path can legitimately use that cell. The mark/restore *is* the choose/un-choose, done in-place on the board instead of a separate visited set.

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

**Picture it.** Place exactly **one queen per row**, top to bottom. A new queen is legal only if its column and both diagonals are still free. The trick: cells on the same `↖↘` diagonal share `r - c`; cells on the same `↗↙` diagonal share `r + c` — so three `HashSet`s give O(1) "is this square attacked?" checks:

```
4x4 board, r-c and r+c label the diagonals:

      c=0  c=1  c=2  c=3
 r=0   .    Q    .    .      Q at (0,1): cols={1}, diag(r-c)={-1}, anti(r+c)={1}
 r=1   .    .    .    Q      row 1: c=0? anti 1+0=1 taken. c=2? diag 1-2=-1 taken.
 r=2  ...                         c=3 ok -> place (1,3)
 r=3  ...                    recurse row by row; when r==n, a full board is recorded.

blocked squares from (0,1):  same col 1 | same ↖↘ (r-c=-1) | same ↗↙ (r+c=1)
```

**What it does:** finds all ways to place `n` non-attacking queens. By committing to one queen per row we only ever choose a *column* for the current row, shrinking the tree enormously. The three sets encode the columns and the two diagonal families already under attack; `r - c` is constant along one diagonal direction and `r + c` along the other, which is why membership tests are O(1). The choose step adds to all three sets and records the column; the un-choose removes them — standard backtracking, but the pruning via constraint sets is what makes N-Queens tractable. Reaching `r == n` means every row got a safe queen, so we render and store the board.

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
