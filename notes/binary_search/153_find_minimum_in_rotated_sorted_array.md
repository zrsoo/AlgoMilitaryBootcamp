# LC <153> — <Find Minimum in Rotated Sorted Array>
- **Pattern:** <Binary Search — pivot finding via comparison with right end>
- **Brute force:** <->
- **Optimized:** <Early exit if nums[l] < nums[r] -- we are in a sorted array, return nums[l]. Otherwise, while `l < r`, mid = (l + r) / 2, look at nums[mid] in comparison to nums[r]. If `nums[mid] > nums[r]` (nums[l] is also greater because of our early escape), that means our minimum is in the right half, so advance left to mid + 1. Otherwise, it means that the result is somewhere between `l` and `mid` (inclusive), so r = mid> — O(n) time, O(1) space
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** <Smooth>
- **Template fragments I reused:** <Binary Search>
- **Would I solve this in 25 min cold next week? Y/N> Possibly. 
