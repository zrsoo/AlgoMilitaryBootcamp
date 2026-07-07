public class Solution {
    public int ShipWithinDays(int[] weights, int days) {
        int max = 0;
        int sum = 0;
        int n = weights.Length;

        for(int i = 0; i < n; ++i)
        {
            max = Math.Max(max, weights[i]);
            sum += weights[i];
        }

        int l = max, r = sum, mid;
        int sumW;
        int dayCount;

        while(l < r)
        {
            mid = (l + r) / 2;

            dayCount = 1;
            sumW = 0;

            for(int i = 0; i < n; ++i)
            {
                if(sumW + weights[i] > mid)
                {
                    dayCount++;
                    sumW = weights[i];
                }
                else
                {
                    sumW += weights[i];
                }
            }

            if(dayCount <= days)
                r = mid;
            else
                l = mid + 1;
        }

        return l;
    }
}
