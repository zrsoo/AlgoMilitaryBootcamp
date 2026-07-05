public class Solution {
    public double FindMedianSortedArrays(int[] nums1, int[] nums2) {
        int n = nums1.Length;
        int m = nums2.Length;

        if(n < m) return cut(n, nums1, m, nums2);
        else return cut(m, nums2, n, nums1);
    }

    private double cut(int n, int[] shortArr, int m, int[] longArr)
    {
        int lo = 0, hi = n;
        bool even = (n + m) % 2 == 0;

        while(lo <= hi)
        {
            int i = (lo + hi) / 2;
            int j = (n + m + 1) / 2 - i;

            int aL = i - 1 >= 0 ? shortArr[i - 1] : int.MinValue;
            int aR = i < n ? shortArr[i] : int.MaxValue; 
            int bL = j - 1 >= 0 ? longArr[j - 1] : int.MinValue;
            int bR = j < m ? longArr[j] : int.MaxValue;

            if(aL <= bR && bL <= aR)
            {
                if(even) return (Math.Max(aL, bL) + Math.Min(aR, bR)) / 2.0;
                else return Math.Max(aL, bL);
            }
            else if(bL > aR) lo = i + 1;
            else hi = i - 1;
        }

        return -1;
    }
}