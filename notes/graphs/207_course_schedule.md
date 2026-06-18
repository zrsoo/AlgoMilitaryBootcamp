# LC <207> — <Course Schedule>
- **Pattern:** <Graphs — topological sort (Kahn's BFS), cycle detection via in-degree>
- **Brute force:** < >
- **Optimized:** <Hold 2 arrays: indeg[numCourses] - counting for each course (node), how many courses need to be done before it (number of incoming vertices); adj - array of List<int>, adjancecy list. Hold a queue of nodes. First, enqueue nodes with indeg 0 (no dependencies so they can be started right away). While queue still has nodes, pop a node, add it to the result (increment counter) and relax neighbors (use adj list to find them faster and decrement their indeg). If a node gets indeg 0 as result of relaxation, enqueue it. At the end, if we've deqeued all courses (by count), then they can be done (no cycles in graph). O(V + E) time, since all vertices are iterated at most once; O(V + E) space>
- **Key insight:** < >
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N> Y
