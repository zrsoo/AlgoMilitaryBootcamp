# LC <211> — <Design Add and Search Words Data Structure>
- **Pattern:** <Trie with wildcard search — recursive DFS over children when char is `.`>
- **Brute force:** <Store words in a list; on Search with a '.' match by scanning every equal-length word char-by-char — O(N·L) per query.>
- **Optimized:** <Implement a trie. Search with recursive function on trie, current index in word = i and word. When searching, you have two branches on the search. If the current letter is '.', recurse on all children of the node (= any letter on any edge suffices). Else, recurse on specific letter of the word that the index is at. Search breaks when we hit a null node -> return false; when i == word.Length -> return node.IsWord; or on the '.' recursive call.>
- **Key insight:** <'.' forks the search across ALL non-null children (any letter suffices); a concrete letter recurses down its single edge. Terminate: null node → false, `i == word.Length` → return node.isWord.>
- **Edge cases I had to handle:** <After the `return true` in the recursive call on '.' in the for, we have to `return false`. The if treats its returns separately. We have already recursed on all children and found nothing, so `return false`. If we don't do that, we will enter the normal recursion loop.>
- **Where I got stuck and for how long:** <Clean first solve.>
- **Template fragments I reused:** <26-way trie Insert (from LC 208) + recursive index-carrying DFS search.>
- **Would I solve this in 25 min cold next week? Y/N> Y
