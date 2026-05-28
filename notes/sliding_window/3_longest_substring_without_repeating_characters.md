# LC 3 — Longest Substring Without Repeating Characters
- **Pattern:** <Sliding Winwdow, Hashing>
- **Brute force:** <Didn't bother> — O() time, O() space
- **Optimized:** <Keep a hash array of letters that are currently in the substring within the window. Initialize two pointers, `l = 0` and `r = 0` to define the window. While no duplicates in the window, advance right and compare `l - r` to `maxLength`. While there are duplicates, advance left and make sure to keep pointers within the array > — O(n) time, O(1) space
- **Key insight:** <Sliding window + Hashing to make sure no duplicates. Hash Array of 201 length can hold all symbols of the problem. (and is faster than a hashset)>
- **Edge cases I had to handle:** <None>
- **Where I got stuck and for how long:** <Smooth>
- **Template fragments I reused:** <Sliding Window>
- **C# notes:** <>
- **Would I solve this in 25 min cold next week?** <Y/N> Think so, yeah
