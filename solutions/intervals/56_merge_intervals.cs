public class Solution {
    public int[][] Merge(int[][] intervals) {
        int n = intervals.Length;
        Array.Sort(intervals, (a, b) => a[0].CompareTo(b[0]));

        var result = new List<int[]>();
        var curr = intervals[0];

        for(int i = 0; i < n; ++i)
        {
            if(curr[1] >= intervals[i][0])
            {
                curr[1] = Math.Max(intervals[i][1], curr[1]);
            }
            else
            {
                result.Add(curr);
                curr = intervals[i];
            }
        }

        result.Add(curr);

        return result.ToArray();
    }
}
