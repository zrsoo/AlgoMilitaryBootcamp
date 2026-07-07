# LC <347> — <Top K Frequent Elements>
- **Pattern:** <Heap — bounded min-heap of size k (by frequency)>
- **Brute force:** <Count frequencies into a map, sort the unique elements by frequency descending, and take the first k — O(m log m).>
- **Optimized:** <Do one pass through the array counting frequencies and storing in a map. Then, iterate the kvps in the map, and for each one, enqueue the element as TElement, and its frequency as Priority. If size of the priority queue exceeds k, dequeue. At the end, you will have a K sized priority queue, with frequencies as priorities = top k most fequent.> — m - nr of unique elements in array O(n + m * log(k)) time, O(m + k) space
- **Key insight:** <PQ for holding sorted elements w.r.t frequency>
- **Edge cases I had to handle:** <Heap only evicts once it hits k+1, so it holds exactly k when unique count >= k; ties in frequency are broken arbitrarily.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Frequency map (Dictionary) + bounded min-heap of size k.>
- **Would I solve this in 25 min cold next week? Y/N> 
