# LC <85> — <Maximal Rectangle>
- **Pattern:** <Stack — histogram reduction (LC 84 per row)>
- **Brute force:** <Didn't bother>
- **Optimized:** <Use the monotonic increasing stack largest histogram rectangle area approach (from LC84). Reduce the matrix to a heights array, where `heights[j] = the number of continuous stacked 1s on column j up to row r`. Process the largest histogram rectangle at each row, and max it up.> — O(n * m) time, O(m) space
- **Key insight:** <The maximum area of the rectangle in the matrix can be reduced to the maximum rectangle area across each submatrix's (matrix - x rows) rectangles, which can be computed with the histogram max rectangle algorithm at each step. This is basically LC84, but for each histogram of the larger matrix.>
- **Edge cases I had to handle:** <Same as LC84's>
- **Where I got stuck and for how long:** <Was suspecting something related to LC84 but didn't find the solution without hint.>
- **Template fragments I reused:** <Monotonic increasing stack + submatrix processing>
- **Would I solve this in 25 min cold next week? Y/N> Maybe, added to tracker
