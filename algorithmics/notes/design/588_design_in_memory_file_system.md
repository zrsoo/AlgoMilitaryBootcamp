# LC 588 — Design In-Memory File System
- **Pattern:** Design — tree of path-segment nodes (generalized trie); each `Node` has `SortedDictionary<string, Node> children` + optional file content; split path on `/`, walk/create nodes; `ls` returns file name or the (already-sorted) child keys
- **Brute force:**
- **Optimized:**
- **Key insight:**
- **Edge cases I had to handle:**
- **Where I got stuck and for how long:**
- **Template fragments I reused:**
- **Would I solve this in 25 min cold next week? Y/N**
