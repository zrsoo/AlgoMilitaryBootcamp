public class Solution {
    public string MinWindow(string s, string t) {
        // Impossible if t is longer than s.
        if (t.Length > s.Length) return "";

        // need[c] = how many of char c must be in the window.
        // ASCII 128 covers upper, lower, digits — t can mix cases.
        var need = new int[128];
        // required = number of DISTINCT chars that must be satisfied
        // (not the total count). This is what `satisfied` will be compared to.
        int required = 0;
        foreach (char c in t) {
            // Post-increment + `== 0` check: true exactly the first time we see c.
            if (need[c]++ == 0) required++;
        }

        // have[c] = count of c currently inside the window [l..r].
        var have = new int[128];
        // satisfied = how many distinct needed chars currently meet their quota
        // (have[c] >= need[c]). Window is valid iff satisfied == required.
        int satisfied = 0;

        int l = 0;
        // Track best window by (start index, length) — avoids holding the
        // substring around until we know it's the final answer.
        int bestL = 0, bestLen = int.MaxValue;

        for (int r = 0; r < s.Length; r++) {
            char cr = s[r];
            // Expand: bring s[r] into the window.
            have[cr]++;
            // We only care about chars t actually needs. The "just crossed
            // the threshold" event is `have == need` AFTER the increment —
            // fires exactly once per (c, threshold crossing upward).
            if (need[cr] > 0 && have[cr] == need[cr]) satisfied++;

            // Shrink as much as possible while the window is still valid.
            // `while`, not `if`: we want the MINIMUM window, so peel from the
            // left as long as the engulfment still holds.
            while (satisfied == required) {
                // Record before shrinking — the current window is valid.
                if (r - l + 1 < bestLen) {
                    bestLen = r - l + 1;
                    bestL = l;
                }

                char cl = s[l];
                // The "just dropped below the threshold" event is `have < need`
                // AFTER the decrement. Check the guard BEFORE mutating so the
                // comparison reflects the post-decrement state.
                have[cl]--;
                if (need[cl] > 0 && have[cl] < need[cl]) satisfied--;
                l++;
            }
        }

        // bestLen never updated => no valid window exists.
        return bestLen == int.MaxValue ? "" : s.Substring(bestL, bestLen);
    }
}
