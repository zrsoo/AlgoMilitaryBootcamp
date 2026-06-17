public class Solution {
    public int FindCircleNum(int[][] isConnected) {
        int n = isConnected.Length;
        int nrProvinces = 0;

        var visited = new bool[n];

        for(int i = 0; i < n; ++i)
        {
            if(!visited[i])
            {
                FillProvince(i, isConnected, visited);
                nrProvinces++;
            }
        }

        return nrProvinces;
    }

    private void FillProvince(int i, int[][] adj, bool[] visited)
    {
        visited[i] = true;
        
        for(int j = 0; j < adj[i].Length; ++j)
            if(j != i && adj[i][j] == 1 && !visited[j])
                FillProvince(j, adj, visited);
    }
}
