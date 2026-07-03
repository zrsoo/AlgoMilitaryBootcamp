# LC 460 — LFU Cache
- **Pattern:** Design — `Dictionary<key, Node>` (val + freq) + `Dictionary<freq, DLL>` (insertion-ordered bucket per frequency) + a `minCount` pointer; access bumps freq (move node from bucket `f` to `f+1`), evict the tail of `buckets[minCount]` on overflow. LRU-within-each-frequency-bucket.
- **Brute force:**
- **Optimized:**
- **Key insight:**
- **Edge cases I had to handle:**
- **Where I got stuck and for how long:**
- **Template fragments I reused:**
- **Would I solve this in 25 min cold next week? Y/N**
