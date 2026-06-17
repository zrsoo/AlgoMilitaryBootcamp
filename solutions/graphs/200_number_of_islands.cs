public class Solution {
    int[][] moves = [[0, 1], [0, -1], [1, 0], [-1, 0]];

    private void Fill(int i, int j, char[][] grid)
    {
        if(i < 0 || i >= grid.Length || j < 0 || j >= grid[0].Length || grid[i][j] == '0')
            return;

        grid[i][j] = '0';

        foreach(var m in moves)
        {
            int ni = i + m[0];
            int nj = j + m[1];

            Fill(ni, nj, grid);
        }
    }

    public int NumIslands(char[][] grid) {
        int result = 0;

        for(int i = 0; i < grid.Length; ++i)
            for(int j = 0; j < grid[i].Length; ++j)
            {
                if(grid[i][j] == '1')
                {
                    result++;
                    Fill(i, j, grid);
                }
            }

        return result;
    }
}
