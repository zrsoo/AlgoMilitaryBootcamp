# LC <208> — <Implement Trie (Prefix Tree)>
- **Pattern:** <Trie — fixed 26-way child array per node, `isWord` flag; shared `Walk` helper for Search/StartsWith>
- **Brute force:** < >
- **Optimized:** <Use trie = Tree where each edge is a character in the word. TrieNode(Node[26] children, bool isWord). Insert - begin node = root; iterate over each char in array (i = c - 'a'); if node.children[i] is null, initialize to new trie node, move to it and continue. Walk - start node = root; if node.children[i] is null return null; else move to it; at the end return the node. Search - Walk the trie; if the returned node is not null and node.isWord == true -> true else false. StartsWith - Walk the trie; if the returned node is not null -> true else false> — O(L) time per op (L = word length), O(total chars) space
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** <Clean first solve.>
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N> Y
