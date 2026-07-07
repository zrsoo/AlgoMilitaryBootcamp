# LC <23> — <Merge K Sorted Lists>
- **Pattern:** <Heap — k-way merge (min-heap of size k)>
- **Brute force:** <Collect every node value into one array, sort it, and rebuild a single linked list — O(N log N).>
- **Optimized:** <Keep a min-heap of current nodes (starting from the head of each list). While the heap still has elements, at each step, pop a node from the heap, add it to result (append to another linked list), and, if its next node is not null, Enqueue it to the heap.> — O(nrNodes * log(nrLists)) time, O(nrLists) space
- **Key insight:** <At any moment only the current head of each list can be the global minimum, so a heap of at most k nodes always surfaces the next node to append.>
- **Edge cases I had to handle:** <Null lists and empty input (0 lists)>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Min-heap and linked lists>
- **Would I solve this in 25 min cold next week? Y/N> Y
