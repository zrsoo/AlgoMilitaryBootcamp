# LC 974 — Subarray Sums Divisible by K
- **Pattern:** <Prefix sum + Modulo Hashing>
- **Brute force:** <Didn't bother> — O(n²) time, O(1) space
- **Optimized:** <Hold hash array of modulos, seed it with `[0] = 1`. No need to hold sum, just hold a running modulo (since it cycles anyway) At each step, add `((nr % k) + k) % k` to the rolling modulo. This is to always have a positive modulo being added (it can be negative for neg numbers). Look for newly computed modulo in hash array, increment result with what's there, add + 1 to the computed modulo in the hash array> — O(n) time, O(n) space
- **Key insight:** <Use the fact that for (a - b) to divide k, a % k must be equal to b % k>
- **Edge cases I had to handle:** <None>
- **Where I got stuck and for how long:** <Modulo is confusing>
- **Template fragments I reused:** <Prefix sum + Modulo Hashing>
- **C# notes:** <int[k] array as modulo buckets seeded with [0]=1; `(prefixMod + nr % k + k) % k` forces a non-negative remainder since C# `%` can return negative for negative operands>
- **Would I solve this in 25 min cold next week?** <Y/N> Maybe, still in tracker for D+7
