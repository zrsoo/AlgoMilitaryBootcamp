# LC <25> — <Reverse Nodes in k-Group>
- **Pattern:** <Linked list — reverse each consecutive group of k nodes, leave a trailing remainder of < k untouched>
- **Brute force:** <Stack of size k: push node copies; when the stack hits k, pop them all into the result list (popping reverses order). Leftover < k stays in original order via `rest = curr.next`.>
- **Optimized:** <In-place pointer reversal per group (no stack, no copies): reverse k links, reconnect the previous group's tail to the new head and the current group's tail to the next group's head. Iterative or recursive.> — O(n) time, O(1) space
- **Key insight:** <Nested loop is amortized O(n), NOT O(n·k): every node is pushed once and popped once, so total work = 2n. The stack version allocates n new nodes though — relinking existing nodes in place avoids that.>
- **Edge cases I had to handle:** <Trailing group of < k nodes must stay in original order (handled because copies leave original `next` intact, so `rest = curr.next` walks the untouched list). `k <= n` guaranteed, so a full group always exists and `r` is non-null at the final `r.next = rest`.>
- **Where I got stuck and for how long:** <Solved on my own with the stack approach. O(k) auxiliary space but O(n) total allocation (copies every node).>
- **Template fragments I reused:** <Stack-reversal; group boundary relinking.>
- **Would I solve this in 25 min cold next week? Y/N> 
