public class Trie {
    private class TrieNode
    {
        public TrieNode[] children = new TrieNode[26];
        public bool isWord = false;
    }

    private TrieNode root;

    public Trie() {
        root = new TrieNode();
    }
    
    public void Insert(string word) {
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
        var node = Walk(word);

        return node != null && node.isWord == true;
    }
    
    public bool StartsWith(string prefix) {
        return Walk(prefix) != null;
    }

    private TrieNode Walk(string word)
    {
        var node = root;

        foreach(char c in word)
        {
            int i = c - 'a';

            if(node.children[i] == null)
                return null;

            node = node.children[i];
        }

        return node;
    }
}

/**
 * Your Trie object will be instantiated and called as such:
 * Trie obj = new Trie();
 * obj.Insert(word);
 * bool param_2 = obj.Search(word);
 * bool param_3 = obj.StartsWith(prefix);
 */
