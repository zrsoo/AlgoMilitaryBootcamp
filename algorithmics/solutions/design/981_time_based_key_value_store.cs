public class TimeMap {
    Dictionary<string, List<(int ts, string value)>> dict;

    public TimeMap() {
        dict = new();
    }
    
    public void Set(string key, string value, int timestamp) {
        if(!dict.TryGetValue(key, out var list))
        {
            list = new List<(int ts, string value)>();
            dict[key] = list;
        }

        list.Add((timestamp, value));
    }
    
    public string Get(string key, int timestamp) {
        if(!dict.ContainsKey(key)) return "";

        var list = dict[key];
        int l = 0, r = list.Count;
        while(l < r)
        {
            int mid = (l + r) / 2;

            if(list[mid].ts == timestamp) return list[mid].value;

            if(list[mid].ts <= timestamp)
                l = mid + 1;
            else
                r = mid;
        }

        return l == 0 ? "" : list[l - 1].value;
    }
}

/**
 * Your TimeMap object will be instantiated and called as such:
 * TimeMap obj = new TimeMap();
 * obj.Set(key,value,timestamp);
 * string param_2 = obj.Get(key,timestamp);
 */
