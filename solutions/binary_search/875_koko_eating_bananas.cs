public class Solution {
    public int MinEatingSpeed(int[] piles, int h) {
        int n = piles.Length;

        int min = Int32.MaxValue, max = Int32.MinValue;

        for(int i = 0; i < n; ++i)
        {
            min = Math.Min(piles[i], min);
            max = Math.Max(piles[i], max);
        }

        int l = 1, r = max, mid;
        int hours, temp;

        while(l < r)
        {
            mid = (l + r) / 2;
            hours = 0;

            for(int i = 0; i < n; ++i)
            {
                hours += (int)Math.Ceiling((double)piles[i] / mid);
            }

            if(hours > h)
                l = mid + 1;
            else if(hours <= h)
                r = mid;
        }

        return l;
    }
}
