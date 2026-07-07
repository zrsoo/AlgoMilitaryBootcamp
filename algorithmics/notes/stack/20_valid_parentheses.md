# LC <20> — <Valid Parentheses>
- **Pattern:** <Stack — matched-pairs / LIFO>
- **Brute force:** <Repeatedly delete adjacent matching pairs (`()`, `[]`, `{}`) from the string until none remain; valid iff the string is empty. O(n²) time.>
- **Optimized:** <On each opener push the EXPECTED closer (`(`→`)` etc.); on any other char fail if the stack is empty or its top `!= c`. Valid iff stack empty at end.> — O(n) time, O(n) space
- **Key insight:** <Pushing the expected closing bracket (not the opener) turns matching into a single equality check on pop — no open→close lookup table needed.>
- **Edge cases I had to handle:** <Closing bracket with empty stack (`stack.Count == 0` → false); leftover openers at end (stack non-empty → false); odd-length / empty string.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Char stack matched-pairs; `switch` on the current char.>
- **Would I solve this in 25 min cold next week? Y/N> 
