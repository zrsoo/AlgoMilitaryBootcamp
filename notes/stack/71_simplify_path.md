# LC <71> — <Simplify Path>
- **Pattern:** <Stack — token processing / path canonicalization>
- **Brute force:** <Didn't bother>
- **Optimized:** <Split the string by '/'. Then, for each word, treat cases separately: skip '.' or whitespace; pop from word stack and continue if ".."; push the word in the stack otherwise. Then, pop from the stack while it has words and insert at the beginning of result. At the end, if result is still empty, return a '/', otherwise return result.> — O(n) time, O(n) space
- **Key insight:** <Split string by '/' and then treat each word as it comes.>
- **Edge cases I had to handle:** <If result is empty at the end, return '/'.>
- **Where I got stuck and for how long:** <Smooth>
- **Template fragments I reused:** <Simple stack usage>
- **Would I solve this in 25 min cold next week? Y/N> Y 
