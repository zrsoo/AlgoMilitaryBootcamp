# LC <435> — <Non-overlapping Intervals>
- **Pattern:** <Intervals / greedy — sort by start, keep the interval that ends earliest on overlap>
- **Brute force:** < >
- **Optimized:** <Sort intervals by start time. Keep a ``prevEnd`` variable, preservint the end of the previous intervals. Initialize it to the end of the first interval, and begin parsing from the second, left to right. If we find an overlap, increment answer (we're greedily removing one of the intervals). However, we preserve ``prevEnd`` as the minimum between itself and the end of the found overlapping interval. By preserving the minimum previous end, we remove a minimum number of intervals, because we eliminate large prevEnds that would have caused more overlaps than needed.> — O(n log n) time, O(1) space
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** <Couldn't solve without hints.>
- **Template fragments I reused:** <Intervals greedy>
- **Would I solve this in 25 min cold next week? Y/N> 
