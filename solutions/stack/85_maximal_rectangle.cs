public class Solution {
    public int MaximalRectangle(char[][] matrix) {
        var n = matrix.Length;
        var m = matrix[0].Length;

        var maxRectangle = 0;

        var heights = new int[m];
        
        for(int i = 0; i < n; ++i)
        {
            for(int j = 0; j < m; ++j)
            {
                if(matrix[i][j] == '1')
                    heights[j]++;
                else
                    heights[j] = 0;
            }

            maxRectangle = Math.Max(maxRectangle, this.maxAreaInHistogram(heights));
        }

        return maxRectangle;
    }

    private int maxAreaInHistogram(int[] histogram)
    {
        var n = histogram.Length;
        var maxArea = 0;
        var stack = new Stack<int>();
        int el;

        for(int i = 0; i <= n; ++i)
        {
            int currentHeight = i == n ? 0 : histogram[i];

            while(stack.Count > 0 && currentHeight < histogram[stack.Peek()])
            {
                el = histogram[stack.Pop()];
                
                if(stack.Count > 0)
                {
                    maxArea = Math.Max(maxArea, el * (i - stack.Peek() - 1));
                }
                else
                {
                    maxArea = Math.Max(maxArea, el * i);
                }
            }

            stack.Push(i);
        }

        return maxArea;
    }
}
