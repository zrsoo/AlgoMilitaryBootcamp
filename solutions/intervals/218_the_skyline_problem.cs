public class Solution {
    public IList<IList<int>> GetSkyline(int[][] buildings) {
        var res = new List<IList<int>>();

        var events = new List<(int x, int h)>();

        foreach(var b in buildings)
        {
            events.Add((b[0], b[2]));
            events.Add((b[1], -b[2]));
        }

        events.Sort((e1, e2) => {
            if(e1.x != e2.x) return e1.x.CompareTo(e2.x);
            return e2.h.CompareTo(e1.h); 
        });

        var pq = new PriorityQueue<int, int>();
        var toRemove = new Dictionary<int, int>();

        pq.Enqueue(0, 0);

        int cMax = -1, prevMax = -1;
        foreach(var (x, h) in events)
        {
            if(h < 0) toRemove[-h] = toRemove.GetValueOrDefault(-h) + 1;
            else pq.Enqueue(h, -h);

            while(pq.Count > 0 && toRemove.GetValueOrDefault(pq.Peek()) > 0)
            {
                toRemove[pq.Peek()]--;
                pq.Dequeue();
            }

            cMax = pq.Count > 0 ? pq.Peek() : 0;

            if(prevMax != cMax)
            {
                res.Add(new List<int>{ x, cMax });
                prevMax = cMax;
            }
        }

        return res;
    }
}