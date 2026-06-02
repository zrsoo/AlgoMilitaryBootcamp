# LC <84> — <Largest Rectangle in Histogram>
- **Pattern:** <Stack — Monotonic Increasing (indices); "next smaller on left & right" in one pass>
- **Brute force:** <Expand two indices left and right beginning from each index i. O(n^2) time>
- **Optimized:** <Keep monotonic increasing stack (pop while element is smaller than peek) of indexes. While popping, it means that the current element is smaller than the top of the stack, i.e. we've found the end of the top stack's rectangle. We already know where it begins. Since we have an increasing monotonic stack, the rectangle begins at the index under peek. So, at each step, while `arr[i] < arr[stack.Peek()]`, we pop the element's index and get its value, and compute area as `value * (i - stack.Peek() - 1)` and store that in a maxArea.>> — O(n) time, O(n) space
- **Key insight:** <For every bar i, the larges rectangle you can form with it begins at the first bar smaller than it to the left, and ends at the first bar smaller than it to the right. Exactly the flow that a monotonic stack goes through.>
- **Edge cases I had to handle:** <Two edge cases: first - Append a 0 to the array so that the last elements are processed correctly (popped and their areas are computed). second - when popping the last element in the stack (the first one in the array), the stack won't have any left elements, so doing stack.Peek() will throw. Also, we can't use `i` in the area computation since it's 0. Conditionally compute maxArea, and when the stack is emtpy, the current area is `el * 1` if `i == 0`, or `el * i` otherwise.>
- **Where I got stuck and for how long:** <Couldn't find optimal approach>
- **Template fragments I reused:** <Monotonic stack of indexes>
- **Would I solve this in 25 min cold next week? Y/N> Probably, added to tracker
