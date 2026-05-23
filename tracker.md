# Retry Queue / Spaced Repetition Tracker

Cadence: **D+3, D+7, D+14**. Each rep is a blind, timed re-solve from a blank file (editorial closed, notes closed).

**Failure rule:** if you fail (couldn't solve cleanly in time budget, or needed >150% of original time at D+3/D+7) — move the row to "Failed reps", then re-add it to Active with today as the new first-solve date.

**Status values:** `scheduled` → `D+3 ✓` → `D+7 ✓` → `done`, or `failed-reset`.

Date format: `YYYY-MM-DD` (sorts lexically — grep today's date to find what's due).

---

## Active queue

| LC  | Title          | Pattern         | First solve | D+3        | D+7        | D+14       | Status    | Notes |
|-----|----------------|-----------------|-------------|------------|------------|------------|-----------|-------|
| 49  | Group Anagrams              | Arrays/Hashing  | 2026-05-23  | 2026-05-26 | 2026-05-30 | 2026-06-06 | scheduled | Needed sort hint at 25 min. C# friction: `Array.Sort(char[])` + `new string(arr)`, `IList<IList<T>>` invariance, `TryGetValue` + `out var` lazy-init. |
| 238 | Product of Array Except Self | Arrays/Hashing | 2026-05-23  | 2026-05-26 | 2026-05-30 | 2026-06-06 | scheduled | Solved with two prefix arrays (O(n) time, O(n) extra space). Missed the O(1)-aux-space follow-up: write left-prefix into `result`, then second pass with a running `right` scalar. Also special-cased `i==0` / `i==n-1` instead of seeding identity (1). |
| 560 | Subarray Sum Equals K        | Prefix Sum / Hashing | 2026-05-23 | 2026-05-26 | 2026-05-30 | 2026-06-06 | scheduled | Prefix sum + hashmap of `prefixSum → count`. Seed `{0:1}` for subarrays starting at index 0. Lookup `psum - k` BEFORE inserting current `psum`. Need counts (not a set) because negatives/zeros make prefix sums recur. Sliding window doesn't work due to negatives. Used `long` for psum to dodge overflow. |

---

## Completed (passed all 3 reps)

| LC | Title | Pattern | Final pass date |
|----|-------|---------|-----------------|

---

## Failed reps (reset history)

| LC | Title | Failed at | Failure date | Reason | New first-solve date |
|----|-------|-----------|--------------|--------|----------------------|
