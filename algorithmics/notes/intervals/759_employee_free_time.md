# LC <759> — <Employee Free Time>
- **Pattern:** <Intervals — flatten all employees' intervals, sort by start, merge, emit the gaps between merged blocks>
- **Brute force:** <Coverage/boolean sweep over unit cells, report uncovered runs (only works for small coordinates).>
- **Optimized:** <Flatten every employee's intervals into one list, sort by `start`. Merge overlapping (touching counts as overlapping: `current.end >= interval.start`). The free intervals are exactly the gaps between consecutive merged blocks: `[merged[i].end, merged[i+1].end.start]`.> — O(N log N) time, O(N) space (N = total intervals)
- **Key insight:** <The infinite leading/trailing ranges are excluded automatically by only emitting gaps strictly between merged blocks. Per-employee sortedness can be exploited via a k-way heap merge (LC 23) for O(N log k), but flatten-and-sort is cleaner to write.>
- **Edge cases I had to handle:** <Touching intervals (half-open ⇒ no free time at the seam, handled by `>=`); full coverage ⇒ empty result; single merged block ⇒ no gaps.>
- **Where I got stuck and for how long:** <Initial instinct was `schedule.Sort()` — doesn't compile (IList has no Sort) and sorting employees is meaningless; the right move is flatten then sort intervals.>
- **Template fragments I reused:** <Merge Intervals (LC 56) sweep; gap-emission mirrors interval complementation.>
- **Would I solve this in 25 min cold next week? Y/N> Y
