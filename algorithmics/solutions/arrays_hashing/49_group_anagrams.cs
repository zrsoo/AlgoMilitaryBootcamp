public class Solution {
    public IList<IList<string>> GroupAnagrams(string[] strs) {
        var groups = new Dictionary<string, IList<string>>();

        foreach (var str in strs) {
            var arr = str.ToCharArray();
            Array.Sort(arr);
            var key = new string(arr);

            if (!groups.TryGetValue(key, out var list)) {
                list = new List<string>();
                groups[key] = list;
            }

            list.Add(str);
        }
        
        return new List<IList<string>>(groups.Values);
    }
}