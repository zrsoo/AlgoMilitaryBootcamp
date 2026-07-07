public class LFUCache {
    private class Node
    {
        public int key, val, cnt;
        public Node next, prev;

        public Node(int k = 0, int v = 0) { this.key = k; this.val = v; this.cnt = 1; }
    }

    private Dictionary<int, (Node head, Node tail)> countBuckets;
    private Dictionary<int, Node> nodeDict;

    int minCount;

    int cap;
    int count;

    public LFUCache(int capacity) {
        countBuckets = new(capacity);
        nodeDict = new(capacity);

        cap = capacity;
        count = 0;
        minCount = 0;
    }
    
    public int Get(int key) {
        if(!nodeDict.TryGetValue(key, out var node)) return -1;

        if(RemoveFromBucket(key, node.cnt))
        {
            node.cnt++;
            AddCreateBucket(node, node.cnt);
        }

        return node.val;
    }
    
    public void Put(int key, int value) {        
        // Node does not exist, create it
        if(!nodeDict.TryGetValue(key, out var node))
        {
            node = new Node(key, value);

            // Remove least used node, i.e. last node of minCount bucket
            if(count == cap) EvictLeastUsed();

            minCount = 1;
            this.count++;
        }
        else
        {
            // Promote bucket
            RemoveFromBucket(node.key, node.cnt);
            node.cnt++;
        }

        AddCreateBucket(node, node.cnt);
        node.val = value;
    }

    private void AddFirst(Node head, Node node)
    {
        node.next = head.next;
        head.next.prev = node;

        head.next = node;
        node.prev = head;
    }

    private void AddCreateBucket(Node node, int count)
    {        
        nodeDict[node.key] = node;

        if(!countBuckets.TryGetValue(count, out var dll))
        {
            dll.head = new Node();
            dll.tail = new Node();

            dll.head.next = dll.tail;
            dll.tail.prev = dll.head;

            countBuckets[count] = (dll.head, dll.tail);
        }

        AddFirst(dll.head, node);
    }

    private bool RemoveFromBucket(int key, int bucket)
    {
        if(!nodeDict.TryGetValue(key, out var node)) return false;
        if(!countBuckets.TryGetValue(bucket, out var dll)) 
        {
            return false;
        }

        nodeDict.Remove(key);

        node.prev.next = node.next;
        node.next.prev = node.prev;
        // node = null;

        if(dll.head.next == dll.tail && minCount == node.cnt) 
        {
            minCount++;
        }

        return true;
    }

    private void EvictLeastUsed()
    {
        if(!countBuckets.TryGetValue(minCount, out var dll)) 
        {
            return;
        }

        if(dll.head.next == dll.tail)
        {
            return;
        }

        var node = dll.tail.prev;

        node.prev.next = dll.tail;
        dll.tail.prev = node.prev;
        dll.tail = node;
        dll.tail.next = null;

        nodeDict.Remove(dll.tail.key);
        count--;
    }
}

/**
 * Your LFUCache object will be instantiated and called as such:
 * LFUCache obj = new LFUCache(capacity);
 * int param_1 = obj.Get(key);
 * obj.Put(key,value);
 */
