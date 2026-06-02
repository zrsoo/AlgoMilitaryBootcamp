public class Solution {
    public int SplitArray(int[] nums, int k) {
        int max = Int32.MinValue;
        int sumArr = 0;
        int n = nums.Length;

        for(int i = 0; i < nums.Length; ++i)
        {
            max = Math.Max(max, nums[i]);
            sumArr += nums[i];
        }

        int l = max, r = sumArr, mid = 0;
        int subArrCount, sum;

        while(l < r)
        {
            mid = (l + r) / 2;

            subArrCount = 1;
            sum = 0;

            for(int i = 0; i < n; ++i)
            {
                if(sum + nums[i] > mid)
                {
                    subArrCount++;
                    sum = nums[i];
                }
                else
                {
                    sum += nums[i];
                }
            }

            if(subArrCount <= k)
                r = mid;
            else
                l = mid + 1;
        }

        return l;
    }
}
