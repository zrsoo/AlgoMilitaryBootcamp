public class Solution {
    public IList<IList<string>> SolveNQueens(int n) {
        var attBoard = new int[n, n];
        var cboard = new List<string>();
        var result = new List<IList<string>>();

        Recurse(0, n, attBoard, cboard, result);

        return result;
    }

    private void Recurse(int i, int n, int[,] attBoard, List<string> cboard, List<IList<string>> result)
    {
        if(i == n)
        {
            result.Add(new List<string>(cboard));
            return;
        }

        // Recurse
        for(int idx = 0; idx < n; ++idx)
        {
            if(attBoard[i, idx] > 0)
                continue;

            // Place
            var s = new char[n];
            Array.Fill(s, '.');
            s[idx] = 'Q';
            cboard.Add(new string(s));

            // Vertical + horizontal attack
            for(int o = 0; o < n; ++o)
            {
                attBoard[i, o]++;
                attBoard[o, idx]++;
            }

            // Diagonal attack
            for(int o = 1; o < n; ++o)
            {
                if(i - o >= 0 && idx - o >= 0)
                    attBoard[i - o, idx - o]++;
                if(i - o >= 0 && idx + o < n)
                    attBoard[i - o, idx + o]++;
                if(i + o < n && idx - o >= 0)
                    attBoard[i + o, idx - o]++;
                if(i + o < n && idx + o < n)
                    attBoard[i + o, idx + o]++;
            }

            Recurse(i + 1, n, attBoard, cboard, result);

            cboard.RemoveAt(cboard.Count - 1);

            // Vertical + horizontal undo attack
            for(int o = 0; o < n; ++o)
            {
                attBoard[i, o]--;
                attBoard[o, idx]--;
            }

            // Diagonal undo attack
            for(int o = 1; o < n; ++o)
            {
                if(i - o >= 0 && idx - o >= 0)
                    attBoard[i - o, idx - o]--;
                if(i - o >= 0 && idx + o < n)
                    attBoard[i - o, idx + o]--;
                if(i + o < n && idx - o >= 0)
                    attBoard[i + o, idx - o]--;
                if(i + o < n && idx + o < n)
                    attBoard[i + o, idx + o]--;
            }
        }
    }
}
