# Class: Trees & BSTs

> Canonical class for the `trees` topic. Living document — fold new templates/pitfalls back in as problems reveal them.

---

## 1. Context & when to reach for it

A **binary tree** is a recursive structure: each node has a value and up to two children (`left`, `right`). A node with no children is a leaf. The **height** `h` is the longest root-to-leaf path; for a balanced tree `h ≈ log n`, for a skewed tree `h ≈ n`.

A **BST (binary search tree)** adds an ordering invariant: for every node, *all* values in the left subtree are `< node.val` and *all* in the right are `> node.val`. That invariant is the whole game — it's what lets you make directional decisions (go left or right) and what an inorder traversal exploits.

**Signals you're in tree territory:**
- Input is a `TreeNode root`, or "binary tree" / "BST" / "n-ary tree" in the prompt.
- Hierarchical / parent-child / ancestor relationships.
- "Level", "depth", "path from root", "lowest common ancestor", "kth smallest".
- Serialize / reconstruct a structure.

**The single most important mindset:** almost every tree problem is solved by **defining what one node's recursive call should return, assuming the recursion already works on its children.** Trust the recursion ("recursive leap of faith"): write the base case, define the combine step, and don't trace the whole tree in your head.

---

## 2. Mental model

Three questions unlock nearly every tree problem:

1. **What do I need from each child?** (the return value of the recursion)
2. **How do I combine the children's answers with the current node?** (the merge step)
3. **What's the base case?** (usually `node == null`, sometimes a leaf)

There are two axes of choice:

**Traversal order** — when do you process the current node relative to its children?
- **Pre-order** (node, left, right): top-down. Use when a node needs info *from its ancestors* (passed down as parameters), e.g. validate-BST bounds, path-from-root.
- **Post-order** (left, right, node): bottom-up. Use when a node needs info *from its subtrees* (returned up), e.g. height, max-path-sum, LCA, "is balanced". **This is the most common for hard problems.**
- **In-order** (left, node, right): for BSTs this visits values **in sorted order** — the key BST trick.

**Traversal strategy** — DFS (recursion / explicit stack) vs BFS (queue):
- **DFS** for path/height/subtree-aggregate problems.
- **BFS** for anything *level-based* (level order, right-side view, min depth, level averages).

> Rule of thumb: if the problem mentions "level" → BFS. If it's about paths, heights, subtree properties, or ancestor relationships → DFS (usually post-order). If it's a BST and asks about order/rank/range → in-order.

---

## 3. Core patterns

### A. Bottom-up aggregate (post-order)
Each node returns a summary of its subtree (height, sum, count, "is valid"). Combine children's summaries, then return your own. Often you maintain a **global/ref answer** separately from the **return value** when the "path through a node" differs from the "contribution to the parent" — this is the LC 124 trick.

### B. Top-down bounds/state (pre-order)
Pass constraints *down* as parameters. Each node validates itself against inherited state, then tightens the state for its children. LC 98 (validate BST) threads `(low, high)`.

### C. In-order for BST
In-order traversal of a BST yields sorted values. Powers: validate BST (must be increasing), kth smallest (stop at the kth visit), BST iterator (lazy in-order with a stack), range queries.

### D. Level-order (BFS with size snapshot)
Snapshot `queue.Count` at the start of each level; that count is exactly the current level's node count. Children enqueued mid-loop wait for the next pass.

### E. Serialize / deserialize
Encode the tree to a string (pre-order with explicit `null` markers is simplest), then rebuild by consuming tokens in the same order. The `null` markers are what make the structure unambiguous.

### F. LCA / split-point
Post-order returning a node. If both sides report non-null, the current node is the split point; otherwise bubble up the one non-null child unchanged.

---

## 4. Reusable templates (C#)

```csharp
public class TreeNode {
    public int val;
    public TreeNode left;
    public TreeNode right;
    public TreeNode(int val = 0, TreeNode left = null, TreeNode right = null) {
        this.val = val; this.left = left; this.right = right;
    }
}
```

### Post-order aggregate (height / subtree summary)
```csharp
int Height(TreeNode node) {
    if (node == null) return 0;
    int l = Height(node.left);
    int r = Height(node.right);
    return 1 + Math.Max(l, r);
}
```

### Post-order with a separate global answer (LC 124 shape)
```csharp
int best = int.MinValue;

int Gain(TreeNode node) {          // returns best DOWNWARD path (one branch only)
    if (node == null) return 0;
    int l = Math.Max(Gain(node.left),  0);   // clamp negatives to 0 (don't take a losing branch)
    int r = Math.Max(Gain(node.right), 0);
    best = Math.Max(best, node.val + l + r);  // path THROUGH node uses both branches
    return node.val + Math.Max(l, r);         // but a parent can only extend ONE branch
}
```

### Top-down bounds (validate BST)
```csharp
bool Valid(TreeNode node, long low, long high) {
    if (node == null) return true;
    if (node.val <= low || node.val >= high) return false;
    return Valid(node.left, low, node.val) && Valid(node.right, node.val, high);
}
```

