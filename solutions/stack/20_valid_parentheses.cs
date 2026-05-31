public class Solution {
    public bool IsValid(string s) {
        var stack = new Stack<char>();

        foreach (char c in s) {
            switch (c) {
                case '(': stack.Push(')'); break;
                case '[': stack.Push(']'); break;
                case '{': stack.Push('}'); break;
                default:
                    if (stack.Count == 0 || stack.Pop() != c) return false;
                    break;
            }
        }

        return stack.Count == 0;
    }
}
