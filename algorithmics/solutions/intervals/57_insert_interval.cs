public class Solution {
    public int[][] Insert(int[][] intervals, int[] newInterval) {
        int n = intervals.Length;

        if(n == 0)
            return new int[][]{newInterval};

        var interm = new List<int[]>();
        var insertPos = 0;

        for(int i = 0; i < n; ++i)
        {
            if(intervals[i][0] > newInterval[0])
                break;
            
            insertPos = i;
            interm.Add(intervals[i]);
        }

        interm.Add(newInterval);

        if(interm.Count == 1)
            interm.Add(intervals[insertPos]);

        for(int i = insertPos + 1; i < n; ++i)
            interm.Add(intervals[i]);

        // One more pass merging intervals
        var result = new List<int[]>();
        var curr = interm[0];

        for(int i = 0; i < interm.Count; ++i)
        {
            if(curr[1] >= interm[i][0])
            {
                curr[1] = Math.Max(curr[1], interm[i][1]);
            }
            else
            {
                result.Add(curr);
                curr = interm[i];
            }
        }

        result.Add(curr);

        return result.ToArray();
    }
}
