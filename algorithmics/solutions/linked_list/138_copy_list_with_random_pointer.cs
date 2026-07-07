/*
// Definition for a Node.
public class Node {
    public int val;
    public Node next;
    public Node random;
    public Node(int _val) {
        val = _val;
        next = null;
        random = null;
    }
}
*/

public class Solution {
    public Node CopyRandomList(Node head) {
        if (head == null) return null;

        var map = new Dictionary<Node, Node>();

        // Pass 1: create a clone for every node, keyed by original.
        for (Node cur = head; cur != null; cur = cur.next)
            map[cur] = new Node(cur.val);

        // Pass 2: wire next/random using the map.
        for (Node cur = head; cur != null; cur = cur.next)
        {
            map[cur].next   = cur.next   == null ? null : map[cur.next];
            map[cur].random = cur.random == null ? null : map[cur.random];
        }

        return map[head];
    }
}
