public class Solution {
    private class TrieNode
    {
        public TrieNode[] children = new TrieNode[26];
        public string word;
    }

    TrieNode root = new TrieNode();

    private void insert(string word)
    {
        var node = root;

        foreach(char c in word)
        {
            int i = c - 'a';
            node.children[i] ??= new TrieNode();
            node = node.children[i];
        }

        node.word = word;
    }

    List<List<int>> moves = [[0,1], [0, -1], [1, 0], [-1, 0]];

    public IList<string> FindWords(char[][] board, string[] words) {
        foreach(var w in words)
            insert(w);

        int n = board.Length;
        int m = board[0].Length;

        var result = new List<string>();

        for(int i = 0; i < n; ++i)
            for(int j = 0; j < m; ++j)
                findWords(root, i, j, board, result);

        return result;
    }

    private void findWords(TrieNode node, int i, int j, char[][] board, List<string> result)
    {
        if((i < 0 || i >= board.Length || j < 0 || j >= board[0].Length) || board[i][j] == '#')
            return;

        var nxt = node.children[board[i][j] - 'a'];
        if(nxt == null) return;

        char ch = board[i][j];
        board[i][j] = '#';

        if(nxt.word != null)
        {
            result.Add(nxt.word);
            nxt.word = null;
        }

        foreach(var m in moves)
        {
            int ni = i + m[0];
            int nj = j + m[1];

            findWords(nxt, ni, nj, board, result);
        }

        board[i][j] = ch;
    }
}
