# LC 410 — Split Array Largest Sum
- **Pattern:** Binary Search on Answer
- **Brute force:** <->
- **Optimized:** <Answer is in the interval [max(array), sum(array)]. We're binary searching. For a given mid, greedily parse the array, computing a rolling sum. If sum exceeds mid, increment a subarrayCount variable. We're keeping track of the number of subarrays we need to split the array into, such that the sum of each subarray does NOT exceed mid. If the subarray count is <= k, it means that we've found a count of subarrays less than or equal to k s.t. their sums don't exceed mid (mid was possibly too big). As such, we can come up with a smaller sum, extending the subarray count: ``r = mid`` (so we continue the search for a lower mid). Otherwise, the reverse is true: mid was too small, so the subarray count exceeded k, in which case we make mid bigger with ``l = mid + 1``.>
- **Key insight:** <->
- **Edge cases I had to handle:** <->
- **Where I got stuck and for how long:** <The for loop keeping track of subarrays is tricky. Couldn't figure out how to process the element I'm actually on and I was counting them wrong>
- **Template fragments I reused:** <Binary Search on Answer>
- **Would I solve this in 25 min cold next week? Y/N** <Maybe, added to tracker>
