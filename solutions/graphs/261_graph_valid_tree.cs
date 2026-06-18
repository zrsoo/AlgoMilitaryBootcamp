using System.Collections.Generic;

public class Solution {
    // n nodes labeled 0..n-1, `edges` is a list of undirected edges [u, v].
    // Return true iff the graph is a valid tree.
    public bool ValidTree(int n, int[][] edges) {
        var adj = new List<int>[n];
        for(int i = 0; i < n; ++i)
            adj[i] = new List<int>();

        if(edges.Length != n - 1)
            return false;

        for(int i = 0; i < edges.Length; ++i)
        {
            adj[edges[i][0]].Add(edges[i][1]);
            adj[edges[i][1]].Add(edges[i][0]);
        }

        var visited = new HashSet<int>();

        return nrNodes(0, adj, visited) + 1 == n;
    }

    private int nrNodes(int node, List<int>[] adj, HashSet<int> visited)
    {
        if(visited.Contains(node))
            return 0;

        visited.Add(node);

        int nrN = 0;

        for(int i = 0; i < adj[node].Count; ++i)
            if(!visited.Contains(adj[node][i]))
                nrN += 1 + nrNodes(adj[node][i], adj, visited);

        return nrN;
    }
}
