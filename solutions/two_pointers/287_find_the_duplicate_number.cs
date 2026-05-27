public class Solution {
    public int FindDuplicate(int[] nums) {
        int slow = 0;
        int fast = 0;

        do
        {
            // Advance 1 step
            slow = nums[slow];

            // Advance 2 steps
            fast = nums[nums[fast]];
        } while(slow != fast);

        // Reset fast to 0 and advance 1 step each until meet again
        fast = 0;

        while(slow != fast)
        {
            slow = nums[slow];
            fast = nums[fast];
        }

        return slow;
    }
}
