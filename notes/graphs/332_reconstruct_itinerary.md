# LC <332> — <Reconstruct Itinerary>
- **Pattern:** <Graphs — Eulerian path via Hierholzer's algorithm: greedy DFS over a lexicographically-ordered adjacency (min-heap per node), post-order push onto a stack, reverse at the end>
- **Brute force:** <Backtracking DFS trying tickets in lexical order, undo on dead ends — exponential worst case>
- **Optimized:** <Hierholzer's algorithm. Allows us to visit all edges of a graph i.e. find an Euler path within the graph. Keep a stack of nodes, an adjacency list and a result list. Initialize and build adjacency list (maintain a sorted data structure in the dict if order is importent i.e. a priority queue). Push the start node in the stack. While the stack still has elements, peek the top of the stack. If the peeked node still has outgoing edges, dequeue one, consume it (remove from adj list), and add the neighbor to the stack. Else, add node to result. Result will contain the Euler path in reverse. At the end, reverse result. O(E * log(E)); E - nr of edges; space O(V + E); V - nr vertices, E - nr edges>
- **Key insight:** <Hierholzer's algorithm finds the path that consumes all edges (tickets). Conditions for Euler graph: either all nodes have indeg == outdeg OR exactly ONE node has indeg - outdeg = 1 and ONE has outdeg - indeg = 1 AND all others are balanced.>
- **Edge cases I had to handle:** <a node with no outgoing tickets needs the `adj.ContainsKey` guard; always start at "JFK"; result is built in reverse (post-order pops) so reverse at the end>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Hierholzer / Euler path>
- **Hint level needed (L0 class / L1 category / L2 structural / L3 editorial):** < >
- **Would I solve this in 25 min cold next week? Y/N> Y
