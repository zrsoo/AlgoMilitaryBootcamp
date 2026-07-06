# LC <394> — <Decode String>
- **Pattern:** <Stack — two stacks (count + prefix builder)>
- **Brute force:** <Recursive parse: scan for a number, find its matching `]`, recurse on the inner substring, repeat the decoded chunk k times, continue. Rescanning for matching brackets is the slow part.>
- **Optimized:** <Two stacks: `countStack` (repeat counts) and `stringStack` (prefix built before each `[`). Build multi-digit `k` with `k*10 + (c-'0')`; on `[` push count+current and reset; on `]` pop count/prev and append `current` repeat times.> — O(N) time (N = decoded length), O(depth) space
- **Key insight:** <Each `[` pushes the current context (count + prefix-so-far) and starts a fresh builder; each `]` pops and splices back. Two parallel stacks capture arbitrarily nested state without recursion.>
- **Edge cases I had to handle:** <Multi-digit repeat counts (`k*10+digit`); nested brackets; literal chars before/after or between bracket groups.>
- **Where I got stuck and for how long:** < >
- **Template fragments I reused:** <Two-stack context save/restore; multi-digit accumulation; `StringBuilder` accumulation.>
- **Would I solve this in 25 min cold next week? Y/N>
