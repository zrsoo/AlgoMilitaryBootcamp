# Class: Tries (Prefix Trees)

> Canonical class for the `tries` topic. Living document — fold new templates/pitfalls back in as problems reveal them.

---

## 1. Context & when to reach for it

A **trie** (prefix tree) is a tree where **each edge is labeled with a character** and each root-to-node path spells a prefix. Words sharing a prefix share the same path until they diverge. A boolean flag (`isWord`) on a node marks "a word ends here."

It trades memory for **prefix-time lookups**: insert/search/startsWith are all **O(L)** where L = word length — independent of how many words are stored. A hash set gives O(L) full-word lookup too, but **cannot answer prefix questions** ("which stored words start with `ca`?") without scanning everything. That prefix capability is the entire reason a trie exists.

**Signals you're in trie territory:**
- "Prefix", "startsWith", "autocomplete", "search suggestions".
- Many words queried repeatedly against a fixed dictionary.
- Wildcard / pattern matching over a word set (`.` matches any char → LC 211).
- A grid/DFS that must check "is this path a prefix of any target word?" → prune with a trie (LC 212 Word Search II).
- Shared-prefix compression, IP routing tables, T9.

> Rule of thumb: if the problem cares about **prefixes** (not just whole-word membership), reach for a trie. If it only needs whole-word membership, a `HashSet` is simpler.

---

## 2. Mental model

Think of a trie as a **map of maps of maps…**. Each node owns a small map `char → child node`. To insert a word, you walk down character by character, creating missing children, and flag the final node. To search, you walk the same way; falling off (no child) means "not present."

Two distinct questions a node answers:
- **`isWord`** — does a complete word end exactly here?
- **"does any word continue past here?"** — implied by having children.

`search("app")` vs `startsWith("app")` differ only in the final check: search demands `node.isWord`, startsWith just demands you reached a node.

**Children representation — the key design choice:**
- **`Dictionary<char, Node>`** — flexible, any charset, sparse-friendly. Default choice.
- **`Node[26]`** — fixed lowercase a–z, fastest, index `c - 'a'`. Use when the charset is constrained and you want raw speed (LC 212 hot loops).

---

## 3. Core patterns

### A. Plain trie (insert / search / startsWith)
The base structure (LC 208). Everything else builds on it.

### B. Wildcard search (`.` matches any char)
When a query char is a wildcard, you can't pick one child — you **branch into all children** and recurse (DFS over the trie). Non-wildcard chars stay deterministic. (LC 211.) This is where a plain `HashSet` breaks down and the trie is essential.

### C. Trie-pruned DFS / backtracking
Drive a grid or string search *by the trie* instead of by a word list. At each step, only continue if the current path is still a prefix in the trie — the trie prunes dead branches instantly. (LC 212 Word Search II — without the trie you'd re-scan the grid per word; with it you scan once.) Optimizations: store the full word on the terminal node to avoid rebuilding it; prune leaf nodes after a word is found.

### D. Trie + ranking for autocomplete
Trie locates the prefix node; then you need the **top-k** completions by frequency/lexicographic order. Either DFS the subtree collecting candidates and sort/heap them, or cache top-k at each node. (LC 642, LC 1268.)

---

## 4. Reusable templates (C#)

### Plain trie (LC 208) — array-backed (a–z)
```csharp
public class TrieNode {
    public TrieNode[] children = new TrieNode[26];
    public bool isWord;
}

public class Trie {
    private readonly TrieNode root = new TrieNode();

    public void Insert(string word) {
        var node = root;
        foreach (var c in word) {
            int i = c - 'a';
            node.children[i] ??= new TrieNode();
            node = node.children[i];
        }
        node.isWord = true;
    }

    public bool Search(string word) {
        var node = Walk(word);
        return node != null && node.isWord;
    }

    public bool StartsWith(string prefix) => Walk(prefix) != null;

    private TrieNode Walk(string s) {
        var node = root;
        foreach (var c in s) {
            int i = c - 'a';
            if (node.children[i] == null) return null;
            node = node.children[i];
        }
        return node;
    }
}
```

### Dictionary-backed node (arbitrary charset)
```csharp
public class TrieNode {
    public Dictionary<char, TrieNode> children = new();
    public bool isWord;
}
// Insert: node.children.TryGetValue(c, out var nxt) ? nxt : (node.children[c] = new TrieNode())
```

### Wildcard search (LC 211) — `.` matches any child
```csharp
public bool Search(string word) => Dfs(word, 0, root);

private bool Dfs(string word, int i, TrieNode node) {
    if (node == null) return false;
    if (i == word.Length) return node.isWord;

    char c = word[i];
    if (c == '.') {
        foreach (var child in node.children) {       // branch into ALL children
            if (child != null && Dfs(word, i + 1, child)) return true;
        }
        return false;
    }
    return Dfs(word, i + 1, node.children[c - 'a']);  // deterministic step
}
```

### Trie-pruned grid DFS (LC 212 skeleton)
```csharp
// Build trie of all target words; store the word string on its terminal node.
// Then DFS the grid; at each cell follow node.children[grid[r][c]-'a'].
// If that child is null -> prune. If child.word != null -> collect it (and null it to dedup).
```

---

## 5. Common pitfalls

- **Forgetting `isWord`** — without the end-of-word flag, `search("app")` can't be distinguished from `startsWith("app")` when "apple" is stored but "app" isn't.
- **Confusing search vs startsWith** — both walk identically; only the final check differs (`node.isWord` vs `node != null`).
- **Array-backed assumption** — `c - 'a'` only works for lowercase a–z. Uppercase, digits, or unicode need a bigger array or a dictionary. Read the constraints.
- **Null child deref** — always null-check before descending (`Walk` returns null on miss).
- **LC 212 without the trie / without pruning** → TLE. The whole point is to drive the DFS by the trie and prune dead prefixes; also remove found words (null the terminal) to avoid duplicates and shrink the search.
- **Rebuilding the word via a path string on every find** — store the full word on the terminal node instead.
- **Memory blowup** — a trie over many long words with a 26-array per node is heavy; mention the dictionary variant or compressed/radix trie if memory is a concern.

---

## 6. Complexity cheat sheet

| Operation | Time | Notes |
|---|---|---|
| Insert word | O(L) | L = word length |
| Search / startsWith | O(L) | independent of #words stored |
| Wildcard search (LC 211) | O(26^d · L) worst | d = number of `.` wildcards |
| Build trie of W words | O(total chars) | |
| Space | O(total chars · alphabet) | array-backed; dictionary is sparser |
| Word Search II (LC 212) | O(cells · 4^maxWordLen) bounded by trie | trie pruning makes this practical |

`L` = query/word length, `W` = number of words.

---

## 7. Map to the problem set (Topic 10 — Tries)

| # | LC | Pattern from above |
|---|---|---|
| 68 | 208 Implement Trie | A — plain trie (insert/search/startsWith) |
| 69 | 211 Add and Search Word | B — wildcard `.` DFS over the trie |
| 70 | 212 Word Search II | C — trie-pruned grid DFS (★, the payoff problem) |
| 71 | 642 Design Search Autocomplete | D — trie + top-k ranking (⚑) |
| 72 | 1268 Search Suggestions System | D — trie or sorted-words + prefix (R) |

Related elsewhere: tries are a specialization of trees (Topic 9); the wildcard DFS (B) and grid DFS (C) reuse the backtracking muscle from Topic 12.
