# LC <238> — <Products of Array Except Self>
- **Pattern:** <Arrays & Hashing>
- **Brute force:** <For each number, parse the rest of the array, compute product, store into result> — O(n^2)
- **Optimized 1:** <Compute two sequential product arrays (result[i] = product of all nrs up until AND INCLUDING i), one from the left and one from the right; use leftie[i-1] for product up until I, rightie[i+1] for product I onward> — O(n) time; O(n) space
- **Optimized 2:** <Write leftie (result[i] = product of all nrs up until AND EXCLUDING i) directly onto result. Repeat logic the other way for rightie, but write directly onto result> — O(n) time; O(1) space
- **Key insight:** <Sequential product arrays save us from re-computing products each time>
- **Edge cases I had to handle:** <Zeros need no special-casing since no division is used — one or more zeros are handled naturally by the prefix/suffix products>
- **Where I got stuck and for how long:** <Kinda slow to find solution, didn't find O(1) space optimization>
- **Template fragments I reused:** <Sequential Product Arrays>
- **Would I solve this in 25 min cold next week? Y/N>