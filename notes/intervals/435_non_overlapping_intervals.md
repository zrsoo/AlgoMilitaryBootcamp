# LC <435> — <Non-overlapping Intervals>
- **Pattern:** <Intervals / greedy — sort by start, keep the interval that ends earliest on overlap>
- **Brute force:** <Try every subset of intervals to remove and check the remainder is conflict-free, or DP on sorted intervals (longest non-overlapping chain, then remove the rest) — O(2^n) exhaustive or O(n^2) DP.>
- **Optimized:** <Sort intervals by start time. Keep a ``prevEnd`` variable, preservint the end of the previous intervals. Initialize it to the end of the first interval, and begin parsing from the second, left to right. If we find an overlap, increment answer (we're greedily removing one of the intervals). However, we preserve ``prevEnd`` as the minimum between itself and the end of the found overlapping interval. By preserving the minimum previous end, we remove a minimum number of intervals, because we eliminate large prevEnds that would have caused more overlaps than needed.> — O(n log n) time, O(1) space
- **Key insight:** <On an overlap, keep the interval that ends earliest (min end) — it leaves the most room for later intervals, so removals stay minimal.>
- **Edge cases I had to handle:** <Must sort by start first; a single interval means zero removals; touching endpoints (prevEnd == start) are not treated as overlaps.>
- **Where I got stuck and for how long:** <Couldn't solve without hints.>
- **Template fragments I reused:** <Intervals greedy>
- **Would I solve this in 25 min cold next week? Y/N> 
