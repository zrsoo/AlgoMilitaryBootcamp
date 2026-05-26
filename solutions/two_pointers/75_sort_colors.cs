public class Solution {
    public void SortColors(int[] nums) {
        int n = nums.Length;

        int low = 0;
        int high = nums.Length - 1;

        int mid = 0;

        int temp;

        while(mid <= high)
        {
            if(nums[mid] == 0)
            {
                temp = nums[low];
                nums[low] = nums[mid];
                nums[mid] = temp;

                low++;
                mid++;
            }
            else if(nums[mid] == 2)
            {
                temp = nums[high];
                nums[high] = nums[mid];
                nums[mid] = temp;

                high--;
            }
            else
            {
                mid++;
            }
        }
    }
}
