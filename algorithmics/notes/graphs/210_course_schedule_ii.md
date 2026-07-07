# LC <210> — <Course Schedule II>
- **Pattern:** <Graphs — topological sort (Kahn's BFS), emit order + cycle detection>
- **Brute force:** <Repeatedly emit any zero-indegree course by rescanning all edges each round — O(V·E)>
- **Optimized:** <Same as 207, but put the popped nodes in a result array. At the end, only return the array if it has length == numCourses (all nodes have been popped, so no cycles), else return an empty array. O(V + E) time; O(V + E) space>
- **Key insight:** <The dequeue order of Kahn's IS a valid topological order; if fewer than numCourses are emitted there's a cycle ⇒ return empty>
- **Edge cases I had to handle:** <return `new int[0]` when a cycle blocks a full ordering (index != numCourses); edge direction prereq[1] → prereq[0]>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Kahn's topological sort (BFS + in-degree)>
- **Would I solve this in 25 min cold next week? Y/N>
