# LC <124> — <Binary Tree Maximum Path Sum>
- **Pattern:** <Post-order DFS returning best single-arm downward gain; a global max records the two-arm "split" path that peaks at each node>
- **Brute force:** < >
- **Optimized:** <Recurse, carrying maximum sum across left and right arms. At the same time, keep a global accumulator that factors in the max sum at each node if we BRANCH off that node.> — O(N) time, O(h) space - h = height of the tree (log(n) <= h <= nr nodes)>>
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N>
