# LC <98> — <Validate Binary Search Tree>
- **Pattern:** <Tree DFS with inherited (low, high) bounds passed down the recursion>
- **Brute force:** < >
- **Optimized:** < > — O(n) time, O(h) space
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** <First attempt compared each node only against its immediate subtree min/max — O(n²) AND unsound (only enforces local order, not ancestor bounds). Needed the bounds-passing approach.>
- **Template fragments I reused:** <BST bounds recursion; alternative is inorder-must-be-increasing.>
- **Would I solve this in 25 min cold next week? Y/N> 
