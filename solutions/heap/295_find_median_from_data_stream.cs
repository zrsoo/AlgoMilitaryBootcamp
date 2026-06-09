public class MedianFinder {
    PriorityQueue<int, int> lo;
    PriorityQueue<int, int> high;


    public MedianFinder() {
        lo = new PriorityQueue<int, int>();
        high = new PriorityQueue<int, int>();
    }
    
    public void AddNum(int num) {
        int el = lo.EnqueueDequeue(num, -num);
        int nr;

        high.Enqueue(el, el);

        if(high.Count > lo.Count)
        {
            nr = high.Dequeue();
            lo.Enqueue(nr, -nr);
        }
    }
    
    public double FindMedian() {
        if((lo.Count + high.Count) % 2 == 0)
            return (lo.Peek() + high.Peek()) / 2.0;

        return lo.Peek();
    }
}

/**
 * Your MedianFinder object will be instantiated and called as such:
 * MedianFinder obj = new MedianFinder();
 * obj.AddNum(num);
 * double param_2 = obj.FindMedian();
 */