### In-order (sorted walk of a BST)
```csharp
void InOrder(TreeNode node, List<int> outv) {
    if (node == null) return;
    InOrder(node.left, outv);
    outv.Add(node.val);          // <-- visit here = sorted order for a BST
    InOrder(node.right, outv);
}
```

### Iterative in-order (BST iterator / kth smallest)
```csharp
var stack = new Stack<TreeNode>();
TreeNode cur = root;
while (cur != null || stack.Count > 0) {
    while (cur != null) { stack.Push(cur); cur = cur.left; }
    cur = stack.Pop();
    // visit cur.val here (in sorted order)
    cur = cur.right;
}
```

### BFS level order (size snapshot)
```csharp
var res = new List<IList<int>>();
var q = new Queue<TreeNode>();
if (root != null) q.Enqueue(root);
while (q.Count > 0) {
    int levelSize = q.Count;                 // freeze before the loop
    var level = new List<int>();
    for (int i = 0; i < levelSize; i++) {
        var node = q.Dequeue();
        level.Add(node.val);
        if (node.left  != null) q.Enqueue(node.left);
        if (node.right != null) q.Enqueue(node.right);
    }
    res.Add(level);
}
```

### Serialize / deserialize (pre-order + null markers)
```csharp
// serialize
void Ser(TreeNode node, StringBuilder sb) {
    if (node == null) { sb.Append("#,"); return; }
    sb.Append(node.val).Append(',');
    Ser(node.left, sb);
    Ser(node.right, sb);
}
// deserialize: consume tokens in the same pre-order
int idx = 0; string[] t;
TreeNode De() {
    string tok = t[idx++];
    if (tok == "#") return null;
    var node = new TreeNode(int.Parse(tok));
    node.left  = De();
    node.right = De();
    return node;
}
```

### LCA (post-order, return node)
```csharp
TreeNode LCA(TreeNode node, TreeNode p, TreeNode q) {
    if (node == null || node == p || node == q) return node;
    var l = LCA(node.left, p, q);
    var r = LCA(node.right, p, q);
    if (l != null && r != null) return node;   // split point
    return l ?? r;                             // bubble up unchanged
}
```

---

## 5. Common pitfalls

- **Missing base case** → infinite recursion / `NullReferenceException`. Almost always `if (node == null) return <identity>;` first.
- **Confusing "path through node" with "contribution to parent."** In LC 124 you return one branch but score using both. Keep the return value and the global answer separate.
- **Forgetting to clamp negative gains** (`Math.Max(childGain, 0)`) in max-path problems — a negative branch should be dropped, not added.
- **BST validation with local-only checks.** Comparing a node only to its immediate children is *unsound* — you must enforce bounds inherited from all ancestors. (Also re-walking subtrees per node is O(n²).)
- **`int` overflow** in BST bounds → use `long` sentinels (`long.MinValue/MaxValue`) to survive `int.MinValue/MaxValue` node values.
- **Re-reading `queue.Count` inside the BFS for-loop** — snapshot it; children enqueued mid-loop otherwise corrupt the level boundary.
- **Re-calling the recursion instead of using stored results** (e.g. `return LCA(node.left,...)` after already computing `l`) → exponential blowup. Store and reuse `l`/`r`.
- **Deep skewed trees** can blow the recursion stack (~10⁴–10⁵ deep). Mention the iterative/explicit-stack alternative if asked.
- **Serialize: forgetting null markers** makes the structure ambiguous and un-rebuildable.

---

## 6. Complexity cheat sheet

| Operation | Time | Space |
|---|---|---|
| Any single DFS/BFS traversal | O(n) | O(h) DFS stack / O(w) BFS queue (w = max width) |
| Balanced-tree height h | — | O(log n) |
| Skewed-tree height h | — | O(n) |
| BST search/insert (balanced) | O(log n) | O(h) |
| BST search/insert (skewed) | O(n) | O(n) |
| Serialize / deserialize | O(n) | O(n) |

`n` = node count, `h` = height, `w` = max level width.

---

## 7. Map to the problem set (Topic 9 — Trees & BSTs)

| # | LC | Pattern from above |
|---|---|---|
| 58 | 104 Max Depth | A — post-order aggregate (height) |
| 59 | 102 Level Order | D — BFS size snapshot |
| 60 | 98 Validate BST | B — top-down bounds (or C in-order increasing) |
| 61 | 236 LCA | F — post-order split point |
| 62 | 124 Max Path Sum | A — post-order with separate global answer |
| 63 | 297 Serialize/Deserialize | E — pre-order + null markers |
| 64 | 428 Serialize N-ary | E — generalized to a child list + count/sentinel |
| 65 | 199 Right Side View | D — BFS, take last node per level |
| 66 | 173 BST Iterator | C — lazy iterative in-order |
| 67 | 230 Kth Smallest in BST | C — in-order, stop at kth |

Related elsewhere: tries (Topic 10) are a tree variant; graph traversal (Topic 11) generalizes DFS/BFS to arbitrary nodes with a visited set.
