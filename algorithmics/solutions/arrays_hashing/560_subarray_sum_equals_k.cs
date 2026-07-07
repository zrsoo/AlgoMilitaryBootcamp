public class Solution {
    public int SubarraySum(int[] nums, int k) {
        var seen = new Dictionary<long, int>{ [0] = 1 };
        long psum = 0;
        int ans = 0;

        foreach(var x in nums)
        {
            psum += x;

            if(seen.TryGetValue(psum - k, out var c))
                ans += c;

            seen[psum] = seen.GetValueOrDefault(psum, 0) + 1;
        }

        return ans;
    }
}
