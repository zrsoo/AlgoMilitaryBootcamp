public class Solution {
    public int CharacterReplacement(string s, int k) {
        var count = new int[26];
        int l = 0, maxFreq = 0, best = 0;

        for (int r = 0; r < s.Length; r++) {
            count[s[r] - 'A']++;
            maxFreq = Math.Max(maxFreq, count[s[r] - 'A']);

            if (r - l + 1 - maxFreq > k) {
                count[s[l] - 'A']--;
                l++;
            }

            best = Math.Max(best, r - l + 1);
        }

        return best;
    }
}
