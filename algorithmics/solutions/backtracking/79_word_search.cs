public class Solution {
    List<List<int>> moves = [[0, 1], [1, 0], [0, -1], [-1, 0]];

    public bool Exist(char[][] board, string word) {
        for(int i = 0; i < board.Length; ++i)
            for(int j = 0; j < board[i].Length; ++j)
                if(Recurse(i, j, 0, board, word))
                    return true;

        return false;
    }

    private bool Recurse(int i, int j, int idx, char[][] board, string word)
    {
        var found = false;

        if(i < 0 || j < 0 || i >= board.Length || j >= board[i].Length)
            return false;

        if(board[i][j] != word[idx])
            return false;

        if(idx == word.Length - 1)
            return true;

        char c = board[i][j];
        board[i][j] = '#';

        foreach(var m in moves)
        {
            int di = i + m[0];
            int dj = j + m[1];

            found = found || Recurse(di, dj, idx + 1, board, word);
        }
        
        board[i][j] = c;
        
        return found;
    }
}
