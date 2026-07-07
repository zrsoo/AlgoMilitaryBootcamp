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
    public void ReorderList(ListNode head) {
        ListNode slow = head;
        ListNode fast = head;

        while(fast != null && fast.next != null)
        {
            slow = slow.next;
            fast = fast.next.next;
        }

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

        if(prev == null)
            return;

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
    }
}
