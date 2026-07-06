# LC <124> — <Binary Tree Maximum Path Sum>
- **Pattern:** <Post-order DFS returning best single-arm downward gain; a global max records the two-arm "split" path that peaks at each node>
- **Brute force:** <For every node compute the best path bending through it by re-exploring all downward paths on each side — recomputes overlapping subtrees, O(n²).>
- **Optimized:** <Recurse, carrying maximum sum across left and right arms. At the same time, keep a global accumulator that factors in the max sum at each node if we BRANCH off that node.> — O(N) time, O(h) space - h = height of the tree (log(n) <= h <= nr nodes)>>
- **Key insight:** <Clamp each arm's gain at 0 (a negative subtree is skipped). "Best through this node" = node.val + left + right (a split that can't extend upward), but the value RETURNED up is node.val + max(left, right) since a parent can only pass through one arm.>
- **Edge cases I had to handle:** <All-negative trees — `best` seeded at int.MinValue so a lone negative node can still win; negative arms clamped to 0 via `Math.Max(gain, 0)`.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Post-order DFS returning one value while a global captures a wider combination (same shape as tree-diameter).>
- **Would I solve this in 25 min cold next week? Y/N>
