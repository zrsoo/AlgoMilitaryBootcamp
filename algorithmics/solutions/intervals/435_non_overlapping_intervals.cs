public class Solution {
    public int EraseOverlapIntervals(int[][] intervals) {
        int n = intervals.Length;
        
        Array.Sort(intervals, (a, b) => a[0].CompareTo(b[0]));

        int prevEnd = intervals[0][1];
        int ans = 0;

        for(int i = 1; i < n; ++i)
        {
            if(prevEnd > intervals[i][0])
            {
                prevEnd = Math.Min(prevEnd, intervals[i][1]);
                ans++;
            }
            else prevEnd = intervals[i][1];
        }

        return ans;
    }
}
