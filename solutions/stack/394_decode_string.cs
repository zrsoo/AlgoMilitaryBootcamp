using System.Text;

public class Solution {
    public string DecodeString(string s) {
        var countStack  = new Stack<int>();
        var stringStack = new Stack<StringBuilder>();
        var current = new StringBuilder();
        int k = 0;

        foreach (char c in s) {
            if (char.IsDigit(c)) {
                k = k * 10 + (c - '0');
            }
            else if (c == '[') {
                countStack.Push(k);
                stringStack.Push(current);
                current = new StringBuilder();
                k = 0;
            }
            else if (c == ']') {
                int repeat = countStack.Pop();
                var prev = stringStack.Pop();
                for (int i = 0; i < repeat; i++) prev.Append(current);
                current = prev;
            }
            else {
                current.Append(c);
            }
        }

        return current.ToString();
    }
}
