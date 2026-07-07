# LC <847> — <Shortest Path Visiting All Nodes>
- **Pattern:** <Graphs — multi-source BFS over (node, visited-bitmask) states; level = path length, answer when mask == full>
- **Brute force:** <Try every path / permutation of nodes (TSP-style) — factorial blowup>
- **Optimized:** <BFS over states (node, visited-bitmask). Seed the queue with all n start states (i, 1<<i) since we may begin anywhere. Process level by level: each level is one more edge; expand to neighbors, OR-ing their bit into the mask. The first time a dequeued state has mask == full ((1<<n)-1) return the current step count. A `seen` set on (node, mask) prevents revisiting. O(2^n · n²) time, O(2^n · n) space>
- **Key insight:** <State is (current node, set of visited nodes as a bitmask); nodes may be revisited, so plain node-BFS fails — the mask keeps the state space finite, and multi-source seeding covers 'start anywhere'>
- **Edge cases I had to handle:** <nodes may be revisited (track mask, not a plain visited set); start from every node; goal is mask == (1<<n)-1>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <BFS over (node, bitmask) states>
- **Would I solve this in 25 min cold next week? Y/N> 
