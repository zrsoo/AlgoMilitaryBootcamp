# LC <79> — <Word Search>
- **Pattern:** <Backtracking — grid/path DFS; mark cell `'#'` before recursing + restore after, `idx` tracks match progress>
- **Brute force:** <Try DFS from every cell exploring all 4-directional paths — the search itself is the brute force; no polynomial alternative.>
- **Optimized:** <DFS from each cell matching `word[idx]`; on a match mark the cell `'#'`, recurse into the 4 neighbors with `idx+1`, then restore the cell; succeed when `idx == word.Length-1`. O(m·n·4^L) time, O(L) recursion depth.>
- **Key insight:** <Overwriting the cell with `'#'` marks it visited in place (no separate visited set) and restoring it on the way out lets other paths reuse it.>
- **Edge cases I had to handle:** <Out-of-bounds guard before reading a cell; char mismatch early-returns false; final-letter match returns true before marking/moving; always restore the cell after exploring.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Grid DFS with in-place `'#'` visited marker; 4-direction move list.>
- **Would I solve this in 25 min cold next week? Y/N> 
