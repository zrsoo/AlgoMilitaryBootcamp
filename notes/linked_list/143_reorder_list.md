# LC <143> — <Reorder List>
- **Pattern:** <Linked list — find middle (slow/fast), reverse second half in place, weave-merge the two halves>
- **Brute force:** <Push all nodes onto a stack, then re-link front node → popped (back) node alternately. O(n) time, O(n) space.>
- **Optimized:** <(1) slow/fast to find middle; (2) 3-pointer in-place reversal of the back half starting at `slow`; (3) weave: alternate nodes from `first` (front) and `second` (reversed back), stashing both `next` links before rewiring. Loop while `second.next != null` so the tail stays null-terminated.> — O(n) time, O(1) space
- **Key insight:** <The stack version is just a manual reverse of the back half; doing the reversal in place removes the O(n) auxiliary space. "Stash before you clobber" applies in both the reversal and the weave.>
- **Edge cases I had to handle:** <Null/short lists (`prev == null` early return); even vs odd length handled naturally by `while(second.next != null)` leaving the last node as a clean tail.>
- **Where I got stuck and for how long:** <Solved first with the O(n)-space stack on my own. Needed the 3-pointer reversal and weave-merge templates for the O(1) version.>
- **Template fragments I reused:** <slow/fast middle finder; 3-pointer reversal (LC 206); merge weave.>
- **Would I solve this in 25 min cold next week? Y/N> 

3 pointer reversal:

    // Slow is now at middle
    ListNode prev = null;
    ListNode curr = slow;

    while(curr != null)
    {
        ListNode next = curr.next;
        curr.next = prev;
        prev = curr;
        curr = next;
    }


weave merge:

    ListNode first = head;
    ListNode second = prev;

    while(second.next != null)
    {
        ListNode t1 = first.next;
        ListNode t2 = second.next;

        first.next = second;
        second.next = t1;

        first = t1;
        second = t2;
    }