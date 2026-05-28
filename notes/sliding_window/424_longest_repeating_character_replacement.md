# LC 424 — Longest Repeating Character Replacement
- **Pattern:** <Sliding window + Frequency Count>
- **Brute force:** <Didn't bother> — O() time, O() space
- **Optimized:** <Two pointers, `l = 0` and `r = 0`. At each step, the gap to a candidate array is `length_subarray - max_freq` where maxFreq is the maximum frequency of characters in the array (you can keep rolling track of this with a freq array). While `gap <= k`, expand `r` and store `length = r - l + 1` in `maxLength`. If gap exceeds `k`, decrease the count of `arr[l]` and advance `l`> — O() time, O() space
- **Key insight:** <MaxFreq doesn't need resetting when advancing l, because it will at worst yield a fake-valid window of maxLength m where m was a previous valid one.>
- **Edge cases I had to handle:** <>
- **Where I got stuck and for how long:** <Found the core part of the solution, but my part was O(n^2) because I was resetting the search when advancing left (shrinking)>
- **Template fragments I reused:** <Sliding window>
- **C# notes:** <>
- **Would I solve this in 25 min cold next week?** <Y/N> Don't know, added to tracker
