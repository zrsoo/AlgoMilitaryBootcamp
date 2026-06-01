# LC <739> — <Daily Temperatures>
- **Pattern:** <Stack — Monotonic Decreasing (indices)>
- **Brute force:** <O(n^2), for each temp, look for the first one that's bigger.>
- **Optimized:** <Use monotonic decreasing stack. Iterate the array. While the stack is not empty and the top element's temp is less than current temp, pop stack and mark result[el.Key] = i - el.Key. Meaning that whenever we find a warmer temp, we pop all previously colder temps and compute the distance in the original array between them. Push KVPs with index as key and temperatures as values> — O(n) time, O(n) space
- **Key insight:** <Monotonic Decreasing Stack>
- **Edge cases I had to handle:** <None>
- **Where I got stuck and for how long:** <Smooth>
- **Template fragments I reused:** <Monotonic Decreasing Stack>
- **Would I solve this in 25 min cold next week? Y/N> Y 
