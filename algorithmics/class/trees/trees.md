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

**Picture it.** "Post-order" means a node waits for **both children to report back** before it computes its own answer — the information flows *upward* from the leaves:

```
        A                Height asks each child first, then adds 1:
       / \
      B   C                 leaf D: max(0,0)+1 = 1   (null children return 0)
     / \                    leaf E: 1
    D   E                   B: max(D=1, E=1)+1 = 2
                            C: max(0,0)+1 = 1
                            A: max(B=2, C=1)+1 = 3   <- final answer

order calls return:  D=1, E=1, B=2, C=1, A=3
```

**What it does:** computes the height (longest path to a leaf) of every subtree, bubbling the answer up from the bottom. The `node == null` base case returns 0 (an empty subtree has no height); every real node takes the taller of its two children and adds 1 for itself. This same shape — *recurse into both children, then combine* — is the skeleton for almost every "summarize a subtree" problem (count nodes, sum values, is-balanced, diameter); only the combine line changes.

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

**Picture it.** The trap here: the best path *passing through* a node can use **both** of its branches (left + node + right, like an upside-down V), but the value a node can *hand to its parent* can only use **one** branch — because a parent extends a straight line, it can't pass through the same node twice:

```
        node                at `node` we score the V-shape:  l + node.val + r
        /   \               and update `best` with it.
       l     r
      .       .             but we RETURN node.val + max(l, r) only,
     /         \            because the parent above can only attach to
  (parent attaches here)    ONE side of `node`, not both.

example:    -10                Gain(15)=15, Gain(7)=7
            /  \               at 9:  best updated with 9+15+7 = 31
           9   20              return to 9's parent: 9 + max(15,7) = 24
          / \
        15   7
```

**What it does:** finds the maximum path sum where a "path" can bend at one node. We keep two separate quantities: `best` (a global, the best complete V-shaped path seen anywhere) and the **return value** (the best straight extension this node can offer its parent). At each node we *score* using both branches but only *return* one. The `Math.Max(child, 0)` clamps a negative branch to 0 — if a branch would only drag the sum down, we simply don't take it (a path can stop at the node). This split between "score here" and "return upward" is the pattern's whole lesson.

### Top-down bounds (validate BST)
```csharp
bool Valid(TreeNode node, long low, long high) {
    if (node == null) return true;
    if (node.val <= low || node.val >= high) return false;
    return Valid(node.left, low, node.val) && Valid(node.right, node.val, high);
}
```

**Picture it.** "Top-down" means information flows *downward*: each node hands its children a tightened legal range `(low, high)`. Going left, the current value becomes the new ceiling; going right, it becomes the new floor:

```
          5            range (-∞, +∞)
         / \
        3   8          3 must be in (-∞, 5) ok;  8 in (5, +∞) ok
       / \
      1   4            1 in (-∞, 3) ok
          ^
          4 in (3, 5) ok

BUT if 4 were 6:   6 in (3, 5)?  6 >= 5  ->  INVALID
(even though 6 < its parent 3's... wait, 6 > 3 locally looks fine -
 the inherited ceiling of 5 from grandparent is what catches it)
```

The key insight the diagram shows: a node isn't just compared to its parent — it must respect bounds inherited from **all** ancestors. A local parent-child check alone would wrongly accept that `6`.

**What it does:** verifies the BST property (every left value < node < every right value) by threading a shrinking `(low, high)` window down the tree. A node is valid only if it sits strictly inside its window; then it narrows the window for each child. We use `long` bounds so node values of `int.MinValue`/`int.MaxValue` don't break the comparison. This top-down threading is the go-to whenever a node's validity depends on its ancestors.

### In-order (sorted walk of a BST)
```csharp
void InOrder(TreeNode node, List<int> outv) {
    if (node == null) return;
    InOrder(node.left, outv);
    outv.Add(node.val);          // <-- visit here = sorted order for a BST
    InOrder(node.right, outv);
}
```

**Picture it.** "In-order" = **left, then self, then right.** On a BST, that rule emits values in increasing order, because everything smaller (left subtree) is fully visited before the node, and everything larger (right subtree) after:

```
          5
         / \
        3   8         in-order visit sequence:
       / \   \
      1   4   9       (1)(3)(4)  5  (8)(9)
                       \_______/  ^  \___/
                       left done  |  right after
                                  visit 5

output: 1 3 4 5 8 9   <- sorted!
```

**What it does:** visits BST nodes in sorted order by always fully exploring the left subtree before recording the current node, then the right. This single property powers a whole family of BST problems: validating a BST (the sequence must be strictly increasing), finding the kth smallest (stop at the kth value emitted), and range queries. Move the `outv.Add` line before the left call and you get pre-order; after the right call and you get post-order — same skeleton, the *position of the visit* is the only knob.

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

