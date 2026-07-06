# LC <994> — <Rotting Oranges>
- **Pattern:** <Graphs — multi-source BFS (seed all rotten cells at time 0, expand rings)>
- **Brute force:** <Rescan the whole grid every minute, rotting fresh neighbors, until no cell changes — O((n·m)²)>
- **Optimized:** <Seed rotten oranges with 0 in the grid. Then, BFS across it and increment the 0 at each neighbor. Answer will be maximum nr in the grid if all oranges have been traversed, or -1 otherwise (there's an untouched orange). Time - O(n * m); Space - O(n * m)>
- **Key insight:** <Seed rottens with 0 and BFS>
- **Edge cases I had to handle:** <count total oranges up front and decrement as each is consumed — any orange left fresh (`nrOranges != 0`) ⇒ return -1; a grid with no fresh oranges returns 0>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Multi-source grid BFS>
- **Would I solve this in 25 min cold next week? Y/N> 
