public class Solution {
    public bool CanJump(int[] nums) {
        int n = nums.Length;

        if(nums[0] == 0)
            return n == 1;

        int maxJump = -1;

        for(int i = 0; i < n; ++i)
        {
            if(i + nums[i] > maxJump)
            {
                maxJump = i + nums[i];

                if(maxJump >= n - 1) return true;
            }

            if(nums[i] == 0)
            {
                if(maxJump <= i) return false;
            }
        }

        return false;
    }
}
