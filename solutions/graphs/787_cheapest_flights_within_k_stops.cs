public class Solution {
    public int FindCheapestPrice(int n, int[][] flights, int src, int dst, int k) {
        var dist = new int[n];
        Array.Fill(dist, int.MaxValue);
        dist[src] = 0;

        for(int round = 0; round <= k; round++)          // k+1 rounds = at most k+1 edges
        {
            var tmp = (int[])dist.Clone();               // snapshot: relax off last round only
            foreach(var f in flights)
            {
                int u = f[0], v = f[1], w = f[2];
                if(dist[u] == int.MaxValue) continue;    // u not yet reachable
                if(dist[u] + w < tmp[v])
                    tmp[v] = dist[u] + w;
            }
            dist = tmp;
        }

        return dist[dst] == int.MaxValue ? -1 : dist[dst];
    }
}
