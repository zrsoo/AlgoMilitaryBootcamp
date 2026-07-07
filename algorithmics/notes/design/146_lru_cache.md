# LC 146 — LRU Cache
- **Pattern:** Design — `Dictionary<key, Node>` + doubly-linked list (sentinel head/tail); O(1) `Get`/`Put` via move-to-front on access, evict from the tail on overflow
- **Brute force:**
- **Optimized:** Use the Dict + Linked list combo. Dict -> access in O(1). Linked List -> order. You need Remove and AddFirst on the linked list. On put, if node is already in dict, create new, then regardless, update value, remove it and AddFirst. If capacity exceeds max, remove the last node. On get, if not in dict return -1, else return the node's value, remove it and AddFirst.
- **Key insight:**
- **Edge cases I had to handle:**
- **Where I got stuck and for how long:**
- **Template fragments I reused:**
- **Would I solve this in 25 min cold next week? Y/N**
