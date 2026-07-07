# LC <212> — <Word Search II>
- **Pattern:** <Trie-pruned grid backtracking — build a trie of all target words, then drive a single DFS over the board by the trie node; `nxt == null` prunes dead prefixes in O(1), the full word is stored on the terminal node, and nulling it after a hit dedups + shrinks the trie>
- **Brute force:** <Run a separate board DFS for each word independently — O(W · n·m·4·3^(L-1)), with no shared-prefix pruning.>
- **Optimized:** <O(n * m * 4 * 3 ^(L - 1)) space - L = length of longest word (size of trie); O(1) - space>
- **Key insight:** <Store word at isWord=true nodes to fetch the word in O(1)>
- **Edge cases I had to handle:** <Mark visited by overwriting the cell with '#' and restore it on backtrack; a null `word` on a node means non-terminal; setting `nxt.word = null` after a hit dedups repeats and avoids re-adding.>
- **Where I got stuck and for how long:** <Didn't think of storing the word on isWord=true nodes. Used search at each step to find the word, didn't fit time.>
- **Template fragments I reused:** <Trie Insert (LC 208) + grid 4-directional backtracking with in-place '#' visited marking (LC 79 Word Search).>
- **Would I solve this in 25 min cold next week? Y/N>
