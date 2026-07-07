public class Solution {
    public int NetworkDelayTime(int[][] times, int n, int k) {
        var dist = new int[n + 1];
        Array.Fill(dist, int.MaxValue);

        var pq = new PriorityQueue<int, int>();

        var adj = new Dictionary<int, List<(int n, int w)>>();
        for(int i = 1; i <= n; ++i)
            adj[i] = new List<(int n, int w)>();

        for(int i = 0; i < times.Length; ++i)
            adj[times[i][0]].Add((times[i][1], times[i][2]));

        dist[k] = 0;
        pq.Enqueue(k, 0);

        int res = 0;
        int nodeCount = 0;

        while(pq.Count > 0)
        {
            pq.TryDequeue(out int cNode, out int cDist);

            if(cDist > dist[cNode]) continue;

            nodeCount++;

            foreach(var (v, w) in adj[cNode])
            {
                if(dist[cNode] + w < dist[v])
                {
                    dist[v] = dist[cNode] + w;
                    pq.Enqueue(v, dist[v]);
                }
            }
        }

        for(int i = 1; i <= n; ++i)
            res = Math.Max(res, dist[i]);

        return nodeCount == n ? res : -1;
    }
}
