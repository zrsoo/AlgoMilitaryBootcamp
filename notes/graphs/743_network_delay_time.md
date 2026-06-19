# LC <743> — <Network Delay Time>
- **Pattern:** <Graphs — single-source shortest path (Dijkstra, min-heap by distance)>
- **Brute force:** < >
- **Optimized:** <Dijkstra. Keep a min-heap of neighbors and distances (sorted by distance). Insert the starting node. Build weighted adjacency list. While heap is not empty, pop node; for each neighbor of the popped node, if the distance up to itself + the weight to one of its neighbors is shorter than what we have recorded until then for its neighbor, minimize distance to neighbor and enqueue neighbor with new distance. Count number of relaxed (visited) nodes, and at the end, if we didn't visit all nodes return -1, else return the maximum distance in the dist array.>
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N> 
