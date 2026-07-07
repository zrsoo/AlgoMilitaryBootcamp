public class LRUCache {
    private class Node
    {
        public int key, val;
        public Node next;
        public Node prev;

        public Node(int k = 0, int v = 0) { this.key = k; this.val = v; } 
    }

    Dictionary<int, Node> dict;
    int cap, count;
    Node head;
    Node tail;

    public LRUCache(int capacity) {
        dict = new(capacity);
        cap = capacity;
        count = 0;
        
        head = new Node();
        tail = new Node();

        head.next = tail;
        tail.prev = head;
    }
    
    public int Get(int key) {
        dict.TryGetValue(key, out var res);

        if(res != null)
        {
            this.Remove(key);
            this.AddFirst(res);
        }

        return res == null ? -1 : res.val;
    }   

    public void Put(int key, int value) 
    {
        dict.TryGetValue(key, out var node);

        if(node == null)
        {
            node = new Node(key, value);
            dict[key] = node;
            count++;
        }
        else this.Remove(key);

        this.AddFirst(node);

        node.val = value;

        if(count == cap + 1) 
        {
            this.RemoveLast();
        }
    }

    private void Remove(int key)
    {
        if(!dict.ContainsKey(key)) return;

        Node n = dict[key];

        n.prev.next = n.next;
        n.next.prev = n.prev;

        n = null;
    }

    private void AddFirst(Node node)
    {
        node.next = head.next;
        head.next.prev = node;

        head.next = node;
        node.prev = head;
    }

    private void RemoveLast()
    {
        Node newTail = tail.prev;
        dict.Remove(newTail.key);

        tail = null;
        tail = newTail;

        count--;
    }
}

/**
 * Your LRUCache object will be instantiated and called as such:
 * LRUCache obj = new LRUCache(capacity);
 * int param_1 = obj.Get(key);
 * obj.Put(key,value);
 */
