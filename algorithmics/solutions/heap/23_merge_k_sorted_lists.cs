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
    public ListNode MergeKLists(ListNode[] lists) {
        if(lists.Length == 0)
            return null;

        var pq = new PriorityQueue<ListNode, int>();
        ListNode node;
        ListNode result = null;
        ListNode r = null;

        foreach(var l in lists)
        {
            if(l != null)
            {
                pq.Enqueue(l, l.val);
            }
        }

        while(pq.Count > 0)
        {
            node = pq.Dequeue();
            
            if(result == null)
            {
                result = node;
                r = node;
            }
            else 
            {
                r.next = node;
                r = r.next;
            }

            if(node.next != null)
                pq.Enqueue(node.next, node.next.val);
        }

        return result;
    }
}
