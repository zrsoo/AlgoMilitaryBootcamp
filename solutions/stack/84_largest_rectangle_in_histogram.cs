public class Solution {
    public int LargestRectangleArea(int[] heights) {
        int n = heights.Length;
        int res = 0;
        var s = new Stack<(int x, int h)>();

        for(int i = 0; i <= n; ++i)
        {
            int cHeight = i < n ? heights[i] : 0;
            int start = i;

            while(s.Count > 0 && s.Peek().h > cHeight)
            {
                var (cx, ch) = s.Pop();

                res = Math.Max((i - cx) * ch, res);

                start = cx;
            }

            s.Push((start, cHeight));
        }

        return res;
    }
}