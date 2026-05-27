# LC <42> — <Trapping Rain Water>
- **Pattern:** <Prefix Max / Suffix Max>
- **Brute force:** <Didn't compute> — O(?) time, O(?) space
- **Optimized:** <Pre-compute rolling prefix left and right max. Two arrays with significance: "for element at index i, what is the biggest element to the right and left of it?". The result then becomes, for each element: if left[i] or right[i] are less than or equal to it -- skip, it can't trap any water since it taller than its neighbors; else, the amount of water it traps is Math.Min(left[i], right[i]) - height[i].> — O(n) time, O(n) space
- **Key insight:** <Use Prefixes to compute the amount of water each position traps>
- **Edge cases I had to handle:** <Covered by default>
- **Where I got stuck and for how long:** <This was actually quite smooth>
- **Template fragments I reused:** <Prefix Max / Suffix Max>
- **Would I solve this in 25 min cold next week? Y/N> Y
