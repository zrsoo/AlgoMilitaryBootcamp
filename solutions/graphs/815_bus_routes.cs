public class Solution {
    public int NumBusesToDestination(int[][] routes, int source, int target) {
        if(source == target) return 0;

        var dict = new Dictionary<int, List<int>>();
        var q = new Queue<int>();

        var visitedRings = new bool[routes.Length];
        var visitedStops = new HashSet<int>();

        for(int i = 0; i < routes.Length; ++i)
        {
            foreach(var stop in routes[i])
            {
                if(!dict.TryGetValue(stop, out var lst))
                {
                    lst = new List<int>();
                    dict[stop] = lst;
                }
                dict[stop].Add(i);

                if(stop == source)
                {
                    q.Enqueue(i);
                    visitedRings[i] = true;
                }
            }
        }

        var steps = 0;

        while(q.Count > 0)
        {
            var lSize = q.Count;
            steps++;

            for(int i = 0; i < lSize; ++i)
            {
                var ring = q.Dequeue();

                foreach(var stop in routes[ring])
                {
                    if(stop == target)
                        return steps;

                    if(!visitedStops.Add(stop)) continue; 

                    if(dict.TryGetValue(stop, out var adjRings))
                    {
                        adjRings.Remove(ring);

                        foreach(var r in adjRings)
                        {
                            if(!visitedRings[r])
                            {
                                q.Enqueue(r);
                                visitedRings[r] = true;
                            }
                        }
                    }
                }
            }
        }

        return -1;
    }
}