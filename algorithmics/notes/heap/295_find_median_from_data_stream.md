# LC <295> — <Find Median from Data Stream>
- **Pattern:** <Heap — two heaps (max-heap lower half / min-heap upper half)>
- **Brute force:** <Keep a sorted list, insert each new number in order (O(n) per add) and read the middle element(s) for the median.>
- **Optimized:** <Two heaps: max-heap `lo` for the lower half (stored with -num priority) and min-heap `high` for the upper half. On add, `lo.EnqueueDequeue(num)` pushes num and pops lo's current max into `high`; if `high` grows larger than `lo`, move high's min back to `lo`, keeping `lo` with the extra element. Median = `lo.Peek()` when total count is odd, else average of both tops.> — O(log n) time, O(n) space
- **Key insight:** <Balancing the two heaps so `lo` holds the smaller half and `high` the larger half puts the median element(s) right at the heap tops.>
- **Edge cases I had to handle:** <Odd vs even total count (odd keeps the extra in `lo`); EnqueueDequeue handles the empty-heap first insert cleanly.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Two-heap balance (max-heap + min-heap); PriorityQueue EnqueueDequeue.>
- **Would I solve this in 25 min cold next week? Y/N> 
