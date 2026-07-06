# LC <98> — <Validate Binary Search Tree>
- **Pattern:** <Tree DFS with inherited (low, high) bounds passed down the recursion>
- **Brute force:** <At each node scan its whole left subtree for a max and right subtree for a min to check order — O(n²), and only catches immediate-subtree violations, not ancestor bounds.>
- **Optimized:** <Recurse carrying (low, high): node must satisfy low < val < high; go left with high = node.val, right with low = node.val. Use `long.MinValue`/`long.MaxValue` sentinels so int-boundary node values still pass.> — O(n) time, O(h) space
- **Key insight:** <Each node is constrained by an open interval inherited from ALL ancestors, not just its parent — tighten the upper bound going left, the lower bound going right. `long` bounds sidestep the int.MinValue/int.MaxValue node-value edge case.>
- **Edge cases I had to handle:** <Equal values invalid (strict `<=`/`>=`); node values at int.MinValue/int.MaxValue handled by using `long` bounds; null → valid.>
- **Where I got stuck and for how long:** <First attempt compared each node only against its immediate subtree min/max — O(n²) AND unsound (only enforces local order, not ancestor bounds). Needed the bounds-passing approach.>
- **Template fragments I reused:** <BST bounds recursion; alternative is inorder-must-be-increasing.>
- **Would I solve this in 25 min cold next week? Y/N> 
