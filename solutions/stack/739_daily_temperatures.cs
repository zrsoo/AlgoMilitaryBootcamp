public class Solution {
    public int[] DailyTemperatures(int[] temperatures) {
        var n = temperatures.Length;
        var stack = new Stack<KeyValuePair<int, int>>();
        var result = new int[n];

        for(int i = 0; i < n; ++i)
        {
            while(stack.Count != 0 && temperatures[i] > stack.Peek().Value)
            {
                var kvp = stack.Pop();
                result[kvp.Key] = i - kvp.Key;
            }

            stack.Push(new KeyValuePair<int, int>(i, temperatures[i]));
        }

        return result;
    }
}
