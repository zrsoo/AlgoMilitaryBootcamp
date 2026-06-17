# Class: Graphs

> Canonical class for the `graphs` topic — the heaviest single block in the set. Living document; fold new templates/pitfalls back in as problems reveal them.

---

## 1. Context & when to reach for it

A **graph** is nodes (vertices) connected by **edges**. Trees and linked lists are special cases; graphs add the two complications that define the topic: **cycles** (so you need a `visited` set) and **multiple paths** between nodes (so "shortest/cheapest" becomes a real question).

Key axes that change the algorithm:
- **Directed vs undirected** — directed edges go one way (course prereqs, flights); undirected both ways (friendships, adjacency).
- **Weighted vs unweighted** — unweighted = "fewest edges" (BFS); weighted = "least total cost" (Dijkstra/Bellman-Ford).
- **Explicit vs implicit** — explicit: you're given edges/adjacency. **Implicit**: the graph is hidden — grid cells with 4-directional neighbors (LC 200, 994, 1631), or states connected by moves (LC 815 stops↔routes, LC 847 bitmask states). Recognizing the implicit graph is half the battle.

**Signals you're in graph territory:**
- Grid + "connected regions / islands / flood fill / shortest path in a maze".
- "Prerequisites", "ordering", "dependencies", "can you finish" → topological sort.
- "Connected components", "groups", "provinces", "redundant connection" → DFS/BFS or Union-Find.
- "Shortest / cheapest / minimum cost path" with weights → Dijkstra (non-negative) or Bellman-Ford (negative / k-stops).
- "Fewest steps / moves / transfers" unweighted → BFS.

> Rule of thumb: **unweighted shortest path → BFS. Weighted shortest path → Dijkstra. Ordering with dependencies → topo sort. Dynamic connectivity / "are these joined" → Union-Find.**

---

## 2. Mental model

Every graph algorithm is a disciplined way of **exploring nodes while never processing the same one twice**. The skeleton is always: a frontier (stack/queue/heap), a `visited` marker, and a rule for expanding neighbors. What changes is *the frontier's ordering*:

- **Stack / recursion (DFS)** — go deep first. Components, cycle detection, topo sort, path existence.
- **Plain queue (BFS)** — expand in rings of equal distance. **Unweighted shortest path** falls out for free: the first time you reach a node is via a shortest path.
- **Min-heap / priority queue (Dijkstra)** — always expand the cheapest-so-far node. **Weighted shortest path** with non-negative edges.

**The single most important habit: mark visited at the right moment.** For BFS shortest path, mark a node visited **when you enqueue it** (not when you dequeue), or you'll enqueue duplicates and blow up. For DFS components, mark on entry.

**Representations:**
- **Adjacency list** `List<int>[]` or `Dictionary<int, List<int>>` — default; O(V+E) space, iterate neighbors cheaply.
- **Adjacency matrix** `int[V][V]` — dense graphs / O(1) edge lookup (LC 547 sometimes given this way).
- **Implicit** — neighbors computed on the fly (grid: 4 directions; build adjacency from `edges`/`routes` first).

---

## 3. Core patterns

### A. Flood fill / connected components (DFS or BFS)
Count or label regions. Walk from each unvisited node, marking everything reachable. Component count = number of fresh starts. (LC 200 Islands, LC 547 Provinces.)

### B. Multi-source BFS
Seed the queue with **all** sources at once, then BFS. Distances come out as "min distance to *any* source." (LC 994 Rotting Oranges — all rotten cells start at time 0.)

### C. Topological sort (directed acyclic ordering)
Order nodes so every edge points forward. Two methods:
- **Kahn's (BFS):** compute in-degrees, queue all in-degree-0 nodes, repeatedly pop and decrement neighbors. If you emit fewer than V nodes → a **cycle** exists. (LC 207/210.)
- **DFS post-order:** finish-order reversed = topo order; a gray/black coloring detects cycles.

