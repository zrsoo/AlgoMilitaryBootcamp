public class WordDictionary {
    private class TrieNode
    {
        public TrieNode[] children = new TrieNode[26];
        public bool isWord = false;
    }

    TrieNode root;

    public WordDictionary() {
        root = new TrieNode();
    }
    
    public void AddWord(string word) {
        var node = root;

        foreach(var c in word)
        {
            int i = c - 'a';
            node.children[i] ??= new TrieNode();
            node = node.children[i];
        }

        node.isWord = true;
    }
    
    public bool Search(string word) {
        return search(root, 0, word);
    }

    private bool search(TrieNode node, int i, string word)
    {
        if(node == null) return false;
        if(i == word.Length) return node.isWord;

        char c = word[i];
        if(c == '.')
        {
            foreach(var n in node.children)
                if(n != null && search(n, i + 1, word))
                    return true;

            return false;
        }
        return search(node.children[c - 'a'], i + 1, word);
    }
}

/**
 * Your WordDictionary object will be instantiated and called as such:
 * WordDictionary obj = new WordDictionary();
 * obj.AddWord(word);
 * bool param_2 = obj.Search(word);
 */
