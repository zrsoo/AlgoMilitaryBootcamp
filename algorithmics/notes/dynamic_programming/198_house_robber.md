# LC 198 — House Robber
- **Pattern:** 1-D DP — linear "take vs skip" with no two adjacent
- **Brute force:**
- **Optimized:** — <Keep dp[n], with dp[i] being the max you can rob up to position i. For position 1, dp[i] = nums[i]. For 2, dp[2] = maximum between nums[1], and nums[2]. From then onward, the maximum amount you can rob is the maximum between: what you have robbed up until two houses ago + the current house VS. what you have robbed up until one house ago. Reasoning is you can't rob two adjacent houses, so you either skip the one just before and rob the current one, or rob the one just before, but then you can no longer rob the current one.> O(n) time, O(n) space
- **Key insight:**
- **Edge cases I had to handle:**
- **Where I got stuck and for how long:**
- **Template fragments I reused:**
- **Would I solve this in 25 min cold next week? Y/N**
