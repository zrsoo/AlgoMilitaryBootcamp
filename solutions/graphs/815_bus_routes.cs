public class Solution {
    public int NumBusesToDestination(int[][] routes, int source, int target) {
        if(source == target)
            return 0;

        int n = routes.Length;

        var visitedStops = new HashSet<int>();
        var visitedRoutes = new HashSet<int>();
        var stopRoutes = new Dictionary<int, List<int>>();

        // Build station list
        // Build stopRoutes; for each stop -> routes passing through
        for(int i = 0; i < n; ++i)
            for(int j = 0; j < routes[i].Length; ++j)
            {
                if(!stopRoutes.TryGetValue(routes[i][j], out var list))
                {
                    list = new List<int>();
                    stopRoutes[routes[i][j]] = list;
                }
                list.Add(i);
            }

        var q = new Queue<int>();

        q.Enqueue(source);
        visitedStops.Add(source);
        int buses = 0;

        while(q.Count > 0)
        {
            buses++;
            int levelSize = q.Count;

            for(int i = 0; i < levelSize; ++i)
            {
                int stop = q.Dequeue();

                if(!stopRoutes.TryGetValue(stop, out var routeIds)) continue;

                foreach(var r in routeIds)
                {
                    if(!visitedRoutes.Add(r)) continue;

                    foreach(var next in routes[r])
                    {
                        if(next == target) return buses;

                        if(visitedStops.Add(next))
                            q.Enqueue(next);
                    }
                }
            }
        }

        return -1;
    }
}
