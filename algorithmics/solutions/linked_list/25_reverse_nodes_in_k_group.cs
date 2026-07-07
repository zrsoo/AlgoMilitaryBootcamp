/**
 * Definition for singly-linked list.
 * public class ListNode {
 *     public int val;
 *     public ListNode next;
 *     public ListNode(int val=0, ListNode next=null) {
 *         this.val = val;
 *         this.next = next;
 *     }
 * }
 */
public class Solution {
    public ListNode ReverseKGroup(ListNode head, int k) {
        // Dummy node before head so we can uniformly re-link the first group too.
        var dummy = new ListNode(0, head);

        // groupPrev = the node just before the group we're about to reverse.
        var groupPrev = dummy;

        while (true)
        {
            // Walk k nodes ahead of groupPrev to find the group's boundary (kth node).
            var kth = groupPrev;
            for (int i = 0; i < k && kth != null; i++)
                kth = kth.next;

            // Fewer than k nodes remain -> leave the rest as-is and finish.
            if (kth == null) break;

            // groupNext = first node of the NEXT group (what the reversed group tail links to).
            var groupNext = kth.next;

            // --- Reverse the k nodes in place ---
            // Standard prev/curr reversal, but stop when curr reaches groupNext.
            var prev = groupNext;                 // reversed tail should point here
            var curr = groupPrev.next;            // first node of current group
            while (curr != groupNext)
            {
                var tmp = curr.next;              // save next before we overwrite it
                curr.next = prev;                 // reverse the pointer
                prev = curr;                      // advance prev
                curr = tmp;                       // advance curr
            }

            // After reversal, `kth` is the new head of the group and
            // groupPrev.next is the old head (now the group's tail).
            var oldGroupHead = groupPrev.next;    // becomes the tail after reversal
            groupPrev.next = kth;                 // link previous section to new head
            groupPrev = oldGroupHead;             // tail is the groupPrev for the next round
        }

        return dummy.next;
    }
}
