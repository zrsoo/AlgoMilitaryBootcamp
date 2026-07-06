# LC <743> — <Network Delay Time>
- **Pattern:** <Graphs — single-source shortest path (Dijkstra, min-heap by distance)>
- **Brute force:** <Bellman-Ford: relax all edges V-1 times — O(V·E)>
- **Optimized:** <Dijkstra. Keep a min-heap of neighbors and distances (sorted by distance). Insert the starting node. Build weighted adjacency list. While heap is not empty, pop node; for each neighbor of the popped node, if the distance up to itself + the weight to one of its neighbors is shorter than what we have recorded until then for its neighbor, minimize distance to neighbor and enqueue neighbor with new distance. Count number of relaxed (visited) nodes, and at the end, if we didn't visit all nodes return -1, else return the maximum distance in the dist array.>
- **Key insight:** <The min-heap always expands the closest unsettled node; stale entries are skipped via `cDist > dist[cNode]`. Answer = max finalized distance, or -1 if some node never got settled>
- **Edge cases I had to handle:** <nodes are 1-indexed (size n+1 arrays); skip stale heap entries; an unreachable node stays int.MaxValue ⇒ return -1>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Dijkstra with PriorityQueue<int,int>>
- **Would I solve this in 25 min cold next week? Y/N> 
