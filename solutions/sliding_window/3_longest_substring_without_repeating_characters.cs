public class Solution {
    public int LengthOfLongestSubstring(string s) {
        int n = s.Length;
        int l = 0;
        int r = 0;
        int maxLength = 0;
        var set = new int[201];

        while(l < n && r < n)
        {
            while(r < n && set[s[r]] == 0)
            {
                set[s[r]]++;
                r++;
            }

            maxLength = Math.Max(r - l, maxLength);

            while(l < r && r < n && set[s[r]] > 0)
            {
                set[s[l]]--;
                l++;
            }
        }

        return maxLength;
    }
}
