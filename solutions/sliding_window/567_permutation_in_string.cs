public class Solution {
    public bool CheckInclusion(string s1, string s2) {
        if(s1.Length > s2.Length)
            return false;

        int[] freqA = new int[26];
        int[] freqB = new int[26];

        for(int i = 0; i < s1.Length; ++i)
        {
            freqA[s1[i] - 'a']++;
            freqB[s2[i] - 'a']++;
        }

        if(freqA.AsSpan().SequenceEqual(freqB))
            return true;

        for(int i = 0; i < s2.Length - s1.Length; ++i)
        {
            freqB[s2[i] - 'a']--;
            freqB[s2[i + s1.Length] - 'a']++;

            if(freqA.AsSpan().SequenceEqual(freqB))
                return true;
        }

        return false;
    }
}
