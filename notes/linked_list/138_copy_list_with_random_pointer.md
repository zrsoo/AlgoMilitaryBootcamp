# LC <138> — <Copy List with Random Pointer>
- **Pattern:** <Linked list deep copy — resolve the "clone's random must point to a clone, not the original" dependency>
- **Brute force:** < >
- **Optimized:** <HashMap `original → clone`. Pass 1: create a bare clone for every node. Pass 2: wire `next`/`random` via map lookups (every clone exists by now, so any target resolves). Return `map[head]`.> — O(n) time, O(n) space
- **Key insight:** <When you first create a clone, its `random` target's clone may not exist yet — a map (or the interleaving trick) defers wiring until all clones exist.>
- **Edge cases I had to handle:** <Null head; null `next`/`random` pointers (guard with `== null ? null : ...`).>
- **Where I got stuck and for how long:** <Got the solution — needed the approach for resolving the forward-reference of random pointers.>
- **Template fragments I reused:** <Original→clone map; alternative is the O(1)-space interleave (weave clone after each original, set `clone.random = cur.random.next`, then unweave).>
- **Would I solve this in 25 min cold next week? Y/N> 
