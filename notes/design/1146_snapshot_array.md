# LC 1146 — Snapshot Array
- **Pattern:** Design — per-index list of `(snapId, val)` recorded only on change; `set` O(1) (overwrite last entry if same snap, else append), `snap` O(1), `get` = binary-search floor (largest `snapId <= query`) on that index's list
- **Brute force:**
- **Optimized:** Keep array of change lists. For every index in the array, there's a list of (snapId, value), keeping track of the value at that index at the snap ID. On put, if the last snapId in the list is equal to the current snapId, just overwrite value, otherwise append to the list. Because of snap nature, these lists will be increasingly sorted by snapId. On Get, binary search the list at the index. It's the smaller or equal variant of the binary search i.e. lo = 0, hi = list.count (NOT list.count - 1). Land on mid, if mid is less than or equal to searched value, lo = mid + 1 (we move past the element, but remember that arr[lo] was actually the tested result). Else, r = mid. At the end, if lo == 0, we didn't find anything so return 0, otherwise return list[lo - 1];
- **Key insight:**
- **Edge cases I had to handle:**
- **Where I got stuck and for how long:**
- **Template fragments I reused:**
- **Would I solve this in 25 min cold next week? Y/N**
