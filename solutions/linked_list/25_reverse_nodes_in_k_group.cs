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
        var st = new Stack<ListNode>();
        var curr = head;
        ListNode result = null;
        ListNode r = null;
        ListNode rest = null;

        while(curr != null)
        {
            st.Push(new ListNode(curr.val, null));

            if(st.Count == k)
            {
                while(st.Count > 0)
                {
                    ListNode node = st.Pop();

                    if(result == null)
                    {
                        result = node;
                        r = result;
                    }
                    else
                    {
                        r.next = node;
                        r = r.next;
                    }
                }
            }

            if(st.Count == 0)
            {
                rest = curr.next;
            }

            curr = curr.next;
        }

        r.next = rest;

        return result;
    }
}
