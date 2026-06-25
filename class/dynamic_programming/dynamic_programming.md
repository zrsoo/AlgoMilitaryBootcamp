# Class: Dynamic Programming

> Canonical class for the `dynamic_programming` topic — the **tool library**. Living document; fold new templates/pitfalls back in as problems reveal them.
> Tools here are taught on **neutral examples** (Fibonacci, climbing stairs, rod cutting, LCS). The mapping to specific problem-set #s is at the bottom — **don't read that mapping for a problem you haven't yet attempted; it's the reduction.**

---

## 1. Context & when to reach for it

**DP = recursion with overlapping subproblems, solved once and reused.** You have a problem whose answer is built from answers to *smaller versions of the same problem*, and the naive recursion re-computes the same subproblems exponentially often. DP gives each distinct subproblem a slot, computes it once, and reads it back.

Two equivalent forms:
- **Top-down (memoized recursion)** — write the natural recursion, cache results in a dictionary/array keyed by the subproblem's parameters.
- **Bottom-up (tabulation)** — fill a table in dependency order so every value you need is already computed.

**Signals you're in DP territory:**
- "**Count the number of ways** …", "**how many** distinct …".
- "**Maximize / minimize**" some total over a sequence of choices, where greedy fails because a locally-bad choice can pay off later.
- "Can you reach / make / partition …" (boolean feasibility over choices).
- Two strings/sequences compared position-by-position (edit distance, common subsequence, matching).
- A naive backtracking solution exists but **TLEs because subproblems repeat**.

> Rule of thumb: **backtracking** enumerates the whole choice tree; **DP** is backtracking where the tree has *overlapping* subtrees you can memoize. If "all configurations" → backtracking; if "count/optimum/feasibility" *and* overlap → DP.

---

## 2. Mental model — the four questions

Every DP is nailed down by answering, in order:
1. **State** — what parameters uniquely identify a subproblem? (`dp[i]`, `dp[c]`, `dp[i,j]`). This is 80% of the work; get it wrong and nothing else lands.
2. **Recurrence** — how is a state's answer built from *smaller* states? (the choice you make at this step).
3. **Base case(s)** — the smallest states whose answers are known outright.
4. **Order** — fill states so dependencies come first (or memoize and let recursion handle order).

A useful discipline: **write the recursion first** (state + recurrence + base), confirm it's correct, *then* decide memo vs. tabulation and (optionally) space-optimize. Don't start from a table.

---

## 3. Core patterns

### A. 1-D linear DP
`dp[i]` depends on a **fixed window** of earlier indices (`dp[i-1]`, `dp[i-2]`, …). The classic shape of "process a sequence, each position's answer leans on the last few." Often space-optimizable to a couple of rolling scalars.

