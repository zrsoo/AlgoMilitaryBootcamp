# LC <102> — <Binary Tree Level Order Traversal>
- **Pattern:** <Tree BFS — process one level at a time using a queue-size snapshot>
- **Brute force:** <Compute the tree height, then for each depth run a separate DFS collecting only the nodes at that depth — O(n·h).>
- **Optimized:** <Queue seeded with root. Each outer iteration: snapshot `levelSize = q.Count`, loop exactly that many times dequeuing nodes into a `level` list and enqueuing their children. Append `level` to result.> — O(n) time, O(n) space
- **Key insight:** <Snapshot the queue size BEFORE the inner loop — that count is exactly the current level. Children enqueued mid-loop wait for the next outer pass. Don't re-read `q.Count` in the for condition.>
- **Edge cases I had to handle:** <Null root → empty result (guard the initial enqueue).>
- **Where I got stuck and for how long:** <Clean first solve.>
- **Template fragments I reused:** <BFS level-snapshot skeleton (also Right Side View, zigzag, level averages).>
- **Would I solve this in 25 min cold next week? Y/N> Y
