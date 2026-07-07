public class Solution {
    public int FindMaxLength(int[] nums) {
        int n = nums.Length;

        var pos = new Dictionary<int, int>();

        int maxLength = 0;
        int sum = 0;
        int el;

        for(int i = 0; i < n; ++i)
        {
            
            el = nums[i] == 1 ? 1 : -1;

            sum += el;

            if(sum == 0)
            {
                maxLength = Math.Max(maxLength, i + 1);
            }
            else
            {
                if(pos.ContainsKey(sum))
                {
                    maxLength = Math.Max(maxLength, i - pos[sum]);
                }
                else
                {
                    pos[sum] = i;
                }
            }
        }

        return maxLength;
    }
}
