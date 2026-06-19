# LC <994> — <Rotting Oranges>
- **Pattern:** <Graphs — multi-source BFS (seed all rotten cells at time 0, expand rings)>
- **Brute force:** < >
- **Optimized:** <Seed rotten oranges with 0 in the grid. Then, BFS across it and increment the 0 at each neighbor. Answer will be maximum nr in the grid if all oranges have been traversed, or -1 otherwise (there's an untouched orange). Time - O(n * m); Space - O(n * m)>
- **Key insight:** <Seed rottens with 0 and BFS>
- **Edge cases I had to handle:** < >
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** < >
- **Would I solve this in 25 min cold next week? Y/N> 
