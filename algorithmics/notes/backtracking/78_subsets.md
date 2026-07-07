# LC <78> — <Subsets>
- **Pattern:** <Backtracking — include/exclude per element (power set); record at EVERY node, `start` cursor, recurse `j+1`>
- **Brute force:** <Iterate bitmasks 0..2ⁿ-1; for each mask include element i when bit i is set. O(n·2ⁿ) time.>
- **Optimized:** <Backtracking: record `curr` at EVERY node, loop from `start` to end, add `nums[start]`, recurse with `start+1`, then remove. O(n·2ⁿ) time, O(n) recursion depth.>
- **Key insight:** <Recording at every recursion node (not only leaves) emits all 2ⁿ subsets; the `start` cursor never revisits earlier indices, so no duplicate or reordered subsets.>
- **Edge cases I had to handle:** <Empty input still records the empty subset; snapshot each subset with `new List<int>(curr)` before storing.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Choose → recurse(start+1) → un-choose with a start cursor.>
- **Would I solve this in 25 min cold next week? Y/N> 
