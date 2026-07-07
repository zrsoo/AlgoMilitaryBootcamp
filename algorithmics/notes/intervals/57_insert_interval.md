# LC <57> — <Insert Interval>
- **Pattern:** <Intervals — insert by start position, then sweep and merge overlapping>
- **Brute force:** <->
- **Optimized:** <Build an intermediate list: copy every interval whose start is <= newInterval's start, then add newInterval, then append the remaining intervals — this keeps the list sorted by start. Do one merge pass over it: hold `curr`; while `curr[1] >= next[0]` extend `curr[1] = Max(curr[1], next[1])`, otherwise push `curr` and set `curr = next`. Push the final `curr`.> — O(n) time, O(n) space
- **Key insight:** <Inserting newInterval at its sorted start position keeps the array ordered, so a single linear merge pass collapses all overlaps — no need to merge during insertion.>
- **Edge cases I had to handle:** <Empty input (return just newInterval); newInterval landing at the front of the array.>
- **Where I got stuck and for how long:** <Tried to merge when inserting, which was useless because we do another merge pass anyway. Also, when inserting, edge case of inserting at the beginning of the array caught me and I spent significant time on it.>
- **Template fragments I reused:** <Interval merge pass (extend end while overlap, else push and advance).>
- **Would I solve this in 25 min cold next week? Y/N> 
