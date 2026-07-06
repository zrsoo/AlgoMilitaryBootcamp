# LC <133> — <Clone Graph>
- **Pattern:** <Graphs — deep copy via Pre-Order DFS + original→clone hashmap>
- **Brute force:** <DFS copy without a clone cache — infinite-loops on cycles and re-clones shared neighbors>
- **Optimized:** <Hold a `Dictionary<Node, Node>`. This serves both as visited and as clone cache. Pre-Order DFS over the nodes. If dictionary contains the node, return immediately. Else, initialize new node and put it in clone dict, then iterate over the neighbors of the original node and add the result of the recursive call to the neighbors of the clone. At the end, return the clone. O(N + E) time - N = nr nodes; E = edges. O(N) space>
- **Key insight:** <Register the clone in the map BEFORE recursing into neighbors — that breaks cycles and dedups shared nodes>
- **Edge cases I had to handle:** <null input returns null; self-loops / cycles handled by pre-registering the clone before descending>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <DFS with visited/clone hashmap>
- **Would I solve this in 25 min cold next week? Y/N> 
