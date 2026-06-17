# LC <547> — <Number of Provinces>
- **Pattern:** <Graphs — connected components via DFS over adjacency matrix>
- **Brute force:** < >
- **Optimized:** <Keep track of visited nodes with a boolean visited array. Iterate over all nodes (rows) in the matrix, and, for each unvisited one, recurse: 1. mark node as visited; 2. recursively visit all connections of the node: a for loop recursing on the node if it is connected and unvisited. Similar to FillIslands, we visit all connected provinces. In the master for loop, increment nrProvnces each time we find an unvisited node. O(n * m) - time; O(n) - space>
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N> Y
