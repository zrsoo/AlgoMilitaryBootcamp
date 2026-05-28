public class Solution {
    public int LongestConsecutive(int[] nums) {
        var set = new HashSet<int>(nums);

        int currL = 0;
        int currEl;
        int maxL = 0;

        foreach(var nr in set)
        {
            if(!set.Contains(nr - 1))
            {
                currEl = nr;
                while(set.Contains(currEl))
                {
                    currEl++;
                    currL++;
                }
            }

            maxL = Math.Max(maxL, currL);
            currL = 0;
        }

        return maxL;
    }
}