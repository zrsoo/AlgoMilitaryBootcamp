# LC 239 — Sliding Window Maximum
- **Pattern:** <Sliding Window + Monotonic Deque>
- **Brute force:** <Don't bother> — O() time, O() space
- **Optimized:** <Work with a monotonic deque (C# - LinkedList) storing indexes. The main theory is that when a new element comes in, it makes all smaller previous elements irrelevant. The deque will always contain the index of the greatest in-window element on `.First`. So, you can iterate the window with a for loop, as the window size is fixed k, the window will be [r - k + 1, r]. Evict from the front of the deque, which is holding the current maximum index, if `r - k + 1 > deque.First`, which means that the window has moved past that index. Then, evict from the back of the deque while the current element, array[r] is greater than the element at the back of the deque: deque.Last. Then, add the index at the back of the deque. Only add to result if r + 1 >= k, first window forms at r + 1 == k> — O(n) time, O(k) space
- **Key insight:** <Monotonic deque in sliding window to keep track of maximums>
- **Edge cases I had to handle:** <None>
- **Where I got stuck and for how long:** <Found brute-force with SortedList, thought SortedList has O(log(n)) inserts and uses Red-Black trees behind the scenes, but turns out that it uses two arrays instead. Only SortedDict has Red-Black trees, but SortedDict can't access last element. With editorial, managed to solve with deque>
- **Template fragments I reused:** <Sliding Window + Monotonic Deque>
- **C# notes:** <SortedList doesn't give O(log(n)) inserts because it doesn't use red-black trees. Sorted dict does but it can't access the last (greater) element>
- **Would I solve this in 25 min cold next week?** <Y/N> Don't know, added to tracker
