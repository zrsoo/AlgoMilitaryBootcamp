# LC 1011 — Capacity To Ship Packages Within D Days
- **Pattern:** Binary Search on Answer
- **Brute force:** Try every capacity from `max(weights)` to `sum(weights)` linearly; for each, simulate the shipment day-count in O(n) and return the first capacity whose day-count ≤ `days`. O(n · sum(weights)).
- **Optimized:** Binary search on capacity in `[max(weights), sum(weights)]`. Lower bound = `max` (any single package must fit in one day's load). Upper bound = `sum` (ship everything in one day). For candidate `mid`, greedy feasibility: walk weights with `dayCount = 1, sumW = 0`; if `sumW + w > mid` close the current day (`dayCount++`, `sumW = w`) else `sumW += w`. If `dayCount <= days` then `mid` is feasible — shrink `r = mid`; else `l = mid + 1`. Loop terminates with `l == r` = smallest feasible capacity. O(n · log(sum - max)).
- **Key insight:** Identical structure to LC 410 Split Array Largest Sum — just renamed (capacity ↔ max subarray sum, days ↔ k). Feasibility is monotonic in `mid` (any capacity ≥ a feasible one is also feasible), which is the precondition for binary-searching the answer.
- **Edge cases I had to handle:** None — bounds `[max, sum]` naturally cover `days == 1` (returns `sum`) and `days >= n` (returns `max`).
- **Where I got stuck and for how long:** Didn't — recognized it as a clone of LC 410 immediately and copy-adapted the template.
- **Template fragments I reused:** Binary Search on Answer + greedy partition-by-cap feasibility check (same as LC 410, sibling of LC 875 Koko).
- **Would I solve this in 25 min cold next week? Y/N** Y
