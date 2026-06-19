public class Solution {
    public int MinimumEffortPath(int[][] heights) {
        int n = heights.Length;
        int m = heights[0].Length;

        if(n == 1 && m == 1)
            return 0;

        var moves = new (int di, int dj)[] { (0, 1), (0, -1), (1, 0), (-1, 0) };

        var q = new PriorityQueue<(int i, int j, int e), int>();

        var dist = new int[n, m];
        for (int i = 0; i < n; i++)
            for (int j = 0; j < m; j++)
                dist[i, j] = int.MaxValue;

        dist[0, 0] = 0;
        q.Enqueue((0, 0, 0), 0);

        while(q.Count > 0)
        {
            (int i, int j, int e) = q.Dequeue();

            if(i == n - 1 && j == m - 1) return e;

            if(e > dist[i, j]) continue;

            foreach(var (di, dj) in moves)
            {
                int ni = i + di;
                int nj = j + dj;

                if(ni >= 0 && ni < n && nj >= 0 && nj < m)
                {
                    int ne = Math.Max(e, Math.Abs(heights[i][j] - heights[ni][nj]));

                    if(ne < dist[ni, nj])
                    {
                        dist[ni, nj] = ne;
                        q.Enqueue((ni, nj, ne), ne);
                    }
                }
            }
        }

        return dist[n - 1, m - 1];
    }
}
