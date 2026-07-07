using System.Collections.Generic;

public class Solution {
    public IList<string> FindItinerary(IList<IList<string>> tickets) {
        var result = new List<string>();
        var stack = new Stack<string>();

        var adj = new Dictionary<string, PriorityQueue<string, string>>();

        foreach(var t in tickets)
        {
            if(!adj.TryGetValue(t[0], out var list))
            {
                list = new PriorityQueue<string, string>();
                adj[t[0]] = list;
            }
            list.Enqueue(t[1], t[1]);
        }

        stack.Push("JFK");

        while(stack.Count > 0)
        {
            var n = stack.Peek();

            if(adj.ContainsKey(n) && adj[n].Count > 0)
            {
                var ne = adj[n].Dequeue();
                stack.Push(ne);
            }
            else
            {
                result.Add(stack.Pop());
            }
        }

        result.Reverse();

        return result;
    }
}
