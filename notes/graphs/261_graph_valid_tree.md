# LC <261> — <Graph Valid Tree>
- **Pattern:** <Graphs — undirected validity: edge-count gate (`n-1`) + connectivity check (DFS/BFS or union-find)>
- **Brute force:** <Union-find: union each edge, reject a re-union (cycle), then verify a single component — O(E·α(n))>
- **Optimized:** <Gate on `edges.Length == n-1` (a tree must have exactly n-1 edges). Build an undirected adjacency list, DFS from node 0 counting reachable nodes, and require that count == n (fully connected). n-1 edges + connected ⇒ acyclic ⇒ valid tree. O(V + E) time, O(V + E) space>
- **Key insight:** <A tree on n nodes has exactly n-1 edges and is connected; with the edge-count gate, connectivity alone (reaching all n from node 0) implies acyclic>
- **Edge cases I had to handle:** <reject early if `edges.Length != n-1`; build adjacency in both directions (undirected); count reachable nodes from 0 and compare to n>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <DFS reachable-node count / connected components>
- **Would I solve this in 25 min cold next week? Y/N> 
