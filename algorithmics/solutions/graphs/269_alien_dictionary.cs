using System;
using System.Collections.Generic;
using System.Text;

public class Solution {
    public string AlienOrder(string[] words) {
        var indeg = new Dictionary<char, int>();
        var adj = new Dictionary<char, List<char>>();

        foreach(var w in words)
            foreach(var c in w)
            {
                indeg.TryAdd(c, 0);
                adj[c] = new List<char>();
            }

        for(int i = 0; i < words.Length - 1; ++i)
        {
            string word1 = words[i];
            string word2 = words[i + 1];
            bool foundDiff = false;

            int minL = Math.Min(word1.Length, word2.Length);

            for(int j = 0; j < minL; ++j)
            {
                if(word1[j] != word2[j])
                {
                    indeg[word2[j]]++;
                    adj[word1[j]].Add(word2[j]);
                    foundDiff = true;
                    break;
                }
            }

            if(!foundDiff && word1.Length > word2.Length)
                return "";
        }

        var q = new Queue<char>();
        var res = new StringBuilder();

        foreach(var (l, c) in indeg)
            if(c == 0)
                q.Enqueue(l);

        while(q.Count > 0)
        {
            var l = q.Dequeue();
            res.Append(l);

            foreach(var ne in adj[l])
            {
                indeg[ne]--;
                
                if(indeg[ne] == 0)
                    q.Enqueue(ne);
            }
        }
        
        return res.Length == indeg.Count ? res.ToString() : "";
    }
}
