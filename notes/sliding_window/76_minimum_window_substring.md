# LC 76 — Minimum Window Substring
- **Pattern:** <Variable-size sliding window + frequency counting>
- **Brute force:** <Check every substring of s, verify it contains all chars of t> — O(n²·m) time, O(m) space
- **Optimized:** <need[] holds the required count per char of t and `required` = number of distinct chars needed; expand r adding s[r] to have[], bumping `satisfied` when a char first meets its quota; while satisfied == required, record the smallest window then shrink from l, dropping `satisfied` when a char falls below quota> — O(n + m) time, O(1) space (fixed 128-size arrays)
- **Key insight:** <Track validity with a single `satisfied == required` counter updated only on threshold crossings (have == need on expand, have < need on shrink), and remember the best window by (start, length) instead of the substring>
- **Edge cases I had to handle:** <t longer than s → return ""; no valid window → bestLen stays int.MaxValue → return "">
- **Where I got stuck and for how long:** Couldn't solve it. Needed the full editorial / walkthrough — both the variable-window shape and the incremental `satisfied`-counter trick for the "engulfs" check.
- **Template fragments I reused:** <Variable-size sliding window (expand-r / shrink-l) + frequency arrays>
- **C# notes:** <int[128] arrays indexed directly by char; `need[c]++ == 0` post-increment counts distinct chars in one pass; `s.Substring(bestL, bestLen)` builds the answer only at the end>
- **Would I solve this in 25 min cold next week?** N
