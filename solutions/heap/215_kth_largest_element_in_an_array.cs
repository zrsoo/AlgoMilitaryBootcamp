public class Solution {
    public int FindKthLargest(int[] nums, int k) {
        var pq = new PriorityQueue<int, int>();

        foreach(var num in nums)
        {
            pq.Enqueue(num, num);

            if(pq.Count == k + 1)
            {
                pq.Dequeue();
            }
        }

        return pq.Peek();
    }
}
