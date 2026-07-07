# LC <104> — <Maximum Depth of Binary Tree>
- **Pattern:** <Tree DFS recursion — depth = 1 + max(left depth, right depth)>
- **Brute force:** <No asymptotically worse approach — any full traversal visits all n nodes once; the iterative BFS level-count is the same O(n).>
- **Optimized:** <Recurse: null → 0, else `1 + Math.Max(MaxDepth(left), MaxDepth(right))`.> — O(n) time, O(h) space (call stack)
- **Key insight:** <"My depth is 1 plus the deeper of my two children." Iterative BFS alternative: count levels via the queue-size snapshot.>
- **Edge cases I had to handle:** <Null root → 0.>
- **Where I got stuck and for how long:** <Clean first solve.>
- **Template fragments I reused:** <Tree DFS height recursion.>
- **Would I solve this in 25 min cold next week? Y/N> Y
