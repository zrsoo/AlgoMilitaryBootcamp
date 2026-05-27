public class Solution {
    public int Trap(int[] height) {
        var n = height.Length;

        var left = new int[n];
        var right = new int [n];

        int max = -1;
        left[0] = -1;

        for(int i = 0; i < n; ++i)
        {
            if(i > 0 && height[i - 1] > max)
                max = height[i - 1];

            left[i] = max;
        }

        max = -1;
        right[n - 1] = -1;

        for(int i = n - 1; i >= 0; --i)
        {
            if(i < n - 1 && height[i + 1] > max)
                max = height[i + 1];

            right[i] = max;
        }

        int totalWater = 0;

        for(int i = 0; i < n; ++i)
        {
            if(left[i] <= height[i] || right[i] <= height[i])
                continue;

            totalWater += Math.Min(left[i], right[i]) - height[i];
        }

        return totalWater;
    }
}