**Picture it.** This is the same left-self-right order as above, but we manage the "come back to me later" list ourselves with a `Stack`. The inner `while` runs as far left as possible, stacking nodes we'll return to; popping gives the next smallest:

```
          5
         / \
        3   8          step 1: push 5, push 3, push 1   stack(bottom->top): [5,3,1]
       /                step 2: pop 1 -> VISIT 1, go right (none)
      1                 step 3: pop 3 -> VISIT 3, go right (none)
                        step 4: pop 5 -> VISIT 5, go right to 8, push 8
                        step 5: pop 8 -> VISIT 8

visits: 1, 3, 5, 8   (and you can STOP early after k pops for kth-smallest)
```

**What it does:** produces the in-order sequence *one value at a time on demand*, without recursion. The stack holds the chain of ancestors we still owe a visit to. Why bother over the clean recursive version? Two reasons: (1) a `BSTIterator` (LC 173) needs to pause between values and resume later — only an explicit stack lets you freeze mid-traversal; (2) for kth-smallest you can stop after exactly `k` pops instead of walking the whole tree. Space is O(h) — the stack only ever holds one root-to-current path.

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

**Picture it.** BFS walks the tree **level by level**, left to right, using a queue. The trick is to *freeze* the queue's size at the start of each level — that snapshot is exactly how many nodes are on the current level, so children we enqueue mid-loop wait for the next pass:

```
        3                queue starts: [3]
       / \
      9  20             level 0: size=1 -> pop 3, enqueue 9,20   level=[3]
         / \            level 1: size=2 -> pop 9,20, enqueue 15,7 level=[9,20]
        15  7           level 2: size=2 -> pop 15,7              level=[15,7]

result: [[3], [9,20], [15,7]]

why snapshot? at the start of level 1 the queue is [9,20] (size 2).
we pop exactly 2, even though 15 and 7 get added during the loop -
they belong to the NEXT level and must not bleed into this one.
```

**What it does:** groups nodes by depth. `q.Count` is read **once** into `levelSize` before the inner loop so the loop processes precisely the nodes that were already queued (this level), while the children it enqueues form the next level. This size-snapshot is the one idea behind every level-based problem: right-side view (take the last node each level), level averages, min depth (first level with a leaf), zigzag order (reverse alternate levels).

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

**Picture it.** Serialize = flatten the tree to a string; deserialize = rebuild it. Pre-order (self, left, right) with a marker `#` for every missing child makes the string **unambiguous** — the `#`s record the exact shape:

```
        1
       / \          serialize (pre-order, # = null):
      2   3              "1,2,#,#,3,#,#,"
                          ^ ^ ^ ^ ^ ^ ^
                          1 2 | | 3 | |
                              2's two nulls   3's two nulls

deserialize consumes tokens left-to-right in the SAME order:
   read 1 -> make node, then build its left, then its right
     read 2 -> make node, left=read'#'=null, right=read'#'=null
     read 3 -> make node, left=null, right=null
   -> exact original tree rebuilt
```

**What it does:** converts a tree to text and back. The `#` null-markers are essential — without them `"1,2,3"` could rebuild into several different trees. The deep symmetry: serialize writes node, then recurses left, then right; deserialize reads node, then recurses left, then right — *same order*, so a shared cursor (`idx`) consuming tokens reconstructs the structure perfectly. The N-ary version (LC 428) is identical except you also record each node's child count (or a sentinel) so you know how many children to rebuild.

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

**Picture it.** Lowest Common Ancestor = the deepest node that has both `p` and `q` somewhere below it. Each call reports "did I find p or q (or the LCA) in my subtree?" If *both* sides report a hit, the current node is where the two paths meet — the LCA:

```
        3                find LCA of 5 and 1
       / \
      5   1              at 5: returns 5 (it IS p)
     / \                 at 1: returns 1 (it IS q)
    6   2                at 3: left=5 (non-null), right=1 (non-null)
                              -> BOTH sides hit -> 3 is the split point -> LCA = 3

find LCA of 6 and 2:
   at 6 -> 6,  at 2 -> 2,  at 5 -> both hit -> LCA = 5
   at 3 -> left=5, right=null -> bubble up 5 unchanged
```

**What it does:** locates the lowest common ancestor in one post-order pass. The base case returns `node` itself when it's null or equals `p`/`q` ("I found a target, here it is"). After recursing, three cases: **both** children returned non-null → the targets are in different subtrees, so *this* node is their meeting point (the answer); **one** non-null → both targets live on that side, so pass that result up unchanged; **neither** → return null. The answer bubbles up to exactly the split node and then rides unchanged to the root.

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
