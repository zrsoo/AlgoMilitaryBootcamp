# LC <56> — <Merge Intervals>
- **Pattern:** <Intervals — sort by start, sweep and merge overlapping>
- **Brute force:** <->
- **Optimized:** <Sort intervals by start. Track a `curr` interval, initialized to the first. Iterate: if `curr` overlaps the current interval (`curr.end >= intervals[i].start`), merge by extending `curr.end = max(curr.end, intervals[i].end)`. Otherwise there's a gap, so commit `curr` to the result and reset `curr` to `intervals[i]`. After the loop, commit the final `curr`.> — O(n log n) time, O(n) space
- **Key insight:** <Sorting intervals makes it easy to work with here.>
- **Edge cases I had to handle:** <If in the else I do ``result.Add(curr)``, and then ``curr = intervals[i]``, at the end of the for loop, curr will hold the last member of intervals[i] (not yet added to result). Make sure to add it.>
- **Where I got stuck and for how long:** <First tackle on this topic so I used resources, was smooth.>
- **Template fragments I reused:** <Sweep intervals>
- **Would I solve this in 25 min cold next week? Y/N> Y 
