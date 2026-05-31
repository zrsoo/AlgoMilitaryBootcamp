public class Solution {
    public int[] MaxSlidingWindow(int[] nums, int k) {
        int n = nums.Length;
        var result = new int[n - k + 1];
        var d = new LinkedList<int>();

        for (int r = 0; r < n; r++) {
            // Evict the front if it slid out of [r-k+1, r].
            if (d.Count > 0 && d.First.Value <= r - k)
                d.RemoveFirst();

            // Maintain monotonic-decreasing values from front to back.
            while (d.Count > 0 && nums[d.Last.Value] <= nums[r])
                d.RemoveLast();

            d.AddLast(r);

            // First full window forms at r == k - 1.
            if (r >= k - 1)
                result[r - k + 1] = nums[d.First.Value];
        }

        return result;
    }
}
