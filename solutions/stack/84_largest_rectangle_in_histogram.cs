public class Solution {
    public int LargestRectangleArea(int[] heights) {
        // Append 0 to the end of array so everything gets processed.
        // Edge case where the largest area contains the last candle.
        int n = heights.Length;

        int[] arr = new int[n + 1];

        for(int i = 0; i < n; ++i)
        {
            arr[i] = heights[i];
        }
        arr[n] = 0;

        //

        Stack<int> stack = new Stack<int>();

        int el, maxArea = 0;

        for(int i = 0; i <= n; ++i)
        {
            while(stack.Count > 0 && arr[i] < arr[stack.Peek()])
            {
                el = arr[stack.Peek()] == 0 ? 1 : arr[stack.Pop()];

                if(stack.Count > 0)
                    maxArea = Math.Max(maxArea, el * (i - stack.Peek() - 1));
                else
                {
                    int multiplier = i == 0 ? 1 : i;
                    maxArea = Math.Max(maxArea, el * multiplier); 
                }
            }

            stack.Push(i);
        }

        return maxArea;
    }
}
