# Retry Queue / Spaced Repetition Tracker

Cadence: **D+3, D+7, D+14**. Each rep is a blind, timed re-solve from a blank file (editorial closed, notes closed).

**Failure rule:** if you fail (couldn't solve cleanly in time budget, or needed >150% of original time at D+3/D+7) — move the row to "Failed reps", then re-add it to Active with today as the new first-solve date.

**Status values:** `scheduled` → `D+3 ✓` → `D+7 ✓` → `done`, or `failed-reset`.

Date format: `YYYY-MM-DD` (sorts lexically — grep today's date to find what's due).

---

## Active queue

| LC  | Title          | Pattern         | First solve | D+3        | D+7        | D+14       | Status    | Notes |
|-----|----------------|-----------------|-------------|------------|------------|------------|-----------|-------|
| 49  | Group Anagrams              | Arrays/Hashing  | 2026-05-23  | 2026-05-26 ✓ | 2026-05-30 | 2026-06-06 | D+3 ✓ | Needed sort hint at 25 min. C# friction: `Array.Sort(char[])` + `new string(arr)`, `IList<IList<T>>` invariance, `TryGetValue` + `out var` lazy-init. |
| 560 | Subarray Sum Equals K        | Prefix Sum / Hashing | 2026-05-23 | 2026-05-26 ✓ | 2026-05-30 | 2026-06-06 | D+3 ✓ | Prefix sum + hashmap of `prefixSum → count`. Seed `{0:1}` for subarrays starting at index 0. Lookup `psum - k` BEFORE inserting current `psum`. Need counts (not a set) because negatives/zeros make prefix sums recur. Sliding window doesn't work due to negatives. Used `long` for psum to dodge overflow. |
| 128 | Longest Consecutive Sequence | Arrays/Hashing  | 2026-05-25  | 2026-05-28 | 2026-06-01 | 2026-06-08 | failed-reset | **Did not solve.** Tried `Hashtable` + `t[nr] == 1` — fails because `Hashtable` indexer returns `object`, so `==` is reference equality on boxed ints (always false). Also `Hashtable.Add` throws on duplicates. Missed the canonical pattern: dump into `HashSet<int>`, then only start a walk when `n-1` is absent (sequence head) — that's what keeps it O(n) instead of O(n²). |
| 974 | Subarray Sums Divisible by K | Prefix Sum / Hashing | 2026-05-25 | 2026-05-28 | 2026-06-01 | 2026-06-08 | failed-reset | **Did not solve without editorial.** Variant of LC 560: count prefix-sum *remainders* mod k instead of raw sums. Key gotcha: C# `%` can return negative for negative dividends — normalize with `((psum % k) + k) % k`. Seed `{0:1}` so prefixes that are themselves divisible count. For each index, add `count[r]` to answer (pairs of equal remainder ⇒ difference divisible by k), then increment `count[r]`. |
| 15  | 3Sum                         | Two Pointers / Sort | 2026-05-26 | 2026-05-29 | 2026-06-02 | 2026-06-09 | failed-reset | **Needed editorial for dedup pattern.** Sort + outer fixes `nums[i]`, inner two-pointer finds pairs summing to `-nums[i]`. Three dedup points: (1) outer skip `nums[i] == nums[i-1]` (i>0); (2) after a hit, while `nums[l] == nums[l+1]` advance l; (3) mirror for r. Early exit on `nums[i] > 0`. Template fragment for 3Sum Closest, 4Sum, kSum. |
| 75  | Sort Colors                  | Two Pointers / Dutch National Flag | 2026-05-26 | 2026-05-29 | 2026-06-02 | 2026-06-09 | failed-reset | **Needed editorial.** One-pass in-place three-way partition: `low/mid/high`. Invariant: `[0..low-1]=0s`, `[low..mid-1]=1s`, `[mid..high]=unknown`, `[high+1..n-1]=2s`. Key asymmetry: 0-swap advances `mid` (swapped-in value was a confirmed 1), 2-swap does NOT advance `mid` (swapped-in value from `high` is still unknown). Loop `while mid <= high`. |

---

## Completed (passed all 3 reps)

| LC | Title | Pattern | Final pass date |
|----|-------|---------|-----------------|

---

## Failed reps (reset history)

| LC | Title | Failed at | Failure date | Reason | New first-solve date |
|----|-------|-----------|--------------|--------|----------------------|
