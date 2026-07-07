public class Solution {
    public int OrangesRotting(int[][] grid) {
        int n = grid.Length;
        int m = grid[0].Length;

        int nrOranges = 0;

        var q = new Queue<(int i, int j, int m)>();

        for(int i = 0; i < n; ++i)
            for(int j = 0; j < m; ++j)
            {
                if(grid[i][j] == 1 || grid[i][j] == 2)
                    nrOranges++;

                if(grid[i][j] == 2)
                    q.Enqueue((i, j, 0));
            }

        int ans = 0;

        while(q.Count > 0)
        {
            var orange = q.Dequeue();
            grid[orange.i][orange.j] = orange.m;
            nrOranges--;

            if(orange.i - 1 >= 0 && grid[orange.i - 1][orange.j] == 1)
            {
                q.Enqueue((orange.i - 1, orange.j, orange.m - 1));
                grid[orange.i - 1][orange.j] = -1;
                ans = Math.Max(ans, Math.Abs(orange.m - 1));
            }
            if(orange.i + 1 < n && grid[orange.i + 1][orange.j] == 1)
            {
                q.Enqueue((orange.i + 1, orange.j, orange.m - 1));
                grid[orange.i + 1][orange.j] = -1;
                ans = Math.Max(ans, Math.Abs(orange.m - 1));
            }
            if(orange.j - 1 >= 0 && grid[orange.i][orange.j - 1] == 1)
            {
                q.Enqueue((orange.i, orange.j - 1, orange.m - 1));
                grid[orange.i][orange.j - 1] = -1;
                ans = Math.Max(ans, Math.Abs(orange.m - 1));
            }
            if(orange.j + 1 < m && grid[orange.i][orange.j + 1] == 1)
            {
                q.Enqueue((orange.i, orange.j + 1, orange.m - 1));
                grid[orange.i][orange.j + 1] = -1;
                ans = Math.Max(ans, Math.Abs(orange.m - 1));
            }
        }

        return nrOranges == 0 ? ans : -1;
    }
}
