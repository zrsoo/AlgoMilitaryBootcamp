public class Solution {
    public int ShortestPathLength(int[][] graph) {
        var n = graph.Length; 
        var full = (1 << n) - 1;

        var q = new Queue<(int node, int mask)>();
        var seen = new HashSet<(int node, int mask)>();

        var steps = 0;

        for(int i = 0; i < n; ++i)
        {
            q.Enqueue((i, (1 << i)));
            seen.Add((i, (1 << i)));
        }
            
        while(q.Count > 0)
        {
            for(int s = q.Count; s > 0; --s)
            {
                (int node, int mask) = q.Dequeue();
                
                if(mask == full) return steps;

                foreach(int ne in graph[node])
                {
                    int newMask = mask | (1 << ne);

                    if(seen.Add((ne, newMask)))
                        q.Enqueue((ne, newMask));
                }
            }

            steps++;
        }

        return 0;
    }
}
