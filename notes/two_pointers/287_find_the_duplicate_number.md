# LC <287> — <Find the Duplicate Number>
- **Pattern:** <Fast & Slow Pointers - Floyd's Cycle Detection (Tortoise and Hare)>
- **Brute force:** <Two fors looking for duplicate> — O(n^2) time, O(1) space
- **Optimized:** <Think of the array as a linked list. The next node from node i is nums[i]. Use Floyd's Tortoise and Hare to detect cycle. Initialize a slow pointer, moving one step at at time, and a fast one, moving 2 steps at a time. They both start from 0. Record first point they meet at. Then, set fast to 0 again and move both pointers one step at a time until they meet again. The node at which they meet again is the entry point to the cycle> — O(n) time, O(1) space
- **Key insight:** <View the array as a linked list and use Tortoise and Hare algorithm.>
- **Edge cases I had to handle:** <Both indexes starting at 0 means they are always equal (meeting) at the beginning, use a "do-while(slow != fast)" on the first step>
- **Where I got stuck and for how long:** <Do-while(slow != fast) instead of while, and couldn't solve without editorial>
- **Template fragments I reused:** <Linked list interpretation of array, Tortoise/Hare algorithm>
- **Would I solve this in 25 min cold next week? Y/N> Probably, added to tracker.
