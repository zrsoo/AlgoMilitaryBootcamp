# LC <236> — <Lowest Common Ancestor of a Binary Tree>
- **Pattern:** <Post-order DFS that returns a found target / the LCA up the call stack (node identity is the signal)>
- **Brute force:** < >
- **Optimized:** <Base case: `null`/`p`/`q` → return node. Recurse both sides into `L`/`R`. If both non-null → this node is the split point = LCA. Otherwise bubble up the one non-null child unchanged (`return L ?? R`).> — O(n) time, O(h) space
- **Key insight:** <Return the NODE, not a bool — the node identity carries the answer. Only return `node` (yourself) when both sides report non-null; every other non-null return is a verbatim pass-through. "Both targets exist" guarantee makes the first two-sided meet the true LCA. Use the stored `L`/`R` — never re-call the recursion.>
- **Edge cases I had to handle:** <Both null → null; one side carries either both targets or an already-computed LCA (same handling).>
- **Where I got stuck and for how long:** <Needed the full explanation of the merge / "bubble up unchanged" logic, and a correction where I re-called the recursion instead of using stored `L`/`R`.>
- **Template fragments I reused:** <Post-order DFS returning a node; not a BST so no value-comparison shortcut.>
- **Would I solve this in 25 min cold next week? Y/N> 
