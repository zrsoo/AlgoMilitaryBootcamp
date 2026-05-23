# LC <49> — <Group Anagrams>
- **Pattern:** <Arrays & Hashing>
- **Brute force:** <For each word, parse list, check if anagram, add to result> — O(n^2 * k*log(k))
- **Optimized:** <One pass, Sort words as you go, Store in Dict using Key = sorted word> — O(n * k*log(k))
- **Optimized:** <One pass, Use hash array of letters in word, No sorting, Store in Dict using Key = hash array> — O(n * k)
- **Key insight:** <Anagrams share a canonical form (sorted string or 26-letter count signature); use it as a hash key to group in O(1) per word instead of O(n) pairwise comparison>
- **Edge cases I had to handle:** <Empty input → foreach no-ops, returns empty list; single word → forms group of one; duplicates / single-char words fall out naturally>
- **Where I got stuck and for how long:** <25 mins, no solution; Peeked at sorting, got stuck on C# syntax. Root causes: (1) `Array.Sort(char[])` + `new string(arr)` idiom for sorting a string, (2) `IList<IList<T>>` invariance — must construct as `new List<IList<string>>()`, not `new List<List<string>>()`, (3) `Dictionary.TryGetValue` + `out var` lazy-init pattern>
- **Template fragments I reused:** <`Dictionary<string, IList<string>>` + `TryGetValue(key, out var list)` + lazy-init — reusable for Two Sum variants, Top K, bucketing problems>
- **Would I solve this in 25 min cold next week? Y/N> Maybe, will add to retry queue