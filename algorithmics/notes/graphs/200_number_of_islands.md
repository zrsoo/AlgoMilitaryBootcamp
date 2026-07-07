# LC <200> — <Number of Islands>
- **Pattern:** <Graphs — grid DFS flood fill (connected components)>
- **Brute force:** <Union-find over adjacent land cells, then count roots — same O(n·m)>
- **Optimized:** <Do a DFS Fill. Hold an array for vertical + horizontal moves (4 entries). The DFS Fill checks if it is out of bounds or if it is on water - return. If within bounds and on land, mark as '0', effectively filling all connected land with water. In the main function body, iterate over all cells and fill all found land with water. The answer is how many land cells we find.> — O(n * m) time, O(n * m) space (recursive call stack)
- **Key insight:** <Fill DFS algorithm>
- **Edge cases I had to handle:** <Bounds check + water ('0') as the DFS base case; grid is mutated ('1'→'0') to mark visited>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <DFS Flood Fill>
- **Would I solve this in 25 min cold next week? Y/N> Y 
