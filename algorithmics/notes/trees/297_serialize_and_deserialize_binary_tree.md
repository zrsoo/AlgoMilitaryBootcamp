# LC <297> — <Serialize and Deserialize Binary Tree>
- **Pattern:** <Preorder DFS with explicit null sentinels — encode each node as `val,` and each null as `#,`; deserialize by consuming tokens in the same order via a shared cursor, so the tree shape is implicit in the token sequence>
- **Brute force:** <Store nodes level-order into a fixed array by index (children at 2i+1 / 2i+2) — wastes O(2^h) space and blows up on skewed trees.>
- **Optimized:** <Serialize: preorder into a StringBuilder, node → "val,", null → "#,". Deserialize: split on ',', consume tokens with a shared `ref int i` cursor — "#" → null, else make the node then recurse left, then right.> — O(n) time, O(n) space
- **Key insight:** <Explicit null sentinels make the token stream unambiguous, so tree shape is implicit in preorder — no indices/brackets needed. Append into ONE shared StringBuilder (not `Append(recurse(child))`) to keep serialize O(n) not O(n²). The same `ref int` cursor keeps encode/decode in lockstep.>
- **Edge cases I had to handle:** <Null root → "#," round-trips back to null; negative values fine (',' delimiter never collides); every leaf emits two "#" children.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Preorder serialize + ref-cursor deserialize; LC 428 generalizes this to n-ary via a child count.>
- **Would I solve this in 25 min cold next week? Y/N>