### D. Cycle detection
- **Directed:** DFS with 3 colors (white/gray/black) — a back-edge to a gray node = cycle. Or Kahn's "didn't emit all nodes."
- **Undirected:** Union-Find (edge joining already-connected nodes = cycle), or DFS tracking parent. (LC 261 Valid Tree = connected ∧ exactly V−1 edges ∧ no cycle.)

### E. BFS shortest path (unweighted)
First arrival = shortest. Track distance per level (size-snapshot) or per node. For **implicit/state graphs**, the "node" may be a composite state. (LC 815 Bus Routes — BFS over stops *or* routes; LC 847 — state = (node, visited-bitmask).)

### F. Dijkstra (weighted, non-negative)
Min-heap of `(cost, node)`. Pop cheapest, relax neighbors, skip stale pops (`if cost > dist[node] continue`). (LC 743 Network Delay, LC 1631 Path With Minimum Effort — "cost" is max edge on path.)

### G. Bellman-Ford / cost-bounded BFS
Handles negative edges or a **hop limit**. Relax all edges K times for "at most K stops." (LC 787 Cheapest Flights Within K Stops.)

### H. Union-Find (Disjoint Set Union)
Near-O(1) "are these connected?" + "join". Path compression + union by rank. Dynamic connectivity, component counting, undirected cycle detection, Kruskal MST. (LC 547, 261, 305.)

---

## 4. Reusable templates (C#)

### Build adjacency list from edges
```csharp
var adj = new List<int>[n];
for (int i = 0; i < n; i++) adj[i] = new List<int>();
foreach (var e in edges) {
    adj[e[0]].Add(e[1]);
    adj[e[1]].Add(e[0]);   // omit this line for a DIRECTED graph
}
```

**What it does:** turns an edge list into neighbor lookups so `adj[u]` gives all of `u`'s neighbors in O(1) amortized. This is the first step for almost every non-grid problem. The one decision that matters: add **both** directions for an undirected graph (friendships, provinces), only **one** (`e[0] -> e[1]`) for a directed graph (course prereqs, flights). Getting this wrong silently breaks topo sort and cycle detection downstream.

### DFS components (grid flood fill)
```csharp
int rows = grid.Length, cols = grid[0].Length;
int[][] dirs = { new[]{1,0}, new[]{-1,0}, new[]{0,1}, new[]{0,-1} };

void Dfs(int r, int c) {
    if (r < 0 || r >= rows || c < 0 || c >= cols || grid[r][c] != '1') return;
    grid[r][c] = '0';                         // mark visited in-place
    foreach (var d in dirs) Dfs(r + d[0], c + d[1]);
}

int components = 0;
for (int r = 0; r < rows; r++)
    for (int c = 0; c < cols; c++)
        if (grid[r][c] == '1') { components++; Dfs(r, c); }
```

**What it does:** counts connected regions in a grid. Each outer-loop cell that's still `'1'` is a fresh region — we increment, then `Dfs` floods the whole region to `'0'` so it's never counted again. **Marking visited in-place** (`grid[r][c] = '0'`) avoids a separate `visited` array. The bounds check + `!= '1'` guard at the top of `Dfs` is the base case that stops recursion at walls, water, and already-visited land. Swap the four `dirs` for 8 if diagonals count. On very large grids, convert to the explicit-stack version below to avoid stack overflow.

### DFS components — explicit stack (no recursion)
```csharp
int rows = grid.Length, cols = grid[0].Length;
int[][] dirs = { new[]{1,0}, new[]{-1,0}, new[]{0,1}, new[]{0,-1} };

void DfsIter(int sr, int sc) {
    var stack = new Stack<(int r, int c)>();
    grid[sr][sc] = '0';                       // mark on PUSH (see note)
    stack.Push((sr, sc));
    while (stack.Count > 0) {
        var (r, c) = stack.Pop();
        foreach (var d in dirs) {
            int nr = r + d[0], nc = c + d[1];
            if (nr < 0 || nr >= rows || nc < 0 || nc >= cols || grid[nr][nc] != '1')
                continue;
            grid[nr][nc] = '0';               // mark BEFORE pushing
            stack.Push((nr, nc));
        }
    }
}

int components = 0;
for (int r = 0; r < rows; r++)
    for (int c = 0; c < cols; c++)
        if (grid[r][c] == '1') { components++; DfsIter(r, c); }
```

