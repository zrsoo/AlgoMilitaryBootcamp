public class Solution {
    public int[] TopKFrequent(int[] nums, int k) {
        int n = nums.Length;

        var pq = new PriorityQueue<int, int>();
        var map = new Dictionary<int, int>();

        foreach(var num in nums)
        {
            if(!map.ContainsKey(num))
                map[num] = 0;
            map[num]++;
        }

        foreach(var kvp in map)
        {
            pq.Enqueue(kvp.Key, kvp.Value);

            if(pq.Count == k + 1)
                pq.Dequeue();
        }

        var result = new List<int>();

        for(int i = 0; i < k; ++i)
        {
            result.Add(pq.Dequeue());
        }

        return result.ToArray();
    }
}