### B. Unbounded knapsack
Fill a capacity using items that may be reused **any number of times**, optimizing value/count/ways. Signature move: the inner transition reads the **same capacity row** (`dp[c - item]`), which is what permits reuse. (Contrast **0/1 knapsack**: each item once → iterate capacity *downward* so an item can't be re-counted.)

### C. 2-D string / grid DP
`dp[i,j]` over two indices — two strings compared prefix-by-prefix, or a position in a grid. The cell depends on its up/left/diagonal neighbors. Edit distance, longest common subsequence, matching, grid path counts.

---

## 4. Reusable templates (C#)

### A. 1-D linear DP — *neutral example: climbing stairs / Fibonacci*
Ways to climb `n` steps taking 1 or 2 at a time. `dp[i]` = ways to reach step `i`.
```csharp
int ClimbStairs(int n) {
    if (n <= 2) return n;
    int prev2 = 1, prev1 = 2;            // dp[1]=1, dp[2]=2
    for (int i = 3; i <= n; i++) {
        int cur = prev1 + prev2;         // recurrence: dp[i] = dp[i-1] + dp[i-2]
        prev2 = prev1; prev1 = cur;      // roll the window
    }
    return prev1;
}
```

**Picture it.** To stand on step `i`, your last move came from `i-1` (a 1-step) or `i-2` (a 2-step) — so the ways into `i` are the sum of the ways into those two. That's literally Fibonacci:

```
step:   1   2   3   4   5
ways:   1   2   3   5   8
              ^   ^   ^
        dp[3]=dp[2]+dp[1]=2+1=3
        dp[4]=dp[3]+dp[2]=3+2=5
        dp[5]=dp[4]+dp[3]=5+3=8

only the last TWO values ever matter -> two rolling scalars, O(1) space
```

**What it does:** counts paths to step `n`. The **state** is "which step," the **recurrence** sums the two states that can reach it, the **base** is the first one/two steps. Because each `dp[i]` only looks back a fixed distance (2), we never need the whole array — two variables `prev1`/`prev2` rolled forward give O(n) time, O(1) space. This same skeleton (state = position, transition = a `max`/`sum`/`min` over the last *k* states) covers the whole family of "scan a sequence, each position leans on a fixed window of earlier answers."

### B. Unbounded knapsack — *neutral example: rod cutting*
A rod of length `N`; `price[len]` is what a piece of that length sells for; cut into pieces (each length reusable) to **maximize** total value. `dp[c]` = best value obtainable from capacity `c`.
```csharp
int RodCut(int[] price, int N) {        // price[1..N] defined
    int[] dp = new int[N + 1];          // dp[0] = 0
    for (int c = 1; c <= N; c++)
        for (int len = 1; len <= c; len++)
            dp[c] = Math.Max(dp[c], price[len] + dp[c - len]);  // reuse -> dp[c-len]
    return dp[N];
}
```

**Picture it.** For each capacity `c`, try every first piece of length `len`, take its `price[len]`, then add the **already-computed best** for the *remaining* capacity `c - len`. Reading `dp[c - len]` (same row, smaller capacity) is what lets a length be used again:

```
price: len  1  2  3  4
       val  1  5  8  9      rod N = 4

dp[0]=0
dp[1]= price[1]+dp[0] = 1
dp[2]= max(price[1]+dp[1]=2, price[2]+dp[0]=5) = 5
dp[3]= max(p1+dp2=6, p2+dp1=6, p3+dp0=8) = 8
dp[4]= max(p1+dp3=9, p2+dp2=10, p3+dp1=9, p4+dp0=9) = 10   (two 2-pieces)
```

**What it does:** maximizes value with **unlimited** reuse of each item. The **state** is remaining capacity `c`; the transition tries each item and recurses on `dp[c - item]` — the same `dp` row, which is precisely what allows the item to be chosen again. Flip the meaning of the `Math.Max` to `min`/`+=` and you get the min-items or count-the-ways variants of the same shape. The structural contrast to remember: **unbounded** reads `dp[c-len]` of the *current* fill (reuse allowed); **0/1** processes items in an outer loop and iterates `c` *downward* so each item lands at most once.

### C. 2-D string DP — *neutral example: Longest Common Subsequence*
LCS length of `a` (length `m`) and `b` (length `n`). `dp[i,j]` = LCS of `a[..i)` and `b[..j)`.
```csharp
int Lcs(string a, string b) {
    int m = a.Length, n = b.Length;
    int[,] dp = new int[m + 1, n + 1];          // row/col 0 = empty prefix = 0
    for (int i = 1; i <= m; i++)
        for (int j = 1; j <= n; j++)
            dp[i, j] = a[i-1] == b[j-1]
                ? dp[i-1, j-1] + 1                       // chars match: extend diagonal
                : Math.Max(dp[i-1, j], dp[i, j-1]);      // else: drop one char, take best
    return dp[m, n];
}
```

**Picture it.** Build a grid where row `i`/col `j` mean "first `i` chars of `a`, first `j` of `b`." If the current chars match, the answer extends the **diagonal** (both prefixes shrink by one); if not, you take the better of dropping `a`'s char (up) or `b`'s char (left):

```
        ""  a  c  e
   ""    0  0  0  0
   a     0  1  1  1
   b     0  1  1  1
   c     0  1  2  2
   d     0  1  2  2
   e     0  1  2  3   <- dp[m,n] = 3   ("ace")

match (a==a, c==c, e==e): cell = up-left diagonal + 1
mismatch: cell = max(up, left)
```

**What it does:** computes the longest subsequence common to both strings. The **state** is the pair of prefix lengths `(i, j)`; the **recurrence** branches on whether the current characters match (extend the diagonal `dp[i-1,j-1]+1`) or not (carry the best of `dp[i-1,j]` / `dp[i,j-1]`); the **base** is an empty prefix scoring 0 (row 0 and col 0). The string-index footgun: `dp` is 1-indexed over *prefix lengths*, so the character at table position `i` is `a[i-1]` — mixing those up is the #1 bug. This grid shape (diagonal on match, max/min of neighbors otherwise) generalizes to edit distance, string matching, and grid path-counting — only the cell formula changes.

---

## 5. Common pitfalls

- **State that doesn't capture enough** — if two different situations map to the same `dp` slot but have different answers, your state is under-specified. Add a dimension. (Most "my DP is subtly wrong" bugs are here.)
- **Base case / empty-prefix off-by-one** — 2-D string DP almost always needs the `0`-th row and column (empty prefix) seeded; forgetting them shifts everything.
- **String index vs. table index** — `dp[i]` over prefix *length* `i` corresponds to character `s[i-1]`. Be consistent.
- **Unbounded vs 0/1 direction** — unbounded reads the same row (`dp[c-item]`); 0/1 must iterate capacity downward (or use a previous-row copy) so each item is used once. Mixing them silently double-counts or under-counts.
- **Wrong fill order (tabulation)** — a cell must be filled after every cell it depends on. Row-major works for up/left/diagonal dependencies; verify before looping.
- **Forgetting to memoize (top-down)** — the recursion is correct but exponential; the cache is the whole point. Key the cache by the *full* state tuple.
- **Over-optimizing space too early** — get the full table correct first, then collapse to rolling rows/scalars. Premature space tricks hide bugs.

---

## 6. Complexity cheat sheet

| Pattern | Time | Space (full → optimized) | Neutral example |
|---|---|---|---|
| 1-D linear | O(n) | O(n) → O(1) (rolling window) | climbing stairs / Fibonacci |
| Unbounded knapsack | O(N · items) | O(N) | rod cutting |
| 0/1 knapsack | O(N · items) | O(N · items) → O(N) | subset sum |
| 2-D string/grid | O(m · n) | O(m · n) → O(min(m,n)) (two rows) | LCS / edit distance |

---

## 7. Map to the problem set

> **Recognition guard:** this maps tools → problems. Reading the row for a problem you haven't attempted hands you the reduction — the exact thing you're meant to derive cold. Use it only during review / spaced reps.

| Pattern from above | Problem-set # (LC) |
|---|---|
| A — 1-D linear DP | #95 (LC 198 House Robber) |
| B — unbounded knapsack | #97 (LC 322 Coin Change) |
| C — 2-D string DP | #104 (LC 10 Regex Matching), #101 (LC 72 Edit Distance) |
| A — 1-D / LIS variant | #98 (LC 300 Longest Increasing Subsequence) |
