using System;
using System.Collections.Generic;

// LC 218 — The Skyline Problem
public class Solution {
    public IList<IList<int>> GetSkyline(int[][] buildings) {
        var result = new List<IList<int>>();

        // Events: start = (L, -H) so taller starts sort first at equal x;
        //         end   = (R, +H) so ends sort after starts at equal x and
        //                 shorter ends come first. Negative height encodes a start.
        var events = new List<(int x, int h)>();
        foreach (var b in buildings)
        {
            events.Add((b[0], -b[2])); // start
            events.Add((b[1], b[2]));  // end
        }
        events.Sort((a, b) => a.x != b.x ? a.x.CompareTo(b.x) : a.h.CompareTo(b.h));

        // Max-heap of active heights (negate priority for max-on-top) + lazy deletion.
        var pq = new PriorityQueue<int, int>();
        var pending = new Dictionary<int, int>(); // height -> # of ends not yet purged
        pq.Enqueue(0, 0); // ground floor so the heap is never empty

        int prevMax = 0;
        foreach (var (x, h) in events)
        {
            if (h < 0)
            {
                // Start: add height |h|.
                int height = -h;
                pq.Enqueue(height, -height);
            }
            else
            {
                // End: schedule lazy removal of height h.
                pending[h] = pending.GetValueOrDefault(h, 0) + 1;
            }

            // Purge any top elements that have been retired.
            while (pq.Count > 0 && pending.GetValueOrDefault(pq.Peek(), 0) > 0)
            {
                int top = pq.Dequeue();
                pending[top]--;
            }

            int curMax = pq.Count > 0 ? pq.Peek() : 0;
            if (curMax != prevMax)
            {
                result.Add(new List<int> { x, curMax });
                prevMax = curMax;
            }
        }

        return result;
    }
}
