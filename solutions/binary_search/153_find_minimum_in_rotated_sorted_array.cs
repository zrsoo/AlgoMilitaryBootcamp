public class Solution {
    public int FindMin(int[] nums) {
        int l = 0;
        int r = nums.Length - 1;
        int mid;

        if(nums[l] < nums[r])
            return nums[l];

        while(l < r)
        {
            mid = (l + r) / 2;

            if(nums[mid] > nums[r])
                l = mid + 1;
            else
            {
                r = mid;
            }
        }

        return nums[l];
    }
}
