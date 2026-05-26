public class Solution {
    public int MaxArea(int[] height) {
        int l = 0;
        int r = height.Length - 1;
        int maxCapacity = -1;

        while(l < r)
        {
            maxCapacity = Math.Max(maxCapacity, Math.Min(height[l], height[r]) * (r - l));

            if(height[l] < height[r])
                l++;
            else
                r--;
        }

        return maxCapacity;
    }
}
