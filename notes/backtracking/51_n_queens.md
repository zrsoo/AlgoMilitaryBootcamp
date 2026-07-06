# LC <51> — <N-Queens>
- **Pattern:** <Backtracking — constraint placement (template E); one queen per row, loop columns and `continue` past attacked cells, counter-based attack board (`++` on place / `--` on undo) so overlapping attacks restore correctly; record board when `row == n`>
- **Brute force:** <Enumerate all ways to place n queens (all column choices / permutations) then validate each full board for row/col/diagonal conflicts.>
- **Optimized:** <Row-by-row backtracking: at each row loop columns, skip any cell with attack-count > 0, place a queen, increment every cell it attacks (row, col, 4 diagonals), recurse, then undo by decrementing; snapshot board when `row == n`. O(n!) time, O(n²) attack-board space.>
- **Key insight:** <Use an int counter board, not booleans: overlapping attacks stack, so an undo `--` only clears a cell if no other queen still attacks it.>
- **Edge cases I had to handle:** <Diagonal index bounds (all four directions guarded by `>= 0` / `< n`); snapshot the board on record with `new List<string>(cboard)` so later mutations don't corrupt stored solutions.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Constraint-placement backtracking (place → recurse → undo); attack-board increment/decrement.>
- **Would I solve this in 25 min cold next week? Y/N> 
