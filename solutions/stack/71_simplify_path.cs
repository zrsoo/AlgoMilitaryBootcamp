public class Solution {
    public string SimplifyPath(string path) {
        var words = path.Split('/');
        var stack = new Stack<string>();
        var result = new StringBuilder();

        foreach(var w in words)
        {
            if(w == "." || w == "")
                continue;

            if(w == "..")
            {
                if(stack.Count > 0)
                    stack.Pop();
                continue;
            }

            stack.Push(w);
        }

        while(stack.Count > 0)
        {
            result.Insert(0, $"/{stack.Pop()}");
        }

        return result.Length == 0 ? new string("/") : result.ToString();
    }
}
