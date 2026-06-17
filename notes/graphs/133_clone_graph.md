# LC <133> — <Clone Graph>
- **Pattern:** <Graphs — deep copy via Pre-Order DFS + original→clone hashmap>
- **Brute force:** < >
- **Optimized:** <Hold a `Dictionary<Node, Node>`. This serves both as visited and as clone cache. Pre-Order DFS over the nodes. If dictionary contains the node, return immediately. Else, initialize new node and put it in clone dict, then iterate over the neighbors of the original node and add the result of the recursive call to the neighbors of the clone. At the end, return the clone. O(N + E) time - N = nr nodes; E = edges. O(N) space>
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N> 
