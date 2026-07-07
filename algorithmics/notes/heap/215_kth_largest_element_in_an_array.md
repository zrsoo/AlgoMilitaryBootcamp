# LC <215> — <Kth Largest Element in an Array>
- **Pattern:** <Heap — bounded min-heap of size k>
- **Brute force:** <Sort the array descending and index element k-1 — O(n log n).>
- **Optimized:** <Enqueue all elements in a C# Priority Queue with the element as TElement and its value as Priority. If the number of elements in the queue exceeds k, dequeue (pop the smallest one). At the end, you will have a priority queue with K elements, Kth largest is `pq.Peek()`> — O(n * log(n)) time, O(n) space
- **Key insight:** <Priority Queue for holding sorted elements.>
- **Edge cases I had to handle:** <Heap grows to k+1 then evicts the smallest, so the k largest survive and the root is the kth largest.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Bounded min-heap of size k (evict when Count exceeds k).>
- **Would I solve this in 25 min cold next week? Y/N> 
