# Retry Queue / Spaced Repetition Tracker

Cadence: **D+3, D+7, D+14**. Each rep is a blind, timed re-solve from a blank file (editorial closed, notes closed).

**Failure rule:** if you fail (couldn't solve cleanly in time budget, or needed >150% of original time at D+3/D+7) — move the row to "Failed reps", then re-add it to Active with today as the new first-solve date.

**Status values:** `scheduled` → `D+3 ✓` → `D+7 ✓` → `done`, or `failed-reset`.

Date format: `YYYY-MM-DD` (sorts lexically — grep today's date to find what's due).

---

## Active queue

| LC  | Title          | Pattern         | First solve | D+3        | D+7        | D+14       | Status    | Notes |
|-----|----------------|-----------------|-------------|------------|------------|------------|-----------|-------|
| 49  | Group Anagrams | Arrays/Hashing  | 2026-05-23  | 2026-05-26 | 2026-05-30 | 2026-06-06 | scheduled | Needed sort hint at 25 min. C# friction: `Array.Sort(char[])` + `new string(arr)`, `IList<IList<T>>` invariance, `TryGetValue` + `out var` lazy-init. |

---

## Completed (passed all 3 reps)

| LC | Title | Pattern | Final pass date |
|----|-------|---------|-----------------|

---

## Failed reps (reset history)

| LC | Title | Failed at | Failure date | Reason | New first-solve date |
|----|-------|-----------|--------------|--------|----------------------|
