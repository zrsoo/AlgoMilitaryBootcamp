# LC <428> — <Serialize and Deserialize N-ary Tree>
- **Pattern:** <Tree pre-order serialization with explicit child-count per node (no brackets/sentinels needed)>
- **Brute force:** < >
- **Optimized:** <Serialize: write `[val],[childCount],` then recurse on each child (`foreach`). Deserialize: split on `,`, walk with a single `ref int` cursor — read val, read count, recurse exactly count times. Empty tree ↔ empty string.> — O(n) time, O(h) space
- **Key insight:** <The child *count* tells the decoder exactly where each child list ends, so no bracket/null markers are required. A shared `ref int i` cursor keeps serialize/deserialize reading tokens in the same order.>
- **Edge cases I had to handle:** <Null tree → `""` and back; leaf = count 0 (foreach runs zero times); negative values (delimiter `,` never collides with `-`); trailing comma harmless because Decode only reads as many tokens as counts dictate.>
- **Where I got stuck and for how long:** <Needed hints: how to recurse on each child (`foreach`), and the null guard in serialize. Deserialize cursor pattern came together after.>
- **Template fragments I reused:** <Pre-order serialize + ref-cursor deserialize; generalizes LC 297 (binary) to an arbitrary child list via the count.>
- **Would I solve this in 25 min cold next week? Y/N> 
