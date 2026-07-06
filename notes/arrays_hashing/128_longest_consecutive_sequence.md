# LC 128 — Longest Consecutive Sequence
- **Pattern:** <HashSet the elements. Only start walk when `n - 1` is not present>
- **Brute force:** <Didn't bother> — O(n log n) time, O(1) space
- **Optimized:** <HashSet all nums; for each value whose predecessor (n-1) is absent, walk upward n, n+1, … counting the run length; track the max> — O(n) time, O(n) space
- **Key insight:** <HashSet the elements. Only start walk when `n - 1` is not present; Can initialize HashSet straight to HashSet<int>(nums); Iterate hashset straight away for speed>
- **Edge cases I had to handle:** <Empty array → returns 0>
- **Where I got stuck and for how long:** <Thinking of how to get around duplicates and how to not iterate consecutive sequences multiple times for each member.>
- **Template fragments I reused:** <Hashset + walking>
- **C# notes:** <new HashSet<int>(nums) dedups in the constructor; iterate the set directly (not nums) so duplicates are skipped and Contains is O(1)>
- **Would I solve this in 25 min cold next week?** <Y/N> Maybe, still in tracker for D+7