**What it does:** the exact same flood fill as the recursive version, but the call stack is replaced by an **explicit `Stack<T>`** — so it can't blow the runtime stack on a huge grid (a snake-shaped or all-land grid of ~10⁶ cells is the killer case for recursion). The mechanics: pop a cell, look at its four neighbors, and push each land neighbor after flipping it to `'0'`.

The one subtlety that trips people up: **mark visited at push time, not pop time.** If you only mark when you *pop*, the same cell can be pushed many times by different neighbors before it's ever popped, so the stack balloons and you may double-count work. By writing `grid[nr][nc] = '0'` immediately before `Push`, each cell enters the stack exactly once — the iterative analogue of "mark on enqueue" in BFS. (The start cell is marked the same way before the initial push.) Note this is still DFS in spirit even though we don't recurse: a `Stack` gives the same deep-first frontier ordering that recursion's call stack does; swap the `Stack` for a `Queue` and the identical code becomes BFS. Same O(V+E) time and O(V) worst-case auxiliary space as the recursive form — but the space is now heap-allocated and bounded by your memory, not the (much smaller) thread stack.

### BFS shortest path (unweighted)
```csharp
var q = new Queue<int>();
var dist = new int[n]; Array.Fill(dist, -1);
q.Enqueue(src); dist[src] = 0;
while (q.Count > 0) {
    int u = q.Dequeue();
    foreach (int v in adj[u]) {
        if (dist[v] == -1) {                  // mark on ENQUEUE
            dist[v] = dist[u] + 1;
            q.Enqueue(v);
        }
    }
}
```

