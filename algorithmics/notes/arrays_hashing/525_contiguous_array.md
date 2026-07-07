# LC 525 — Contiguous Array
- **Pattern:** Prefix Sum + Hashing (first-seen index variant)
- **Brute force:** Enumerate all subarrays `[l..r]`, count 0s and 1s, record max length where counts are equal. O(n^2) time, O(1) space.
- **Optimized:** Remap `0 → -1`, walk a running `sum`. If `sum == 0` the whole prefix `[0..i]` is balanced → candidate length `i + 1`. Otherwise, the first time we see a given `sum` value, store its index in a `Dictionary<int, int>`; on every later collision the subarray `(firstIdx, i]` has equal +1/-1 contributions, so length `i - firstIdx` is a candidate. O(n) time, O(n) space.
- **Key insight:** Equal count of 0s and 1s ⟺ the +1/-1 prefix sum returns to the same value. Two prefixes with the same sum bracket a balanced subarray, just like LC 560 brackets a target-sum subarray. We want **max length**, so store the **first** index per sum (never overwrite); contrast with LC 560 where we store **counts**.
- **Why first-seen, not counts:** Maximizing length means the earliest occurrence of a sum yields the longest window on every later hit. Overwriting on later occurrences would shorten every future candidate.
- **The `sum == 0` case:** Easy to miss — if the running sum is 0 at index `i`, the balanced subarray is the whole prefix `[0..i]` (length `i+1`), not `i - dict[0]`. Two clean ways to handle it:
  1. Seed the map with `{0: -1}` so the formula `i - dict[sum]` gives `i - (-1) = i + 1` for free.
  2. Special-case `if (sum == 0) max = Math.Max(max, i + 1);` before the dictionary lookup (what my current solution does).
- **Edge cases I had to handle:** All-zeros or all-ones array (answer = 0, sum never returns to start); subarray starting at index 0 (the `sum == 0` case above); single element (answer = 0).
- **Where I got stuck and for how long:** Brute force was fast. ~1h to land on running-sum + dictionary, but initially only saw the `sum == 0` half and missed the "same sum twice = balanced window between them" half. Once both cases clicked, the code was quick.
- **Template fragments I reused:** Same `Dictionary<int,int>` + running prefix-sum skeleton as LC 560 and LC 974. The family discriminator: **counts** (560, 974 — counting subarrays) vs **first-seen index** (525, 325 Maximum Size Subarray Sum Equals k — maximizing length).
- **C# notes:** `Dictionary<int,int>` indexer throws on missing key — guard with `ContainsKey` or `TryGetValue`. `dict[k] = v` both inserts and updates, so to preserve first-seen semantics: `if (!dict.ContainsKey(sum)) dict[sum] = i;`.
- **Would I solve this in 25 min cold next week?** Yes — but on the tracker anyway because the "first-seen vs counts" distinction within the prefix-sum family is exactly the kind of detail that decays.
