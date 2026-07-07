# LC <206> — <Reverse Linked List>
- **Pattern:** <Linked list — in-place iterative reversal (prev/curr/next)>
- **Brute force:** <Push every node (or value) onto a stack, then pop to rebuild links in reverse. O(n) time, O(n) space.>
- **Optimized:** <3-pointer in-place reversal: stash `next = curr.next`, rewire `curr.next = prev`, advance `prev`/`curr`; return `prev` as the new head.> — O(n) time, O(1) space
- **Key insight:** <Stash `curr.next` into a temp BEFORE overwriting it with `prev`, or the rest of the list is lost.>
- **Edge cases I had to handle:** <Empty list (head null → loop skipped, returns null); single node (returns itself).>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <3-pointer (prev/curr/next) reversal.>
- **Would I solve this in 25 min cold next week? Y/N> 
