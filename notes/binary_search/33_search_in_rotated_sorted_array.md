# LC 33 — Search in Rotated Sorted Array
- **Pattern:** Binary Search — "which half is sorted" pivot
- **Brute force:** Linear scan for `target`. O(n) time, O(1) space.
- **Optimized:** Standard binary search loop. At each `mid`, first check `nums[mid] == target`. Then determine which half is sorted: if `nums[l] <= nums[mid]` the **left** half `[l..mid]` is sorted, otherwise the **right** half `[mid..r]` is. Check whether `target` falls inside the sorted half's range (inclusive on the far end, exclusive on `mid` since equality is already handled). If yes → recurse into that half; if no → recurse into the other (still-rotated) half — the same "which side is sorted" test re-applies on the new window. — O(log n) time, O(1) space.
- **Key insight:** A rotated sorted array, split at any `mid`, always has **at least one sorted half**. Once you identify it, membership is an O(1) range check; the other half (if needed) is itself a rotated sorted array, so the recursion is self-similar.
- **Edge cases I had to handle:** The `=` in `nums[l] <= nums[mid]` matters when the window shrinks to 1–2 elements (`l == mid`) — without it you misclassify the sorted side. Array of length 1. `target == nums[mid]` early-return. No duplicates in input (that's LC 81 — `nums[l] == nums[mid]` would be ambiguous there and needs an `l++` fallback).
- **Where I got stuck and for how long:** Needed a hint to see the "one half is always sorted" pivot — wasn't obvious from the rotated structure alone.
- **Template fragments I reused:** Classic `while (l <= r)` binary search with inclusive bounds; sorted-range membership test (`lo <= target && target < hi`).
- **Would I solve this in 25 min cold next week? Y/N** Probably — added to tracker for D+3/D+7/D+14 reps.
