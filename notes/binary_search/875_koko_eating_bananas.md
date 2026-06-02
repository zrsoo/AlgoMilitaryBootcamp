# LC 875 — Koko Eating Bananas
- **Pattern:** Binary Search on Answer
- **Brute force:** <->
- **Optimized:** <The answer is for sure in the [1, max(piles)] interval. For a candidate speed of k, we can compute the hours it'd take Koko to eat all bananas in O(n) by iterating over all piles and ``hours += (int)Math.Ceiling((double)piles[i] / k)``. We can also determine if our speed is too low ``hours > h``, or too high (or potentially right): ``hours <= h``. If ``hours > h`, speed is too low so move l = mid + 1. If `hours <= h`, speed might be right, but we have to keep searching for the minimum speed, so do `r = mid`.>
- **Key insight:** <Whenever the problem reveals the interval that the result is in, and allows us to assess if a candidate result is higher or lower than the correct result (in max O(n) complexity), we can employ binary search and just look for it.>
- **Edge cases I had to handle:** <None>
- **Where I got stuck and for how long:** <Honestly, if the problem weren't tagged as Binary Search, I would've never figured it out. After a brute-force attempt, I figured that if we have interval + a way of determining if result is too high or too low, we can just binary search for it.>
- **Template fragments I reused:** <Binary Search to Minimum on Answer>
- **Would I solve this in 25 min cold next week? Y/N** <Maybe>
