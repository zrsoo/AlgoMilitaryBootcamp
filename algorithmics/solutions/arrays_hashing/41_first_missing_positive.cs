public class Solution {
    public int FirstMissingPositive(int[] nums) {
        int n = nums.Length;
        bool contains1 = false;

        for(int i = 0; i < n; ++i)
        {
            if(nums[i] == 1)
                contains1 = true;

            if(nums[i] <= 0 || nums[i] > n)
                nums[i] = 1;
        }

        for(int i = 0; i < n; ++i)
        {
            if(nums[Math.Abs(nums[i]) - 1] < 0)
                continue;

            nums[Math.Abs(nums[i]) - 1] *= -1;
        }

        if(!contains1)
            return 1;

        for(int i = 0; i < n; ++i)
        {
            if(nums[i] > 0)
                return i + 1;
        }

        return n + 1;
    }
}