**What it does:** finds the fewest-edges distance from `src` to every node. BFS expands in concentric rings, so **the first time a node is reached is along a shortest path** — no re-relaxation needed (that's the unweighted win over Dijkstra). The `dist[v] == -1` test doubles as the `visited` check *and* the distance initializer. Critical detail: we set `dist[v]` and enqueue **in the same breath** — marking on enqueue, not dequeue — so a node never enters the queue twice. `-1` means unreached at the end.

### Multi-source BFS (seed all sources)
```csharp
var q = new Queue<(int r, int c)>();
foreach source cell -> q.Enqueue(cell);       // all at distance 0
// then standard BFS; first arrival = min distance to any source
```

**What it does:** computes the min distance from the *nearest* of many sources simultaneously. By seeding **all** sources at distance 0 before the loop starts, the rings expand outward from every source at once, so the first arrival at any cell is its distance to the closest source. This is strictly better than running single-source BFS from each source separately (which would be O(sources · (V+E))). The classic use is LC 994 Rotting Oranges: all rotten cells start the clock together, and the answer is the last cell's arrival time (or -1 if a fresh cell is never reached).

### Topological sort — Kahn's (BFS)
```csharp
var indeg = new int[n];
foreach (var e in edges) indeg[e[1]]++;       // edge u -> v
var q = new Queue<int>();
for (int i = 0; i < n; i++) if (indeg[i] == 0) q.Enqueue(i);
var order = new List<int>();
while (q.Count > 0) {
    int u = q.Dequeue(); order.Add(u);
    foreach (int v in adj[u]) if (--indeg[v] == 0) q.Enqueue(v);
}
bool hasCycle = order.Count != n;             // didn't emit all -> cycle
```

**What it does:** linearizes a directed graph so every edge points forward (prerequisites before dependents). **In-degree** = number of unmet prerequisites; a node is ready to emit only when its in-degree hits 0. We start with all already-ready nodes, and each time we emit one we "satisfy" its outgoing edges by decrementing neighbors' in-degrees, queuing any that reach 0. The cycle test is the elegant part: in a cycle, no node in it ever reaches in-degree 0, so they never get emitted — if `order.Count != n`, a cycle exists and **no valid ordering is possible** (LC 207 returns false; LC 210 returns empty).

### Dijkstra (weighted, non-negative)
```csharp
var dist = new int[n]; Array.Fill(dist, int.MaxValue);
var pq = new PriorityQueue<int, int>();       // (node, cost)
dist[src] = 0; pq.Enqueue(src, 0);
while (pq.Count > 0) {
    pq.TryDequeue(out int u, out int d);
    if (d > dist[u]) continue;                // stale entry, skip
    foreach (var (v, w) in adj[u]) {
        if (dist[u] + w < dist[v]) {
            dist[v] = dist[u] + w;
            pq.Enqueue(v, dist[v]);
        }
    }
}
```

**What it does:** finds the least-*total-cost* path with non-negative weights. The min-heap always hands you the cheapest-so-far node, and because edges are non-negative, **the first time you pop a node its distance is final** — that's the invariant that makes Dijkstra correct. The `if (d > dist[u]) continue` line is essential: C#'s `PriorityQueue` can't decrease-key, so we leave stale `(node, oldCost)` entries in the heap and just skip them when popped. "Relaxing" an edge means: if routing through `u` beats `v`'s best known cost, update it and push the new entry. For variants where path cost isn't a sum (LC 1631, cost = max edge on the path), replace `dist[u] + w` with `Math.Max(dist[u], w)`.

### Union-Find (path compression + union by rank)
```csharp
int[] parent, rank_;
int Find(int x) => parent[x] == x ? x : parent[x] = Find(parent[x]); // path compression
bool Union(int a, int b) {
    int ra = Find(a), rb = Find(b);
    if (ra == rb) return false;               // already joined -> cycle
    if (rank_[ra] < rank_[rb]) (ra, rb) = (rb, ra);
    parent[rb] = ra;
    if (rank_[ra] == rank_[rb]) rank_[ra]++;
    return true;
}
```

**What it does:** answers "are these two nodes in the same group?" and "merge their groups" in near-constant time. `Find` returns a component's representative (root); two nodes are connected iff they share a root. **Path compression** (`parent[x] = Find(parent[x])`) flattens the tree on every lookup so future finds are O(1); **union by rank** attaches the shorter tree under the taller to keep depth low. `Union` returning `false` means both nodes were *already* connected — joining them would close a cycle, which is exactly the undirected cycle-detection signal (LC 261: a valid tree has `n-1` edges and every `Union` succeeds). Initialize `parent[i] = i` and `rank_[i] = 0` before use.

### Bellman-Ford / K-stops relaxation (LC 787)
```csharp
var dist = new int[n]; Array.Fill(dist, int.MaxValue); dist[src] = 0;
for (int i = 0; i <= K; i++) {                // at most K stops = K+1 edges
    var tmp = (int[])dist.Clone();            // use last round's dist only
    foreach (var (u, v, w) in flights)
        if (dist[u] != int.MaxValue && dist[u] + w < tmp[v]) tmp[v] = dist[u] + w;
    dist = tmp;
}
```

**What it does:** finds the cheapest path using **at most K stops** (K+1 edges). Each round of the outer loop relaxes *every* edge once, which extends every shortest path by one more edge — so after round `i`, `dist` holds the cheapest cost reachable in `i` edges. The subtle, must-not-skip detail is `tmp = dist.Clone()`: we read from **last round's** distances and write into a fresh copy, so a single round can't chain multiple edges together and accidentally exceed the K-edge budget. Unlike Dijkstra, this tolerates negative edge weights (it just can't have negative *cycles*). Looping `K+1` times because "K stops" allows K+1 flights.

---

## 5. Common pitfalls

- **Marking visited on dequeue instead of enqueue (BFS)** → the same node enters the queue many times; can blow memory/time and corrupt distances. Mark when you *add* it.
- **No `visited` set at all** → infinite loops on cycles. Trees let you skip it; graphs never do.
- **Directed vs undirected mix-up** — adding both edge directions on a directed graph (or vice versa) silently breaks topo sort / cycle logic.
- **Dijkstra with negative edges** → wrong answer. Use Bellman-Ford. And don't forget the **stale-pop skip** (`if d > dist[u] continue`) or it's slow.
- **K-stops without snapshotting last round** (LC 787) — relaxing within the same array lets a path use >K edges in one pass. Clone `dist` each round.
- **Recursion depth on big grids** — DFS flood fill on a 10⁶-cell grid can overflow the stack; switch to iterative BFS/stack.
- **Building adjacency wrong for implicit graphs** — LC 815: connect *stops to routes* (bipartite) or precompute route-to-route adjacency; naive stop→stop is O(stops²) and TLEs.
- **Counting components: forgetting to start only from unvisited nodes** → over-counts.
- **Topo sort: not detecting the cycle** — if `order.Count != n`, there's no valid ordering; many problems hinge on returning that.

---

## 6. Complexity cheat sheet

| Algorithm | Time | Space | Use for |
|---|---|---|---|
| DFS / BFS traversal | O(V + E) | O(V) | components, reachability, unweighted dist |
| Multi-source BFS | O(V + E) | O(V) | min dist to any source |
| Topological sort (Kahn / DFS) | O(V + E) | O(V) | dependency ordering, cycle detect |
| Dijkstra (binary heap) | O((V + E) log V) | O(V) | weighted shortest path, non-neg |
| Bellman-Ford | O(V · E) | O(V) | negative edges, K-hop limit |
| Union-Find (with both opts) | ~O(α(N)) per op | O(N) | dynamic connectivity, undirected cycle |

`V` = vertices, `E` = edges, `α` = inverse Ackermann (≈ constant).

---

## 7. Map to the problem set (Topic 11 — Graphs, 15 problems)

| # | LC | Pattern from above |
|---|---|---|
| 73 | 200 Number of Islands | A — grid flood fill (★) |
| 74 | 133 Clone Graph | DFS/BFS + node→clone map (cf. LC 138) |
| 75 | 207 Course Schedule | C/D — topo sort cycle detection (★) |
| 76 | 210 Course Schedule II | C — topo sort, emit order |
| 77 | 994 Rotting Oranges | B — multi-source BFS (⚑) |
| 78 | 261 Graph Valid Tree | D/H — connected ∧ V−1 edges ∧ acyclic |
| 79 | 547 Number of Provinces | A/H — components via DFS or Union-Find |
| 80 | 743 Network Delay Time | F — Dijkstra (★) |
| 81 | 1631 Path With Minimum Effort | F — Dijkstra, cost = max edge on path |
| 82 | 815 Bus Routes | E — BFS on implicit stop/route graph (★ ⚑) |
| 83 | 269 Alien Dictionary | C — build edges from adjacent words + topo sort (★ ⚑) |
| 84 | 332 Reconstruct Itinerary | Hierholzer's Eulerian path (⚑) |
| 85 | 847 Shortest Path Visiting All Nodes | E — BFS over (node, visited-bitmask) state (★ ⚑) |
| 86 | 305 Number of Islands II | H — Union-Find with incremental adds (R) |
| 87 | 787 Cheapest Flights Within K Stops | G — Bellman-Ford / cost-bounded BFS (R) |

Related elsewhere: grid DFS reuses backtracking (Topic 12); Dijkstra reuses the heap (Topic 7); the trie-pruned grid DFS (LC 212) is the same 4-direction walk.
