# LC 128 — Longest Consecutive Sequence
- **Pattern:** <HashSet the elements. Only start walk when `n - 1` is not present>
- **Brute force:** <Didn't bother> — O() time, O() space
- **Optimized:** <> — O(n) time, O(n) space
- **Key insight:** <HashSet the elements. Only start walk when `n - 1` is not present; Can initialize HashSet straight to HashSet<int>(nums); Iterate hashset straight away for speed>
- **Edge cases I had to handle:** <>
- **Where I got stuck and for how long:** <Thinking of how to get around duplicates and how to not iterate consecutive sequences multiple times for each member.>
- **Template fragments I reused:** <Hashset + walking>
- **C# notes:** <>
- **Would I solve this in 25 min cold next week?** <Y/N> Maybe, still in tracker for D+7
